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
│   ├── Wordfolio.Api.DataAccess.Tests/  # Data layer unit tests (F#)
│   └── Wordfolio.Api.Tests.Utils/ # Test utilities (F#)
│
├── Wordfolio.Frontend/     # React SPA
│   ├── src/
│   │   ├── routes/         # TanStack Router file-based routes
│   │   ├── pages/          # Page components
│   │   ├── components/     # Reusable UI components
│   │   ├── api/           # API client modules
│   │   ├── stores/        # Zustand state stores
│   │   ├── queries/       # React Query queries
│   │   ├── mutations/     # React Query mutations
│   │   ├── hooks/         # Custom React hooks
│   │   ├── schemas/       # Zod validation schemas
│   │   ├── contexts/      # React contexts
│   │   └── utils/         # Utility functions
│   ├── tests/             # Test files
│   └── package.json
│
├── Wordfolio.AppHost/      # .NET Aspire orchestration (C#)
├── Wordfolio.ServiceDefaults/  # Shared infrastructure (F#)
├── Wordfolio.Common/       # Shared utilities (F#)
├── .editorconfig           # Code style rules
├── fantomas-config.json    # F# formatting (Microsoft profile)
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
- Run Identity migrations (EF Core) automatically
- Run Wordfolio migrations (FluentMigrator) automatically
- Start the API service
- Start the frontend dev server
- Open the Aspire dashboard

**Manual migration commands (if needed):**

```bash
# Run Identity migrations manually
dotnet ef database update \
  --startup-project ./Wordfolio.Api/Wordfolio.Api/Wordfolio.Api.fsproj \
  --connection "Host=localhost;Port=5432;Database=wordfoliodb;User ID=myuser;Password=mypassword"

# Run Wordfolio migrations manually
dotnet build Wordfolio.Api/Wordfolio.Api.Migrations
dotnet fm migrate \
  -p PostgreSQL15_0 \
  -a "./Wordfolio.Api/Wordfolio.Api.Migrations/bin/Debug/net9.0/Wordfolio.Api.Migrations.dll" \
  -c "Host=localhost;Port=5432;Database=wordfoliodb;User ID=myuser;Password=mypassword"
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
6. **No Comments:** Prefer self-explanatory names and clear code structure over comments
7. **Minimal Changes:** Keep changes focused and atomic; split independent changes into separate commits
8. **Descriptive Naming:** Use descriptive variable and type names; avoid abbreviations

### F# Conventions

#### Naming

- **Modules:** PascalCase, match file names exactly
- **Types/Records:** PascalCase
- **Functions:** camelCase (public and private)
- **Constants:** PascalCase
- **Parameters:** camelCase

#### Formatting

Run formatter: `dotnet fantomas .` (fix) or `dotnet fantomas --check .` (check only)

#### Import Organization

F# modules should organize `open` statements in three groups, separated by blank lines:

1. **System imports** (e.g., `System.*`, `Microsoft.*`)
2. **Third-party imports** (e.g., `Dapper.*`, `Npgsql.*`)
3. **Local imports** (e.g., `Wordfolio.*`)

Within each group, sort imports alphabetically. Remove unused `open` statements.

#### Function Signatures (Data Access Pattern)

All data access functions follow this parameter order:

- Business parameters first
- Infrastructure parameters last (connection, transaction, cancellationToken)
- Always suffix async functions with `Async`
- Always return `Task<'T>` for async operations
- Use `task { }` computation expression

### C# Conventions

#### Naming

- **Classes/Interfaces:** PascalCase
- **Methods:** PascalCase
- **Parameters/Variables:** camelCase
- **Constants:** PascalCase

#### Formatting

Run formatter: `dotnet format` (fix) or `dotnet format --verify-no-changes` (check only)

### TypeScript/React Conventions

#### Naming

- **Components:** PascalCase (files and exports)
- **Hooks:** camelCase starting with `use` (e.g., `useTokenRefresh`)
- **Utilities:** camelCase (files and functions)
- **Types/Interfaces:** PascalCase
- **Constants:** UPPER_SNAKE_CASE

#### File Structure

- **Component files:** `ComponentName.tsx`
- **Hook files:** `useHookName.ts`
- **Utility files:** `utilityName.ts`
- **Test files:** `fileName.test.tsx` or `fileName.test.ts`

