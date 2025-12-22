namespace Wordfolio.Api.Tests.Utils.Wordfolio

open System
open System.Data.Common
open System.Threading.Tasks

open Microsoft.EntityFrameworkCore

open Wordfolio.Common

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
    let makeUser id : Mapping.User =
        { Id = id; Collections = ResizeArray() }

    let makeCollection
        (user: Mapping.User)
        (name: string)
        (description: string option)
        (createdAt: DateTimeOffset)
        (updatedAt: DateTimeOffset option)
        : Mapping.Collection =
        let collection: Mapping.Collection =
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
        (collection: Mapping.Collection)
        (name: string)
        (description: string option)
        (createdAt: DateTimeOffset)
        (updatedAt: DateTimeOffset option)
        : Mapping.Vocabulary =
        let vocabulary: Mapping.Vocabulary =
            { Id = 0
              CollectionId = 0
              Name = name
              Description = description |> Option.toObj
              CreatedAt = createdAt
              UpdatedAt = updatedAt |> Option.toNullable
              Collection = collection }

        collection.Vocabularies.Add(vocabulary)
        vocabulary

type WordfolioSeeder internal (context: Mapping.WordfolioTestDbContext) =
    member internal _.Context = context

    interface IDisposable with
        member this.Dispose() : unit = this.Context.Dispose()

[<RequireQualifiedAccess>]
module Seeder =
    let private toUser(entity: Mapping.User) : User = { Id = entity.Id }

    let private toCollection(entity: Mapping.Collection) : Collection =
        { Id = entity.Id
          UserId = entity.UserId
          Name = entity.Name
          Description = Option.ofObj entity.Description
          CreatedAt = entity.CreatedAt
          UpdatedAt = Option.ofNullable entity.UpdatedAt }

    let private toVocabulary(entity: Mapping.Vocabulary) : Vocabulary =
        { Id = entity.Id
          CollectionId = entity.CollectionId
          Name = entity.Name
          Description = Option.ofObj entity.Description
          CreatedAt = entity.CreatedAt
          UpdatedAt = Option.ofNullable entity.UpdatedAt }

    let create(connection: DbConnection) : WordfolioSeeder =
        let builder =
            DbContextOptionsBuilder<Mapping.WordfolioTestDbContext>()
                .UseNpgsql(connection)
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)

        let context =
            new Mapping.WordfolioTestDbContext(builder.Options)

        new WordfolioSeeder(context)

    let addUsers (users: Mapping.User list) (seeder: WordfolioSeeder) : WordfolioSeeder =
        seeder.Context.Users.AddRange(users)
        seeder

    let addCollections (collections: Mapping.Collection list) (seeder: WordfolioSeeder) : WordfolioSeeder =
        seeder.Context.Collections.AddRange(collections)
        seeder

    let addVocabularies (vocabularies: Mapping.Vocabulary list) (seeder: WordfolioSeeder) : WordfolioSeeder =
        seeder.Context.Vocabularies.AddRange(vocabularies)
        seeder

    let saveChangesAsync(seeder: WordfolioSeeder) : Task =
        task {
            do!
                seeder.Context.SaveChangesAsync()
                |> Task.ignore

            seeder.Context.ChangeTracker.Clear()
        }

    let getAllUsersAsync(seeder: WordfolioSeeder) : Task<User list> =
        task {
            let! users = seeder.Context.Users.ToArrayAsync()
            return users |> Seq.map toUser |> Seq.toList
        }

    let getAllCollectionsAsync(seeder: WordfolioSeeder) : Task<Collection list> =
        task {
            let! collections = seeder.Context.Collections.ToArrayAsync()

            return
                collections
                |> Seq.map toCollection
                |> Seq.toList
        }

    let getAllVocabulariesAsync(seeder: WordfolioSeeder) : Task<Vocabulary list> =
        task {
            let! vocabularies = seeder.Context.Vocabularies.ToArrayAsync()

            return
                vocabularies
                |> Seq.map toVocabulary
                |> Seq.toList
        }

    let getCollectionByIdAsync (id: int) (seeder: WordfolioSeeder) : Task<Collection option> =
        task {
            let! collection =
                seeder.Context.Collections
                    .AsNoTracking()
                    .FirstOrDefaultAsync(fun c -> c.Id = id)

            return
                collection
                |> Option.ofObj
                |> Option.map toCollection
        }

    let getVocabularyByIdAsync (id: int) (seeder: WordfolioSeeder) : Task<Vocabulary option> =
        task {
            let! vocabulary =
                seeder.Context.Vocabularies
                    .AsNoTracking()
                    .FirstOrDefaultAsync(fun v -> v.Id = id)

            return
                vocabulary
                |> Option.ofObj
                |> Option.map toVocabulary
        }
