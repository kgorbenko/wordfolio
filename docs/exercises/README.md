# Exercise Feature – Design Overview

This directory documents the agreed backend design for the Wordfolio exercise feature (v1). The design is intentionally concrete so that implementation can follow directly from these documents.

## Documents in this directory

| Document | Contents |
|---|---|
| [schema.md](schema.md) | Relational table definitions, column rationale, index strategy, and FK decisions |
| [module-structure.md](module-structure.md) | F# module layout across Domain, DataAccess, API, and Infrastructure layers |
| [data-flows.md](data-flows.md) | Step-by-step flows for session creation, session resume, and attempt submission |
| [retention-policy.md](retention-policy.md) | Purge policy, durable-history strategy, and future-extension notes |
| [review-decisions.md](review-decisions.md) | Decision log: one section per reviewed design point with decision and rationale |

## Key decisions at a glance

1. **PostgreSQL only (v1).** No Redis or external cache. All session, attempt, and knowledge state lives in the relational store.

2. **Exercise-abstract core, exercise-specific modules.** Session lifecycle (create, selector resolution, attempt commit) is a shared core. Prompt generation and correctness evaluation are in per-exercise-type modules under a DU-based dispatch layer (`Dispatch.fs`): `Dispatch.generatePrompt` and `Dispatch.evaluate`. No `IExerciseType` registry.

3. **Prompt determinism via stored payloads.** Prompts are generated once at session-creation time and stored in `ExerciseSessionEntries.PromptData`. `POST /exercises/sessions` returns the full session bundle (all `PromptData` entries including client-visible answer/checking data); `GET /exercises/sessions/{id}` returns the same bundle for resume or reload, enriched with per-entry attempt metadata (`attempt: { rawAnswer, isCorrect, attemptedAt } option`). No re-generation occurs. At attempt-submit time `PromptData` is copied from the session entry into `ExerciseAttempts` so each attempt row is self-describing and survives session purge without requiring a separate durable prompt table.

4. **Generic selector model resolved at session creation.** Selectors express intent (vocabulary scope, collection scope, worst-known entries, explicit list). Resolution produces a fixed snapshot stored as `ExerciseSessionEntries` rows, capped at `MaxSessionEntries = 10`. No selector logic runs during answer submission. `UserId` is never part of the selector payload; it comes from auth context. Ownership is validated for all selector types that pass handler-level size validation. **Oversize selectors are rejected with 400 Bad Request at the handler before any DB access:** `ExplicitEntries` with more than `MaxSessionEntries` IDs and `WorstKnown` with `count > MaxSessionEntries` are rejected pre-DB; ownership validation in `IResolveEntrySelector` applies only to requests that pass size validation.

5. **Three relational tables:** `ExerciseSessions`, `ExerciseSessionEntries`, `ExerciseAttempts`. No `EntryKnowledge` table. No JSONB or array-centric schema.

6. **Int identity PKs wrapped in domain ID types** (e.g. `ExerciseSessionId of int`), consistent with the existing `EntryId`, `CollectionId`, etc. pattern.

7. **Idempotent attempts by `(SessionId, EntryId)`.** Client sends only `RawAnswer`; the server evaluates correctness from stored `PromptData` using `Dispatch.evaluate`, which returns `Result<bool, EvaluateError>`. On `Ok isCorrect`, the server persists the authoritative `IsCorrect` and returns it in the response. On `Error EvaluateError`, the handler returns `500 Internal Server Error`. Idempotency uses `INSERT ... ON CONFLICT DO NOTHING RETURNING Id`; if no row returned, re-reads and compares `RawAnswer` to distinguish idempotent replay from conflicting replay. Conflicting replay returns `409 Conflict`. `SubmitAttemptResult` uses named record payloads: `Inserted of AttemptInserted` and `IdempotentReplay of AttemptAlreadyRecorded`.

8. **Unanswered entries produce no `ExerciseAttempts` row** and do not affect stats.

9. **All knowledge metrics derived from `ExerciseAttempts` directly.** There is no `EntryKnowledge` table. `TotalAttempts`, `LastAttemptedAt`, and recent hit rate are all computed from `ExerciseAttempts` at query time. `WorstKnown` uses windowed CTEs over the last `KnowledgeWindowSize = 10` attempts per `(UserId, EntryId)`. `KnowledgeWindowSize` is a named constant; SQL sketches use `@knowledgeWindowSize` rather than raw literals.