#### Component Declaration Style

**Always use arrow function expressions** for component declarations:

```typescript
// ✅ Correct
export const MyComponent = () => {
    return <div>Hello</div>;
};

// ❌ Wrong
export function MyComponent() {
    return <div>Hello</div>;
}
```

#### Import Organization

Organize imports with CSS imports after JavaScript/TypeScript imports, separated by a blank line.

#### Formatting

Run formatter: `npm run format` (fix) or `npm run format:check` (check only)

#### Linting

Run linter: `npm run lint`

Max warnings: 0 (strict enforcement)

### Database Conventions

#### Schema Naming

- **Schema:** `wordfolio` (lowercase)
- **Tables:** PascalCase (e.g., `Users`, `Collections`, `Vocabularies`)
- **Columns:** PascalCase (e.g., `Id`, `CreatedAt`, `UpdatedAt`)
- **Foreign Keys:** `FK_{Table}_{ReferencedTable}_{Column}`
- **Indexes:** `IX_{Table}_{Column(s)}`

#### Timestamp Handling

- **Always use `DateTimeOffset`** for timestamps (timezone-aware), never `DateTime`
- Database stores UTC
- Columns: `CreatedAt`, `UpdatedAt` (PascalCase)
- Nullable in database, represented as `Option<DateTimeOffset>` in F#
- Convert using `Option.ofNullable` (DB → Domain) and `Option.toNullable` (Domain → DB)

### Project File Conventions

- **Indentation:** Use 2 spaces (not 4) for indentation in `.csproj` and `.fsproj` files
- **Blank lines:** Separate `PropertyGroup` and `ItemGroup` sections with blank lines
- **Consistency:** Follow existing patterns in the project files

### Dependency Management

**Use strict version numbers** without `^` or `~` prefixes in `package.json`:

```json
{
  "dependencies": {
    "react": "19.1.1",
    "react-dom": "19.1.1"
  }
}
```

This ensures reproducible builds and prevents unexpected updates.

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

F# doesn't use constructor injection heavily. Instead, services are retrieved explicitly from `app.Services` in `Program.fs` and passed to handlers as parameters.

#### 3. Dapper.FSharp Query Composition

Use Dapper.FSharp computation expressions: `select { }`, `insert { }`, `update { }`, `delete { }`

#### 4. Transaction Management

Connections and transactions are managed explicitly. Pass `null` for transaction parameter when no transaction is needed.

#### 5. Identity Integration

The custom `UserStore` coordinates between two databases using `IUserStoreExtension` interface to trigger side effects when users are created, ensuring both Identity and Wordfolio databases stay in sync.

### Frontend Patterns

#### 1. File-Based Routing (TanStack Router)

Routes are auto-generated from `src/routes/` directory structure. Each route file exports a `Route` using `createFileRoute`.

#### 2. State Management Strategy

| State Type | Tool | Use Case |
|-----------|------|----------|
| **Global UI State** | Zustand | Auth tokens, user preferences |
| **Server State** | React Query | API data, caching, mutations |
| **Form State** | React Hook Form | Form inputs, validation |
| **URL State** | TanStack Router | Route params, search params |

#### 3. API Client Pattern

Centralized API modules with typed requests/responses.

#### 4. React Query Mutations

Always invalidate relevant queries in `onSuccess` callback to ensure fresh data.

#### 5. Form Handling (React Hook Form + Zod)

Use `zodResolver` to integrate Zod schemas with React Hook Form for type-safe validation.

---

## Testing Strategy

### Backend Tests

#### Integration Tests (Wordfolio.Api.Tests)

- **Framework:** XUnit
- **Pattern:** WebApplicationFactory for in-memory API
- **Database:** Real PostgreSQL (via test containers or local instance)
- **Scope:** End-to-end API tests with database assertions

#### Data Access Tests (Wordfolio.Api.DataAccess.Tests)

- **Framework:** XUnit
- **Fixtures:** Custom test fixtures with database reset
- **Scope:** Unit tests for data access functions

#### Database Testing Best Practices

**CRITICAL RULES for database tests:**

