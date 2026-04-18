# Exercise Feature – Diagrams

Visual reference for the exercise feature design. All diagrams use Mermaid; they render inline on GitHub and in any Mermaid-aware previewer.

---

## 1. Entity-relationship model

Shows the three tables, their columns, and — crucially — which relationships are hard FKs (with cascade) versus plain indexed columns. The retention tier of each table is noted in the entity label.

```mermaid
erDiagram
    Users ||--o{ ExerciseSessions : "owns"
    Users ||--o{ ExerciseAttempts : "owns"

    Entries ||--o{ ExerciseSessionEntries : "FK cascade"
    Entries ||--o{ ExerciseAttempts : "FK cascade"

    ExerciseSessions ||--o{ ExerciseSessionEntries : "FK cascade"
    ExerciseSessions }o..o{ ExerciseAttempts : "SessionId nullable, NO FK"

    ExerciseSessions {
        int Id PK
        int UserId FK "no cascade"
        smallint ExerciseType "0=MC, 1=Translation"
        datetimeoffset CreatedAt "TTL anchor (30d)"
    }

    ExerciseSessionEntries {
        int Id PK
        int SessionId FK "cascade from ExerciseSessions"
        int EntryId FK "cascade from Entries"
        int DisplayOrder
        text PromptData "JSON snapshot"
        smallint PromptSchemaVersion
    }

    ExerciseAttempts {
        int Id PK
        int UserId FK "no cascade"
        int SessionId "nullable; nulled on purge, NO FK"
        int EntryId FK "cascade from Entries"
        smallint ExerciseType "denormalised"
        text PromptData "copied from session entry at submit"
        smallint PromptSchemaVersion "copied at submit"
        text RawAnswer "used for idempotency"
        bool IsCorrect "server-evaluated"
        datetimeoffset AttemptedAt "server-generated"
    }
```

Key rules encoded above:

- `EntryId` cascades everywhere — deleting an entry wipes all related history (this is the user’s intent).
- `ExerciseAttempts.SessionId` is **nullable** and carries **no FK**. The purge job nulls it before deleting the session row so attempt history survives.
- `ExerciseAttempts.PromptData` is copied from `ExerciseSessionEntries` at submit time — prompt context survives session purge without a separate durable table.
- `ExerciseAttempts.IsCorrect` is server-evaluated; the client does not supply it.
- `ExerciseAttempts.AttemptedAt` is server-generated; the client does not supply it.
- No `EntryKnowledge` table; all knowledge metrics are derived from `ExerciseAttempts` at query time.
- No `Status` / `CompletedAt` on `ExerciseSessions`: sessions are age-purged scaffolding.

---

## 2. Retention tiers

Two retention tiers with opposite policies. The diagram highlights why the FK graph is shaped the way it is.

```mermaid
flowchart TB
    subgraph Tier2["Tier 2 — Purgeable scaffolding (30-day TTL)"]
        direction LR
        Sessions[ExerciseSessions]
        SessionEntries[ExerciseSessionEntries]
        Sessions -->|ON DELETE CASCADE| SessionEntries
    end

    subgraph Tier1["Tier 1 — Durable learning history (kept indefinitely)"]
        direction LR
        Attempts[ExerciseAttempts]
    end

    PurgeJob[["Background purge job<br/>WHERE CreatedAt &lt; NOW() - 30d"]]
    PurgeJob -->|"1. SET SessionId = NULL on attempts"| Attempts
    PurgeJob -->|"2. DELETE sessions"| Sessions

    Sessions -. "SessionId nullable, no FK<br/>purge nulls SessionId then deletes session" .-> Attempts

    classDef tier2 fill:#fff2cc,stroke:#d6b656
    classDef tier1 fill:#d5e8d4,stroke:#82b366
    class Sessions,SessionEntries tier2
    class Attempts tier1
```

---

## 3. Layered architecture and dispatch

