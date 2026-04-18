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
   ├─ Pre-DB size validation (400 Bad Request if oversized):
   │   ├─ ExplicitEntries with length > MaxSessionEntries (= 10) → 400
   │   └─ WorstKnown with count > MaxSessionEntries (= 10) → 400
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
   │       │                         windowed CTE over last @knowledgeWindowSize (= 10)
   │       │                         ExerciseAttempts rows per (UserId, EntryId)
   │       │                         LEFT JOIN so cold entries (no attempts) included
   │       │                         COALESCE windowed hit rate to 0.0 for cold entries
   │       │                         tie-break: LastAttemptedAt derived from ExerciseAttempts ASC NULLS FIRST
   │       │                         stable tiebreak: EntryId ASC
   │       │                         LIMIT n
   │       └─ ExplicitEntries ids → verify all ids belong to UserId
   │                                return ids as-is
   ├─ Validate: resolved list is non-empty → Error CreateSessionError.NoEntriesResolved
   ├─ Cap resolved list at MaxSessionEntries (= 10)
   ├─ Batch-load all resolved entries in one query
   │   DataAccess: SELECT * FROM Entries WHERE Id IN (<resolved ids>)
   ├─ For each resolved Entry:
   │   └─ Call Dispatch.generatePrompt exerciseType entry → GeneratedPrompt { PromptData; PromptSchemaVersion }
   ├─ Call Capabilities.createExerciseSession env (CreateExerciseSessionData { ... }) now
   │   └─ AppEnv.ICreateExerciseSession (single transaction / snapshot)
   │       ├─ INSERT INTO ExerciseSessions (UserId, ExerciseType, CreatedAt) → returns SessionId
   │       └─ INSERT INTO ExerciseSessionEntries (batch)
   │              one row per (EntryId, DisplayOrder, PromptData, PromptSchemaVersion)
   └─ Return Ok (SessionBundle)

3. Handler
   └─ 201 Created with session bundle:
      {
        "sessionId": <id>,
        "exerciseType": "<type>",
        "entries": [
          {
            "entryId": <id>,
            "displayOrder": <n>,
            "promptData": { ... },
            "attempt": null
          },
          ...
        ]
      }
      Each promptData object includes all client-visible answer/checking data.
      attempt is null for a freshly created session.
```

---

## Flow 2: Resume Session

**Endpoint:** `GET /exercises/sessions/{sessionId}`

Returns the stored session bundle enriched with per-entry attempt metadata. Used when the client needs to reload or resume a session. The response shape is a superset of the `POST /exercises/sessions` response: each entry includes an `attempt` field (`null` if not yet answered, or `{ rawAnswer, isCorrect, attemptedAt }` if answered).

```
1. Handler
   ├─ Parse sessionId from URL
   ├─ Extract UserId from auth context
   └─ Call Operations.getSession env userId sessionId

2. Operations.getSession
   ├─ Load session → verify exists and belongs to UserId
   ├─ Load all ExerciseSessionEntries for sessionId (ordered by DisplayOrder)
   ├─ Load all ExerciseAttempts for (sessionId, userId) — keyed by EntryId
   └─ Assemble SessionBundle: for each entry, attach attempt option
         (attempt = None if no row in ExerciseAttempts for this EntryId)
   └─ Return Ok (SessionBundle)

3. Handler
   └─ 200 OK with session bundle:
      {
        "sessionId": <id>,
        "exerciseType": "<type>",
        "entries": [
          {
            "entryId": <id>,
            "displayOrder": <n>,
            "promptData": { ... },
            "attempt": {
              "rawAnswer": "<raw answer>",
              "isCorrect": true,
              "attemptedAt": "<ISO 8601>"
            }
          },
          {
            "entryId": <id>,
            "displayOrder": <n>,
            "promptData": { ... },
            "attempt": null
          },
          ...
        ]
      }