1. **Seeding:** Perform all initial database seeding via `DatabaseSeeder` only
2. **No Tested Functions in Setup:** Do NOT use functions from tested modules for database seeding or assertions
3. **Test Isolation:** Preferably make only ONE call to the tested function per test
4. **Assertions via Seeder:** Query the database for assertions using `DatabaseSeeder` only
5. **Compare Full Records:** Use tested records for assertions instead of asserting properties one at a time

### Frontend Tests

#### Unit/Component Tests (Vitest)

- **Framework:** Vitest
- **Library:** React Testing Library
- **Environment:** happy-dom (lightweight)
- **Location:** All tests MUST be in `Wordfolio.Frontend/tests/` directory
- **Structure:** Mirror source directory structure (e.g., `tests/components/`, `tests/utils/`, `tests/contexts/`)
- **File Extension:** Use `.test.ts` or `.test.tsx`
- **Imports:** Use relative paths from tests directory (e.g., `../../src/components/MyComponent`)

### Test Fixtures

**WordfolioTestFixture:** Manages Wordfolio database lifecycle

**WordfolioIdentityTestFixture:** Manages both databases for auth tests

---

## Build & CI/CD Workflows

### Local Build Commands

#### Backend

```bash
dotnet tool restore         # Restore tools
dotnet restore              # Restore dependencies
dotnet build --configuration Release
dotnet test --configuration Release
dotnet fantomas --check .   # F# format check
dotnet format --verify-no-changes  # C# format check
dotnet fantomas .           # F# format fix
dotnet format               # C# format fix
```

#### Frontend

```bash
npm ci                      # Clean install (CI)
npm install                 # Regular install
npm run build
npm test                    # Watch mode
npm test run                # Single run
npm run lint
npm run format:check        # Check
npm run format              # Fix
```

### GitHub Actions CI/CD

#### Backend CI (.github/workflows/backend.yml)

**Triggers:** Pull requests to `main` branch with changes in backend code

**Steps:** Checkout → Setup .NET → Restore tools → Restore deps → Build → Test → Format checks

**Success criteria:** All steps pass, zero warnings

#### Frontend CI (.github/workflows/frontend.yml)

**Triggers:** Pull requests to `main` branch with changes in frontend code

**Steps:** Checkout → Setup Node.js → Install deps → Format check → Lint → Test → Build

**Success criteria:** All steps pass, zero lint warnings

### Pre-Commit Checklist

Before committing changes, **run ALL verification commands**:

**Backend:**
- [ ] `dotnet build` - Code builds without warnings
- [ ] `dotnet test` - All tests pass
- [ ] `dotnet fantomas --check .` - F# code is formatted
- [ ] `dotnet format --verify-no-changes` - C# code is formatted

**Frontend:**
- [ ] `cd Wordfolio.Frontend && npm run build` - TypeScript compiles and Vite builds
- [ ] `npm test` - All tests pass
- [ ] `npm run lint` - No ESLint warnings (max warnings = 0)
- [ ] `npm run format:check` - Code is formatted with Prettier

**General:**
- [ ] New code has tests (if applicable)
- [ ] Commit message is clear and descriptive
- [ ] Changes are minimal and focused (split large changes into separate commits)

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

```bash
# Create new migration
dotnet ef migrations add MigrationName \
  --project Wordfolio.Api/Wordfolio.Api.Identity \
  --startup-project Wordfolio.Api/Wordfolio.Api

# Apply migrations
dotnet ef database update \
  --startup-project Wordfolio.Api/Wordfolio.Api/Wordfolio.Api.fsproj \
  --connection "Host=localhost;Port=5432;Database=wordfoliodb;User ID=user;Password=pass"

# Rollback
dotnet ef database update PreviousMigrationName \
  --startup-project Wordfolio.Api/Wordfolio.Api/Wordfolio.Api.fsproj
```

#### Wordfolio Migrations (FluentMigrator)

**Create new migration:**

1. Add new file in `Wordfolio.Api.Migrations/`
2. Use naming: `YYYYMMDDNNN_Description.fs`
3. Implement migration class inheriting from `AutoReversingMigration()`
4. Add file to `.fsproj`

**Apply migrations:**

```bash
dotnet build Wordfolio.Api/Wordfolio.Api.Migrations

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

Always use `Schema.fs` for type-safe column references. Define tables using `table'<RecordType> "TableName" |> inSchema Constants.Schema`.

---

