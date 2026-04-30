module Wordfolio.Api.Domain.Exercises.Operations

open System
open System.Threading.Tasks

open Wordfolio.Api.Domain
open Wordfolio.Api.Domain.Exercises
open Wordfolio.Api.Domain.Exercises.Capabilities

module DomainCapabilities = Wordfolio.Api.Domain.Capabilities

let resolveEntrySelector env (userId: UserId) (selector: EntrySelector) : Task<Result<EntryId list, SelectorError>> =
    task {
        match selector with
        | VocabularyScope vocabularyId ->
            let! hasAccess =
                DomainCapabilities.hasVocabularyAccess
                    env
                    { VocabularyId = vocabularyId
                      UserId = userId }

            if not hasAccess then
                return Error(SelectorError.VocabularyNotOwnedByUser vocabularyId)
            else
                let! ids =
                    getEntryIdsByVocabularyId
                        env
                        { VocabularyId = vocabularyId
                          UserId = userId }

                return Ok ids

        | CollectionScope collectionId ->
            let! maybeCollection = DomainCapabilities.getCollectionById env collectionId

            match maybeCollection with
            | None -> return Error(SelectorError.CollectionNotOwnedByUser collectionId)
            | Some collection when collection.UserId <> userId ->
                return Error(SelectorError.CollectionNotOwnedByUser collectionId)
            | Some _ ->
                let! ids =
                    getEntryIdsByCollectionId
                        env
                        { CollectionId = collectionId
                          UserId = userId }

                return Ok ids

        | ExplicitEntries requestedIds ->
            if requestedIds.IsEmpty then
                return Ok []
            else
                let! ownedIds =
                    getOwnedEntryIds
                        env
                        { EntryIds = requestedIds
                          UserId = userId }

                let ownedSet = ownedIds |> Set.ofList

                let notOwned =
                    requestedIds
                    |> List.filter(fun id -> not(ownedSet.Contains id))

                if not notOwned.IsEmpty then
                    return Error(SelectorError.EntryNotOwnedByUser notOwned)
                else
                    return Ok requestedIds

        | WorstKnown(AllUserEntries, count) ->
            let! allIds = getEntryIdsByUserId env userId

            let! worstIds =
                getWorstKnownEntryIds
                    env
                    { UserId = userId
                      ScopedEntryIds = allIds
                      Count = count
                      KnowledgeWindowSize = Limits.KnowledgeWindowSize }

            return Ok worstIds

        | WorstKnown(WithinVocabulary vocabularyId, count) ->
            let! hasAccess =
                DomainCapabilities.hasVocabularyAccess
                    env
                    { VocabularyId = vocabularyId
                      UserId = userId }

            if not hasAccess then
                return Error(SelectorError.VocabularyNotOwnedByUser vocabularyId)
            else
                let! scopedIds =
                    getEntryIdsByVocabularyId
                        env
                        { VocabularyId = vocabularyId
                          UserId = userId }

                let! worstIds =
                    getWorstKnownEntryIds
                        env
                        { UserId = userId
                          ScopedEntryIds = scopedIds
                          Count = count
                          KnowledgeWindowSize = Limits.KnowledgeWindowSize }

                return Ok worstIds

        | WorstKnown(WithinCollection collectionId, count) ->
            let! maybeCollection = DomainCapabilities.getCollectionById env collectionId

            match maybeCollection with
            | None -> return Error(SelectorError.CollectionNotOwnedByUser collectionId)
            | Some collection when collection.UserId <> userId ->
                return Error(SelectorError.CollectionNotOwnedByUser collectionId)
            | Some _ ->
                let! scopedIds =
                    getEntryIdsByCollectionId
                        env
                        { CollectionId = collectionId
                          UserId = userId }

                let! worstIds =
                    getWorstKnownEntryIds
                        env
                        { UserId = userId
                          ScopedEntryIds = scopedIds
                          Count = count
                          KnowledgeWindowSize = Limits.KnowledgeWindowSize }

                return Ok worstIds
    }

