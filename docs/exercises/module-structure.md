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
  Data Access        (Wordfolio.Api.DataAccess/ExerciseSessions.fs, ExerciseAttempts.fs)
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

// Opaque domain wrappers for prompt payload and raw answer.
[<Struct>] type PromptData    = | PromptData    of string
[<Struct>] type RawAnswer     = | RawAnswer     of string
// PromptSchemaVersion is kept as plain int16 at the persistence boundary; no wrapper.

// Named constant for the knowledge window.
[<Literal>] let KnowledgeWindowSize = 10

// Maximum number of entries per session.
[<Literal>] let MaxSessionEntries = 10

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
    { SessionId: ExerciseSessionId
      EntryId: EntryId
      DisplayOrder: int
      PromptData: PromptData            // serialised JSON; included in session bundle
      PromptSchemaVersion: int16 }

// Per-entry attempt metadata included in the resume bundle.
type AttemptSummary =
    { RawAnswer: RawAnswer
      IsCorrect: bool
      AttemptedAt: DateTimeOffset }

type SessionBundleEntry =
    { EntryId: EntryId
      DisplayOrder: int
      PromptData: PromptData            // includes client-visible answer/checking data
      Attempt: AttemptSummary option }  // None if not yet answered; Some on resume

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
      PromptData: PromptData            // copied from ExerciseSessionEntry at submit time
      PromptSchemaVersion: int16        // copied from ExerciseSessionEntry at submit time
      RawAnswer: RawAnswer
      IsCorrect: bool                   // server-evaluated via Dispatch.evaluate
      AttemptedAt: DateTimeOffset }     // server-generated

type AttemptInserted =
    { AttemptId: ExerciseAttemptId
      IsCorrect: bool }

type AttemptAlreadyRecorded =
    { IsCorrect: bool }

type EvaluateError =
    | UnsupportedPromptSchemaVersion
    | MalformedPromptData

type GeneratedPrompt =
    { PromptData: PromptData
      PromptSchemaVersion: int16 }

type SubmitAttemptResult =
    | Inserted of AttemptInserted
    | IdempotentReplay of AttemptAlreadyRecorded
    | ConflictingReplay
```

### Capabilities.fs

Interfaces that the domain depends on; implemented by `AppEnv`.

Input records are used for capability methods to keep parameter lists manageable.

```fsharp
// Input record for ICreateExerciseSession.
type CreateExerciseSessionData =
    { UserId: UserId
      ExerciseType: ExerciseType
      Entries: (EntryId * int * PromptData * int16) list  // (EntryId, DisplayOrder, PromptData, PromptSchemaVersion)
      CreatedAt: DateTimeOffset }

// Input record for ICommitAttempt.
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

type IResolveEntrySelector =
    abstract ResolveEntrySelector: UserId -> EntrySelector -> Task<Result<EntryId list, SelectorError>>

type ICreateExerciseSession =
    abstract CreateExerciseSession: CreateExerciseSessionData -> Task<SessionBundle>

type IGetExerciseSession =
    abstract GetExerciseSession: ExerciseSessionId -> Task<ExerciseSession option>

type IGetExerciseSessionEntry =
    abstract GetExerciseSessionEntry: ExerciseSessionId -> EntryId -> Task<ExerciseSessionEntry option>

type IGetSessionBundle =
    abstract GetSessionBundle: UserId -> ExerciseSessionId -> Task<SessionBundle option>

type ICommitAttempt =
    abstract CommitAttempt: CommitAttemptData -> Task<SubmitAttemptResult>
```

`SelectorError` covers ownership failures (e.g. `VocabularyNotOwnedByUser`, `CollectionNotOwnedByUser`, `EntryNotOwnedByUser`).

### Operations.fs

Pure business logic. Delegates all I/O to capabilities.

```fsharp
val createSession  : env -> CreateSessionParameters -> Task<Result<SessionBundle, CreateSessionError>>
val getSession     : env -> UserId -> ExerciseSessionId -> Task<Result<SessionBundle, GetSessionError>>
val submitAttempt  : env -> UserId -> ExerciseSessionId -> EntryId -> rawAnswer: RawAnswer -> DateTimeOffset -> Task<Result<SubmitAttemptOutcome, SubmitAttemptError>>
```

`createSession` resolves the selector (including ownership validation), validates the resulting entry list is non-empty, caps it at `MaxSessionEntries`, batch-loads all entries in a single query, generates a prompt for each via `Dispatch.generatePrompt`, then persists the session and its entries (including `PromptData` and `PromptSchemaVersion`) in a single transaction. Returns the full `SessionBundle` with `Attempt = None` for each entry.

`getSession` verifies session ownership, loads all `ExerciseSessionEntries` for the session (ordered by `DisplayOrder`), loads all `ExerciseAttempts` for the session keyed by `EntryId`, assembles them into a `SessionBundle` with `Attempt` populated where available. Used by the resume endpoint.

`submitAttempt` loads the session and verifies ownership via `IGetExerciseSession`. Verifies the entry is in the session via `IGetExerciseSessionEntry`, reading the stored `PromptData` and `PromptSchemaVersion` at the same time. Calls `Dispatch.evaluate exerciseType promptSchemaVersion promptData rawAnswer` → `Result<bool, EvaluateError>`. On `Ok isCorrect`, passes all fields (including the server-computed `IsCorrect` and server-generated `AttemptedAt`) into `ICommitAttempt` as a `CommitAttemptData` record. On `Error EvaluateError`, propagates the error to the handler.

---

## Exercise-type dispatch

Exercise-type-specific logic lives in a DU-based dispatch module, **not** in an `IExerciseType` registry. There is no interface lookup or dynamic dispatch by string key.

### ExerciseTypes.fs

```fsharp
module MultipleChoice =
    val generatePrompt : Entry -> GeneratedPrompt   // pure; no I/O; returns PromptData + PromptSchemaVersion
    val evaluate       : promptSchemaVersion:int16 -> PromptData -> RawAnswer -> Result<bool, EvaluateError>

