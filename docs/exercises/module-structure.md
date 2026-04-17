# Exercise Feature – Module Structure

The exercise feature follows the same layered architecture as the rest of the application.

```
HTTP Request
     ↓
  Handlers           (Wordfolio.Api/Api/Exercises/)
     ↓
  Domain Operations  (Wordfolio.Api.Domain/Exercises/Operations.fs)
     ↓
  Capabilities       (Wordfolio.Api.Domain/Exercises/Capabilities.fs)
     ↓
  AppEnv             (Wordfolio.Api/Infrastructure/Environment.fs)
     ↓
  Data Access        (Wordfolio.Api.DataAccess/ExerciseSessions.fs, ExerciseAttempts.fs, EntryKnowledge.fs)
     ↓
  PostgreSQL
```

---

## Wordfolio.Api.Domain / Exercises

### Types.fs

Domain ID wrappers and core record types.

```fsharp
[<Struct>] type ExerciseSessionId = | ExerciseSessionId of int
[<Struct>] type ExerciseAttemptId = | ExerciseAttemptId of int

type ExerciseType =
    | MultipleChoice
    | Translation
    // Maps to int16 (PostgreSQL smallint) at the persistence boundary:
    //   MultipleChoice → 0   Translation → 1

type ExerciseSession =
    { Id: ExerciseSessionId
      UserId: UserId
      ExerciseType: ExerciseType
      CreatedAt: DateTimeOffset }

type ExerciseSessionEntry =
    { Id: int
      SessionId: ExerciseSessionId
      EntryId: EntryId
      DisplayOrder: int
      PromptData: string }   // serialised JSON; included in session bundle

type SessionBundleEntry =
    { EntryId: EntryId
      DisplayOrder: int
      PromptData: string }   // includes client-visible answer/checking data

type SessionBundle =
    { SessionId: ExerciseSessionId
      ExerciseType: ExerciseType
      Entries: SessionBundleEntry list }

type WorstKnownScope =
    | AllUserEntries
    | WithinVocabulary of VocabularyId
    | WithinCollection of CollectionId

type EntrySelector =
    | VocabularyScope of VocabularyId
    | CollectionScope of CollectionId
    | WorstKnown      of scope: WorstKnownScope * count: int
    | ExplicitEntries of EntryId list

type CreateSessionParameters =
    { UserId: UserId
      ExerciseType: ExerciseType
      Selector: EntrySelector }

type SubmitAttemptParameters =
    { SessionId: ExerciseSessionId
      EntryId: EntryId
      UserId: UserId
      RawAnswer: string
      IsCorrect: bool        // client-provided; persisted as-is
      AttemptedAt: DateTimeOffset }

type SubmitAttemptResult =
    | Inserted of ExerciseAttemptId
    | IdempotentReplay
    | ConflictingReplay

type EntryKnowledge =
    { UserId: UserId
      EntryId: EntryId
      TotalAttempts: int
      CorrectAttempts: int
      LastAttemptedAt: DateTimeOffset }
```

### Capabilities.fs

Interfaces that the domain depends on; implemented by `AppEnv`.

```fsharp
type IResolveEntrySelector =
    abstract ResolveEntrySelector: UserId -> EntrySelector -> Task<Result<EntryId list, SelectorError>>

type ICreateExerciseSession =
    abstract CreateExerciseSession: CreateSessionParameters -> entryIds: EntryId list -> promptDataList: string list -> DateTimeOffset -> Task<SessionBundle>

type IGetExerciseSession =
    abstract GetExerciseSession: ExerciseSessionId -> Task<ExerciseSession option>

type IGetSessionBundle =
    abstract GetSessionBundle: UserId -> ExerciseSessionId -> Task<SessionBundle option>

type ICommitAttempt =
    abstract CommitAttempt: SubmitAttemptParameters -> Task<SubmitAttemptResult>

type IGetEntryKnowledge =
    abstract GetEntryKnowledge: UserId -> EntryId -> Task<EntryKnowledge option>
```

