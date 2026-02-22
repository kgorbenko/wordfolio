module Wordfolio.Api.Domain.Tests.Entries.EntryOperations.CreateTests

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
        getEntryByTextAndVocabularyId: VocabularyId * string -> Task<Entry option>,
        hasVocabularyAccessInCollection: VocabularyId * CollectionId * UserId -> Task<bool>,
        createEntry: VocabularyId * string * DateTimeOffset -> Task<EntryId>,
        createDefinition: EntryId * string * DefinitionSource * int -> Task<DefinitionId>,
        createTranslation: EntryId * string * TranslationSource * int -> Task<TranslationId>,
        createExamplesForDefinition: DefinitionId * ExampleInput list -> Task<unit>,
        createExamplesForTranslation: TranslationId * ExampleInput list -> Task<unit>
    ) =
    let getEntryByIdCalls =
        ResizeArray<EntryId>()

    let getEntryByTextAndVocabularyIdCalls =
        ResizeArray<VocabularyId * string>()

    let hasVocabularyAccessInCollectionCalls =
        ResizeArray<VocabularyId * CollectionId * UserId>()

    member _.GetEntryByIdCalls =
        getEntryByIdCalls |> Seq.toList

    member _.GetEntryByTextAndVocabularyIdCalls =
        getEntryByTextAndVocabularyIdCalls
        |> Seq.toList

    member _.HasVocabularyAccessInCollectionCalls =
        hasVocabularyAccessInCollectionCalls
        |> Seq.toList

    interface IGetEntryById with
        member _.GetEntryById(id) =
            getEntryByIdCalls.Add(id)
            getEntryById id

    interface IGetEntryByTextAndVocabularyId with
        member _.GetEntryByTextAndVocabularyId(vocabularyId, text) =
            getEntryByTextAndVocabularyIdCalls.Add(vocabularyId, text)
            getEntryByTextAndVocabularyId(vocabularyId, text)

    interface IHasVocabularyAccessInCollection with
        member _.HasVocabularyAccessInCollection(vocabularyId, collectionId, userId) =
            hasVocabularyAccessInCollectionCalls.Add(vocabularyId, collectionId, userId)
            hasVocabularyAccessInCollection(vocabularyId, collectionId, userId)

    interface ICreateEntry with
        member _.CreateEntry(vocabularyId, text, createdAt) =
            createEntry(vocabularyId, text, createdAt)

    interface ICreateDefinition with
        member _.CreateDefinition(entryId, text, source, displayOrder) =
            createDefinition(entryId, text, source, displayOrder)

    interface ICreateTranslation with
        member _.CreateTranslation(entryId, text, source, displayOrder) =
            createTranslation(entryId, text, source, displayOrder)

    interface ICreateExamplesForDefinition with
        member _.CreateExamplesForDefinition(definitionId, examples) =
            createExamplesForDefinition(definitionId, examples)

    interface ICreateExamplesForTranslation with
        member _.CreateExamplesForTranslation(translationId, examples) =
            createExamplesForTranslation(translationId, examples)

    interface ITransactional<TestEnv> with
        member this.RunInTransaction(operation) = operation this

let makeEntry id vocabularyId text =
    { Id = EntryId id
      VocabularyId = VocabularyId vocabularyId
      EntryText = text
      CreatedAt = DateTimeOffset.UtcNow
      UpdatedAt = None
      Definitions = []
      Translations = [] }

let makeDefinitionInput text source examples =
    { DefinitionText = text
      Source = source
      Examples = examples }

let makeTranslationInput text source examples =
    { TranslationText = text
      Source = source
      Examples = examples }

let makeExampleInput text source = { ExampleText = text; Source = source }

let makeParameters userId vocabularyId entryText definitions translations allowDuplicate createdAt =
    { UserId = UserId userId
      VocabularyId = VocabularyId vocabularyId
      EntryText = entryText
      Definitions = definitions
      Translations = translations
      AllowDuplicate = allowDuplicate
      CreatedAt = createdAt }

