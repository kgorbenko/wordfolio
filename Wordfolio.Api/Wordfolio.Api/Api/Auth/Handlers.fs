module Wordfolio.Api.Api.Auth.Handlers

open System

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Identity
open Microsoft.AspNetCore.Routing
open Microsoft.Extensions.Options

open Wordfolio.Api
open Wordfolio.Api.Api.Auth.Mappers
open Wordfolio.Api.Api.Auth.Types
open Wordfolio.Api.Identity

let mapAuthEndpoints(group: RouteGroupBuilder) =
    group.MapIdentityApi<User>().AllowAnonymous()
    |> ignore

    group
        .MapGet(
            Urls.Auth.PasswordRequirements,
            Func<IOptions<IdentityOptions>, PasswordRequirementsResponse>(fun identityOptions ->
                identityOptions.Value.Password
                |> toPasswordRequirementsResponse)
        )
        .AllowAnonymous()
    |> ignore
