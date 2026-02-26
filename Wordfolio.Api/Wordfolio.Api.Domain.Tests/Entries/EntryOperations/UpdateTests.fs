module Wordfolio.Api.Domain.Tests.Entries.EntryOperations.UpdateTests

open System
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Entries
open Wordfolio.Api.Domain.Entries.EntryOperations

open Wordfolio.Api.Domain.Entries.Helpers

type TestEnv
    (
        getEntryById: EntryId -> Task<Entry option>,
        hasVocabularyAccessInCollection: HasVocabularyAccessInCollectionData -> Task<bool>,
        updateEntry: UpdateEntryData -> Task<unit>,
        clearEntryChildren: EntryId -> Task<unit>,
        createDefinition: CreateDefinitionData -> Task<DefinitionId>,
        createTranslation: CreateTranslationData -> Task<TranslationId>,
        createExamplesForDefinition: CreateExamplesForDefinitionData -> Task<unit>,
        createExamplesForTranslation: CreateExamplesForTranslationData -> Task<unit>
    ) =
    let getEntryByIdCalls =
        ResizeArray<EntryId>()

    let hasVocabularyAccessInCollectionCalls =
        ResizeArray<HasVocabularyAccessInCollectionData>()

    let updateEntryCalls =
        ResizeArray<UpdateEntryData>()

    let clearEntryChildrenCalls =
        ResizeArray<EntryId>()

    let createDefinitionCalls =
        ResizeArray<CreateDefinitionData>()

    let createTranslationCalls =
        ResizeArray<CreateTranslationData>()

    let createExamplesForDefinitionCalls =
        ResizeArray<CreateExamplesForDefinitionData>()

    let createExamplesForTranslationCalls =
        ResizeArray<CreateExamplesForTranslationData>()

    member _.GetEntryByIdCalls =
        getEntryByIdCalls |> Seq.toList

    member _.HasVocabularyAccessInCollectionCalls =
        hasVocabularyAccessInCollectionCalls
        |> Seq.toList

    member _.UpdateEntryCalls =
        updateEntryCalls |> Seq.toList

    member _.ClearEntryChildrenCalls =
        clearEntryChildrenCalls |> Seq.toList

    member _.CreateDefinitionCalls =
        createDefinitionCalls |> Seq.toList

    member _.CreateTranslationCalls =
        createTranslationCalls |> Seq.toList

    member _.CreateExamplesForDefinitionCalls =
        createExamplesForDefinitionCalls
        |> Seq.toList

    member _.CreateExamplesForTranslationCalls =
        createExamplesForTranslationCalls
        |> Seq.toList

    interface IGetEntryById with
        member _.GetEntryById(id) =
            getEntryByIdCalls.Add(id)
            getEntryById id

    interface IHasVocabularyAccessInCollection with
        member _.HasVocabularyAccessInCollection(data) =
            hasVocabularyAccessInCollectionCalls.Add(data)
            hasVocabularyAccessInCollection data

    interface IUpdateEntry with
        member _.UpdateEntry(data) =
            updateEntryCalls.Add(data)
            updateEntry data

    interface IClearEntryChildren with
        member _.ClearEntryChildren(entryId) =
            clearEntryChildrenCalls.Add(entryId)
            clearEntryChildren entryId

    interface ICreateDefinition with
        member _.CreateDefinition(data) =
            createDefinitionCalls.Add(data)
            createDefinition data

    interface ICreateTranslation with
        member _.CreateTranslation(data) =
            createTranslationCalls.Add(data)
            createTranslation data

    interface ICreateExamplesForDefinition with
        member _.CreateExamplesForDefinition(data) =
            createExamplesForDefinitionCalls.Add(data)
            createExamplesForDefinition(data)

    interface ICreateExamplesForTranslation with
        member _.CreateExamplesForTranslation(data) =
            createExamplesForTranslationCalls.Add(data)
            createExamplesForTranslation(data)

    interface ITransactional<TestEnv> with
        member this.RunInTransaction(operation) = operation this

let makeEntry id vocabularyId text definitions translations createdAt updatedAt =
    { Id = EntryId id
      VocabularyId = VocabularyId vocabularyId
      EntryText = text
      CreatedAt = createdAt
      UpdatedAt = updatedAt
      Definitions = definitions
      Translations = translations }

