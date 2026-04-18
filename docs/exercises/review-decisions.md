# Exercise Feature – Review Decisions

This file is the decision log for the design review. One section per reviewed point. Each section states the decision (Accept / Partially Accept / Reject) and the rationale that drove it.

---

## 1. Prompt determinism

**Decision:** Accept.

**Change:** Add `PromptData text NOT NULL` to `ExerciseSessionEntries`. Prompts are generated once at session-creation time (using entry data plus any knowledge context available at that moment) and serialised as JSON into this column.

- `POST /exercises/sessions` returns the full session bundle (all `PromptData` entries) so the client has everything it needs at creation time. See Decision 15 (supersedes per-entry endpoint below).
- `GET /exercises/sessions/{id}` returns the stored session bundle for resume/reload — same shape as session creation. See Decision 16 (supersedes per-entry endpoint below).
- ~~`GET /exercises/sessions/{id}/entries/{entryId}/prompt` reads and returns the stored payload verbatim.~~ **Superseded by Decision 16.**
- ~~`POST .../attempts` evaluates correctness against the stored `PromptData`, not a freshly generated prompt.~~ **Superseded by Decision 15** — correctness is evaluated client-side; the server accepts `IsCorrect` from the client. **Decision 15 was later reversed by Decision 22 (Round 4)**: the server now evaluates correctness using `Dispatch.evaluate` with the stored `PromptData`.

**Rationale:** Without a stored payload, two requests to the same endpoint could receive different prompts if the underlying entry or knowledge data changed between the requests (e.g. another session updated hit counters). This would make correctness evaluation non-deterministic and break idempotency. Storing the payload once removes this class of bug entirely. Because correctness was evaluated client-side in rounds 2–3 (see Decision 15 — reversed by Decision 22 in Round 4, which restored server-side evaluation via `Dispatch.evaluate`), ~~the submit path does not read `PromptData`~~ (**⚠ superseded by Decision 18, then extended by Decision 22** — the submit path reads `PromptData` to copy it into `ExerciseAttempts`, and after Decision 22 it also passes `PromptData` to `Dispatch.evaluate` for authoritative server-side correctness); the stored snapshot exists to give the client everything it needs at session-creation time and to support session resume (see Decision 16).

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

**Change:** Add `Dispatch.fs` with `Dispatch.generatePrompt`, pattern-matching on `ExerciseType`. ~~Add `Dispatch.evaluate`~~ — **`Dispatch.evaluate` was superseded by Decision 15** (correctness evaluation moved client-side; `Dispatch.evaluate` was not added), **but Decision 15 was reversed by Decision 22 (Round 4)**: `Dispatch.evaluate` is re-added and the server is now authoritative for correctness. Each per-type module (`MultipleChoice.fs`, `Translation.fs`) exposes both `generatePrompt` and `evaluate` functions.

---

## 5. WorstKnown semantics

**Decision:** Accept.

**Change:** `WorstKnown` gains an optional scope parameter (`AllUserEntries | WithinVocabulary of VocabularyId | WithinCollection of CollectionId`). `UserId` is removed from the selector payload entirely (taken from auth context throughout). The `getWorstKnownEntriesAsync` query uses a LEFT JOIN between the candidate entry set and `EntryKnowledge` (filtered by `UserId`), coalescing null hit rates to `0.0`, and orders by:
1. `COALESCE(CorrectAttempts::float / NULLIF(TotalAttempts, 0), 0.0) ASC`
2. `LastAttemptedAt ASC NULLS FIRST`
3. `EntryId ASC` (stable tiebreak)

> **⚠ Partially superseded by Decision 20 (Round 3).** The hit-rate computation above (using `CorrectAttempts / TotalAttempts` from `EntryKnowledge`) is replaced by a windowed CTE over the last `KnowledgeWindowSize = 10` `ExerciseAttempts` rows. The scope, cold-entry inclusion, and ordering columns are unchanged; only the source of the hit rate value changes. See Decision 20 for the authoritative query shape.

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

> **⚠ Superseded by Decision 13.** The no-FK policy stated below is reversed. `ExerciseSessionEntries.EntryId`, `ExerciseAttempts.EntryId`, and `EntryKnowledge.EntryId` carry hard FKs with `ON DELETE CASCADE`. `ExerciseAttempts.SessionId` remaining a plain indexed `int` is retained. See Decision 13 for the authoritative `EntryId` FK policy.

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

> **⚠ Partially superseded by Decision 24.** `EntryKnowledge` is removed entirely (Decision 24), so the `EntryKnowledge.EntryId` FK cascade below no longer applies. The `ExerciseSessionEntries.EntryId` and `ExerciseAttempts.EntryId` FK cascades are retained.

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

