namespace Wordfolio.Api.Tests.Utils.Wordfolio

open System
open System.Collections.Generic
open System.Data.Common
open System.Threading.Tasks

open Microsoft.EntityFrameworkCore

open Wordfolio.Api.DataAccess
open Wordfolio.Common

[<CLIMutable>]
type UserEntity =
    { Id: int
      mutable Collections: ResizeArray<CollectionEntity> }

and [<CLIMutable>] CollectionEntity =
    { mutable Id: int
      UserId: int
      Name: string
      Description: string
      CreatedAt: DateTimeOffset
      UpdatedAt: Nullable<DateTimeOffset>
      mutable User: UserEntity
      mutable Vocabularies: ResizeArray<VocabularyEntity> }

and [<CLIMutable>] VocabularyEntity =
    { mutable Id: int
      CollectionId: int
      Name: string
      Description: string
      CreatedAt: DateTimeOffset
      UpdatedAt: Nullable<DateTimeOffset>
      mutable Collection: CollectionEntity }

type User = { Id: int }

type Collection =
    { Id: int
      UserId: int
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset option }

type Vocabulary =
    { Id: int
      CollectionId: int
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset option }

[<RequireQualifiedAccess>]
module Entities =
    let makeUser id : UserEntity = { Id = id; Collections = ResizeArray() }

    let makeCollection
        (user: UserEntity)
        (name: string)
        (description: string option)
        (createdAt: DateTimeOffset)
        (updatedAt: DateTimeOffset option)
        : CollectionEntity =
        let collection =
            { Id = 0
              UserId = user.Id
              Name = name
              Description = description |> Option.toObj
              CreatedAt = createdAt
              UpdatedAt = updatedAt |> Option.toNullable
              User = user
              Vocabularies = ResizeArray() }

        user.Collections.Add(collection)
        collection

    let makeVocabulary
        (collection: CollectionEntity)
        (name: string)
        (description: string option)
        (createdAt: DateTimeOffset)
        (updatedAt: DateTimeOffset option)
        : VocabularyEntity =
        let vocabulary =
            { Id = 0
              CollectionId = 0
              Name = name
              Description = description |> Option.toObj
              CreatedAt = createdAt
              UpdatedAt = updatedAt |> Option.toNullable
              Collection = collection }

        collection.Vocabularies.Add(vocabulary)
        vocabulary

type internal WordfolioTestDbContext(options: DbContextOptions<WordfolioTestDbContext>) =
    inherit DbContext(options)

    member this.Users: DbSet<UserEntity> = base.Set<UserEntity>()

    member this.Collections: DbSet<CollectionEntity> = base.Set<CollectionEntity>()

    member this.Vocabularies: DbSet<VocabularyEntity> = base.Set<VocabularyEntity>()

    override _.OnModelCreating(modelBuilder: ModelBuilder) =
        let users = modelBuilder.Entity<UserEntity>()

        users.ToTable(Schema.UsersTable.Name, Schema.Name).HasKey(fun x -> x.Id :> obj)
        |> ignore

        users.Property(_.Id).ValueGeneratedNever() |> ignore

        users
            .HasMany(fun u -> u.Collections :> IEnumerable<CollectionEntity>)
            .WithOne(fun c -> c.User)
            .HasForeignKey(fun c -> c.UserId :> obj)
        |> ignore

        let collections = modelBuilder.Entity<CollectionEntity>()

        collections
            .ToTable(Schema.CollectionsTable.Name, Schema.Name)
            .HasKey(fun x -> x.Id :> obj)
        |> ignore

        collections.Property(_.Id).ValueGeneratedOnAdd() |> ignore

        collections.Property(_.UserId).IsRequired() |> ignore

        collections
            .HasMany(fun c -> c.Vocabularies :> IEnumerable<VocabularyEntity>)
            .WithOne(fun v -> v.Collection)
            .HasForeignKey(fun v -> v.CollectionId :> obj)
        |> ignore

        collections.Property(_.Name).IsRequired().HasMaxLength(255) |> ignore

        collections.Property(_.Description).IsRequired(false) |> ignore

        collections.Property(_.CreatedAt).IsRequired() |> ignore

        collections.Property(_.UpdatedAt).IsRequired(false) |> ignore

        let vocabularies = modelBuilder.Entity<VocabularyEntity>()

        vocabularies
            .ToTable(Schema.VocabulariesTable.Name, Schema.Name)
            .HasKey(fun x -> x.Id :> obj)
        |> ignore

        vocabularies.Property(_.Id).ValueGeneratedOnAdd() |> ignore

        vocabularies.Property(_.CollectionId).IsRequired() |> ignore

        vocabularies.Property(_.Name).IsRequired().HasMaxLength(255) |> ignore

        vocabularies.Property(_.Description).IsRequired(false) |> ignore

        vocabularies.Property(_.CreatedAt).IsRequired() |> ignore

        vocabularies.Property(_.UpdatedAt).IsRequired(false) |> ignore

        base.OnModelCreating(modelBuilder)

