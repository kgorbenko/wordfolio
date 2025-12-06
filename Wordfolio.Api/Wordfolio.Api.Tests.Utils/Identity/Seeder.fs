namespace Wordfolio.Api.Tests.Utils.Identity

open System
open System.Data.Common
open System.Threading.Tasks

open Microsoft.EntityFrameworkCore

open Wordfolio.Api.Identity
open Wordfolio.Common

type User =
    { Id: int
      UserName: string option
      Email: string option }

type IdentitySeeder(context: IdentityDbContext) =
    member _.DbContext = context

    interface IDisposable with
        member this.Dispose() =
            (this.DbContext :> IDisposable).Dispose()

[<RequireQualifiedAccess>]
module Seeder =
    let private toUser(entity: Wordfolio.Api.Identity.User) : User =
        { Id = entity.Id
          UserName = Option.ofObj entity.UserName
          Email = Option.ofObj entity.Email }

    let create(connection: DbConnection) : IdentitySeeder =
        let builder =
            DbContextOptionsBuilder<IdentityDbContext>()
                .UseNpgsql(connection)
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)

        let context =
            new IdentityDbContext(builder.Options)

        new IdentitySeeder(context)

    let addUsers (users: Wordfolio.Api.Identity.User list) (seeder: IdentitySeeder) =
        seeder.DbContext.Users.AddRange(users)
        seeder

    let saveChangesAsync(seeder: IdentitySeeder) =
        task {
            do!
                seeder.DbContext.SaveChangesAsync()
                |> Task.ignore

            seeder.DbContext.ChangeTracker.Clear()
        }

    let getAllUsersAsync(seeder: IdentitySeeder) : Task<User list> =
        task {
            let! users = seeder.DbContext.Users.ToArrayAsync()
            return users |> Seq.map toUser |> Seq.toList
        }
