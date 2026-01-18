# Wordfolio - AI Assistant Guide

**Last Updated:** 2026-01-18

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

**Architecture Style:** Layered monolith with clear separation between API, domain logic, data access, and identity concerns.

---

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

---

## Architecture & Technology Stack

### Backend (.NET 10.0)

| Category | Technology | Notes |
|----------|-----------|-------|
| **Primary Language** | F# | Functional-first approach for business logic |
| **Secondary Language** | C# | Used only for ASP.NET Core Identity integration |
| **Web Framework** | ASP.NET Core | Minimal APIs with functional composition |
| **Database** | PostgreSQL 15.0 | Single database, two schemas (Identity + Wordfolio) |
| **Domain Layer** | F# Pure Domain | Capabilities, Operations, and Types (no dependencies) |
| **ORM - Identity** | Entity Framework Core | Code-first migrations for auth tables |
| **Data Access - Business** | Dapper.FSharp | Type-safe functional queries |
| **Migrations - Business** | FluentMigrator | Version-numbered schema migrations |
| **Authentication** | ASP.NET Core Identity | Bearer tokens + refresh tokens |
| **Observability** | OpenTelemetry | Metrics, traces, logging |
| **AI Integration** | OpenAI / Groq | For AI-powered features (definitions, translations) |
| **Orchestration** | .NET Aspire | Local dev environment management |

### Frontend (Node.js 20)

| Category | Technology | Notes |
|----------|-----------|-------|
| **Language** | TypeScript | Strict mode enabled |
| **Framework** | React | Latest stable with concurrent features |
| **Build Tool** | Vite | Fast dev server, optimized builds |
| **Routing** | TanStack Router | File-based routing with type safety |
| **State (Global)** | Zustand | Lightweight with localStorage persistence |
| **State (Server)** | TanStack Query | Async state, caching, mutations |
| **Forms** | React Hook Form | Performant, uncontrolled inputs |
| **Validation** | Zod | Runtime type safety for forms/API |
| **UI Library** | Material-UI (MUI) | Component library |
| **Styling** | Emotion | CSS-in-JS |
| **Testing** | Vitest | Fast unit/component tests |
| **Linting** | ESLint | TypeScript support, max warnings = 0 |
| **Formatting** | Prettier | Consistent code style |

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
│
├── Wordfolio.Api/          # Backend API projects
│   ├── Wordfolio.Api/      # Main API service (F#)
│   │   ├── Handlers/       # HTTP endpoint handlers
│   │   ├── Infrastructure/ # AppEnv implementation & DI
│   │   └── Program.fs      # Entry point
│   │
│   ├── Wordfolio.Api.Domain/      # Pure Domain Layer (F#)
│   │   ├── Shared/                # Shared domain types
│   │   ├── Collections/           # Collection capabilities & operations
│   │   ├── Vocabularies/          # Vocabulary capabilities & operations
│   │   └── Entries/               # Entry capabilities & operations
│   │
│   ├── Wordfolio.Api.DataAccess/  # Data Access Implementation (F#)
│   │   ├── Dapper.fs       # Query execution helpers
│   │   ├── Database.fs     # Connection/transaction management
│   │   ├── Schema.fs       # Type-safe SQL column references
│   │   └── [Entity].fs     # Data access functions
│   │
│   ├── Wordfolio.Api.Identity/    # Auth system (C#)
│   ├── Wordfolio.Api.Migrations/  # Business schema migrations (F#)
│   ├── Wordfolio.Api.Tests/       # Integration tests
│   └── Wordfolio.Api.DataAccess.Tests/  # Data layer unit tests
│
├── Wordfolio.Frontend/     # React SPA
│   ├── src/
│   │   ├── routes/         # TanStack Router file-based routes
│   │   ├── pages/          # Page components
│   │   ├── components/     # Reusable UI components
│   │   ├── api/            # API client modules
│   │   ├── stores/         # Zustand state stores
│   │   ├── queries/        # React Query queries
│   │   ├── mutations/      # React Query mutations
│   │   ├── hooks/          # Custom React hooks
│   │   └── schemas/        # Zod validation schemas
│   ├── tests/              # Test files
│   └── package.json
│
├── Wordfolio.AppHost/      # .NET Aspire orchestration
└── Wordfolio.sln           # Solution file
```

---

## Development Environment Setup

### Prerequisites

- .NET 10.0 SDK
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
dotnet user-secrets set Parameters:groq-api-key <groq-api-key>
cd ..

# 3. Run Aspire AppHost (starts PostgreSQL, API, and frontend)
dotnet run --project Wordfolio.AppHost
```

**Note:** Database migrations are applied automatically when the application starts via Aspire.

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

#### Naming & Formatting

- **Modules:** PascalCase, match file names exactly
- **Types/Records:** PascalCase
- **Functions:** camelCase
- **Constants:** PascalCase
- **Run formatter:** `dotnet fantomas .`

#### Import Organization

F# modules should organize `open` statements in three groups, separated by blank lines:

1. **System imports** (e.g., `System.*`, `Microsoft.*`)
2. **Third-party imports** (e.g., `Dapper.*`)
3. **Local imports** (e.g., `Wordfolio.*`)

Within each group, sort imports alphabetically. Remove unused `open` statements.

#### Function Signatures (Data Access)

- Business parameters first
- Infrastructure parameters last (connection, transaction, cancellationToken)
- Always suffix async functions with `Async`
- Return `Task<'T>` and use `task { }`

### TypeScript/React Conventions