The module layering follows the project-wide pattern (Handlers → Operations → Capabilities → AppEnv → DataAccess). `Dispatch.fs` is a pure DU pattern-match used by `Operations.createSession` (for `generatePrompt`) and by `Operations.submitAttempt` (for `evaluate`).

```mermaid
flowchart TB
    HTTP[/HTTP request/]

    subgraph API["Wordfolio.Api / Api / Exercises/"]
        CreateH[CreateSessionHandler.fs]
        GetH[GetSessionHandler.fs]
        SubmitH[SubmitAttemptHandler.fs]
    end

    subgraph Domain["Wordfolio.Api.Domain / Exercises"]
        Ops[Operations.fs<br/>createSession · getSession · submitAttempt]
        Caps[Capabilities.fs<br/>IResolveEntrySelector · ICreateExerciseSession ·<br/>IGetExerciseSession · IGetSessionBundle ·<br/>IGetExerciseSessionEntry · ICommitAttempt]
        Dispatch[Dispatch.fs<br/>generatePrompt · evaluate]
        Types[ExerciseTypes.fs<br/>MultipleChoice · Translation]
    end

    subgraph Infra["Wordfolio.Api / Infrastructure"]
        AppEnv[Environment.fs / AppEnv<br/>thin mapping only]
    end

    subgraph DA["Wordfolio.Api.DataAccess"]
        Sessions[ExerciseSessions.fs]
        Attempts[ExerciseAttempts.fs]
    end

    DB[(PostgreSQL)]

    HTTP --> API
    API --> Ops
    Ops --> Caps
    Ops --> Dispatch
    Dispatch --> Types
    Caps -. implemented by .-> AppEnv
    AppEnv --> DA
    DA --> DB

    classDef handler fill:#dae8fc,stroke:#6c8ebf
    classDef domain fill:#d5e8d4,stroke:#82b366
    classDef infra fill:#ffe6cc,stroke:#d79b00
    classDef da fill:#f8cecc,stroke:#b85450
    class CreateH,GetH,SubmitH handler
    class Ops,Caps,Dispatch,Types domain
    class AppEnv infra
    class Sessions,Attempts da
```

---

## 4. Selector resolution

Selectors express **intent**, not entry IDs. Resolution happens once at session creation, with oversize selectors rejected at the handler (pre-DB `400 Bad Request` for `ExplicitEntries` > `MaxSessionEntries` or `WorstKnown count` > `MaxSessionEntries`), ownership validated for requests that pass size validation, and the resulting list (capped at `MaxSessionEntries = 10`) is frozen into `ExerciseSessionEntries`.

```mermaid
flowchart LR
    Req["Client request<br/>(selector + auth context UserId)"]

    subgraph Selector["EntrySelector DU"]
        direction TB
        V[VocabularyScope v]
        C[CollectionScope c]
        W[WorstKnown scope, count]
        E[ExplicitEntries ids]
    end

    SizeCheck{{"Pre-DB size validation<br/>ExplicitEntries length &gt; MaxSessionEntries = 10<br/>or WorstKnown count &gt; MaxSessionEntries = 10"}}
    BadReq[[400 Bad Request]]

    Own{{"Ownership validation<br/>(UserId from auth, NEVER from payload)"}}

    subgraph Resolved["Resolved EntryId list (capped at MaxSessionEntries = 10)"]
        R1[Vocabulary entries]
        R2[Collection entries]
        R3["Worst-known entries<br/>windowed CTE over last @knowledgeWindowSize ExerciseAttempts<br/>per (UserId, EntryId) — no EntryKnowledge table<br/>LEFT JOIN so cold entries included<br/>COALESCE hit rate → 0.0<br/>tie-break: LastAttemptedAt from ExerciseAttempts ASC NULLS FIRST<br/>stable tiebreak: EntryId ASC"]
        R4[Client-supplied IDs<br/>all verified owned]
    end

    Frozen[(ExerciseSessionEntries rows<br/>with PromptData + PromptSchemaVersion snapshot)]
    Err[[SelectorError → 403 Forbidden]]

    Req --> Selector
    V & C & W & E --> SizeCheck
    SizeCheck -- oversized --> BadReq
    SizeCheck -- ok --> Own
    Own -- ok --> R1 & R2 & R3 & R4
    Own -- fail --> Err
    R1 & R2 & R3 & R4 --> Frozen
```

