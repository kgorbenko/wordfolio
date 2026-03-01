# Data Access Refactor Code Review

## Overview

This review plan is for a Ralph loop that performs a parity audit of the data access refactor on the current branch against `origin/main`. The goal is to provide merge-confidence evidence, file by file, that refactoring did not change behavior, did not reduce effective test coverage, and preserved moved tests during the monolith-to-split test restructuring.

The loop is review-only. It does not modify production or test code. Each iteration reviews exactly one data access module, updates checklist state, and appends evidence to this file's Progress Log.

## Specification

### Scope and Constraints

- In scope paths:
  - `Wordfolio.Api/Wordfolio.Api.DataAccess/*.fs`
  - `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/**`
  - `Wordfolio.Api/Wordfolio.Api/Infrastructure/Environment.fs`
- Baseline for comparisons: `origin/main`.
- Review mode is documentation-only; do not edit production or test files.
- Completion policy: all modules must be reviewed; gaps are allowed only if explicitly documented with actionable follow-up.

### Pattern to Follow

- Module order pattern (source of truth): `Wordfolio.Api/Wordfolio.Api.DataAccess/Wordfolio.Api.DataAccess.fsproj`.
- Test include pattern (post-refactor split layout): `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Wordfolio.Api.DataAccess.Tests.fsproj`.
- Existing per-operation split style reference: `Wordfolio.Api/Wordfolio.Api.Domain.Tests/**`.
- Integration mapping reference for data access calls: `Wordfolio.Api/Wordfolio.Api/Infrastructure/Environment.fs`.

### Review Evidence Checklist (Apply to Every Reviewed File)

For each production data access file under review, record evidence for all three criteria:

1. **Behavior parity**
   - Compare current file against `origin/main` and verify changes are structural/mechanical only.
   - Confirm query/filter/order semantics and return-shape behavior are unchanged.
   - For removed APIs, prove they were dead from production call paths.
2. **Coverage parity**
   - Use baseline-to-current test mapping to verify that pre-refactor test intent is still covered.
   - Coverage parity is judged by scenario mapping, not numeric coverage tooling.
3. **Moved tests preserved**
   - Compare baseline test cases (`member _.``...```) to split files.
   - If a test name or location changed, document explicit one-to-one mapping and why it is mechanical.

### Per-Iteration Baseline Procedure (Mandatory for Every Module)

At the start of every module step, perform and document these baseline checks for that module before concluding parity:

- Confirm module scope and baseline target file at `origin/main`.
- Confirm baseline test source file and current split destination(s) from this spec.
- Compare baseline and current test-case names (`member _.``...```) and document exact preservation/mapping.
- Compare module behavior surfaces (queries, filters, sorting, return shape, constraints, and removed APIs).
- Check `Wordfolio.Api/Wordfolio.Api/Infrastructure/Environment.fs` call-site parity when that module is consumed there.

### Baseline Test Mapping

- `Wordfolio.Api/Wordfolio.Api.DataAccess/Users.fs`
  - baseline: `origin/main:Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/UsersTests.fs`
  - current: `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Users/CreateUserTests.fs`
- `Wordfolio.Api/Wordfolio.Api.DataAccess/Collections.fs`
  - baseline: `origin/main:Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/CollectionsTests.fs`
  - current: `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Collections/*.fs`
- `Wordfolio.Api/Wordfolio.Api.DataAccess/CollectionsHierarchy.fs`
  - baseline: `origin/main:Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/CollectionsHierarchyTests.fs`
  - current: `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/CollectionsHierarchy/*.fs`
- `Wordfolio.Api/Wordfolio.Api.DataAccess/Vocabularies.fs`
  - baseline: `origin/main:Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/VocabulariesTests.fs`
  - current: `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Vocabularies/*.fs`
- `Wordfolio.Api/Wordfolio.Api.DataAccess/Entries.fs`
  - baseline: `origin/main:Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/EntriesTests.fs`
  - current: `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Entries/*.fs`
- `Wordfolio.Api/Wordfolio.Api.DataAccess/Definitions.fs`
  - baseline: `origin/main:Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/DefinitionsTests.fs`
  - current: `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Definitions/CreateDefinitionsTests.fs`
- `Wordfolio.Api/Wordfolio.Api.DataAccess/Translations.fs`
  - baseline: `origin/main:Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/TranslationsTests.fs`
  - current: `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Translations/CreateTranslationsTests.fs`