> **⚠ Superseded by Decision 22.** Client-side evaluation and client-provided `IsCorrect` described below are reversed. The server is authoritative for correctness: the submit endpoint accepts only `RawAnswer`; `Operations.submitAttempt` calls `Dispatch.evaluate` with the stored `PromptData` and returns the server-computed `isCorrect`. See Decision 22 for the authoritative flow.

**Decision:** Accept.

**Supersedes:** Decision 10 (Submit flow layering — server-side evaluation via `Dispatch.evaluate` in `Operations.submitAttempt`).

**Change:**
- `POST /exercises/sessions` returns the full session bundle: all `PromptData` entries including client-visible answer/checking data (question, options, correct answer for `MultipleChoice`; word and accepted translations for `Translation`).
- The submit endpoint (`POST /exercises/sessions/{id}/entries/{entryId}/attempts`) accepts `RawAnswer` **and** `IsCorrect` from the client. The server validates ownership and session membership, then persists the attempt and updates `EntryKnowledge` counters using the client-provided `IsCorrect`. `Dispatch.evaluate` is not called during submission. **⚠ Partially superseded by Decision 19 (Round 3):** the `EntryKnowledge` update no longer uses `IsCorrect`; only `TotalAttempts` and `LastAttemptedAt` are updated. `IsCorrect` is stored on `ExerciseAttempts` only.

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
- `docs/exercises/module-structure.md` — added `SessionBundle` and `SessionBundleEntry` types; noted `ExerciseType` maps to `int16`; updated `ICreateExerciseSession` to return `SessionBundle`; added `IGetSessionBundle` (`IGetExerciseSessionEntry` is retained alongside it for use by `submitAttempt`); updated `Operations` signatures and descriptions; replaced `GetPromptHandler` with `GetSessionHandler`; removed `Dispatch.evaluate` from dispatch module; updated `AppEnv` interface implementations.
- `docs/exercises/retention-policy.md` — updated tier-2 `ExerciseSessionEntries` rationale; rewrote "Why purging sessions does not corrupt..." section to reflect EntryId FK cascade policy.
- `docs/exercises/review-decisions.md` — added Round 2 sections 13–17 (this entry).

---

## Round 3 revisions

The following decisions were made in a third design review. Where a Round 3 decision conflicts with a prior round, the Round 3 decision is authoritative.

---

### 18. PromptData denormalized onto ExerciseAttempts

**Decision:** Accept.

**Change:** Add `PromptData text NOT NULL` to `ExerciseAttempts`. At attempt-submit time, `PromptData` is copied from the corresponding `ExerciseSessionEntries.PromptData` row and persisted directly onto the attempt record.

No separate durable prompt table is introduced.

**Rationale:** After `ExerciseSessions` and `ExerciseSessionEntries` are purged (30-day TTL), `ExerciseAttempts` rows would otherwise lose all prompt context. By denormalising `PromptData` onto each attempt row at submit time, every attempt is fully self-describing. This preserves the original prompt for analytics, debugging, and any future re-evaluation without adding a new table or query join. The cost is a modest increase in row size, which is acceptable given that `ExerciseAttempts` already stores `RawAnswer` as a variable-length text column.

The submit path reads `PromptData` from the `ExerciseSessionEntry` already loaded during the session-membership check (no additional DB call). `SubmitAttemptParameters` gains a `PromptData: string` field to carry it into `ICommitAttempt`.

**Note:** Decision 1 (Prompt determinism) stated that "the submit path does not read `PromptData`". This decision reverses that specific sentence; the submit path now does read `PromptData` — not to evaluate correctness, but to copy it into the attempt record.

---

### 19. Remove CorrectAttempts from EntryKnowledge

> **⚠ Superseded by Decision 24.** `EntryKnowledge` is removed entirely, not just the `CorrectAttempts` column. This decision's retained columns (`TotalAttempts`, `LastAttemptedAt`) are therefore also not created. See Decision 24.

**Decision:** Accept.

**Change:** Remove the `CorrectAttempts int NOT NULL` column from `EntryKnowledge`. The table retains only `TotalAttempts` and `LastAttemptedAt` alongside the composite PK `(UserId, EntryId)`.

The `EntryKnowledge` UPSERT in `upsertEntryKnowledgeAsync` no longer accepts an `isCorrect` parameter. It increments `TotalAttempts` and sets `LastAttemptedAt` only.

