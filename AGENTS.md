# Wordfolio - AI Agent Guide

**Last Updated:** 2025-12-29

This guide provides essential information for AI agents working on the Wordfolio codebase.

## Project Overview

**Wordfolio** is a full-stack web application for managing word collections and vocabularies.
- **Backend:** F# (.NET 10.0) with ASP.NET Core Minimal APIs, PostgreSQL
- **Frontend:** TypeScript 5.9, React 19.1, Vite, TanStack Router/Query
- **Architecture:** Layered monolith with clear separation between API, data access, and identity

## Quick Commands

### Backend
```bash
dotnet build                      # Build all projects
dotnet test                       # Run all tests
dotnet test --filter "FullyQualifiedName~TestName"  # Run single test by name pattern
dotnet test --filter "FullyQualifiedName=Namespace.Class.Method"  # Run exact test
dotnet fantomas .                 # Format F# code
dotnet fantomas --check .         # Check F# format
dotnet format                     # Format C# code (Identity project)
dotnet format --verify-no-changes # Check C# format
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
npm run format:check              # Check Prettier format
```

### Database Migrations
```bash
# Identity (EF Core)
dotnet ef database update --startup-project ./Wordfolio.Api/Wordfolio.Api/Wordfolio.Api.fsproj --connection "Host=localhost;Port=5432;Database=wordfoliodb;User ID=user;Password=pass"

# Wordfolio (FluentMigrator)
dotnet build Wordfolio.Api/Wordfolio.Api.Migrations
dotnet fm migrate -p PostgreSQL15_0 -a "./Wordfolio.Api/Wordfolio.Api.Migrations/bin/Debug/net10.0/Wordfolio.Api.Migrations.dll" -c "Host=localhost;Port=5432;Database=wordfoliodb;User ID=user;Password=pass"
```

## Tech Stack Summary

| Layer | Technology |
|-------|-----------|
| **Backend Language** | F# (primary), C# (Identity only) |
| **Backend Framework** | ASP.NET Core 9.0 |
| **Database** | PostgreSQL 15.0 (two schemas: Identity + Wordfolio) |
| **ORM - Identity** | Entity Framework Core 9.0 |
| **ORM - Business** | Dapper.FSharp 4.8.0 |
| **Migrations - Business** | FluentMigrator 7.1.0 |
| **Frontend Language** | TypeScript 5.9 |
| **Frontend Framework** | React 19.1 |
| **Build Tool** | Vite 7.1 |
| **Routing** | TanStack Router 1.134 |
| **State (Global)** | Zustand 5.0 |
| **State (Server)** | TanStack Query 5.90 |
| **Forms** | React Hook Form 7.66 |
| **Validation** | Zod 4.1 |
| **UI Library** | Material-UI 6.3 |
| **Testing** | Vitest 4.0 (frontend), XUnit (backend) |

## Code Conventions

### F# Conventions
- **Modules:** PascalCase, match file names exactly
- **Types/Records:** PascalCase
- **Functions:** camelCase (public and private)
- **Parameters:** camelCase
- **Imports:** Organize in three groups (System, Third-party, Local) separated by blank lines, sorted alphabetically
- **Async functions:** Suffix with `Async`, return `Task<'T>`, use `task { }`
- **Data access functions:** Business params → connection → transaction → cancellationToken
- **Error handling:** Use `Result<'T, DomainError>` for validation/business logic errors; discriminated unions for error types
- **Project files:** Use double spaces for indentation in .csproj/.fsproj; separate PropertyGroup/ItemGroup with blank lines

### C# Conventions
- **Classes/Interfaces/Methods:** PascalCase
- **Parameters/Variables:** camelCase

### TypeScript/React Conventions
- **Components:** PascalCase (files and exports)
- **Hooks:** camelCase starting with `use`
- **Utilities:** camelCase
- **Types/Interfaces:** PascalCase
- **Constants:** UPPER_SNAKE_CASE
- **Component declaration:** Always use arrow functions: `export const MyComponent = () => { ... }`
- **Imports:** JS/TS imports first, then CSS imports (separated by blank line)

