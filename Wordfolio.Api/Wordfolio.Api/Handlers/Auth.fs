module Wordfolio.Api.Handlers.Auth

open System

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Identity
open Microsoft.AspNetCore.Routing
open Microsoft.Extensions.Options

open Wordfolio.Api
open Wordfolio.Api.Identity

type PasswordRequirements =
    { RequiredLength: int
      RequireDigit: bool
      RequireLowercase: bool
      RequireUppercase: bool
      RequireNonAlphanumeric: bool
      RequiredUniqueChars: int }

let mapAuthEndpoints(group: RouteGroupBuilder) =
    group.MapIdentityApi<User>().AllowAnonymous()
    |> ignore

    group
        .MapGet(
            Urls.Auth.PasswordRequirements,
            Func<IOptions<IdentityOptions>, PasswordRequirements>(fun identityOptions ->
                let passwordOptions =
                    identityOptions.Value.Password

                { RequiredLength = passwordOptions.RequiredLength
                  RequireDigit = passwordOptions.RequireDigit
                  RequireLowercase = passwordOptions.RequireLowercase
                  RequireUppercase = passwordOptions.RequireUppercase
                  RequireNonAlphanumeric = passwordOptions.RequireNonAlphanumeric
                  RequiredUniqueChars = passwordOptions.RequiredUniqueChars })
        )
        .AllowAnonymous()
    |> ignore
