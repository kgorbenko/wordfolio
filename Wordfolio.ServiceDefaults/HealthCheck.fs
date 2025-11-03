module Wordfolio.ServiceDefaults.HealthCheck

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Diagnostics.HealthChecks
open Microsoft.Extensions.Hosting

let HealthEndpointPath = "/health"
let AlivenessEndpointPath = "/alive"

let internal LiveTag = "live"

let mapHealthChecks(app: WebApplication) =
    if app.Environment.IsDevelopment() then
        app.MapHealthChecks(HealthEndpointPath).AllowAnonymous()
        |> ignore

        app
            .MapHealthChecks(AlivenessEndpointPath, HealthCheckOptions(Predicate = _.Tags.Contains(LiveTag)))
            .AllowAnonymous()
        |> ignore

    app