module Translation =
    val generatePrompt : Entry -> GeneratedPrompt
    val evaluate       : promptSchemaVersion:int16 -> PromptData -> RawAnswer -> Result<bool, EvaluateError>
```

### Dispatch.fs

```fsharp
module Dispatch =
    val generatePrompt : ExerciseType -> Entry -> GeneratedPrompt
    val evaluate       : ExerciseType -> promptSchemaVersion:int16 -> PromptData -> RawAnswer -> Result<bool, EvaluateError>
```

`generatePrompt` pattern-matches on the `ExerciseType` DU and delegates to the appropriate module. The function is **pure**: it performs no I/O, makes no DB calls, and invokes no capabilities. It takes `ExerciseType` and `Entry` and returns a `GeneratedPrompt` record containing both `PromptData` (serialised JSON including all client-visible answer/checking data) and `PromptSchemaVersion`. If a future exercise type requires extra context (e.g. a distractor pool), `Operations.createSession` must batch-load that context before the loop and pass it explicitly.

`evaluate` pattern-matches on the `ExerciseType` DU and delegates to the appropriate module’s `evaluate` function. It accepts `promptSchemaVersion` so each exercise-type module can reject payloads from schema versions it does not recognise. Returns `Result<bool, EvaluateError>` where `EvaluateError` is `UnsupportedPromptSchemaVersion | MalformedPromptData`. Called by `Operations.submitAttempt`; any error is propagated and mapped to `500 Internal Server Error` by the handler.

---

## Wordfolio.Api.DataAccess

### ExerciseSessions.fs

Data access for `ExerciseSessions` and `ExerciseSessionEntries`.

```fsharp
type ExerciseSessionRow       = { ... }   // CLIMutable record mirroring DB row; no Status/CompletedAt
type ExerciseSessionEntryRow  = { ... }   // includes PromptData: string and PromptSchemaVersion: int16

val createSessionWithEntriesAsync
    : data: CreateSessionRowData -> IDbConnection -> IDbTransaction -> CancellationToken
    -> Task<int>   // returns the new ExerciseSessions.Id; AppEnv assembles SessionBundle

val getSessionAsync
    : sessionId: int -> IDbConnection -> IDbTransaction -> CancellationToken
    -> Task<ExerciseSessionRow option>

val getSessionEntryAsync
    : sessionId: int -> entryId: int -> IDbConnection -> IDbTransaction -> CancellationToken
    -> Task<ExerciseSessionEntryRow option>

val getSessionEntriesAsync
    : sessionId: int -> IDbConnection -> IDbTransaction -> CancellationToken
    -> Task<ExerciseSessionEntryRow list>
```

### ExerciseAttempts.fs

```fsharp
type ExerciseAttemptRow = { ... }
// includes PromptData: string, PromptSchemaVersion: int16, RawAnswer: string,
// IsCorrect: bool (server-evaluated), SessionId: int option (nullable)

val commitAttemptAsync
    : data: CommitAttemptRowData -> IDbConnection -> IDbTransaction -> CancellationToken
    -> Task<CommitResult>
// CommitResult = Inserted of AttemptInserted | IdempotentReplay of AttemptAlreadyRecorded | ConflictingReplay
// Note: CommitResult is an internal DataAccess type; AppEnv maps it to the domain SubmitAttemptResult.

val getAttemptsBySessionAsync
    : sessionId: int -> userId: int -> IDbConnection -> IDbTransaction -> CancellationToken
    -> Task<ExerciseAttemptRow list>

val getWorstKnownEntriesAsync
    : userId: int -> scope: WorstKnownScopeRow -> count: int -> knowledgeWindowSize: int
    -> IDbConnection -> IDbTransaction -> CancellationToken
    -> Task<int list>  // EntryId list