let makeEntryWithContent id vocabularyId text createdAt definitions translations =
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

[<Fact>]
let ``creates entry when vocabulary is in collection``() =
    task {
        let now = DateTimeOffset.UtcNow
        let createdEntry = makeEntry 1 10 "hello"

        let definitions =
            [ makeDefinitionInput "a greeting" DefinitionSource.Manual [] ]

        let parameters =
            makeParameters 1 10 "hello" definitions [] false now

        let env =
            TestEnv(
                getEntryById =
                    (fun id ->
                        if id = EntryId 1 then
                            Task.FromResult(Some createdEntry)
                        else
                            Task.FromResult(None)),
                getEntryByTextAndVocabularyId = (fun _ -> Task.FromResult(None)),
                hasVocabularyAccessInCollection = (fun _ -> Task.FromResult(true)),
                createEntry = (fun _ -> Task.FromResult(EntryId 1)),
                createDefinition = (fun _ -> Task.FromResult(DefinitionId 10)),
                createTranslation = (fun _ -> failwith "Should not be called"),
                createExamplesForDefinition = (fun _ -> Task.FromResult(())),
                createExamplesForTranslation = (fun _ -> failwith "Should not be called")
            )

        let! result = create env (CollectionId 5) parameters

        Assert.Equal(Ok createdEntry, result)

        Assert.Equal<(VocabularyId * CollectionId * UserId) list>(
            [ VocabularyId 10, CollectionId 5, UserId 1 ],
            env.HasVocabularyAccessInCollectionCalls
        )
    }

[<Fact>]
let ``returns VocabularyNotFoundOrAccessDenied when vocabulary is not in collection``() =
    task {
        let definitions =
            [ makeDefinitionInput "a greeting" DefinitionSource.Manual [] ]

        let parameters =
            makeParameters 1 10 "hello" definitions [] false DateTimeOffset.UtcNow

        let env =
            TestEnv(
                getEntryById = (fun _ -> failwith "Should not be called"),
                getEntryByTextAndVocabularyId = (fun _ -> failwith "Should not be called"),
                hasVocabularyAccessInCollection = (fun _ -> Task.FromResult(false)),
                createEntry = (fun _ -> failwith "Should not be called"),
                createDefinition = (fun _ -> failwith "Should not be called"),
                createTranslation = (fun _ -> failwith "Should not be called"),
                createExamplesForDefinition = (fun _ -> failwith "Should not be called"),
                createExamplesForTranslation = (fun _ -> failwith "Should not be called")
            )

        let! result = create env (CollectionId 5) parameters

        Assert.Equal(Error(VocabularyNotFoundOrAccessDenied(VocabularyId 10)), result)

        Assert.Equal<(VocabularyId * CollectionId * UserId) list>(
            [ VocabularyId 10, CollectionId 5, UserId 1 ],
            env.HasVocabularyAccessInCollectionCalls
        )
    }

[<Fact>]
let ``returns DuplicateEntry when entry text already exists in vocabulary``() =
    task {
        let now = DateTimeOffset.UtcNow
        let duplicateEntry = makeEntry 99 10 "hello"

        let definitions =
            [ makeDefinitionInput "a greeting" DefinitionSource.Manual [] ]

        let parameters =
            makeParameters 1 10 "hello" definitions [] false now

        let env =
            TestEnv(
                getEntryById =
                    (fun id ->
                        if id = EntryId 99 then
                            Task.FromResult(Some duplicateEntry)
                        else
                            Task.FromResult(None)),
                getEntryByTextAndVocabularyId = (fun _ -> Task.FromResult(Some duplicateEntry)),
                hasVocabularyAccessInCollection = (fun _ -> Task.FromResult(true)),
                createEntry = (fun _ -> failwith "Should not be called"),
                createDefinition = (fun _ -> failwith "Should not be called"),
                createTranslation = (fun _ -> failwith "Should not be called"),
                createExamplesForDefinition = (fun _ -> failwith "Should not be called"),
                createExamplesForTranslation = (fun _ -> failwith "Should not be called")
            )

        let! result = create env (CollectionId 5) parameters

        Assert.Equal(Error(DuplicateEntry duplicateEntry), result)
    }

