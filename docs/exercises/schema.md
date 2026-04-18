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
| `UserId` | `int` | No | FK → `Users.Id` (no cascade; see UserId FK policy below) |
| `ExerciseType` | `smallint` | No | Discriminator; see [ExerciseType numeric mapping](#exercisetype-numeric-mapping) below |
| `CreatedAt` | `datetimeoffset` | No | Purge anchor; purge when `CreatedAt < NOW() - INTERVAL '30 days'` |

**Indexes:**
- `IX_ExerciseSessions_UserId` on `UserId`
- `IX_ExerciseSessions_CreatedAt` on `CreatedAt` — supports age-based purge queries

---

### ExerciseSessionEntries

The resolved snapshot of entries selected for a session, with the prompt payload persisted at session-creation time. Created once; never updated. FK/cascades from `ExerciseSessions`.

`EntryId` carries a **hard FK to `Entries.Id` with `ON DELETE CASCADE`**. Deleting an entry removes all `ExerciseSessionEntries` rows for that entry, including their stored `PromptData` snapshots.

Session creation is capped at `MaxSessionEntries = 10` entries.

| Column | Type | Nullable | Notes |
|---|---|---|---|
| `Id` | `int` | No | PK, identity |
| `SessionId` | `int` | No | FK → `ExerciseSessions.Id` ON DELETE CASCADE |
| `EntryId` | `int` | No | FK → `Entries.Id` ON DELETE CASCADE; indexed |
| `DisplayOrder` | `int` | No | Presentation order assigned at resolution time |
| `PromptData` | `text` | No | Serialised prompt payload (JSON). Generated once at session creation; returned as part of the session bundle at creation and by the resume endpoint. |
| `PromptSchemaVersion` | `smallint` | No | Version of the `PromptData` JSON schema. Used for forward-compatible deserialisation. |

**Indexes:**
- `IX_ExerciseSessionEntries_SessionId` on `SessionId`
- `IX_ExerciseSessionEntries_EntryId` on `EntryId`
- `UQ_ExerciseSessionEntries_SessionId_EntryId` unique on `(SessionId, EntryId)` — one row per entry per session

---

### ExerciseAttempts

One row per answered entry per session. Kept indefinitely as durable learning history.

**No hard FK to `ExerciseSessions`** — this makes the table self-sufficient after session scaffolding is purged. `SessionId` is a **nullable** `int`; the purge job nulls it before deleting the session row so attempt history survives with `SessionId = NULL`. Post-purge, `SessionId` being absent on an attempt is intentional and expected. `EntryId` carries a hard FK to `Entries.Id` with `ON DELETE CASCADE`; deleting an entry removes its attempt history.

`PromptData` is copied from `ExerciseSessionEntries.PromptData` at submit time. This makes each attempt row fully self-describing: even after the parent `ExerciseSessions` / `ExerciseSessionEntries` rows are purged, the attempt record retains the original prompt context for analytics and audit without requiring a separate durable prompt table.

`IsCorrect` is **server-generated**: the server evaluates the submitted `RawAnswer` against the stored `PromptData` using `Dispatch.evaluate`. The client does not supply `IsCorrect`. `AttemptedAt` is always server-generated.

| Column | Type | Nullable | Notes |
|---|---|---|---|
| `Id` | `int` | No | PK, identity |
| `UserId` | `int` | No | FK → `Users.Id` (no cascade; see UserId FK policy below); owned directly so history survives session purge |
| `SessionId` | `int` | **Yes** | Nullable; no FK. Indexed for idempotency checks and traceability. Nulled by the purge job before the session row is deleted. |
| `EntryId` | `int` | No | FK → `Entries.Id` ON DELETE CASCADE; indexed |
| `ExerciseType` | `smallint` | No | Denormalised from session; see [ExerciseType numeric mapping](#exercisetype-numeric-mapping) |
| `PromptData` | `text` | No | Copied from `ExerciseSessionEntries.PromptData` at submit time. Survives session purge; makes the attempt self-describing. |
| `PromptSchemaVersion` | `smallint` | No | Version of the `PromptData` JSON schema copied from the session entry at submit time. |
| `RawAnswer` | `text` | No | The exact answer string submitted by the client. Used by idempotency check to distinguish true replay from conflicting replay. |
| `IsCorrect` | `bool` | No | Server-evaluated at submit time via `Dispatch.evaluate`. |
| `AttemptedAt` | `datetimeoffset` | No | Server-generated at submit time; the client does not supply this value. |

**Indexes:**
- `UQ_ExerciseAttempts_SessionId_EntryId` unique on `(SessionId, EntryId)` — idempotency key
- `IX_ExerciseAttempts_UserId_EntryId_AttemptedAt` on `(UserId, EntryId, AttemptedAt DESC)` — supports windowed knowledge-score queries (last N attempts per entry)
- `IX_ExerciseAttempts_SessionId` on `SessionId` — supports traceability lookups
- `IX_ExerciseAttempts_EntryId` on `EntryId`

**Idempotency contract (with race handling):**

Use `INSERT ... ON CONFLICT (SessionId, EntryId) DO NOTHING RETURNING Id` to attempt the insert atomically.

- **Row returned (new insert):** return `AttemptInserted`.
- **No row returned (conflict):** re-read the existing row by `(SessionId, EntryId)`. Compare `RawAnswer`:
  - Same `RawAnswer` → idempotent replay; return `AttemptAlreadyRecorded` (no further write).
  - Different `RawAnswer` → conflicting replay; return `ConflictingReplay`.

This pattern is safe under concurrent requests: the unique index prevents double-inserts and the re-read path resolves the winner deterministically without a second write.

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

`ExerciseSessionEntries` and `ExerciseAttempts` carry `EntryId FK → Entries.Id ON DELETE CASCADE`. Rationale:

- Deleting an entry removes all associated session context and attempt history for that entry. This is the correct semantics: a user who removes a vocabulary entry intends to stop practising it.
- Orphaned rows from deleted entries would silently pollute `WorstKnown` results and knowledge summaries.

`ExerciseAttempts.SessionId` is **not** a hard FK to `ExerciseSessions`. `ExerciseSessions` is purgeable scaffolding; the purge job nulls `SessionId` on all related attempts before deleting the session row. See the next section.

---

## UserId FK policy and account-deletion

`ExerciseSessions.UserId` and `ExerciseAttempts.UserId` carry hard FKs to `Users.Id` with **no cascade**. Rationale:

- Cascading user deletion would silently destroy the user’s entire learning history. This is not the intended behaviour; account deletion is an explicit, multi-step operation.
- An explicit account-deletion flow must remove `ExerciseAttempts` rows (and `ExerciseSessions` rows) via application logic before the `Users` row is deleted. The no-cascade constraint enforces that this sequence is followed.

---

## No hard FK from ExerciseAttempts to ExerciseSessions

`ExerciseSessions` is purgeable scaffolding. Purging sessions must not cascade-delete attempt history. Instead:

- `SessionId` is stored as a **nullable** `int` column with an index.
- Before deleting a session row, the purge job sets `ExerciseAttempts.SessionId = NULL` for all attempts belonging to that session. Post-purge, `SessionId` being `NULL` on an attempt is intentional and expected — it signals that the originating session has been purged.
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

The resolved list is capped at `MaxSessionEntries = 10` before `ExerciseSessionEntries` rows are created.

**Pre-DB size validation:** The handler rejects oversize selector requests with `400 Bad Request` before any DB access is attempted:
- `ExplicitEntries` with more than `MaxSessionEntries` IDs → `400 Bad Request`.
- `WorstKnown` with `count > MaxSessionEntries` → `400 Bad Request`.

Ownership validation in `IResolveEntrySelector` is performed only for requests that pass size validation.

**WorstKnown resolution** uses a windowed CTE over `ExerciseAttempts` only. There is no `EntryKnowledge` table.

```sql
WITH ranked_attempts AS (
    SELECT EntryId, IsCorrect,
           ROW_NUMBER() OVER (PARTITION BY EntryId ORDER BY AttemptedAt DESC) AS rn
    FROM wordfolio."ExerciseAttempts"
    WHERE UserId = @userId
      AND EntryId IN (<scoped_entry_ids>)
),
windowed_scores AS (
    SELECT EntryId,
           SUM(CASE WHEN IsCorrect THEN 1.0 ELSE 0.0 END) / COUNT(*) AS hit_rate
    FROM ranked_attempts
    WHERE rn <= @knowledgeWindowSize
    GROUP BY EntryId
)
SELECT e.Id
FROM <scoped_entries> e
LEFT JOIN windowed_scores ws ON ws.EntryId = e.Id
LEFT JOIN (
    SELECT EntryId, MAX(AttemptedAt) AS LastAttemptedAt
    FROM wordfolio."ExerciseAttempts"
    WHERE UserId = @userId
    GROUP BY EntryId
) last_att ON last_att.EntryId = e.Id
ORDER BY
    COALESCE(ws.hit_rate, 0.0) ASC,           -- cold entries (NULL) rank first
    last_att.LastAttemptedAt ASC NULLS FIRST,  -- least recently attempted breaks ties
    e.Id ASC                                   -- stable tiebreak
LIMIT @count
```

`@knowledgeWindowSize` corresponds to the named constant `KnowledgeWindowSize = 10` in the codebase.

Cold-entry inclusion: the LEFT JOIN against `windowed_scores` ensures entries with no `ExerciseAttempts` rows produce `NULL` hit rate, which `COALESCE` maps to `0.0`. Cold entries therefore rank before any attempted entry with a non-zero hit rate.

Tie-breaking: a subquery over `ExerciseAttempts` derives `LastAttemptedAt` directly from attempt rows (no `EntryKnowledge` table). Entries with no attempts have `NULL LastAttemptedAt`, placing them before any attempted entry at the same hit rate.

**Ownership validation** is required for all selector types before resolution proceeds:
- `VocabularyScope`: verify `UserId` owns the vocabulary.
- `CollectionScope`: verify `UserId` owns the collection.
- `WorstKnown` with a scoped variant: verify `UserId` owns the scope vocabulary/collection.
- `ExplicitEntries`: verify all `EntryId` values belong to `UserId`.

Resolution expands the selector into a concrete `EntryId list`, which is then capped at `MaxSessionEntries = 10` and written as `ExerciseSessionEntries` rows. After that point the selector is discarded.

---

## Migration numbering

Follow the existing convention: `YYYYMMDDNNN_<Description>.fs`. Three migrations are anticipated:

1. `CreateExerciseSessionsTable`
2. `CreateExerciseSessionEntriesTable`
3. `CreateExerciseAttemptsTable`
