namespace Wordfolio.Api.Tests.Utils

open System

open FluentMigrator.Runner
open Microsoft.EntityFrameworkCore
open Microsoft.Extensions.DependencyInjection

open Wordfolio.Api.Identity
open Wordfolio.Api.Migrations

[<RequireQualifiedAccess>]
module SchemaMigrations =
    let runWordfolioMigrations(connectionString: string) : unit =
        let serviceProvider =
            ServiceCollection()
                .AddFluentMigratorCore()
                .ConfigureRunner(fun builder ->
                    builder
                        .AddPostgres()
                        .WithGlobalConnectionString(connectionString)
                        .ScanIn(typeof<CreateUsersTable>.Assembly)
                        .For.Migrations()
                    |> ignore)
                .BuildServiceProvider()

        let scope =
            serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope()

        let runner =
            scope.ServiceProvider.GetRequiredService<IMigrationRunner>()

        runner.MigrateUp()

    let runIdentityMigrations(connectionString: string) : unit =
        let options =
            DbContextOptionsBuilder<IdentityDbContext>()
                .UseNpgsql(
                    connectionString,
                    fun o ->
                        o.MigrationsHistoryTable(Constants.MigrationsHistoryTableName, Constants.SchemaName)
                        |> ignore
                )
                .Options

        use identityContext =
            new IdentityDbContext(options)

        identityContext.Database.Migrate()
