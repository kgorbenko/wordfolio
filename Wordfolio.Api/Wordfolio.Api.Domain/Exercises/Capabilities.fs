namespace Wordfolio.Api.Domain.Exercises

open System
open System.Threading.Tasks

open Wordfolio.Api.Domain

type CreateExerciseSessionEntryData =
    { EntryId: EntryId
      DisplayOrder: int
      PromptData: PromptData
      PromptSchemaVersion: int16 }

type CreateExerciseSessionData =
    { UserId: UserId
      ExerciseType: ExerciseType
      Entries: CreateExerciseSessionEntryData list
      CreatedAt: DateTimeOffset }

type CommitAttemptData =
    { SessionId: ExerciseSessionId
      EntryId: EntryId
      UserId: UserId
      ExerciseType: ExerciseType
      PromptData: PromptData
      PromptSchemaVersion: int16
      RawAnswer: RawAnswer
      IsCorrect: bool
      AttemptedAt: DateTimeOffset }

type GetEntryIdsByVocabularyIdData =
    { VocabularyId: VocabularyId
      UserId: UserId }

type GetEntryIdsByCollectionIdData =
    { CollectionId: CollectionId
      UserId: UserId }

type GetOwnedEntryIdsData =
    { EntryIds: EntryId list
      UserId: UserId }

type GetWorstKnownEntryIdsData =
    { UserId: UserId
      ScopedEntryIds: EntryId list
      Count: int
      KnowledgeWindowSize: int }

type IResolveEntrySelector =
    abstract ResolveEntrySelector: UserId -> EntrySelector -> Task<Result<EntryId list, SelectorError>>

type IGetEntriesByIds =
    abstract GetEntriesByIds: EntryId list -> Task<Entry list>

type ICreateExerciseSession =
    abstract CreateExerciseSession: CreateExerciseSessionData -> Task<ExerciseSessionId>

type IGetExerciseSession =
    abstract GetExerciseSession: ExerciseSessionId -> Task<ExerciseSession option>

type IGetExerciseSessionEntry =
    abstract GetExerciseSessionEntry: ExerciseSessionId -> EntryId -> Task<ExerciseSessionEntry option>

type IGetExerciseSessionEntries =
    abstract GetExerciseSessionEntries: ExerciseSessionId -> Task<ExerciseSessionEntry list>

type IGetAttemptsBySession =
    abstract GetAttemptsBySession: ExerciseSessionId -> Task<SessionAttempt list>

type IGetEntryIdsByVocabularyId =
    abstract GetEntryIdsByVocabularyId: GetEntryIdsByVocabularyIdData -> Task<EntryId list>

type IGetEntryIdsByCollectionId =
    abstract GetEntryIdsByCollectionId: GetEntryIdsByCollectionIdData -> Task<EntryId list>

type IGetOwnedEntryIds =
    abstract GetOwnedEntryIds: GetOwnedEntryIdsData -> Task<EntryId list>

type IGetEntryIdsByUserId =
    abstract GetEntryIdsByUserId: UserId -> Task<EntryId list>

type IGetWorstKnownEntryIds =
    abstract GetWorstKnownEntryIds: GetWorstKnownEntryIdsData -> Task<EntryId list>

type ICommitAttempt =
    abstract CommitAttempt: CommitAttemptData -> Task<SubmitAttemptResult>

module Capabilities =
    let resolveEntrySelector (env: #IResolveEntrySelector) userId selector =
        env.ResolveEntrySelector userId selector

    let getEntriesByIds (env: #IGetEntriesByIds) entryIds = env.GetEntriesByIds entryIds

    let createExerciseSession (env: #ICreateExerciseSession) data = env.CreateExerciseSession data

    let getExerciseSession (env: #IGetExerciseSession) sessionId = env.GetExerciseSession sessionId

    let getExerciseSessionEntry (env: #IGetExerciseSessionEntry) sessionId entryId =
        env.GetExerciseSessionEntry sessionId entryId

    let getExerciseSessionEntries (env: #IGetExerciseSessionEntries) sessionId = env.GetExerciseSessionEntries sessionId

    let getAttemptsBySession (env: #IGetAttemptsBySession) sessionId = env.GetAttemptsBySession sessionId

    let getEntryIdsByVocabularyId (env: #IGetEntryIdsByVocabularyId) data = env.GetEntryIdsByVocabularyId data

    let getEntryIdsByCollectionId (env: #IGetEntryIdsByCollectionId) data = env.GetEntryIdsByCollectionId data

    let getOwnedEntryIds (env: #IGetOwnedEntryIds) data = env.GetOwnedEntryIds data

    let getEntryIdsByUserId (env: #IGetEntryIdsByUserId) userId = env.GetEntryIdsByUserId userId

    let getWorstKnownEntryIds (env: #IGetWorstKnownEntryIds) data = env.GetWorstKnownEntryIds data

    let commitAttempt (env: #ICommitAttempt) data = env.CommitAttempt data
