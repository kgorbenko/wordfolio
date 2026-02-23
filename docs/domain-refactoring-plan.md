# Domain Refactoring Plan

## Goal

Refactor `Wordfolio.Api.Domain` to align with the agreed rules:

- Shared code uses only `Wordfolio.Api.Domain` namespace.
- Shared root files are `Types.fs`, `Capabilities.fs`, `Operations.fs`.
- No feature-to-feature imports.
- Capability methods use `...Parameters` records for non-trivial calls.
- Public operations have explicit input and output types.
- Errors are operation-specific.
- Naming reflects data shape and intent.

## Progress Status

Last updated: 2026-02-23

- Phase 0 - Completed
- Phase 1 - Completed
- Phase 2 - Completed
- Phase 3 - Completed
- Phase 4 - Not started
- Phase 5 - Not started

Completed implementation notes:

- Root shared files are now in place: `Types.fs`, `Capabilities.fs`, `Operations.fs`.
- `Wordfolio.Api.Domain.Shared` was fully removed from active code.
- Cross-feature contract `IGetCollectionById` was extracted to root shared `Capabilities.fs`.
- `Ids.fs` and `Transactions.fs` were merged into root shared files:
  - ID value types moved into `Types.fs`.
  - `ITransactional` and `runInTransaction` moved into `Capabilities.fs`.
- Feature type-alias files were removed (`Collections/Collection.fs`, `Vocabularies/Vocabulary.fs`) to enforce no re-export aliases.
- `Vocabularies/Types.fs` now hosts feature-specific `VocabularyDetail`.
- Rulebook was updated to prohibit aliasing root types under feature namespaces.

Process note:

- After each completed phase, update this plan document with status and key deltas.

## Scope

Projects/files in scope:

- `Wordfolio.Api/Wordfolio.Api.Domain/Wordfolio.Api.Domain.fsproj`
- Root domain files (`Ids.fs`, `Transactions.fs`, and new root shared files)
- Feature folders:
  - `Collections/`
  - `CollectionsHierarchy/`
  - `Vocabularies/`
  - `Entries/`
- Downstream projects that consume domain contracts (handlers, infrastructure, data access mappings, tests) will need incremental updates after each phase.

## Constraints

- Keep behavior stable unless intentionally changed.
- Keep refactor compile-safe in small steps.
- Avoid large one-shot rewrites.
- Update tests in lockstep with contract changes.

## Phase Plan

### Phase 0 - Baseline and Safety Net

1. Create a dedicated refactor branch.
2. Run full baseline checks:
   - `dotnet build`
   - `dotnet test`
3. Record current public domain API shape (modules/functions/interfaces used by other projects).

Exit criteria:

- Baseline passes and is captured.

Status:

- Completed on 2026-02-23.

---

### Phase 1 - Introduce Root Shared Files (No behavior change)

1. Add new root files:
   - `Wordfolio.Api/Wordfolio.Api.Domain/Types.fs`
   - `Wordfolio.Api/Wordfolio.Api.Domain/Capabilities.fs`
   - `Wordfolio.Api/Wordfolio.Api.Domain/Operations.fs`
2. Move shared contracts/types from `Shared/*` into root namespace `Wordfolio.Api.Domain`.
3. Update all references immediately in domain and downstream projects that consume moved code.
4. Update `Wordfolio.Api.Domain.fsproj` compile order:
   - Root shared files compile before feature folders.

Exit criteria:

- Domain builds.
- No feature behavior changes.
- Shared namespace migration is complete in active code paths.

Status:

- Completed on 2026-02-23.

---

### Phase 2 - Remove `Wordfolio.Api.Domain.Shared` Namespace

1. Update all domain files to import from `Wordfolio.Api.Domain` root shared modules/types.
2. Remove usage of `Wordfolio.Api.Domain.Shared` in:
   - `Entries/Entry.fs`
   - `Entries/DraftOperations.fs`
   - `Collections/Collection.fs`
   - `Vocabularies/Vocabulary.fs`
   - any other remaining references.
3. Delete `Shared/Types.fs`, `Shared/Capabilities.fs`, `Shared/Operations.fs` once all references are gone.
4. Clean up `fsproj` entries accordingly.

Exit criteria:

- No `Wordfolio.Api.Domain.Shared` namespace remains.
- Build and tests pass.

Status:

- Completed on 2026-02-23.

---

### Phase 3 - Enforce Feature Boundary Rule (No feature imports)

1. Identify current cross-feature dependencies:
   - `Vocabularies` depending on `Collections` capability contracts.
   - Any other `FeatureA -> FeatureB` imports.
2. Extract shared contracts required across features into root `Capabilities.fs`/`Types.fs`.
3. Replace feature imports with root-domain imports.
4. Keep each feature dependent only on root + itself.

Exit criteria:

- No direct feature-to-feature imports in `Wordfolio.Api.Domain`.

Status:

- Completed on 2026-02-23.

---

### Phase 4 - Feature-by-Feature Refactoring Loop

After Phase 3, refactor one feature at a time and complete all targeted changes for that feature before moving to the next.

Recommended order:

1. `Collections`
2. `Vocabularies`
3. `Entries`
4. `CollectionsHierarchy`

For each feature, execute this full loop:

1. Normalize file layout to `Types.fs`, `Errors.fs`, `Capabilities.fs`, `Operations.fs`.
   - Replace ambiguous files such as `Collection.fs`, `Vocabulary.fs`, `Entry.fs`, and `CollectionsHierarchy.fs` with `Types.fs`.
2. Refactor capability signatures:
   - Introduce `...Parameters` records for non-trivial methods.
   - Remove long unnamed tuple signatures.
3. Add explicit parameter and return types to every public operation.
4. Refactor errors to be operation-specific.
5. Rename misleading types so names match payload shape and intent.
6. Normalize operation behavior:
   - Remove silent error-to-default fallbacks unless explicitly required.
   - Apply consistent access-denied vs not-found policy.
7. Update dependent projects and tests immediately for the changed feature.
8. Run formatting and verification before moving to the next feature.

Exit criteria (per feature):

- Feature conforms to agreed rules and compiles cleanly with updated consumers.
- Tests for impacted areas pass.

---

### Phase 5 - Final Hardening

1. Run format and checks:
   - `dotnet fantomas .`
   - `dotnet build`
   - `dotnet test`
2. Update docs if file/module names changed.

Exit criteria:

- Clean domain codebase matching agreed architecture.
- All checks pass.

## Suggested Delivery Strategy

- Deliver as a sequence of small PRs aligned with phases:
  1. Shared namespace consolidation + removal of `Domain.Shared`
  2. Feature boundary cleanup (no feature imports)
  3. Collections full refactor loop
  4. Vocabularies full refactor loop
  5. Entries full refactor loop
  6. CollectionsHierarchy full refactor loop
  7. Final hardening
- Keep each PR independently buildable and testable.

## Validation Checklist

- No `Wordfolio.Api.Domain.Shared` namespace remains.
- Shared root contains `Types.fs`, `Capabilities.fs`, `Operations.fs`.
- No `FeatureA -> FeatureB` imports.
- Non-trivial capability methods use `...Parameters` records.
- Public operations have explicit signatures.
- Operation return errors are operation-specific.
- Names reflect actual type semantics.
- Build, tests, and formatting pass.