---

## 5. Flow — Create session

`POST /exercises/sessions`. Returns the **full session bundle** with `attempt = null` for each entry.

```mermaid
sequenceDiagram
    autonumber
    participant C as Client
    participant H as CreateSessionHandler
    participant O as Operations.createSession
    participant E as AppEnv
    participant DA as DataAccess
    participant D as Dispatch

    C->>H: POST /exercises/sessions {exerciseType, selector}
    H->>H: Pre-DB size validation (400 if oversized)
    H->>O: createSession env params (UserId from auth)

    O->>E: resolveEntrySelector userId selector
    E->>DA: SELECT entries + ownership check
    DA-->>E: EntryId list
    E-->>O: Ok entryIds  /  Error SelectorError

    Note over O: Validate non-empty; cap at MaxSessionEntries = 10

    O->>DA: batch load all Entries WHERE Id IN (...)
    DA-->>O: Entry list

    loop for each entry
        O->>D: Dispatch.generatePrompt exerciseType entry
        D-->>O: GeneratedPrompt { PromptData; PromptSchemaVersion }
    end

    O->>E: createExerciseSession (CreateExerciseSessionData { ... })
    E->>DA: BEGIN TX
    E->>DA: INSERT ExerciseSessions → SessionId
    E->>DA: INSERT ExerciseSessionEntries (batch, with PromptData + PromptSchemaVersion)
    E->>DA: COMMIT
    E-->>O: SessionBundle (all attempts = None)

    O-->>H: Ok SessionBundle
    H-->>C: 201 Created + full bundle<br/>(sessionId, exerciseType, entries[] with attempt=null)
```

---

## 6. Flow — Resume / reload session

`GET /exercises/sessions/{id}`. Returns the bundle with per-entry `attempt` metadata populated.

```mermaid
sequenceDiagram
    autonumber
    participant C as Client
    participant H as GetSessionHandler
    participant O as Operations.getSession
    participant E as AppEnv
    participant DA as DataAccess

    C->>H: GET /exercises/sessions/{id}
    H->>O: getSession env userId sessionId

    O->>E: getSessionBundle userId sessionId
    E->>DA: SELECT ExerciseSessions WHERE Id=? (verify owner)
    E->>DA: SELECT ExerciseSessionEntries WHERE SessionId=? ORDER BY DisplayOrder
    E->>DA: SELECT ExerciseAttempts WHERE SessionId=? AND UserId=?
    E-->>O: SessionBundle option (Attempt populated per entry)

    alt owned & exists
        O-->>H: Ok SessionBundle
        H-->>C: 200 OK + bundle (attempt={rawAnswer,isCorrect,attemptedAt} or null per entry)
    else not owned / not found
        O-->>H: Error NotFound
        H-->>C: 404 Not Found
    end
```

---

## 7. Flow — Submit attempt (server-side evaluation)

`POST /exercises/sessions/{id}/entries/{entryId}/attempts`. The client sends only `rawAnswer`; the server evaluates correctness using `Dispatch.evaluate` and returns `isCorrect` in the response.