- `Wordfolio.Api/Wordfolio.Api.DataAccess/Examples.fs`
  - baseline: `origin/main:Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/ExamplesTests.fs`
  - current: `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Examples/CreateExamplesTests.fs`
- `Wordfolio.Api/Wordfolio.Api.DataAccess/EntriesHierarchy.fs`
  - baseline: `origin/main:Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/EntriesHierarchyTests.fs`
  - current: `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/EntriesHierarchy/*.fs`
- Removed production file check:
  - `origin/main:Wordfolio.Api/Wordfolio.Api.DataAccess/Database.fs` (removed in refactor branch)

### File Manifest

- Production files to review:
  - `Wordfolio.Api/Wordfolio.Api.DataAccess/Users.fs`
  - `Wordfolio.Api/Wordfolio.Api.DataAccess/Collections.fs`
  - `Wordfolio.Api/Wordfolio.Api.DataAccess/CollectionsHierarchy.fs`
  - `Wordfolio.Api/Wordfolio.Api.DataAccess/Vocabularies.fs`
  - `Wordfolio.Api/Wordfolio.Api.DataAccess/Entries.fs`
  - `Wordfolio.Api/Wordfolio.Api.DataAccess/Definitions.fs`
  - `Wordfolio.Api/Wordfolio.Api.DataAccess/Translations.fs`
  - `Wordfolio.Api/Wordfolio.Api.DataAccess/Examples.fs`
  - `Wordfolio.Api/Wordfolio.Api.DataAccess/EntriesHierarchy.fs`
  - Removed-file parity: `origin/main:Wordfolio.Api/Wordfolio.Api.DataAccess/Database.fs`
- Cross-file integration checks:
  - `Wordfolio.Api/Wordfolio.Api/Infrastructure/Environment.fs`

## Verification Commands

`git rev-parse --verify origin/main && git diff --name-only origin/main...HEAD -- Wordfolio.Api/Wordfolio.Api.DataAccess Wordfolio.Api/Wordfolio.Api.DataAccess.Tests Wordfolio.Api/Wordfolio.Api/Infrastructure/Environment.fs > /dev/null`

## Implementation Steps

### 1. Review Database module removal parity
- [x] Review `origin/main:Wordfolio.Api/Wordfolio.Api.DataAccess/Database.fs` as a removed module and verify parity evidence: no remaining compile include in `Wordfolio.Api/Wordfolio.Api.DataAccess/Wordfolio.Api.DataAccess.fsproj`, no production call sites, and no baseline test coverage dependency on its private helpers.

### 2. Review Users module parity
- [x] Review `Wordfolio.Api/Wordfolio.Api.DataAccess/Users.fs` against `origin/main`, verify baseline tests from `origin/main:Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/UsersTests.fs` are preserved in `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Users/CreateUserTests.fs`, and document behavior and coverage parity evidence.

### 3. Review Collections module parity
- [x] Review `Wordfolio.Api/Wordfolio.Api.DataAccess/Collections.fs` against `origin/main`, verify baseline tests from `origin/main:Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/CollectionsTests.fs` are preserved across `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Collections/*.fs`, and include `Wordfolio.Api/Wordfolio.Api/Infrastructure/Environment.fs` call-site parity for collection and default-collection flows.

### 4. Review CollectionsHierarchy module parity
- [x] Review `Wordfolio.Api/Wordfolio.Api.DataAccess/CollectionsHierarchy.fs` against `origin/main`, verify baseline tests from `origin/main:Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/CollectionsHierarchyTests.fs` are preserved across `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/CollectionsHierarchy/*.fs`, and include `Wordfolio.Api/Wordfolio.Api/Infrastructure/Environment.fs` query/sort mapping parity checks.

### 5. Review Vocabularies module parity
- [x] Review `Wordfolio.Api/Wordfolio.Api.DataAccess/Vocabularies.fs` against `origin/main`, verify baseline tests from `origin/main:Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/VocabulariesTests.fs` are preserved across `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Vocabularies/*.fs`, and include `Wordfolio.Api/Wordfolio.Api/Infrastructure/Environment.fs` default-vocabulary call-site parity checks.

### 6. Review Entries module parity
- [x] Review `Wordfolio.Api/Wordfolio.Api.DataAccess/Entries.fs` against `origin/main`, verify baseline tests from `origin/main:Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/EntriesTests.fs` are preserved across `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Entries/*.fs`, include `Wordfolio.Api/Wordfolio.Api/Infrastructure/Environment.fs` call-site parity checks, and document dead-API removals as production-unreachable if applicable.