**Rationale:** A running `CorrectAttempts` counter represents the full lifetime of attempts. For `WorstKnown` selection — the primary consumer of knowledge scores — lifetime hit rate is a poor signal for a user who was once weak at an entry but has recently improved (or vice versa). Removing `CorrectAttempts` from `EntryKnowledge` forces all hit-rate queries to derive the score from recent attempt history (see Decision 20), which is more actionable. It also simplifies the UPSERT and removes a column that could become stale if attempt rows are ever deleted for a single entry.

---

### 20. Windowed hit rate and WorstKnown CTE

**Decision:** Accept.

**Change:** Hit rate (knowledge score) is computed at query time from the last `KnowledgeWindowSize = 10` `ExerciseAttempts` rows for each `(UserId, EntryId)` pair. This constant is named `KnowledgeWindowSize` in the codebase.

`getWorstKnownEntriesAsync` is rewritten to use a two-CTE windowed query:

1. `ranked_attempts` — `ROW_NUMBER() OVER (PARTITION BY EntryId ORDER BY AttemptedAt DESC)` for all candidate entries.
2. `windowed_scores` — aggregate the top `KnowledgeWindowSize` rows per entry to compute `hit_rate`.
3. LEFT JOIN the scoped entry set against `windowed_scores` (for windowed hit rate) and `EntryKnowledge` (for `LastAttemptedAt` tie-breaking). **⚠ The `EntryKnowledge` join here is superseded by Decision 24**, which removes the table; `LastAttemptedAt` is instead derived via `MAX(AttemptedAt)` subquery over `ExerciseAttempts` directly.

Ordering remains:
1. `COALESCE(windowed_hit_rate, 0.0) ASC` — cold entries (no attempts) rank first.
2. `EntryKnowledge.LastAttemptedAt ASC NULLS FIRST` — least recently attempted breaks ties; entries with no `EntryKnowledge` row rank before any attempted entry at the same hit rate. **⚠ Superseded by Decision 24**: `LastAttemptedAt` is derived from `ExerciseAttempts` directly; the ordering column is unchanged but the source table is not `EntryKnowledge`.
3. `EntryId ASC` — stable tiebreak.

The scope, cold-entry inclusion (LEFT JOIN), and deterministic ordering from Decision 5 are preserved unchanged.

The index `IX_ExerciseAttempts_UserId_EntryId_AttemptedAt` on `(UserId, EntryId, AttemptedAt DESC)` replaces the former `IX_ExerciseAttempts_UserId_EntryId` to support the windowed ordering efficiently.

**Rationale:** A window of the last 10 attempts reflects recent performance rather than historical accumulation. A user who struggled with an entry early on but has answered it correctly in the last 10 attempts should not appear in `WorstKnown` results; a trailing lifetime average would keep them there. The window size of 10 is large enough to be statistically meaningful but small enough to respond to recent improvement quickly.

---

### 21. Documents updated (Round 3)

**Files changed:**
- `docs/exercises/README.md` — updated key decision 3 (PromptData copy on submit); updated key decision 9 (removed `CorrectAttempts`, windowed scoring).
- `docs/exercises/schema.md` — added `PromptData text NOT NULL` to `ExerciseAttempts`; updated index from `IX_ExerciseAttempts_UserId_EntryId` to `IX_ExerciseAttempts_UserId_EntryId_AttemptedAt DESC`; removed `CorrectAttempts` from `EntryKnowledge`; rewrote `EntryKnowledge` derived-values section to describe windowed scoring; replaced `WorstKnown` LEFT-JOIN description with full windowed-CTE SQL sketch.
- `docs/exercises/data-flows.md` — updated Flow 1 WorstKnown description; updated Flow 3 INSERT to include `PromptData`; updated `EntryKnowledge` UPSERT to `TotalAttempts + 1` and `LastAttemptedAt` only (no `CorrectAttempts` delta).
- `docs/exercises/module-structure.md` — removed `CorrectAttempts` from `EntryKnowledge` type; added `PromptData: string` to `SubmitAttemptParameters`; updated `ExerciseAttemptRow` comment; removed `isCorrect` param from `upsertEntryKnowledgeAsync`; rewrote `getWorstKnownEntriesAsync` description to cover windowed CTE; updated `AppEnv.ICommitAttempt` comment; added `PromptDataColumn` to `ExerciseAttemptsTable` in Schema.fs additions.
- `docs/exercises/retention-policy.md` — updated Tier 1 rationale for `ExerciseAttempts` (PromptData durability) and `EntryKnowledge` (no `CorrectAttempts`, windowed scoring); added PromptData bullet to "Why purging sessions does not corrupt…" section.
- `docs/exercises/diagrams.md` — removed `CorrectAttempts` from `EntryKnowledge` ER entity; added `PromptData` to `ExerciseAttempts` ER entity; updated key-rules note; updated selector-resolution diagram WorstKnown label; added `PromptData` copy note to submit-flow sequence; updated UPSERT label in submit-flow and idempotency-tree diagrams; updated lifecycle state diagram attempt-insertion label.
- `docs/exercises/review-decisions.md` — added Round 3 sections 18–21 (this entry).

