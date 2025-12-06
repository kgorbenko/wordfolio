module Wordfolio.Api.MigrationRunner

open System

open Microsoft.EntityFrameworkCore
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging

open FluentMigrator.Runner
open Npgsql

open Wordfolio.Api.Identity
open Wordfolio.Api.Migrations

let private runWordfolioMigrations (connectionString: string) (logger: ILogger) : unit =
    try
        logger.LogInformation("Starting Wordfolio schema migrations...")

        use serviceProvider =
            ServiceCollection()
                .AddFluentMigratorCore()
                .ConfigureRunner(fun builder ->
                    builder
                        .AddPostgres()
                        .WithGlobalConnectionString(connectionString)
                        .ScanIn(typeof<CreateUsersTable>.Assembly)
                        .For.Migrations()
                    |> ignore)
                .AddLogging(fun lb -> lb.AddConsole() |> ignore)
                .BuildServiceProvider()

        use scope =
            serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope()

        let runner =
            scope.ServiceProvider.GetRequiredService<IMigrationRunner>()

        runner.MigrateUp()
        logger.LogInformation("Wordfolio schema migrations completed successfully")
    with ex ->
        logger.LogError(ex, "Error running Wordfolio schema migrations")
        reraise()

let private runIdentityMigrations (connectionString: string) (logger: ILogger) : unit =
    try
        logger.LogInformation("Starting Identity schema migrations...")

        let options =
            DbContextOptionsBuilder<IdentityDbContext>().UseNpgsql(connectionString).Options

        use identityContext =
            new IdentityDbContext(options)

        identityContext.Database.Migrate()
        logger.LogInformation("Identity schema migrations completed successfully")
    with ex ->
        logger.LogError(ex, "Error running Identity schema migrations")
        reraise()

let runMigrations(app: IHost) : unit =
    let logger =
        app.Services.GetRequiredService<ILogger<obj>>()

    // Check if migrations should run automatically
    // Default to true in production, but allow disabling via configuration
    let configuration =
        app.Services.GetRequiredService<IConfiguration>()

    let runMigrationsValue =
        configuration.["RunMigrationsOnStartup"]

    let runMigrations =
        match runMigrationsValue with
        | null -> true // Default to true if not specified
        | value -> not(value.Equals("false", StringComparison.OrdinalIgnoreCase))

    if not runMigrations then
        logger.LogInformation("Automatic migrations are disabled. Skipping migration check.")
    else
        try
            logger.LogInformation("Checking for pending database migrations...")

            // Get the connection string from the NpgsqlDataSource
            let dataSource =
                app.Services.GetRequiredService<NpgsqlDataSource>()

            let connectionString =
                dataSource.ConnectionString

            // Run both Identity and Wordfolio migrations
            runIdentityMigrations connectionString logger
            runWordfolioMigrations connectionString logger

            logger.LogInformation("All database migrations completed successfully")
        with ex ->
            logger.LogCritical(ex, "Failed to run database migrations. Application startup aborted.")
            reraise()
