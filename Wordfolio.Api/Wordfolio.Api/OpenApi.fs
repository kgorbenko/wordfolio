module Wordfolio.Api.OpenApi

open System
open System.Collections.Generic
open System.Threading.Tasks

open Microsoft.AspNetCore.Authorization
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.OpenApi

[<Literal>]
let private FSharpOptionPrefix =
    "FSharpOptionOf"

let private inlineFSharpOptionSchemas(document: OpenApiDocument) =
    let schemas = document.Components.Schemas

    let fsOptionKeys =
        schemas.Keys
        |> Seq.filter(fun k -> k.StartsWith(FSharpOptionPrefix))
        |> Seq.toArray

    let fsOptionSchemas =
        fsOptionKeys
        |> Array.map(fun k -> k, schemas[k])
        |> dict

    for schema in (schemas.Values |> Seq.toArray) do
        if not(isNull schema.Properties) then
            let replacements =
                schema.Properties
                |> Seq.choose(fun kvp ->
                    let refId =
                        match kvp.Value with
                        | :? OpenApiSchemaReference as r -> r.Reference.Id
                        | _ -> null

                    if
                        not(isNull refId)
                        && refId.StartsWith(FSharpOptionPrefix)
                    then
                        Some(kvp.Key, refId)
                    else
                        None)
                |> Seq.toArray

            for propName, refId in replacements do
                let innerSchema = fsOptionSchemas[refId]

                let inlinedSchema =
                    if
                        innerSchema.Type.HasValue
                        && innerSchema.Type.Value = JsonSchemaType.Object
                    then
                        let innerName =
                            refId.Substring(FSharpOptionPrefix.Length)

                        if not(schemas.ContainsKey(innerName)) then
                            schemas[innerName] <- innerSchema

                        let s = OpenApiSchema()

                        s.OneOf <-
                            ResizeArray<IOpenApiSchema>(
                                [ OpenApiSchema(Type = Nullable(JsonSchemaType.Null)) :> IOpenApiSchema
                                  OpenApiSchemaReference(innerName, document) :> IOpenApiSchema ]
                            )

                        s
                    elif
                        innerSchema.Type.HasValue
                        && innerSchema.Type.Value.HasFlag(JsonSchemaType.Array)
                    then
                        let s = OpenApiSchema()

                        s.Type <-
                            Nullable(
                                JsonSchemaType.Array
                                ||| JsonSchemaType.Null
                            )

                        s.Items <- innerSchema.Items
                        s
                    else
                        let s = OpenApiSchema()

                        s.Type <-
                            Nullable(
                                innerSchema.Type.Value
                                ||| JsonSchemaType.Null
                            )

                        if not(isNull innerSchema.Format) then
                            s.Format <- innerSchema.Format

                        s

                schema.Properties[propName] <- inlinedSchema

                if not(isNull schema.Required) then
                    schema.Required.Remove(propName)
                    |> ignore

    for key in fsOptionKeys do
        schemas.Remove(key) |> ignore

let private removeNullFromRequiredProperties(document: OpenApiDocument) =
    for schema in document.Components.Schemas.Values do
        if
            not(isNull schema.Required)
            && not(isNull schema.Properties)
        then
            for requiredProp in schema.Required do
                match schema.Properties.TryGetValue(requiredProp) with
                | true, (:? OpenApiSchema as propSchema) when propSchema.Type.HasValue ->
                    let t = propSchema.Type.Value

                    if
                        t.HasFlag(JsonSchemaType.Null)
                        && not(t.HasFlag(JsonSchemaType.Object))
                    then
                        propSchema.Type <- Nullable(t &&& ~~~JsonSchemaType.Null)
                | _ -> ()

let private removeStringFromIntegerProperties(document: OpenApiDocument) =
    for schema in document.Components.Schemas.Values do
        if not(isNull schema.Properties) then
            for propSchema in schema.Properties.Values do
                match propSchema with
                | :? OpenApiSchema as p when p.Type.HasValue ->
                    let schemaType = p.Type.Value

                    if
                        schemaType.HasFlag(JsonSchemaType.String)
                        && (schemaType.HasFlag(JsonSchemaType.Integer)
                            || schemaType.HasFlag(JsonSchemaType.Number)
                            || p.Format = "int32"
                            || p.Format = "int64")
                    then
                        p.Type <- Nullable(schemaType &&& ~~~JsonSchemaType.String)
                        p.Pattern <- null
                | _ -> ()

