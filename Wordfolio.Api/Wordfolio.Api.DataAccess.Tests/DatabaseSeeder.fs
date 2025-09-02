namespace Wordfolio.Api.DataAccess.Tests

open System
open System.Data.Common
open System.Threading.Tasks

open Microsoft.EntityFrameworkCore

open Wordfolio.Api.DataAccess
open Wordfolio.Common

[<CLIMutable>]
type UserEntity = {
    Id: int
}

type TestDbContext(options: DbContextOptions<TestDbContext>) =
    inherit DbContext(options)

    member this.Users with get(): DbSet<UserEntity> = base.Set<UserEntity>()

    override _.OnModelCreating(modelBuilder: ModelBuilder) =
        let users = modelBuilder.Entity<UserEntity>()

        users
            .ToTable(Schema.UsersTable.Name, Schema.Name)
            .HasKey(fun x -> x.Id :> obj)
        |> ignore

        users.Property(_.Id).ValueGeneratedNever() |> ignore

        base.OnModelCreating(modelBuilder)

type TestDatabaseSeeder(DbContext: TestDbContext) =
    member this.DbContext with get() = DbContext

    interface IDisposable with
        member this.Dispose (): unit =
            this.DbContext.Dispose()

[<RequireQualifiedAccess>]
module DatabaseSeeder =
    let create (connection: DbConnection): TestDatabaseSeeder =
        let builder = DbContextOptionsBuilder<TestDbContext>()
        builder.UseNpgsql(connection) |> ignore
        let context = new TestDbContext(builder.Options)
        new TestDatabaseSeeder(context)

    let addUsers (users: UserEntity list) (seeder: TestDatabaseSeeder) =
        do seeder.DbContext.Users.AddRange(users)
        seeder

    let saveChangesAsync (seeder: TestDatabaseSeeder) = task {
        do! seeder.DbContext.SaveChangesAsync() |> Task.ignore
    }

    let getAllUsersAsync (seeder: TestDatabaseSeeder): Task<UserEntity list> = task {
        let! users = seeder.DbContext.Users.ToArrayAsync()
        return users |> List.ofSeq
    }
