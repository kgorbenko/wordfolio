namespace Wordfolio.Api.Tests.Utils.Wordfolio

open System
open System.Data.Common
open System.Linq
open System.Threading.Tasks

open Microsoft.EntityFrameworkCore
open Microsoft.FSharp.Core.LanguagePrimitives

open Wordfolio.Api.DataAccess.Definitions
open Wordfolio.Api.DataAccess.Examples
open Wordfolio.Api.DataAccess.Translations
open Wordfolio.Common

type User = { Id: int }

type Collection =
    { Id: int
      UserId: int
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset option
      IsSystem: bool }

type Vocabulary =
    { Id: int
      CollectionId: int
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset option
      IsDefault: bool }

type Entry =
    { Id: int
      VocabularyId: int
      EntryText: string
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset option }

type Definition =
    { Id: int
      EntryId: int
      DefinitionText: string
      Source: DefinitionSource
      DisplayOrder: int }

type Translation =
    { Id: int
      EntryId: int
      TranslationText: string
      Source: TranslationSource
      DisplayOrder: int }

type Example =
    { Id: int
      DefinitionId: int option
      TranslationId: int option
      ExampleText: string
      Source: ExampleSource }

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
        (isSystem: bool)
        : Mapping.Collection =
        let collection: Mapping.Collection =
            { Id = 0
              UserId = user.Id
              Name = name
              Description = description |> Option.toObj
              CreatedAt = createdAt
              UpdatedAt = updatedAt |> Option.toNullable
              IsSystem = isSystem
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
        (isDefault: bool)
        : Mapping.Vocabulary =
        let vocabulary: Mapping.Vocabulary =
            { Id = 0
              CollectionId = collection.Id
              Name = name
              Description = description |> Option.toObj
              CreatedAt = createdAt
              UpdatedAt = updatedAt |> Option.toNullable
              IsDefault = isDefault
              Collection = collection
              Entries = ResizeArray() }

        collection.Vocabularies.Add(vocabulary)
        vocabulary

    let makeEntry
        (vocabulary: Mapping.Vocabulary)
        (entryText: string)
        (createdAt: DateTimeOffset)
        (updatedAt: DateTimeOffset option)
        : Mapping.Entry =
        let entry: Mapping.Entry =
            { Id = 0
              VocabularyId = vocabulary.Id
              EntryText = entryText
              CreatedAt = createdAt
              UpdatedAt = updatedAt |> Option.toNullable
              Vocabulary = vocabulary
              Definitions = ResizeArray()
              Translations = ResizeArray() }

        vocabulary.Entries.Add(entry)
        entry

    let makeDefinition
        (entry: Mapping.Entry)
        (definitionText: string)
        (source: DefinitionSource)
        (displayOrder: int)
        : Mapping.Definition =
        let definition: Mapping.Definition =
            { Id = 0
              EntryId = entry.Id
              DefinitionText = definitionText
              Source = int16 source
              DisplayOrder = displayOrder
              Entry = entry
              Examples = ResizeArray() }

        entry.Definitions.Add(definition)
        definition

    let makeTranslation
        (entry: Mapping.Entry)
        (translationText: string)
        (source: TranslationSource)
        (displayOrder: int)
        : Mapping.Translation =
        let translation: Mapping.Translation =
            { Id = 0
              EntryId = entry.Id
              TranslationText = translationText
              Source = int16 source
              DisplayOrder = displayOrder
              Entry = entry
              Examples = ResizeArray() }

        entry.Translations.Add(translation)
        translation

    let makeExampleForDefinition
        (definition: Mapping.Definition)
        (exampleText: string)
        (source: ExampleSource)
        : Mapping.Example =
        let example: Mapping.Example =
            { Id = 0
              DefinitionId = Nullable(definition.Id)
              TranslationId = Nullable()
              ExampleText = exampleText
              Source = int16 source
              Definition = definition
              Translation = Unchecked.defaultof<Mapping.Translation> }

        definition.Examples.Add(example)
        example

    let makeExampleForTranslation
        (translation: Mapping.Translation)
        (exampleText: string)
        (source: ExampleSource)
        : Mapping.Example =
        let example: Mapping.Example =
            { Id = 0
              DefinitionId = Nullable()
              TranslationId = Nullable(translation.Id)
              ExampleText = exampleText
              Source = int16 source
              Definition = Unchecked.defaultof<Mapping.Definition>
              Translation = translation }

        translation.Examples.Add(example)
        example

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
          UpdatedAt = Option.ofNullable entity.UpdatedAt
          IsSystem = entity.IsSystem }

    let private toVocabulary(entity: Mapping.Vocabulary) : Vocabulary =
        { Id = entity.Id
          CollectionId = entity.CollectionId
          Name = entity.Name
          Description = Option.ofObj entity.Description
          CreatedAt = entity.CreatedAt
          UpdatedAt = Option.ofNullable entity.UpdatedAt
          IsDefault = entity.IsDefault }

    let private toEntry(entity: Mapping.Entry) : Entry =
        { Id = entity.Id
          VocabularyId = entity.VocabularyId
          EntryText = entity.EntryText
          CreatedAt = entity.CreatedAt
          UpdatedAt = Option.ofNullable entity.UpdatedAt }

    let private toDefinition(entity: Mapping.Definition) : Definition =
        { Id = entity.Id
          EntryId = entity.EntryId
          DefinitionText = entity.DefinitionText
          Source = EnumOfValue<int16, DefinitionSource>(entity.Source)
          DisplayOrder = entity.DisplayOrder }

    let private toTranslation(entity: Mapping.Translation) : Translation =
        { Id = entity.Id
          EntryId = entity.EntryId
          TranslationText = entity.TranslationText
          Source = EnumOfValue<int16, TranslationSource>(entity.Source)
          DisplayOrder = entity.DisplayOrder }

    let private toExample(entity: Mapping.Example) : Example =
        { Id = entity.Id
          DefinitionId = Option.ofNullable entity.DefinitionId
          TranslationId = Option.ofNullable entity.TranslationId
          ExampleText = entity.ExampleText
          Source = EnumOfValue<int16, ExampleSource>(entity.Source) }

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

    let addEntries (entries: Mapping.Entry list) (seeder: WordfolioSeeder) : WordfolioSeeder =
        seeder.Context.Entries.AddRange(entries)
        seeder

    let addDefinitions (definitions: Mapping.Definition list) (seeder: WordfolioSeeder) : WordfolioSeeder =
        seeder.Context.Definitions.AddRange(definitions)
        seeder

    let addTranslations (translations: Mapping.Translation list) (seeder: WordfolioSeeder) : WordfolioSeeder =
        seeder.Context.Translations.AddRange(translations)
        seeder

    let addExamples (examples: Mapping.Example list) (seeder: WordfolioSeeder) : WordfolioSeeder =
        seeder.Context.Examples.AddRange(examples)
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

    let getAllEntriesAsync(seeder: WordfolioSeeder) : Task<Entry list> =
        task {
            let! entries = seeder.Context.Entries.AsNoTracking().ToArrayAsync()

            return entries |> Seq.map toEntry |> Seq.toList
        }

    let getEntryByIdAsync (id: int) (seeder: WordfolioSeeder) : Task<Entry option> =
        task {
            let! entry = seeder.Context.Entries.AsNoTracking().FirstOrDefaultAsync(fun e -> e.Id = id)

            return
                entry
                |> Option.ofObj
                |> Option.map toEntry
        }

    let getAllDefinitionsAsync(seeder: WordfolioSeeder) : Task<Definition list> =
        task {
            let! definitions = seeder.Context.Definitions.AsNoTracking().ToArrayAsync()

            return
                definitions
                |> Seq.map toDefinition
                |> Seq.toList
        }

    let getAllTranslationsAsync(seeder: WordfolioSeeder) : Task<Translation list> =
        task {
            let! translations = seeder.Context.Translations.AsNoTracking().ToArrayAsync()

            return
                translations
                |> Seq.map toTranslation
                |> Seq.toList
        }

    let getAllExamplesAsync(seeder: WordfolioSeeder) : Task<Example list> =
        task {
            let! examples = seeder.Context.Examples.AsNoTracking().ToArrayAsync()

            return
                examples
                |> Seq.map toExample
                |> Seq.toList
        }