[<Fact>]
let ``creates entry when AllowDuplicate is true even if duplicate exists``() =
    task {
        let now = DateTimeOffset.UtcNow
        let createdEntry = makeEntry 2 10 "hello"

        let definitions =
            [ makeDefinitionInput "another meaning" DefinitionSource.Manual [] ]

        let parameters =
            makeParameters 1 10 "hello" definitions [] true now

        let env =
            TestEnv(
                getEntryById =
                    (fun id ->
                        if id = EntryId 2 then
                            Task.FromResult(Some createdEntry)
                        else
                            Task.FromResult(None)),
                getEntryByTextAndVocabularyId = (fun _ -> failwith "Should not be called"),
                hasVocabularyAccessInCollection = (fun _ -> Task.FromResult(true)),
                createEntry = (fun _ -> Task.FromResult(EntryId 2)),
                createDefinition = (fun _ -> Task.FromResult(DefinitionId 10)),
                createTranslation = (fun _ -> failwith "Should not be called"),
                createExamplesForDefinition = (fun _ -> Task.FromResult(())),
                createExamplesForTranslation = (fun _ -> failwith "Should not be called")
            )

        let! result = create env (CollectionId 5) parameters

        Assert.Equal(Ok createdEntry, result)
        Assert.Empty(env.GetEntryByTextAndVocabularyIdCalls)
    }

[<Fact>]
let ``returns NoDefinitionsOrTranslations when both lists are empty``() =
    task {
        let parameters =
            makeParameters 1 10 "hello" [] [] false DateTimeOffset.UtcNow

        let env =
            TestEnv(
                getEntryById = (fun _ -> failwith "Should not be called"),
                getEntryByTextAndVocabularyId = (fun _ -> failwith "Should not be called"),
                hasVocabularyAccessInCollection = (fun _ -> failwith "Should not be called"),
                createEntry = (fun _ -> failwith "Should not be called"),
                createDefinition = (fun _ -> failwith "Should not be called"),
                createTranslation = (fun _ -> failwith "Should not be called"),
                createExamplesForDefinition = (fun _ -> failwith "Should not be called"),
                createExamplesForTranslation = (fun _ -> failwith "Should not be called")
            )

        let! result = create env (CollectionId 5) parameters

        Assert.Equal(Error NoDefinitionsOrTranslations, result)
    }

[<Fact>]
let ``returns EntryTextRequired when entry text is empty``() =
    task {
        let definitions =
            [ makeDefinitionInput "a greeting" DefinitionSource.Manual [] ]

        let parameters =
            makeParameters 1 10 "" definitions [] false DateTimeOffset.UtcNow

        let env =
            TestEnv(
                getEntryById = (fun _ -> failwith "Should not be called"),
                getEntryByTextAndVocabularyId = (fun _ -> failwith "Should not be called"),
                hasVocabularyAccessInCollection = (fun _ -> failwith "Should not be called"),
                createEntry = (fun _ -> failwith "Should not be called"),
                createDefinition = (fun _ -> failwith "Should not be called"),
                createTranslation = (fun _ -> failwith "Should not be called"),
                createExamplesForDefinition = (fun _ -> failwith "Should not be called"),
                createExamplesForTranslation = (fun _ -> failwith "Should not be called")
            )

        let! result = create env (CollectionId 5) parameters

        Assert.Equal(Error EntryTextRequired, result)
    }