```

`commitAttemptAsync` uses `INSERT ... ON CONFLICT (SessionId, EntryId) DO NOTHING RETURNING Id`. The insert includes `PromptData`, `PromptSchemaVersion`, `RawAnswer`, and `IsCorrect` (all server-computed). If the insert returns an `Id`, the result is `Inserted (AttemptInserted { AttemptId; IsCorrect })`. If no `Id` is returned, it re-reads the existing row and compares `RawAnswer` to return either `IdempotentReplay (AttemptAlreadyRecorded { IsCorrect })` or `ConflictingReplay`. `CommitResult` is an internal DataAccess type; `AppEnv.ICommitAttempt` maps it to the domain `SubmitAttemptResult`.

`getWorstKnownEntriesAsync` uses a windowed CTE over `ExerciseAttempts` to compute the recent hit rate for each candidate entry:

1. A `ranked_attempts` CTE assigns `ROW_NUMBER() OVER (PARTITION BY EntryId ORDER BY AttemptedAt DESC)` to each attempt row for the scoped candidate entries.
2. A `windowed_scores` CTE aggregates the top `@knowledgeWindowSize` rows per entry to produce `hit_rate`.
3. A subquery derives `LastAttemptedAt` directly from `ExerciseAttempts` (no `EntryKnowledge` table).
4. A LEFT JOIN between the candidate entry set and `windowed_scores` ensures cold entries (no attempts) are included with `NULL` hit rate, coalesced to `0.0`.
5. Order: `COALESCE(hit_rate, 0.0) ASC`, `last_att.LastAttemptedAt ASC NULLS FIRST`, `EntryId ASC`.

The `knowledgeWindowSize` parameter corresponds to the `KnowledgeWindowSize` constant in the domain layer; SQL sketches use `@knowledgeWindowSize` rather than a raw literal.

---

## Wordfolio.Api / Infrastructure / Environment.fs

`AppEnv` grows new interface implementations following the existing pattern:

```fsharp
interface IResolveEntrySelector with
    member _.ResolveEntrySelector userId selector =
        // validate ownership, branch on selector DU, call DataAccess query functions

interface ICreateExerciseSession with
    member _.CreateExerciseSession data =
        // data already contains all entries with PromptData + PromptSchemaVersion (assembled in Operations.createSession)
        // call ExerciseSessions.createSessionWithEntriesAsync (passes PromptData + PromptSchemaVersion per entry)
        // return SessionBundle (wraps returned int as SessionId, assembles entries with Attempt = None)

interface IGetExerciseSession with
    member _.GetExerciseSession sessionId =
        // call ExerciseSessions.getSessionAsync → map row to ExerciseSession option

interface IGetSessionBundle with
    member _.GetSessionBundle userId sessionId =
        // call ExerciseSessions.getSessionAsync → verify ownership
        // call ExerciseSessions.getSessionEntriesAsync → map to SessionBundleEntry list
        // call ExerciseAttempts.getAttemptsBySessionAsync → build EntryId → AttemptSummary map
        // join attempts onto entries to populate Attempt option
        // return SessionBundle option

interface IGetExerciseSessionEntry with
    member _.GetExerciseSessionEntry sessionId entryId =
        // call ExerciseSessions.getSessionEntryAsync → return ExerciseSessionEntry option

interface ICommitAttempt with
    member _.CommitAttempt data =
        // call ExerciseAttempts.commitAttemptAsync
        //   (passes PromptData, PromptSchemaVersion, RawAnswer, IsCorrect, AttemptedAt)
        // map internal CommitResult to domain SubmitAttemptResult
        //   (CommitResult.Inserted → domain Inserted of AttemptInserted,
        //    CommitResult.IdempotentReplay → domain IdempotentReplay of AttemptAlreadyRecorded,
        //    CommitResult.ConflictingReplay → domain ConflictingReplay)
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

Handlers parse the HTTP request, call the relevant `Operations` function, and map domain results to HTTP responses. **No business logic, no evaluation, and no prompt generation in handlers.** All orchestration (validate ownership, verify session membership, evaluate correctness, commit) lives in `Operations`.

The `CreateSessionHandler` performs **pre-DB size validation** before calling `Operations.createSession`: if the selector is `ExplicitEntries` with more than `MaxSessionEntries` IDs, or `WorstKnown` with `count > MaxSessionEntries`, the handler returns `400 Bad Request` immediately without touching the database. Ownership validation remains in `IResolveEntrySelector` and is only reached for requests that pass size validation.

The `SubmitAttemptHandler` reads only `rawAnswer` from the request body; it does not read or forward `isCorrect`. The handler returns `{ "isCorrect": <bool> }` in the response body (from the domain result). If `Operations.submitAttempt` returns an `EvaluateError`, the handler returns `500 Internal Server Error`.

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
    // includes PromptDataColumn = "PromptData" and PromptSchemaVersionColumn = "PromptSchemaVersion"
    ...

module ExerciseAttemptsTable =
    // includes PromptDataColumn = "PromptData", PromptSchemaVersionColumn = "PromptSchemaVersion",
    // RawAnswerColumn = "RawAnswer", IsCorrectColumn = "IsCorrect" (server-evaluated)
    ...
```
