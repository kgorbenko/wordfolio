namespace Wordfolio.Api.Tests

open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Mvc.Testing
open Microsoft.Extensions.Hosting

type WebApplicationFactory(connectionString: string) =
    inherit WebApplicationFactory<Program.Program>()

    override _.ConfigureWebHost(builder: IWebHostBuilder) =
        builder.UseEnvironment(Environments.Development)
        |> ignore

        builder.UseSetting("ConnectionStrings:wordfoliodb", connectionString)
        |> ignore

        // Disable automatic migrations in tests - migrations are handled by test fixtures
        builder.UseSetting("RunMigrationsOnStartup", "false")
        |> ignore
