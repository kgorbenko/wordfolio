module Wordfolio.ServiceDefaults.OpenApi

open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting

let OpenApiPath = "/openapi/v1.json"

let mapOpenApi(app: WebApplication) =
    if app.Environment.IsDevelopment() then
        app.MapOpenApi(OpenApiPath) |> ignore

        app.UseSwaggerUI(fun options -> options.SwaggerEndpoint(OpenApiPath, "OpenAPI V1"))
        |> ignore

    app