### Database Conventions
- **Schema:** `wordfolio` (lowercase)
- **Tables/Columns:** PascalCase (e.g., `Users`, `Collections`, `Id`, `CreatedAt`)
- **Timestamps:** Always use `DateTimeOffset` (never `DateTime`)
- **Nullable timestamps:** `Option<DateTimeOffset>` in F#, use `Option.ofNullable` / `Option.toNullable`

## Architecture Patterns

### Backend Layered Architecture
```
HTTP Request → Handler (Wordfolio.Api/Handlers)
              ↓
         Data Access Function (Wordfolio.Api.DataAccess)
              ↓
         Dapper.FSharp Query
              ↓
         PostgreSQL Database
```

### AppEnv Implementation Pattern
AppEnv methods are thin integration layers. Follow these rules:
- **Single database call per method**
- **Simple parameter/result mapping only**
- **No business logic or orchestration**
- **Prefer optimized queries** (e.g., `getEntryByIdWithHierarchyAsync`) over multiple calls

### Frontend State Management
| State Type | Tool | Use Case |
|-----------|------|----------|
| Global UI State | Zustand | Auth tokens, user preferences |
| Server State | TanStack Query | API data, caching, mutations |
| Form State | React Hook Form | Form inputs, validation |
| URL State | TanStack Router | Route params, search params |

## Critical Do's and Don'ts

### Always Do
1. **Read files before modifying** - Never propose changes to code you haven't seen
2. **Follow existing patterns** - Match the style and structure of surrounding code
3. **Maintain type safety** - Avoid `any` types in TypeScript, use proper F# types
4. **Run formatters after changes** - `dotnet fantomas .`, `npm run format`
5. **Update tests** - Add/modify tests when changing functionality
6. **Use consistent naming** - Descriptive names, follow language conventions
7. **Handle nullability correctly** - `Option<'T>` in F#, proper null checks in TypeScript
8. **Use DateTimeOffset** - Never use `DateTime` for timestamps
9. **Follow parameter order** - Business params → connection → transaction → cancellationToken
10. **Compose functions** - Use F# pipeline operators (`|>`)
11. **Organize imports** - System → Third-party → Local, sorted alphabetically
12. **Use arrow functions for React components** - `export const MyComponent = () => { ... }`
13. **Keep commits atomic** - Split independent changes into separate commits
14. **Write self-explanatory code** - Avoid comments

### Never Do
1. ❌ Don't use `DateTime` for timestamps - Use `DateTimeOffset`
2. ❌ Don't skip formatters - Code must be formatted
3. ❌ Don't ignore warnings - CI fails on any warnings
4. ❌ Don't break layering - Keep handlers thin, logic in data access layer
5. ❌ Don't use magic strings - Use constants from `Schema.fs`
6. ❌ Don't mix C# and F# unnecessarily - F# is default; C# only for identity
7. ❌ Don't use `any` in TypeScript - Provide proper types
8. ❌ Don't skip tests - New features need coverage
9. ❌ Don't use function declarations for React components - Use arrow functions
10. ❌ Don't use `^` or `~` in package.json - Use strict versions only
11. ❌ Don't use tested functions in database test seeding - Use DatabaseSeeder only
12. ❌ Don't make large, unfocused commits - Keep changes minimal

## Testing Best Practices

### Backend Tests (F#/XUnit)
1. **Assert Whole Objects:** Always compare complete objects instead of individual properties to prevent false-positives when new properties are added
   ```fsharp
   // ❌ DON'T: Assert properties individually
   let result = doSomething()
   Assert.Equal("Prop1Value", result.Prop1)
   Assert.Equal("Prop2Value", result.Prop2)
   
   // ✅ DO: Assert against complete expected object
   let actual = doSomething()
   let expected: ResultType = { Prop1 = "Prop1Value"; Prop2 = "Prop2Value" }
   Assert.Equal(expected, actual)
   ```

### Database Tests (Critical Rules)
1. **Seeding:** Perform all initial database seeding via `DatabaseSeeder` only
2. **No Tested Functions in Setup:** Do NOT use functions from tested modules for seeding/assertions
3. **Test Isolation:** Preferably make only ONE call to the tested function per test
4. **Assertions via Seeder:** Query the database for assertions using `DatabaseSeeder` only
5. **Compare Full Records:** Use tested records for assertions (see "Assert Whole Objects" above)