### 7. Review Definitions module parity
- [x] Review `Wordfolio.Api/Wordfolio.Api.DataAccess/Definitions.fs` against `origin/main`, verify baseline tests from `origin/main:Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/DefinitionsTests.fs` are preserved in `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Definitions/CreateDefinitionsTests.fs`, include `Wordfolio.Api/Wordfolio.Api/Infrastructure/Environment.fs` call-site parity checks, and document removed dead APIs with reachability evidence.

### 8. Review Translations module parity
- [x] Review `Wordfolio.Api/Wordfolio.Api.DataAccess/Translations.fs` against `origin/main`, verify baseline tests from `origin/main:Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/TranslationsTests.fs` are preserved in `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Translations/CreateTranslationsTests.fs`, include `Wordfolio.Api/Wordfolio.Api/Infrastructure/Environment.fs` call-site parity checks, and document removed dead APIs with reachability evidence.

### 9. Review Examples module parity
- [x] Review `Wordfolio.Api/Wordfolio.Api.DataAccess/Examples.fs` against `origin/main`, verify baseline tests from `origin/main:Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/ExamplesTests.fs` are preserved in `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Examples/CreateExamplesTests.fs`, include `Wordfolio.Api/Wordfolio.Api/Infrastructure/Environment.fs` call-site parity checks, and document removed dead APIs with reachability evidence.

### 10. Review EntriesHierarchy module parity and finalize matrix
- [x] Review `Wordfolio.Api/Wordfolio.Api.DataAccess/EntriesHierarchy.fs` against `origin/main`, verify baseline tests from `origin/main:Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/EntriesHierarchyTests.fs` are preserved across `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/EntriesHierarchy/*.fs`, include `Wordfolio.Api/Wordfolio.Api/Infrastructure/Environment.fs` hierarchy call-site parity checks, and add a final matrix in Progress Log with one row per production module and explicit statuses for behavior parity, coverage parity, and moved-tests preserved.

## Progress Log

Agents append entries here after completing each step.

### Review Database module removal parity
- Files changed: `docs/data-access-refactor-code-review/spec.md`
- What was done: Compared `origin/main:Wordfolio.Api/Wordfolio.Api.DataAccess/Database.fs` and `origin/main:Wordfolio.Api/Wordfolio.Api.DataAccess/Wordfolio.Api.DataAccess.fsproj` to current state and confirmed the `Database.fs` compile include was removed while all remaining module includes persist. Verified parity evidence via baseline/current call-site searches (`Wordfolio.Api/Wordfolio.Api`, `Wordfolio.Api/Wordfolio.Api.DataAccess`, `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests`) showing no production references beyond the removed file include and no baseline test dependency on private helpers (`withConnectionAsync`, `inTransactionAsync`).
- Issues encountered: None
- Learnings: The removed `Database` module exposed only private helpers in baseline, so removal is behavior-neutral when compile inclusion and call-site reachability are both absent; for removed modules, test-parity proof is a negative mapping (no baseline `member _.``...``` ownership for that module) backed by targeted symbol searches.

### Review Users module parity
- Files changed: `docs/data-access-refactor-code-review/spec.md`
- What was done: Confirmed module scope (`Wordfolio.Api/Wordfolio.Api.DataAccess/Users.fs`) against baseline `origin/main` and verified behavior parity from diff evidence showing only a mechanical move of `usersTable` from module scope into `createUserAsync` with identical insert semantics (same table, values, and return shape). Verified coverage and moved-test parity by mapping baseline `origin/main:Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/UsersTests.fs` to `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Users/CreateUserTests.fs` with exact preservation of both test cases (`createUserAsync inserts a row`, `createUserAsync fails on duplicate Id`), and confirmed `Wordfolio.Api/Wordfolio.Api/Infrastructure/Environment.fs` has no `Users` data-access call sites in baseline or current.
- Issues encountered: None
- Learnings: For single-function modules, parity proof can be established by showing unchanged SQL operation shape plus one-to-one test-name preservation; when a module is not consumed by `Environment.fs`, call-site parity should be recorded explicitly as absent in both baseline and current.