let private addExercisePromptSchemas(document: OpenApiDocument) =
    let schemas = document.Components.Schemas

    let createObjectSchema(requiredProperties: string list) =
        let schema = OpenApiSchema()
        schema.Type <- Nullable JsonSchemaType.Object
        schema.Required <- HashSet<string>(requiredProperties)
        schema.Properties <- Dictionary<string, IOpenApiSchema>()
        schema

    let createStringSchema() =
        OpenApiSchema(Type = Nullable JsonSchemaType.String)

    let createStringArraySchema() =
        let items = createStringSchema()
        let schema = OpenApiSchema()
        schema.Type <- Nullable JsonSchemaType.Array
        schema.Items <- items
        schema

    let createSchemaReference(name: string) =
        OpenApiSchemaReference(name, document) :> IOpenApiSchema

    let multipleChoiceOptionSchema =
        let schema =
            createObjectSchema [ "id"; "text" ]

        schema.Properties["id"] <- createStringSchema()
        schema.Properties["text"] <- createStringSchema()
        schema

    schemas["MultipleChoicePromptOptionResponse"] <- multipleChoiceOptionSchema

    let multipleChoicePromptSchema =
        let schema =
            createObjectSchema [ "entryText"; "options"; "correctOptionId" ]

        let optionsSchema = OpenApiSchema()
        optionsSchema.Type <- Nullable JsonSchemaType.Array
        optionsSchema.Items <- createSchemaReference "MultipleChoicePromptOptionResponse"

        schema.Properties["entryText"] <- createStringSchema()
        schema.Properties["options"] <- optionsSchema
        schema.Properties["correctOptionId"] <- createStringSchema()
        schema

    schemas["MultipleChoicePromptDataResponse"] <- multipleChoicePromptSchema

    let translationPromptSchema =
        let schema =
            createObjectSchema [ "entryText"; "acceptedTranslations" ]

        schema.Properties["entryText"] <- createStringSchema()
        schema.Properties["acceptedTranslations"] <- createStringArraySchema()
        schema

    schemas["TranslationPromptDataResponse"] <- translationPromptSchema

    match schemas.TryGetValue("SessionBundleEntryResponse") with
    | true, (:? OpenApiSchema as sessionBundleEntrySchema) when not(isNull sessionBundleEntrySchema.Properties) ->
        let promptDataSchema = OpenApiSchema()

        promptDataSchema.OneOf <-
            ResizeArray<IOpenApiSchema>(
                [ createSchemaReference "MultipleChoicePromptDataResponse"
                  createSchemaReference "TranslationPromptDataResponse" ]
            )

        sessionBundleEntrySchema.Properties["promptData"] <- promptDataSchema
    | _ -> ()

let private normalizeEntrySelectorSchema(document: OpenApiDocument) =
    match document.Components.Schemas.TryGetValue("EntrySelectorRequest") with
    | true, (:? OpenApiSchema as entrySelectorSchema) when not(isNull entrySelectorSchema.Properties) ->
        entrySelectorSchema.Required.Remove("entryIds")
        |> ignore

        match entrySelectorSchema.Properties.TryGetValue("entryIds") with
        | true, (:? OpenApiSchema as entryIdsSchema) ->
            let normalizedEntryIdsSchema =
                OpenApiSchema()

            normalizedEntryIdsSchema.Type <-
                Nullable(
                    JsonSchemaType.Array
                    ||| JsonSchemaType.Null
                )

            normalizedEntryIdsSchema.Items <- entryIdsSchema.Items
            entrySelectorSchema.Properties["entryIds"] <- normalizedEntryIdsSchema
        | _ -> ()
    | _ -> ()

let addOpenApi<'TBuilder when 'TBuilder :> IHostApplicationBuilder>(builder: 'TBuilder) =
    builder.Services.AddOpenApi(fun options ->
        options.AddDocumentTransformer(fun document _ _ ->
            if isNull document.Components then
                document.Components <- OpenApiComponents()

            if isNull document.Components.SecuritySchemes then
                document.Components.SecuritySchemes <- Dictionary<string, IOpenApiSecurityScheme>()

            document.Components.SecuritySchemes["Bearer"] <-
                OpenApiSecurityScheme(
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "Enter your JWT token"
                )

            Task.CompletedTask)
        |> ignore

        options.AddDocumentTransformer(fun document _ _ ->
            inlineFSharpOptionSchemas document
            Task.CompletedTask)
        |> ignore

        options.AddDocumentTransformer(fun document _ _ ->
            removeNullFromRequiredProperties document
            Task.CompletedTask)
        |> ignore

        options.AddDocumentTransformer(fun document _ _ ->
            removeStringFromIntegerProperties document
            Task.CompletedTask)
        |> ignore

        options.AddDocumentTransformer(fun document _ _ ->
            addExercisePromptSchemas document
            Task.CompletedTask)
        |> ignore

        options.AddDocumentTransformer(fun document _ _ ->
            normalizeEntrySelectorSchema document
            Task.CompletedTask)
        |> ignore

        options.AddOperationTransformer(fun operation context _ ->
            let metadata =
                context.Description.ActionDescriptor.EndpointMetadata

            if not(isNull operation.RequestBody) then
                operation.RequestBody <-
                    OpenApiRequestBody(
                        Content = operation.RequestBody.Content,
                        Description = operation.RequestBody.Description,
                        Required = true
                    )

            let hasAllowAnonymous =
                metadata
                |> Seq.exists(fun m -> m :? IAllowAnonymous)

            if not hasAllowAnonymous then
                let securityRequirement =
                    OpenApiSecurityRequirement()

                securityRequirement.Add(OpenApiSecuritySchemeReference("Bearer"), List<string>())
                operation.Security <- ResizeArray([ securityRequirement ])

            Task.CompletedTask)
        |> ignore)
    |> ignore

    builder
