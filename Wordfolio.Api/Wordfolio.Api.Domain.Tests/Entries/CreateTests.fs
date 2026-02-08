module Wordfolio.Api.Domain.Tests.Entries.CreateTests

open System
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Collections
open Wordfolio.Api.Domain.Entries
open Wordfolio.Api.Domain.Entries.Operations
open Wordfolio.Api.Domain.Shared
open Wordfolio.Api.Domain.Vocabularies

type TestEnv
    (
        getEntryById: EntryId -> Task<Entry option>,
        getEntryByTextAndVocabularyId: VocabularyId * string -> Task<Entry option>,
        createEntry: VocabularyId * string * DateTimeOffset -> Task<EntryId>,
        createDefinition: EntryId * string * DefinitionSource * int -> Task<DefinitionId>,
        createTranslation: EntryId * string * TranslationSource * int -> Task<TranslationId>,
        createExamplesForDefinition: DefinitionId * ExampleInput list -> Task<unit>,
        createExamplesForTranslation: TranslationId * ExampleInput list -> Task<unit>,
        hasVocabularyAccess: VocabularyId * UserId -> Task<bool>
    ) =
    let getEntryByIdCalls =
        ResizeArray<EntryId>()

    let getEntryByTextAndVocabularyIdCalls =
        ResizeArray<VocabularyId * string>()

    let createEntryCalls =
        ResizeArray<VocabularyId * string * DateTimeOffset>()

    let createDefinitionCalls =
        ResizeArray<EntryId * string * DefinitionSource * int>()

    let createTranslationCalls =
        ResizeArray<EntryId * string * TranslationSource * int>()

    let createExamplesForDefinitionCalls =
        ResizeArray<DefinitionId * ExampleInput list>()

    let createExamplesForTranslationCalls =
        ResizeArray<TranslationId * ExampleInput list>()

    let hasVocabularyAccessCalls =
        ResizeArray<VocabularyId * UserId>()

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

    member _.HasVocabularyAccessCalls =
        hasVocabularyAccessCalls |> Seq.toList

    interface IGetEntryById with
        member _.GetEntryById(id) =
            getEntryByIdCalls.Add(id)
            getEntryById id

    interface IGetEntryByTextAndVocabularyId with
        member _.GetEntryByTextAndVocabularyId(vocabularyId, text) =
            getEntryByTextAndVocabularyIdCalls.Add(vocabularyId, text)
            getEntryByTextAndVocabularyId(vocabularyId, text)

    interface ICreateEntry with
        member _.CreateEntry(vocabularyId, text, createdAt) =
            createEntryCalls.Add(vocabularyId, text, createdAt)
            createEntry(vocabularyId, text, createdAt)

    interface ICreateDefinition with
        member _.CreateDefinition(entryId, text, source, displayOrder) =
            createDefinitionCalls.Add(entryId, text, source, displayOrder)
            createDefinition(entryId, text, source, displayOrder)

    interface ICreateTranslation with
        member _.CreateTranslation(entryId, text, source, displayOrder) =
            createTranslationCalls.Add(entryId, text, source, displayOrder)
            createTranslation(entryId, text, source, displayOrder)

    interface ICreateExamplesForDefinition with
        member _.CreateExamplesForDefinition(definitionId, examples) =
            createExamplesForDefinitionCalls.Add(definitionId, examples)
            createExamplesForDefinition(definitionId, examples)

    interface ICreateExamplesForTranslation with
        member _.CreateExamplesForTranslation(translationId, examples) =
            createExamplesForTranslationCalls.Add(translationId, examples)
            createExamplesForTranslation(translationId, examples)

    interface IHasVocabularyAccess with
        member _.HasVocabularyAccess(vocabularyId, userId) =
            hasVocabularyAccessCalls.Add(vocabularyId, userId)
            hasVocabularyAccess(vocabularyId, userId)

    interface ITransactional<TestEnv> with
        member this.RunInTransaction(operation) = operation this

    interface IGetDefaultVocabulary with
        member _.GetDefaultVocabulary(_) =
            failwith "IGetDefaultVocabulary not expected in these tests"

    interface ICreateDefaultVocabulary with
        member _.CreateDefaultVocabulary(_) =
            failwith "ICreateDefaultVocabulary not expected in these tests"

    interface IGetDefaultCollection with
        member _.GetDefaultCollection(_) =
            failwith "IGetDefaultCollection not expected in these tests"

    interface ICreateDefaultCollection with
        member _.CreateDefaultCollection(_) =
            failwith "ICreateDefaultCollection not expected in these tests"

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