### Review Collections module parity
- Files changed: `docs/data-access-refactor-code-review/spec.md`
- What was done: Confirmed module scope (`Wordfolio.Api/Wordfolio.Api.DataAccess/Collections.fs`) against `origin/main` and verified behavior parity via diff: only mechanical refactor changes (parameter type renames and moving table bindings from module scope into each function) with unchanged query/filter semantics (`IsSystem` guards, user/id predicates), write behavior, and return shapes for collection and default-collection flows. Verified moved-tests and coverage parity by mapping baseline `origin/main:Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/CollectionsTests.fs` test names to `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Collections/*.fs` with exact preservation of all 20 baseline cases plus one additive case (`createDefaultCollectionAsync fails with foreign key violation for non-existent user`), and confirmed `Wordfolio.Api/Wordfolio.Api/Infrastructure/Environment.fs` collection/default-collection call sites are parity-preserving with only data-access parameter type alias renames.
- Issues encountered: None
- Learnings: For split test migrations, test-name set comparison provides fast parity evidence (missing baseline cases must be empty), and additive cases can be documented as coverage expansion when behavior assertions remain unchanged.

### Review CollectionsHierarchy module parity
- Files changed: `docs/data-access-refactor-code-review/spec.md`
- What was done: Confirmed module scope (`Wordfolio.Api/Wordfolio.Api.DataAccess/CollectionsHierarchy.fs`) and baseline target (`origin/main`), then verified behavior parity from the module diff showing only mechanical type/record/query-type renames (`VocabularySummary`/`CollectionSummary`/`CollectionOverview` and `VocabularySummarySortBy`/`VocabularySummaryQuery`) with unchanged SQL, filters, sorting expressions, and return-shape semantics across `getCollectionsByUserIdAsync`, `searchUserCollectionsAsync`, `searchCollectionVocabulariesAsync`, and `getDefaultVocabularySummaryByUserIdAsync`. Verified moved-tests and coverage parity by comparing baseline `member _.``...``` names in `origin/main:Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/CollectionsHierarchyTests.fs` to current split files in `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/CollectionsHierarchy/*.fs`, confirming all 49 baseline test cases are preserved with exact names (no missing mappings) and that 8 additional vocabulary sorting cases were added; also confirmed `Wordfolio.Api/Wordfolio.Api/Infrastructure/Environment.fs` query/sort call-site parity is preserved with mechanical mapping renames only (domain sort enums still map one-to-one to data-access sort enums and query records).
- Issues encountered: None
- Learnings: For hierarchy modules with multiple query surfaces, parity proof is strongest when combining production diff evidence (SQL/filter/order stability), exact baseline-to-split test-name set comparison, and explicit Environment sort/query mapping checks to show call-site behavior remains unchanged despite type renames.

### Review Vocabularies module parity
- Files changed: `docs/data-access-refactor-code-review/spec.md`
- What was done: Confirmed module scope (`Wordfolio.Api/Wordfolio.Api.DataAccess/Vocabularies.fs`) against `origin/main` and verified behavior parity from the file diff: only mechanical renames (`VocabularyCreationParameters`/`VocabularyUpdateParameters` -> `CreateVocabularyParameters`/`UpdateVocabularyParameters`) plus relocation of table bindings into function scope, with unchanged query predicates (`IsDefault`/`IsSystem` guards), SQL update/delete constraints, insert flags, and return-shape mapping. Verified moved-tests and coverage parity by comparing baseline `member _.``...``` names in `origin/main:Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/VocabulariesTests.fs` to current split files in `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Vocabularies/*.fs`, confirming all 24 baseline cases are preserved with exact names and that 2 additional cases were added (`createDefaultVocabularyAsync fails with foreign key violation for non-existent collection`, `getDefaultVocabularyByUserIdAsync returns default vocabulary for requested user when other users have defaults`); confirmed `Wordfolio.Api/Wordfolio.Api/Infrastructure/Environment.fs` vocabulary/default-vocabulary call sites preserve behavior with only data-access parameter type alias renames.
- Issues encountered: None
- Learnings: For mixed Dapper SQL and Dapper.FSharp modules, parity evidence remains strong when semantic guards are unchanged and test-name set preservation is exact; additive split tests can be recorded as coverage expansion while still proving one-to-one baseline retention.

