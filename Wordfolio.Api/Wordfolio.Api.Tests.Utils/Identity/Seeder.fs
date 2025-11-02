namespace Wordfolio.Api.Tests.Utils.Identity

open System
open System.Data.Common
open System.Threading.Tasks

open Microsoft.EntityFrameworkCore

open Wordfolio.Api.Identity
open Wordfolio.Common

type IdentitySeeder(context: IdentityDbContext) =
    member _.DbContext = context

    interface IDisposable with
        member this.Dispose() =
            (this.DbContext :> IDisposable).Dispose()

[<RequireQualifiedAccess>]
module Seeder =
    let create(connection: DbConnection) : IdentitySeeder =
        let builder =
            DbContextOptionsBuilder<IdentityDbContext>()

        builder.UseNpgsql(connection) |> ignore

        let context =
            new IdentityDbContext(builder.Options)

        new IdentitySeeder(context)

    let addUsers (users: User list) (seeder: IdentitySeeder) =
        seeder.DbContext.Users.AddRange(users)
        seeder

    let saveChangesAsync(seeder: IdentitySeeder) =
        task {
            do!
                seeder.DbContext.SaveChangesAsync()
                |> Task.ignore
        }

    let getAllUsersAsync(seeder: IdentitySeeder) : Task<User list> =
        task {
            let! users = seeder.DbContext.Users.AsNoTracking().ToArrayAsync()
            return users |> List.ofSeq
        }