let makeCreateParams userId vocabularyId entryText definitions translations createdAt : CreateEntryParameters =
    { UserId = userId
      VocabularyId = Some vocabularyId
      EntryText = entryText
      Definitions = definitions
      Translations = translations
      AllowDuplicate = false
      CreatedAt = createdAt }

[<Fact>]
let ``creates entry with definitions only``() =
    task {
        let now = DateTimeOffset.UtcNow

        let vocabulary =
            makeVocabulary 1 1 "Test Vocabulary"

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
                hasVocabularyAccess = (fun _ -> Task.FromResult(true))
            )

        let! result = create env (makeCreateParams (UserId 1) (VocabularyId 1) "test word" definitionInputs [] now)

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

        let vocabulary =
            makeVocabulary 1 1 "Test Vocabulary"

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
                hasVocabularyAccess = (fun _ -> Task.FromResult(true))
            )

        let! result = create env (makeCreateParams (UserId 1) (VocabularyId 1) "test word" [] translationInputs now)

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

        let vocabulary =
            makeVocabulary 1 1 "Test Vocabulary"

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
                hasVocabularyAccess = (fun _ -> Task.FromResult(true))
            )

        let! result =
            create env (makeCreateParams (UserId 1) (VocabularyId 1) "test word" definitionInputs translationInputs now)

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

        let vocabulary =
            makeVocabulary 1 1 "Test Vocabulary"

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
                    (fun (_, text, _) ->
                        if text <> "test word" then
                            failwith $"Expected trimmed text 'test word', got: '{text}'"

                        Task.FromResult(EntryId 1)),
                createDefinition = (fun _ -> Task.FromResult(DefinitionId 1)),
                createTranslation = (fun _ -> failwith "Should not be called"),
                createExamplesForDefinition = (fun _ -> Task.FromResult(())),
                createExamplesForTranslation = (fun _ -> failwith "Should not be called"),
                hasVocabularyAccess = (fun _ -> Task.FromResult(true))
            )

        let! result = create env (makeCreateParams (UserId 1) (VocabularyId 1) "  test word  " definitionInputs [] now)

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
                hasVocabularyAccess = (fun _ -> failwith "Should not be called")
            )

        let! result =
            create
                env
                (makeCreateParams
                    (UserId 1)
                    (VocabularyId 1)
                    ""
                    [ makeDefinitionInput "test" DefinitionSource.Manual [] ]
                    []
                    DateTimeOffset.UtcNow)

        Assert.Equal(Error EntryTextRequired, result)
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
                hasVocabularyAccess = (fun _ -> failwith "Should not be called")
            )

        let! result =
            create
                env
                (makeCreateParams
                    (UserId 1)
                    (VocabularyId 1)
                    "   "
                    [ makeDefinitionInput "test" DefinitionSource.Manual [] ]
                    []
                    DateTimeOffset.UtcNow)

        Assert.Equal(Error EntryTextRequired, result)
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
                hasVocabularyAccess = (fun _ -> failwith "Should not be called")
            )

        let! result =
            create
                env
                (makeCreateParams
                    (UserId 1)
                    (VocabularyId 1)
                    longText
                    [ makeDefinitionInput "test" DefinitionSource.Manual [] ]
                    []
                    DateTimeOffset.UtcNow)

        Assert.Equal(Error(EntryTextTooLong MaxEntryTextLength), result)
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
                hasVocabularyAccess = (fun _ -> failwith "Should not be called")
            )

        let! result = create env (makeCreateParams (UserId 1) (VocabularyId 1) "test word" [] [] DateTimeOffset.UtcNow)

        Assert.Equal(Error NoDefinitionsOrTranslations, result)
    }