## Common Tasks & Workflows

### Adding a New API Endpoint

1. **Define handler function** in `Wordfolio.Api/Handlers/`
2. **Register in Program.fs** by adding to the endpoint mapping chain
3. **Add data access function** (if needed) in `Wordfolio.Api.DataAccess/`
4. **Add tests** in `Wordfolio.Api.Tests/` or `Wordfolio.Api.DataAccess.Tests/`

### Adding a New Database Table

1. **Create migration** in `Wordfolio.Api.Migrations/` with naming `YYYYMMDDNNN_Description.fs`
2. **Define records** in `Wordfolio.Api.DataAccess/` (CLIMutable record for DB, domain type for business logic)
3. **Add to Schema.fs** with table definition
4. **Add migration to .fsproj** file
5. **Run migration** using `dotnet fm migrate`

### Adding a Frontend Route

1. **Create route file** in `src/routes/` using `createFileRoute`
2. **Create page component** in `src/pages/`
3. **Add navigation** (if needed) using TanStack Router's `Link` component

### Adding a React Query Mutation

1. **Add API function** in `src/api/` with typed request/response interfaces
2. **Create mutation hook** in `src/mutations/` with `useMutation` and invalidate queries in `onSuccess`
3. **Use in component** by calling the mutation hook and using `mutate` method

---

## Important Guidelines for AI Assistants

### What to Always Do

1. **Read files before modifying** - Never propose changes to code you haven't seen
2. **Follow existing patterns** - Match the style and structure of surrounding code
3. **Maintain type safety** - Leverage F#/TypeScript types, avoid `any` or untyped code
4. **Run formatters after changes** - Use `dotnet fantomas .` and `npm run format`
5. **Update tests** - Add/modify tests when changing functionality
6. **Use consistent naming** - Use descriptive names, avoid abbreviations; follow language conventions (F# = camelCase, C# = PascalCase, etc.)
7. **Handle nullability correctly** - Use `Option<'T>` in F#, proper null checks in TypeScript
8. **Use DateTimeOffset** - Never use `DateTime` for timestamps
9. **Follow parameter order** - Business params → connection → transaction → cancellationToken
10. **Compose functions** - Use F# pipeline operators (`|>`) for readability
11. **Organize imports** - Group and sort imports (System → Third-party → Local)
12. **Use arrow functions for React components** - `export const MyComponent = () => { ... }`
13. **Keep commits atomic** - Split independent changes into separate commits
14. **Avoid comments** - Write self-explanatory code instead

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
11. **❌ Don't add comments** - Write clear, self-explanatory code instead
12. **❌ Don't use function declarations for React components** - Use arrow functions
13. **❌ Don't use `^` or `~` in package.json versions** - Use strict versions only
14. **❌ Don't use tested functions in database test seeding** - Use DatabaseSeeder only
15. **❌ Don't make large, unfocused commits** - Keep changes minimal and atomic

### Common Pitfalls

#### Backend

- Forgetting to add new migration to project file
- Using `DateTime` instead of `DateTimeOffset`
- Not handling `Option` types correctly (accessing `.Value` without checking)

#### Frontend

- Not invalidating queries after mutations (leads to stale data)
- Storing server state in Zustand instead of React Query

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
- **AI Guidelines:** `.github/copilot-instructions.md` (concise version for GitHub Copilot)
- **Code Style:** `.editorconfig`, `fantomas-config.json`
- **Tools:** `.config/dotnet-tools.json`
- **CI/CD:** `.github/workflows/`

**Note:** This CLAUDE.md file is the comprehensive guide for AI assistants. The `.github/copilot-instructions.md` file contains a condensed version of these rules optimized for GitHub Copilot. Both should be kept in sync.

---

## Changelog

| Date | Changes |
|------|---------|
| 2025-12-06 | Initial version - Comprehensive codebase analysis and documentation |
| 2025-12-06 | Updated with guidelines from `.github/copilot-instructions.md`: import organization, component declaration style, project file formatting, dependency management, database testing best practices, and enhanced pre-commit checklist |
| 2025-12-06 | Optimized for size: Added migration commands to Quick Start, removed Manual Setup section, removed verbose formatting rules and code snippets |

---

**End of Document**

*This file should be updated whenever significant architectural changes are made to the codebase.*
