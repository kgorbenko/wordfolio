open System
open System.Threading.Tasks
open System.Diagnostics

open FluentMigrator.Runner
open Microsoft.EntityFrameworkCore
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging

open Wordfolio.Api.Identity
open Wordfolio.ServiceDefaults.Builder

type MigrationWorker(serviceProvider: IServiceProvider, hostApplicationLifetime: IHostApplicationLifetime) =
    inherit BackgroundService()

    [<Literal>]
    let ActivitySourceName = "Migrations"

    let activitySource =
        new ActivitySource(ActivitySourceName)

    let runIdentityMigrations(logger: ILogger) =
        task {
            use activity =
                activitySource.StartActivity("Running Identity migrations", ActivityKind.Client)

            try
                use scope = serviceProvider.CreateScope()

                let dbContext =
                    scope.ServiceProvider.GetRequiredService<IdentityDbContext>()

                logger.LogInformation("Running Identity migrations...")
                do! dbContext.Database.MigrateAsync()

                logger.LogInformation("Identity migrations completed successfully")
            with ex ->
                if not(isNull activity) then
                    activity.SetStatus(ActivityStatusCode.Error, ex.Message)
                    |> ignore

                    activity.AddException(ex) |> ignore

                logger.LogError(ex, "Identity migrations failed")
                raise ex
        }

    let runWordfolioMigrations(logger: ILogger) =
        task {
            use activity =
                activitySource.StartActivity("Running Wordfolio migrations", ActivityKind.Client)

            try
                use scope = serviceProvider.CreateScope()

                let migrationRunner =
                    scope.ServiceProvider.GetRequiredService<IMigrationRunner>()

                logger.LogInformation("Running Wordfolio migrations...")
                migrationRunner.MigrateUp()

                logger.LogInformation("Wordfolio migrations completed successfully")
            with ex ->
                if not(isNull activity) then
                    activity.SetStatus(ActivityStatusCode.Error, ex.Message)
                    |> ignore

                    activity.AddException(ex) |> ignore

                logger.LogError(ex, "Wordfolio migrations failed")
                raise ex
        }

    override this.ExecuteAsync(stoppingToken) =
        task {
            use activity =
                activitySource.StartActivity("Migrating database", ActivityKind.Client)

            let logger =
                serviceProvider.GetRequiredService<ILogger<MigrationWorker>>()

            try

                logger.LogInformation("Starting database migrations...")

                do! runIdentityMigrations logger
                do! runWordfolioMigrations logger

                logger.LogInformation("All database migrations completed successfully")
            with ex ->
                if not(isNull activity) then
                    activity.SetStatus(ActivityStatusCode.Error, ex.Message)
                    |> ignore

                    activity.AddException(ex) |> ignore
                    logger.LogError(ex, "Wordfolio migrations failed")
                    raise ex

            hostApplicationLifetime.StopApplication()
        }
        :> Task

[<EntryPoint>]
let main args =
    let builder =
        Host.CreateApplicationBuilder(args)

    builder
    |> configureOpenTelemetry
    |> addDefaultHealthChecks
    |> addServiceDiscovery
    |> configureHttpClientDefaults
    |> ignore

    builder.AddNpgsqlDbContext<IdentityDbContext>("wordfoliodb")

    let connectionString =
        builder.Configuration.GetConnectionString("wordfoliodb")

    builder.Services
        .AddFluentMigratorCore()
        .ConfigureRunner(fun rb ->
            rb
                .AddPostgres15_0()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(typeof<Wordfolio.Api.Migrations.CreateWordfolioSchema>.Assembly)
                .For.Migrations()
            |> ignore)
        .AddLogging(fun lb -> lb.AddFluentMigratorConsole() |> ignore)
    |> ignore

    builder.Services.AddHostedService<MigrationWorker>()
    |> ignore

    let app = builder.Build()

    app.Run()

    0