- **Components:** PascalCase, Arrow functions (`const Component = () => {}`)
- **Hooks:** camelCase starting with `use`
- **Constants:** UPPER_SNAKE_CASE
- **Run formatter:** `npm run format`

### Database Conventions

- **Schema:** `wordfolio` (lowercase)
- **Tables/Columns:** PascalCase
- **Timestamps:** `DateTimeOffset` (never `DateTime`)

### Dependency Management

**Use strict version numbers** without `^` or `~` prefixes in `package.json`:

## Key Patterns & Practices

### Backend Patterns

#### 1. Domain-Driven Layered Architecture

The backend follows a strict flow to separate concerns:

```
HTTP Request (Wordfolio.Api.Handlers)
   ↓
Domain Operation (Wordfolio.Api.Domain.*.Operations)
   ↓ calls
Capabilities Interface (Wordfolio.Api.Domain.*.Capabilities)
   ↓ implemented by
AppEnv (Wordfolio.Api.Infrastructure)
   ↓ calls
Data Access Function (Wordfolio.Api.DataAccess)
   ↓
PostgreSQL Database
```

- **Handlers:** Thin layer, parse requests, call Domain Operations, return responses.
- **Domain:** Pure business logic. Defines `Capabilities` (interfaces) for data access/side effects. `Operations` contain the business flow using these capabilities.
- **Infrastructure (AppEnv):** Implements the `Capabilities` interfaces. Acts as an integration layer mapping Domain types to Data Access types.
- **Data Access:** Pure functions performing DB operations.

#### 2. AppEnv Pattern

`AppEnv` methods in `Wordfolio.Api.Infrastructure` must be **thin integration layers**:
- **Single database call per method** (ideal).
- **Simple mapping** between Domain and Data Access types.
- **No business logic.**
- Use `TransactionalEnv` to wrap operations in database transactions.

#### 3. Data Access Layer

- Use `Dapper.FSharp` for queries.
- Explicitly pass `connection` and `transaction`.
- Return `Result` or `Option` types.

### Frontend Patterns

#### 1. State Management

| State Type | Tool | Use Case |
|-----------|------|----------|
| **Global UI State** | Zustand | Auth tokens, user preferences |
| **Server State** | TanStack Query | API data, caching, mutations |
| **Form State** | React Hook Form | Form inputs, validation |
| **URL State** | TanStack Router | Route params, search params |

#### 2. React Query Mutations

- Always invalidate relevant queries in `onSuccess` to ensure data consistency.
- Use typed API clients.

---

## Testing Strategy

### Backend Tests

- **Integration Tests (Wordfolio.Api.Tests):** End-to-end API tests using `WebApplicationFactory`.
- **Data Access Tests (Wordfolio.Api.DataAccess.Tests):** Unit tests for DB functions.

#### Critical Database Test Rules
1. **Seeding:** Use `DatabaseSeeder` ONLY.
2. **No Tested Functions in Setup:** Do not use the function being tested to set up data.
3. **Assertions via Seeder:** Query the DB via `DatabaseSeeder` to verify writes.
4. **Compare Full Records:** Assert against complete objects, not individual properties.

### Frontend Tests

- **Unit/Component:** Vitest + React Testing Library.
- **Location:** `Wordfolio.Frontend/tests/`.
- **Structure:** Mirror source directory structure.

---

## Common Tasks & Workflows

### Adding a New Feature (Vertical Slice)

1.  **Database:**
    *   Create migration (`Wordfolio.Api.Migrations`)
    *   Add records/types (`Wordfolio.Api.DataAccess/Schema.fs`, `[Entity].fs`)
    *   Add data access functions (`Wordfolio.Api.DataAccess/[Entity].fs`)
2.  **Domain:**
    *   Define types (`Wordfolio.Api.Domain/[Entity]/Types.fs`)
    *   Define capabilities (`Wordfolio.Api.Domain/[Entity]/Capabilities.fs`)
    *   Implement operations (`Wordfolio.Api.Domain/[Entity]/Operations.fs`)
3.  **Infrastructure:**
    *   Implement capabilities in `AppEnv` (`Wordfolio.Api/Infrastructure/Environment.fs`)
4.  **API:**
    *   Create Handler (`Wordfolio.Api/Handlers/[Entity].fs`)
    *   Register endpoint (`Wordfolio.Api/Program.fs`)
5.  **Frontend:**
    *   Add API client functions (`src/api/`)
    *   Add React Query hooks (`src/queries/`, `src/mutations/`)
    *   Add Routes/Pages (`src/routes/`)

---

## Important Guidelines for AI Assistants

### What to Always Do

1.  **Respect the Architecture:** Don't bypass the Domain layer. Handlers call Operations, Operations call Capabilities.
2.  **Type Safety:** Use strict types everywhere.
3.  **Format Code:** Always run `dotnet fantomas` and `npm run format`.
4.  **Use DateTimeOffset:** For all timestamps.
5.  **Follow Naming Conventions:** F# (camelCase), C# (PascalCase), React (PascalCase).
6.  **Update Tests:** Ensure new functionality is covered.

### What to Avoid

1.  ❌ Mixing business logic into Handlers or AppEnv.
2.  ❌ Using `DateTime`.
3.  ❌ Leaving "any" types in TypeScript.
4.  ❌ Committing unformatted code.
5.  ❌ Breaking the separation between `Wordfolio.Api.Domain` (pure) and `Wordfolio.Api.DataAccess` (impure).

---

**End of Document**
