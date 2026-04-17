# Exercise Feature – Review Decisions

This file is the decision log for the design review. One section per reviewed point. Each section states the decision (Accept / Partially Accept / Reject) and the rationale that drove it.

---

## 1. Prompt determinism

**Decision:** Accept.

**Change:** Add `PromptData text NOT NULL` to `ExerciseSessionEntries`. Prompts are generated once at session-creation time (using entry data plus any knowledge context available at that moment) and serialised as JSON into this column.

- `POST /exercises/sessions` returns the full session bundle (all `PromptData` entries) so the client has everything it needs at creation time. See Decision 15 (supersedes per-entry endpoint below).
- `GET /exercises/sessions/{id}` returns the stored session bundle for resume/reload — same shape as session creation. See Decision 16 (supersedes per-entry endpoint below).
- ~~`GET /exercises/sessions/{id}/entries/{entryId}/prompt` reads and returns the stored payload verbatim.~~ **Superseded by Decision 16.**
- ~~`POST .../attempts` evaluates correctness against the stored `PromptData`, not a freshly generated prompt.~~ **Superseded by Decision 15** — correctness is evaluated client-side; the server accepts `IsCorrect` from the client.

**Rationale:** Without a stored payload, two requests to the same endpoint could receive different prompts if the underlying entry or knowledge data changed between the requests (e.g. another session updated hit counters). This would make correctness evaluation non-deterministic and break idempotency. Storing the payload once removes this class of bug entirely and simplifies the submit path: `Operations.submitAttempt` reads `PromptData` from a single DB row rather than reloading and re-running prompt generation.

---

## 2. Session lifecycle

**Decision:** Accept.

**Change:** Remove `Status varchar(32)` and `CompletedAt datetimeoffset` from `ExerciseSessions`. Replace `IX_ExerciseSessions_Status_CompletedAt` with `IX_ExerciseSessions_CreatedAt`.

Sessions are scaffolding rows with a single 30-day TTL anchored to `CreatedAt`. There is no Active/Completed/Abandoned state machine.

**Rationale:** A status column introduces complexity without proportionate benefit for v1. The domain does not need to track whether a session is "done" — attempt rows are independent (owned directly by `UserId`), and the purge strategy does not need to distinguish completed from abandoned sessions. Removing the state machine eliminates the need for status-transition logic, status-check guards in handlers, and the risk of stale or inconsistent status values.

---

## 3. Raw answer

**Decision:** Accept.

**Change:** Add `RawAnswer text NOT NULL` to `ExerciseAttempts`.

Idempotency comparison uses `RawAnswer` (not only `IsCorrect`). Two submissions for the same `(SessionId, EntryId)` are idempotent if and only if `RawAnswer` matches; otherwise they conflict.

**Rationale:** Comparing only `IsCorrect` cannot distinguish "same answer, same outcome" from "different answer, happens to have the same outcome" (e.g. two wrong guesses that both evaluate to `false`). Storing `RawAnswer` makes the idempotency check unambiguous. It also preserves the verbatim user input for future audit or analytics use cases without requiring a separate table.

---

## 4. Exercise-type dispatch

**Decision:** Partially accept.

**Accepted:** Use an F# DU-based dispatch module (`Dispatch.fs`) that pattern-matches on the `ExerciseType` DU. Submit orchestration (load context → verify session membership → commit) lives in `Operations.submitAttempt`, not in handlers.

**Rejected:** `IExerciseType` registry / lookup interface. An OO registry pattern adds indirection and interface plumbing that is unnecessary in a codebase that already models exercise types as a closed DU. Adding a new exercise type requires a new DU case and new match arms — the compiler enforces exhaustiveness. No runtime dispatch, no dictionary lookup, no interface implementation required.

**Change:** Add `Dispatch.fs` with `Dispatch.generatePrompt`, pattern-matching on `ExerciseType`. ~~Add `Dispatch.evaluate`~~ — **`Dispatch.evaluate` is superseded by Decision 15**: correctness evaluation is client-side; `Dispatch.evaluate` is not added. Each per-type module (`MultipleChoice.fs`, `Translation.fs`) exposes only pure functions for prompt generation.

---

## 5. WorstKnown semantics

**Decision:** Accept.