### Review Entries module parity
- Files changed: `docs/data-access-refactor-code-review/spec.md`
- What was done: Confirmed module scope (`Wordfolio.Api/Wordfolio.Api.DataAccess/Entries.fs`) and baseline target (`origin/main`), then verified behavior parity from the diff as mechanical-only changes: parameter type renames (`Entry*Parameters` -> `Create/Update/Move*Parameters`), table bindings moved from module scope into function scope, and replacement of cross-module vocabulary record/table references with local equivalent `VocabularyRecord` bindings while preserving predicates, update/delete constraints, joins, and return-shape behavior for retained APIs. Verified coverage and moved-test parity by comparing baseline `origin/main:Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/EntriesTests.fs` `member _.``...``` names against `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Entries/*.fs`, confirming 26 baseline tests are preserved with exact names, 4 removed read-surface tests are mechanically mapped to `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/EntriesHierarchy/GetEntryByIdWithHierarchyTests.fs` and `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/EntriesHierarchy/GetEntriesHierarchyByVocabularyIdTests.fs`, and 2 additive cases were introduced (`updateEntryAsync only updates targeted entry`, `moveEntryAsync fails with foreign key violation when target vocabulary does not exist`).
- Issues encountered: None
- Learnings: For modules that drop legacy read APIs, parity evidence should pair dead-call-path proof (no `Environment.fs` production consumption in baseline/current) with explicit scenario mapping to the successor hierarchy module tests so behavior and coverage claims remain auditable.

### Review Definitions module parity
- Files changed: `docs/data-access-refactor-code-review/spec.md`
- What was done: Confirmed module scope (`Wordfolio.Api/Wordfolio.Api.DataAccess/Definitions.fs`) against `origin/main` and verified behavior parity for the retained `createDefinitionsAsync` surface (same insert table, record mapping, empty-input behavior, and returned inserted-id shape) with only mechanical changes (`DefinitionCreationParameters` -> `CreateDefinitionParameters` and localizing `definitionsInsertTable`). Verified moved-tests and coverage parity by mapping baseline `origin/main:Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/DefinitionsTests.fs` create-surface tests to `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Definitions/CreateDefinitionsTests.fs` with exact preservation of 4 baseline test names plus 1 additive ordering case, and documented dead-API removal evidence: baseline-only `getDefinitionsByEntryIdAsync`/`updateDefinitionsAsync`/`deleteDefinitionsAsync` call sites are absent from production `Wordfolio.Api/Wordfolio.Api/Infrastructure/Environment.fs` (baseline and current), with `createDefinitionsAsync` remaining the only production Definitions API call.
- Issues encountered: None
- Learnings: When a module is narrowed to a single production-used API, parity evidence should separate retained-surface one-to-one test preservation from removed dead-surface tests, then prove removals are behavior-safe via production call-site reachability checks against `Environment.fs` in both baseline and current.

### Review Translations module parity
- Files changed: `docs/data-access-refactor-code-review/spec.md`
- What was done: Confirmed module scope (`Wordfolio.Api/Wordfolio.Api.DataAccess/Translations.fs`) and baseline target (`origin/main`), then verified behavior parity for the retained `createTranslationsAsync` surface: unchanged empty-input behavior, insert table/schema, record mapping (`EntryId`, `TranslationText`, `Source`, `DisplayOrder`), and inserted-id return ordering, with only mechanical changes (`TranslationCreationParameters` -> `CreateTranslationParameters` and localization of `translationsInsertTable`). Verified moved-tests/coverage parity by comparing baseline `origin/main:Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/TranslationsTests.fs` to `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Translations/CreateTranslationsTests.fs`: 5 baseline tests are preserved with exact names (`createTranslationsAsync inserts multiple translations`, `createTranslationsAsync returns empty list when given empty list`, `createTranslationsAsync fails with foreign key violation for non-existent entry`, `createTranslationsAsync fails with unique constraint violation for duplicate DisplayOrder`, `deleting entry cascades to delete translations`) and 1 additive create-surface test was added (`createTranslationsAsync returns ids in the same order as input parameters`).
- Issues encountered: None
- Learnings: For narrowed modules, treat retained API scenarios and removed API scenarios separately: prove retained behavior with direct diff + test-name preservation, then prove removed APIs (`getTranslationsByEntryIdAsync`, `updateTranslationsAsync`, `deleteTranslationsAsync`) are dead from production call paths via baseline/current reachability checks (`Environment.fs` keeps only `createTranslationsAsync` usage; removed symbols appear only in baseline data-access/tests and nowhere in current `.fs` files).