let makeDefinition id text source displayOrder examples =
    { Id = DefinitionId id
      DefinitionText = text
      Source = source
      DisplayOrder = displayOrder
      Examples = examples }

let makeTranslation id text source displayOrder examples =
    { Id = TranslationId id
      TranslationText = text
      Source = source
      DisplayOrder = displayOrder
      Examples = examples }

let makeExample id text source =
    { Id = ExampleId id
      ExampleText = text
      Source = source }

let makeDefinitionInput text source examples =
    { DefinitionText = text
      Source = source
      Examples = examples }

let makeTranslationInput text source examples =
    { TranslationText = text
      Source = source
      Examples = examples }

let makeExampleInput text source = { ExampleText = text; Source = source }

[<Fact>]
let ``updates entry with new definitions and translations``() =
    task {
        let now = DateTimeOffset.UtcNow

        let existingEntry =
            makeEntry 1 10 "old" [] [] now None

        let example =
            makeExample 1 "example" ExampleSource.Custom

        let definition =
            makeDefinition 10 "definition" DefinitionSource.Manual 0 [ example ]

        let translation =
            makeTranslation 20 "translation" TranslationSource.Manual 0 [ example ]

        let updatedEntry =
            makeEntry 1 10 "new" [ definition ] [ translation ] now (Some now)

        let getEntryByIdCallCount = ref 0

        let env =
            TestEnv(
                getEntryById =
                    (fun _ ->
                        let callIndex = getEntryByIdCallCount.Value
                        getEntryByIdCallCount.Value <- callIndex + 1

                        if callIndex = 0 then
                            Task.FromResult(Some existingEntry)
                        elif callIndex = 1 then
                            Task.FromResult(Some updatedEntry)
                        else
                            failwith "Unexpected getEntryById call"),
                hasVocabularyAccessInCollection = (fun _ -> Task.FromResult(true)),
                updateEntry =
                    (fun data ->
                        let text = data.EntryText
                        let updatedAt = data.UpdatedAt

                        if text <> "new" then
                            failwith $"Expected trimmed text 'new', got: '{text}'"

                        if updatedAt <> now then
                            failwith $"Unexpected updatedAt: {updatedAt}"

                        Task.FromResult(())),
                clearEntryChildren = (fun _ -> Task.FromResult(())),
                createDefinition = (fun _ -> Task.FromResult(DefinitionId 10)),
                createTranslation = (fun _ -> Task.FromResult(TranslationId 20)),
                createExamplesForDefinition = (fun _ -> Task.FromResult(())),
                createExamplesForTranslation = (fun _ -> Task.FromResult(()))
            )

        let definitions =
            [ makeDefinitionInput
                  "definition"
                  DefinitionSource.Manual
                  [ makeExampleInput "example" ExampleSource.Custom ] ]

        let translations =
            [ makeTranslationInput
                  "translation"
                  TranslationSource.Manual
                  [ makeExampleInput "example" ExampleSource.Custom ] ]

        let! result =
            update
                env
                { UserId = UserId 3
                  CollectionId = CollectionId 5
                  VocabularyId = VocabularyId 10
                  EntryId = EntryId 1
                  EntryText = "  new  "
                  Definitions = definitions
                  Translations = translations
                  UpdatedAt = now }

        Assert.Equal(Ok updatedEntry, result)
        Assert.Equal<EntryId list>([ EntryId 1; EntryId 1 ], env.GetEntryByIdCalls)

        Assert.Equal<HasVocabularyAccessInCollectionData list>(
            [ ({ VocabularyId = VocabularyId 10
                 CollectionId = CollectionId 5
                 UserId = UserId 3 }
              : HasVocabularyAccessInCollectionData) ],
            env.HasVocabularyAccessInCollectionCalls
        )

        Assert.Equal<EntryId list>([ EntryId 1 ], env.ClearEntryChildrenCalls)

        Assert.Equal<UpdateEntryData list>(
            [ { EntryId = EntryId 1
                EntryText = "new"
                UpdatedAt = now } ],
            env.UpdateEntryCalls
        )

        Assert.Equal<CreateDefinitionData list>(
            [ ({ EntryId = EntryId 1
                 Text = "definition"
                 Source = DefinitionSource.Manual
                 DisplayOrder = 0 }
              : CreateDefinitionData) ],
            env.CreateDefinitionCalls
        )

        Assert.Equal<CreateTranslationData list>(
            [ ({ EntryId = EntryId 1
                 Text = "translation"
                 Source = TranslationSource.Manual
                 DisplayOrder = 0 }
              : CreateTranslationData) ],
            env.CreateTranslationCalls
        )

        Assert.Equal<CreateExamplesForDefinitionData list>(
            [ ({ DefinitionId = DefinitionId 10
                 Examples = [ makeExampleInput "example" ExampleSource.Custom ] }
              : CreateExamplesForDefinitionData) ],
            env.CreateExamplesForDefinitionCalls
        )

        Assert.Equal<CreateExamplesForTranslationData list>(
            [ ({ TranslationId = TranslationId 20
                 Examples = [ makeExampleInput "example" ExampleSource.Custom ] }
              : CreateExamplesForTranslationData) ],
            env.CreateExamplesForTranslationCalls
        )
    }