```mermaid
sequenceDiagram
    autonumber
    participant C as Client
    participant H as SubmitAttemptHandler
    participant O as Operations.submitAttempt
    participant E as AppEnv
    participant DA as DataAccess
    participant D as Dispatch

    C->>H: POST .../attempts {rawAnswer}
    H->>O: submitAttempt env userId sessionId entryId rawAnswer now

    O->>E: getExerciseSession sessionId
    E->>DA: SELECT ExerciseSessions WHERE Id=?
    E-->>O: ExerciseSession (or 404)

    O->>E: getExerciseSessionEntry sessionId entryId
    E->>DA: SELECT ExerciseSessionEntries WHERE SessionId=? AND EntryId=?
    E-->>O: ExerciseSessionEntry with PromptData + PromptSchemaVersion (or 404)

    O->>D: Dispatch.evaluate exerciseType promptSchemaVersion promptData rawAnswer
    D-->>O: Result<bool, EvaluateError>

    alt Ok isCorrect
        O->>E: commitAttempt (CommitAttemptData with server-computed IsCorrect + AttemptedAt=now)
        E->>DA: BEGIN TX
        E->>DA: INSERT ExerciseAttempts ... ON CONFLICT (SessionId, EntryId) DO NOTHING RETURNING Id
        Note over E,DA: Includes PromptData, PromptSchemaVersion, IsCorrect (server), AttemptedAt (server)

        alt Id returned (new row)
            E->>DA: COMMIT
            E-->>O: Inserted (AttemptInserted { AttemptId; IsCorrect })
            O-->>H: Ok (AttemptInserted { ... })
            H-->>C: 201 Created { "isCorrect": <bool> }
        else no Id (conflict)
            E->>DA: SELECT RawAnswer WHERE (SessionId, EntryId)
            alt RawAnswer matches
                E->>DA: COMMIT (no write)
                E-->>O: IdempotentReplay (AttemptAlreadyRecorded { IsCorrect })
                O-->>H: Ok (AttemptAlreadyRecorded { ... })
                H-->>C: 200 OK { "isCorrect": <bool> }
            else RawAnswer differs
                E->>DA: COMMIT (no write)
                E-->>O: ConflictingReplay
                O-->>H: Error ConflictingAttempt
                H-->>C: 409 Conflict
            end
        end
    else Error EvaluateError
        O-->>H: Error EvaluateError
        H-->>C: 500 Internal Server Error
    end
```

---

## 8. Idempotency decision tree

Distilled view of how `RawAnswer` resolves replays. Comparing `IsCorrect` alone cannot distinguish "same wrong answer" from "different wrong answer" — `RawAnswer` is what makes the check unambiguous.

```mermaid
flowchart TB
    Start([INSERT ... ON CONFLICT DO NOTHING RETURNING Id])
    New{Id returned?}
    Match{Existing RawAnswer<br/>matches submitted?}

    Ins[["Inserted (AttemptInserted { AttemptId; IsCorrect })<br/>→ 201 Created { isCorrect }"]]
    Idem[["IdempotentReplay (AttemptAlreadyRecorded { IsCorrect })<br/>→ 200 OK { isCorrect }"]]
    Conf[[ConflictingReplay<br/>→ no write<br/>→ 409 Conflict]]

    Start --> New
    New -- yes --> Ins
    New -- no --> Match
    Match -- yes --> Idem
    Match -- no --> Conf
```

---

## 9. Lifecycle state of a single entry

How one `EntryId` moves through the system over its lifetime. Notice that `ExerciseAttempts` is the sole source of knowledge metrics; there is no `EntryKnowledge` table. Entry deletion cascades through every tier.

```mermaid
stateDiagram-v2
    [*] --> Unseen: entry created<br/>(no ExerciseAttempts rows)

    Unseen --> InSession: selector resolves<br/>→ ExerciseSessionEntries row<br/>(+ PromptData + PromptSchemaVersion snapshot)
    InSession --> Answered: client submits rawAnswer<br/>server evaluates → isCorrect<br/>→ ExerciseAttempts row (PromptData copy, server IsCorrect + AttemptedAt)

    Unseen --> Answered: first attempt<br/>(ExerciseAttempts row created)
    Answered --> Answered: further attempts<br/>(additional ExerciseAttempts rows)

    InSession --> Skipped: session purged<br/>before answer
    Skipped --> [*]: session TTL expires<br/>(no attempt recorded)

    Answered --> Deleted: entry row deleted<br/>CASCADE wipes attempts +<br/>session entries
    InSession --> Deleted: entry deletion mid-session
    Unseen --> Deleted: entry deletion
    Deleted --> [*]
```
