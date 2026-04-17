# Exercise Feature – Visual Diagrams

---

## 1. Database Schema (ER Diagram)

```mermaid
erDiagram
    Users {
        int Id PK
    }

    ExerciseSessions {
        int Id PK
        int UserId FK
        smallint ExerciseType
        datetimeoffset CreatedAt
    }

    ExerciseSessionEntries {
        int Id PK
        int SessionId FK
        int EntryId "FK → Entries.Id CASCADE"
        int DisplayOrder
        text PromptData
    }

    ExerciseAttempts {
        int Id PK
        int UserId FK
        int SessionId "plain int — no FK"
        int EntryId "FK → Entries.Id CASCADE"
        smallint ExerciseType
        text RawAnswer
        bool IsCorrect
        datetimeoffset AttemptedAt
    }

    EntryKnowledge {
        int UserId PK
        int EntryId PK "FK → Entries.Id CASCADE"
        int TotalAttempts
        int CorrectAttempts
        datetimeoffset LastAttemptedAt
    }

    Entries {
        int Id PK
    }

    Users ||--o{ ExerciseSessions : "owns"
    ExerciseSessions ||--o{ ExerciseSessionEntries : "CASCADE DELETE"
    Entries ||--o{ ExerciseSessionEntries : "CASCADE DELETE"
    Entries ||--o{ ExerciseAttempts : "CASCADE DELETE"
    Entries ||--o{ EntryKnowledge : "CASCADE DELETE"
    Users ||--o{ ExerciseAttempts : "owns (durable)"
    Users ||--o{ EntryKnowledge : "owns (durable)"
```

> **FK notes:** `ExerciseAttempts.SessionId` is a plain indexed `int` with no FK to `ExerciseSessions` — session purge does not cascade-delete attempt history. `ExerciseSessionEntries.EntryId`, `ExerciseAttempts.EntryId`, and `EntryKnowledge.EntryId` carry hard FKs to `Entries.Id ON DELETE CASCADE` — deleting an entry removes all associated session context, attempt history, and knowledge counters.

---

## 2. Data Retention Tiers

```mermaid
graph TD
    subgraph Tier1["Tier 1 — Durable History (keep indefinitely)"]
        EA[ExerciseAttempts]
        EK[EntryKnowledge]
    end

    subgraph Tier2["Tier 2 — Purgeable Scaffolding (purge after 30 days)"]
        ES[ExerciseSessions]
        ESE[ExerciseSessionEntries]
    end

    Entries[Entries]

    ES -- "ON DELETE CASCADE" --> ESE
    ES -. "SessionId = plain int (no FK)" .-> EA
    Entries -- "EntryId CASCADE" --> ESE
    Entries -- "EntryId CASCADE" --> EA
    Entries -- "EntryId CASCADE" --> EK

    Purge["Background job:\nDELETE WHERE CreatedAt < NOW() - 30 days"] --> ES
```

---

## 3. Layered Architecture

```mermaid
graph TD
    HTTP["HTTP Request"]

    subgraph Handlers["Handlers — Api/Exercises/"]
        CH[CreateSessionHandler]
        GSH[GetSessionHandler]
        SAH[SubmitAttemptHandler]
    end

    subgraph Domain["Domain — Domain/Exercises/"]
        OPS["Operations.fs\n(createSession · getSession · submitAttempt)"]
        CAPS["Capabilities.fs\n(IResolveEntrySelector · ICreateExerciseSession\nIGetExerciseSession · IGetSessionBundle\nICommitAttempt · IGetEntryKnowledge)"]
        DISP["Dispatch.fs\n(generatePrompt)"]
        MC["MultipleChoice module"]
        TR["Translation module"]
    end

    subgraph AppEnv["AppEnv — Infrastructure/Environment.fs"]
        ENV["Interface implementations\n(thin mapping layer, no business logic)"]
    end

    subgraph DataAccess["DataAccess"]
        ESDA["ExerciseSessions.fs"]
        EADA["ExerciseAttempts.fs"]
        EKDA["EntryKnowledge.fs"]
    end

    PG[(PostgreSQL)]

    HTTP --> Handlers
    Handlers --> OPS
    OPS --> CAPS
    OPS --> DISP
    DISP --> MC
    DISP --> TR
    CAPS --> ENV
    ENV --> ESDA
    ENV --> EADA
    ENV --> EKDA
    ESDA --> PG
    EADA --> PG
    EKDA --> PG
```

---

## 4. Flow 1 — Create Session

```mermaid
sequenceDiagram
    participant Client
    participant Handler as CreateSessionHandler
    participant Ops as Operations.createSession
    participant Sel as IResolveEntrySelector (AppEnv)
    participant DA_ES as ExerciseSessions DA
    participant Dispatch as Dispatch.generatePrompt
    participant PG as PostgreSQL

    Client->>Handler: POST /exercises/sessions { exerciseType, selector }
    Handler->>Ops: createSession env params (userId from auth)

    Ops->>Sel: ResolveEntrySelector userId selector
    Sel->>PG: validate ownership + fetch EntryId list
    PG-->>Sel: EntryId list (or error)
    Sel-->>Ops: Ok (EntryId list) | Error SelectorError

    Ops->>Ops: validate list is non-empty

    loop for each EntryId
        Ops->>Dispatch: generatePrompt exerciseType entry entryKnowledge
        Dispatch-->>Ops: PromptData (serialised JSON)
    end

    Ops->>DA_ES: createSessionWithEntriesAsync (single transaction)
    DA_ES->>PG: INSERT ExerciseSessions → SessionId
    DA_ES->>PG: INSERT ExerciseSessionEntries (one row per entry, with PromptData)
    PG-->>DA_ES: done
    DA_ES-->>Ops: ExerciseSessionId

    Ops-->>Handler: Ok SessionBundle
    Handler-->>Client: 201 Created { sessionId, exerciseType, entries: [{ entryId, displayOrder, promptData }, ...] }
```

