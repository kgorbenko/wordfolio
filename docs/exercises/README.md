# Exercise Feature – Design Overview

This directory documents the agreed backend design for the Wordfolio exercise feature (v1). The design is intentionally concrete so that implementation can follow directly from these documents.

## Documents in this directory

| Document | Contents |
|---|---|
| [schema.md](schema.md) | Relational table definitions, column rationale, index strategy, and FK decisions |
| [module-structure.md](module-structure.md) | F# module layout across Domain, DataAccess, API, and Infrastructure layers |
| [data-flows.md](data-flows.md) | Step-by-step flows for session creation, prompt retrieval, and attempt submission |
| [retention-policy.md](retention-policy.md) | Purge policy, durable-history strategy, and future-extension notes |
| [review-decisions.md](review-decisions.md) | Decision log: one section per reviewed design point with decision and rationale |

## Key decisions at a glance

1. **PostgreSQL only (v1).** No Redis or external cache. All session, attempt, and knowledge state lives in the relational store.

2. **Exercise-abstract core, exercise-specific modules.** Session lifecycle (create, selector resolution, attempt commit, knowledge update) is a shared core. Prompt generation and correctness evaluation are in per-exercise-type modules under a DU-based dispatch layer (`Dispatch.fs`). No `IExerciseType` registry.

3. **Prompt determinism via stored payloads.** Prompts are generated once at session-creation time and stored in `ExerciseSessionEntries.PromptData`. `POST /exercises/sessions` returns the full session bundle (all `PromptData` entries including client-visible answer/checking data); `GET /exercises/sessions/{id}` returns the same bundle for resume or reload. No re-generation occurs.

4. **Generic selector model resolved at session creation.** Selectors express intent (vocabulary scope, collection scope, worst-known entries, explicit list). Resolution produces a fixed snapshot stored as `ExerciseSessionEntries` rows. No selector logic runs during answer submission. `UserId` is never part of the selector payload; it comes from auth context. Ownership is validated for all selector types.

5. **Four relational tables:** `ExerciseSessions`, `ExerciseSessionEntries`, `ExerciseAttempts`, `EntryKnowledge`. No JSONB or array-centric schema.

6. **Int identity PKs wrapped in domain ID types** (e.g. `ExerciseSessionId of int`), consistent with the existing `EntryId`, `CollectionId`, etc. pattern.

7. **Idempotent attempts by `(SessionId, EntryId)`.** Uses `INSERT ... ON CONFLICT DO NOTHING RETURNING Id`; if no row returned, re-reads and compares `RawAnswer` to distinguish idempotent replay from conflicting replay. Conflicting replay returns `409 Conflict`.

8. **Unanswered entries produce no `ExerciseAttempts` row** and do not affect stats.

9. **`EntryKnowledge` stores raw counters** (`TotalAttempts`, `CorrectAttempts`, `LastAttemptedAt`). Hit rate is derived. Richer scoring (SM-2, Leitner) is deferred.

10. **`EntryId` FK → `Entries.Id` ON DELETE CASCADE on snapshot and history tables.** `ExerciseSessionEntries`, `ExerciseAttempts`, and `EntryKnowledge` each carry a hard FK `EntryId → Entries.Id ON DELETE CASCADE`. Deleting an entry cascade-deletes all related session context, attempt history, and knowledge counters for that entry. `ExerciseAttempts.SessionId` remains a plain indexed `int` with no FK to `ExerciseSessions`, so attempt history survives session purge.

11. **No session status machine.** `ExerciseSessions` has no `Status` or `CompletedAt` column. Sessions are scaffolding rows purged 30 days after `CreatedAt`.

12. **Submit orchestration in the domain layer.** Handlers parse HTTP only. `Operations.submitAttempt` validates ownership and session membership, then commits the attempt using the client-provided `IsCorrect`.

13. **`ExerciseType` stored as `smallint`.** `ExerciseSessions.ExerciseType` and `ExerciseAttempts.ExerciseType` are PostgreSQL `smallint` columns with a stable numeric mapping (`0 = MultipleChoice`, `1 = Translation`). The domain model uses the F# DU for ergonomics; the persistence boundary maps to `int16`. No string storage.

14. **Batch preload + client-side evaluation.** `POST /exercises/sessions` returns the full session bundle containing all `PromptData` entries (including client-visible answer/checking data). The client evaluates answers locally and advances immediately. The submit endpoint accepts `RawAnswer` and `IsCorrect` from the client; the server validates ownership and session membership, then persists the attempt and updates `EntryKnowledge` counters using the client-provided `IsCorrect`. No server-side evaluation occurs during submission.