[<Fact>]
let ``returns EntryTextTooLong when entry text exceeds max length``() =
    task {
        let longText =
            String.replicate (MaxEntryTextLength + 1) "a"

        let definitions =
            [ makeDefinitionInput "a greeting" DefinitionSource.Manual [] ]

        let parameters =
            makeParameters 1 10 longText definitions [] false DateTimeOffset.UtcNow

        let env =
            TestEnv(
                getEntryById = (fun _ -> failwith "Should not be called"),
                getEntryByTextAndVocabularyId = (fun _ -> failwith "Should not be called"),
                hasVocabularyAccessInCollection = (fun _ -> failwith "Should not be called"),
                createEntry = (fun _ -> failwith "Should not be called"),
                createDefinition = (fun _ -> failwith "Should not be called"),
                createTranslation = (fun _ -> failwith "Should not be called"),
                createExamplesForDefinition = (fun _ -> failwith "Should not be called"),
                createExamplesForTranslation = (fun _ -> failwith "Should not be called")
            )

        let! result = create env (CollectionId 5) parameters

        Assert.Equal(Error(EntryTextTooLong MaxEntryTextLength), result)
    }

[<Fact>]
let ``returns ExampleTextTooLong when definition example text exceeds max length``() =
    task {
        let longExample =
            String.replicate (MaxExampleTextLength + 1) "a"

        let definitions =
            [ makeDefinitionInput
                  "a greeting"
                  DefinitionSource.Manual
                  [ makeExampleInput longExample ExampleSource.Custom ] ]

        let parameters =
            makeParameters 1 10 "hello" definitions [] false DateTimeOffset.UtcNow

        let env =
            TestEnv(
                getEntryById = (fun _ -> failwith "Should not be called"),
                getEntryByTextAndVocabularyId = (fun _ -> Task.FromResult(None)),
                hasVocabularyAccessInCollection = (fun _ -> Task.FromResult(true)),
                createEntry = (fun _ -> failwith "Should not be called"),
                createDefinition = (fun _ -> failwith "Should not be called"),
                createTranslation = (fun _ -> failwith "Should not be called"),
                createExamplesForDefinition = (fun _ -> failwith "Should not be called"),
                createExamplesForTranslation = (fun _ -> failwith "Should not be called")
            )

        let! result = create env (CollectionId 5) parameters

        Assert.Equal(Error(ExampleTextTooLong MaxExampleTextLength), result)
    }

[<Fact>]
let ``creates entry with translations only``() =
    task {
        let now = DateTimeOffset.UtcNow

        let expectedTranslation =
            makeTranslation 1 "hello in spanish" TranslationSource.Manual 0 []

        let createdEntry =
            makeEntryWithContent 1 10 "hello" now [] [ expectedTranslation ]

        let translations =
            [ makeTranslationInput "hello in spanish" TranslationSource.Manual [] ]

        let parameters =
            makeParameters 1 10 "hello" [] translations false now

        let env =
            TestEnv(
                getEntryById = (fun _ -> Task.FromResult(Some createdEntry)),
                getEntryByTextAndVocabularyId = (fun _ -> Task.FromResult(None)),
                hasVocabularyAccessInCollection = (fun _ -> Task.FromResult(true)),
                createEntry = (fun _ -> Task.FromResult(EntryId 1)),
                createDefinition = (fun _ -> failwith "Should not be called"),
                createTranslation = (fun _ -> Task.FromResult(TranslationId 1)),
                createExamplesForDefinition = (fun _ -> failwith "Should not be called"),
                createExamplesForTranslation = (fun _ -> Task.FromResult(()))
            )

        let! result = create env (CollectionId 5) parameters

        Assert.Equal(Ok createdEntry, result)
    }

[<Fact>]
let ``creates entry with both definitions and translations``() =
    task {
        let now = DateTimeOffset.UtcNow

        let expectedDefinition =
            makeDefinition 10 "a greeting" DefinitionSource.Manual 0 []

        let expectedTranslation =
            makeTranslation 20 "hello in spanish" TranslationSource.Manual 0 []

        let createdEntry =
            makeEntryWithContent 1 10 "hello" now [ expectedDefinition ] [ expectedTranslation ]

        let definitions =
            [ makeDefinitionInput "a greeting" DefinitionSource.Manual [] ]

        let translations =
            [ makeTranslationInput "hello in spanish" TranslationSource.Manual [] ]

        let parameters =
            makeParameters 1 10 "hello" definitions translations false now

        let env =
            TestEnv(
                getEntryById = (fun _ -> Task.FromResult(Some createdEntry)),
                getEntryByTextAndVocabularyId = (fun _ -> Task.FromResult(None)),
                hasVocabularyAccessInCollection = (fun _ -> Task.FromResult(true)),
                createEntry = (fun _ -> Task.FromResult(EntryId 1)),
                createDefinition = (fun _ -> Task.FromResult(DefinitionId 10)),
                createTranslation = (fun _ -> Task.FromResult(TranslationId 20)),
                createExamplesForDefinition = (fun _ -> Task.FromResult(())),
                createExamplesForTranslation = (fun _ -> Task.FromResult(()))
            )

        let! result = create env (CollectionId 5) parameters

        Assert.Equal(Ok createdEntry, result)
    }