---

## Round 4 revisions

The following decisions were made in a fourth design review. Where a Round 4 decision conflicts with a prior round, the Round 4 decision is authoritative.

---

### 22. Server-side correctness evaluation — **supersedes Decision 15**

**Decision:** Reversed. The server is authoritative for correctness.

**Supersedes:** Decision 15 (Batch preload and client-side evaluation — the server accepted `IsCorrect` from the client; `Dispatch.evaluate` was not called during submission).

**Change:**
- `POST /exercises/sessions/{id}/entries/{entryId}/attempts` accepts **only `RawAnswer`** in the request body. `IsCorrect` is no longer accepted from the client.
- `Operations.submitAttempt` calls `Dispatch.evaluate exerciseType promptData rawAnswer` → `isCorrect: bool` using the `PromptData` stored on the `ExerciseSessionEntry`.
- The server-computed `IsCorrect` is persisted on `ExerciseAttempts` and returned in the submit response (`{ "isCorrect": <bool> }`).
- `AttemptedAt` is always server-generated; the client does not supply it.
- `Dispatch.evaluate` is re-added to `Dispatch.fs` and each exercise-type module exposes an `evaluate` function alongside `generatePrompt`.

**Rationale:** Client-provided `IsCorrect` cannot be trusted as an authoritative learning signal. A misconfigured or adversarial client could record incorrect answers as correct, corrupting the user’s knowledge history and `WorstKnown` selection. Evaluating on the server with the stored `PromptData` gives deterministic, auditable correctness results without adding latency for prompt re-generation (the prompt is already stored). The `PromptData` is already read during the session-membership check required by the submit flow, so no extra DB call is needed.

---

### 23. Resume bundle includes per-entry attempt metadata — **extends Decision 16**

**Decision:** Accept (extends Decision 16).

**Change:** `GET /exercises/sessions/{sessionId}` returns a richer bundle than `POST /exercises/sessions`. Each entry in the bundle includes an `attempt` field:

```json
{
  "entryId": <id>,
  "displayOrder": <n>,
  "promptData": { ... },
  "attempt": {
    "rawAnswer": "<raw answer>",
    "isCorrect": true,
    "attemptedAt": "<ISO 8601>"
  }
}
```

`attempt` is `null` if the entry has not been answered. `POST /exercises/sessions` returns `attempt = null` for all entries (the session is freshly created).

`Operations.getSession` loads `ExerciseAttempts` for the session (keyed by `EntryId`) and joins them onto the entry list to populate the `Attempt option`.

**Rationale:** On resume, the client needs to know which entries have already been answered (and what the results were) to restore UI state correctly and avoid re-presenting already-answered entries. Without this data, the client cannot distinguish a fresh session from a partially-completed one.

---

### 24. Remove EntryKnowledge entirely — **supersedes Decisions 13, 19**

**Decision:** Reversed. `EntryKnowledge` is removed.

**Supersedes:**
- Decision 13 (EntryId FK policy — added FK cascade to `EntryKnowledge`).
- Decision 19 (Remove CorrectAttempts from EntryKnowledge — retained `TotalAttempts` and `LastAttemptedAt` in `EntryKnowledge`).

**Change:** The `EntryKnowledge` table is not created. All knowledge metrics (`TotalAttempts`, `LastAttemptedAt`, windowed hit rate) are derived from `ExerciseAttempts` at query time.

- `upsertEntryKnowledgeAsync` is removed. `commitAttemptAsync` no longer performs a knowledge upsert.
- `getWorstKnownEntriesAsync` derives `LastAttemptedAt` via a `MAX(AttemptedAt)` subquery over `ExerciseAttempts` instead of reading from `EntryKnowledge`.
- `IGetEntryKnowledge` capability is removed. `Dispatch.generatePrompt` no longer takes an `EntryKnowledge option` parameter.
- The `EntryKnowledge.fs` data access module is removed.
- Three migrations are anticipated (previously four): `CreateExerciseSessionsTable`, `CreateExerciseSessionEntriesTable`, `CreateExerciseAttemptsTable`.

