namespace Wordfolio.Api.Tests

open System
open System.Net.Http
open System.Net.Http.Headers
open System.Threading.Tasks

open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Identity
open Microsoft.AspNetCore.Mvc.Testing
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting

open Wordfolio.Api.Tests.Utils
open Wordfolio.Api

type WebApplicationFactory(fixture: WordfolioIdentityTestFixture) =
    inherit WebApplicationFactory<Program.Program>()

    override _.ConfigureWebHost(builder: IWebHostBuilder) =
        builder.UseEnvironment(Environments.Development)
        |> ignore

        builder.UseSetting("ConnectionStrings:wordfoliodb", fixture.ConnectionString)
        |> ignore

        builder.ConfigureServices(fun services ->
            services.AddScoped<ITestTokenGenerator, TestTokenGenerator>()
            |> ignore)
        |> ignore

    member this.CreateAuthenticatedClientAsync(user: Identity.User) : Task<HttpClient> =
        task {
            use scope = this.Services.CreateScope()

            let tokenGenerator =
                scope.ServiceProvider.GetRequiredService<ITestTokenGenerator>()

            let! token = tokenGenerator.GenerateAccessTokenAsync(user)

            let client = this.CreateClient()
            client.DefaultRequestHeaders.Authorization <- AuthenticationHeaderValue("Bearer", token)
            return client
        }

    member this.CreateUserAsync
        (
            id: int,
            email: string,
            password: string
        ) : Task<Identity.User * Wordfolio.Mapping.User> =
        task {
            use scope = this.Services.CreateScope()

            let passwordHasher =
                scope.ServiceProvider.GetRequiredService<IPasswordHasher<Identity.User>>()

            let normalizer =
                scope.ServiceProvider.GetRequiredService<ILookupNormalizer>()

            let identityUser =
                Identity.User(
                    Id = id,
                    UserName = email,
                    Email = email,
                    NormalizedUserName = normalizer.NormalizeName(email),
                    NormalizedEmail = normalizer.NormalizeEmail(email),
                    SecurityStamp = Guid.NewGuid().ToString()
                )

            identityUser.PasswordHash <- passwordHasher.HashPassword(identityUser, password)

            return! fixture.CreateUserAsync identityUser
        }
