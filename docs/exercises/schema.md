# Exercise Feature – Database Schema

Schema: `wordfolio` (consistent with the rest of the application).

All timestamps use `DateTimeOffset` (never `DateTime`). All PKs are `int identity` wrapped in domain ID types. Table and column names are PascalCase, matching existing tables.

---

## Tables

### ExerciseSessions

Represents one exercise run. Created when a user starts an exercise. Treated as purgeable scaffolding and purged 30 days after `CreatedAt` regardless of completion state — there is no status machine.

| Column | Type | Nullable | Notes |
|---|---|---|---|
| `Id` | `int` | No | PK, identity |
| `UserId` | `int` | No | FK → `Users.Id` (no cascade; do not orphan attempts) |
| `ExerciseType` | `smallint` | No | Discriminator; see [ExerciseType numeric mapping](#exercisetype-numeric-mapping) below |
| `CreatedAt` | `datetimeoffset` | No | Purge anchor; purge when `CreatedAt < NOW() - INTERVAL '30 days'` |

**Indexes:**
- `IX_ExerciseSessions_UserId` on `UserId`
- `IX_ExerciseSessions_CreatedAt` on `CreatedAt` — supports age-based purge queries

---

### ExerciseSessionEntries

The resolved snapshot of entries selected for a session, with the prompt payload persisted at session-creation time. Created once; never updated. FK/cascades from `ExerciseSessions`.

`EntryId` carries a **hard FK to `Entries.Id` with `ON DELETE CASCADE`**. Deleting an entry removes all `ExerciseSessionEntries` rows for that entry, including their stored `PromptData` snapshots.

| Column | Type | Nullable | Notes |
|---|---|---|---|
| `Id` | `int` | No | PK, identity |
| `SessionId` | `int` | No | FK → `ExerciseSessions.Id` ON DELETE CASCADE |
| `EntryId` | `int` | No | FK → `Entries.Id` ON DELETE CASCADE; indexed |
| `DisplayOrder` | `int` | No | Presentation order assigned at resolution time |
| `PromptData` | `text` | No | Serialised prompt payload (JSON). Generated once at session creation; returned as part of the session bundle at creation and by the resume endpoint. |

**Indexes:**
- `IX_ExerciseSessionEntries_SessionId` on `SessionId`
- `IX_ExerciseSessionEntries_EntryId` on `EntryId`
- `UQ_ExerciseSessionEntries_SessionId_EntryId` unique on `(SessionId, EntryId)` — one row per entry per session

---

### ExerciseAttempts

One row per answered entry per session. Kept indefinitely as durable learning history.

**No hard FK to `ExerciseSessions`** — this makes the table self-sufficient after session scaffolding is purged. `EntryId` carries a hard FK to `Entries.Id` with `ON DELETE CASCADE`; deleting an entry removes its attempt history.

| Column | Type | Nullable | Notes |
|---|---|---|---|
| `Id` | `int` | No | PK, identity |
| `UserId` | `int` | No | FK → `Users.Id`; owned directly so history survives session purge |
| `SessionId` | `int` | No | Plain column (no FK); indexed for idempotency checks and traceability |
| `EntryId` | `int` | No | FK → `Entries.Id` ON DELETE CASCADE; indexed |
| `ExerciseType` | `smallint` | No | Denormalised from session; see [ExerciseType numeric mapping](#exercisetype-numeric-mapping) |
| `RawAnswer` | `text` | No | The exact answer string submitted by the client. Used by idempotency check to distinguish true replay from conflicting replay. |
| `IsCorrect` | `bool` | No | Client-provided at submit time; persisted as-is |
| `AttemptedAt` | `datetimeoffset` | No | |

**Indexes:**
- `UQ_ExerciseAttempts_SessionId_EntryId` unique on `(SessionId, EntryId)` — idempotency key
- `IX_ExerciseAttempts_UserId_EntryId` on `(UserId, EntryId)` — supports knowledge-history queries
- `IX_ExerciseAttempts_SessionId` on `SessionId` — supports traceability lookups
- `IX_ExerciseAttempts_EntryId` on `EntryId`

**Idempotency contract (with race handling):**

Use `INSERT ... ON CONFLICT (SessionId, EntryId) DO NOTHING RETURNING Id` to attempt the insert atomically.

- **Row returned (new insert):** proceed to upsert `EntryKnowledge`. Return `AttemptInserted`.
- **No row returned (conflict):** re-read the existing row by `(SessionId, EntryId)`. Compare `RawAnswer`:
  - Same `RawAnswer` → idempotent replay; return `AttemptAlreadyRecorded` (no further write).
  - Different `RawAnswer` → conflicting replay; return `ConflictingReplay`.

This pattern is safe under concurrent requests: the unique index prevents double-inserts and the re-read path resolves the winner deterministically without a second write.

---

### EntryKnowledge

One row per `(UserId, EntryId)` pair. Kept indefinitely. Updated atomically alongside each `ExerciseAttempts` insert.

`EntryId` carries a **hard FK to `Entries.Id` with `ON DELETE CASCADE`**. Deleting an entry removes its knowledge counters.

| Column | Type | Nullable | Notes |
|---|---|---|---|
| `UserId` | `int` | No | PK (composite) |
| `EntryId` | `int` | No | PK (composite); FK → `Entries.Id` ON DELETE CASCADE |
| `TotalAttempts` | `int` | No | Incremented on every new attempt |
| `CorrectAttempts` | `int` | No | Incremented on correct attempts |
| `LastAttemptedAt` | `datetimeoffset` | No | Updated on every new attempt |

**Derived values (not stored):**
- Hit rate = `CorrectAttempts / TotalAttempts` — computed at query time. Entries with no `EntryKnowledge` row are treated as hit rate `0.0` (COALESCE to zero in queries).

**Future columns (deferred):**
- SM-2 easiness factor, interval, repetition count, next review date — add when spaced-repetition is introduced.

**Indexes:**
- PK is `(UserId, EntryId)`.
- `IX_EntryKnowledge_UserId` on `UserId` — supports worst-known-entry selector with LEFT JOIN.

---

## ExerciseType numeric mapping

`ExerciseType` is stored as `smallint` in `ExerciseSessions` and `ExerciseAttempts`. The mapping is stable and must not be changed once data exists:

| Value | `ExerciseType` DU case |
|---|---|
| `0` | `MultipleChoice` |
| `1` | `Translation` |

The domain model keeps `ExerciseType` as a discriminated union. The data access layer maps the DU to `int16` before writing and maps `int16` back to the DU after reading. New exercise types append to this table; existing values are immutable.

---

## EntryId FK policy

`ExerciseSessionEntries`, `ExerciseAttempts`, and `EntryKnowledge` all carry `EntryId FK → Entries.Id ON DELETE CASCADE`. Rationale:

- Deleting an entry removes all associated session context, attempt history, and knowledge counters for that entry. This is the correct semantics: a user who removes a vocabulary entry intends to stop practising it.
- Orphaned rows from deleted entries would silently pollute `WorstKnown` results and knowledge summaries.

`ExerciseAttempts.SessionId` is **not** a hard FK to `ExerciseSessions`. `ExerciseSessions` is purgeable scaffolding; purging sessions must not cascade-delete attempt history. See the next section.

---

## No hard FK from ExerciseAttempts to ExerciseSessions

`ExerciseSessions` is purgeable scaffolding. Purging sessions must not cascade-delete attempt history. Instead:

- `SessionId` is stored as a plain `int` column with an index.
- `UserId` is stored directly on `ExerciseAttempts` so ownership is unambiguous after the session row is gone.
- `ExerciseType` is denormalised onto `ExerciseAttempts` so per-type reporting does not require a join to a potentially purged session.

---

## Selector model (not a persisted table)

Selectors are request-time input only, resolved at session creation and never stored as structured data.

```
type EntrySelector =
    | VocabularyScope  of VocabularyId
    | CollectionScope  of CollectionId
    | WorstKnown       of scope: WorstKnownScope * count: int
    | ExplicitEntries  of EntryId list

type WorstKnownScope =
    | AllUserEntries
    | WithinVocabulary of VocabularyId
    | WithinCollection of CollectionId
```

`UserId` is **not** part of the selector payload; it is always taken from the auth context.

**WorstKnown resolution** uses a LEFT JOIN between the candidate entries and `EntryKnowledge` so that entries with no knowledge row (cold/unseen entries) are included with a coalesced hit rate of `0.0`. Ordering: hit rate ASC, then `LastAttemptedAt ASC NULLS FIRST`, then `EntryId ASC` (stable tiebreak).

**Ownership validation** is required for all selector types before resolution proceeds:
- `VocabularyScope`: verify `UserId` owns the vocabulary.
- `CollectionScope`: verify `UserId` owns the collection.
- `WorstKnown` with a scoped variant: verify `UserId` owns the scope vocabulary/collection.
- `ExplicitEntries`: verify all `EntryId` values belong to `UserId`.

Resolution expands the selector into a concrete `EntryId list`, which is then written as `ExerciseSessionEntries` rows. After that point the selector is discarded.

---

## Migration numbering

Follow the existing convention: `YYYYMMDDNNN_<Description>.fs`. Four migrations are anticipated:

1. `CreateExerciseSessionsTable`
2. `CreateExerciseSessionEntriesTable`
3. `CreateExerciseAttemptsTable`
4. `CreateEntryKnowledgeTable`