### Frontend Tests
- **Location:** All tests MUST be in `Wordfolio.Frontend/tests/`
- **Structure:** Mirror source directory structure
- **File Extension:** `.test.ts` or `.test.tsx`
- **Imports:** Use relative paths from tests directory (e.g., `../../src/components/MyComponent`)

## Pre-Commit Checklist

**Backend:**
- [ ] `dotnet build` - Builds without warnings
- [ ] `dotnet test` - All tests pass
- [ ] `dotnet fantomas --check .` - F# formatted
- [ ] `dotnet format --verify-no-changes` - C# formatted

**Frontend:**
- [ ] `npm run build` - TypeScript compiles, Vite builds
- [ ] `npm test` - All tests pass
- [ ] `npm run lint` - No ESLint warnings (max warnings = 0)
- [ ] `npm run format:check` - Formatted with Prettier

**General:**
- [ ] New code has tests (if applicable)
- [ ] Commit message is clear and descriptive
- [ ] Changes are minimal and focused

## Common Tasks

### Adding a New API Endpoint
1. Define handler function in `Wordfolio.Api/Handlers/`
2. Register in Program.fs
3. Add data access function in `Wordfolio.Api.DataAccess/` (if needed)
4. Add tests in `Wordfolio.Api.Tests/` or `Wordfolio.Api.DataAccess.Tests/`

### Adding a New Database Table
1. Create migration in `Wordfolio.Api.Migrations/` with naming `YYYYMMDDNNN_Description.fs`
2. Define records in `Wordfolio.Api.DataAccess/`
3. Add to Schema.fs
4. Add migration to .fsproj
5. Run migration using `dotnet fm migrate`

### Adding a Frontend Route
1. Create route file in `src/routes/` using `createFileRoute`
2. Create page component in `src/pages/`
3. Add navigation using TanStack Router's `Link` component

### Adding a React Query Mutation
1. Add API function in `src/api/` with typed request/response interfaces
2. Create mutation hook in `src/mutations/` with `useMutation` and invalidate queries in `onSuccess`
3. Use in component

## Directory Structure

```
wordfolio/
├── Wordfolio.Api/
│   ├── Wordfolio.Api/              # Main API (F#)
│   │   ├── Handlers/
│   │   └── Program.fs
│   ├── Wordfolio.Api.DataAccess/   # Data layer (F#)
│   │   ├── Dapper.fs
│   │   ├── Database.fs
│   │   ├── Schema.fs
│   │   ├── Users.fs
│   │   ├── Collections.fs
│   │   └── Vocabularies.fs
│   ├── Wordfolio.Api.Identity/     # Auth (C#)
│   ├── Wordfolio.Api.Migrations/   # Migrations (F#)
│   ├── Wordfolio.Api.Tests/        # Integration tests
│   └── Wordfolio.Api.DataAccess.Tests/  # Data access tests
├── Wordfolio.Frontend/
│   ├── src/
│   │   ├── routes/                 # File-based routes
│   │   ├── pages/                  # Page components
│   │   ├── components/             # Reusable components
│   │   ├── api/                    # API client modules
│   │   ├── stores/                 # Zustand stores
│   │   ├── queries/                # React Query queries
│   │   ├── mutations/              # React Query mutations
│   │   ├── hooks/                  # Custom hooks
│   │   ├── schemas/                # Zod schemas
│   │   ├── contexts/               # React contexts
│   │   └── utils/                  # Utilities
│   └── tests/                      # Frontend tests
└── Wordfolio.AppHost/              # .NET Aspire orchestration
```

## Performance Tips

1. **Use `task { }` in F#** - More efficient than `async { }`
2. **Avoid N+1 queries** - Use joins or batching
3. **Enable React Query caching** - Set appropriate `staleTime`
4. **Use Zustand selectors** - Prevent unnecessary re-renders
5. **Debounce user input** - For search/autocomplete features

## Project Files Reference

- **Setup:** `README.md`
- **Comprehensive AI Guide:** `CLAUDE.md`
- **CI/CD:** `.github/workflows/`
- **Code Style:** `.editorconfig`, `fantomas-config.json`
- **Tools:** `.config/dotnet-tools.json`

---

**End of Document**
