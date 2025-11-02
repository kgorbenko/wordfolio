namespace Wordfolio.Api.Tests.Utils

open Microsoft.EntityFrameworkCore

open Wordfolio.Api.Identity

[<RequireQualifiedAccess>]
module SchemaMigrations =
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
