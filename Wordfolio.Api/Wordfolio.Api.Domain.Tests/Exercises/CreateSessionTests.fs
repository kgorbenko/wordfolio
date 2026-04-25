module Wordfolio.Api.Domain.Tests.Exercises.CreateSessionTests

open System
open System.Threading.Tasks

open Xunit

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Exercises
open Wordfolio.Api.Domain.Exercises.Operations

type TestEnv
    (
        resolveEntrySelector: UserId -> EntrySelector -> Task<Result<EntryId list, SelectorError>>,
        getEntriesByIds: EntryId list -> Task<Entry list>,
        createExerciseSession: CreateExerciseSessionData -> Task<SessionBundle>
    ) =
    let resolveEntrySelectorCalls =
        ResizeArray<UserId * EntrySelector>()

    let getEntriesByIdsCalls =
        ResizeArray<EntryId list>()

    let createExerciseSessionCalls =
        ResizeArray<CreateExerciseSessionData>()

    member _.ResolveEntrySelectorCalls =
        resolveEntrySelectorCalls |> Seq.toList

    member _.GetEntriesByIdsCalls =
        getEntriesByIdsCalls |> Seq.toList

    member _.CreateExerciseSessionCalls =
        createExerciseSessionCalls |> Seq.toList

    interface IResolveEntrySelector with
        member _.ResolveEntrySelector userId selector =
            resolveEntrySelectorCalls.Add((userId, selector))
            resolveEntrySelector userId selector

    interface IGetEntriesByIds with
        member _.GetEntriesByIds entryIds =
            getEntriesByIdsCalls.Add(entryIds)
            getEntriesByIds entryIds

    interface ICreateExerciseSession with
        member _.CreateExerciseSession data =
            createExerciseSessionCalls.Add(data)
            createExerciseSession data

let makeTranslation id text =
    { Id = TranslationId id
      TranslationText = text
      Source = TranslationSource.Manual
      DisplayOrder = 0
      Examples = [] }

let makeEntry id text translations =
    let timestamp =
        DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)

    { Id = EntryId id
      VocabularyId = VocabularyId 1
      EntryText = text
      CreatedAt = timestamp
      UpdatedAt = timestamp
      Definitions = []
      Translations = translations }

let makeBundle sessionId exerciseType =
    { SessionId = ExerciseSessionId sessionId
      ExerciseType = exerciseType
      Entries = [] }

[<Fact>]
let ``returns SelectorFailed when selector errors``() =
    task {
        let userId = UserId 1

        let selector =
            EntrySelector.VocabularyScope(VocabularyId 10)

        let selectorError =
            SelectorError.VocabularyNotOwnedByUser(VocabularyId 10)

        let env =
            TestEnv(
                resolveEntrySelector = (fun _ _ -> Task.FromResult(Error selectorError)),
                getEntriesByIds = (fun _ -> failwith "Should not be called"),
                createExerciseSession = (fun _ -> failwith "Should not be called")
            )

        let parameters: CreateSessionParameters =
            { UserId = userId
              ExerciseType = ExerciseType.Translation
              Selector = selector
              CreatedAt = DateTimeOffset(2025, 6, 1, 12, 0, 0, TimeSpan.Zero) }

        let! result = createSession env parameters

        Assert.Equal(Error(CreateSessionError.SelectorFailed selectorError), result)
        Assert.Equal<(UserId * EntrySelector) list>([ (userId, selector) ], env.ResolveEntrySelectorCalls)
        Assert.Empty(env.GetEntriesByIdsCalls)
        Assert.Empty(env.CreateExerciseSessionCalls)
    }

[<Fact>]
let ``returns NoEntriesResolved when selector resolves empty list``() =
    task {
        let userId = UserId 1

        let selector =
            EntrySelector.VocabularyScope(VocabularyId 10)

        let env =
            TestEnv(
                resolveEntrySelector = (fun _ _ -> Task.FromResult(Ok [])),
                getEntriesByIds = (fun _ -> failwith "Should not be called"),
                createExerciseSession = (fun _ -> failwith "Should not be called")
            )

        let parameters: CreateSessionParameters =
            { UserId = userId
              ExerciseType = ExerciseType.Translation
              Selector = selector
              CreatedAt = DateTimeOffset(2025, 6, 1, 12, 0, 0, TimeSpan.Zero) }

        let! result = createSession env parameters

        Assert.Equal(Error CreateSessionError.NoEntriesResolved, result)
        Assert.Equal<(UserId * EntrySelector) list>([ (userId, selector) ], env.ResolveEntrySelectorCalls)
        Assert.Empty(env.GetEntriesByIdsCalls)
        Assert.Empty(env.CreateExerciseSessionCalls)
    }

