# Handlers Refactoring (Vertical API Slices)

## Overview

This refactoring moves Wordfolio API handlers from flat files in `Wordfolio.Api/Wordfolio.Api/Handlers/` into vertical feature slices under `Wordfolio.Api/Wordfolio.Api/Api/`. The primary goal is maintainable API slices for backend developers and predictable autonomous iteration boundaries.

The loop must migrate all seven handlers (`Auth`, `Collections`, `CollectionsHierarchy`, `Vocabularies`, `Entries`, `Drafts`, `Dictionary`) with one handler targeted per implement step. API compatibility is a hard constraint: no breaking changes to routes, methods, auth requirements, status codes, or request/response contracts.

## Specification

### Architecture / Design Decisions

- Target shared root API files:
  - `Wordfolio.Api/Wordfolio.Api/Api/Types.fs`
  - `Wordfolio.Api/Wordfolio.Api/Api/Mappers.fs`
  - `Wordfolio.Api/Wordfolio.Api/Api/Helpers.fs`
- Target feature slices:
  - `Wordfolio.Api/Wordfolio.Api/Api/<Feature>/Types.fs`
  - `Wordfolio.Api/Wordfolio.Api/Api/<Feature>/Mappers.fs`
  - `Wordfolio.Api/Wordfolio.Api/Api/<Feature>/Handlers.fs`
- Keep endpoint registration centralized in `Wordfolio.Api/Wordfolio.Api/Program.fs`, but update imports to new handler module paths.
- Keep explicit compile order in `Wordfolio.Api/Wordfolio.Api/Wordfolio.Api.fsproj`.

### API Compatibility Contract (Hard Constraint)

- No API breaking changes:
  - keep routes from `Wordfolio.Api/Wordfolio.Api/Urls.fs`
  - keep HTTP methods
  - keep auth requirements
  - keep status codes
  - keep request/response JSON shapes
- Keep behavior unchanged; this is structural refactoring only.
- Route-parameter cleanup (unused parameters) is out of scope.

### Namespace and Dependency Contract

- Namespaces/modules must match physical file location on disk.
- API feature boundaries are strict:
  - forbidden: `Wordfolio.Api.Api.<FeatureA>` importing `Wordfolio.Api.Api.<FeatureB>`
- Shared cross-feature contracts/mappers/helpers must live in root `Api/Types.fs`, `Api/Mappers.fs`, `Api/Helpers.fs`.
- Domain error DU to HTTP mapping is allowed in feature `Handlers.fs`.
- For unexpected `Result<_, unit>` paths, fail fast with explicit exception context; do not use `Result.defaultValue`.

### Pattern to Follow

- Vertical slicing and compile ordering pattern:
  - `Wordfolio.Api/Wordfolio.Api.Domain/Wordfolio.Api.Domain.fsproj`
  - `Wordfolio.Api/Wordfolio.Api.Domain/Collections/Types.fs`
  - `Wordfolio.Api/Wordfolio.Api.Domain/Collections/Operations.fs`
- API rules source of truth:
  - `Wordfolio.Api/Wordfolio.Api/AGENTS.md`
  - `docs/handlers-refactoring/rulebook.md`

### Handler Order (Dependency-Aware)

1. Auth
2. Collections
3. CollectionsHierarchy
4. Vocabularies
5. Entries
6. Drafts
7. Dictionary

Note: `Wordfolio.Api/Wordfolio.Api/Handlers/Drafts.fs` currently depends on Entries handler symbols; after refactor, Drafts must use only root shared API contracts/mappers/helpers (never another feature).

### File Manifest

