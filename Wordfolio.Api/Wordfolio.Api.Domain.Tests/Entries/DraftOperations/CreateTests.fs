module Wordfolio.Api.Domain.Tests.Entries.DraftOperations.CreateTests

open System
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Entries
open Wordfolio.Api.Domain.Entries.DraftOperations
open Wordfolio.Api.Domain.Entries.Helpers

type TestEnv
    (
        getEntryById: EntryId -> Task<Entry option>,
        getEntryByTextAndVocabularyId: VocabularyId * string -> Task<Entry option>,
        createEntry: CreateEntryData -> Task<EntryId>,
        createDefinition: CreateDefinitionData -> Task<DefinitionId>,
        createTranslation: CreateTranslationData -> Task<TranslationId>,
        createExamplesForDefinition: DefinitionId * ExampleInput list -> Task<unit>,
        createExamplesForTranslation: TranslationId * ExampleInput list -> Task<unit>,
        getDefaultVocabulary: UserId -> Task<Vocabulary option>,
        getDefaultCollection: UserId -> Task<Collection option>,
        createDefaultVocabulary: CreateDefaultVocabularyParameters -> Task<VocabularyId>,
        createDefaultCollection: CreateDefaultCollectionParameters -> Task<CollectionId>
    ) =
    let getEntryByIdCalls =
        ResizeArray<EntryId>()

    let getEntryByTextAndVocabularyIdCalls =
        ResizeArray<VocabularyId * string>()

    let createEntryCalls =
        ResizeArray<CreateEntryData>()

    let createDefinitionCalls =
        ResizeArray<CreateDefinitionData>()

    let createTranslationCalls =
        ResizeArray<CreateTranslationData>()

    let createExamplesForDefinitionCalls =
        ResizeArray<DefinitionId * ExampleInput list>()

    let createExamplesForTranslationCalls =
        ResizeArray<TranslationId * ExampleInput list>()

    member _.GetEntryByIdCalls =
        getEntryByIdCalls |> Seq.toList

    member _.GetEntryByTextAndVocabularyIdCalls =
        getEntryByTextAndVocabularyIdCalls
        |> Seq.toList

    member _.CreateEntryCalls =
        createEntryCalls |> Seq.toList

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

    interface IGetEntryByTextAndVocabularyId with
        member _.GetEntryByTextAndVocabularyId(vocabularyId, text) =
            getEntryByTextAndVocabularyIdCalls.Add(vocabularyId, text)
            getEntryByTextAndVocabularyId(vocabularyId, text)

    interface ICreateEntry with
        member _.CreateEntry(data) =
            createEntryCalls.Add(data)
            createEntry data

    interface ICreateDefinition with
        member _.CreateDefinition(data) =
            createDefinitionCalls.Add(data)
            createDefinition data

    interface ICreateTranslation with
        member _.CreateTranslation(data) =
            createTranslationCalls.Add(data)
            createTranslation data

    interface ICreateExamplesForDefinition with
        member _.CreateExamplesForDefinition(definitionId, examples) =
            createExamplesForDefinitionCalls.Add(definitionId, examples)
            createExamplesForDefinition(definitionId, examples)

    interface ICreateExamplesForTranslation with
        member _.CreateExamplesForTranslation(translationId, examples) =
            createExamplesForTranslationCalls.Add(translationId, examples)
            createExamplesForTranslation(translationId, examples)

    interface ITransactional<TestEnv> with
        member this.RunInTransaction(operation) = operation this

    interface IGetDefaultVocabulary with
        member _.GetDefaultVocabulary(userId) = getDefaultVocabulary userId

    interface ICreateDefaultVocabulary with
        member _.CreateDefaultVocabulary(parameters) = createDefaultVocabulary parameters

    interface IGetDefaultCollection with
        member _.GetDefaultCollection(userId) = getDefaultCollection userId

    interface ICreateDefaultCollection with
        member _.CreateDefaultCollection(parameters) = createDefaultCollection parameters

