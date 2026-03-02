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
- [ ] Implement: Migrate Auth feature by strictly following this specification and `docs/handlers-refactoring/rulebook.md`. Run verification commands after implementation.
- [ ] Improve: Review Auth migration against this specification and `docs/handlers-refactoring/rulebook.md`; apply targeted fixes, then re-run verification commands.

### 2. Collections Feature
- [ ] Implement: Migrate Collections feature by strictly following this specification and `docs/handlers-refactoring/rulebook.md`. Run verification commands after implementation.
- [ ] Improve: Review Collections migration against this specification and `docs/handlers-refactoring/rulebook.md`; apply targeted fixes, then re-run verification commands.

### 3. CollectionsHierarchy Feature
- [ ] Implement: Migrate CollectionsHierarchy feature by strictly following this specification and `docs/handlers-refactoring/rulebook.md`. Run verification commands after implementation.
- [ ] Improve: Review CollectionsHierarchy migration against this specification and `docs/handlers-refactoring/rulebook.md`; apply targeted fixes, then re-run verification commands.

### 4. Vocabularies Feature
- [ ] Implement: Migrate Vocabularies feature by strictly following this specification and `docs/handlers-refactoring/rulebook.md`. Run verification commands after implementation.
- [ ] Improve: Review Vocabularies migration against this specification and `docs/handlers-refactoring/rulebook.md`; apply targeted fixes, then re-run verification commands.

### 5. Entries Feature
- [ ] Implement: Migrate Entries feature by strictly following this specification and `docs/handlers-refactoring/rulebook.md`. Run verification commands after implementation.
- [ ] Improve: Review Entries migration against this specification and `docs/handlers-refactoring/rulebook.md`; apply targeted fixes, then re-run verification commands.

### 6. Drafts Feature
- [ ] Implement: Migrate Drafts feature by strictly following this specification and `docs/handlers-refactoring/rulebook.md`. Run verification commands after implementation.
- [ ] Improve: Review Drafts migration against this specification and `docs/handlers-refactoring/rulebook.md`; apply targeted fixes, then re-run verification commands.

### 7. Dictionary Feature
- [ ] Implement: Migrate Dictionary feature by strictly following this specification and `docs/handlers-refactoring/rulebook.md`. Run verification commands after implementation.
- [ ] Improve: Review Dictionary migration against this specification and `docs/handlers-refactoring/rulebook.md`; apply targeted fixes, then re-run verification commands.

## Progress Log

_No entries yet._