[<Fact>]
let ``returns VocabularyNotFoundOrAccessDenied when vocabulary is not in collection``() =
    task {
        let env =
            TestEnv(
                getEntryById = (fun _ -> failwith "Should not be called"),
                hasVocabularyAccessInCollection = (fun _ -> Task.FromResult(false)),
                updateEntry = (fun _ -> failwith "Should not be called"),
                clearEntryChildren = (fun _ -> failwith "Should not be called"),
                createDefinition = (fun _ -> failwith "Should not be called"),
                createTranslation = (fun _ -> failwith "Should not be called"),
                createExamplesForDefinition = (fun _ -> failwith "Should not be called"),
                createExamplesForTranslation = (fun _ -> failwith "Should not be called")
            )

        let definitions =
            [ makeDefinitionInput "definition" DefinitionSource.Manual [] ]

        let! result =
            update
                env
                { UserId = UserId 1
                  CollectionId = CollectionId 5
                  VocabularyId = VocabularyId 10
                  EntryId = EntryId 1
                  EntryText = "text"
                  Definitions = definitions
                  Translations = []
                  UpdatedAt = DateTimeOffset.UtcNow }

        Assert.Equal(Error(UpdateEntryError.VocabularyNotFoundOrAccessDenied(VocabularyId 10)), result)
        Assert.Empty(env.GetEntryByIdCalls)
        Assert.Empty(env.UpdateEntryCalls)

        Assert.Equal<HasVocabularyAccessInCollectionData list>(
            [ ({ VocabularyId = VocabularyId 10
                 CollectionId = CollectionId 5
                 UserId = UserId 1 }
              : HasVocabularyAccessInCollectionData) ],
            env.HasVocabularyAccessInCollectionCalls
        )
    }

[<Fact>]
let ``returns EntryNotFound when entry does not exist``() =
    task {
        let env =
            TestEnv(
                getEntryById = (fun _ -> Task.FromResult(None)),
                hasVocabularyAccessInCollection = (fun _ -> Task.FromResult(true)),
                updateEntry = (fun _ -> failwith "Should not be called"),
                clearEntryChildren = (fun _ -> failwith "Should not be called"),
                createDefinition = (fun _ -> failwith "Should not be called"),
                createTranslation = (fun _ -> failwith "Should not be called"),
                createExamplesForDefinition = (fun _ -> failwith "Should not be called"),
                createExamplesForTranslation = (fun _ -> failwith "Should not be called")
            )

        let definitions =
            [ makeDefinitionInput "definition" DefinitionSource.Manual [] ]

        let! result =
            update
                env
                { UserId = UserId 1
                  CollectionId = CollectionId 5
                  VocabularyId = VocabularyId 10
                  EntryId = EntryId 99
                  EntryText = "text"
                  Definitions = definitions
                  Translations = []
                  UpdatedAt = DateTimeOffset.UtcNow }

        Assert.Equal(Error(UpdateEntryError.EntryNotFound(EntryId 99)), result)
        Assert.Equal<EntryId list>([ EntryId 99 ], env.GetEntryByIdCalls)
        Assert.Empty(env.UpdateEntryCalls)
    }

