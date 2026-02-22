module Wordfolio.Api.Domain.Tests.Entries.EntryOperations.UpdateTests

open System
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Entries
open Wordfolio.Api.Domain.Entries.EntryOperations
open Wordfolio.Api.Domain.Entries.Helpers

type UpdateEntryCall =
    { EntryId: EntryId
      EntryText: string
      UpdatedAt: DateTimeOffset }

type CreateDefinitionCall =
    { EntryId: EntryId
      Text: string
      Source: DefinitionSource
      DisplayOrder: int }

type CreateTranslationCall =
    { EntryId: EntryId
      Text: string
      Source: TranslationSource
      DisplayOrder: int }

type CreateExamplesForDefinitionCall =
    { DefinitionId: DefinitionId
      Examples: ExampleInput list }

type CreateExamplesForTranslationCall =
    { TranslationId: TranslationId
      Examples: ExampleInput list }

type TestEnv
    (
        getEntryById: EntryId -> Task<Entry option>,
        hasVocabularyAccessInCollection: VocabularyId * CollectionId * UserId -> Task<bool>,
        updateEntry: EntryId * string * DateTimeOffset -> Task<unit>,
        clearEntryChildren: EntryId -> Task<unit>,
        createDefinition: EntryId * string * DefinitionSource * int -> Task<DefinitionId>,
        createTranslation: EntryId * string * TranslationSource * int -> Task<TranslationId>,
        createExamplesForDefinition: DefinitionId * ExampleInput list -> Task<unit>,
        createExamplesForTranslation: TranslationId * ExampleInput list -> Task<unit>
    ) =
    let getEntryByIdCalls =
        ResizeArray<EntryId>()

    let hasVocabularyAccessInCollectionCalls =
        ResizeArray<VocabularyId * CollectionId * UserId>()

    let updateEntryCalls =
        ResizeArray<UpdateEntryCall>()

    let clearEntryChildrenCalls =
        ResizeArray<EntryId>()

    let createDefinitionCalls =
        ResizeArray<CreateDefinitionCall>()

    let createTranslationCalls =
        ResizeArray<CreateTranslationCall>()

    let createExamplesForDefinitionCalls =
        ResizeArray<CreateExamplesForDefinitionCall>()

    let createExamplesForTranslationCalls =
        ResizeArray<CreateExamplesForTranslationCall>()

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
        member _.HasVocabularyAccessInCollection(vocabularyId, collectionId, userId) =
            hasVocabularyAccessInCollectionCalls.Add(vocabularyId, collectionId, userId)
            hasVocabularyAccessInCollection(vocabularyId, collectionId, userId)

    interface IUpdateEntry with
        member _.UpdateEntry(entryId, text, updatedAt) =
            updateEntryCalls.Add(
                { EntryId = entryId
                  EntryText = text
                  UpdatedAt = updatedAt }
            )

            updateEntry(entryId, text, updatedAt)

    interface IClearEntryChildren with
        member _.ClearEntryChildren(entryId) =
            clearEntryChildrenCalls.Add(entryId)
            clearEntryChildren entryId

    interface ICreateDefinition with
        member _.CreateDefinition(entryId, text, source, displayOrder) =
            createDefinitionCalls.Add(
                { EntryId = entryId
                  Text = text
                  Source = source
                  DisplayOrder = displayOrder }
            )

            createDefinition(entryId, text, source, displayOrder)

    interface ICreateTranslation with
        member _.CreateTranslation(entryId, text, source, displayOrder) =
            createTranslationCalls.Add(
                { EntryId = entryId
                  Text = text
                  Source = source
                  DisplayOrder = displayOrder }
            )

            createTranslation(entryId, text, source, displayOrder)

    interface ICreateExamplesForDefinition with
        member _.CreateExamplesForDefinition(definitionId, examples) =
            createExamplesForDefinitionCalls.Add(
                { DefinitionId = definitionId
                  Examples = examples }
            )

            createExamplesForDefinition(definitionId, examples)

    interface ICreateExamplesForTranslation with
        member _.CreateExamplesForTranslation(translationId, examples) =
            createExamplesForTranslationCalls.Add(
                { TranslationId = translationId
                  Examples = examples }
            )

            createExamplesForTranslation(translationId, examples)

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
                    (fun (_, text, updatedAt) ->
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
            update env (UserId 3) (CollectionId 5) (VocabularyId 10) (EntryId 1) "  new  " definitions translations now

        Assert.Equal(Ok updatedEntry, result)
        Assert.Equal<EntryId list>([ EntryId 1; EntryId 1 ], env.GetEntryByIdCalls)

        Assert.Equal<(VocabularyId * CollectionId * UserId) list>(
            [ VocabularyId 10, CollectionId 5, UserId 3 ],
            env.HasVocabularyAccessInCollectionCalls
        )

        Assert.Equal<EntryId list>([ EntryId 1 ], env.ClearEntryChildrenCalls)

        Assert.Equal<UpdateEntryCall list>(
            [ { EntryId = EntryId 1
                EntryText = "new"
                UpdatedAt = now } ],
            env.UpdateEntryCalls
        )

        Assert.Equal<CreateDefinitionCall list>(
            [ { CreateDefinitionCall.EntryId = EntryId 1
                Text = "definition"
                Source = DefinitionSource.Manual
                DisplayOrder = 0 } ],
            env.CreateDefinitionCalls
        )

        Assert.Equal<CreateTranslationCall list>(
            [ { EntryId = EntryId 1
                Text = "translation"
                Source = TranslationSource.Manual
                DisplayOrder = 0 } ],
            env.CreateTranslationCalls
        )

        Assert.Equal<CreateExamplesForDefinitionCall list>(
            [ { DefinitionId = DefinitionId 10
                Examples = [ makeExampleInput "example" ExampleSource.Custom ] } ],
            env.CreateExamplesForDefinitionCalls
        )

        Assert.Equal<CreateExamplesForTranslationCall list>(
            [ { TranslationId = TranslationId 20
                Examples = [ makeExampleInput "example" ExampleSource.Custom ] } ],
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
                (UserId 1)
                (CollectionId 5)
                (VocabularyId 10)
                (EntryId 1)
                "text"
                definitions
                []
                DateTimeOffset.UtcNow

        Assert.Equal(Error(VocabularyNotFoundOrAccessDenied(VocabularyId 10)), result)
        Assert.Empty(env.GetEntryByIdCalls)
        Assert.Empty(env.UpdateEntryCalls)

        Assert.Equal<(VocabularyId * CollectionId * UserId) list>(
            [ VocabularyId 10, CollectionId 5, UserId 1 ],
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
                (UserId 1)
                (CollectionId 5)
                (VocabularyId 10)
                (EntryId 99)
                "text"
                definitions
                []
                DateTimeOffset.UtcNow

        Assert.Equal(Error(EntryNotFound(EntryId 99)), result)
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
                (UserId 1)
                (CollectionId 5)
                (VocabularyId 10)
                (EntryId 1)
                "text"
                definitions
                []
                DateTimeOffset.UtcNow

        Assert.Equal(Error(EntryNotFound(EntryId 1)), result)
        Assert.Equal<EntryId list>([ EntryId 1 ], env.GetEntryByIdCalls)
        Assert.Empty(env.UpdateEntryCalls)
    }

[<Fact>]
let ``returns error when no definitions or translations``() =
    task {
        let entry =
            makeEntry 1 10 "text" [] [] DateTimeOffset.UtcNow None

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

        let! result =
            update env (UserId 1) (CollectionId 5) (VocabularyId 10) (EntryId 1) "text" [] [] DateTimeOffset.UtcNow

        Assert.Equal(Error NoDefinitionsOrTranslations, result)
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
            update env (UserId 1) (CollectionId 5) (VocabularyId 10) (EntryId 1) "" definitions [] DateTimeOffset.UtcNow

        Assert.Equal(Error EntryTextRequired, result)
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
                (UserId 1)
                (CollectionId 5)
                (VocabularyId 10)
                (EntryId 1)
                longText
                definitions
                []
                DateTimeOffset.UtcNow

        Assert.Equal(Error(EntryTextTooLong MaxEntryTextLength), result)
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
            update env (UserId 1) (CollectionId 5) (VocabularyId 10) (EntryId 1) "   " definitions [] DateTimeOffset.UtcNow

        Assert.Equal(Error EntryTextRequired, result)
        Assert.Empty(env.GetEntryByIdCalls)
    }

[<Fact>]
let ``returns error when definition example text is too long``() =
    task {
        let entry =
            makeEntry 1 10 "text" [] [] DateTimeOffset.UtcNow None

        let longExample =
            String.replicate (MaxExampleTextLength + 1) "a"

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
            [ makeDefinitionInput
                  "definition"
                  DefinitionSource.Manual
                  [ makeExampleInput longExample ExampleSource.Custom ] ]

        let! result =
            update env (UserId 2) (CollectionId 5) (VocabularyId 10) (EntryId 1) "text" definitions [] DateTimeOffset.UtcNow

        Assert.Equal(Error(ExampleTextTooLong MaxExampleTextLength), result)
        Assert.Empty(env.UpdateEntryCalls)
    }

[<Fact>]
let ``returns error when definition has too many examples``() =
    task {
        let entry =
            makeEntry 1 10 "text" [] [] DateTimeOffset.UtcNow None

        let examples =
            [ 1 .. MaxExamplesPerItem + 1 ]
            |> List.map(fun i -> makeExampleInput $"example {i}" ExampleSource.Custom)

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
            [ makeDefinitionInput "definition" DefinitionSource.Manual examples ]

        let! result =
            update env (UserId 2) (CollectionId 5) (VocabularyId 10) (EntryId 1) "text" definitions [] DateTimeOffset.UtcNow

        Assert.Equal(Error(TooManyExamples MaxExamplesPerItem), result)
        Assert.Empty(env.UpdateEntryCalls)
    }

[<Fact>]
let ``returns error when translation example text is too long``() =
    task {
        let entry =
            makeEntry 1 10 "text" [] [] DateTimeOffset.UtcNow None

        let longExample =
            String.replicate (MaxExampleTextLength + 1) "a"

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

        let translations =
            [ makeTranslationInput
                  "translation"
                  TranslationSource.Manual
                  [ makeExampleInput longExample ExampleSource.Custom ] ]

        let! result =
            update env (UserId 2) (CollectionId 5) (VocabularyId 10) (EntryId 1) "text" [] translations DateTimeOffset.UtcNow

        Assert.Equal(Error(ExampleTextTooLong MaxExampleTextLength), result)
        Assert.Empty(env.UpdateEntryCalls)
    }

[<Fact>]
let ``returns error when translation has too many examples``() =
    task {
        let entry =
            makeEntry 1 10 "text" [] [] DateTimeOffset.UtcNow None

        let examples =
            [ 1 .. MaxExamplesPerItem + 1 ]
            |> List.map(fun i -> makeExampleInput $"example {i}" ExampleSource.Custom)

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

        let translations =
            [ makeTranslationInput "translation" TranslationSource.Manual examples ]

        let! result =
            update env (UserId 2) (CollectionId 5) (VocabularyId 10) (EntryId 1) "text" [] translations DateTimeOffset.UtcNow

        Assert.Equal(Error(TooManyExamples MaxExamplesPerItem), result)
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
            update env (UserId 3) (CollectionId 5) (VocabularyId 10) (EntryId 1) "word" definitions [] now

        Assert.Equal(Ok updatedEntry, result)

        Assert.Equal<CreateDefinitionCall list>(
            [ { CreateDefinitionCall.EntryId = EntryId 1
                Text = "definition"
                Source = DefinitionSource.Manual
                DisplayOrder = 0 } ],
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
            update env (UserId 3) (CollectionId 5) (VocabularyId 10) (EntryId 1) "word" [] translations now

        Assert.Equal(Ok updatedEntry, result)

        Assert.Equal<CreateTranslationCall list>(
            [ { EntryId = EntryId 1
                Text = "translation"
                Source = TranslationSource.Manual
                DisplayOrder = 0 } ],
            env.CreateTranslationCalls
        )

        Assert.Empty(env.CreateDefinitionCalls)
    }
