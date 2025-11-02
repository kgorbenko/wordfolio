namespace Wordfolio.Api.Tests.Utils.Wordfolio

open System
open System.Data.Common
open System.Threading.Tasks

open Microsoft.EntityFrameworkCore

open Wordfolio.Api.DataAccess
open Wordfolio.Common

[<CLIMutable>]
type UserEntity = { Id: int }

type WordfolioTestDbContext(options: DbContextOptions<WordfolioTestDbContext>) =
    inherit DbContext(options)

    member this.Users: DbSet<UserEntity> =
        base.Set<UserEntity>()

    override _.OnModelCreating(modelBuilder: ModelBuilder) =
        let users =
            modelBuilder.Entity<UserEntity>()

        users.ToTable(Schema.UsersTable.Name, Schema.Name).HasKey(fun x -> x.Id :> obj)
        |> ignore

        users.Property(_.Id).ValueGeneratedNever()
        |> ignore

        base.OnModelCreating(modelBuilder)

[<Sealed>]
type WordfolioSeeder(context: WordfolioTestDbContext) =
    member _.DbContext = context

    interface IDisposable with
        member this.Dispose() : unit = this.DbContext.Dispose()

[<RequireQualifiedAccess>]
module Seeder =
    let create(connection: DbConnection) : WordfolioSeeder =
        let builder =
            DbContextOptionsBuilder<WordfolioTestDbContext>()

        builder.UseNpgsql(connection) |> ignore

        let context =
            new WordfolioTestDbContext(builder.Options)

        new WordfolioSeeder(context)

    let addUsers (users: UserEntity list) (seeder: WordfolioSeeder) =
        do seeder.DbContext.Users.AddRange(users)
        seeder

    let saveChangesAsync(seeder: WordfolioSeeder) =
        task {
            do!
                seeder.DbContext.SaveChangesAsync()
                |> Task.ignore
        }

    let getAllUsersAsync(seeder: WordfolioSeeder) : Task<UserEntity list> =
        task {
            let! users = seeder.DbContext.Users.ToArrayAsync()
            return users |> List.ofSeq
        }