**Rationale:** `EntryKnowledge` was a lightweight denormalised cache of counters already computable from `ExerciseAttempts`. Removing it eliminates a write that must be atomically coordinated with every attempt insert, simplifies the schema, and removes a class of potential inconsistency (e.g. `EntryKnowledge.TotalAttempts` drifting out of sync with actual attempt counts). `ExerciseAttempts` with the `IX_ExerciseAttempts_UserId_EntryId_AttemptedAt` index provides sufficient performance for windowed queries at v1 scale.

---

### 25. PromptSchemaVersion column

**Decision:** Accept.

**Change:** Add `PromptSchemaVersion smallint NOT NULL` to `ExerciseSessionEntries` and `ExerciseAttempts`. It is set at session creation (from the version in effect at the time `generatePrompt` runs) and copied onto the attempt row at submit time alongside `PromptData`.

`PromptSchemaVersion` is kept as plain `int16` in the domain model (no DU wrapper).

**Rationale:** `PromptData` is a serialised JSON blob whose schema may evolve over time as new exercise-type features are added. Without a version tag, deserialisation code cannot distinguish a v1 payload from a v2 payload. Storing the version alongside the payload enables forward-compatible deserialisation and future in-place or lazy migration of stored payloads without rewriting rows.

---

### 26. Domain wrappers PromptData and RawAnswer

**Decision:** Accept.

**Change:** Introduce single-case DU wrappers `PromptData` and `RawAnswer` in the domain model. These are opaque at the domain boundary; the data access layer unwraps them to `string` before writing and re-wraps after reading. `PromptSchemaVersion` is kept as plain `int16`.

**Rationale:** Wrapping `string` values that carry specific semantic roles prevents accidental confusion between, for example, a `PromptData` string and a `RawAnswer` string in function parameters. The compiler enforces the distinction. `PromptSchemaVersion` is not wrapped because its role as a small integer version discriminant is already clear from its type and name.

---

### 27. Capability record types: CreateExerciseSessionData and CommitAttemptData

**Decision:** Accept.

**Change:** Replace long parameter lists on `ICreateExerciseSession` and `ICommitAttempt` with named record types: `CreateExerciseSessionData` and `CommitAttemptData`. Callers in `Operations` construct these records; `AppEnv` implementations receive them.

**Rationale:** Long positional parameter lists are fragile: reordering or adding parameters silently breaks callers if types happen to match. Named records make each field explicit, allow future additions without changing all call sites, and improve readability.

---

### 28. ExerciseAttempts.SessionId nullable and nulled on purge — **extends prior no-FK decision**

**Decision:** Accept (extends Decision 9 / Decision 13 on `SessionId`).

**Change:** `ExerciseAttempts.SessionId` changes from `int NOT NULL` to `int NULL`. The purge job, before deleting a session row, issues:

```sql
UPDATE wordfolio."ExerciseAttempts"
SET "SessionId" = NULL
WHERE "SessionId" = <sessionId>;
```

(or in batch for the age-based purge). After the session row is deleted, attempts for that session have `SessionId = NULL`. This is documented as the expected post-purge state.

**Rationale:** A non-nullable `SessionId` with no FK constraint was already relying on application-level discipline to avoid orphaned references. Making it nullable and explicitly nulling it on purge makes the post-purge state unambiguous: `NULL` means "originating session purged", whereas a stale non-null integer could be mistaken for a valid (but missing) session reference. The unique index `UQ_ExerciseAttempts_SessionId_EntryId` is retained; PostgreSQL unique indexes treat `NULL` values as distinct from each other, so nulling `SessionId` on multiple attempts does not violate the constraint.

---

### 29. KnowledgeWindowSize as named constant; SQL uses @knowledgeWindowSize

**Decision:** Accept.

**Change:** `KnowledgeWindowSize = 10` is a named constant in the domain layer (not a magic literal). All SQL sketches in the documentation and in `getWorstKnownEntriesAsync` use `@knowledgeWindowSize` as a parameter name rather than the raw literal `10`. The data access function receives `knowledgeWindowSize: int` as an explicit parameter.

**Rationale:** Hardcoding `10` in SQL strings couples the query to a value that may change and makes it invisible to code search. A named constant is one source of truth; passing it as a parameter to the query function avoids SQL string interpolation and keeps the query parameterised.

---

### 30. UserId FK policy and account-deletion documented explicitly

**Decision:** Accept.

**Change:** Document `ExerciseSessions.UserId` and `ExerciseAttempts.UserId` as hard FKs to `Users.Id` with **no cascade**. Document the required account-deletion sequence explicitly: delete `ExerciseAttempts` → delete `ExerciseSessions` (cascades `ExerciseSessionEntries`) → delete `Users`.