### Review Examples module parity
- Files changed: `docs/data-access-refactor-code-review/spec.md`
- What was done: Confirmed module scope (`Wordfolio.Api/Wordfolio.Api.DataAccess/Examples.fs`) against `origin/main` and verified behavior parity for the retained `createExamplesAsync` surface: same empty-input behavior, insert table/schema, record mapping (`DefinitionId`, `TranslationId`, `ExampleText`, `Source`), and inserted-id return shape, with only mechanical refactor changes (type rename `ExampleCreationParameters` -> `CreateExampleParameters`, localized insert-table binding, and removal of dead helper/read-write APIs). Verified moved-tests/coverage parity by comparing baseline `origin/main:Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/ExamplesTests.fs` to `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Examples/CreateExamplesTests.fs`: 7 create-surface tests are preserved with exact names, baseline cascade scenarios were mechanically remapped to current entry-anchored deletion coverage (`deleting entry cascades through definitions/translations to delete examples`) and `EntriesHierarchy/ClearEntryChildrenTests.fs` cascade checks, and baseline-only removed-surface tests (`getExamplesByDefinitionIdAsync`, `getExamplesByTranslationIdAsync`, `updateExamplesAsync`, `deleteExamplesAsync`) were validated as dead via reachability evidence (baseline/current `Environment.fs` call sites invoke only `Examples.createExamplesAsync`).
- Issues encountered: None
- Learnings: For modules narrowed to write-only production usage, parity evidence should combine one-to-one retained test preservation with explicit dead-surface reachability proof and scenario remapping to successor modules when lifecycle/cascade assertions move to the owning aggregate delete path.

### Review EntriesHierarchy module parity and finalize matrix
- Files changed: `docs/data-access-refactor-code-review/spec.md`
- What was done: Confirmed module scope (`Wordfolio.Api/Wordfolio.Api.DataAccess/EntriesHierarchy.fs`) against `origin/main` and verified behavior parity from the production diff: only mechanical renames (`EntryWithHierarchy`/`DefinitionWithExamples`/`TranslationWithExamples` -> `EntryHierarchy`/`DefinitionHierarchy`/`TranslationHierarchy`), localization of table bindings to function scope, and equivalent table references in `clearEntryChildrenAsync`, with unchanged query filters, ordering, hierarchy assembly, and return-shape semantics for `getEntryByIdWithHierarchyAsync`, `clearEntryChildrenAsync`, and `getEntriesHierarchyByVocabularyIdAsync`. Verified coverage and moved-tests parity by comparing baseline `origin/main:Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/EntriesHierarchyTests.fs` to `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/EntriesHierarchy/*.fs`, confirming all 20 baseline `member _.``...``` test names are preserved exactly (no missing mappings) with 2 additive cases, and confirmed `Wordfolio.Api/Wordfolio.Api/Infrastructure/Environment.fs` hierarchy call sites (`IGetEntryById`, `IClearEntryChildren`, `IGetEntriesHierarchyByVocabularyId`) preserve behavior with mechanical variable/type alias renames only.
- Issues encountered: None
- Learnings: For modules split into multiple test files, exact baseline-name preservation plus additive-case accounting provides auditable moved-test evidence; pairing that with call-site checks in `Environment.fs` closes parity proof for domain-facing hierarchy flows.

#### Final Parity Matrix
| Production module | Behavior parity | Coverage parity | Moved tests preserved |
| --- | --- | --- | --- |
| `origin/main:Wordfolio.Api/Wordfolio.Api.DataAccess/Database.fs` (removed) | Pass | Pass | Pass |
| `Wordfolio.Api/Wordfolio.Api.DataAccess/Users.fs` | Pass | Pass | Pass |
| `Wordfolio.Api/Wordfolio.Api.DataAccess/Collections.fs` | Pass | Pass | Pass |
| `Wordfolio.Api/Wordfolio.Api.DataAccess/CollectionsHierarchy.fs` | Pass | Pass | Pass |
| `Wordfolio.Api/Wordfolio.Api.DataAccess/Vocabularies.fs` | Pass | Pass | Pass |
| `Wordfolio.Api/Wordfolio.Api.DataAccess/Entries.fs` | Pass | Pass | Pass |
| `Wordfolio.Api/Wordfolio.Api.DataAccess/Definitions.fs` | Pass | Pass | Pass |
| `Wordfolio.Api/Wordfolio.Api.DataAccess/Translations.fs` | Pass | Pass | Pass |
| `Wordfolio.Api/Wordfolio.Api.DataAccess/Examples.fs` | Pass | Pass | Pass |
| `Wordfolio.Api/Wordfolio.Api.DataAccess/EntriesHierarchy.fs` | Pass | Pass | Pass |