Create:
- `Wordfolio.Api/Wordfolio.Api/Api/Types.fs`
- `Wordfolio.Api/Wordfolio.Api/Api/Mappers.fs`
- `Wordfolio.Api/Wordfolio.Api/Api/Helpers.fs`
- `Wordfolio.Api/Wordfolio.Api/Api/Auth/Types.fs`
- `Wordfolio.Api/Wordfolio.Api/Api/Auth/Mappers.fs`
- `Wordfolio.Api/Wordfolio.Api/Api/Auth/Handlers.fs`
- `Wordfolio.Api/Wordfolio.Api/Api/Collections/Types.fs`
- `Wordfolio.Api/Wordfolio.Api/Api/Collections/Mappers.fs`
- `Wordfolio.Api/Wordfolio.Api/Api/Collections/Handlers.fs`
- `Wordfolio.Api/Wordfolio.Api/Api/CollectionsHierarchy/Types.fs`
- `Wordfolio.Api/Wordfolio.Api/Api/CollectionsHierarchy/Mappers.fs`
- `Wordfolio.Api/Wordfolio.Api/Api/CollectionsHierarchy/Handlers.fs`
- `Wordfolio.Api/Wordfolio.Api/Api/Vocabularies/Types.fs`
- `Wordfolio.Api/Wordfolio.Api/Api/Vocabularies/Mappers.fs`
- `Wordfolio.Api/Wordfolio.Api/Api/Vocabularies/Handlers.fs`
- `Wordfolio.Api/Wordfolio.Api/Api/Entries/Types.fs`
- `Wordfolio.Api/Wordfolio.Api/Api/Entries/Mappers.fs`
- `Wordfolio.Api/Wordfolio.Api/Api/Entries/Handlers.fs`
- `Wordfolio.Api/Wordfolio.Api/Api/Drafts/Types.fs`
- `Wordfolio.Api/Wordfolio.Api/Api/Drafts/Mappers.fs`
- `Wordfolio.Api/Wordfolio.Api/Api/Drafts/Handlers.fs`
- `Wordfolio.Api/Wordfolio.Api/Api/Dictionary/Types.fs`
- `Wordfolio.Api/Wordfolio.Api/Api/Dictionary/Mappers.fs`
- `Wordfolio.Api/Wordfolio.Api/Api/Dictionary/Handlers.fs`

Modify:
- `Wordfolio.Api/Wordfolio.Api/Wordfolio.Api.fsproj`
- `Wordfolio.Api/Wordfolio.Api/Program.fs`
- Minimal namespace/type-reference updates only (no behavior/assertion changes):
  - `Wordfolio.Api/Wordfolio.Api.Tests/AuthTests.fs`
  - `Wordfolio.Api/Wordfolio.Api.Tests/CollectionsTests.fs`
  - `Wordfolio.Api/Wordfolio.Api.Tests/CollectionsHierarchyTests.fs`
  - `Wordfolio.Api/Wordfolio.Api.Tests/VocabulariesTests.fs`
  - `Wordfolio.Api/Wordfolio.Api.Tests/EntriesTests.fs`
  - `Wordfolio.Api/Wordfolio.Api.Tests/DraftsTests.fs`
  - `Wordfolio.Api/Wordfolio.Api.Tests/DictionaryTests.fs`

## Verification Commands

``dotnet fantomas . && dotnet build && dotnet test``

## Implementation Steps

### 1. Auth Feature
- [x] Implement: Migrate Auth feature by strictly following this specification and `docs/handlers-refactoring/rulebook.md`. Run verification commands after implementation.
- [x] Improve: Review Auth migration against this specification and `docs/handlers-refactoring/rulebook.md`; apply targeted fixes, then re-run verification commands.

### 2. Collections Feature
- [x] Implement: Migrate Collections feature by strictly following this specification and `docs/handlers-refactoring/rulebook.md`. Run verification commands after implementation.
- [x] Improve: Review Collections migration against this specification and `docs/handlers-refactoring/rulebook.md`; apply targeted fixes, then re-run verification commands.

### 3. CollectionsHierarchy Feature
- [x] Implement: Migrate CollectionsHierarchy feature by strictly following this specification and `docs/handlers-refactoring/rulebook.md`. Run verification commands after implementation.
- [x] Improve: Review CollectionsHierarchy migration against this specification and `docs/handlers-refactoring/rulebook.md`; apply targeted fixes, then re-run verification commands.

