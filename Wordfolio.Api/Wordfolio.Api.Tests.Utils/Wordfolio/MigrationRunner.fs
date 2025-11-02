module Wordfolio.Api.Tests.Utils.Wordfolio.MigrationRunner

open Microsoft.Extensions.DependencyInjection

open FluentMigrator.Runner

open Wordfolio.Api.Migrations

let run(connectionString: string) : unit =
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
            .BuildServiceProvider()

    use scope =
        serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope()

    let runner =
        scope.ServiceProvider.GetRequiredService<IMigrationRunner>()

    runner.MigrateUp()