`SelectorError` covers ownership failures (e.g. `VocabularyNotOwnedByUser`, `CollectionNotOwnedByUser`, `EntryNotOwnedByUser`).

### Operations.fs

Pure business logic. Delegates all I/O to capabilities.

```fsharp
val createSession  : env -> CreateSessionParameters -> Task<Result<SessionBundle, CreateSessionError>>
val getSession     : env -> UserId -> ExerciseSessionId -> Task<Result<SessionBundle, GetSessionError>>
val submitAttempt  : env -> UserId -> ExerciseSessionId -> EntryId -> rawAnswer: string -> isCorrect: bool -> DateTimeOffset -> Task<Result<SubmitAttemptOutcome, SubmitAttemptError>>
```

`createSession` resolves the selector (including ownership validation), validates the resulting entry list is non-empty, generates a prompt for each entry via `Dispatch.generatePrompt`, then persists the session and its entries (including `PromptData`) in a single transaction. Returns the full `SessionBundle`.

`getSession` verifies session ownership, loads all `ExerciseSessionEntries` for the session (ordered by `DisplayOrder`), and assembles them into a `SessionBundle`. Used by the resume endpoint.

`submitAttempt` loads the session and verifies ownership. Verifies the entry is in the session. Accepts `isCorrect` from the caller (client-provided); does **not** call `Dispatch.evaluate`. Calls `ICommitAttempt` to persist the attempt and update knowledge counters.

---

## Exercise-type dispatch

Exercise-type-specific logic lives in a DU-based dispatch module, **not** in an `IExerciseType` registry. There is no interface lookup or dynamic dispatch by string key.

### ExerciseTypes.fs

```fsharp
module MultipleChoice =
    val generatePrompt : Entry -> EntryKnowledge option -> string   // returns serialised JSON PromptData; includes correct answer for client-side evaluation

module Translation =
    val generatePrompt : Entry -> EntryKnowledge option -> string
```

### Dispatch.fs

```fsharp
module Dispatch =
    val generatePrompt : ExerciseType -> Entry -> EntryKnowledge option -> string
```

`generatePrompt` pattern-matches on the `ExerciseType` DU and delegates to the appropriate module. The generated `PromptData` includes all client-visible answer/checking data so the client can evaluate answers locally.

`Operations.createSession` calls `Dispatch.generatePrompt` for each entry before writing `ExerciseSessionEntries`. `Operations.submitAttempt` does not call `Dispatch.evaluate`; correctness is accepted from the client.

---

## Wordfolio.Api.DataAccess

### ExerciseSessions.fs

Data access for `ExerciseSessions` and `ExerciseSessionEntries`.

```fsharp
type ExerciseSessionRow       = { ... }   // CLIMutable record mirroring DB row; no Status/CompletedAt
type ExerciseSessionEntryRow  = { ... }   // includes PromptData: string

val createSessionWithEntriesAsync
    : sessionParams -> entryIds -> promptDataList -> IDbConnection -> IDbTransaction -> CancellationToken
    -> Task<SessionBundle>

val getSessionAsync
    : sessionId: int -> IDbConnection -> IDbTransaction -> CancellationToken
    -> Task<ExerciseSessionRow option>

val getSessionEntriesAsync
    : sessionId: int -> IDbConnection -> IDbTransaction -> CancellationToken
    -> Task<ExerciseSessionEntryRow list>
```

### ExerciseAttempts.fs

```fsharp
type ExerciseAttemptRow = { ... }  // includes RawAnswer: string

val commitAttemptAsync
    : params -> IDbConnection -> IDbTransaction -> CancellationToken
    -> Task<CommitResult>
// CommitResult = Inserted of int | IdempotentReplay | ConflictingReplay
```

