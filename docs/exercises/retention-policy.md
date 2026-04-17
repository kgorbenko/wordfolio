# Exercise Feature – Retention Policy and Future Extensions

---

## Stores required (v1)

**PostgreSQL only.**

No Redis, no external cache, no secondary store. All session state, attempt history, and knowledge counters live in the relational database. This is deliberately simple for v1 and sufficient for the expected write volume.

If caching becomes necessary (e.g. hot knowledge lookups for large user bases), it can be added as a pure optimisation without any schema change. The design does not assume or require it.

---

## Data retention tiers

Two tiers with distinct retention policies reflect the different purposes of the tables:

### Tier 1 – Durable learning history (keep indefinitely)

| Table | Rationale |
|---|---|
| `ExerciseAttempts` | Per-attempt history is the source of truth for learning analytics, debugging, and any future algorithm (SM-2, Leitner). Deleting it would silently corrupt a user's learning record. |
| `EntryKnowledge` | Aggregated counters built from attempt history. Kept in sync with `ExerciseAttempts`; losing it means losing the derived knowledge model. |

These tables are **never purged** as part of normal operation. Deletion requires an explicit user-initiated account-deletion flow.

### Tier 2 – Purgeable scaffolding (single TTL)

| Table | Policy | Rationale |
|---|---|---|
| `ExerciseSessions` | Purge 30 days after `CreatedAt` | Session scaffolding has no independent retention value after the TTL. All durable learning data is captured in Tier 1. There is no completion state — sessions are purged by age regardless of how many entries were answered. |
| `ExerciseSessionEntries` | Cascade with parent `ExerciseSessions` | No independent retention value once the session is purged. `ON DELETE CASCADE` from `ExerciseSessions` handles this automatically. Additionally, `EntryId FK → Entries.Id ON DELETE CASCADE` removes session entries when the underlying entry is deleted. |

### Why purging sessions does not corrupt attempt history

- `ExerciseAttempts` has **no hard FK to `ExerciseSessions`**. `SessionId` is stored as a plain indexed `int`. When a session row is purged, attempt history survives intact.
- `UserId` is stored directly on `ExerciseAttempts`, so ownership is unambiguous after the session row is gone.
- `ExerciseAttempts.EntryId` carries a FK to `Entries.Id ON DELETE CASCADE`. Attempt history is removed if the entry is deleted — this is the intended behaviour (see EntryId FK policy in `schema.md`).
- `EntryKnowledge.EntryId` also carries a FK to `Entries.Id ON DELETE CASCADE`. Knowledge counters are removed if the entry is deleted.
- `ExerciseSessionEntries` cascades with `ExerciseSessions` (purge of a session removes its entries). `ExerciseSessionEntries.EntryId` also carries a FK to `Entries.Id ON DELETE CASCADE`, so if the entry is deleted first, the session entry row is removed before the session is purged.

---

## Purge implementation

Purge should run as a background job (e.g. a hosted service or scheduled task), not as part of request handling. A single query covers both tables:

```sql
DELETE FROM wordfolio."ExerciseSessions"
WHERE "CreatedAt" < NOW() - INTERVAL '30 days';
```

`ExerciseSessionEntries` rows cascade automatically via `ON DELETE CASCADE`. The `IX_ExerciseSessions_CreatedAt` index makes this query efficient.

---

## Future extensions

### Spaced repetition (SM-2 / Leitner)

`EntryKnowledge` is designed to be extended without breaking existing rows. Add new columns with defaults:

```sql
ALTER TABLE wordfolio."EntryKnowledge"
    ADD COLUMN "EasinessFactor" real NOT NULL DEFAULT 2.5,
    ADD COLUMN "Interval" int NOT NULL DEFAULT 0,
    ADD COLUMN "Repetitions" int NOT NULL DEFAULT 0,
    ADD COLUMN "NextReviewAt" datetimeoffset NULL;
```

The `upsertEntryKnowledgeAsync` function would be extended to compute and write these fields alongside the existing counters.

### Per-type attempt metadata

Exercise types that need to record extra structured data (e.g. the chosen distractors in a multiple-choice prompt) can store this in a separate table (e.g. `MultipleChoiceAttemptDetails`) linked by `AttemptId`. The `RawAnswer` column on `ExerciseAttempts` already preserves the user's verbatim input for all types. A separate details table keeps `ExerciseAttempts` schema-stable and avoids JSONB.

### Richer selectors

Additional selector types (e.g. `DueForReview` based on next-review date, `ByTag`) can be added to the `EntrySelector` DU and resolved by new `getXxxEntriesAsync` functions in `DataAccess`, with no changes to the session-creation transaction logic.

### Multiple sessions per entry

The current design allows the same entry to appear in multiple concurrent sessions (no lock or reservation). If exclusive-reservation semantics are needed, an `ActiveSessionEntry` table or a status column on `ExerciseSessionEntries` can be added.

### External caching

If `EntryKnowledge` reads become a hot path, a read-through cache (e.g. Redis) can front the `getEntryKnowledgeAsync` call in `AppEnv`. The domain and data-access layers require no changes.