let createSession env (parameters: CreateSessionParameters) : Task<Result<SessionBundle, CreateSessionError>> =
    task {
        let! selectorResult = Capabilities.resolveEntrySelector env parameters.UserId parameters.Selector

        match selectorResult with
        | Error selectorError -> return Error(CreateSessionError.SelectorFailed selectorError)
        | Ok [] -> return Error CreateSessionError.NoEntriesResolved
        | Ok resolvedEntryIds ->
            let cappedEntryIds =
                resolvedEntryIds
                |> List.truncate Limits.MaxSessionEntries

            let! allEntries = getEntriesByIds env cappedEntryIds

            let orderedEntries =
                cappedEntryIds
                |> List.choose(fun entryId ->
                    allEntries
                    |> List.tryFind(fun entry -> entry.Id = entryId))
                |> List.filter(fun entry -> not entry.Translations.IsEmpty)

            match orderedEntries with
            | [] -> return Error CreateSessionError.NoEntriesResolved
            | _ ->

                let entries =
                    orderedEntries
                    |> List.mapi(fun index entry ->
                        let prompt =
                            Dispatch.generatePrompt parameters.ExerciseType entry

                        (entry.Id, index, prompt.PromptData, prompt.PromptSchemaVersion))

                let sessionData: CreateExerciseSessionData =
                    { UserId = parameters.UserId
                      ExerciseType = parameters.ExerciseType
                      Entries = entries
                      CreatedAt = parameters.CreatedAt }

                let! sessionId = createExerciseSession env sessionData

                let bundleEntries =
                    entries
                    |> List.map(fun (entryId, displayOrder, promptData, _schemaVersion) ->
                        { EntryId = entryId
                          DisplayOrder = displayOrder
                          PromptData = promptData
                          Attempt = None }
                        : SessionBundleEntry)

                return
                    Ok
                        { SessionId = sessionId
                          ExerciseType = parameters.ExerciseType
                          Entries = bundleEntries }
    }

let getSession env (userId: UserId) (sessionId: ExerciseSessionId) : Task<Result<SessionBundle, GetSessionError>> =
    task {
        let! maybeSession = getExerciseSession env sessionId

        match maybeSession with
        | None -> return Error GetSessionError.NotFound
        | Some session when session.UserId <> userId -> return Error GetSessionError.NotFound
        | Some session ->
            let! sessionEntries = getExerciseSessionEntries env sessionId
            let! attempts = getAttemptsBySession env sessionId

            let attemptByEntryId =
                attempts
                |> List.map(fun a -> (a.EntryId, a))
                |> Map.ofList

            let bundleEntries =
                sessionEntries
                |> List.map(fun entry ->
                    { EntryId = entry.EntryId
                      DisplayOrder = entry.DisplayOrder
                      PromptData = entry.PromptData
                      Attempt =
                        attemptByEntryId
                        |> Map.tryFind entry.EntryId
                        |> Option.map(fun a ->
                            { RawAnswer = a.RawAnswer
                              IsCorrect = a.IsCorrect
                              AttemptedAt = a.AttemptedAt }
                            : AttemptSummary) }
                    : SessionBundleEntry)

            return
                Ok
                    { SessionId = session.Id
                      ExerciseType = session.ExerciseType
                      Entries = bundleEntries }
    }

let submitAttempt
    env
    (userId: UserId)
    (sessionId: ExerciseSessionId)
    (entryId: EntryId)
    (rawAnswer: RawAnswer)
    (attemptedAt: DateTimeOffset)
    : Task<Result<SubmitAttemptResult, SubmitAttemptError>> =
    task {
        let! maybeSession = getExerciseSession env sessionId

        match maybeSession with
        | None -> return Error SubmitAttemptError.SessionNotFound
        | Some session when session.UserId <> userId -> return Error SubmitAttemptError.SessionNotFound
        | Some session ->
            let! maybeSessionEntry = getExerciseSessionEntry env sessionId entryId

            match maybeSessionEntry with
            | None -> return Error(SubmitAttemptError.EntryNotInSession entryId)
            | Some sessionEntry ->
                let evaluateResult =
                    Dispatch.evaluate
                        session.ExerciseType
                        sessionEntry.PromptSchemaVersion
                        sessionEntry.PromptData
                        rawAnswer

                match evaluateResult with
                | Error evaluateError -> return Error(SubmitAttemptError.EvaluateError evaluateError)
                | Ok isCorrect ->
                    let commitData: CommitAttemptData =
                        { SessionId = sessionId
                          EntryId = entryId
                          UserId = userId
                          ExerciseType = session.ExerciseType
                          PromptData = sessionEntry.PromptData
                          PromptSchemaVersion = sessionEntry.PromptSchemaVersion
                          RawAnswer = rawAnswer
                          IsCorrect = isCorrect
                          AttemptedAt = attemptedAt }

                    let! commitResult = commitAttempt env commitData

                    return
                        match commitResult with
                        | Inserted result -> Ok(Inserted result)
                        | IdempotentReplay result -> Ok(IdempotentReplay result)
                        | ConflictingReplay -> Error SubmitAttemptError.ConflictingAttempt
    }
