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
      UpdatedAt: DateTimeOffset
      IsSystem: bool }

type Vocabulary =
    { Id: int
      CollectionId: int
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset
      IsDefault: bool }

type Entry =
    { Id: int
      VocabularyId: int
      EntryText: string
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset }

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

type ExerciseSession =
    { Id: int
      UserId: int
      ExerciseType: int16
      CreatedAt: DateTimeOffset }

type ExerciseSessionEntry =
    { Id: int
      SessionId: int
      EntryId: int
      DisplayOrder: int
      PromptData: string
      PromptSchemaVersion: int16 }

type ExerciseAttempt =
    { Id: int
      UserId: int
      SessionId: int option
      EntryId: int
      ExerciseType: int16
      PromptData: string
      PromptSchemaVersion: int16
      RawAnswer: string
      IsCorrect: bool
      AttemptedAt: DateTimeOffset }

[<RequireQualifiedAccess>]
module Entities =
    let makeUser id : Mapping.User =
        { Id = id; Collections = ResizeArray() }

    let makeCollection
        (user: Mapping.User)
        (name: string)
        (description: string option)
        (createdAt: DateTimeOffset)
        (updatedAt: DateTimeOffset)
        (isSystem: bool)
        : Mapping.Collection =
        let collection: Mapping.Collection =
            { Id = 0
              UserId = user.Id
              Name = name
              Description = description |> Option.toObj
              CreatedAt = createdAt
              UpdatedAt = updatedAt
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
        (updatedAt: DateTimeOffset)
        (isDefault: bool)
        : Mapping.Vocabulary =
        let vocabulary: Mapping.Vocabulary =
            { Id = 0
              CollectionId = collection.Id
              Name = name
              Description = description |> Option.toObj
              CreatedAt = createdAt
              UpdatedAt = updatedAt
              IsDefault = isDefault
              Collection = collection
              Entries = ResizeArray() }

        collection.Vocabularies.Add(vocabulary)
        vocabulary

    let makeEntry
        (vocabulary: Mapping.Vocabulary)
        (entryText: string)
        (createdAt: DateTimeOffset)
        (updatedAt: DateTimeOffset)
        : Mapping.Entry =
        let entry: Mapping.Entry =
            { Id = 0
              VocabularyId = vocabulary.Id
              EntryText = entryText
              CreatedAt = createdAt
              UpdatedAt = updatedAt
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

    let makeExerciseSession
        (user: Mapping.User)
        (exerciseType: int16)
        (createdAt: DateTimeOffset)
        : Mapping.ExerciseSession =
        let session: Mapping.ExerciseSession =
            { Id = 0
              UserId = user.Id
              ExerciseType = exerciseType
              CreatedAt = createdAt
              User = user
              Entries = ResizeArray() }

        session

    let makeExerciseSessionEntry
        (session: Mapping.ExerciseSession)
        (entry: Mapping.Entry)
        (displayOrder: int)
        (promptData: string)
        (promptSchemaVersion: int16)
        : Mapping.ExerciseSessionEntry =
        let sessionEntry: Mapping.ExerciseSessionEntry =
            { Id = 0
              SessionId = session.Id
              EntryId = entry.Id
              DisplayOrder = displayOrder
              PromptData = promptData
              PromptSchemaVersion = promptSchemaVersion
              Session = session
              Entry = entry }

        session.Entries.Add(sessionEntry)
        sessionEntry

    let makeExerciseAttempt
        (user: Mapping.User)
        (session: Mapping.ExerciseSession option)
        (entry: Mapping.Entry)
        (exerciseType: int16)
        (promptData: string)
        (promptSchemaVersion: int16)
        (rawAnswer: string)
        (isCorrect: bool)
        (attemptedAt: DateTimeOffset)
        : Mapping.ExerciseAttempt =
        { Id = 0
          UserId = user.Id
          SessionId =
            session
            |> Option.map(fun s -> s.Id)
            |> Option.toNullable
          EntryId = entry.Id
          ExerciseType = exerciseType
          PromptData = promptData
          PromptSchemaVersion = promptSchemaVersion
          RawAnswer = rawAnswer
          IsCorrect = isCorrect
          AttemptedAt = attemptedAt
          User = Unchecked.defaultof<Mapping.User>
          Entry = Unchecked.defaultof<Mapping.Entry> }

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
          UpdatedAt = entity.UpdatedAt
          IsSystem = entity.IsSystem }

    let private toVocabulary(entity: Mapping.Vocabulary) : Vocabulary =
        { Id = entity.Id
          CollectionId = entity.CollectionId
          Name = entity.Name
          Description = Option.ofObj entity.Description
          CreatedAt = entity.CreatedAt
          UpdatedAt = entity.UpdatedAt
          IsDefault = entity.IsDefault }

    let private toEntry(entity: Mapping.Entry) : Entry =
        { Id = entity.Id
          VocabularyId = entity.VocabularyId
          EntryText = entity.EntryText
          CreatedAt = entity.CreatedAt
          UpdatedAt = entity.UpdatedAt }

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

    let private toExerciseSession(entity: Mapping.ExerciseSession) : ExerciseSession =
        { Id = entity.Id
          UserId = entity.UserId
          ExerciseType = entity.ExerciseType
          CreatedAt = entity.CreatedAt }

    let private toExerciseSessionEntry(entity: Mapping.ExerciseSessionEntry) : ExerciseSessionEntry =
        { Id = entity.Id
          SessionId = entity.SessionId
          EntryId = entity.EntryId
          DisplayOrder = entity.DisplayOrder
          PromptData = entity.PromptData
          PromptSchemaVersion = entity.PromptSchemaVersion }

    let private toExerciseAttempt(entity: Mapping.ExerciseAttempt) : ExerciseAttempt =
        { Id = entity.Id
          UserId = entity.UserId
          SessionId = Option.ofNullable entity.SessionId
          EntryId = entity.EntryId
          ExerciseType = entity.ExerciseType
          PromptData = entity.PromptData
          PromptSchemaVersion = entity.PromptSchemaVersion
          RawAnswer = entity.RawAnswer
          IsCorrect = entity.IsCorrect
          AttemptedAt = entity.AttemptedAt }

    let addExerciseSessions (sessions: Mapping.ExerciseSession list) (seeder: WordfolioSeeder) : WordfolioSeeder =
        seeder.Context.ExerciseSessions.AddRange(sessions)
        seeder

    let addExerciseSessionEntries
        (entries: Mapping.ExerciseSessionEntry list)
        (seeder: WordfolioSeeder)
        : WordfolioSeeder =
        seeder.Context.ExerciseSessionEntries.AddRange(entries)
        seeder

    let addExerciseAttempts (attempts: Mapping.ExerciseAttempt list) (seeder: WordfolioSeeder) : WordfolioSeeder =
        seeder.Context.ExerciseAttempts.AddRange(attempts)
        seeder

    let getAllExerciseSessionsAsync(seeder: WordfolioSeeder) : Task<ExerciseSession list> =
        task {
            let! sessions = seeder.Context.ExerciseSessions.AsNoTracking().ToArrayAsync()

            return
                sessions
                |> Seq.map toExerciseSession
                |> Seq.toList
        }

    let getExerciseSessionByIdAsync (id: int) (seeder: WordfolioSeeder) : Task<ExerciseSession option> =
        task {
            let! session =
                seeder.Context.ExerciseSessions
                    .AsNoTracking()
                    .FirstOrDefaultAsync(fun s -> s.Id = id)

            return
                session
                |> Option.ofObj
                |> Option.map toExerciseSession
        }

    let getAllExerciseSessionEntriesAsync(seeder: WordfolioSeeder) : Task<ExerciseSessionEntry list> =
        task {
            let! entries = seeder.Context.ExerciseSessionEntries.AsNoTracking().ToArrayAsync()

            return
                entries
                |> Seq.map toExerciseSessionEntry
                |> Seq.toList
        }

    let getExerciseSessionEntriesBySessionIdAsync
        (sessionId: int)
        (seeder: WordfolioSeeder)
        : Task<ExerciseSessionEntry list> =
        task {
            let! entries =
                seeder.Context.ExerciseSessionEntries
                    .AsNoTracking()
                    .Where(fun e -> e.SessionId = sessionId)
                    .ToArrayAsync()

            return
                entries
                |> Seq.map toExerciseSessionEntry
                |> Seq.toList
        }

    let getExerciseSessionEntryAsync
        (sessionId: int)
        (entryId: int)
        (seeder: WordfolioSeeder)
        : Task<ExerciseSessionEntry option> =
        task {
            let! entry =
                seeder.Context.ExerciseSessionEntries
                    .AsNoTracking()
                    .FirstOrDefaultAsync(fun e ->
                        e.SessionId = sessionId
                        && e.EntryId = entryId)

            return
                entry
                |> Option.ofObj
                |> Option.map toExerciseSessionEntry
        }

    let getAllExerciseAttemptsAsync(seeder: WordfolioSeeder) : Task<ExerciseAttempt list> =
        task {
            let! attempts = seeder.Context.ExerciseAttempts.AsNoTracking().ToArrayAsync()

            return
                attempts
                |> Seq.map toExerciseAttempt
                |> Seq.toList
        }

    let getExerciseAttemptsBySessionIdAsync (sessionId: int) (seeder: WordfolioSeeder) : Task<ExerciseAttempt list> =
        task {
            let! attempts =
                seeder.Context.ExerciseAttempts
                    .AsNoTracking()
                    .Where(fun a -> a.SessionId = Nullable(sessionId))
                    .ToArrayAsync()

            return
                attempts
                |> Seq.map toExerciseAttempt
                |> Seq.toList
        }

    let getExerciseAttemptByIdAsync (id: int) (seeder: WordfolioSeeder) : Task<ExerciseAttempt option> =
        task {
            let! attempt =
                seeder.Context.ExerciseAttempts
                    .AsNoTracking()
                    .FirstOrDefaultAsync(fun a -> a.Id = id)

            return
                attempt
                |> Option.ofObj
                |> Option.map toExerciseAttempt
        }