### 4. Vocabularies Feature
- [x] Implement: Migrate Vocabularies feature by strictly following this specification and `docs/handlers-refactoring/rulebook.md`. Run verification commands after implementation.
- [x] Improve: Review Vocabularies migration against this specification and `docs/handlers-refactoring/rulebook.md`; apply targeted fixes, then re-run verification commands.

### 5. Entries Feature
- [x] Implement: Migrate Entries feature by strictly following this specification and `docs/handlers-refactoring/rulebook.md`. Run verification commands after implementation.
- [ ] Improve: Review Entries migration against this specification and `docs/handlers-refactoring/rulebook.md`; apply targeted fixes, then re-run verification commands.

### 6. Drafts Feature
- [ ] Implement: Migrate Drafts feature by strictly following this specification and `docs/handlers-refactoring/rulebook.md`. Run verification commands after implementation.
- [ ] Improve: Review Drafts migration against this specification and `docs/handlers-refactoring/rulebook.md`; apply targeted fixes, then re-run verification commands.

### 7. Dictionary Feature
- [ ] Implement: Migrate Dictionary feature by strictly following this specification and `docs/handlers-refactoring/rulebook.md`. Run verification commands after implementation.
- [ ] Improve: Review Dictionary migration against this specification and `docs/handlers-refactoring/rulebook.md`; apply targeted fixes, then re-run verification commands.

## Progress Log

### Implement: Migrate Auth feature by strictly following this specification and `docs/handlers-refactoring/rulebook.md`. Run verification commands after implementation.
- Files changed: Wordfolio.Api/Wordfolio.Api/Api/Types.fs, Wordfolio.Api/Wordfolio.Api/Api/Mappers.fs, Wordfolio.Api/Wordfolio.Api/Api/Helpers.fs, Wordfolio.Api/Wordfolio.Api/Api/Auth/Types.fs, Wordfolio.Api/Wordfolio.Api/Api/Auth/Mappers.fs, Wordfolio.Api/Wordfolio.Api/Api/Auth/Handlers.fs, Wordfolio.Api/Wordfolio.Api/Handlers/Auth.fs, Wordfolio.Api/Wordfolio.Api/Program.fs, Wordfolio.Api/Wordfolio.Api/Wordfolio.Api.fsproj, Wordfolio.Api/Wordfolio.Api.Tests/AuthTests.fs, docs/handlers-refactoring/spec.md
- What was done: Migrated the Auth handler into the new vertical slice under Api/Auth with transport types and pure mapper extraction, and introduced root Api shared files required by the compile-order contract. Updated Program and project compile includes to use the new Auth handler module and removed the old flat handler file.
- Issues encountered: None
- Learnings: Root Api files must be introduced early (even if minimal) so future feature slices can depend on them without compile-order churn; Auth tests only needed namespace cleanup because endpoint contracts remained unchanged.

### Improve: Review Auth migration against this specification and `docs/handlers-refactoring/rulebook.md`; apply targeted fixes, then re-run verification commands.
- Files changed: docs/handlers-refactoring/spec.md
- What was done: Reviewed the migrated Auth slice against namespace, feature-boundary, API-contract, and compile-order rules and confirmed it already complied without requiring code changes. Re-ran `dotnet fantomas . && dotnet build && dotnet test` to verify no regressions.
- Issues encountered: None
- Learnings: The Auth migration is already aligned with the vertical-slice contracts, so future iterations can focus on migrating the remaining handlers while preserving the same endpoint compatibility checks.