type WordfolioSeeder internal (context: WordfolioTestDbContext) =
    member internal _.Context = context

    interface IDisposable with
        member this.Dispose() : unit = this.Context.Dispose()

[<RequireQualifiedAccess>]
module Seeder =
    let private toUser (entity: UserEntity) : User = { Id = entity.Id }

    let private toCollection (entity: CollectionEntity) : Collection =
        { Id = entity.Id
          UserId = entity.UserId
          Name = entity.Name
          Description = Option.ofObj entity.Description
          CreatedAt = entity.CreatedAt
          UpdatedAt = Option.ofNullable entity.UpdatedAt }

    let private toVocabulary (entity: VocabularyEntity) : Vocabulary =
        { Id = entity.Id
          CollectionId = entity.CollectionId
          Name = entity.Name
          Description = Option.ofObj entity.Description
          CreatedAt = entity.CreatedAt
          UpdatedAt = Option.ofNullable entity.UpdatedAt }

    let create (connection: DbConnection) : WordfolioSeeder =
        let builder = DbContextOptionsBuilder<WordfolioTestDbContext>()

        builder
            .UseNpgsql(connection)
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
        |> ignore

        let context = new WordfolioTestDbContext(builder.Options)
        new WordfolioSeeder(context)

    let addUsers (users: UserEntity list) (seeder: WordfolioSeeder) : WordfolioSeeder =
        seeder.Context.Users.AddRange(users)
        seeder

    let saveChangesAsync (seeder: WordfolioSeeder) : Task =
        task {
            do! seeder.Context.SaveChangesAsync() |> Task.ignore
            seeder.Context.ChangeTracker.Clear()
        }

    let getAllUsersAsync (seeder: WordfolioSeeder) : Task<User list> =
        task {
            let! users = seeder.Context.Users.ToArrayAsync()
            return users |> Seq.map toUser |> Seq.toList
        }

    let getAllCollectionsAsync (seeder: WordfolioSeeder) : Task<Collection list> =
        task {
            let! collections = seeder.Context.Collections.ToArrayAsync()
            return collections |> Seq.map toCollection |> Seq.toList
        }

    let getAllVocabulariesAsync (seeder: WordfolioSeeder) : Task<Vocabulary list> =
        task {
            let! vocabularies = seeder.Context.Vocabularies.ToArrayAsync()
            return vocabularies |> Seq.map toVocabulary |> Seq.toList
        }

    let getCollectionByIdAsync (id: int) (seeder: WordfolioSeeder) : Task<Collection option> =
        task {
            let! collection =
                seeder.Context.Collections
                    .AsNoTracking()
                    .FirstOrDefaultAsync(fun c -> c.Id = id)

            return
                if isNull (box collection) then
                    None
                else
                    Some(toCollection collection)
        }

    let getVocabularyByIdAsync (id: int) (seeder: WordfolioSeeder) : Task<Vocabulary option> =
        task {
            let! vocabulary =
                seeder.Context.Vocabularies
                    .AsNoTracking()
                    .FirstOrDefaultAsync(fun v -> v.Id = id)

            return
                if isNull (box vocabulary) then
                    None
                else
                    Some(toVocabulary vocabulary)
        }