10. **`EntryId` FK → `Entries.Id` ON DELETE CASCADE on snapshot and history tables.** `ExerciseSessionEntries` and `ExerciseAttempts` each carry a hard FK `EntryId → Entries.Id ON DELETE CASCADE`. Deleting an entry cascade-deletes all related session context and attempt history. `ExerciseAttempts.SessionId` is a **nullable** `int` (no FK to `ExerciseSessions`); the session purge job sets it to `NULL` before deleting the session row so attempt history survives purge. Post-purge, `SessionId` on an attempt is intentionally absent.

11. **No session status machine.** `ExerciseSessions` has no `Status` or `CompletedAt` column. Sessions are scaffolding rows purged 30 days after `CreatedAt`.

12. **Server is authoritative for correctness.** Handlers parse HTTP only and perform pre-DB size validation (oversize selectors rejected with `400 Bad Request` before any DB access). `Operations.submitAttempt` validates ownership and session membership, calls `Dispatch.evaluate` with stored `PromptData` and the submitted `RawAnswer` — returning `Result<bool, EvaluateError>` — then persists the authoritative `IsCorrect` and returns it in the response. `Dispatch.evaluate` errors are mapped to `500 Internal Server Error` by the handler. `AttemptedAt` is always server-generated; the client does not supply it.

13. **`ExerciseType` stored as `smallint`.** `ExerciseSessions.ExerciseType` and `ExerciseAttempts.ExerciseType` are PostgreSQL `smallint` columns with a stable numeric mapping (`0 = MultipleChoice`, `1 = Translation`). The domain model uses the F# DU for ergonomics; the persistence boundary maps to `int16`. No string storage.

14. **Batch preload + server-side evaluation.** `POST /exercises/sessions` returns the full session bundle containing all `PromptData` entries. The client uses `PromptData` to display prompts and — if desired — to show an immediate local preview, but the submit endpoint accepts only `RawAnswer`. The server re-evaluates using `Dispatch.evaluate` (with the same stored `PromptData`) to produce the authoritative `IsCorrect` returned in the response.

15. **`PromptSchemaVersion` column on entries and attempts.** `ExerciseSessionEntries` and `ExerciseAttempts` carry `PromptSchemaVersion smallint NOT NULL`. It records the version of the `PromptData` JSON schema in effect when the row was written, enabling forward-compatible deserialisation and future migrations of prompt payloads without rewriting rows.

16. **Domain wrappers `PromptData` and `RawAnswer`.** These are opaque single-case DU wrappers in the domain model. `PromptSchemaVersion` is kept as plain `int16` (no wrapper). This keeps the type boundary clear at the persistence layer without over-engineering version tracking.

17. **`UserId` FK policy and account-deletion.** `ExerciseSessions.UserId` and `ExerciseAttempts.UserId` carry hard FKs to `Users.Id` with **no cascade**. Session and attempt rows are never deleted as a side effect of the FK; deletion of a user account requires an explicit account-deletion flow that removes these rows via application logic before removing the user record.

18. **Session-create idempotency deferred.** There is no idempotency key on `POST /exercises/sessions`. Creating duplicate sessions for the same selector is possible under concurrent requests. This is a known gap; a client-supplied `IdempotencyKey` header (stored on `ExerciseSessions`) is documented as a future extension.

19. **`Dispatch.generatePrompt` is pure.** `generatePrompt` takes `ExerciseType` and `Entry` and returns a `GeneratedPrompt` record (`{ PromptData; PromptSchemaVersion }`). It performs no I/O, makes no DB calls, and invokes no capabilities. If a future exercise type needs extra context (e.g. distractor pool), `Operations.createSession` must batch-load that context before the loop and pass it explicitly to `generatePrompt`.

20. **`ExerciseSessionEntry.Id` is not in the domain type.** The DB PK column (`Id int identity`) exists on `ExerciseSessionEntries` for relational integrity, but the domain record `ExerciseSessionEntry` does not include an `Id` field. Session entries are identified by `(SessionId, EntryId)`.

21. **`EvaluateError` type.** `Dispatch.evaluate` returns `Result<bool, EvaluateError>`. `EvaluateError` is a DU with cases `UnsupportedPromptSchemaVersion` and `MalformedPromptData`. `Operations.submitAttempt` propagates the error; the handler maps any `EvaluateError` to `500 Internal Server Error`.
