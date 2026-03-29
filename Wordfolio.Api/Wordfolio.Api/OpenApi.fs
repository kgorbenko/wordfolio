module Wordfolio.Api.OpenApi

open System
open System.Threading.Tasks
open System.Collections.Generic

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

                        let s = OpenApiSchema()

                        s.OneOf <-
                            ResizeArray<IOpenApiSchema>(
                                [ OpenApiSchema(Type = Nullable(JsonSchemaType.Null)) :> IOpenApiSchema
                                  OpenApiSchemaReference(innerName, document) :> IOpenApiSchema ]
                            )

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
                | :? OpenApiSchema as p when p.Format = "int32" || p.Format = "int64" ->
                    p.Type <-
                        Nullable(
                            p.Type.GetValueOrDefault()
                            &&& ~~~JsonSchemaType.String
                        )

                    p.Pattern <- null
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

        options.AddOperationTransformer(fun operation context _ ->
            let metadata =
                context.Description.ActionDescriptor.EndpointMetadata

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
