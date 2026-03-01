# Data Access Refactoring

## Overview

This refactor aligns the data access layer with the completed domain refactor while preserving runtime behavior. The goal is to make naming consistent with the domain model, remove dead data access APIs, inline Dapper table declarations to reduce cross-module coupling, and make data access tests easier to own by splitting them into self-contained function-focused files.

The loop executes one data access file per iteration unit. Each implement step updates exactly one production file in `Wordfolio.Api.DataAccess` plus all related tests (and required call-site/project updates), then an improve step validates quality and parity for that same unit.

## Specification

## Scope and Constraints

- In scope:
  - `Wordfolio.Api/Wordfolio.Api.DataAccess/*.fs`
  - `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/**`
  - Required call-site updates in `Wordfolio.Api/Wordfolio.Api/Infrastructure/Environment.fs`
  - Required call-site updates in `Wordfolio.Api/Wordfolio.Api/IdentityIntegration.fs`
  - Project include updates in `Wordfolio.Api/Wordfolio.Api.DataAccess/Wordfolio.Api.DataAccess.fsproj`
  - Project include updates in `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Wordfolio.Api.DataAccess.Tests.fsproj`
- Out of scope:
  - Frontend changes
  - Domain-layer behavior changes
  - API contract behavior changes
- Hard constraints:
  - No behavior changes (query semantics, business rules, response behavior remain equivalent)
  - Keep production file structure in `Wordfolio.Api.DataAccess` as-is (do not split production functions into separate files)
  - Tests must be self-contained per file (no shared seed/setup helper files)
  - Test file names must not include `Async`

## Pattern to Follow

- Domain refactor naming and test-organization reference commit:
  - `1a71b727cf04d1d6cd82ca52364f8fa36ada0a51`
- Domain per-operation test split examples:
  - `Wordfolio.Api/Wordfolio.Api.Domain.Tests/Collections/CreateTests.fs`
  - `Wordfolio.Api/Wordfolio.Api.Domain.Tests/Entries/EntryOperations/GetByIdTests.fs`
- Data access integration-test fixture pattern:
  - `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/UsersTests.fs`
  - `Wordfolio.Api/Wordfolio.Api.Tests.Utils/WordfolioTestFixture.fs`
  - `Wordfolio.Api/Wordfolio.Api.Tests.Utils/Wordfolio/Seeder.fs`
- Rule source:
  - `docs/data-access-refactoring/rulebook.md`

## Naming and Reachability Rules

- Align data access type/function/property names with renamed domain concepts when they represent the same business meaning.
- Remove data access functions with no production call path (test-only APIs are considered dead for this refactor).
- Keep names explicit and intent-focused (avoid mixed naming schemes for equivalent patterns).
- Keep updates mechanical and parity-preserving; when renaming, update all call sites in the same step.

## Dapper and Record Rules

