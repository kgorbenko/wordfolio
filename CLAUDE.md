# Wordfolio - AI Assistant Guide

**Last Updated:** 2025-12-06

This document provides a comprehensive guide for AI assistants working on the Wordfolio codebase. It covers architecture, conventions, workflows, and best practices.

## Table of Contents

1. [Project Overview](#project-overview)
2. [Architecture & Technology Stack](#architecture--technology-stack)
3. [Codebase Structure](#codebase-structure)
4. [Development Environment Setup](#development-environment-setup)
5. [Code Conventions & Standards](#code-conventions--standards)
6. [Key Patterns & Practices](#key-patterns--practices)
7. [Testing Strategy](#testing-strategy)
8. [Build & CI/CD Workflows](#build--cicd-workflows)
9. [Database Management](#database-management)
10. [Common Tasks & Workflows](#common-tasks--workflows)
11. [Important Guidelines for AI Assistants](#important-guidelines-for-ai-assistants)

---

## Project Overview

**Wordfolio** is a full-stack web application for managing word collections and vocabularies. It features:

- User authentication and authorization
- Collection management (create, read, update, delete)
- Vocabulary tracking within collections
- RESTful API backend with OpenAPI documentation
- Modern React SPA frontend
- PostgreSQL database with timezone-aware timestamps

**Architecture Style:** Layered monolith with clear separation between API, data access, and identity concerns.

---

## Architecture & Technology Stack

### Backend (.NET 9.0)

| Category | Technology | Notes |
|----------|-----------|-------|
| **Primary Language** | F# | Functional-first approach for business logic |
| **Secondary Language** | C# | Used only for ASP.NET Core Identity integration |
| **Web Framework** | ASP.NET Core 9.0 | Minimal APIs with functional composition |
| **Database** | PostgreSQL 15.0 | Single database, two schemas (Identity + Wordfolio) |
| **ORM - Identity** | Entity Framework Core 9.0 | Code-first migrations for auth tables |
| **Data Access - Business** | Dapper.FSharp 4.8.0 | Type-safe functional queries |
| **Migrations - Business** | FluentMigrator 7.1.0 | Version-numbered schema migrations |
| **Authentication** | ASP.NET Core Identity | Bearer tokens + refresh tokens |
| **Observability** | OpenTelemetry | Metrics, traces, logging |
| **Orchestration** | .NET Aspire 9.4.1 | Local dev environment management |

### Frontend (Node.js 20)

| Category | Technology | Version | Notes |
|----------|-----------|---------|-------|
| **Language** | TypeScript | 5.9.2 | Strict mode enabled |
| **Framework** | React | 19.1.1 | Latest stable with concurrent features |
| **Build Tool** | Vite | 7.1.2 | Fast dev server, optimized builds |
| **Routing** | TanStack Router | 1.134.17 | File-based routing with type safety |
| **State (Global)** | Zustand | 5.0.8 | Lightweight with localStorage persistence |
| **State (Server)** | TanStack Query | 5.90.7 | Async state, caching, mutations |
| **Forms** | React Hook Form | 7.66.0 | Performant, uncontrolled inputs |
| **Validation** | Zod | 4.1.12 | Runtime type safety for forms/API |
| **UI Library** | Material-UI (MUI) | 6.3.0 | Component library |
| **Styling** | Emotion | 11.14.0 | CSS-in-JS |
| **Testing** | Vitest | 4.0.8 | Fast unit/component tests |
| **Linting** | ESLint | 9.33.0 | TypeScript support, max warnings = 0 |
| **Formatting** | Prettier | 3.4.2 | Consistent code style |

### DevOps & Tooling

- **CI/CD:** GitHub Actions (separate workflows for backend/frontend)
- **F# Formatting:** Fantomas 6.2.3 (Microsoft profile)
- **C# Formatting:** `dotnet format`
- **Containerization:** Docker (via Aspire publish)
- **Version Control:** Git

---

## Codebase Structure

```
wordfolio/
├── .github/
│   └── workflows/          # CI/CD pipelines
│       ├── backend.yml     # .NET build, test, format checks
│       └── frontend.yml    # Node build, test, lint checks
│
├── Wordfolio.Api/          # Backend API projects
│   ├── Wordfolio.Api/      # Main API service (F#)
│   │   ├── Handlers/       # HTTP endpoint handlers
│   │   │   └── Auth.fs     # Authentication endpoints
│   │   ├── IdentityIntegration.fs  # DI setup for auth
│   │   └── Program.fs      # Entry point, service composition
│   │
│   ├── Wordfolio.Api.DataAccess/  # Data layer (F#)
│   │   ├── Dapper.fs       # Query execution helpers
│   │   ├── Database.fs     # Connection/transaction management
│   │   ├── Schema.fs       # Type-safe SQL column references
│   │   ├── Users.fs        # User data access functions
│   │   ├── Collections.fs  # Collection data access
│   │   └── Vocabularies.fs # Vocabulary data access
│   │
│   ├── Wordfolio.Api.Identity/    # Auth system (C#)
│   │   ├── User.cs, Role.cs       # Domain models
│   │   ├── IdentityDbContext.cs   # EF Core context
│   │   ├── UserStore.cs           # Custom user store
│   │   └── Migrations/            # EF Core identity migrations
│   │
│   ├── Wordfolio.Api.Migrations/  # Business schema migrations (F#)
│   │   ├── 20250831001_CreaseWordfolioSchema.fs
│   │   ├── 20250831002_CreateUsersTable.fs
│   │   ├── 20250831003_CreateCollectionsTable.fs
│   │   └── 20250831004_CreateVocabulariesTable.fs
│   │
│   ├── Wordfolio.Api.Tests/       # Integration tests (F#)
│   │   ├── AuthTests.fs           # End-to-end auth tests
│   │   └── WebApplicationFactory.fs
│   │
│   ├── Wordfolio.Api.DataAccess.Tests/  # Data layer unit tests (F#)
│   │   ├── CollectionsTests.fs
│   │   ├── VocabulariesTests.fs
│   │   └── UsersTests.fs
│   │
│   └── Wordfolio.Api.Tests.Utils/ # Test utilities (F#)
│       ├── BaseDatabaseTestFixture.fs
│       ├── WordfolioTestFixture.fs
│       └── WordfolioIdentityTestFixture.fs
│
├── Wordfolio.Frontend/     # React SPA
│   ├── src/
│   │   ├── routes/         # TanStack Router file-based routes
│   │   │   ├── __root.tsx  # Root layout
│   │   │   ├── index.tsx   # Home page
│   │   │   ├── login.tsx   # Login route
│   │   │   └── register.tsx
│   │   │
│   │   ├── pages/          # Page components
│   │   │   ├── HomePage.tsx
│   │   │   ├── LoginPage.tsx
│   │   │   └── RegisterPage.tsx
│   │   │
│   │   ├── components/     # Reusable UI components
│   │   ├── api/           # API client modules
│   │   │   └── authApi.ts
│   │   │
│   │   ├── stores/        # Zustand state stores
│   │   │   └── authStore.ts
│   │   │
│   │   ├── queries/       # React Query queries
│   │   ├── mutations/     # React Query mutations
│   │   │   ├── useLoginMutation.ts
│   │   │   ├── useRegisterMutation.ts
│   │   │   └── useRefreshMutation.ts
│   │   │
│   │   ├── hooks/         # Custom React hooks
│   │   │   └── useTokenRefresh.ts
│   │   │
│   │   ├── schemas/       # Zod validation schemas
│   │   │   └── authSchemas.ts
│   │   │
│   │   ├── contexts/      # React contexts
│   │   └── utils/         # Utility functions
│   │       ├── errorHandling.ts
│   │       └── passwordValidation.ts
│   │
│   ├── tests/             # Test files
│   └── package.json
│
├── Wordfolio.AppHost/      # .NET Aspire orchestration (C#)
│   ├── AppHost.cs          # Service configuration
│   └── Configuration.cs    # Database options
│
├── Wordfolio.ServiceDefaults/  # Shared infrastructure (F#)
│   ├── Builder.fs          # Service registration helpers
│   ├── HealthCheck.fs      # Custom health checks
│   ├── OpenApi.fs          # Swagger configuration
│   └── Status.fs           # Status endpoint
│
├── Wordfolio.Common/       # Shared utilities (F#)
│   └── Task.fs             # Task computation expression
│
├── .editorconfig           # Code style rules
├── fantomas-config.json    # F# formatting (Microsoft profile)
├── .config/
│   └── dotnet-tools.json   # .NET local tools manifest
├── Wordfolio.sln           # Solution file
└── README.md               # Setup instructions
```

---

## Development Environment Setup

### Prerequisites

- .NET 9.0 SDK
- Node.js 20.x
- PostgreSQL 15+ (or use Aspire's containerized instance)
- Docker (for Aspire orchestration)

### Quick Start (with Aspire)

**Recommended for local development:**

```bash
# 1. Restore .NET tools
dotnet tool restore

# 2. Configure database credentials (user secrets)
cd Wordfolio.AppHost
dotnet user-secrets set Parameters:postgres-username myuser
dotnet user-secrets set Parameters:postgres-password mypassword
cd ..

# 3. Run Aspire AppHost (starts everything)
dotnet run --project Wordfolio.AppHost
```

This will:
- Start PostgreSQL in a container
- Run database migrations automatically
- Start the API service
- Start the frontend dev server
- Open the Aspire dashboard

### Manual Setup (without Aspire)

```bash
# 1. Install dependencies
dotnet restore
cd Wordfolio.Frontend && npm install && cd ..

# 2. Start PostgreSQL manually (ensure it's running on port 5432)

# 3. Run Identity migrations
dotnet ef database update \
  --startup-project ./Wordfolio.Api/Wordfolio.Api/Wordfolio.Api.fsproj \
  --connection "Host=localhost;Port=5432;Database=wordfoliodb;User ID=myuser;Password=mypassword"

# 4. Run Wordfolio migrations
dotnet fm migrate \
  -p PostgreSQL15_0 \
  -a "./Wordfolio.Api/Wordfolio.Api.Migrations/bin/Debug/net9.0/Wordfolio.Api.Migrations.dll" \
  -c "Host=localhost;Port=5432;Database=wordfoliodb;User ID=myuser;Password=mypassword"

# 5. Run backend
cd Wordfolio.Api/Wordfolio.Api
dotnet run

# 6. Run frontend (in separate terminal)
cd Wordfolio.Frontend
npm run dev
```

### Running Tests

```bash
# Backend tests
dotnet test

# Frontend tests
cd Wordfolio.Frontend
npm test              # Watch mode
npm test run          # Single run
```

---

## Code Conventions & Standards

### General Principles

1. **Type Safety First:** Leverage F# and TypeScript type systems; avoid `any` types
2. **Immutability:** Prefer immutable data structures (F# records, React state)
3. **Functional Composition:** Use pipeline operators (`|>`) and function composition
4. **Explicit Over Implicit:** Clear naming, no magic strings/numbers
5. **Zero Warnings:** CI fails on any compiler/linter warnings

### F# Conventions

#### Naming

- **Modules:** PascalCase, match file names exactly
- **Types/Records:** PascalCase
- **Functions:** camelCase (public and private)
- **Constants:** PascalCase
- **Parameters:** camelCase

```fsharp
module Collections  // Matches Collections.fs

type Collection = { Id: int; Name: string }

let getCollectionByIdAsync id connection transaction cancellationToken = ...
```

#### Formatting (Fantomas - Microsoft Profile)

- **Line length:** 120 characters max
- **Indentation:** 4 spaces (no tabs)
- **Brace style:** Stroustrup
- **Pipeline alignment:** Disabled (no vertical alignment on `|>`)
- **Line endings:** LF (Unix-style)

**Run formatter:**
```bash
dotnet fantomas .           # Format all files
dotnet fantomas --check .   # Check without modifying
```

#### Function Signatures (Data Access Pattern)

All data access functions follow this parameter order:

```fsharp
let functionNameAsync
    (businessParam1: Type1)
    (businessParam2: Type2)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<'ReturnType> =
    task {
        // Implementation
    }
```

**Key points:**
- Business parameters first
- Infrastructure parameters last (connection, transaction, cancellationToken)
- Always suffix async functions with `Async`
- Always return `Task<'T>` for async operations
- Use `task { }` computation expression

#### Handler Registration Pattern

```fsharp
let mapEndpointsAsync (app: IEndpointRouteBuilder) =
    app
        .MapGet("/path", handler)
        .AllowAnonymous()
    |> ignore

    app
        .MapPost("/path", handler)
        .RequireAuthorization()
    |> ignore

    app  // Return app for composition
```

### C# Conventions

#### Naming

- **Classes/Interfaces:** PascalCase
- **Methods:** PascalCase
- **Parameters/Variables:** camelCase
- **Constants:** PascalCase

#### Formatting

- **Indentation:** 4 spaces
- **Line endings:** LF
- **Braces:** Next line (Allman style for C#)

**Run formatter:**
```bash
dotnet format --verify-no-changes  # Check
dotnet format                      # Fix
```

### TypeScript/React Conventions

#### Naming

- **Components:** PascalCase (files and exports)
- **Hooks:** camelCase starting with `use` (e.g., `useTokenRefresh`)
- **Utilities:** camelCase (files and functions)
- **Types/Interfaces:** PascalCase
- **Constants:** UPPER_SNAKE_CASE

```typescript
// Component
export function LoginPage() { ... }

// Hook
export function useTokenRefresh() { ... }

// Constant
const API_BASE_URL = '/api';
```

#### File Structure

- **Component files:** `ComponentName.tsx`
- **Hook files:** `useHookName.ts`
- **Utility files:** `utilityName.ts`
- **Test files:** `fileName.test.tsx` or `fileName.test.ts`

#### Formatting (Prettier)

- **Print width:** 80 characters
- **Tab width:** 4 spaces
- **Trailing commas:** ES5
- **Semicolons:** Yes
- **Single quotes:** Yes

**Run formatter:**
```bash
npm run format        # Fix all files
npm run format:check  # Check only
```

#### Linting (ESLint)

- **Indentation:** 4 spaces
- **Max warnings:** 0 (strict)
- **React Hooks:** Rules enforced
- **TypeScript:** Strict rules enabled

**Run linter:**
```bash
npm run lint
```

### Database Conventions

#### Schema Naming

- **Schema:** `wordfolio` (lowercase)
- **Tables:** PascalCase (e.g., `Users`, `Collections`, `Vocabularies`)
- **Columns:** PascalCase (e.g., `Id`, `CreatedAt`, `UpdatedAt`)
- **Foreign Keys:** `FK_{Table}_{ReferencedTable}_{Column}`
- **Indexes:** `IX_{Table}_{Column(s)}`

#### Timestamp Handling

- **Always use `DateTimeOffset`** for timestamps (timezone-aware)
- Database stores UTC
- Columns: `CreatedAt`, `UpdatedAt` (PascalCase)
- Nullable in database, represented as `Option<DateTimeOffset>` in F#

```fsharp
// Database record (nullable)
[<CLIMutable>]
type CollectionRecord = {
    CreatedAt: DateTimeOffset
    UpdatedAt: Nullable<DateTimeOffset>
}

// Domain model (option)
type Collection = {
    CreatedAt: DateTimeOffset
    UpdatedAt: Option<DateTimeOffset>
}

// Conversion
let fromRecord (record: CollectionRecord) : Collection =
    { CreatedAt = record.CreatedAt
      UpdatedAt = Option.ofNullable record.UpdatedAt }
```

---

## Key Patterns & Practices

### Backend Patterns

#### 1. Layered Architecture

```
HTTP Request → Handler (Wordfolio.Api/Handlers)
              ↓
         Data Access Function (Wordfolio.Api.DataAccess)
              ↓
         Dapper.FSharp Query
              ↓
         PostgreSQL Database
```

- **No business logic in handlers** (thin layer)
- **Data access layer is pure functions** (no side effects except DB I/O)
- **Clear separation from identity** (separate project)

#### 2. Dependency Injection via Parameters

F# doesn't use constructor injection heavily. Instead:

```fsharp
// In Program.fs
let app = builder.Build()

// Get services explicitly
let dataSource = app.Services.GetRequiredService<NpgsqlDataSource>()

// Pass to handlers
app |> mapEndpoints dataSource |> ignore
```

Handlers receive dependencies as parameters:

```fsharp
let mapEndpoints (dataSource: NpgsqlDataSource) (app: IEndpointRouteBuilder) =
    app.MapGet("/users/{id}", fun (id: int) ->
        task {
            use connection = dataSource.CreateConnection()
            let! user = Users.getByIdAsync id connection null CancellationToken.None
            return Results.Ok(user)
        })
    |> ignore
```

#### 3. Dapper.FSharp Query Composition

```fsharp
// SELECT with WHERE
select {
    for c in collectionsTable do
        where (c.Id = id)
}
|> trySelectFirstAsync connection transaction cancellationToken

// INSERT
insert {
    into collectionsTable
    value newRecord
}
|> insertAsync connection transaction cancellationToken

// UPDATE
update {
    for c in collectionsTable do
        set updateRecord
        where (c.Id = id)
}
|> updateAsync connection transaction cancellationToken

// DELETE
delete {
    for c in collectionsTable do
        where (c.Id = id)
}
|> deleteAsync connection transaction cancellationToken
```

#### 4. Transaction Management

Connections and transactions are managed explicitly:

```fsharp
// No transaction (null is acceptable)
let! result = queryAsync connection null cancellationToken

// With transaction
use! connection = dataSource.OpenConnectionAsync(cancellationToken)
use transaction = connection.BeginTransaction()
try
    let! result1 = query1Async connection transaction cancellationToken
    let! result2 = query2Async connection transaction cancellationToken
    transaction.Commit()
    return result2
with ex ->
    transaction.Rollback()
    reraise()
```

#### 5. Identity Integration

The custom `UserStore` coordinates between two databases:

1. **Create user in Identity DB** (via EF Core)
2. **Trigger side effect** (via `IUserStoreExtension`)
3. **Create corresponding record in Wordfolio DB** (via Dapper)

```fsharp
// IUserStoreExtension interface
type IUserStoreExtension =
    abstract member AfterCreateAsync: User -> CancellationToken -> Task
```

This ensures both databases stay in sync during user registration.

### Frontend Patterns

#### 1. File-Based Routing (TanStack Router)

Routes are auto-generated from `src/routes/` directory structure:

```
routes/
├── __root.tsx       → Root layout (all pages)
├── index.tsx        → / (home)
├── login.tsx        → /login
└── register.tsx     → /register
```

Each route file exports a `Route` using `createFileRoute`:

```typescript
import { createFileRoute } from '@tanstack/react-router';

export const Route = createFileRoute('/login')({
    component: LoginPage,
});
```

#### 2. State Management Strategy

| State Type | Tool | Use Case |
|-----------|------|----------|
| **Global UI State** | Zustand | Auth tokens, user preferences |
| **Server State** | React Query | API data, caching, mutations |
| **Form State** | React Hook Form | Form inputs, validation |
| **URL State** | TanStack Router | Route params, search params |

**Example: Auth Store (Zustand)**

```typescript
interface AuthState {
    accessToken: string | null;
    refreshToken: string | null;
    setTokens: (access: string, refresh: string) => void;
    clearTokens: () => void;
}

export const useAuthStore = create<AuthState>()(
    persist(
        (set) => ({
            accessToken: null,
            refreshToken: null,
            setTokens: (access, refresh) =>
                set({ accessToken: access, refreshToken: refresh }),
            clearTokens: () =>
                set({ accessToken: null, refreshToken: null }),
        }),
        { name: 'auth-storage' }  // localStorage key
    )
);
```

#### 3. API Client Pattern

Centralized API modules with typed requests/responses:

```typescript
// api/authApi.ts
export interface LoginRequest {
    email: string;
    password: string;
}

export interface LoginResponse {
    accessToken: string;
    refreshToken: string;
}

export const login = async (data: LoginRequest): Promise<LoginResponse> => {
    const response = await fetch(`${API_BASE_URL}/auth/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data),
    });

    if (!response.ok) {
        throw await parseApiError(response);
    }

    return response.json();
};
```

#### 4. React Query Mutations

```typescript
// mutations/useLoginMutation.ts
export function useLoginMutation() {
    const navigate = useNavigate();
    const { setTokens } = useAuthStore();

    return useMutation({
        mutationFn: login,
        onSuccess: (data) => {
            setTokens(data.accessToken, data.refreshToken);
            navigate({ to: '/' });
        },
    });
}

// Usage in component
const loginMutation = useLoginMutation();
loginMutation.mutate({ email, password });
```

#### 5. Form Handling (React Hook Form + Zod)

```typescript
// schemas/authSchemas.ts
export const loginSchema = z.object({
    email: z.string().email('Invalid email'),
    password: z.string().min(8, 'Password must be at least 8 characters'),
});

export type LoginFormData = z.infer<typeof loginSchema>;

// Component
const {
    register,
    handleSubmit,
    formState: { errors },
} = useForm<LoginFormData>({
    resolver: zodResolver(loginSchema),
});

const onSubmit = (data: LoginFormData) => {
    loginMutation.mutate(data);
};
```

---

## Testing Strategy

### Backend Tests

#### Integration Tests (Wordfolio.Api.Tests)

- **Framework:** XUnit
- **Pattern:** WebApplicationFactory for in-memory API
- **Database:** Real PostgreSQL (via test containers or local instance)
- **Scope:** End-to-end API tests with database assertions

**Example:**

```fsharp
[<Fact>]
member _.``Register endpoint creates user in both databases``() =
    task {
        // Arrange
        let request = { Email = "test@example.com"; Password = "SecurePass123!" }

        // Act
        let! response = client.PostAsJsonAsync("/auth/register", request)

        // Assert
        response.EnsureSuccessStatusCode() |> ignore

        // Verify in Identity database
        let! identityUser = identityContext.Users.FirstOrDefaultAsync(fun u -> u.Email = request.Email)
        Assert.NotNull(identityUser)

        // Verify in Wordfolio database
        let! wordfolioUser = Users.getByEmailAsync request.Email connection null CancellationToken.None
        Assert.True(wordfolioUser.IsSome)
    }
```

#### Data Access Tests (Wordfolio.Api.DataAccess.Tests)

- **Framework:** XUnit
- **Fixtures:** Custom test fixtures with database reset
- **Scope:** Unit tests for data access functions

**Example:**

```fsharp
type CollectionsTests(fixture: WordfolioTestFixture) =
    interface IClassFixture<WordfolioTestFixture>

    [<Fact>]
    member _.``getCollectionByIdAsync returns Some when collection exists``() =
        task {
            // Arrange
            let! seededCollection = DatabaseSeeder.seedCollectionAsync fixture.Connection

            // Act
            let! result = Collections.getCollectionByIdAsync seededCollection.Id fixture.Connection null CancellationToken.None

            // Assert
            Assert.True(result.IsSome)
            Assert.Equal(seededCollection.Id, result.Value.Id)
        }
```

### Frontend Tests

#### Unit/Component Tests (Vitest)

- **Framework:** Vitest
- **Library:** React Testing Library
- **Environment:** happy-dom (lightweight)
- **Location:** `tests/` directory or co-located

**Example:**

```typescript
// utils/errorHandling.test.ts
import { describe, it, expect } from 'vitest';
import { parseApiError } from './errorHandling';

describe('parseApiError', () => {
    it('parses error message from response', async () => {
        const response = new Response(
            JSON.stringify({ message: 'Invalid credentials' }),
            { status: 401 }
        );

        const error = await parseApiError(response);

        expect(error.message).toBe('Invalid credentials');
        expect(error.status).toBe(401);
    });
});
```

**Component Test:**

```typescript
// components/Notification.test.tsx
import { render, screen } from '@testing-library/react';
import { Notification } from './Notification';

it('displays error message', () => {
    render(<Notification message="Error occurred" severity="error" />);

    expect(screen.getByText('Error occurred')).toBeInTheDocument();
});
```

### Test Fixtures

**WordfolioTestFixture:** Manages Wordfolio database lifecycle

```fsharp
type WordfolioTestFixture() =
    let dataSource = createDataSource()
    let connection = dataSource.CreateConnection()

    do
        runMigrationsAsync() |> Async.AwaitTask |> Async.RunSynchronously
        resetDatabaseAsync() |> Async.AwaitTask |> Async.RunSynchronously

    member _.Connection = connection

    interface IDisposable with
        member _.Dispose() =
            connection.Dispose()
            dataSource.Dispose()
```

**WordfolioIdentityTestFixture:** Manages both databases for auth tests

---

## Build & CI/CD Workflows

### Local Build Commands

#### Backend

```bash
# Restore tools
dotnet tool restore

# Restore dependencies
dotnet restore

# Build
dotnet build --configuration Release

# Test
dotnet test --configuration Release

# Format check
dotnet fantomas --check .     # F#
dotnet format --verify-no-changes  # C#

# Format fix
dotnet fantomas .
dotnet format
```

#### Frontend

```bash
# Install
npm ci  # Clean install (CI)
npm install  # Regular install

# Build
npm run build

# Test
npm test        # Watch mode
npm test run    # Single run

# Lint
npm run lint

# Format
npm run format:check  # Check
npm run format        # Fix
```

### GitHub Actions CI/CD

#### Backend CI (.github/workflows/backend.yml)

**Triggers:**
- Pull requests to `main` branch
- Changes in:
  - `Wordfolio.Api/**`
  - `Wordfolio.AppHost/**`
  - `Wordfolio.Common/**`
  - `Wordfolio.ServiceDefaults/**`
  - `*.sln`, `*.props`, `*.targets`
  - Workflow file itself

**Steps:**
1. Checkout code
2. Setup .NET 9.0
3. Restore .NET tools (`dotnet tool restore`)
4. Restore dependencies
5. Build (Release configuration)
6. Run tests
7. Check F# formatting (Fantomas)
8. Check C# formatting (dotnet format)

**Success criteria:** All steps pass, zero warnings

#### Frontend CI (.github/workflows/frontend.yml)

**Triggers:**
- Pull requests to `main` branch
- Changes in:
  - `Wordfolio.Frontend/**`
  - Workflow file

**Steps:**
1. Checkout code
2. Setup Node.js 20 (with npm cache)
3. Install dependencies (`npm ci`)
4. Check formatting (Prettier)
5. Run linter (ESLint)
6. Run tests (Vitest)
7. Build (TypeScript + Vite)

**Success criteria:** All steps pass, zero lint warnings

### Pre-Commit Checklist

Before committing changes, ensure:

- [ ] Code builds without warnings
- [ ] All tests pass
- [ ] Code is formatted (run formatters)
- [ ] No linter warnings
- [ ] New code has tests (if applicable)
- [ ] Commit message is clear and descriptive

---

## Database Management

### Two-Database Architecture

Wordfolio uses a **single PostgreSQL database** with **two schemas**:

1. **Identity Schema** (managed by EF Core)
   - Tables: `AspNetUsers`, `AspNetRoles`, `AspNetUserTokens`, etc.
   - Migration tool: Entity Framework Core
   - Language: C#

2. **Wordfolio Schema** (managed by FluentMigrator)
   - Tables: `wordfolio.Users`, `wordfolio.Collections`, `wordfolio.Vocabularies`
   - Migration tool: FluentMigrator
   - Language: F#

### Migration Workflows

#### Identity Migrations (EF Core)

**Create new migration:**

```bash
dotnet ef migrations add MigrationName \
  --project Wordfolio.Api/Wordfolio.Api.Identity \
  --startup-project Wordfolio.Api/Wordfolio.Api
```

**Apply migrations:**

```bash
dotnet ef database update \
  --startup-project Wordfolio.Api/Wordfolio.Api/Wordfolio.Api.fsproj \
  --connection "Host=localhost;Port=5432;Database=wordfoliodb;User ID=user;Password=pass"
```

**Rollback:**

```bash
dotnet ef database update PreviousMigrationName \
  --startup-project Wordfolio.Api/Wordfolio.Api/Wordfolio.Api.fsproj
```

#### Wordfolio Migrations (FluentMigrator)

**Create new migration:**

1. Add new file in `Wordfolio.Api.Migrations/`
2. Use naming: `YYYYMMDDNNN_Description.fs`
3. Implement migration class:

```fsharp
[<Migration(20251206001L)>]
type CreateItemsTable() =
    inherit AutoReversingMigration()

    override this.Up() =
        base.Create
            .Table("Items")
            .InSchema(Constants.Schema)
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("Name").AsString(200).NotNullable()
            .WithColumn("CreatedAt").AsDateTimeOffset().NotNullable()
        |> ignore
```

**Apply migrations:**

```bash
# Build migrations project first
dotnet build Wordfolio.Api/Wordfolio.Api.Migrations

# Run migrations
dotnet fm migrate \
  -p PostgreSQL15_0 \
  -a "./Wordfolio.Api/Wordfolio.Api.Migrations/bin/Debug/net9.0/Wordfolio.Api.Migrations.dll" \
  -c "Host=localhost;Port=5432;Database=wordfoliodb;User ID=user;Password=pass"
```

**Rollback:**

```bash
dotnet fm rollback \
  -p PostgreSQL15_0 \
  -a "./Wordfolio.Api/Wordfolio.Api.Migrations/bin/Debug/net9.0/Wordfolio.Api.Migrations.dll" \
  -c "Host=localhost;Port=5432;Database=wordfoliodb;User ID=user;Password=pass" \
  --steps 1
```

### Schema Constants

Always use `Schema.fs` for type-safe column references:

```fsharp
// Schema.fs
module rec Schema

open Dapper.FSharp.PostgreSQL

let collectionsTable = table'<CollectionRecord> "Collections" |> inSchema Constants.Schema

// Usage in queries
select {
    for c in collectionsTable do
        where (c.Id = id)
}
```

### DateTimeOffset Handling

**CRITICAL:** Always use `DateTimeOffset` for timestamps, never `DateTime`.

```fsharp
// ✅ Correct
CreatedAt = DateTimeOffset.UtcNow
UpdatedAt = Some DateTimeOffset.UtcNow

// ❌ Wrong
CreatedAt = DateTime.UtcNow  // Don't use DateTime
```

**Conversion between database and domain:**

```fsharp
// Database → Domain
let fromRecord (record: CollectionRecord) : Collection =
    { Id = record.Id
      CreatedAt = record.CreatedAt
      UpdatedAt = Option.ofNullable record.UpdatedAt }

// Domain → Database
let toRecord (collection: Collection) : CollectionRecord =
    { Id = collection.Id
      CreatedAt = collection.CreatedAt
      UpdatedAt = collection.UpdatedAt |> Option.toNullable }
```

---

## Common Tasks & Workflows

### Adding a New API Endpoint

1. **Define handler function** in `Wordfolio.Api/Handlers/`

```fsharp
// Handlers/Collections.fs
let getCollectionHandler (dataSource: NpgsqlDataSource) (id: int) =
    task {
        use! connection = dataSource.OpenConnectionAsync(CancellationToken.None)
        let! collection = Collections.getCollectionByIdAsync id connection null CancellationToken.None

        return
            match collection with
            | Some c -> Results.Ok(c)
            | None -> Results.NotFound()
    }

let mapCollectionEndpoints (dataSource: NpgsqlDataSource) (app: IEndpointRouteBuilder) =
    app
        .MapGet("/collections/{id}", fun (id: int) -> getCollectionHandler dataSource id)
        .RequireAuthorization()
    |> ignore

    app
```

2. **Register in Program.fs**

```fsharp
app
    |> mapHealthChecks
    |> mapStatusEndpoint
    |> mapAuthEndpoints
    |> mapCollectionEndpoints dataSource  // Add this
    |> ignore
```

3. **Add data access function** (if needed) in `Wordfolio.Api.DataAccess/`

```fsharp
// DataAccess/Collections.fs
let getCollectionByIdAsync (id: int) (connection) (transaction) (cancellationToken) =
    task {
        let! result =
            select {
                for c in collectionsTable do
                    where (c.Id = id)
            }
            |> trySelectFirstAsync connection transaction cancellationToken
        return result |> Option.map fromRecord
    }
```

4. **Add tests**

```fsharp
// Api.Tests/CollectionsTests.fs
[<Fact>]
member _.``GET /collections/{id} returns 200 when collection exists``() =
    task {
        let! collection = DatabaseSeeder.seedCollectionAsync connection

        let! response = client.GetAsync($"/collections/{collection.Id}")

        response.EnsureSuccessStatusCode() |> ignore
        let! result = response.Content.ReadFromJsonAsync<Collection>()
        Assert.Equal(collection.Id, result.Id)
    }
```

### Adding a New Database Table

1. **Create migration** in `Wordfolio.Api.Migrations/`

```fsharp
[<Migration(20251206001L)>]
type CreateItemsTable() =
    inherit AutoReversingMigration()

    override this.Up() =
        base.Create
            .Table("Items")
            .InSchema(Constants.Schema)
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("Name").AsString(200).NotNullable()
            .WithColumn("UserId").AsInt32().NotNullable()
                .ForeignKey("FK_Items_Users_UserId", Constants.Schema, "Users", "Id")
            .WithColumn("CreatedAt").AsDateTimeOffset().NotNullable()
            .WithColumn("UpdatedAt").AsDateTimeOffset().Nullable()
        |> ignore

        base.Create
            .Index("IX_Items_UserId")
            .OnTable("Items")
            .InSchema(Constants.Schema)
            .OnColumn("UserId")
        |> ignore
```

2. **Define records** in `Wordfolio.Api.DataAccess/Items.fs`

```fsharp
[<CLIMutable>]
type ItemRecord = {
    Id: int
    Name: string
    UserId: int
    CreatedAt: DateTimeOffset
    UpdatedAt: Nullable<DateTimeOffset>
}

type Item = {
    Id: int
    Name: string
    UserId: int
    CreatedAt: DateTimeOffset
    UpdatedAt: Option<DateTimeOffset>
}

let fromRecord (record: ItemRecord) : Item = ...
let toRecord (item: Item) : ItemRecord = ...
```

3. **Add to Schema.fs**

```fsharp
let itemsTable = table'<ItemRecord> "Items" |> inSchema Constants.Schema
```

4. **Run migration**

```bash
dotnet build Wordfolio.Api/Wordfolio.Api.Migrations
dotnet fm migrate -p PostgreSQL15_0 -a "..." -c "..."
```

### Adding a Frontend Route

1. **Create route file** in `src/routes/`

```typescript
// src/routes/collections.tsx
import { createFileRoute } from '@tanstack/react-router';
import { CollectionsPage } from '../pages/CollectionsPage';

export const Route = createFileRoute('/collections')({
    component: CollectionsPage,
});
```

2. **Create page component** in `src/pages/`

```typescript
// src/pages/CollectionsPage.tsx
export function CollectionsPage() {
    return (
        <div>
            <h1>Collections</h1>
            {/* Page content */}
        </div>
    );
}
```

3. **Add navigation** (if needed)

```typescript
import { Link } from '@tanstack/react-router';

<Link to="/collections">Collections</Link>
```

### Adding a React Query Mutation

1. **Add API function** in `src/api/`

```typescript
// api/collectionsApi.ts
export interface CreateCollectionRequest {
    name: string;
    description?: string;
}

export interface Collection {
    id: number;
    name: string;
    description: string | null;
    createdAt: string;
    updatedAt: string | null;
}

export const createCollection = async (
    data: CreateCollectionRequest
): Promise<Collection> => {
    const response = await fetch(`${API_BASE_URL}/collections`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            Authorization: `Bearer ${getAccessToken()}`,
        },
        body: JSON.stringify(data),
    });

    if (!response.ok) {
        throw await parseApiError(response);
    }

    return response.json();
};
```

2. **Create mutation hook** in `src/mutations/`

```typescript
// mutations/useCreateCollectionMutation.ts
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { createCollection } from '../api/collectionsApi';

export function useCreateCollectionMutation() {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: createCollection,
        onSuccess: () => {
            // Invalidate collections query to refetch
            queryClient.invalidateQueries({ queryKey: ['collections'] });
        },
    });
}
```

3. **Use in component**

```typescript
const createMutation = useCreateCollectionMutation();

const onSubmit = (data: CreateCollectionFormData) => {
    createMutation.mutate(data, {
        onSuccess: () => {
            showNotification('Collection created!', 'success');
        },
        onError: (error) => {
            showNotification(error.message, 'error');
        },
    });
};
```

---

## Important Guidelines for AI Assistants

### What to Always Do

1. **Read files before modifying** - Never propose changes to code you haven't seen
2. **Follow existing patterns** - Match the style and structure of surrounding code
3. **Maintain type safety** - Leverage F#/TypeScript types, avoid `any` or untyped code
4. **Run formatters after changes** - Use `dotnet fantomas .` and `npm run format`
5. **Update tests** - Add/modify tests when changing functionality
6. **Use consistent naming** - Follow conventions for the language (F# = camelCase, C# = PascalCase, etc.)
7. **Handle nullability correctly** - Use `Option<'T>` in F#, proper null checks in TypeScript
8. **Use DateTimeOffset** - Never use `DateTime` for timestamps
9. **Follow parameter order** - Business params → connection → transaction → cancellationToken
10. **Compose functions** - Use F# pipeline operators (`|>`) for readability

### What to Avoid

1. **❌ Don't use `DateTime`** - Always use `DateTimeOffset` for timestamps
2. **❌ Don't skip formatters** - Code must be formatted before committing
3. **❌ Don't ignore warnings** - CI fails on any warnings; fix them
4. **❌ Don't break layering** - Keep handlers thin, logic in data access layer
5. **❌ Don't use magic strings** - Use constants from `Schema.fs` or config
6. **❌ Don't mix C# and F# unnecessarily** - F# is default; C# only for identity
7. **❌ Don't create inconsistent APIs** - Follow RESTful conventions
8. **❌ Don't forget transactions** - Coordinate writes across databases carefully
9. **❌ Don't use `any` in TypeScript** - Provide proper types
10. **❌ Don't skip tests** - New features need test coverage

### Common Pitfalls

#### Backend

**Pitfall:** Forgetting to add new migration to project file

```xml
<!-- Wordfolio.Api.Migrations.fsproj -->
<ItemGroup>
  <Compile Include="20251206001_CreateItemsTable.fs" />  <!-- Add this -->
</ItemGroup>
```

**Pitfall:** Using `DateTime` instead of `DateTimeOffset`

```fsharp
// ❌ Wrong
CreatedAt = DateTime.UtcNow

// ✅ Correct
CreatedAt = DateTimeOffset.UtcNow
```

**Pitfall:** Not handling `Option` types correctly

```fsharp
// ❌ Wrong - crashes if None
let name = collection.UpdatedAt.Value

// ✅ Correct
let name =
    match collection.UpdatedAt with
    | Some date -> date.ToString()
    | None -> "N/A"
```

#### Frontend

**Pitfall:** Not invalidating queries after mutations

```typescript
// ❌ Wrong - stale data
useMutation({
    mutationFn: createCollection,
    // No onSuccess
});

// ✅ Correct - refetches data
useMutation({
    mutationFn: createCollection,
    onSuccess: () => {
        queryClient.invalidateQueries({ queryKey: ['collections'] });
    },
});
```

**Pitfall:** Storing server state in Zustand instead of React Query

```typescript
// ❌ Wrong - use React Query for API data
const useCollectionsStore = create((set) => ({
    collections: [],
    fetchCollections: async () => { /* ... */ },
}));

// ✅ Correct - React Query handles server state
const { data: collections } = useQuery({
    queryKey: ['collections'],
    queryFn: fetchCollections,
});
```

### File Organization Tips

- **Backend:** One file per domain entity (e.g., `Users.fs`, `Collections.fs`)
- **Frontend:** Group by feature, not by type (prefer `features/collections/` over `components/`, `hooks/` split)
- **Tests:** Mirror source structure (e.g., `Collections.fs` → `CollectionsTests.fs`)
- **Migrations:** Number sequentially with descriptive names

### Performance Considerations

1. **Use `task { }` in F#** - More efficient than `async { }`
2. **Avoid N+1 queries** - Use joins or batching
3. **Enable React Query caching** - Set appropriate `staleTime`
4. **Use Zustand selectors** - Prevent unnecessary re-renders
5. **Debounce user input** - For search/autocomplete features

---

## Additional Resources

### Official Documentation

- [.NET 9.0 Docs](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-9)
- [F# Language Guide](https://learn.microsoft.com/en-us/dotnet/fsharp/)
- [ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/)
- [Dapper.FSharp](https://github.com/Dzoukr/Dapper.FSharp)
- [FluentMigrator](https://fluentmigrator.github.io/)
- [React 19 Docs](https://react.dev/)
- [TanStack Router](https://tanstack.com/router/latest)
- [TanStack Query](https://tanstack.com/query/latest)
- [Zustand](https://docs.pmnd.rs/zustand/getting-started/introduction)
- [Material-UI](https://mui.com/)
- [Vitest](https://vitest.dev/)

### Project-Specific Files

- **Setup:** `README.md`
- **Code Style:** `.editorconfig`, `fantomas-config.json`
- **Tools:** `.config/dotnet-tools.json`
- **CI/CD:** `.github/workflows/`

---

## Changelog

| Date | Changes |
|------|---------|
| 2025-12-06 | Initial version - Comprehensive codebase analysis and documentation |

---

**End of Document**

*This file should be updated whenever significant architectural changes are made to the codebase.*
