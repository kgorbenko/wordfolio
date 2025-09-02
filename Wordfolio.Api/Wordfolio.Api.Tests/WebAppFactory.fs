namespace Wordfolio.Api.Tests

open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Mvc.Testing
open Microsoft.Extensions.Hosting

open Wordfolio.Api.IdentityIntegration

type WebAppFactory() =
    inherit WebApplicationFactory<UserStoreExtension>()
    override _.ConfigureWebHost(builder: IWebHostBuilder) =
        builder.UseEnvironment(Environments.Development) |> ignore