[<Fact>]
let ``returns EntryNotFound when entry belongs to different vocabulary``() =
    task {
        let entry =
            makeEntry 1 99 "text" [] [] DateTimeOffset.UtcNow None

        let env =
            TestEnv(
                getEntryById = (fun _ -> Task.FromResult(Some entry)),
                hasVocabularyAccessInCollection = (fun _ -> Task.FromResult(true)),
                updateEntry = (fun _ -> failwith "Should not be called"),
                clearEntryChildren = (fun _ -> failwith "Should not be called"),
                createDefinition = (fun _ -> failwith "Should not be called"),
                createTranslation = (fun _ -> failwith "Should not be called"),
                createExamplesForDefinition = (fun _ -> failwith "Should not be called"),
                createExamplesForTranslation = (fun _ -> failwith "Should not be called")
            )

        let definitions =
            [ makeDefinitionInput "definition" DefinitionSource.Manual [] ]

        let! result =
            update
                env
                { UserId = UserId 1
                  CollectionId = CollectionId 5
                  VocabularyId = VocabularyId 10
                  EntryId = EntryId 1
                  EntryText = "text"
                  Definitions = definitions
                  Translations = []
                  UpdatedAt = DateTimeOffset.UtcNow }

        Assert.Equal(Error(UpdateEntryError.EntryNotFound(EntryId 1)), result)
        Assert.Equal<EntryId list>([ EntryId 1 ], env.GetEntryByIdCalls)
        Assert.Empty(env.UpdateEntryCalls)
    }

[<Fact>]
let ``returns error when no definitions or translations``() =
    task {
        let env =
            TestEnv(
                getEntryById = (fun _ -> failwith "Should not be called"),
                hasVocabularyAccessInCollection = (fun _ -> failwith "Should not be called"),
                updateEntry = (fun _ -> failwith "Should not be called"),
                clearEntryChildren = (fun _ -> failwith "Should not be called"),
                createDefinition = (fun _ -> failwith "Should not be called"),
                createTranslation = (fun _ -> failwith "Should not be called"),
                createExamplesForDefinition = (fun _ -> failwith "Should not be called"),
                createExamplesForTranslation = (fun _ -> failwith "Should not be called")
            )

        let! result =
            update
                env
                { UserId = UserId 1
                  CollectionId = CollectionId 5
                  VocabularyId = VocabularyId 10
                  EntryId = EntryId 1
                  EntryText = "text"
                  Definitions = []
                  Translations = []
                  UpdatedAt = DateTimeOffset.UtcNow }

        Assert.Equal(Error UpdateEntryError.NoDefinitionsOrTranslations, result)
        Assert.Empty(env.UpdateEntryCalls)
    }

[<Fact>]
let ``returns error when entry text is empty``() =
    task {
        let env =
            TestEnv(
                getEntryById = (fun _ -> failwith "Should not be called"),
                hasVocabularyAccessInCollection = (fun _ -> failwith "Should not be called"),
                updateEntry = (fun _ -> failwith "Should not be called"),
                clearEntryChildren = (fun _ -> failwith "Should not be called"),
                createDefinition = (fun _ -> failwith "Should not be called"),
                createTranslation = (fun _ -> failwith "Should not be called"),
                createExamplesForDefinition = (fun _ -> failwith "Should not be called"),
                createExamplesForTranslation = (fun _ -> failwith "Should not be called")
            )

        let definitions =
            [ makeDefinitionInput "definition" DefinitionSource.Manual [] ]

        let! result =
            update
                env
                { UserId = UserId 1
                  CollectionId = CollectionId 5
                  VocabularyId = VocabularyId 10
                  EntryId = EntryId 1
                  EntryText = ""
                  Definitions = definitions
                  Translations = []
                  UpdatedAt = DateTimeOffset.UtcNow }

        Assert.Equal(Error UpdateEntryError.EntryTextRequired, result)
        Assert.Empty(env.GetEntryByIdCalls)
    }

