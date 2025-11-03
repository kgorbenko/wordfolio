module Wordfolio.ServiceDefaults.Builder

open System

open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Diagnostics.HealthChecks
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging

open OpenTelemetry
open OpenTelemetry.Metrics
open OpenTelemetry.Trace

open Wordfolio.ServiceDefaults.HealthCheck

let private filterTraces(context: HttpContext) =
    let pathStartsWith(segment: string) =
        context.Request.Path.StartsWithSegments segment

    (not(pathStartsWith HealthEndpointPath))
    && (not(pathStartsWith AlivenessEndpointPath))

let addOpenTelemetryExporters<'TBuilder when 'TBuilder :> IHostApplicationBuilder>(builder: 'TBuilder) =
    let useOtlpExporter =
        String.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"])
        |> not

    if useOtlpExporter then
        builder.Services.AddOpenTelemetry().UseOtlpExporter()
        |> ignore

    builder

let configureOpenTelemetry<'TBuilder when 'TBuilder :> IHostApplicationBuilder>(builder: 'TBuilder) =
    builder.Logging.AddOpenTelemetry(fun logging ->
        logging.IncludeFormattedMessage <- true
        logging.IncludeScopes <- true)
    |> ignore

    builder.Services
        .AddOpenTelemetry()
        .WithMetrics(fun metrics ->
            metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
            |> ignore)
        .WithTracing(fun tracing ->
            tracing
                .AddSource(builder.Environment.ApplicationName)
                .AddAspNetCoreInstrumentation(fun tracing -> tracing.Filter <- filterTraces)
                .AddHttpClientInstrumentation()
            |> ignore)
    |> ignore

    builder |> addOpenTelemetryExporters

let addDefaultHealthChecks<'TBuilder when 'TBuilder :> IHostApplicationBuilder>(builder: 'TBuilder) =
    let livenessHealthCheck() = HealthCheckResult.Healthy()

    builder.Services
        .AddHealthChecks()
        .AddCheck("self", livenessHealthCheck, ([ LiveTag ] |> Seq.ofList))
    |> ignore

    builder

let addServiceDiscovery<'TBuilder when 'TBuilder :> IHostApplicationBuilder>(builder: 'TBuilder) =
    builder.Services.AddServiceDiscovery()
    |> ignore

    builder

let configureHttpClientDefaults<'TBuilder when 'TBuilder :> IHostApplicationBuilder>(builder: 'TBuilder) =
    builder.Services.ConfigureHttpClientDefaults(fun clientBuilder ->
        clientBuilder.AddStandardResilienceHandler()
        |> ignore

        clientBuilder.AddServiceDiscovery()
        |> ignore)
    |> ignore

    builder

let addOpenApi<'TBuilder when 'TBuilder :> IHostApplicationBuilder>(builder: 'TBuilder) =
    builder.Services.AddOpenApi() |> ignore

    builder
