namespace Wordfolio.Api.Tests.Utils.Wordfolio

open System
open System.Data.Common
open System.Threading.Tasks

open Microsoft.EntityFrameworkCore

open Wordfolio.Api.DataAccess
open Wordfolio.Common

[<CLIMutable>]
type UserEntity = { Id: int }

[<CLIMutable>]
type CollectionEntity =
    { Id: int
      UserId: int
      Name: string
      Description: string
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset }

[<CLIMutable>]
type VocabularyEntity =
    { Id: int
      CollectionId: int
      Name: string
      Description: string
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset }

type WordfolioTestDbContext(options: DbContextOptions<WordfolioTestDbContext>) =
    inherit DbContext(options)

    member this.Users: DbSet<UserEntity> =
        base.Set<UserEntity>()

    member this.Collections: DbSet<CollectionEntity> =
        base.Set<CollectionEntity>()

    member this.Vocabularies: DbSet<VocabularyEntity> =
        base.Set<VocabularyEntity>()

    override _.OnModelCreating(modelBuilder: ModelBuilder) =
        let users =
            modelBuilder.Entity<UserEntity>()

        users.ToTable(Schema.UsersTable.Name, Schema.Name).HasKey(fun x -> x.Id :> obj)
        |> ignore

        users.Property(_.Id).ValueGeneratedNever()
        |> ignore

        let collections =
            modelBuilder.Entity<CollectionEntity>()

        collections
            .ToTable(Schema.CollectionsTable.Name, Schema.Name)
            .HasKey(fun x -> x.Id :> obj)
        |> ignore

        collections.Property(_.Id).ValueGeneratedNever()
        |> ignore

        collections.Property(_.UserId).IsRequired()
        |> ignore

        collections.Property(_.Name).IsRequired().HasMaxLength(255)
        |> ignore

        collections.Property(_.Description).IsRequired(false)
        |> ignore

        collections.Property(_.CreatedAt).IsRequired()
        |> ignore

        collections.Property(_.UpdatedAt).IsRequired()
        |> ignore

        let vocabularies =
            modelBuilder.Entity<VocabularyEntity>()

        vocabularies
            .ToTable(Schema.VocabulariesTable.Name, Schema.Name)
            .HasKey(fun x -> x.Id :> obj)
        |> ignore

        vocabularies.Property(_.Id).ValueGeneratedNever()
        |> ignore

        vocabularies.Property(_.CollectionId).IsRequired()
        |> ignore

        vocabularies.Property(_.Name).IsRequired().HasMaxLength(255)
        |> ignore

        vocabularies.Property(_.Description).IsRequired(false)
        |> ignore

        vocabularies.Property(_.CreatedAt).IsRequired()
        |> ignore

        vocabularies.Property(_.UpdatedAt).IsRequired()
        |> ignore

        base.OnModelCreating(modelBuilder)

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

    let addCollections (collections: CollectionEntity list) (seeder: WordfolioSeeder) =
        do seeder.DbContext.Collections.AddRange(collections)
        seeder

    let getAllCollectionsAsync(seeder: WordfolioSeeder) : Task<CollectionEntity list> =
        task {
            let! collections = seeder.DbContext.Collections.ToArrayAsync()
            return collections |> List.ofSeq
        }

    let getCollectionByIdAsync (id: int) (seeder: WordfolioSeeder) : Task<CollectionEntity option> =
        task {
            let! collection = seeder.DbContext.Collections.FindAsync(id)
            return collection |> Option.ofObj
        }

    let addVocabularies (vocabularies: VocabularyEntity list) (seeder: WordfolioSeeder) =
        do seeder.DbContext.Vocabularies.AddRange(vocabularies)
        seeder

    let getAllVocabulariesAsync(seeder: WordfolioSeeder) : Task<VocabularyEntity list> =
        task {
            let! vocabularies = seeder.DbContext.Vocabularies.ToArrayAsync()
            return vocabularies |> List.ofSeq
        }