**Change:** `WorstKnown` gains an optional scope parameter (`AllUserEntries | WithinVocabulary of VocabularyId | WithinCollection of CollectionId`). `UserId` is removed from the selector payload entirely (taken from auth context throughout). The `getWorstKnownEntriesAsync` query uses a LEFT JOIN between the candidate entry set and `EntryKnowledge` (filtered by `UserId`), coalescing null hit rates to `0.0`, and orders by:
1. `COALESCE(CorrectAttempts::float / NULLIF(TotalAttempts, 0), 0.0) ASC`
2. `LastAttemptedAt ASC NULLS FIRST`
3. `EntryId ASC` (stable tiebreak)

**Rationale:** Without a LEFT JOIN, entries that have never been attempted are excluded from `WorstKnown` results. This defeats the purpose of the selector for new users or new vocabulary additions — the most important entries to practice are precisely those never seen before. COALESCE to `0.0` ensures cold entries rank at the bottom of the hit-rate ordering (i.e. they come first, as the worst-known). `UserId` in the selector payload was an inconsistency because all other selectors are scoped by the auth context user; removing it unifies the pattern.

---

## 6. Idempotency race

**Decision:** Accept.

**Change:** Replace the SELECT-then-INSERT pattern in `commitAttemptAsync` with `INSERT ... ON CONFLICT (SessionId, EntryId) DO NOTHING RETURNING Id`. If the insert returns no `Id` (conflict), re-read the existing row and compare `RawAnswer`:
- Match → `IdempotentReplay`
- Mismatch → `ConflictingReplay`

**Rationale:** The original SELECT-then-INSERT pattern has a TOCTOU race: two concurrent requests for the same `(SessionId, EntryId)` can both observe no existing row, both proceed to insert, and one will fail with a unique-constraint violation (or worse, if the constraint is not enforced, produce a duplicate). The `INSERT ... ON CONFLICT DO NOTHING RETURNING Id` pattern is atomic under the unique index and eliminates the race entirely. The re-read on conflict is a single additional SELECT that only executes on the rare collision path.

---

## 7. Selector inconsistency

**Decision:** Accept.

**Change:** `UserId` is removed from all selector variants. It is never part of the selector payload in requests or in domain types. All selector resolution uses the `UserId` extracted from the auth context. This applies to all four variants: `VocabularyScope`, `CollectionScope`, `WorstKnown`, and `ExplicitEntries`.