**Rationale:** Without explicit documentation, the no-cascade policy can appear to be an oversight. The rationale (preserve learning history during normal operation; enforce explicit deletion sequence) needs to be stated so implementers do not inadvertently add `ON DELETE CASCADE` or omit the deletion steps.

---

### 31. Session-create idempotency deferred (known gap)

**Decision:** Defer.

**Change:** No idempotency key is added to `POST /exercises/sessions` at this time. Concurrent duplicate requests can create duplicate sessions. This is documented as a known gap.

Future extension: add an optional `IdempotencyKey` column to `ExerciseSessions` populated from a client-supplied `Idempotency-Key` header. The creation handler would use `INSERT ... ON CONFLICT (UserId, IdempotencyKey) DO NOTHING RETURNING Id` and re-read and return the existing session on conflict.

**Rationale:** Session-create idempotency adds meaningful schema complexity (unique constraint, conflict-read path, header validation) for a case that is rare in practice (the client creating the same session twice in rapid succession). Deferring it keeps v1 simpler while documenting the gap so it is not forgotten.

---

### 32. Batch entry loading at session creation; MaxSessionEntries = 50

**Decision:** Accept.

**Change:** `Operations.createSession` loads all resolved entries in a single batch query (`SELECT * FROM Entries WHERE Id IN (...)`) rather than one query per entry. The resolved list is capped at `MaxSessionEntries = 50` entries before the batch load and before `ExerciseSessionEntries` rows are created. `MaxSessionEntries` is a named constant.

`Dispatch.generatePrompt` no longer receives an `EntryKnowledge option` parameter (removed alongside `EntryKnowledge`).

**Rationale:** N+1 queries at session creation are unnecessary: all entry IDs are known after selector resolution, so a single IN-list query retrieves all entries at once. The cap prevents unbounded session sizes that would produce large prompt bundles and slow session-creation inserts.

---

### 33. Documents updated (Round 4)

**Files changed:**
- `docs/exercises/README.md` — updated key decisions to reflect three tables (no `EntryKnowledge`); server-side evaluation; server-generated `AttemptedAt`; `MaxSessionEntries = 50`; `PromptSchemaVersion`; `PromptData`/`RawAnswer` domain wrappers; `SessionId` nullable on purge; `UserId` FK policy; session-create idempotency as deferred/known gap.
- `docs/exercises/schema.md` — removed `EntryKnowledge` table; added `PromptSchemaVersion` to `ExerciseSessionEntries` and `ExerciseAttempts`; made `ExerciseAttempts.SessionId` nullable; noted `IsCorrect` is server-evaluated and `AttemptedAt` is server-generated; added `UserId` FK policy section; updated no-FK section to describe nullable + purge-null strategy; updated `WorstKnown` SQL to remove `EntryKnowledge` tie-break join and use `@knowledgeWindowSize`; updated migration count to three; added `MaxSessionEntries = 50` to selector section.
- `docs/exercises/data-flows.md` — removed `isCorrect` from submit request body; added `Dispatch.evaluate` call in Flow 3; submit response now returns `isCorrect`; Flow 1 uses batch entry load; removed `EntryKnowledge` upsert from Flow 3; resume bundle (Flow 2) now includes `attempt` metadata per entry; `AttemptedAt` noted as server-generated; updated transaction boundary table.
- `docs/exercises/module-structure.md` — added `PromptData`/`RawAnswer` wrappers and `AttemptSummary`/`MaxSessionEntries`/`KnowledgeWindowSize` to `Types.fs`; updated `SessionBundleEntry` with `Attempt option`; updated `SubmitAttemptParameters` (removed `IsCorrect` as input, made it server-computed); updated `SubmitAttemptResult` to carry `isCorrect`; renamed capability input records to `CreateExerciseSessionData`/`CommitAttemptData`; removed `IGetEntryKnowledge`; added `Dispatch.evaluate`; removed `EntryKnowledge.fs` from DataAccess; added `getAttemptsBySessionAsync`; updated `AppEnv` implementations; updated handler description to note `rawAnswer` only in request.
- `docs/exercises/retention-policy.md` — removed `EntryKnowledge` from Tier 1; updated purge implementation to two-step (null SessionId, then delete); added `UserId` FK / account-deletion section; updated future-extensions spaced-repetition note; added session-create idempotency as known gap.
- `docs/exercises/diagrams.md` — removed `EntryKnowledge` from ER diagram; made `SessionId` nullable on `ExerciseAttempts`; removed `EntryKnowledge` from retention-tier diagram; updated architecture diagram to remove `EntryKnowledge.fs`; updated selector diagram to reflect no `EntryKnowledge` tie-break; updated create-session sequence to batch entry load; updated resume sequence to load attempts; updated submit sequence to show `Dispatch.evaluate` call and `isCorrect` in response; removed `EntryKnowledge` upsert from idempotency tree; updated lifecycle state diagram.
- `docs/exercises/review-decisions.md` — added Round 4 sections 22–33 (this entry).

