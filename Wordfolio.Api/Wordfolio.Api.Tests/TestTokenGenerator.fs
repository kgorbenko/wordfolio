namespace Wordfolio.Api.Tests

open System
open System.Threading.Tasks

open Microsoft.AspNetCore.Authentication
open Microsoft.AspNetCore.Authentication.BearerToken
open Microsoft.AspNetCore.Identity
open Microsoft.Extensions.Options

open Wordfolio.Api.Identity

type ITestTokenGenerator =
    abstract member GenerateAccessTokenAsync: User -> Task<string>

type TestTokenGenerator
    (
        claimsFactory: IUserClaimsPrincipalFactory<User>,
        bearerOptions: IOptionsMonitor<BearerTokenOptions>,
        timeProvider: TimeProvider
    ) =

    interface ITestTokenGenerator with
        member _.GenerateAccessTokenAsync(user: User) : Task<string> =
            task {
                let! principal = claimsFactory.CreateAsync(user)

                let options =
                    bearerOptions.Get(IdentityConstants.BearerScheme)

                let utcNow = timeProvider.GetUtcNow()

                let ticket =
                    AuthenticationTicket(
                        principal,
                        AuthenticationProperties(
                            ExpiresUtc = Nullable(utcNow + options.BearerTokenExpiration),
                            IssuedUtc = Nullable(utcNow)
                        ),
                        $"{IdentityConstants.BearerScheme}:AccessToken"
                    )

                return options.BearerTokenProtector.Protect(ticket)
            }
