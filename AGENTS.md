# Wordfolio — Agent Instructions

Wordfolio is a full-stack vocabulary management application (.NET 10 / F# backend, React / TypeScript frontend). Follow all rules in this file exactly. For domain-layer-specific rules, see the imported file at the end.

## Quick Commands

### Backend
```bash
dotnet build                      # Build all projects
dotnet test                       # Run all tests
dotnet test --filter "FullyQualifiedName~TestName"  # Run single test by name pattern
dotnet test --filter "FullyQualifiedName=Namespace.Class.Method"  # Run exact test
dotnet fantomas .                 # Format F# code
dotnet format                     # Format C# code (Identity project)
```

### Frontend
```bash
cd Wordfolio.Frontend
npm run build                     # Build (TypeScript compile + Vite)
npm test                          # Run all tests (single run)
npm run test:watch                # Run tests in watch mode
npm test -- -t "test name"        # Run single test by name pattern
npm test -- path/to/test.test.ts  # Run specific test file
npm run lint                      # Lint (max warnings = 0)
npm run format                    # Format with Prettier
```

### Database Migrations
```bash
# Identity (EF Core)
dotnet ef database update --startup-project ./Wordfolio.Api/Wordfolio.Api/Wordfolio.Api.fsproj --connection "Host=localhost;Port=5432;Database=wordfoliodb;User ID=user;Password=pass"

# Wordfolio (FluentMigrator)
dotnet build Wordfolio.Api/Wordfolio.Api.Migrations
dotnet fm migrate -p PostgreSQL15_0 -a "./Wordfolio.Api/Wordfolio.Api.Migrations/bin/Debug/net10.0/Wordfolio.Api.Migrations.dll" -c "Host=localhost;Port=5432;Database=wordfoliodb;User ID=user;Password=pass"
```

## Technology Stack

### Backend (.NET 10.0)

| Category | Technology |
|----------|-----------|
| Primary Language | F# |
| Secondary Language | C# (Identity only) |
| Web Framework | ASP.NET Core Minimal APIs |
| Database | PostgreSQL 15.0 |
| ORM (Identity) | Entity Framework Core |
| Data Access | Dapper.FSharp |
| Migrations | FluentMigrator |
| Auth | ASP.NET Core Identity (bearer + refresh tokens) |
| Observability | OpenTelemetry |
| AI | OpenAI / Groq |
| Orchestration | .NET Aspire |

### Frontend (Node.js 20)

| Category | Technology |
|----------|-----------|
| Language | TypeScript (strict) |
| Framework | React |
| Build | Vite |
| Routing | TanStack Router (file-based) |
| Global State | Zustand |
| Server State | TanStack Query |
| Forms | React Hook Form |
| Validation | Zod |
| UI | Material-UI (MUI) |
| Styling | Emotion + SCSS modules |
| Testing | Vitest + React Testing Library |
| Linting / Formatting | ESLint + Prettier |

## Codebase Structure

```
wordfolio/
├── .github/workflows/                   # CI/CD pipelines
│
├── Wordfolio.Api/
│   ├── Wordfolio.Api/                   # Main API service (F#)
│   │   ├── Handlers/                    # HTTP endpoint handlers
│   │   ├── Infrastructure/              # AppEnv implementation & DI
│   │   └── Program.fs
│   ├── Wordfolio.Api.Domain/            # Pure domain layer (F#)
│   ├── Wordfolio.Api.DataAccess/        # Data access (F#)
│   │   ├── Dapper.fs                    # Query execution helpers
│   │   ├── Database.fs                  # Connection/transaction management
│   │   ├── Schema.fs                    # Type-safe SQL column references
│   │   └── [Entity].fs
│   ├── Wordfolio.Api.Identity/          # Auth system (C#)
│   ├── Wordfolio.Api.Migrations/        # Schema migrations (F#)
│   ├── Wordfolio.Api.Tests/             # Integration tests
│   ├── Wordfolio.Api.Tests.Utils/       # Test utilities
│   ├── Wordfolio.Api.Domain.Tests/      # Domain unit tests
│   └── Wordfolio.Api.DataAccess.Tests/  # Data access unit tests
│
├── Wordfolio.Frontend/
│   ├── src/
│   │   ├── routes/                      # TanStack Router route files (config only)
│   │   ├── features/                    # Feature vertical slices
│   │   │   └── [feature]/
│   │   │       ├── api/
│   │   │       ├── components/
│   │   │       ├── hooks/
│   │   │       ├── pages/
│   │   │       ├── schemas/
│   │   │       └── styles/
│   │   ├── components/common/           # Shared UI components
│   │   ├── stores/                      # Zustand global state
│   │   └── config/                      # Global configuration & utilities
│   └── tests/                           # Mirrors src/ structure
│
├── Wordfolio.AppHost/                   # .NET Aspire orchestration
└── Wordfolio.sln
```

## Code Conventions

### General Principles

1. **Type Safety First:** Avoid `any` types in TypeScript and inferred broad types in F#.
2. **Immutability:** Prefer immutable data structures (F# records, React state).
3. **Functional Composition:** Use pipeline operators (`|>`) and function composition.
4. **Explicit Over Implicit:** Clear naming, no magic strings or numbers.
5. **Zero Warnings:** CI fails on any compiler or linter warning.
6. **No Comments:** Prefer self-explanatory names and clear code structure.
7. **Minimal Changes:** Keep changes focused and atomic; split independent changes into separate commits.
8. **Descriptive Naming:** Use full descriptive names; avoid abbreviations.

### F# Conventions

**Naming:** Modules → PascalCase (match file name). Types/Records → PascalCase. Functions → camelCase. Constants → PascalCase. Run `dotnet fantomas .` before committing.

**Import organization** — three groups, alphabetically sorted within each, blank line between groups:
1. System (`System.*`, `Microsoft.*`)
2. Third-party (`Dapper.*`, etc.)
3. Local (`Wordfolio.*`)

**Data Access function signatures:** business parameters first, then infrastructure parameters last (`connection`, `transaction`, `cancellationToken`). Async functions suffixed with `Async`, returning `Task<'T>` via `task { }`.

### TypeScript/React Conventions

Components → PascalCase arrow functions. Hooks → camelCase prefixed with `use`. Constants → UPPER_SNAKE_CASE. Run `npm run format` before committing.

### Database Conventions

Schema: `wordfolio` (lowercase). Tables/columns: PascalCase. Timestamps: `DateTimeOffset` — never `DateTime`.

### Dependency Management

Use strict version numbers in `package.json` — no `^` or `~` prefixes.

## Architecture

### Layered Flow

```
HTTP Request
     ↓
  Handlers           (Wordfolio.Api/Handlers/)
     ↓
  Domain Operations  (Wordfolio.Api.Domain/*/Operations.fs)
     ↓
  Capabilities       (Wordfolio.Api.Domain/*/Capabilities.fs)
     ↓
  AppEnv             (Wordfolio.Api/Infrastructure/Environment.fs)
     ↓
  Data Access        (Wordfolio.Api.DataAccess/)
     ↓
  PostgreSQL
```

- **Handlers:** Parse requests, call Operations, return responses. No business logic.
- **Operations:** Pure business logic. Call Capabilities for data access and side effects.
- **Capabilities:** Interfaces defined in Domain, implemented by AppEnv.
- **AppEnv:** Thin integration layer. Maps Domain types to Data Access types. No business logic. Single DB call per method (ideal). Use `TransactionalEnv` for transactions.
- **Data Access:** Pure functions. Use Dapper.FSharp. Explicitly pass `connection` and `transaction`. Return `Result` or `Option`.

`Wordfolio.Api.Domain` is pure — no dependencies on DataAccess or Infrastructure.

## Frontend Patterns

### State Management

| State Type | Tool |
|-----------|------|
| Global UI | Zustand |
| Server | TanStack Query |
| Form | React Hook Form |
| URL | TanStack Router |

### React Query

Always invalidate relevant queries in `onSuccess`. Use typed API clients.

### Feature-Based Architecture

Organize by feature, not file type. Each feature lives in `src/features/<feature>/` with subdirectories: `api/`, `components/`, `hooks/`, `pages/`, `schemas/`, `styles/`.

Shared UI components → `src/components/common/`. Global config → `src/config/`.

Presentational components receive data via props and focus on rendering. Page/container components handle data fetching and state, then pass data down. Custom hooks encapsulate reusable logic and side effects.

If a component file exceeds 200 lines, decompose it.

### Route-Page Separation

Routes (`src/routes/`): configuration only — paths, loaders, params. No UI logic.
Pages (`src/features/*/pages/`): implementation only. Imported by routes.

### Styling & Navigation

Extract complex styles to SCSS modules (`.module.scss`). Avoid heavy inline `sx` props for structural layout.

Use TanStack Router's `Link` or `useNavigate` for all navigation. No hardcoded URL strings.

### Boundary Handling

Every async operation must explicitly handle `Loading`, `Error`, and `Empty` states in the UI.

## Testing

### Backend

- **Integration tests** (`Wordfolio.Api.Tests`): end-to-end via `WebApplicationFactory`.
- **Data Access tests** (`Wordfolio.Api.DataAccess.Tests`): unit tests for DB functions.

**Critical Database Test Rules:**
1. Seed via `DatabaseSeeder` only.
2. Do not use the function under test to set up test data.
3. Assert writes by querying via `DatabaseSeeder`.
4. Assert against complete objects, not individual properties.

### Frontend

Vitest + React Testing Library. Tests in `Wordfolio.Frontend/tests/`, mirroring the `src/` structure.

@Wordfolio.Api/Wordfolio.Api.Domain/AGENTS.md
@Wordfolio.Api/Wordfolio.Api.Domain.Tests/AGENTS.md
@Wordfolio.Api/Wordfolio.Api.DataAccess/AGENTS.md
@Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/AGENTS.md