---

## Round 5 revisions

The following decisions were made in a fifth design review. Where a Round 5 decision conflicts with a prior round, the Round 5 decision is authoritative.

---

### 34. MaxSessionEntries reduced to 10 — **refines Decision 32**

**Decision:** Accept.

**Supersedes/Refines:** Decision 32 (`MaxSessionEntries = 50`).

**Change:** `MaxSessionEntries` is set to `10` everywhere (domain constant, schema docs, selector section, diagrams). All prior references to `MaxSessionEntries = 50` are updated to `MaxSessionEntries = 10`.

**Rationale:** 50 entries per session is larger than needed for a focused vocabulary exercise and would produce unwieldy session bundles. 10 entries is the agreed v1 cap; the named constant means this can be raised in a single-line change if needed.

---

### 35. Handler-level pre-DB 400 validation for oversized selectors — **refines Decision 8**

**Decision:** Accept.

**Refines:** Decision 8 (Ownership checks — `IResolveEntrySelector` returns `Result<EntryId list, SelectorError>`).

**Change:** Oversize selector requests are rejected with `400 Bad Request` at the handler **before any DB access**:
- `ExplicitEntries` with more than `MaxSessionEntries` IDs → `400 Bad Request`.
- `WorstKnown` with `count > MaxSessionEntries` → `400 Bad Request`.

Ownership validation in `IResolveEntrySelector` is unchanged; it is reached only for requests that pass size validation. `SelectorError` and `403 Forbidden` mapping are unaffected.

**Rationale:** Validating selector size at the handler avoids an unnecessary DB round-trip for requests that will always be rejected. It also produces a clear `400 Bad Request` (client error) rather than silently truncating the list, which would confuse clients expecting the full requested set.

---

### 36. Dispatch.generatePrompt purity rule

**Decision:** Accept.

