namespace Wordfolio.Api.Tests.Utils.Wordfolio

open System
open System.Collections.Generic
open System.Data.Common
open System.Threading.Tasks

open Microsoft.EntityFrameworkCore

open Wordfolio.Api.DataAccess
open Wordfolio.Common

// ===== Seed Input Types (what tests provide for seeding) =====

type UserSeed = { Id: int }

type VocabularySeed =
    { Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset option }

type CollectionSeed =
    { UserId: int
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset option
      Vocabularies: VocabularySeed list }

// ===== Query/Result Types (what tests use for assertions) =====

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

type SeededData =
    { Users: User list
      Collections: Collection list
      Vocabularies: Vocabulary list }

// ===== Internal EF Entities (for EF relationships) =====

[<CLIMutable>]
type internal UserEntity =
    { Id: int
      mutable Collections: ResizeArray<CollectionEntity> }

and [<CLIMutable>] internal CollectionEntity =
    { mutable Id: int
      UserId: int
      Name: string
      Description: string
      CreatedAt: DateTimeOffset
      UpdatedAt: Nullable<DateTimeOffset>
      mutable User: UserEntity
      mutable Vocabularies: ResizeArray<VocabularyEntity> }

and [<CLIMutable>] internal VocabularyEntity =
    { mutable Id: int
      CollectionId: int
      Name: string
      Description: string
      CreatedAt: DateTimeOffset
      UpdatedAt: Nullable<DateTimeOffset>
      mutable Collection: CollectionEntity }

// ===== Internal DbContext =====

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

// ===== Seeder Type =====

type WordfolioSeeder =
    internal
        { Context: WordfolioTestDbContext
          mutable PendingUsers: UserEntity list
          mutable PendingCollections: CollectionEntity list }

    interface IDisposable with
        member this.Dispose() : unit = this.Context.Dispose()

// ===== Seeder Module =====

[<RequireQualifiedAccess>]
module Seeder =

    // ===== Private Conversion Functions =====

    let private toUserEntity (seed: UserSeed) : UserEntity =
        { Id = seed.Id
          Collections = ResizeArray() }

    let private toCollectionEntity (seed: CollectionSeed) : CollectionEntity * VocabularyEntity list =
        let collection: CollectionEntity =
            { Id = 0
              UserId = seed.UserId
              Name = seed.Name
              Description = seed.Description |> Option.toObj
              CreatedAt = seed.CreatedAt
              UpdatedAt = seed.UpdatedAt |> Option.toNullable
              User = Unchecked.defaultof<UserEntity>
              Vocabularies = ResizeArray() }

        let vocabularies =
            seed.Vocabularies
            |> List.map (fun v ->
                let vocab: VocabularyEntity =
                    { Id = 0
                      CollectionId = 0
                      Name = v.Name
                      Description = v.Description |> Option.toObj
                      CreatedAt = v.CreatedAt
                      UpdatedAt = v.UpdatedAt |> Option.toNullable
                      Collection = Unchecked.defaultof<CollectionEntity> }

                collection.Vocabularies.Add(vocab)
                vocab)

        (collection, vocabularies)

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

    // ===== Create =====

    let create (connection: DbConnection) : WordfolioSeeder =
        let builder = DbContextOptionsBuilder<WordfolioTestDbContext>()

        builder
            .UseNpgsql(connection)
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
        |> ignore

        let context = new WordfolioTestDbContext(builder.Options)

        { Context = context
          PendingUsers = []
          PendingCollections = [] }

    // ===== Add Functions (fluent API) =====

    let addUsers (users: UserSeed list) (seeder: WordfolioSeeder) : WordfolioSeeder =
        let entities = users |> List.map toUserEntity
        seeder.PendingUsers <- seeder.PendingUsers @ entities
        seeder

    let addCollections (collections: CollectionSeed list) (seeder: WordfolioSeeder) : WordfolioSeeder =
        let entities =
            collections
            |> List.map (fun seed ->
                let (collectionEntity, _) = toCollectionEntity seed

                let userEntity =
                    seeder.PendingUsers |> List.tryFind (fun u -> u.Id = seed.UserId)

                match userEntity with
                | Some user -> user.Collections.Add(collectionEntity)
                | None -> ()

                collectionEntity)

        seeder.PendingCollections <- seeder.PendingCollections @ entities
        seeder

    // ===== Save =====

    let saveChangesAsync (seeder: WordfolioSeeder) : Task<SeededData> =
        task {
            seeder.Context.Users.AddRange(seeder.PendingUsers)

            do! seeder.Context.SaveChangesAsync() |> Task.ignore

            let users = seeder.PendingUsers |> List.map toUser

            let collections = seeder.PendingCollections |> List.map toCollection

            let vocabularies =
                seeder.PendingCollections
                |> List.collect (fun c -> c.Vocabularies |> Seq.toList)
                |> List.map toVocabulary

            seeder.Context.ChangeTracker.Clear()
            seeder.PendingUsers <- []
            seeder.PendingCollections <- []

            return
                { Users = users
                  Collections = collections
                  Vocabularies = vocabularies }
        }

    // ===== Query Functions =====

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