[<Fact>]
let ``returns error when entry text exceeds max length``() =
    task {
        let longText =
            String.replicate (MaxEntryTextLength + 1) "a"

        let env =
            TestEnv(
                getEntryById = (fun _ -> failwith "Should not be called"),
                hasVocabularyAccessInCollection = (fun _ -> failwith "Should not be called"),
                updateEntry = (fun _ -> failwith "Should not be called"),
                clearEntryChildren = (fun _ -> failwith "Should not be called"),
                createDefinition = (fun _ -> failwith "Should not be called"),
                createTranslation = (fun _ -> failwith "Should not be called"),
                createExamplesForDefinition = (fun _ -> failwith "Should not be called"),
                createExamplesForTranslation = (fun _ -> failwith "Should not be called")
            )

        let definitions =
            [ makeDefinitionInput "definition" DefinitionSource.Manual [] ]

        let! result =
            update
                env
                { UserId = UserId 1
                  CollectionId = CollectionId 5
                  VocabularyId = VocabularyId 10
                  EntryId = EntryId 1
                  EntryText = longText
                  Definitions = definitions
                  Translations = []
                  UpdatedAt = DateTimeOffset.UtcNow }

        Assert.Equal(Error(UpdateEntryError.EntryTextTooLong MaxEntryTextLength), result)
        Assert.Empty(env.GetEntryByIdCalls)
    }

[<Fact>]
let ``returns error when entry text is whitespace only``() =
    task {
        let env =
            TestEnv(
                getEntryById = (fun _ -> failwith "Should not be called"),
                hasVocabularyAccessInCollection = (fun _ -> failwith "Should not be called"),
                updateEntry = (fun _ -> failwith "Should not be called"),
                clearEntryChildren = (fun _ -> failwith "Should not be called"),
                createDefinition = (fun _ -> failwith "Should not be called"),
                createTranslation = (fun _ -> failwith "Should not be called"),
                createExamplesForDefinition = (fun _ -> failwith "Should not be called"),
                createExamplesForTranslation = (fun _ -> failwith "Should not be called")
            )

        let definitions =
            [ makeDefinitionInput "definition" DefinitionSource.Manual [] ]

        let! result =
            update
                env
                { UserId = UserId 1
                  CollectionId = CollectionId 5
                  VocabularyId = VocabularyId 10
                  EntryId = EntryId 1
                  EntryText = "   "
                  Definitions = definitions
                  Translations = []
                  UpdatedAt = DateTimeOffset.UtcNow }

        Assert.Equal(Error UpdateEntryError.EntryTextRequired, result)
        Assert.Empty(env.GetEntryByIdCalls)
    }

[<Fact>]
let ``returns error when definition example text is too long``() =
    task {
        let longExample =
            String.replicate (MaxExampleTextLength + 1) "a"

        let env =
            TestEnv(
                getEntryById = (fun _ -> failwith "Should not be called"),
                hasVocabularyAccessInCollection = (fun _ -> failwith "Should not be called"),
                updateEntry = (fun _ -> failwith "Should not be called"),
                clearEntryChildren = (fun _ -> failwith "Should not be called"),
                createDefinition = (fun _ -> failwith "Should not be called"),
                createTranslation = (fun _ -> failwith "Should not be called"),
                createExamplesForDefinition = (fun _ -> failwith "Should not be called"),
                createExamplesForTranslation = (fun _ -> failwith "Should not be called")
            )

        let definitions =
            [ makeDefinitionInput
                  "definition"
                  DefinitionSource.Manual
                  [ makeExampleInput longExample ExampleSource.Custom ] ]

        let! result =
            update
                env
                { UserId = UserId 2
                  CollectionId = CollectionId 5
                  VocabularyId = VocabularyId 10
                  EntryId = EntryId 1
                  EntryText = "text"
                  Definitions = definitions
                  Translations = []
                  UpdatedAt = DateTimeOffset.UtcNow }

        Assert.Equal(Error(UpdateEntryError.ExampleTextTooLong MaxExampleTextLength), result)
        Assert.Empty(env.UpdateEntryCalls)
    }

