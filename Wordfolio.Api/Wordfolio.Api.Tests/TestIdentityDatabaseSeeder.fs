namespace Wordfolio.Api.Tests

open System
open System.Data.Common
open System.Threading.Tasks

open Microsoft.EntityFrameworkCore

open Wordfolio.Api.Identity
open Wordfolio.Common

[<CLIMutable>]
type IdentityUserEntity =
    { Id: int
      UserName: string
      NormalizedUserName: string
      Email: string
      NormalizedEmail: string
      EmailConfirmed: bool
      PasswordHash: string }

type TestIdentityDbContext(options: DbContextOptions<TestIdentityDbContext>) =
    inherit DbContext(options)

    member this.Users: DbSet<IdentityUserEntity> =
        base.Set<IdentityUserEntity>()

    override _.OnModelCreating(modelBuilder: ModelBuilder) =
        let users =
            modelBuilder.Entity<IdentityUserEntity>()

        users.ToTable("AspNetUsers", Constants.SchemaName).HasKey(fun x -> x.Id :> obj)
        |> ignore

        users.Property(fun x -> x.Id).ValueGeneratedOnAdd()
        |> ignore

        users.Property(fun x -> x.UserName)
        |> ignore

        users.Property(fun x -> x.NormalizedUserName)
        |> ignore

        users.Property(fun x -> x.Email)
        |> ignore

        users.Property(fun x -> x.NormalizedEmail)
        |> ignore

        users.Property(fun x -> x.EmailConfirmed)
        |> ignore

        users.Property(fun x -> x.PasswordHash)
        |> ignore

        base.OnModelCreating(modelBuilder)

[<Sealed>]
type TestIdentityDatabaseSeeder(context: TestIdentityDbContext) =
    member _.DbContext = context

    interface IDisposable with
        member this.Dispose() =
            (this.DbContext :> IDisposable).Dispose()

[<RequireQualifiedAccess>]
module IdentityDatabaseSeeder =
    let create(connection: DbConnection) : TestIdentityDatabaseSeeder =
        let builder =
            DbContextOptionsBuilder<TestIdentityDbContext>()

        builder.UseNpgsql(connection) |> ignore

        let context =
            new TestIdentityDbContext(builder.Options)

        new TestIdentityDatabaseSeeder(context)

    let addUsers (users: IdentityUserEntity list) (seeder: TestIdentityDatabaseSeeder) =
        seeder.DbContext.Users.AddRange(users)
        |> ignore

        seeder

    let saveChangesAsync(seeder: TestIdentityDatabaseSeeder) =
        task {
            do!
                seeder.DbContext.SaveChangesAsync()
                |> Task.ignore
        }

    let getAllUsersAsync(seeder: TestIdentityDatabaseSeeder) : Task<IdentityUserEntity list> =
        task {
            let! users = seeder.DbContext.Users.AsNoTracking().ToArrayAsync()
            return users |> List.ofSeq
        }
