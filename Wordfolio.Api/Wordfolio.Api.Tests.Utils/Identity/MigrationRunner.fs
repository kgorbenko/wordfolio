module Wordfolio.Api.Tests.Utils.Identity.MigrationRunner

open Microsoft.EntityFrameworkCore

open Wordfolio.Api.Identity

let run(connectionString: string) =
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