[<Fact>]
let ``returns error when definition has too many examples``() =
    task {
        let examples =
            [ 1 .. MaxExamplesPerItem + 1 ]
            |> List.map(fun i -> makeExampleInput $"example {i}" ExampleSource.Custom)

        let env =
            TestEnv(
                getEntryById = (fun _ -> failwith "Should not be called"),
                hasVocabularyAccessInCollection = (fun _ -> failwith "Should not be called"),
                updateEntry = (fun _ -> failwith "Should not be called"),
                clearEntryChildren = (fun _ -> failwith "Should not be called"),
                createDefinition = (fun _ -> failwith "Should not be called"),
                createTranslation = (fun _ -> failwith "Should not be called"),
                createExamplesForDefinition = (fun _ -> failwith "Should not be called"),
                createExamplesForTranslation = (fun _ -> failwith "Should not be called")
            )

        let definitions =
            [ makeDefinitionInput "definition" DefinitionSource.Manual examples ]

        let! result =
            update
                env
                { UserId = UserId 2
                  CollectionId = CollectionId 5
                  VocabularyId = VocabularyId 10
                  EntryId = EntryId 1
                  EntryText = "text"
                  Definitions = definitions
                  Translations = []
                  UpdatedAt = DateTimeOffset.UtcNow }

        Assert.Equal(Error(UpdateEntryError.TooManyExamples MaxExamplesPerItem), result)
        Assert.Empty(env.UpdateEntryCalls)
    }

[<Fact>]
let ``returns error when translation example text is too long``() =
    task {
        let longExample =
            String.replicate (MaxExampleTextLength + 1) "a"

        let env =
            TestEnv(
                getEntryById = (fun _ -> failwith "Should not be called"),
                hasVocabularyAccessInCollection = (fun _ -> failwith "Should not be called"),
                updateEntry = (fun _ -> failwith "Should not be called"),
                clearEntryChildren = (fun _ -> failwith "Should not be called"),
                createDefinition = (fun _ -> failwith "Should not be called"),
                createTranslation = (fun _ -> failwith "Should not be called"),
                createExamplesForDefinition = (fun _ -> failwith "Should not be called"),
                createExamplesForTranslation = (fun _ -> failwith "Should not be called")
            )

        let translations =
            [ makeTranslationInput
                  "translation"
                  TranslationSource.Manual
                  [ makeExampleInput longExample ExampleSource.Custom ] ]

        let! result =
            update
                env
                { UserId = UserId 2
                  CollectionId = CollectionId 5
                  VocabularyId = VocabularyId 10
                  EntryId = EntryId 1
                  EntryText = "text"
                  Definitions = []
                  Translations = translations
                  UpdatedAt = DateTimeOffset.UtcNow }

        Assert.Equal(Error(UpdateEntryError.ExampleTextTooLong MaxExampleTextLength), result)
        Assert.Empty(env.UpdateEntryCalls)
    }

[<Fact>]
let ``returns error when translation has too many examples``() =
    task {
        let examples =
            [ 1 .. MaxExamplesPerItem + 1 ]
            |> List.map(fun i -> makeExampleInput $"example {i}" ExampleSource.Custom)

        let env =
            TestEnv(
                getEntryById = (fun _ -> failwith "Should not be called"),
                hasVocabularyAccessInCollection = (fun _ -> failwith "Should not be called"),
                updateEntry = (fun _ -> failwith "Should not be called"),
                clearEntryChildren = (fun _ -> failwith "Should not be called"),
                createDefinition = (fun _ -> failwith "Should not be called"),
                createTranslation = (fun _ -> failwith "Should not be called"),
                createExamplesForDefinition = (fun _ -> failwith "Should not be called"),
                createExamplesForTranslation = (fun _ -> failwith "Should not be called")
            )

        let translations =
            [ makeTranslationInput "translation" TranslationSource.Manual examples ]

        let! result =
            update
                env
                { UserId = UserId 2
                  CollectionId = CollectionId 5
                  VocabularyId = VocabularyId 10
                  EntryId = EntryId 1
                  EntryText = "text"
                  Definitions = []
                  Translations = translations
                  UpdatedAt = DateTimeOffset.UtcNow }

        Assert.Equal(Error(UpdateEntryError.TooManyExamples MaxExamplesPerItem), result)
        Assert.Empty(env.UpdateEntryCalls)
    }

