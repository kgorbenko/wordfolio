module Wordfolio.Api.IdentityIntegration

open System.Data
open System.Threading
open System.Threading.Tasks

open Microsoft.AspNetCore.Authorization
open Microsoft.AspNetCore.Identity
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting

open Wordfolio.Api.Identity

module DataAccess = Wordfolio.Api.DataAccess.Users

type UserStoreExtension() =
    interface IUserStoreExtension with
        member _.OnAfterUserCreatedAsync (parameters: UserCreationParameters, connection: IDbConnection, transaction: IDbTransaction, cancellationToken: CancellationToken): Task =
            let dataAccessParameters =
                { Id = parameters.Id }
                : DataAccess.UserCreationParameters

            DataAccess.createUserAsync dataAccessParameters connection transaction cancellationToken

let addIdentity<'TBuilder when 'TBuilder :> IHostApplicationBuilder> (builder: 'TBuilder) =
    builder.AddNpgsqlDbContext<IdentityDbContext>("wordfoliodb")

    builder.Services.AddScoped<IUserStoreExtension, UserStoreExtension>()
    |> ignore

    builder.Services.AddIdentityCore<User>()
        .AddEntityFrameworkStores<IdentityDbContext>()
        .AddUserStore<UserStore>()
        .AddApiEndpoints()
    |> ignore

    builder

let addAuthentication<'TBuilder when 'TBuilder :> IHostApplicationBuilder> (builder: 'TBuilder) =
    builder.Services.AddAuthentication().AddBearerToken(IdentityConstants.BearerScheme)
    |> ignore

    builder.Services.AddAuthorization(fun options ->
        options.FallbackPolicy <- AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()
    ) |> ignore

    builder