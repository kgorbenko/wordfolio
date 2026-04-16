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

### Steps

```
1. Handler
   ├─ Parse and validate request body
   ├─ Extract UserId from auth context
   └─ Call Operations.createSession env params

2. Operations.createSession
   ├─ Call Capabilities.resolveEntrySelector env userId selector
   │   └─ AppEnv.IResolveEntrySelector
   │       ├─ VocabularyScope  → DataAccess: SELECT EntryIds WHERE VocabularyId = ?
   │       ├─ CollectionScope  → DataAccess: SELECT EntryIds WHERE CollectionId = ?
   │       ├─ WorstKnown       → DataAccess: getWorstKnownEntriesAsync (ORDER BY hit rate ASC)
   │       └─ ExplicitEntries  → return as-is (validate all entries belong to user)
   ├─ Validate: resolved list is non-empty → Error CreateSessionError.NoEntriesResolved
   ├─ Call Capabilities.createExerciseSession env userId exerciseType resolvedEntries now
   │   └─ AppEnv.ICreateExerciseSession (single transaction)
   │       ├─ INSERT INTO ExerciseSessions → returns SessionId (int)
   │       └─ INSERT INTO ExerciseSessionEntries (one row per EntryId, with DisplayOrder)
   └─ Return Ok (ExerciseSessionId)

3. Handler
   └─ 201 Created with { "sessionId": <id>, "entryCount": <n> }
```

---

## Flow 2: Get Prompt for an Entry

**Endpoint:** `GET /exercises/sessions/{sessionId}/entries/{entryId}/prompt`

```
1. Handler
   ├─ Parse sessionId, entryId from URL
   ├─ Extract UserId from auth context
   └─ Call Operations.getSession env userId sessionId

2. Operations.getSession
   ├─ Load session → verify exists and belongs to user
   ├─ Verify entryId is in ExerciseSessionEntries for this session
   ├─ Load Entry data (text, definitions, etc.)
   ├─ Load EntryKnowledge option for (userId, entryId)
   └─ Return Ok (session, entry, knowledge option)

3. Handler
   ├─ Dispatch to exercise-type module:
   │   MultipleChoice.generatePrompt entry knowledge
   │   Translation.generatePrompt entry knowledge
   └─ 200 OK with prompt payload
```

---

## Flow 3: Submit Attempt

**Endpoint:** `POST /exercises/sessions/{sessionId}/entries/{entryId}/attempts`

**Request body:**
```json
{ "answer": "<raw answer; format is exercise-type-specific, e.g. selected option ID or free text>" }
```

> Correctness is evaluated **server-side**. The client submits its raw answer; the handler dispatches to the appropriate exercise-type module's `evaluate` function to compute `isCorrect` before recording the attempt.

### Steps

```
1. Handler
   ├─ Parse sessionId, entryId, answer from request
   ├─ Extract UserId from auth context
   ├─ Load session → determine ExerciseType and verify session is Active
   ├─ Load entry data and EntryKnowledge option (same data as generatePrompt)
   ├─ Dispatch to exercise-type module:
   │   MultipleChoice.evaluate prompt answer  → isCorrect
   │   Translation.evaluate    prompt answer  → isCorrect
   └─ Call Operations.submitAttempt env params (with computed isCorrect)

2. Operations.submitAttempt
   ├─ Load session → verify exists, belongs to user, status = Active
   ├─ Verify entryId is in ExerciseSessionEntries for this session
   └─ Call Capabilities.commitAttempt env attemptParams
       └─ AppEnv.ICommitAttempt (single transaction)
           ├─ SELECT * FROM ExerciseAttempts WHERE SessionId = ? AND EntryId = ?
           │
           ├─ [No existing row]
           │   ├─ INSERT INTO ExerciseAttempts (UserId, SessionId, EntryId, ExerciseType, IsCorrect, AttemptedAt)
           │   ├─ UPSERT EntryKnowledge (UserId, EntryId):
           │   │   INSERT ... ON CONFLICT DO UPDATE
           │   │       TotalAttempts   = TotalAttempts + 1
           │   │       CorrectAttempts = CorrectAttempts + (1 if correct else 0)
           │   │       LastAttemptedAt = NOW()
           │   └─ Return Inserted (attemptId)
           │
           ├─ [Existing row, same IsCorrect]
           │   └─ Return IdempotentReplay   (no DB write)
           │
           └─ [Existing row, different IsCorrect]
               └─ Return ConflictingReplay  (no DB write)

3. Operations.submitAttempt maps result to:
   │
   ├─ Inserted _         → Ok (AttemptInserted)
   ├─ IdempotentReplay   → Ok (AttemptAlreadyRecorded)
   └─ ConflictingReplay  → Error (ConflictingAttempt)

4. Handler maps to HTTP:
   ├─ AttemptInserted       → 201 Created
   ├─ AttemptAlreadyRecorded→ 200 OK
   └─ ConflictingAttempt    → 409 Conflict
```

---

## Flow 4: Unanswered Entries

An entry that the user skips produces **no `ExerciseAttempts` row**. This is the correct behaviour:

- No attempt submission request is sent for that entry.
- `EntryKnowledge` counters are not incremented.
- The entry remains at whatever hit rate it had before.
- The absence of an attempt row does not affect session completion logic (the session is marked `Completed` or `Abandoned` independently, not based on attempt coverage).

---

## Transaction boundaries

| Operation | Transaction scope |
|---|---|
| Create session | Single transaction covering `INSERT ExerciseSessions` + `INSERT ExerciseSessionEntries` (batch) |
| Submit attempt | Single transaction covering `SELECT` (idempotency check) + `INSERT ExerciseAttempts` + `UPSERT EntryKnowledge` |
| Get session / get prompt | Read-only; no transaction required |