[<Fact>]
let ``updates entry with definitions only``() =
    task {
        let now = DateTimeOffset.UtcNow

        let existingEntry =
            makeEntry 1 10 "word" [] [] now None

        let definition =
            makeDefinition 10 "definition" DefinitionSource.Manual 0 []

        let updatedEntry =
            makeEntry 1 10 "word" [ definition ] [] now (Some now)

        let getEntryByIdCallCount = ref 0

        let env =
            TestEnv(
                getEntryById =
                    (fun _ ->
                        let callIndex = getEntryByIdCallCount.Value
                        getEntryByIdCallCount.Value <- callIndex + 1

                        if callIndex = 0 then
                            Task.FromResult(Some existingEntry)
                        elif callIndex = 1 then
                            Task.FromResult(Some updatedEntry)
                        else
                            failwith "Unexpected getEntryById call"),
                hasVocabularyAccessInCollection = (fun _ -> Task.FromResult(true)),
                updateEntry = (fun _ -> Task.FromResult(())),
                clearEntryChildren = (fun _ -> Task.FromResult(())),
                createDefinition = (fun _ -> Task.FromResult(DefinitionId 10)),
                createTranslation = (fun _ -> failwith "Should not be called"),
                createExamplesForDefinition = (fun _ -> Task.FromResult(())),
                createExamplesForTranslation = (fun _ -> failwith "Should not be called")
            )

        let definitions =
            [ makeDefinitionInput "definition" DefinitionSource.Manual [] ]

        let! result =
            update
                env
                { UserId = UserId 3
                  CollectionId = CollectionId 5
                  VocabularyId = VocabularyId 10
                  EntryId = EntryId 1
                  EntryText = "word"
                  Definitions = definitions
                  Translations = []
                  UpdatedAt = now }

        Assert.Equal(Ok updatedEntry, result)

        Assert.Equal<CreateDefinitionData list>(
            [ ({ EntryId = EntryId 1
                 Text = "definition"
                 Source = DefinitionSource.Manual
                 DisplayOrder = 0 }
              : CreateDefinitionData) ],
            env.CreateDefinitionCalls
        )

        Assert.Empty(env.CreateTranslationCalls)
    }

[<Fact>]
let ``updates entry with translations only``() =
    task {
        let now = DateTimeOffset.UtcNow

        let existingEntry =
            makeEntry 1 10 "word" [] [] now None

        let translation =
            makeTranslation 20 "translation" TranslationSource.Manual 0 []

        let updatedEntry =
            makeEntry 1 10 "word" [] [ translation ] now (Some now)

        let getEntryByIdCallCount = ref 0

        let env =
            TestEnv(
                getEntryById =
                    (fun _ ->
                        let callIndex = getEntryByIdCallCount.Value
                        getEntryByIdCallCount.Value <- callIndex + 1

                        if callIndex = 0 then
                            Task.FromResult(Some existingEntry)
                        elif callIndex = 1 then
                            Task.FromResult(Some updatedEntry)
                        else
                            failwith "Unexpected getEntryById call"),
                hasVocabularyAccessInCollection = (fun _ -> Task.FromResult(true)),
                updateEntry = (fun _ -> Task.FromResult(())),
                clearEntryChildren = (fun _ -> Task.FromResult(())),
                createDefinition = (fun _ -> failwith "Should not be called"),
                createTranslation = (fun _ -> Task.FromResult(TranslationId 20)),
                createExamplesForDefinition = (fun _ -> failwith "Should not be called"),
                createExamplesForTranslation = (fun _ -> Task.FromResult(()))
            )

        let translations =
            [ makeTranslationInput "translation" TranslationSource.Manual [] ]

        let! result =
            update
                env
                { UserId = UserId 3
                  CollectionId = CollectionId 5
                  VocabularyId = VocabularyId 10
                  EntryId = EntryId 1
                  EntryText = "word"
                  Definitions = []
                  Translations = translations
                  UpdatedAt = now }

        Assert.Equal(Ok updatedEntry, result)

        Assert.Equal<CreateTranslationData list>(
            [ ({ EntryId = EntryId 1
                 Text = "translation"
                 Source = TranslationSource.Manual
                 DisplayOrder = 0 }
              : CreateTranslationData) ],
            env.CreateTranslationCalls
        )

        Assert.Empty(env.CreateDefinitionCalls)
    }