- Inline Dapper table declarations into the function that uses them.
- Remove cross-module table coupling (for example, one module using another module's internal table declaration).
- Re-evaluate `[<CLIMutable>]` on every record and keep it only where required for Dapper materialization.
- Remove dead private declarations discovered during each module refactor.

## Test Restructure Rules

- Split monolithic module test files into function-focused files under module folders.
- Keep every test file fully self-contained (fixture setup, seed setup, assertions).
- Keep existing test behavior coverage, then add missing high-value cases discovered during split.
- Keep test project compile includes explicit and ordered in `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Wordfolio.Api.DataAccess.Tests.fsproj`.

## Dead/Test-Only API Candidates (Validate Per Step)

- `Wordfolio.Api/Wordfolio.Api.DataAccess/Database.fs`
  - private helpers only, no call sites
- `Wordfolio.Api/Wordfolio.Api.DataAccess/Entries.fs`
  - `getEntryByIdAsync`
  - `getEntriesByVocabularyIdAsync`
- `Wordfolio.Api/Wordfolio.Api.DataAccess/Definitions.fs`
  - `getDefinitionsByEntryIdAsync`
  - `updateDefinitionsAsync`
  - `deleteDefinitionsAsync`
- `Wordfolio.Api/Wordfolio.Api.DataAccess/Translations.fs`
  - `getTranslationsByEntryIdAsync`
  - `updateTranslationsAsync`
  - `deleteTranslationsAsync`
- `Wordfolio.Api/Wordfolio.Api.DataAccess/Examples.fs`
  - `getExamplesByDefinitionIdAsync`
  - `getExamplesByTranslationIdAsync`
  - `updateExamplesAsync`
  - `deleteExamplesAsync`

## Target Test Folder Layout

```text
Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/
  SqlErrorCodes.fs
  Users/
    CreateUserTests.fs
  Collections/
    CreateCollectionTests.fs
    GetCollectionByIdTests.fs
    GetCollectionsByUserIdTests.fs
    UpdateCollectionTests.fs
    DeleteCollectionTests.fs
    GetDefaultCollectionByUserIdTests.fs
    CreateDefaultCollectionTests.fs
  CollectionsHierarchy/
    GetCollectionsByUserIdTests.fs
    SearchUserCollectionsTests.fs
    SearchCollectionVocabulariesTests.fs
    GetDefaultVocabularySummaryByUserIdTests.fs
  Vocabularies/
    CreateVocabularyTests.fs
    GetVocabularyByIdTests.fs
    GetVocabulariesByCollectionIdTests.fs
    UpdateVocabularyTests.fs
    DeleteVocabularyTests.fs
    GetDefaultVocabularyByUserIdTests.fs
    CreateDefaultVocabularyTests.fs
  Entries/
    CreateEntryTests.fs
    GetEntryByTextAndVocabularyIdTests.fs
    UpdateEntryTests.fs
    MoveEntryTests.fs
    DeleteEntryTests.fs
    HasVocabularyAccessTests.fs
    HasVocabularyAccessInCollectionTests.fs
  Definitions/
    CreateDefinitionsTests.fs
  Translations/
    CreateTranslationsTests.fs
  Examples/
    CreateExamplesTests.fs
  EntriesHierarchy/
    GetEntryByIdWithHierarchyTests.fs
    ClearEntryChildrenTests.fs
    GetEntriesHierarchyByVocabularyIdTests.fs
```

## Verification Commands

`dotnet fantomas . && dotnet build && dotnet test`

## Implementation Steps

### 1. Database module cleanup
- [x] Implement: Refactor `Wordfolio.Api/Wordfolio.Api.DataAccess/Database.fs` by removing dead private helpers; if the file becomes empty, delete it and remove its compile include from `Wordfolio.Api/Wordfolio.Api.DataAccess/Wordfolio.Api.DataAccess.fsproj`.
- [x] Improve: Verify there are no remaining references to `Wordfolio.Api.DataAccess.Database` and confirm this cleanup is purely structural.

### 2. Users file and users tests
- [x] Implement: Refactor `Wordfolio.Api/Wordfolio.Api.DataAccess/Users.fs` (naming alignment, table declaration inlining, `[<CLIMutable>]` review), update `Wordfolio.Api/Wordfolio.Api/IdentityIntegration.fs` call sites, split `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/UsersTests.fs` into `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Users/CreateUserTests.fs`, and update `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Wordfolio.Api.DataAccess.Tests.fsproj`.
- [x] Improve: Review users changes for parity and readability; ensure duplicate-key and happy-path coverage remain complete and self-contained.

### 3. Collections file and collections tests
- [x] Implement: Refactor `Wordfolio.Api/Wordfolio.Api.DataAccess/Collections.fs` (naming alignment, table declaration inlining, `[<CLIMutable>]` review), update `Wordfolio.Api/Wordfolio.Api/Infrastructure/Environment.fs` call sites, split `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/CollectionsTests.fs` into function-focused files under `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Collections/`, and update `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Wordfolio.Api.DataAccess.Tests.fsproj`.
- [x] Improve: Review for missing filtering and constraint coverage (`IsSystem`, foreign-key failures, non-existent ids) and keep each test file self-contained.

### 4. CollectionsHierarchy file and collections hierarchy tests
- [x] Implement: Refactor `Wordfolio.Api/Wordfolio.Api.DataAccess/CollectionsHierarchy.fs` (naming alignment to domain hierarchy types, table declaration inlining, `[<CLIMutable>]` review), update `Wordfolio.Api/Wordfolio.Api/Infrastructure/Environment.fs` call sites, split `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/CollectionsHierarchyTests.fs` into function-focused files under `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/CollectionsHierarchy/`, and update `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Wordfolio.Api.DataAccess.Tests.fsproj`.
- [x] Improve: Review for search/sort safety and coverage (escape behavior, sort direction, default vocabulary summary semantics) without changing behavior.

### 5. Vocabularies file and vocabularies tests
- [x] Implement: Refactor `Wordfolio.Api/Wordfolio.Api.DataAccess/Vocabularies.fs` (naming alignment, table declaration inlining, `[<CLIMutable>]` review), update `Wordfolio.Api/Wordfolio.Api/Infrastructure/Environment.fs` call sites, split `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/VocabulariesTests.fs` into function-focused files under `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Vocabularies/`, and update `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Wordfolio.Api.DataAccess.Tests.fsproj`.
- [x] Improve: Review default-vocabulary and collection-ownership coverage, including uniqueness/constraint cases and self-contained setup per test file.

### 6. Entries file and entries tests
- [x] Implement: Refactor `Wordfolio.Api/Wordfolio.Api.DataAccess/Entries.fs` (naming alignment, table declaration inlining, remove cross-module table dependencies, `[<CLIMutable>]` review), remove dead test-only APIs (`getEntryByIdAsync`, `getEntriesByVocabularyIdAsync`) if still unreferenced, update `Wordfolio.Api/Wordfolio.Api/Infrastructure/Environment.fs` call sites, split `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/EntriesTests.fs` into function-focused files under `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Entries/`, and update `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Wordfolio.Api.DataAccess.Tests.fsproj`.
- [x] Improve: Review access-check and move/update coverage for positive/negative boundaries while preserving existing query behavior.

### 7. Definitions file and definitions tests
- [x] Implement: Refactor `Wordfolio.Api/Wordfolio.Api.DataAccess/Definitions.fs` (naming alignment, table declaration inlining, `[<CLIMutable>]` review), remove dead test-only APIs (`getDefinitionsByEntryIdAsync`, `updateDefinitionsAsync`, `deleteDefinitionsAsync`) if unreferenced, update `Wordfolio.Api/Wordfolio.Api/Infrastructure/Environment.fs` call sites, split `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/DefinitionsTests.fs` into function-focused files under `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Definitions/`, and update `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Wordfolio.Api.DataAccess.Tests.fsproj`.
- [x] Improve: Review create-definitions coverage for ordering and constraint behavior, and verify removed dead API tests were deleted in the same step.

### 8. Translations file and translations tests
- [ ] Implement: Refactor `Wordfolio.Api/Wordfolio.Api.DataAccess/Translations.fs` (naming alignment, table declaration inlining, `[<CLIMutable>]` review), remove dead test-only APIs (`getTranslationsByEntryIdAsync`, `updateTranslationsAsync`, `deleteTranslationsAsync`) if unreferenced, update `Wordfolio.Api/Wordfolio.Api/Infrastructure/Environment.fs` call sites, split `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/TranslationsTests.fs` into function-focused files under `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Translations/`, and update `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Wordfolio.Api.DataAccess.Tests.fsproj`.
- [ ] Improve: Review create-translations coverage for ordering and constraint behavior, and verify removed dead API tests were deleted in the same step.

### 9. Examples file and examples tests
- [ ] Implement: Refactor `Wordfolio.Api/Wordfolio.Api.DataAccess/Examples.fs` (naming alignment, table declaration inlining, `[<CLIMutable>]` review), remove dead test-only APIs (`getExamplesByDefinitionIdAsync`, `getExamplesByTranslationIdAsync`, `updateExamplesAsync`, `deleteExamplesAsync`) if unreferenced, update `Wordfolio.Api/Wordfolio.Api/Infrastructure/Environment.fs` call sites, split `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/ExamplesTests.fs` into function-focused files under `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Examples/`, and update `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Wordfolio.Api.DataAccess.Tests.fsproj`.
- [ ] Improve: Review create-examples coverage for definition/translation branches and constraint behavior, and verify removed dead API tests were deleted in the same step.

### 10. EntriesHierarchy file and entries hierarchy tests
- [ ] Implement: Refactor `Wordfolio.Api/Wordfolio.Api.DataAccess/EntriesHierarchy.fs` (naming alignment, table declaration inlining, remove cross-module table dependencies, remove unused `examplesTable`, `[<CLIMutable>]` review), update `Wordfolio.Api/Wordfolio.Api/Infrastructure/Environment.fs` call sites, split `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/EntriesHierarchyTests.fs` into function-focused files under `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/EntriesHierarchy/`, and update `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Wordfolio.Api.DataAccess.Tests.fsproj`.
- [ ] Improve: Review hierarchy assembly and child-clear coverage for empty/partial hierarchies and ensure no behavior drift.

## Progress Log

### Database module cleanup
- Files changed: `Wordfolio.Api/Wordfolio.Api.DataAccess/Database.fs`, `Wordfolio.Api/Wordfolio.Api.DataAccess/Wordfolio.Api.DataAccess.fsproj`
- What was done: Removed the unused `Database.fs` module because it only contained dead private helpers with no consumers. Deleted its compile include from the data access project file.
- Issues encountered: None
- Learnings: This module had no production or test call paths, so removing the file and fsproj include is the clean parity-preserving outcome for dead private helper cleanup.

### Verify no remaining Database module references
- Files changed: `docs/data-access-refactoring/spec.md`
- What was done: Searched the repository for `Wordfolio.Api.DataAccess.Database` references and found none outside this spec text. Confirmed the prior Database cleanup remains a structural-only change with no behavior impact.
- Issues encountered: None
- Learnings: A repo-wide symbol search is sufficient to validate dead-module cleanup for this module because the removed code had no runtime call paths.

### Users file and users tests
- Files changed: `Wordfolio.Api/Wordfolio.Api.DataAccess/Users.fs`, `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Users/CreateUserTests.fs`, `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/UsersTests.fs`, `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Wordfolio.Api.DataAccess.Tests.fsproj`, `docs/data-access-refactoring/spec.md`
- What was done: Inlined the users table declaration into `createUserAsync` and split the monolithic users data access tests into a self-contained `Users/CreateUserTests.fs` file with the existing happy-path and duplicate-key coverage preserved. Updated test project compile includes to point at the new test file and removed the old file.
- Issues encountered: None
- Learnings: For single-operation modules, the per-function test split is a direct file move while preserving fixture and seeder usage unchanged.

### Review users changes for parity and readability
- Files changed: `docs/data-access-refactoring/spec.md`
- What was done: Reviewed `Users.fs`, `IdentityIntegration.fs`, and `Users/CreateUserTests.fs` for behavior parity and readability, and confirmed happy-path plus duplicate-key coverage remain present in a self-contained test file. Re-ran full verification with `dotnet fantomas . && dotnet build && dotnet test` and all checks passed.
- Issues encountered: None
- Learnings: Improve steps can be validation-only when implement output already satisfies rulebook concerns; the required outcome is explicit parity confirmation plus green verification.

### Collections file and collections tests
- Files changed: `Wordfolio.Api/Wordfolio.Api.DataAccess/Collections.fs`, `Wordfolio.Api/Wordfolio.Api/Infrastructure/Environment.fs`, `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Collections/CreateCollectionTests.fs`, `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Collections/GetCollectionByIdTests.fs`, `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Collections/GetCollectionsByUserIdTests.fs`, `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Collections/UpdateCollectionTests.fs`, `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Collections/DeleteCollectionTests.fs`, `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Collections/GetDefaultCollectionByUserIdTests.fs`, `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Collections/CreateDefaultCollectionTests.fs`, `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/CollectionsTests.fs`, `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Wordfolio.Api.DataAccess.Tests.fsproj`, `docs/data-access-refactoring/spec.md`
- What was done: Renamed collection data access parameter records to `CreateCollectionParameters` and `UpdateCollectionParameters` to align with domain naming, and inlined table declarations into each collection function while keeping query behavior unchanged. Split monolithic collections tests into self-contained, function-focused files under `Collections/` and updated the test project compile includes accordingly.
- Issues encountered: None
- Learnings: Collections function-level test splitting maps cleanly to one file per public data access function, and local table declarations remove shared module state without changing SQL behavior.

### Review collections filtering and constraint coverage
- Files changed: `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Collections/CreateDefaultCollectionTests.fs`, `docs/data-access-refactoring/spec.md`
- What was done: Reviewed all split collections test files for `IsSystem` filtering and non-existent-id behavior coverage, and added a missing foreign-key constraint test for `createDefaultCollectionAsync` with a non-existent user. Kept the test self-contained with its own fixture reset, inputs, and assertions.
- Issues encountered: None
- Learnings: Improve steps can stay narrowly targeted by only filling concrete coverage gaps found during review while preserving existing behavior-focused assertions.

### CollectionsHierarchy file and collections hierarchy tests
- Files changed: `Wordfolio.Api/Wordfolio.Api.DataAccess/CollectionsHierarchy.fs`, `Wordfolio.Api/Wordfolio.Api/Infrastructure/Environment.fs`, `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/CollectionsHierarchy/GetCollectionsByUserIdTests.fs`, `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/CollectionsHierarchy/SearchUserCollectionsTests.fs`, `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/CollectionsHierarchy/SearchCollectionVocabulariesTests.fs`, `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/CollectionsHierarchy/GetDefaultVocabularySummaryByUserIdTests.fs`, `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/CollectionsHierarchyTests.fs`, `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Wordfolio.Api.DataAccess.Tests.fsproj`, `docs/data-access-refactoring/spec.md`
- What was done: Renamed collections-hierarchy data access DTO/query/sort types to align with domain hierarchy naming and updated `Environment.fs` mapping call sites to the new names without changing query behavior. Split the monolithic `CollectionsHierarchyTests.fs` into four self-contained function-focused test files under `CollectionsHierarchy/` and updated the test project compile includes.
- Issues encountered: Initial verification failed due to a test type-name collision with an existing collections test type in the same namespace; resolved by renaming the new test class types to unique names.
- Learnings: When splitting data access tests under a shared namespace, test class type names must remain globally unique across the project even when file paths differ.

### Review collections hierarchy search/sort safety and coverage
- Files changed: `docs/data-access-refactoring/spec.md`
- What was done: Reviewed `CollectionsHierarchy.fs` and its split test files to confirm wildcard escaping coverage (`%`, `_`, `\`), sort-direction coverage across searchable collection and vocabulary queries, and default-vocabulary summary behavior for present/missing/cross-user scenarios. No production or test code changes were required because existing coverage already satisfied the improve-step concerns.
- Issues encountered: None
- Learnings: Improve steps may be validation-only when the preceding implement step already includes comprehensive parity coverage for the required behavioral dimensions.

### Vocabularies file and vocabularies tests
- Files changed: `Wordfolio.Api/Wordfolio.Api.DataAccess/Vocabularies.fs`, `Wordfolio.Api/Wordfolio.Api/Infrastructure/Environment.fs`, `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Vocabularies/CreateVocabularyTests.fs`, `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Vocabularies/GetVocabularyByIdTests.fs`, `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Vocabularies/GetVocabulariesByCollectionIdTests.fs`, `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Vocabularies/UpdateVocabularyTests.fs`, `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Vocabularies/DeleteVocabularyTests.fs`, `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Vocabularies/GetDefaultVocabularyByUserIdTests.fs`, `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Vocabularies/CreateDefaultVocabularyTests.fs`, `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/VocabulariesTests.fs`, `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Wordfolio.Api.DataAccess.Tests.fsproj`, `docs/data-access-refactoring/spec.md`
- What was done: Renamed vocabulary parameter records to `CreateVocabularyParameters` and `UpdateVocabularyParameters`, and inlined the vocabularies/collections table declarations into each vocabulary data access function without changing query semantics. Split the monolithic vocabularies test file into seven self-contained function-focused files under `Vocabularies/` and updated the test project compile includes.
- Issues encountered: None
- Learnings: The collections-step pattern applies directly to vocabularies: keep module-level record mappers intact, inline table declarations per function, and map one public function to one self-contained test file.

### Review vocabularies default and ownership coverage
- Files changed: `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Vocabularies/CreateDefaultVocabularyTests.fs`, `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Vocabularies/GetDefaultVocabularyByUserIdTests.fs`, `docs/data-access-refactoring/spec.md`
- What was done: Reviewed the split vocabularies tests for default-vocabulary behavior, uniqueness handling, and ownership boundaries, then added a foreign-key constraint test for `createDefaultVocabularyAsync` and a cross-user ownership-boundary test for `getDefaultVocabularyByUserIdAsync`. Both additions keep each test file self-contained with local reset/seed/assert flow.
- Issues encountered: None
- Learnings: The improve pass should focus on boundary completeness for already split tests; targeted constraint and ownership assertions close parity gaps without touching production query behavior.

### Entries file and entries tests
- Files changed: `Wordfolio.Api/Wordfolio.Api.DataAccess/Entries.fs`, `Wordfolio.Api/Wordfolio.Api/Infrastructure/Environment.fs`, `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Entries/CreateEntryTests.fs`, `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Entries/GetEntryByTextAndVocabularyIdTests.fs`, `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Entries/UpdateEntryTests.fs`, `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Entries/MoveEntryTests.fs`, `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Entries/DeleteEntryTests.fs`, `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Entries/HasVocabularyAccessTests.fs`, `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Entries/HasVocabularyAccessInCollectionTests.fs`, `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/EntriesTests.fs`, `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Wordfolio.Api.DataAccess.Tests.fsproj`, `docs/data-access-refactoring/spec.md`
- What was done: Renamed entry parameter records to `CreateEntryParameters`, `UpdateEntryParameters`, and `MoveEntryParameters`, inlined table declarations into each entries function, and removed the dead test-only APIs `getEntryByIdAsync` and `getEntriesByVocabularyIdAsync`. Eliminated the cross-module dependency on `Vocabularies.vocabulariesTable`, updated environment call-site type usage, and split `EntriesTests.fs` into self-contained function-focused files under `Entries/` with test project includes updated.
- Issues encountered: None
- Learnings: For access-check functions, introducing local query-only record/table declarations removes cross-module table coupling cleanly while preserving SQL and return behavior.

### Review entries access-check and move/update boundary coverage
- Files changed: `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Entries/UpdateEntryTests.fs`, `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Entries/MoveEntryTests.fs`, `docs/data-access-refactoring/spec.md`
- What was done: Reviewed the entries split tests for positive/negative boundary coverage and added targeted parity checks for update and move behavior: a test ensuring `updateEntryAsync` only modifies the targeted row and a test asserting `moveEntryAsync` preserves foreign-key constraint behavior when the target vocabulary does not exist. These additions keep existing query semantics unchanged and maintain self-contained test setup in each file.
- Issues encountered: None
- Learnings: For improve steps on write-path functions, high-value boundary checks are row-target isolation and constraint-surface verification because they confirm SQL `WHERE` scope and database-enforced invariants without altering production code.

### Definitions file and definitions tests
- Files changed: `Wordfolio.Api/Wordfolio.Api.DataAccess/Definitions.fs`, `Wordfolio.Api/Wordfolio.Api/Infrastructure/Environment.fs`, `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Definitions/CreateDefinitionsTests.fs`, `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/DefinitionsTests.fs`, `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/ExamplesTests.fs`, `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Wordfolio.Api.DataAccess.Tests.fsproj`, `docs/data-access-refactoring/spec.md`
- What was done: Renamed definitions create input type to `CreateDefinitionParameters`, inlined insert table declaration into `createDefinitionsAsync`, and removed dead test-only APIs (`getDefinitionsByEntryIdAsync`, `updateDefinitionsAsync`, `deleteDefinitionsAsync`) that had no production call paths. Split monolithic definitions tests into a self-contained function-focused `Definitions/CreateDefinitionsTests.fs` file, updated test project includes, and adjusted the examples cascade test to avoid relying on removed dead APIs.
- Issues encountered: None
- Learnings: When dead data-access APIs are removed, any cross-module tests using them should be updated in the same step so the refactor stays parity-preserving and keeps test intent while eliminating test-only production surface area.

### Review create-definitions coverage and dead API test removal
- Files changed: `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Definitions/CreateDefinitionsTests.fs`, `docs/data-access-refactoring/spec.md`
- What was done: Reviewed definitions improve concerns and confirmed dead API tests are no longer present by validating only `Definitions/CreateDefinitionsTests.fs` remains for definitions data access tests. Added a targeted ordering-parity test that verifies `createDefinitionsAsync` returns IDs in the same order as input parameters while keeping existing foreign-key and unique-constraint coverage intact.
- Issues encountered: None
- Learnings: For bulk-insert functions, pairing constraint tests with an explicit output-order assertion keeps improve-step coverage aligned with parity goals without changing production logic.