let makeVocabulary id collectionId name =
    { Id = VocabularyId id
      CollectionId = CollectionId collectionId
      Name = name
      Description = None
      CreatedAt = DateTimeOffset.UtcNow
      UpdatedAt = None }

let makeEntry id vocabularyId text createdAt definitions translations =
    { Id = EntryId id
      VocabularyId = VocabularyId vocabularyId
      EntryText = text
      CreatedAt = createdAt
      UpdatedAt = None
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

let makeCreateParams userId entryText definitions translations createdAt : CreateParameters =
    { UserId = userId
      EntryText = entryText
      Definitions = definitions
      Translations = translations
      AllowDuplicate = false
      CreatedAt = createdAt }

let defaultVocabulary =
    makeVocabulary 1 1 "Default"

[<Fact>]
let ``creates entry with definitions only``() =
    task {
        let now = DateTimeOffset.UtcNow

        let definitionInputs =
            [ makeDefinitionInput "A test definition" DefinitionSource.Manual [] ]

        let expectedDefinition =
            makeDefinition 1 "A test definition" DefinitionSource.Manual 0 []

        let createdEntry =
            makeEntry 1 1 "test word" now [ expectedDefinition ] []

        let env =
            TestEnv(
                getEntryById = (fun _ -> Task.FromResult(Some createdEntry)),
                getEntryByTextAndVocabularyId = (fun _ -> Task.FromResult(None)),
                createEntry = (fun _ -> Task.FromResult(EntryId 1)),
                createDefinition = (fun _ -> Task.FromResult(DefinitionId 1)),
                createTranslation = (fun _ -> failwith "Should not be called"),
                createExamplesForDefinition = (fun _ -> Task.FromResult(())),
                createExamplesForTranslation = (fun _ -> failwith "Should not be called"),
                getDefaultVocabulary = (fun _ -> Task.FromResult(Some defaultVocabulary)),
                getDefaultCollection = (fun _ -> failwith "Should not be called"),
                createDefaultVocabulary = (fun _ -> failwith "Should not be called"),
                createDefaultCollection = (fun _ -> failwith "Should not be called")
            )

        let! result =
            Wordfolio.Api.Domain.Entries.DraftOperations.create
                env
                (makeCreateParams (UserId 1) "test word" definitionInputs [] now)

        Assert.Equal(Ok createdEntry, result)

        Assert.Single(env.CreateEntryCalls)
        |> ignore

        Assert.Single(env.CreateDefinitionCalls)
        |> ignore

        Assert.Empty(env.CreateTranslationCalls)
    }

[<Fact>]
let ``creates entry with translations only``() =
    task {
        let now = DateTimeOffset.UtcNow

        let translationInputs =
            [ makeTranslationInput "test translation" TranslationSource.Manual [] ]

        let expectedTranslation =
            makeTranslation 1 "test translation" TranslationSource.Manual 0 []

        let createdEntry =
            makeEntry 1 1 "test word" now [] [ expectedTranslation ]

        let env =
            TestEnv(
                getEntryById = (fun _ -> Task.FromResult(Some createdEntry)),
                getEntryByTextAndVocabularyId = (fun _ -> Task.FromResult(None)),
                createEntry = (fun _ -> Task.FromResult(EntryId 1)),
                createDefinition = (fun _ -> failwith "Should not be called"),
                createTranslation = (fun _ -> Task.FromResult(TranslationId 1)),
                createExamplesForDefinition = (fun _ -> failwith "Should not be called"),
                createExamplesForTranslation = (fun _ -> Task.FromResult(())),
                getDefaultVocabulary = (fun _ -> Task.FromResult(Some defaultVocabulary)),
                getDefaultCollection = (fun _ -> failwith "Should not be called"),
                createDefaultVocabulary = (fun _ -> failwith "Should not be called"),
                createDefaultCollection = (fun _ -> failwith "Should not be called")
            )

        let! result =
            Wordfolio.Api.Domain.Entries.DraftOperations.create
                env
                (makeCreateParams (UserId 1) "test word" [] translationInputs now)

        Assert.Equal(Ok createdEntry, result)

        Assert.Single(env.CreateEntryCalls)
        |> ignore

        Assert.Empty(env.CreateDefinitionCalls)

        Assert.Single(env.CreateTranslationCalls)
        |> ignore
    }

[<Fact>]
let ``creates entry with both definitions and translations``() =
    task {
        let now = DateTimeOffset.UtcNow

        let definitionInputs =
            [ makeDefinitionInput "A test definition" DefinitionSource.Manual [] ]

        let translationInputs =
            [ makeTranslationInput "test translation" TranslationSource.Manual [] ]

        let expectedDefinition =
            makeDefinition 1 "A test definition" DefinitionSource.Manual 0 []

        let expectedTranslation =
            makeTranslation 1 "test translation" TranslationSource.Manual 0 []

        let createdEntry =
            makeEntry 1 1 "test word" now [ expectedDefinition ] [ expectedTranslation ]

        let env =
            TestEnv(
                getEntryById = (fun _ -> Task.FromResult(Some createdEntry)),
                getEntryByTextAndVocabularyId = (fun _ -> Task.FromResult(None)),
                createEntry = (fun _ -> Task.FromResult(EntryId 1)),
                createDefinition = (fun _ -> Task.FromResult(DefinitionId 1)),
                createTranslation = (fun _ -> Task.FromResult(TranslationId 1)),
                createExamplesForDefinition = (fun _ -> Task.FromResult(())),
                createExamplesForTranslation = (fun _ -> Task.FromResult(())),
                getDefaultVocabulary = (fun _ -> Task.FromResult(Some defaultVocabulary)),
                getDefaultCollection = (fun _ -> failwith "Should not be called"),
                createDefaultVocabulary = (fun _ -> failwith "Should not be called"),
                createDefaultCollection = (fun _ -> failwith "Should not be called")
            )

        let! result =
            Wordfolio.Api.Domain.Entries.DraftOperations.create
                env
                (makeCreateParams (UserId 1) "test word" definitionInputs translationInputs now)

        Assert.Equal(Ok createdEntry, result)

        Assert.Single(env.CreateEntryCalls)
        |> ignore

        Assert.Single(env.CreateDefinitionCalls)
        |> ignore

        Assert.Single(env.CreateTranslationCalls)
        |> ignore
    }

[<Fact>]
let ``trims whitespace from entry text``() =
    task {
        let now = DateTimeOffset.UtcNow

        let definitionInputs =
            [ makeDefinitionInput "A test definition" DefinitionSource.Manual [] ]

        let expectedDefinition =
            makeDefinition 1 "A test definition" DefinitionSource.Manual 0 []

        let createdEntry =
            makeEntry 1 1 "test word" now [ expectedDefinition ] []

        let env =
            TestEnv(
                getEntryById = (fun _ -> Task.FromResult(Some createdEntry)),
                getEntryByTextAndVocabularyId = (fun _ -> Task.FromResult(None)),
                createEntry =
                    (fun data ->
                        let text = data.EntryText

                        if text <> "test word" then
                            failwith $"Expected trimmed text 'test word', got: '{text}'"

                        Task.FromResult(EntryId 1)),
                createDefinition = (fun _ -> Task.FromResult(DefinitionId 1)),
                createTranslation = (fun _ -> failwith "Should not be called"),
                createExamplesForDefinition = (fun _ -> Task.FromResult(())),
                createExamplesForTranslation = (fun _ -> failwith "Should not be called"),
                getDefaultVocabulary = (fun _ -> Task.FromResult(Some defaultVocabulary)),
                getDefaultCollection = (fun _ -> failwith "Should not be called"),
                createDefaultVocabulary = (fun _ -> failwith "Should not be called"),
                createDefaultCollection = (fun _ -> failwith "Should not be called")
            )

        let! result =
            Wordfolio.Api.Domain.Entries.DraftOperations.create
                env
                (makeCreateParams (UserId 1) "  test word  " definitionInputs [] now)

        Assert.True(Result.isOk result)
    }

[<Fact>]
let ``returns error when entry text is empty``() =
    task {
        let env =
            TestEnv(
                getEntryById = (fun _ -> failwith "Should not be called"),
                getEntryByTextAndVocabularyId = (fun _ -> failwith "Should not be called"),
                createEntry = (fun _ -> failwith "Should not be called"),
                createDefinition = (fun _ -> failwith "Should not be called"),
                createTranslation = (fun _ -> failwith "Should not be called"),
                createExamplesForDefinition = (fun _ -> failwith "Should not be called"),
                createExamplesForTranslation = (fun _ -> failwith "Should not be called"),
                getDefaultVocabulary = (fun _ -> failwith "Should not be called"),
                getDefaultCollection = (fun _ -> failwith "Should not be called"),
                createDefaultVocabulary = (fun _ -> failwith "Should not be called"),
                createDefaultCollection = (fun _ -> failwith "Should not be called")
            )

        let! result =
            create
                env
                (makeCreateParams
                    (UserId 1)
                    ""
                    [ makeDefinitionInput "test" DefinitionSource.Manual [] ]
                    []
                    DateTimeOffset.UtcNow)

        Assert.Equal(Error CreateDraftEntryError.EntryTextRequired, result)
        Assert.Empty(env.CreateEntryCalls)
    }

[<Fact>]
let ``returns error when entry text is whitespace only``() =
    task {
        let env =
            TestEnv(
                getEntryById = (fun _ -> failwith "Should not be called"),
                getEntryByTextAndVocabularyId = (fun _ -> failwith "Should not be called"),
                createEntry = (fun _ -> failwith "Should not be called"),
                createDefinition = (fun _ -> failwith "Should not be called"),
                createTranslation = (fun _ -> failwith "Should not be called"),
                createExamplesForDefinition = (fun _ -> failwith "Should not be called"),
                createExamplesForTranslation = (fun _ -> failwith "Should not be called"),
                getDefaultVocabulary = (fun _ -> failwith "Should not be called"),
                getDefaultCollection = (fun _ -> failwith "Should not be called"),
                createDefaultVocabulary = (fun _ -> failwith "Should not be called"),
                createDefaultCollection = (fun _ -> failwith "Should not be called")
            )

        let! result =
            create
                env
                (makeCreateParams
                    (UserId 1)
                    "   "
                    [ makeDefinitionInput "test" DefinitionSource.Manual [] ]
                    []
                    DateTimeOffset.UtcNow)

        Assert.Equal(Error CreateDraftEntryError.EntryTextRequired, result)
    }

[<Fact>]
let ``returns error when entry text exceeds max length``() =
    task {
        let longText = String.replicate 201 "a"

        let env =
            TestEnv(
                getEntryById = (fun _ -> failwith "Should not be called"),
                getEntryByTextAndVocabularyId = (fun _ -> failwith "Should not be called"),
                createEntry = (fun _ -> failwith "Should not be called"),
                createDefinition = (fun _ -> failwith "Should not be called"),
                createTranslation = (fun _ -> failwith "Should not be called"),
                createExamplesForDefinition = (fun _ -> failwith "Should not be called"),
                createExamplesForTranslation = (fun _ -> failwith "Should not be called"),
                getDefaultVocabulary = (fun _ -> failwith "Should not be called"),
                getDefaultCollection = (fun _ -> failwith "Should not be called"),
                createDefaultVocabulary = (fun _ -> failwith "Should not be called"),
                createDefaultCollection = (fun _ -> failwith "Should not be called")
            )

        let! result =
            create
                env
                (makeCreateParams
                    (UserId 1)
                    longText
                    [ makeDefinitionInput "test" DefinitionSource.Manual [] ]
                    []
                    DateTimeOffset.UtcNow)

        Assert.Equal(Error(CreateDraftEntryError.EntryTextTooLong MaxEntryTextLength), result)
    }

[<Fact>]
let ``returns error when both definitions and translations are empty``() =
    task {
        let env =
            TestEnv(
                getEntryById = (fun _ -> failwith "Should not be called"),
                getEntryByTextAndVocabularyId = (fun _ -> failwith "Should not be called"),
                createEntry = (fun _ -> failwith "Should not be called"),
                createDefinition = (fun _ -> failwith "Should not be called"),
                createTranslation = (fun _ -> failwith "Should not be called"),
                createExamplesForDefinition = (fun _ -> failwith "Should not be called"),
                createExamplesForTranslation = (fun _ -> failwith "Should not be called"),
                getDefaultVocabulary = (fun _ -> failwith "Should not be called"),
                getDefaultCollection = (fun _ -> failwith "Should not be called"),
                createDefaultVocabulary = (fun _ -> failwith "Should not be called"),
                createDefaultCollection = (fun _ -> failwith "Should not be called")
            )

        let! result =
            Wordfolio.Api.Domain.Entries.DraftOperations.create
                env
                (makeCreateParams (UserId 1) "test word" [] [] DateTimeOffset.UtcNow)

        Assert.Equal(Error CreateDraftEntryError.NoDefinitionsOrTranslations, result)
    }

[<Fact>]
let ``returns error when duplicate entry exists``() =
    task {
        let now = DateTimeOffset.UtcNow

        let existingDefinition =
            makeDefinition 1 "existing definition" DefinitionSource.Manual 0 []

        let existingEntry =
            makeEntry 1 1 "test word" now [ existingDefinition ] []

        let env =
            TestEnv(
                getEntryById = (fun _ -> Task.FromResult(Some existingEntry)),
                getEntryByTextAndVocabularyId = (fun _ -> Task.FromResult(Some existingEntry)),
                createEntry = (fun _ -> failwith "Should not be called"),
                createDefinition = (fun _ -> failwith "Should not be called"),
                createTranslation = (fun _ -> failwith "Should not be called"),
                createExamplesForDefinition = (fun _ -> failwith "Should not be called"),
                createExamplesForTranslation = (fun _ -> failwith "Should not be called"),
                getDefaultVocabulary = (fun _ -> Task.FromResult(Some defaultVocabulary)),
                getDefaultCollection = (fun _ -> failwith "Should not be called"),
                createDefaultVocabulary = (fun _ -> failwith "Should not be called"),
                createDefaultCollection = (fun _ -> failwith "Should not be called")
            )

        let! result =
            create
                env
                (makeCreateParams
                    (UserId 1)
                    "test word"
                    [ makeDefinitionInput "test" DefinitionSource.Manual [] ]
                    []
                    now)

        Assert.Equal(Error(CreateDraftEntryError.DuplicateEntry existingEntry), result)
    }

[<Fact>]
let ``creates entry when duplicate exists and AllowDuplicate is true``() =
    task {
        let now = DateTimeOffset.UtcNow

        let existingEntry =
            makeEntry 1 1 "test word" now [] []

        let definitionInputs =
            [ makeDefinitionInput "A test definition" DefinitionSource.Manual [] ]

        let expectedDefinition =
            makeDefinition 2 "A test definition" DefinitionSource.Manual 0 []

        let createdEntry =
            makeEntry 2 1 "test word" now [ expectedDefinition ] []

        let env =
            TestEnv(
                getEntryById = (fun _ -> Task.FromResult(Some createdEntry)),
                getEntryByTextAndVocabularyId = (fun _ -> Task.FromResult(Some existingEntry)),
                createEntry = (fun _ -> Task.FromResult(EntryId 2)),
                createDefinition = (fun _ -> Task.FromResult(DefinitionId 2)),
                createTranslation = (fun _ -> failwith "Should not be called"),
                createExamplesForDefinition = (fun _ -> Task.FromResult(())),
                createExamplesForTranslation = (fun _ -> failwith "Should not be called"),
                getDefaultVocabulary = (fun _ -> Task.FromResult(Some defaultVocabulary)),
                getDefaultCollection = (fun _ -> failwith "Should not be called"),
                createDefaultVocabulary = (fun _ -> failwith "Should not be called"),
                createDefaultCollection = (fun _ -> failwith "Should not be called")
            )

        let parameters =
            { makeCreateParams (UserId 1) "test word" definitionInputs [] now with
                AllowDuplicate = true }

        let! result = Wordfolio.Api.Domain.Entries.DraftOperations.create env parameters

        Assert.Equal(Ok createdEntry, result)

        Assert.Single(env.CreateEntryCalls)
        |> ignore
    }

[<Fact>]
let ``returns error when example text is too long``() =
    task {
        let longExampleText =
            String.replicate 201 "a"

        let definitionInputs =
            [ makeDefinitionInput
                  "test"
                  DefinitionSource.Manual
                  [ makeExampleInput longExampleText ExampleSource.Custom ] ]

        let env =
            TestEnv(
                getEntryById = (fun _ -> failwith "Should not be called"),
                getEntryByTextAndVocabularyId = (fun _ -> failwith "Should not be called"),
                createEntry = (fun _ -> failwith "Should not be called"),
                createDefinition = (fun _ -> failwith "Should not be called"),
                createTranslation = (fun _ -> failwith "Should not be called"),
                createExamplesForDefinition = (fun _ -> failwith "Should not be called"),
                createExamplesForTranslation = (fun _ -> failwith "Should not be called"),
                getDefaultVocabulary = (fun _ -> failwith "Should not be called"),
                getDefaultCollection = (fun _ -> failwith "Should not be called"),
                createDefaultVocabulary = (fun _ -> failwith "Should not be called"),
                createDefaultCollection = (fun _ -> failwith "Should not be called")
            )

        let! result =
            Wordfolio.Api.Domain.Entries.DraftOperations.create
                env
                (makeCreateParams (UserId 1) "test word" definitionInputs [] DateTimeOffset.UtcNow)

        Assert.Equal(Error(CreateDraftEntryError.ExampleTextTooLong MaxExampleTextLength), result)
    }

[<Fact>]
let ``returns error when too many examples in definition``() =
    task {
        let examples =
            [ 1..6 ]
            |> List.map(fun i -> makeExampleInput $"example {i}" ExampleSource.Custom)

        let definitionInputs =
            [ makeDefinitionInput "test" DefinitionSource.Manual examples ]

        let env =
            TestEnv(
                getEntryById = (fun _ -> failwith "Should not be called"),
                getEntryByTextAndVocabularyId = (fun _ -> failwith "Should not be called"),
                createEntry = (fun _ -> failwith "Should not be called"),
                createDefinition = (fun _ -> failwith "Should not be called"),
                createTranslation = (fun _ -> failwith "Should not be called"),
                createExamplesForDefinition = (fun _ -> failwith "Should not be called"),
                createExamplesForTranslation = (fun _ -> failwith "Should not be called"),
                getDefaultVocabulary = (fun _ -> failwith "Should not be called"),
                getDefaultCollection = (fun _ -> failwith "Should not be called"),
                createDefaultVocabulary = (fun _ -> failwith "Should not be called"),
                createDefaultCollection = (fun _ -> failwith "Should not be called")
            )

        let! result =
            Wordfolio.Api.Domain.Entries.DraftOperations.create
                env
                (makeCreateParams (UserId 1) "test word" definitionInputs [] DateTimeOffset.UtcNow)

        Assert.Equal(Error(CreateDraftEntryError.TooManyExamples MaxExamplesPerItem), result)
    }

[<Fact>]
let ``returns error when too many examples in translation``() =
    task {
        let examples =
            [ 1..6 ]
            |> List.map(fun i -> makeExampleInput $"example {i}" ExampleSource.Custom)

        let translationInputs =
            [ makeTranslationInput "test" TranslationSource.Manual examples ]

        let env =
            TestEnv(
                getEntryById = (fun _ -> failwith "Should not be called"),
                getEntryByTextAndVocabularyId = (fun _ -> failwith "Should not be called"),
                createEntry = (fun _ -> failwith "Should not be called"),
                createDefinition = (fun _ -> failwith "Should not be called"),
                createTranslation = (fun _ -> failwith "Should not be called"),
                createExamplesForDefinition = (fun _ -> failwith "Should not be called"),
                createExamplesForTranslation = (fun _ -> failwith "Should not be called"),
                getDefaultVocabulary = (fun _ -> failwith "Should not be called"),
                getDefaultCollection = (fun _ -> failwith "Should not be called"),
                createDefaultVocabulary = (fun _ -> failwith "Should not be called"),
                createDefaultCollection = (fun _ -> failwith "Should not be called")
            )

        let! result =
            Wordfolio.Api.Domain.Entries.DraftOperations.create
                env
                (makeCreateParams (UserId 1) "test word" [] translationInputs DateTimeOffset.UtcNow)

        Assert.Equal(Error(CreateDraftEntryError.TooManyExamples MaxExamplesPerItem), result)
    }

[<Fact>]
let ``proceeds when duplicate text match finds a stale record``() =
    task {
        let now = DateTimeOffset.UtcNow

        let staleEntry =
            makeEntry 99 1 "test word" now [] []

        let definitionInputs =
            [ makeDefinitionInput "A test definition" DefinitionSource.Manual [] ]

        let expectedDefinition =
            makeDefinition 1 "A test definition" DefinitionSource.Manual 0 []

        let createdEntry =
            makeEntry 1 1 "test word" now [ expectedDefinition ] []

        let env =
            TestEnv(
                getEntryById =
                    (fun id ->
                        if id = EntryId 99 then
                            Task.FromResult(None)
                        else
                            Task.FromResult(Some createdEntry)),
                getEntryByTextAndVocabularyId = (fun _ -> Task.FromResult(Some staleEntry)),
                createEntry = (fun _ -> Task.FromResult(EntryId 1)),
                createDefinition = (fun _ -> Task.FromResult(DefinitionId 1)),
                createTranslation = (fun _ -> failwith "Should not be called"),
                createExamplesForDefinition = (fun _ -> Task.FromResult(())),
                createExamplesForTranslation = (fun _ -> failwith "Should not be called"),
                getDefaultVocabulary = (fun _ -> Task.FromResult(Some defaultVocabulary)),
                getDefaultCollection = (fun _ -> failwith "Should not be called"),
                createDefaultVocabulary = (fun _ -> failwith "Should not be called"),
                createDefaultCollection = (fun _ -> failwith "Should not be called")
            )

        let! result =
            Wordfolio.Api.Domain.Entries.DraftOperations.create
                env
                (makeCreateParams (UserId 1) "test word" definitionInputs [] now)

        Assert.Equal(Ok createdEntry, result)
    }

[<Fact>]
let ``throws when post-create entry fetch returns None``() =
    task {
        let now = DateTimeOffset.UtcNow

        let definitionInputs =
            [ makeDefinitionInput "A test definition" DefinitionSource.Manual [] ]

        let env =
            TestEnv(
                getEntryById = (fun _ -> Task.FromResult(None)),
                getEntryByTextAndVocabularyId = (fun _ -> Task.FromResult(None)),
                createEntry = (fun _ -> Task.FromResult(EntryId 1)),
                createDefinition = (fun _ -> Task.FromResult(DefinitionId 1)),
                createTranslation = (fun _ -> failwith "Should not be called"),
                createExamplesForDefinition = (fun _ -> Task.FromResult(())),
                createExamplesForTranslation = (fun _ -> failwith "Should not be called"),
                getDefaultVocabulary = (fun _ -> Task.FromResult(Some defaultVocabulary)),
                getDefaultCollection = (fun _ -> failwith "Should not be called"),
                createDefaultVocabulary = (fun _ -> failwith "Should not be called"),
                createDefaultCollection = (fun _ -> failwith "Should not be called")
            )

        let! ex =
            Assert.ThrowsAsync<Exception>(fun () ->
                Wordfolio.Api.Domain.Entries.DraftOperations.create
                    env
                    (makeCreateParams (UserId 1) "test word" definitionInputs [] now)
                :> Task)

        Assert.Equal("Entry EntryId 1 not found after creation", ex.Message)
    }

[<Fact>]
let ``creates entry when default vocabulary does not exist but default collection does``() =
    task {
        let now = DateTimeOffset.UtcNow

        let definitionInputs =
            [ makeDefinitionInput "A test definition" DefinitionSource.Manual [] ]

        let expectedDefinition =
            makeDefinition 1 "A test definition" DefinitionSource.Manual 0 []

        let createdEntry =
            makeEntry 1 1 "test word" now [ expectedDefinition ] []

        let existingCollection: Collection =
            { Id = CollectionId 1
              UserId = UserId 1
              Name = "[System] Unsorted"
              Description = None
              CreatedAt = now
              UpdatedAt = None }

        let env =
            TestEnv(
                getEntryById = (fun _ -> Task.FromResult(Some createdEntry)),
                getEntryByTextAndVocabularyId = (fun _ -> Task.FromResult(None)),
                createEntry = (fun _ -> Task.FromResult(EntryId 1)),
                createDefinition = (fun _ -> Task.FromResult(DefinitionId 1)),
                createTranslation = (fun _ -> failwith "Should not be called"),
                createExamplesForDefinition = (fun _ -> Task.FromResult(())),
                createExamplesForTranslation = (fun _ -> failwith "Should not be called"),
                getDefaultVocabulary = (fun _ -> Task.FromResult(None)),
                getDefaultCollection = (fun _ -> Task.FromResult(Some existingCollection)),
                createDefaultVocabulary = (fun _ -> Task.FromResult(VocabularyId 1)),
                createDefaultCollection = (fun _ -> failwith "Should not be called")
            )

        let! result =
            Wordfolio.Api.Domain.Entries.DraftOperations.create
                env
                (makeCreateParams (UserId 1) "test word" definitionInputs [] now)

        Assert.Equal(Ok createdEntry, result)
    }

[<Fact>]
let ``creates entry when neither default vocabulary nor default collection exists``() =
    task {
        let now = DateTimeOffset.UtcNow

        let definitionInputs =
            [ makeDefinitionInput "A test definition" DefinitionSource.Manual [] ]

        let expectedDefinition =
            makeDefinition 1 "A test definition" DefinitionSource.Manual 0 []

        let createdEntry =
            makeEntry 1 1 "test word" now [ expectedDefinition ] []

        let env =
            TestEnv(
                getEntryById = (fun _ -> Task.FromResult(Some createdEntry)),
                getEntryByTextAndVocabularyId = (fun _ -> Task.FromResult(None)),
                createEntry = (fun _ -> Task.FromResult(EntryId 1)),
                createDefinition = (fun _ -> Task.FromResult(DefinitionId 1)),
                createTranslation = (fun _ -> failwith "Should not be called"),
                createExamplesForDefinition = (fun _ -> Task.FromResult(())),
                createExamplesForTranslation = (fun _ -> failwith "Should not be called"),
                getDefaultVocabulary = (fun _ -> Task.FromResult(None)),
                getDefaultCollection = (fun _ -> Task.FromResult(None)),
                createDefaultVocabulary = (fun _ -> Task.FromResult(VocabularyId 1)),
                createDefaultCollection = (fun _ -> Task.FromResult(CollectionId 1))
            )

        let! result =
            Wordfolio.Api.Domain.Entries.DraftOperations.create
                env
                (makeCreateParams (UserId 1) "test word" definitionInputs [] now)

        Assert.Equal(Ok createdEntry, result)
    }
