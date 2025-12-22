namespace Wordfolio.Api.Tests

open System.Net.Http
open System.Net.Http.Headers
open System.Threading.Tasks

open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Mvc.Testing
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting

open Wordfolio.Api.Identity

type WebApplicationFactory(connectionString: string) =
    inherit WebApplicationFactory<Program.Program>()

    override _.ConfigureWebHost(builder: IWebHostBuilder) =
        builder.UseEnvironment(Environments.Development)
        |> ignore

        builder.UseSetting("ConnectionStrings:wordfoliodb", connectionString)
        |> ignore

        builder.ConfigureServices(fun services ->
            services.AddScoped<ITestTokenGenerator, TestTokenGenerator>()
            |> ignore)
        |> ignore

    member this.CreateAuthenticatedClientAsync(user: User) : Task<HttpClient> =
        task {
            use scope = this.Services.CreateScope()

            let tokenGenerator =
                scope.ServiceProvider.GetRequiredService<ITestTokenGenerator>()

            let! token = tokenGenerator.GenerateAccessTokenAsync(user)

            let client = this.CreateClient()
            client.DefaultRequestHeaders.Authorization <- AuthenticationHeaderValue("Bearer", token)
            return client
        }
