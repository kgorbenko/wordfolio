namespace Wordfolio.Api.Tests

open System
open System.Data.Common
open System.Threading.Tasks

open Microsoft.EntityFrameworkCore

open Wordfolio.Api.Identity
open Wordfolio.Common

[<Sealed>]
type TestIdentityDatabaseSeeder(context: IdentityDbContext) =
    member _.DbContext = context

    interface IDisposable with
        member this.Dispose() =
            (this.DbContext :> IDisposable).Dispose()

[<RequireQualifiedAccess>]
module IdentityDatabaseSeeder =
    let create(connection: DbConnection) : TestIdentityDatabaseSeeder =
        let builder =
            DbContextOptionsBuilder<IdentityDbContext>()

        builder.UseNpgsql(connection) |> ignore

        let context =
            new IdentityDbContext(builder.Options)

        new TestIdentityDatabaseSeeder(context)

    let addUsers (users: User list) (seeder: TestIdentityDatabaseSeeder) =
        seeder.DbContext.Users.AddRange(users)
        seeder

    let saveChangesAsync(seeder: TestIdentityDatabaseSeeder) =
        task {
            do!
                seeder.DbContext.SaveChangesAsync()
                |> Task.ignore
        }

    let getAllUsersAsync(seeder: TestIdentityDatabaseSeeder) : Task<User list> =
        task {
            let! users = seeder.DbContext.Users.AsNoTracking().ToArrayAsync()
            return users |> List.ofSeq
        }