### Implement: Migrate Collections feature by strictly following this specification and `docs/handlers-refactoring/rulebook.md`. Run verification commands after implementation.
- Files changed: Wordfolio.Api/Wordfolio.Api/Api/Collections/Types.fs, Wordfolio.Api/Wordfolio.Api/Api/Collections/Mappers.fs, Wordfolio.Api/Wordfolio.Api/Api/Collections/Handlers.fs, Wordfolio.Api/Wordfolio.Api/Api/Helpers.fs, Wordfolio.Api/Wordfolio.Api/Handlers/Collections.fs, Wordfolio.Api/Wordfolio.Api/Program.fs, Wordfolio.Api/Wordfolio.Api/Wordfolio.Api.fsproj, Wordfolio.Api/Wordfolio.Api.Tests/CollectionsTests.fs, docs/handlers-refactoring/spec.md
- What was done: Migrated Collections into `Api/Collections` with separated transport types, pure mappers, and handler endpoint composition while preserving routes, auth, status codes, and payload contracts. Updated compile order and endpoint imports to reference the new slice and moved shared `getUserId` plus fail-fast unit-result handling into root `Api/Helpers.fs`.
- Issues encountered: None
- Learnings: Feature migrations should centralize shared handler utilities in `Api/Helpers.fs` during the first migration that needs them, which reduces duplication and keeps upcoming feature moves within dependency rules.

### Improve: Review Collections migration against this specification and `docs/handlers-refactoring/rulebook.md`; apply targeted fixes, then re-run verification commands.
- Files changed: docs/handlers-refactoring/spec.md
- What was done: Reviewed the Collections slice against the vertical file structure, feature dependency boundaries, API-contract compatibility requirements, and fail-fast helper rule; no additional code changes were required because the migration already complied. Re-ran `dotnet fantomas . && dotnet build && dotnet test` to verify formatting, compilation, and test stability.
- Issues encountered: None
- Learnings: The Collections slice now serves as a stable reference for remaining feature migrations by demonstrating the expected separation of types, pure mappers, thin handlers, and root helper reuse without cross-feature API imports.

### Implement: Migrate CollectionsHierarchy feature by strictly following this specification and `docs/handlers-refactoring/rulebook.md`. Run verification commands after implementation.
- Files changed: Wordfolio.Api/Wordfolio.Api/Api/CollectionsHierarchy/Types.fs, Wordfolio.Api/Wordfolio.Api/Api/CollectionsHierarchy/Mappers.fs, Wordfolio.Api/Wordfolio.Api/Api/CollectionsHierarchy/Handlers.fs, Wordfolio.Api/Wordfolio.Api/Handlers/CollectionsHierarchy.fs, Wordfolio.Api/Wordfolio.Api/Program.fs, Wordfolio.Api/Wordfolio.Api/Wordfolio.Api.fsproj, Wordfolio.Api/Wordfolio.Api.Tests/CollectionsHierarchyTests.fs, docs/handlers-refactoring/spec.md
- What was done: Migrated CollectionsHierarchy into `Api/CollectionsHierarchy` by extracting transport contracts and pure mapping/query conversion functions into dedicated Types and Mappers modules, then wired endpoint composition in the new Handlers module. Updated compile order and Program imports to use the new slice, removed the legacy flat handler file, and replaced unit-result default fallbacks with shared fail-fast helper usage.
- Issues encountered: None
- Learnings: For hierarchy endpoints with unit-error operation results, `Api/Helpers.failOnUnitError` preserves structural refactoring goals while aligning with the explicit fail-fast rule and avoiding hidden fallback payloads.

### Improve: Review CollectionsHierarchy migration against this specification and `docs/handlers-refactoring/rulebook.md`; apply targeted fixes, then re-run verification commands.
- Files changed: Wordfolio.Api/Wordfolio.Api/Api/CollectionsHierarchy/Types.fs, docs/handlers-refactoring/spec.md
- What was done: Reviewed the CollectionsHierarchy slice against the rulebook and removed unused API response type aliases so the feature `Types.fs` contains only concrete transport contracts used by handlers/tests. Re-ran `dotnet fantomas . && dotnet build && dotnet test` to verify formatting, compile order integrity, and full test compatibility.
- Issues encountered: None
- Learnings: Improvement passes should trim leftover compatibility scaffolding (like redundant aliases) when it does not affect wire contracts, which keeps vertical slices explicit and easier to maintain.