**Change:** `Dispatch.generatePrompt` (and each per-type module's `generatePrompt`) is declared **pure**: no I/O, no DB access, no capability calls. The signature is `ExerciseType -> Entry -> GeneratedPrompt`. If a future exercise type requires extra context (e.g. a distractor pool), `Operations.createSession` must batch-load that context before the loop and pass it explicitly to `generatePrompt`; the function signature must not perform I/O as a side effect.

**Rationale:** Purity makes `generatePrompt` trivially testable and reasoning about it straightforward. It also enforces a clear boundary: all I/O required for session creation is in `Operations.createSession` (batch load) or `AppEnv` (DB writes); the generation step is a pure transform.

---

### 37. Named record payloads for SubmitAttemptResult — **refines Decision 22**

**Decision:** Accept.

**Refines:** Decision 22 (`SubmitAttemptResult` used anonymous tuple/inline payloads: `Inserted of ExerciseAttemptId * isCorrect: bool` and `IdempotentReplay of isCorrect: bool`).

**Change:** Replace tuple payloads with named records:

```fsharp
type AttemptInserted =
    { AttemptId: ExerciseAttemptId
      IsCorrect: bool }

type AttemptAlreadyRecorded =
    { IsCorrect: bool }

type SubmitAttemptResult =
    | Inserted of AttemptInserted
    | IdempotentReplay of AttemptAlreadyRecorded
    | ConflictingReplay
```

The internal DataAccess `CommitResult` type is kept as-is. `AppEnv.ICommitAttempt` maps `CommitResult` to the domain `SubmitAttemptResult` using these named records.

**Rationale:** Named records make field access explicit, prevent positional confusion, and allow new fields to be added without breaking all match arms. Anonymous tuples and `isCorrect:` label syntax are fragile if the DU is extended.

---

### 38. Drop ExerciseSessionEntry.Id from the domain type

**Decision:** Accept.

**Change:** `ExerciseSessionEntry` in the domain no longer has an `Id: int` field. The DB PK column (`Id int identity NOT NULL`) is retained on `ExerciseSessionEntries` for relational integrity and idempotency-index purposes; it simply does not surface in the domain record. Session entries are identified by `(SessionId, EntryId)` at the domain level.

**Rationale:** The domain has no use for the DB-assigned row ID of a session entry — entries are always located by their natural key `(SessionId, EntryId)`. Exposing the PK leaks a persistence detail into the domain layer without providing value.

---

### 39. GeneratedPrompt type for generatePrompt return value — **refines Decisions 32, 25**

**Decision:** Accept.

**Refines:** Decision 32 (`generatePrompt` returned `PromptData` directly) and Decision 25 (`PromptSchemaVersion` column).

**Change:** Introduce a `GeneratedPrompt` record:

```fsharp
type GeneratedPrompt =
    { PromptData: PromptData
      PromptSchemaVersion: int16 }
```

`Dispatch.generatePrompt` and each per-type module's `generatePrompt` return `GeneratedPrompt` instead of bare `PromptData`. `Operations.createSession` destructures the record to obtain both `PromptData` and `PromptSchemaVersion` when building `CreateExerciseSessionData`.

**Rationale:** Returning `PromptData` alone required callers to separately know or compute `PromptSchemaVersion`. Bundling both into a single `GeneratedPrompt` return value makes the generation step self-contained and eliminates the risk of mismatching version and payload.

---

### 40. EvaluateError type and Result return for Dispatch.evaluate — **refines Decision 22**

**Decision:** Accept.

**Refines:** Decision 22 (`Dispatch.evaluate` returned `bool` directly).

**Change:**

```fsharp
type EvaluateError =
    | UnsupportedPromptSchemaVersion
    | MalformedPromptData
```

`Dispatch.evaluate` and each per-type module's `evaluate` return `Result<bool, EvaluateError>` instead of plain `bool`. The signature is:

```fsharp
val evaluate : ExerciseType -> promptSchemaVersion:int16 -> PromptData -> RawAnswer -> Result<bool, EvaluateError>
```

`Operations.submitAttempt` handles the `Result`: on `Ok isCorrect` it proceeds to `ICommitAttempt`; on `Error EvaluateError` it propagates the error. The handler maps any `EvaluateError` to `500 Internal Server Error`.

**Rationale:** `evaluate` can legitimately fail if the stored `PromptData` payload is from a schema version the current evaluator does not support, or if the JSON is malformed. Returning `bool` directly silently swallows these cases or forces exceptions. A typed `Result` makes the failure surface explicit at the call site and forces callers (`Operations.submitAttempt`) to handle it. Mapping to `500` (not `400`) is correct: the `PromptData` is server-owned; a malformed payload is a server-side integrity failure, not a client error.

---

### 41. Documents updated (Round 5)

**Files changed:**
- `docs/exercises/README.md` — updated `MaxSessionEntries = 50` → `10`; updated decision 4 with pre-DB validation and ownership note; updated decision 7 with named records and `EvaluateError` + `500` mapping; updated decision 12 with evaluate `Result` and 500 path; added decisions 19–21 (purity, domain type drop of `Id`, `EvaluateError`).
- `docs/exercises/schema.md` — updated `MaxSessionEntries = 50` → `10` in `ExerciseSessionEntries` section and selector section; added pre-DB size validation note.
- `docs/exercises/data-flows.md` — Flow 1 handler step: added pre-DB size validation (400); updated cap to `10`; updated `Dispatch.generatePrompt` return to `GeneratedPrompt`. Flow 3: updated `Dispatch.evaluate` signature to `promptSchemaVersion` + `Result<bool, EvaluateError>`; added EvaluateError → 500 path; updated `Inserted`/`IdempotentReplay` branch comments to named record shape; added `EvaluateError` to handler HTTP mapping table.
- `docs/exercises/module-structure.md` — updated `MaxSessionEntries = 10`; dropped `Id` field from `ExerciseSessionEntry`; added `AttemptInserted`, `AttemptAlreadyRecorded`, `EvaluateError`, `GeneratedPrompt` types; updated `SubmitAttemptResult` DU; updated `ExerciseTypes.fs` and `Dispatch.fs` signatures; updated `generatePrompt` and `evaluate` prose descriptions; updated `submitAttempt` description; updated handler section with pre-DB 400 validation and EvaluateError → 500; updated `commitAttemptAsync` description and `CommitResult` comment; updated `AppEnv.ICommitAttempt` mapping comment.
- `docs/exercises/diagrams.md` — updated `MaxSessionEntries = 50` → `10` in selector and create-session diagrams; added pre-DB size validation node and `BadReq` to selector resolution diagram; updated create-session sequence handler step; updated `Dispatch.generatePrompt` return label to `GeneratedPrompt`; updated submit-attempt sequence evaluate call to include `promptSchemaVersion`; changed evaluate return to `Result<bool, EvaluateError>`; wrapped commit path in `alt Ok isCorrect / else Error EvaluateError` branch showing `500` path; updated `Inserted`/`IdempotentReplay` labels to named records; updated idempotency decision tree diagram node labels to named records.
- `docs/exercises/review-decisions.md` — added Round 5 sections 34–41 (this entry).