---

## 5. Flow 2 — Resume Session

```mermaid
sequenceDiagram
    participant Client
    participant Handler as GetSessionHandler
    participant Ops as Operations.getSession
    participant DA as ExerciseSessions DA
    participant PG as PostgreSQL

    Client->>Handler: GET /exercises/sessions/{sessionId}
    Handler->>Ops: getSession env userId sessionId

    Ops->>DA: getSessionAsync sessionId
    DA->>PG: SELECT ExerciseSessions WHERE Id = sessionId
    PG-->>DA: ExerciseSession option
    DA-->>Ops: ExerciseSession option
    Ops->>Ops: verify session exists + UserId matches

    Ops->>DA: getSessionEntriesAsync sessionId
    DA->>PG: SELECT ExerciseSessionEntries WHERE SessionId = ? ORDER BY DisplayOrder
    PG-->>DA: ExerciseSessionEntry list (includes PromptData)
    DA-->>Ops: ExerciseSessionEntry list

    Ops-->>Handler: Ok SessionBundle (stored JSON — no re-generation)
    Handler-->>Client: 200 OK { sessionId, exerciseType, entries: [{ entryId, displayOrder, promptData }, ...] }
```

---

## 6. Flow 3 — Submit Attempt (with idempotency)

```mermaid
sequenceDiagram
    participant Client
    participant Handler as SubmitAttemptHandler
    participant Ops as Operations.submitAttempt
    participant DA_Att as ExerciseAttempts DA
    participant DA_EK as EntryKnowledge DA
    participant PG as PostgreSQL

    Client->>Handler: POST /exercises/sessions/{sessionId}/entries/{entryId}/attempts { answer, isCorrect }
    Handler->>Ops: submitAttempt env userId sessionId entryId rawAnswer isCorrect now

    Ops->>PG: getSessionAsync → verify ownership
    Ops->>PG: getSessionEntryAsync → verify entry is in session

    Ops->>DA_Att: commitAttemptAsync params (single transaction)

    DA_Att->>PG: INSERT ExerciseAttempts ON CONFLICT (SessionId,EntryId) DO NOTHING RETURNING Id

    alt New row inserted (Id returned)
        PG-->>DA_Att: Id
        DA_Att->>DA_EK: upsertEntryKnowledgeAsync
        DA_EK->>PG: INSERT EntryKnowledge ON CONFLICT DO UPDATE (TotalAttempts++, etc.)
        PG-->>DA_EK: done
        DA_Att-->>Ops: Inserted attemptId
        Ops-->>Handler: Ok AttemptInserted
        Handler-->>Client: 201 Created
    else Conflict (no Id returned)
        PG-->>DA_Att: no row
        DA_Att->>PG: SELECT RawAnswer WHERE SessionId=? AND EntryId=?
        PG-->>DA_Att: existing RawAnswer

        alt RawAnswer matches submitted answer
            DA_Att-->>Ops: IdempotentReplay
            Ops-->>Handler: Ok AttemptAlreadyRecorded
            Handler-->>Client: 200 OK
        else RawAnswer differs
            DA_Att-->>Ops: ConflictingReplay
            Ops-->>Handler: Error ConflictingAttempt
            Handler-->>Client: 409 Conflict
        end
    end
```

---

## 7. Entry Selector Resolution

```mermaid
flowchart TD
    Sel["EntrySelector (from request)"]

    Sel --> VS[VocabularyScope v]
    Sel --> CS[CollectionScope c]
    Sel --> WK["WorstKnown scope n"]
    Sel --> EE[ExplicitEntries ids]

    VS --> V_OWN{"UserId owns\nvocabulary?"}
    CS --> C_OWN{"UserId owns\ncollection?"}
    WK --> W_OWN{"UserId owns\nscope?"}
    EE --> E_OWN{"All EntryIds\nbelong to UserId?"}

    V_OWN -- No --> ERR["Error SelectorError"]
    C_OWN -- No --> ERR
    W_OWN -- No --> ERR
    E_OWN -- No --> ERR

    V_OWN -- Yes --> V_Q["SELECT EntryIds\nWHERE VocabularyId = v"]
    C_OWN -- Yes --> C_Q["SELECT EntryIds\nWHERE CollectionId = c"]
    W_OWN -- Yes --> WK_Q["LEFT JOIN EntryKnowledge\nCOALESCE hit_rate 0.0\nORDER BY hit_rate ASC,\nLastAttemptedAt ASC NULLS FIRST,\nEntryId ASC\nLIMIT n"]
    E_OWN -- Yes --> E_Q["Return ids as-is"]

    V_Q --> RESULT["EntryId list"]
    C_Q --> RESULT
    WK_Q --> RESULT
    E_Q --> RESULT

    RESULT --> EMPTY{"List empty?"}
    EMPTY -- Yes --> ERR2["Error NoEntriesResolved"]
    EMPTY -- No --> PROCEED["Proceed to prompt generation\n+ session creation"]
```
