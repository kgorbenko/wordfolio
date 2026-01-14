module Wordfolio.Api.OpenApi

open System.Threading.Tasks
open System.Collections.Generic

open Microsoft.AspNetCore.Authorization
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting

open Microsoft.OpenApi.Models

let addOpenApi<'TBuilder when 'TBuilder :> IHostApplicationBuilder>(builder: 'TBuilder) =
    builder.Services.AddOpenApi(fun options ->
        options.AddDocumentTransformer(fun document _ _ ->
            if isNull document.Components then
                document.Components <- OpenApiComponents()

            if isNull document.Components.SecuritySchemes then
                document.Components.SecuritySchemes <- Dictionary<string, OpenApiSecurityScheme>()

            document.Components.SecuritySchemes["Bearer"] <-
                OpenApiSecurityScheme(
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "Enter your JWT token"
                )

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

                securityRequirement.Add(
                    OpenApiSecurityScheme(
                        Reference = OpenApiReference(Type = ReferenceType.SecurityScheme, Id = "Bearer")
                    ),
                    List<string>()
                )

                operation.Security <- ResizeArray([ securityRequirement ])

            Task.CompletedTask)
        |> ignore)
    |> ignore

    builder
