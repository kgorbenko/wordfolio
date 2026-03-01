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
- [ ] Review `Wordfolio.Api/Wordfolio.Api.DataAccess/Users.fs` against `origin/main`, verify baseline tests from `origin/main:Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/UsersTests.fs` are preserved in `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Users/CreateUserTests.fs`, and document behavior and coverage parity evidence.

### 3. Review Collections module parity
- [ ] Review `Wordfolio.Api/Wordfolio.Api.DataAccess/Collections.fs` against `origin/main`, verify baseline tests from `origin/main:Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/CollectionsTests.fs` are preserved across `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Collections/*.fs`, and include `Wordfolio.Api/Wordfolio.Api/Infrastructure/Environment.fs` call-site parity for collection and default-collection flows.

### 4. Review CollectionsHierarchy module parity
- [ ] Review `Wordfolio.Api/Wordfolio.Api.DataAccess/CollectionsHierarchy.fs` against `origin/main`, verify baseline tests from `origin/main:Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/CollectionsHierarchyTests.fs` are preserved across `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/CollectionsHierarchy/*.fs`, and include `Wordfolio.Api/Wordfolio.Api/Infrastructure/Environment.fs` query/sort mapping parity checks.

### 5. Review Vocabularies module parity
- [ ] Review `Wordfolio.Api/Wordfolio.Api.DataAccess/Vocabularies.fs` against `origin/main`, verify baseline tests from `origin/main:Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/VocabulariesTests.fs` are preserved across `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Vocabularies/*.fs`, and include `Wordfolio.Api/Wordfolio.Api/Infrastructure/Environment.fs` default-vocabulary call-site parity checks.

### 6. Review Entries module parity
- [ ] Review `Wordfolio.Api/Wordfolio.Api.DataAccess/Entries.fs` against `origin/main`, verify baseline tests from `origin/main:Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/EntriesTests.fs` are preserved across `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Entries/*.fs`, include `Wordfolio.Api/Wordfolio.Api/Infrastructure/Environment.fs` call-site parity checks, and document dead-API removals as production-unreachable if applicable.

### 7. Review Definitions module parity
- [ ] Review `Wordfolio.Api/Wordfolio.Api.DataAccess/Definitions.fs` against `origin/main`, verify baseline tests from `origin/main:Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/DefinitionsTests.fs` are preserved in `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Definitions/CreateDefinitionsTests.fs`, include `Wordfolio.Api/Wordfolio.Api/Infrastructure/Environment.fs` call-site parity checks, and document removed dead APIs with reachability evidence.

### 8. Review Translations module parity
- [ ] Review `Wordfolio.Api/Wordfolio.Api.DataAccess/Translations.fs` against `origin/main`, verify baseline tests from `origin/main:Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/TranslationsTests.fs` are preserved in `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Translations/CreateTranslationsTests.fs`, include `Wordfolio.Api/Wordfolio.Api/Infrastructure/Environment.fs` call-site parity checks, and document removed dead APIs with reachability evidence.

### 9. Review Examples module parity
- [ ] Review `Wordfolio.Api/Wordfolio.Api.DataAccess/Examples.fs` against `origin/main`, verify baseline tests from `origin/main:Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/ExamplesTests.fs` are preserved in `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/Examples/CreateExamplesTests.fs`, include `Wordfolio.Api/Wordfolio.Api/Infrastructure/Environment.fs` call-site parity checks, and document removed dead APIs with reachability evidence.

### 10. Review EntriesHierarchy module parity and finalize matrix
- [ ] Review `Wordfolio.Api/Wordfolio.Api.DataAccess/EntriesHierarchy.fs` against `origin/main`, verify baseline tests from `origin/main:Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/EntriesHierarchyTests.fs` are preserved across `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/EntriesHierarchy/*.fs`, include `Wordfolio.Api/Wordfolio.Api/Infrastructure/Environment.fs` hierarchy call-site parity checks, and add a final matrix in Progress Log with one row per production module and explicit statuses for behavior parity, coverage parity, and moved-tests preserved.

## Progress Log

Agents append entries here after completing each step.

### Review Database module removal parity
- Files changed: `docs/data-access-refactor-code-review/spec.md`
- What was done: Compared `origin/main:Wordfolio.Api/Wordfolio.Api.DataAccess/Database.fs` and `origin/main:Wordfolio.Api/Wordfolio.Api.DataAccess/Wordfolio.Api.DataAccess.fsproj` to current state and confirmed the `Database.fs` compile include was removed while all remaining module includes persist. Verified parity evidence via baseline/current call-site searches (`Wordfolio.Api/Wordfolio.Api`, `Wordfolio.Api/Wordfolio.Api.DataAccess`, `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests`) showing no production references beyond the removed file include and no baseline test dependency on private helpers (`withConnectionAsync`, `inTransactionAsync`).
- Issues encountered: None
- Learnings: The removed `Database` module exposed only private helpers in baseline, so removal is behavior-neutral when compile inclusion and call-site reachability are both absent; for removed modules, test-parity proof is a negative mapping (no baseline `member _.``...``` ownership for that module) backed by targeted symbol searches.