[<Fact>]
let ``success path preserves selector order, passes prompt data and parameters.CreatedAt``() =
    task {
        let userId = UserId 1

        let selector =
            EntrySelector.ExplicitEntries([ EntryId 2; EntryId 1 ])

        let createdAt =
            DateTimeOffset(2025, 6, 1, 12, 0, 0, TimeSpan.Zero)

        let entry1 =
            makeEntry 1 "word1" [ makeTranslation 1 "trans1" ]

        let entry2 =
            makeEntry 2 "word2" [ makeTranslation 2 "trans2" ]

        let expectedBundle =
            makeBundle 42 ExerciseType.Translation

        let env =
            TestEnv(
                resolveEntrySelector = (fun _ _ -> Task.FromResult(Ok [ EntryId 2; EntryId 1 ])),
                getEntriesByIds = (fun _ -> Task.FromResult([ entry1; entry2 ])),
                createExerciseSession = (fun _ -> Task.FromResult(expectedBundle))
            )

        let parameters: CreateSessionParameters =
            { UserId = userId
              ExerciseType = ExerciseType.Translation
              Selector = selector
              CreatedAt = createdAt }

        let! result = createSession env parameters

        Assert.Equal(Ok expectedBundle, result)

        let prompt2 =
            Dispatch.generatePrompt ExerciseType.Translation entry2

        let prompt1 =
            Dispatch.generatePrompt ExerciseType.Translation entry1

        let expectedData: CreateExerciseSessionData =
            { UserId = userId
              ExerciseType = ExerciseType.Translation
              Entries =
                [ (EntryId 2, 0, prompt2.PromptData, prompt2.PromptSchemaVersion)
                  (EntryId 1, 1, prompt1.PromptData, prompt1.PromptSchemaVersion) ]
              CreatedAt = createdAt }

        Assert.Equal<(UserId * EntrySelector) list>([ (userId, selector) ], env.ResolveEntrySelectorCalls)
        Assert.Equal<EntryId list list>([ [ EntryId 2; EntryId 1 ] ], env.GetEntriesByIdsCalls)
        Assert.Equal<CreateExerciseSessionData list>([ expectedData ], env.CreateExerciseSessionCalls)
    }

[<Fact>]
let ``success path truncates resolved entries to MaxSessionEntries``() =
    task {
        let userId = UserId 1

        let selector =
            EntrySelector.VocabularyScope(VocabularyId 10)

        let createdAt =
            DateTimeOffset(2025, 6, 1, 12, 0, 0, TimeSpan.Zero)

        let resolvedIds =
            [ 1..11 ] |> List.map EntryId

        let entries =
            resolvedIds
            |> List.map(fun id ->
                makeEntry (EntryId.value id) "word" [ makeTranslation (EntryId.value id) "translation" ])

        let expectedBundle =
            makeBundle 42 ExerciseType.Translation

        let env =
            TestEnv(
                resolveEntrySelector = (fun _ _ -> Task.FromResult(Ok resolvedIds)),
                getEntriesByIds =
                    (fun ids ->
                        Task.FromResult(
                            entries
                            |> List.filter(fun e -> List.contains e.Id ids)
                        )),
                createExerciseSession = (fun _ -> Task.FromResult(expectedBundle))
            )

        let parameters: CreateSessionParameters =
            { UserId = userId
              ExerciseType = ExerciseType.Translation
              Selector = selector
              CreatedAt = createdAt }

        let! result = createSession env parameters

        Assert.Equal(Ok expectedBundle, result)
        Assert.Equal<(UserId * EntrySelector) list>([ (userId, selector) ], env.ResolveEntrySelectorCalls)
        Assert.Equal<EntryId list list>([ [ 1..10 ] |> List.map EntryId ], env.GetEntriesByIdsCalls)

        let capturedData =
            env.CreateExerciseSessionCalls
            |> List.exactlyOne

        Assert.Equal(10, capturedData.Entries |> List.length)
    }
