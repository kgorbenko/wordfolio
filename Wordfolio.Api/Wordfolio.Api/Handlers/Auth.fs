module Wordfolio.Api.Handlers.Auth

open System

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Identity
open Microsoft.AspNetCore.Routing
open Microsoft.Extensions.Options

open Wordfolio.Api.Identity

module Urls =
    [<Literal>]
    let PasswordRequirements =
        "/password-requirements"

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
            Urls.PasswordRequirements,
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