**Rationale:** Having `UserId` in the `WorstKnown` variant but not in `VocabularyScope` or `CollectionScope` was an inconsistency that could lead to ownership-bypass bugs (a client could supply a different user's ID). Centralising `UserId` in the auth context makes the ownership model uniform and removes a class of potential security issues.

---

## 8. Ownership checks

**Decision:** Accept.

**Change:** `IResolveEntrySelector` returns `Result<EntryId list, SelectorError>` instead of `Task<EntryId list>`. Ownership is validated before resolution proceeds:
- `VocabularyScope v`: verify `UserId` owns vocabulary `v`.
- `CollectionScope c`: verify `UserId` owns collection `c`.
- `WorstKnown (WithinVocabulary v) _`: verify `UserId` owns vocabulary `v`.
- `WorstKnown (WithinCollection c) _`: verify `UserId` owns collection `c`.
- `ExplicitEntries ids`: verify all entry IDs in `ids` belong to `UserId`.

Failures surface as `SelectorError` (e.g. `VocabularyNotOwnedByUser`, `CollectionNotOwnedByUser`, `EntryNotOwnedByUser`) and map to `403 Forbidden` in the handler.

**Rationale:** Without explicit ownership checks, a user could create a session over another user's vocabulary by supplying that vocabulary's ID. The session-creation flow is the correct place to enforce this because it is the only point where the selector is resolved against the database.

---

## 9. Entry deletion

**Decision:** Accept.

**Change:** Remove hard FK constraints from `ExerciseSessionEntries.EntryId`, `ExerciseAttempts.EntryId`, and `EntryKnowledge.EntryId`. All three columns become plain indexed `int` values with no FK to `Entries`.

**Rationale:** A hard FK would prevent entry deletion while any of these rows reference the entry, or (with `ON DELETE CASCADE`) would silently delete learning history and in-flight session context. Neither behaviour is correct:
- Blocking deletion degrades the user experience (users cannot freely manage their vocabulary).
- Cascade deletion corrupts the learning record and could invalidate a session that the user is actively using.
- `ExerciseSessionEntries.PromptData` is a self-contained snapshot; the prompt remains valid even if the live entry changes or is deleted. The session can be completed from its stored context.

This is consistent with the existing decision to store `ExerciseAttempts.SessionId` as a plain int rather than a FK to `ExerciseSessions`.

---

## 10. Submit flow layering

> **⚠ Superseded by Decision 15.** The server-side evaluation step (step 4 below, `Dispatch.evaluate`) is removed. The client submits `IsCorrect` directly. The remaining steps (ownership validation, session membership check, commit) are retained.

**Decision:** Accept.

**Change:** `SubmitAttemptHandler` parses the HTTP request (extract `sessionId`, `entryId`, `rawAnswer` from URL/body and `UserId` from auth context) and calls `Operations.submitAttempt`. All business logic is in the operation:
1. Load session, verify ownership.
2. Load `ExerciseSessionEntry`, verify entry is in session.
3. Deserialise `PromptData`.
4. Call `Dispatch.evaluate exerciseType promptData rawAnswer` → `isCorrect`.
5. Call `ICommitAttempt`.

**Rationale:** Handlers that perform DB reads, deserialise domain objects, and invoke evaluation logic are mixing HTTP concerns with business logic. This violates the layering contract established elsewhere in the codebase (handlers parse and map; operations orchestrate). Moving all orchestration to the operation also makes it testable without an HTTP layer.

---

## 11. Retention detail

**Decision:** Accept (single TTL).

**Change:** Simplify the purge policy to a single TTL: purge `ExerciseSessions` rows (and their cascaded `ExerciseSessionEntries` rows) 30 days after `CreatedAt`. Remove the previous two-tier policy (30 days for Completed, 7 days for Abandoned) — that policy depended on a status column that no longer exists.

Purge query:
```sql
DELETE FROM wordfolio."ExerciseSessions"
WHERE "CreatedAt" < NOW() - INTERVAL '30 days';
```

**Rationale:** A single TTL is simpler to implement and reason about. The 30-day window is generous enough to cover any active or recently-started session. Since there is no status column, the only available anchor is `CreatedAt`. The shorter 7-day window for abandoned sessions was only meaningful if sessions could be explicitly abandoned; without a status machine, that distinction does not exist.

---

## 12. Documents updated

**Decision:** All listed documents updated.

**Files changed:**
- `docs/exercises/README.md` — updated key decisions summary; added link to `review-decisions.md`.
- `docs/exercises/schema.md` — removed `Status`/`CompletedAt`; added `PromptData`; added `RawAnswer`; removed hard FKs from `EntryId` columns; updated idempotency contract; updated `WorstKnown` selector shape and ordering; added ownership validation notes.
- `docs/exercises/data-flows.md` — Flow 1 includes prompt generation and `PromptData` storage; Flow 2 reads stored payload (no re-generation); Flow 3 moves evaluation to domain operation; all status checks removed; `RawAnswer`-based idempotency; updated transaction boundaries.
- `docs/exercises/module-structure.md` — removed `SessionStatus`; updated `ExerciseSession`, `ExerciseSessionEntry`, `EntrySelector`, `SubmitAttemptParameters`; added `Dispatch.fs`; added `ExerciseTypes.fs` separation; updated capabilities; updated data-access signatures; updated handler responsibilities.
- `docs/exercises/retention-policy.md` — replaced two-tier status-based purge with single 30-day `CreatedAt` TTL; updated purge query; updated rationale sections.
- `docs/exercises/review-decisions.md` — **new file** (this document).

---

## Round 2 revisions

The following decisions were made in a second design review. Where a Round 2 decision conflicts with a Round 1 decision, the Round 2 decision is authoritative.

---

### 13. EntryId FK policy — **supersedes Decision 9**

**Decision:** Reversed. Add hard FK constraints.

**Supersedes:** Decision 9 (Entry deletion — no-EntryId-FK policy stated that `ExerciseSessionEntries.EntryId`, `ExerciseAttempts.EntryId`, and `EntryKnowledge.EntryId` should be plain indexed `int` columns with no FK to `Entries`).

**Change:** Add `EntryId FK → Entries.Id ON DELETE CASCADE` to `ExerciseSessionEntries`, `ExerciseAttempts`, and `EntryKnowledge`. `ExerciseAttempts.SessionId` remains a plain indexed `int` (no FK to `ExerciseSessions`); that part of Decision 9 is retained.

**Rationale:** Keeping orphaned rows for deleted entries creates stale learning records:
- `WorstKnown` would surface entries the user has deleted.
- Knowledge counters for deleted entries pollute aggregate stats.
- Prompt snapshots for in-flight sessions become unreachable context with no user-visible entry to match them.

Cascade-delete provides clean, predictable semantics. A user who deletes a vocabulary entry intends to stop practising it; cascading all related rows is the correct outcome.

---

### 14. ExerciseType storage

**Decision:** Accept. Store as `smallint`.

**Change:** Replace `varchar(64)` with `smallint` in `ExerciseSessions.ExerciseType` and `ExerciseAttempts.ExerciseType`. Stable numeric mapping:

| Value | `ExerciseType` DU case |
|---|---|
| `0` | `MultipleChoice` |
| `1` | `Translation` |

The domain model keeps the F# DU. The data access layer maps `ExerciseType → int16` before writing and `int16 → ExerciseType` after reading.

**Rationale:** `varchar(64)` for a small closed discriminator wastes space, requires string comparison, and makes rename refactors risky (stored strings are not migrated automatically). A `smallint` with a documented mapping is compact and schema-stable. The DU↔int16 translation is a thin persistence concern isolated to the data access layer.

---

### 15. Batch preload and client-side evaluation — **supersedes Decision 10**

**Decision:** Accept.

**Supersedes:** Decision 10 (Submit flow layering — server-side evaluation via `Dispatch.evaluate` in `Operations.submitAttempt`).

**Change:**
- `POST /exercises/sessions` returns the full session bundle: all `PromptData` entries including client-visible answer/checking data (question, options, correct answer for `MultipleChoice`; word and accepted translations for `Translation`).
- The submit endpoint (`POST /exercises/sessions/{id}/entries/{entryId}/attempts`) accepts `RawAnswer` **and** `IsCorrect` from the client. The server validates ownership and session membership, then persists the attempt and updates `EntryKnowledge` counters using the client-provided `IsCorrect`. `Dispatch.evaluate` is not called during submission.

**Rationale:** A server round-trip for each evaluation adds latency that degrades the user experience. Since all prompt data (including answer keys) is already on the client after session creation, it can evaluate locally and advance immediately. The server's role in the submit flow is validation and persistence — not re-evaluation. For a vocabulary learning tool, the risk of a client spoofing `IsCorrect = true` is not a meaningful threat worth the latency cost of server-side re-evaluation.

---

### 16. Resume endpoint — **supersedes per-entry GET prompt**

**Decision:** Accept (incorporated into Decision 15 above).

**Supersedes:** The per-entry `GET /exercises/sessions/{id}/entries/{entryId}/prompt` endpoint described in Flow 2 of the original data-flows document.

**Change:** `GET /exercises/sessions/{sessionId}` returns the complete session bundle (same shape as the `POST /exercises/sessions` response). This endpoint serves both resume and reload cases. `Operations.getSession` replaces `Operations.getPrompt`.

**Rationale:** A per-entry prompt endpoint requires the client to fetch prompts one at a time, which is inefficient and complicates client state management. Returning the full bundle on both creation and resume gives the client everything it needs in a single response and keeps the API surface minimal.

---

### 17. Documents updated (Round 2)

**Files changed:**
- `docs/exercises/README.md` — updated key decisions 3 and 10; added decisions 13 and 14.
- `docs/exercises/schema.md` — changed `ExerciseType` columns from `varchar(64)` to `smallint`; added numeric mapping section; added `EntryId FK → Entries.Id ON DELETE CASCADE` to `ExerciseSessionEntries`, `ExerciseAttempts`, and `EntryKnowledge`; replaced no-EntryId-FK rationale with EntryId FK policy section; updated `EntryKnowledge` description; noted `IsCorrect` is client-provided.
- `docs/exercises/data-flows.md` — Flow 1 returns full session bundle; Flow 2 replaced with resume/reload (`GET /exercises/sessions/{id}`); Flow 3 adds `isCorrect` to request body and removes `Dispatch.evaluate` step; updated transaction boundaries.
- `docs/exercises/module-structure.md` — added `SessionBundle` and `SessionBundleEntry` types; noted `ExerciseType` maps to `int16`; updated `ICreateExerciseSession` to return `SessionBundle`; replaced `IGetExerciseSessionEntry` with `IGetSessionBundle`; updated `Operations` signatures and descriptions; replaced `GetPromptHandler` with `GetSessionHandler`; removed `Dispatch.evaluate` from dispatch module; updated `AppEnv` interface implementations.
- `docs/exercises/retention-policy.md` — updated tier-2 `ExerciseSessionEntries` rationale; rewrote "Why purging sessions does not corrupt..." section to reflect EntryId FK cascade policy.
- `docs/exercises/review-decisions.md` — added Round 2 sections 13–17 (this entry).
