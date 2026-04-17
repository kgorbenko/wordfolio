# Exercise Feature – Data Flows

All flows run inside the existing connection/transaction lifecycle managed by `AppEnv`.

---

## Flow 1: Create Session

**Endpoint:** `POST /exercises/sessions`

**Request body:**
```json
{
  "exerciseType": "MultipleChoice",
  "selector": { "type": "WorstKnown", "count": 10 }
}
```

`UserId` is never part of the selector payload; it is always taken from the auth context.

### Steps

```
1. Handler
   ├─ Parse and validate request body
   ├─ Extract UserId from auth context
   └─ Call Operations.createSession env params

2. Operations.createSession
   ├─ Call Capabilities.resolveEntrySelector env userId selector
   │   └─ AppEnv.IResolveEntrySelector
   │       ├─ VocabularyScope v  → verify UserId owns v
   │       │                       DataAccess: SELECT EntryIds WHERE VocabularyId = v
   │       ├─ CollectionScope c  → verify UserId owns c
   │       │                       DataAccess: SELECT EntryIds WHERE CollectionId = c
   │       ├─ WorstKnown scope n → verify UserId owns scope (if scoped)
   │       │                       DataAccess: getWorstKnownEntriesAsync
   │       │                         LEFT JOIN EntryKnowledge ON (UserId, EntryId)
   │       │                         COALESCE hit rate to 0.0 for unseen entries
   │       │                         ORDER BY hit_rate ASC, LastAttemptedAt ASC NULLS FIRST, EntryId ASC
   │       │                         LIMIT n
   │       └─ ExplicitEntries ids → verify all ids belong to UserId
   │                                return ids as-is
   ├─ Validate: resolved list is non-empty → Error CreateSessionError.NoEntriesResolved
   ├─ For each resolved EntryId, generate prompt via Dispatch.generatePrompt exerciseType entry
   ├─ Call Capabilities.createExerciseSession env userId exerciseType resolvedEntries promptDataList now
   │   └─ AppEnv.ICreateExerciseSession (single transaction)
   │       ├─ INSERT INTO ExerciseSessions (UserId, ExerciseType, CreatedAt) → returns SessionId
   │       └─ INSERT INTO ExerciseSessionEntries
   │              one row per (EntryId, DisplayOrder, PromptData) — PromptData is serialised JSON
   └─ Return Ok (SessionBundle)

3. Handler
   └─ 201 Created with session bundle:
      {
        "sessionId": <id>,
        "exerciseType": "<type>",
        "entries": [
          { "entryId": <id>, "displayOrder": <n>, "promptData": { ... } },
          ...
        ]
      }
      Each promptData object includes all client-visible answer/checking data needed to
      evaluate the answer locally (e.g. the question, the options, and the correct answer
      for MultipleChoice; the word and accepted translations for Translation).
```

---

## Flow 2: Resume Session

**Endpoint:** `GET /exercises/sessions/{sessionId}`

Returns the stored session bundle. Used when the client needs to reload or resume a session. The response shape is identical to the `POST /exercises/sessions` response so the client can resume without special handling.

```
1. Handler
   ├─ Parse sessionId from URL
   ├─ Extract UserId from auth context
   └─ Call Operations.getSession env userId sessionId

2. Operations.getSession
   ├─ Load session → verify exists and belongs to UserId
   ├─ Load all ExerciseSessionEntries for sessionId (ordered by DisplayOrder)
   └─ Return Ok (SessionBundle)

3. Handler
   └─ 200 OK with session bundle (same shape as POST /exercises/sessions response)
```

---

## Flow 3: Submit Attempt

**Endpoint:** `POST /exercises/sessions/{sessionId}/entries/{entryId}/attempts`

**Request body:**
```json
{
  "answer": "<raw answer; format is exercise-type-specific, e.g. selected option ID or free text>",
  "isCorrect": true
}
```

`IsCorrect` is evaluated **client-side** using the `PromptData` returned at session creation (or resume). The server does not re-evaluate correctness; it validates ownership and session membership, then persists the attempt using the client-provided value.

### Steps

```
1. Handler
   ├─ Parse sessionId, entryId, rawAnswer, isCorrect from request
   ├─ Extract UserId from auth context
   └─ Call Operations.submitAttempt env userId sessionId entryId rawAnswer isCorrect now

2. Operations.submitAttempt
   ├─ Load session → verify exists and belongs to UserId
   ├─ Verify ExerciseSessionEntry exists for (sessionId, entryId) → entry is in session
   └─ Call Capabilities.commitAttempt env attemptParams
       └─ AppEnv.ICommitAttempt (single transaction)
           │
           ├─ Attempt INSERT:
           │   INSERT INTO ExerciseAttempts
           │       (UserId, SessionId, EntryId, ExerciseType, RawAnswer, IsCorrect, AttemptedAt)
           │   ON CONFLICT (SessionId, EntryId) DO NOTHING
           │   RETURNING Id
           │
           ├─ [Id returned — new row inserted]
           │   ├─ UPSERT EntryKnowledge (UserId, EntryId):
           │   │   INSERT ... ON CONFLICT (UserId, EntryId) DO UPDATE
           │   │       TotalAttempts   = TotalAttempts + 1
           │   │       CorrectAttempts = CorrectAttempts + (1 if correct else 0)
           │   │       LastAttemptedAt = now
           │   └─ Return Inserted (attemptId)
           │
           └─ [No Id returned — conflict]
               ├─ SELECT RawAnswer FROM ExerciseAttempts
               │   WHERE SessionId = ? AND EntryId = ?
               ├─ [RawAnswer matches submitted answer]
               │   └─ Return IdempotentReplay   (no DB write)
               └─ [RawAnswer differs]
                   └─ Return ConflictingReplay  (no DB write)

3. Operations.submitAttempt maps result to:
   │
   ├─ Inserted _         → Ok (AttemptInserted)
   ├─ IdempotentReplay   → Ok (AttemptAlreadyRecorded)
   └─ ConflictingReplay  → Error (ConflictingAttempt)

4. Handler maps to HTTP:
   ├─ AttemptInserted        → 201 Created
   ├─ AttemptAlreadyRecorded → 200 OK
   └─ ConflictingAttempt     → 409 Conflict
```

---

## Flow 4: Unanswered Entries

An entry that the user skips produces **no `ExerciseAttempts` row**. This is the correct behaviour:

- No attempt submission request is sent for that entry.
- `EntryKnowledge` counters are not incremented.
- The entry remains at whatever hit rate it had before.
- Session rows are purged by age regardless of attempt coverage; there is no completion state to reach.

---

## Transaction boundaries

| Operation | Transaction scope |
|---|---|
| Create session | Single transaction covering `INSERT ExerciseSessions` + `INSERT ExerciseSessionEntries` (batch, including `PromptData`); returns full session bundle |
| Submit attempt | Single transaction covering `INSERT ... ON CONFLICT DO NOTHING RETURNING Id` + optional `UPSERT EntryKnowledge` + optional re-read for conflict detection |
| Resume session | Read-only; no transaction required |