[<Fact>]
let ``trims whitespace from entry text``() =
    task {
        let now = DateTimeOffset.UtcNow
        let createdEntry = makeEntry 1 10 "hello"

        let definitions =
            [ makeDefinitionInput "a greeting" DefinitionSource.Manual [] ]

        let parameters =
            makeParameters 1 10 "  hello  " definitions [] false now

        let capturedText = ref ""

        let env =
            TestEnv(
                getEntryById = (fun _ -> Task.FromResult(Some createdEntry)),
                getEntryByTextAndVocabularyId = (fun _ -> Task.FromResult(None)),
                hasVocabularyAccessInCollection = (fun _ -> Task.FromResult(true)),
                createEntry =
                    (fun (_, text, _) ->
                        capturedText.Value <- text
                        Task.FromResult(EntryId 1)),
                createDefinition = (fun _ -> Task.FromResult(DefinitionId 10)),
                createTranslation = (fun _ -> failwith "Should not be called"),
                createExamplesForDefinition = (fun _ -> Task.FromResult(())),
                createExamplesForTranslation = (fun _ -> failwith "Should not be called")
            )

        let! result = create env (CollectionId 5) parameters

        Assert.Equal(Ok createdEntry, result)
        Assert.Equal("hello", capturedText.Value)
    }

[<Fact>]
let ``returns EntryTextRequired when entry text is whitespace only``() =
    task {
        let definitions =
            [ makeDefinitionInput "a greeting" DefinitionSource.Manual [] ]

        let parameters =
            makeParameters 1 10 "   " definitions [] false DateTimeOffset.UtcNow

        let env =
            TestEnv(
                getEntryById = (fun _ -> failwith "Should not be called"),
                getEntryByTextAndVocabularyId = (fun _ -> failwith "Should not be called"),
                hasVocabularyAccessInCollection = (fun _ -> failwith "Should not be called"),
                createEntry = (fun _ -> failwith "Should not be called"),
                createDefinition = (fun _ -> failwith "Should not be called"),
                createTranslation = (fun _ -> failwith "Should not be called"),
                createExamplesForDefinition = (fun _ -> failwith "Should not be called"),
                createExamplesForTranslation = (fun _ -> failwith "Should not be called")
            )

        let! result = create env (CollectionId 5) parameters

        Assert.Equal(Error EntryTextRequired, result)
    }

[<Fact>]
let ``returns TooManyExamples when definition has too many examples``() =
    task {
        let examples =
            [ 1 .. MaxExamplesPerItem + 1 ]
            |> List.map(fun i -> makeExampleInput $"example {i}" ExampleSource.Custom)

        let definitions =
            [ makeDefinitionInput "a greeting" DefinitionSource.Manual examples ]

        let parameters =
            makeParameters 1 10 "hello" definitions [] false DateTimeOffset.UtcNow

        let env =
            TestEnv(
                getEntryById = (fun _ -> failwith "Should not be called"),
                getEntryByTextAndVocabularyId = (fun _ -> Task.FromResult(None)),
                hasVocabularyAccessInCollection = (fun _ -> Task.FromResult(true)),
                createEntry = (fun _ -> failwith "Should not be called"),
                createDefinition = (fun _ -> failwith "Should not be called"),
                createTranslation = (fun _ -> failwith "Should not be called"),
                createExamplesForDefinition = (fun _ -> failwith "Should not be called"),
                createExamplesForTranslation = (fun _ -> failwith "Should not be called")
            )

        let! result = create env (CollectionId 5) parameters

        Assert.Equal(Error(TooManyExamples MaxExamplesPerItem), result)
    }