### Implement: Migrate Vocabularies feature by strictly following this specification and `docs/handlers-refactoring/rulebook.md`. Run verification commands after implementation.
- Files changed: Wordfolio.Api/Wordfolio.Api/Api/Vocabularies/Types.fs, Wordfolio.Api/Wordfolio.Api/Api/Vocabularies/Mappers.fs, Wordfolio.Api/Wordfolio.Api/Api/Vocabularies/Handlers.fs, Wordfolio.Api/Wordfolio.Api/Handlers/Vocabularies.fs, Wordfolio.Api/Wordfolio.Api/Program.fs, Wordfolio.Api/Wordfolio.Api/Wordfolio.Api.fsproj, Wordfolio.Api/Wordfolio.Api.Tests/VocabulariesTests.fs, docs/handlers-refactoring/spec.md
- What was done: Migrated Vocabularies into `Api/Vocabularies` by extracting transport contracts and pure mapping functions into dedicated Types and Mappers modules and keeping endpoint composition plus error-to-response mapping in the new Handlers module. Updated compile order and Program imports to use the new vertical slice, removed the legacy flat handler file, and kept all route/auth/status/payload behavior unchanged.
- Issues encountered: None
- Learnings: The Vocabularies migration follows the same stable feature pattern as prior slices, and test updates were limited to namespace/type import relocation because API wire contracts remained intact.

### Improve: Review Vocabularies migration against this specification and `docs/handlers-refactoring/rulebook.md`; apply targeted fixes, then re-run verification commands.
- Files changed: docs/handlers-refactoring/spec.md
- What was done: Reviewed the Vocabularies slice against vertical structure, feature-boundary constraints, API-compatibility requirements, and handler/type/mapper separation rules and confirmed it already complied without requiring code changes. Re-ran `dotnet fantomas . && dotnet build && dotnet test` to verify formatting, compilation, and full test stability.
- Issues encountered: None
- Learnings: The Vocabularies slice is a clean baseline for remaining migrations, showing that preserving route and response contracts can coexist with strict feature isolation and explicit API contracts.

### Implement: Migrate Entries feature by strictly following this specification and `docs/handlers-refactoring/rulebook.md`. Run verification commands after implementation.
- Files changed: Wordfolio.Api/Wordfolio.Api/Api/Entries/Types.fs, Wordfolio.Api/Wordfolio.Api/Api/Entries/Mappers.fs, Wordfolio.Api/Wordfolio.Api/Api/Entries/Handlers.fs, Wordfolio.Api/Wordfolio.Api/Handlers/Entries.fs, Wordfolio.Api/Wordfolio.Api/Handlers/Drafts.fs, Wordfolio.Api/Wordfolio.Api/Program.fs, Wordfolio.Api/Wordfolio.Api/Wordfolio.Api.fsproj, Wordfolio.Api/Wordfolio.Api.Tests/EntriesTests.fs, Wordfolio.Api/Wordfolio.Api.Tests/DraftsTests.fs, docs/handlers-refactoring/spec.md
- What was done: Migrated Entries into `Api/Entries` by splitting transport contracts, pure mapping functions, and endpoint composition into dedicated Types, Mappers, and Handlers modules while preserving routes, auth requirements, status codes, and payload shapes. Updated compile order and Program imports to the new module path, removed the legacy flat Entries handler file, and adjusted Drafts/test namespace references to keep behavior unchanged.
- Issues encountered: None
- Learnings: Entries contracts and mapper functions are shared by Drafts today, so during incremental migration Drafts can temporarily consume `Api/Entries` modules without introducing `Api.<Feature>` to `Api.<Feature>` imports.
