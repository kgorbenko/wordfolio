# Exercise Feature – Retention Policy and Future Extensions

---

## Stores required (v1)

**PostgreSQL only.**

No Redis, no external cache, no secondary store. All session state and attempt history live in the relational database. This is deliberately simple for v1 and sufficient for the expected write volume.

If caching becomes necessary (e.g. hot knowledge lookups for large user bases), it can be added as a pure optimisation without any schema change. The design does not assume or require it.

---

## Data retention tiers

Two tiers with distinct retention policies reflect the different purposes of the tables:

### Tier 1 – Durable learning history (keep indefinitely)

| Table | Rationale |
|---|---|
| `ExerciseAttempts` | Per-attempt history is the source of truth for all knowledge metrics (`TotalAttempts`, `LastAttemptedAt`, windowed hit rate), learning analytics, debugging, and any future algorithm (SM-2, Leitner). The `PromptData` column (copied from `ExerciseSessionEntries` at submit time) means each row is fully self-describing even after the parent session is purged — no separate durable prompt table is needed. Deleting attempt history would silently corrupt a user’s learning record and lose the windowed hit-rate signal used by `WorstKnown`. |

There is no `EntryKnowledge` table. All knowledge metrics are derived from `ExerciseAttempts` at query time.

These rows are **never purged** as part of normal operation. Deletion requires an explicit user-initiated account-deletion flow (see below).

### Tier 2 – Purgeable scaffolding (single TTL)

| Table | Policy | Rationale |
|---|---|---|
| `ExerciseSessions` | Purge 30 days after `CreatedAt` | Session scaffolding has no independent retention value after the TTL. All durable learning data is captured in Tier 1. There is no completion state — sessions are purged by age regardless of how many entries were answered. |
| `ExerciseSessionEntries` | Cascade with parent `ExerciseSessions` | No independent retention value once the session is purged. `ON DELETE CASCADE` from `ExerciseSessions` handles this automatically. Additionally, `EntryId FK → Entries.Id ON DELETE CASCADE` removes session entries when the underlying entry is deleted. |

### Why purging sessions does not corrupt attempt history or prompt context

- `ExerciseAttempts` has **no hard FK to `ExerciseSessions`**. Before deleting a session row, the purge job sets `ExerciseAttempts.SessionId = NULL` for all attempts belonging to that session. Post-purge, `SessionId = NULL` on an attempt is intentional and expected — it signals that the originating session has been purged. Attempt history survives intact.
- `PromptData` is denormalised onto `ExerciseAttempts` at submit time (copied from `ExerciseSessionEntries.PromptData`). The prompt context for every answered entry therefore survives session purge without requiring a separate durable table.
- `UserId` is stored directly on `ExerciseAttempts`, so ownership is unambiguous after the session row is gone.
- `ExerciseAttempts.EntryId` carries a FK to `Entries.Id ON DELETE CASCADE`. Attempt history is removed if the entry is deleted — this is the intended behaviour (see EntryId FK policy in `schema.md`).
- `ExerciseSessionEntries` cascades with `ExerciseSessions` (purge of a session removes its entries). `ExerciseSessionEntries.EntryId` also carries a FK to `Entries.Id ON DELETE CASCADE`, so if the entry is deleted first, the session entry row is removed before the session is purged.

---

## Account-deletion behaviour

`ExerciseSessions.UserId` and `ExerciseAttempts.UserId` carry hard FKs to `Users.Id` with **no cascade**. An explicit account-deletion flow must:

1. Delete all `ExerciseAttempts` rows where `UserId = <userId>`.
2. Delete all `ExerciseSessions` rows where `UserId = <userId>` (cascades `ExerciseSessionEntries` automatically).
3. Delete the `Users` row.

Performing step 3 before steps 1–2 will fail at the database level due to the FK constraint, which enforces the correct sequence.

---

## Purge implementation

The purge job runs in two steps to preserve attempt history:

```sql
-- Step 1: null out SessionId on attempts belonging to sessions about to be purged
UPDATE wordfolio."ExerciseAttempts"
SET "SessionId" = NULL
WHERE "SessionId" IN (
    SELECT "Id" FROM wordfolio."ExerciseSessions"
    WHERE "CreatedAt" < NOW() - INTERVAL '30 days'
);

-- Step 2: delete the sessions (cascades ExerciseSessionEntries automatically)
DELETE FROM wordfolio."ExerciseSessions"
WHERE "CreatedAt" < NOW() - INTERVAL '30 days';
```

The `IX_ExerciseSessions_CreatedAt` index makes the age-based predicate efficient. The UPDATE in step 1 uses a subquery so both operations share the same age condition.

The purge job should run as a background hosted service or scheduled task, not as part of request handling.

---

## Future extensions

### Spaced repetition (SM-2 / Leitner)

All attempt history is already in `ExerciseAttempts`, which is the correct input for spaced-repetition algorithms. A `DueForReview` selector variant can be added to `EntrySelector` and resolved by a new `getDueForReviewEntriesAsync` query that reads `ExerciseAttempts` and computes the next-review date. If persistent per-entry scheduling state is needed (e.g. easiness factor, interval), a new `EntrySchedule` table can be added without modifying `ExerciseAttempts`.

### Per-type attempt metadata

Exercise types that need to record extra structured data (e.g. the chosen distractors in a multiple-choice prompt) can store this in a separate table (e.g. `MultipleChoiceAttemptDetails`) linked by `AttemptId`. The `RawAnswer` column on `ExerciseAttempts` already preserves the user’s verbatim input for all types. A separate details table keeps `ExerciseAttempts` schema-stable and avoids JSONB.

### Richer selectors

Additional selector types (e.g. `DueForReview` based on next-review date, `ByTag`) can be added to the `EntrySelector` DU and resolved by new `getXxxEntriesAsync` functions in `DataAccess`, with no changes to the session-creation transaction logic.

### Multiple sessions per entry

The current design allows the same entry to appear in multiple concurrent sessions (no lock or reservation). If exclusive-reservation semantics are needed, an `ActiveSessionEntry` table or a status column on `ExerciseSessionEntries` can be added.

### External caching

If windowed hit-rate queries become a hot path, a read-through cache (e.g. Redis) can front the relevant `ExerciseAttempts` aggregation in `AppEnv`. The domain and data-access layers require no changes.

### Session-create idempotency (known gap)

`POST /exercises/sessions` has no idempotency key. Duplicate concurrent requests can create duplicate sessions for the same selector. A future extension can add an optional `IdempotencyKey` column to `ExerciseSessions`, populated from a client-supplied header (e.g. `Idempotency-Key: <uuid>`). The creation handler would use `INSERT ... ON CONFLICT (UserId, IdempotencyKey) DO NOTHING RETURNING Id` and re-read the existing session on conflict, returning the same bundle. This is deferred to a future iteration.