[<Fact>]
let ``returns error when vocabulary is not found``() =
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
                hasVocabularyAccess = (fun _ -> Task.FromResult(false))
            )

        let! result =
            create
                env
                (makeCreateParams
                    (UserId 1)
                    (VocabularyId 1)
                    "test word"
                    [ makeDefinitionInput "test" DefinitionSource.Manual [] ]
                    []
                    DateTimeOffset.UtcNow)

        Assert.Equal(Error(VocabularyNotFoundOrAccessDenied(VocabularyId 1)), result)
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
                hasVocabularyAccess = (fun _ -> Task.FromResult(true))
            )

        let! result =
            create
                env
                (makeCreateParams
                    (UserId 1)
                    (VocabularyId 1)
                    "test word"
                    [ makeDefinitionInput "test" DefinitionSource.Manual [] ]
                    []
                    now)

        Assert.Equal(Error(DuplicateEntry existingEntry), result)
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
                hasVocabularyAccess = (fun _ -> Task.FromResult(true))
            )

        let parameters =
            { makeCreateParams (UserId 1) (VocabularyId 1) "test word" definitionInputs [] now with
                AllowDuplicate = true }

        let! result = create env parameters

        Assert.Equal(Ok createdEntry, result)

        Assert.Single(env.CreateEntryCalls)
        |> ignore
    }

[<Fact>]
let ``returns error when example text is too long``() =
    task {
        let vocabulary =
            makeVocabulary 1 1 "Test Vocabulary"

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
                getEntryByTextAndVocabularyId = (fun _ -> Task.FromResult(None)),
                createEntry = (fun _ -> failwith "Should not be called"),
                createDefinition = (fun _ -> failwith "Should not be called"),
                createTranslation = (fun _ -> failwith "Should not be called"),
                createExamplesForDefinition = (fun _ -> failwith "Should not be called"),
                createExamplesForTranslation = (fun _ -> failwith "Should not be called"),
                hasVocabularyAccess = (fun _ -> Task.FromResult(true))
            )

        let! result =
            create
                env
                (makeCreateParams (UserId 1) (VocabularyId 1) "test word" definitionInputs [] DateTimeOffset.UtcNow)

        Assert.Equal(Error(ExampleTextTooLong MaxExampleTextLength), result)
    }

[<Fact>]
let ``returns error when too many examples in definition``() =
    task {
        let vocabulary =
            makeVocabulary 1 1 "Test Vocabulary"

        let examples =
            [ 1..6 ]
            |> List.map(fun i -> makeExampleInput $"example {i}" ExampleSource.Custom)

        let definitionInputs =
            [ makeDefinitionInput "test" DefinitionSource.Manual examples ]

        let env =
            TestEnv(
                getEntryById = (fun _ -> failwith "Should not be called"),
                getEntryByTextAndVocabularyId = (fun _ -> Task.FromResult(None)),
                createEntry = (fun _ -> failwith "Should not be called"),
                createDefinition = (fun _ -> failwith "Should not be called"),
                createTranslation = (fun _ -> failwith "Should not be called"),
                createExamplesForDefinition = (fun _ -> failwith "Should not be called"),
                createExamplesForTranslation = (fun _ -> failwith "Should not be called"),
                hasVocabularyAccess = (fun _ -> Task.FromResult(true))
            )

        let! result =
            create
                env
                (makeCreateParams (UserId 1) (VocabularyId 1) "test word" definitionInputs [] DateTimeOffset.UtcNow)

        Assert.Equal(Error(TooManyExamples MaxExamplesPerItem), result)
    }

[<Fact>]
let ``returns error when too many examples in translation``() =
    task {
        let vocabulary =
            makeVocabulary 1 1 "Test Vocabulary"

        let examples =
            [ 1..6 ]
            |> List.map(fun i -> makeExampleInput $"example {i}" ExampleSource.Custom)

        let translationInputs =
            [ makeTranslationInput "test" TranslationSource.Manual examples ]

        let env =
            TestEnv(
                getEntryById = (fun _ -> failwith "Should not be called"),
                getEntryByTextAndVocabularyId = (fun _ -> Task.FromResult(None)),
                createEntry = (fun _ -> failwith "Should not be called"),
                createDefinition = (fun _ -> failwith "Should not be called"),
                createTranslation = (fun _ -> failwith "Should not be called"),
                createExamplesForDefinition = (fun _ -> failwith "Should not be called"),
                createExamplesForTranslation = (fun _ -> failwith "Should not be called"),
                hasVocabularyAccess = (fun _ -> Task.FromResult(true))
            )

        let! result =
            create
                env
                (makeCreateParams (UserId 1) (VocabularyId 1) "test word" [] translationInputs DateTimeOffset.UtcNow)

        Assert.Equal(Error(TooManyExamples MaxExamplesPerItem), result)
    }