[<Fact>]
let ``returns TooManyExamples when translation has too many examples``() =
    task {
        let examples =
            [ 1 .. MaxExamplesPerItem + 1 ]
            |> List.map(fun i -> makeExampleInput $"example {i}" ExampleSource.Custom)

        let translations =
            [ makeTranslationInput "hola" TranslationSource.Manual examples ]

        let parameters =
            makeParameters 1 10 "hello" [] translations false DateTimeOffset.UtcNow

        let env =
            TestEnv(
                getEntryById = (fun _ -> failwith "Should not be called"),
                getEntryByTextAndVocabularyId = (fun _ -> Task.FromResult(None)),
                hasVocabularyAccessInCollection = (fun _ -> Task.FromResult(true)),
                createEntry = (fun _ -> failwith "Should not be called"),
                createDefinition = (fun _ -> failwith "Should not be called"),
                createTranslation = (fun _ -> failwith "Should not be called"),
                createExamplesForDefinition = (fun _ -> failwith "Should not be called"),
                createExamplesForTranslation = (fun _ -> failwith "Should not be called")
            )

        let! result = create env (CollectionId 5) parameters

        Assert.Equal(Error(TooManyExamples MaxExamplesPerItem), result)
    }

[<Fact>]
let ``proceeds when duplicate text match finds a stale record``() =
    task {
        let now = DateTimeOffset.UtcNow
        let staleEntry = makeEntry 99 10 "hello"
        let createdEntry = makeEntry 1 10 "hello"

        let definitions =
            [ makeDefinitionInput "a greeting" DefinitionSource.Manual [] ]

        let parameters =
            makeParameters 1 10 "hello" definitions [] false now

        let env =
            TestEnv(
                getEntryById =
                    (fun id ->
                        if id = EntryId 99 then
                            Task.FromResult(None)
                        else
                            Task.FromResult(Some createdEntry)),
                getEntryByTextAndVocabularyId = (fun _ -> Task.FromResult(Some staleEntry)),
                hasVocabularyAccessInCollection = (fun _ -> Task.FromResult(true)),
                createEntry = (fun _ -> Task.FromResult(EntryId 1)),
                createDefinition = (fun _ -> Task.FromResult(DefinitionId 10)),
                createTranslation = (fun _ -> failwith "Should not be called"),
                createExamplesForDefinition = (fun _ -> Task.FromResult(())),
                createExamplesForTranslation = (fun _ -> failwith "Should not be called")
            )

        let! result = create env (CollectionId 5) parameters

        Assert.Equal(Ok createdEntry, result)
    }

[<Fact>]
let ``returns EntryTextRequired when post-create entry fetch returns None``() =
    task {
        let now = DateTimeOffset.UtcNow

        let definitions =
            [ makeDefinitionInput "a greeting" DefinitionSource.Manual [] ]

        let parameters =
            makeParameters 1 10 "hello" definitions [] false now

        let env =
            TestEnv(
                getEntryById = (fun _ -> Task.FromResult(None)),
                getEntryByTextAndVocabularyId = (fun _ -> Task.FromResult(None)),
                hasVocabularyAccessInCollection = (fun _ -> Task.FromResult(true)),
                createEntry = (fun _ -> Task.FromResult(EntryId 1)),
                createDefinition = (fun _ -> Task.FromResult(DefinitionId 10)),
                createTranslation = (fun _ -> failwith "Should not be called"),
                createExamplesForDefinition = (fun _ -> Task.FromResult(())),
                createExamplesForTranslation = (fun _ -> failwith "Should not be called")
            )

        let! result = create env (CollectionId 5) parameters

        Assert.Equal(Error EntryTextRequired, result)
    }