`commitAttemptAsync` uses `INSERT ... ON CONFLICT (SessionId, EntryId) DO NOTHING RETURNING Id`. If the insert returns no `Id`, it re-reads the existing row and compares `RawAnswer` to determine whether the replay is idempotent or conflicting.

### EntryKnowledge.fs

```fsharp
type EntryKnowledgeRow = { ... }

val upsertEntryKnowledgeAsync
    : userId: int -> entryId: int -> isCorrect: bool -> attemptedAt: DateTimeOffset
    -> IDbConnection -> IDbTransaction -> CancellationToken
    -> Task<unit>

val getEntryKnowledgeAsync
    : userId: int -> entryId: int -> IDbConnection -> IDbTransaction -> CancellationToken
    -> Task<EntryKnowledgeRow option>

val getWorstKnownEntriesAsync
    : userId: int -> scope: WorstKnownScopeRow -> count: int -> IDbConnection -> IDbTransaction -> CancellationToken
    -> Task<int list>  // EntryId list
```

`getWorstKnownEntriesAsync` uses a LEFT JOIN between the scoped entry set and `EntryKnowledge` (filtered to `UserId`), coalescing null hit rates to `0.0` so unseen/cold entries are included and rank first. Order: `COALESCE(hit_rate, 0.0) ASC`, `LastAttemptedAt ASC NULLS FIRST`, `EntryId ASC`.

`upsertEntryKnowledgeAsync` uses `INSERT ... ON CONFLICT (UserId, EntryId) DO UPDATE` so it is safe to call in any order.

---

## Wordfolio.Api / Infrastructure / Environment.fs

`AppEnv` grows new interface implementations following the existing pattern:

```fsharp
interface IResolveEntrySelector with
    member _.ResolveEntrySelector userId selector =
        // validate ownership, branch on selector DU, call DataAccess query functions

interface ICreateExerciseSession with
    member _.CreateExerciseSession parameters entries promptDataList now =
        // call ExerciseSessions.createSessionWithEntriesAsync (passes PromptData per entry)
        // return SessionBundle (wraps returned int as SessionId, assembles entries)

interface IGetSessionBundle with
    member _.GetSessionBundle userId sessionId =
        // call ExerciseSessions.getSessionAsync → verify ownership
        // call ExerciseSessions.getSessionEntriesAsync → map to SessionBundleEntry list
        // return SessionBundle option

interface ICommitAttempt with
    member _.CommitAttempt parameters =
        // call ExerciseAttempts.commitAttemptAsync (passes RawAnswer)
        // map CommitResult to domain SubmitAttemptResult
```

Each method is a thin mapping layer. No business logic or orchestration in `AppEnv`.

---

## Wordfolio.Api / Api / Exercises/

```
Api/Exercises/
    CreateSessionHandler.fs   POST /exercises/sessions
    GetSessionHandler.fs      GET  /exercises/sessions/{id}
    SubmitAttemptHandler.fs   POST /exercises/sessions/{id}/entries/{entryId}/attempts
```

Handlers parse the HTTP request, call the relevant `Operations` function, and map domain results to HTTP responses. **No business logic, no evaluation, and no prompt generation in handlers.** All orchestration (validate ownership, verify session membership, commit) lives in `Operations`.

---

## Schema.fs additions

```fsharp
module ExerciseSessionsTable =
    [<Literal>] let Name = "ExerciseSessions"
    [<Literal>] let IdColumn = "Id"
    [<Literal>] let UserIdColumn = "UserId"
    [<Literal>] let ExerciseTypeColumn = "ExerciseType"
    [<Literal>] let CreatedAtColumn = "CreatedAt"
    // Note: no StatusColumn, no CompletedAtColumn

module ExerciseSessionEntriesTable =
    // includes PromptDataColumn = "PromptData"
    ...

module ExerciseAttemptsTable =
    // includes RawAnswerColumn = "RawAnswer"
    ...

module EntryKnowledgeTable = ...
```