```

---

## Flow 3: Submit Attempt

**Endpoint:** `POST /exercises/sessions/{sessionId}/entries/{entryId}/attempts`

**Request body:**
```json
{
  "rawAnswer": "<raw answer; format is exercise-type-specific, e.g. selected option ID or free text>"
}
```

`IsCorrect` is **not** supplied by the client. The server evaluates correctness using `Dispatch.evaluate` against the stored `PromptData`, persists the authoritative result, and returns it in the response. `AttemptedAt` is always server-generated.

### Steps

```
1. Handler
   ├─ Parse sessionId, entryId, rawAnswer from request
   ├─ Extract UserId from auth context
   └─ Call Operations.submitAttempt env userId sessionId entryId rawAnswer now

2. Operations.submitAttempt
   ├─ Load session → verify exists and belongs to UserId
   ├─ Verify ExerciseSessionEntry exists for (sessionId, entryId) → entry is in session
   │   (the loaded ExerciseSessionEntry.PromptData and PromptSchemaVersion are used next)
   ├─ Call Dispatch.evaluate exerciseType promptSchemaVersion promptData rawAnswer
   │       → Result<bool, EvaluateError>
   │   ├─ Ok isCorrect  → continue
   │   └─ Error _       → propagate EvaluateError (handler returns 500)
   └─ Call Capabilities.commitAttempt env (CommitAttemptData { ... })
       └─ AppEnv.ICommitAttempt (single transaction)
           │
           ├─ Attempt INSERT:
           │   INSERT INTO ExerciseAttempts
           │       (UserId, SessionId, EntryId, ExerciseType,
           │        PromptData, PromptSchemaVersion, RawAnswer, IsCorrect, AttemptedAt)
           │   VALUES (..., @promptData, @promptSchemaVersion, @rawAnswer, @isCorrect, @now)
           │   ON CONFLICT (SessionId, EntryId) DO NOTHING
           │   RETURNING Id
           │
           ├─ [Id returned — new row inserted]
           │   └─ Return Inserted (AttemptInserted { AttemptId; IsCorrect })
           │
           └─ [No Id returned — conflict]
               ├─ SELECT RawAnswer FROM ExerciseAttempts
               │   WHERE SessionId = ? AND EntryId = ?
               ├─ [RawAnswer matches submitted answer]
               │   └─ Return IdempotentReplay (AttemptAlreadyRecorded { IsCorrect })
               └─ [RawAnswer differs]
                   └─ Return ConflictingReplay  (no DB write)

3. Operations.submitAttempt maps result to:
   │
   ├─ Inserted (AttemptInserted { IsCorrect })        → Ok (AttemptInserted { AttemptId; IsCorrect })
   ├─ IdempotentReplay (AttemptAlreadyRecorded { IsCorrect }) → Ok (AttemptAlreadyRecorded { IsCorrect })
   └─ ConflictingReplay                               → Error (ConflictingAttempt)

4. Handler maps to HTTP:
   ├─ AttemptInserted { isCorrect }        → 201 Created  { "isCorrect": <bool> }
   ├─ AttemptAlreadyRecorded { isCorrect } → 200 OK       { "isCorrect": <bool> }
   ├─ ConflictingAttempt                   → 409 Conflict
   └─ EvaluateError (from Dispatch.evaluate) → 500 Internal Server Error
```

---

## Flow 4: Unanswered Entries

An entry that the user skips produces **no `ExerciseAttempts` row**. This is the correct behaviour:

- No attempt submission request is sent for that entry.
- The entry remains at whatever hit rate it had before.
- Session rows are purged by age regardless of attempt coverage; there is no completion state to reach.
- On resume, the entry’s `attempt` field in the bundle is `null`.

---

## Transaction boundaries

| Operation | Transaction scope |
|---|---|
| Create session | Single transaction covering `INSERT ExerciseSessions` + `INSERT ExerciseSessionEntries` (batch, including `PromptData` and `PromptSchemaVersion`); all entries loaded in one batch query before the transaction; returns full session bundle |
| Submit attempt | Single transaction covering `INSERT ... ON CONFLICT DO NOTHING RETURNING Id` + optional re-read for conflict detection |
| Resume session | Read-only; no transaction required |
