# Domain Rules Compliance Sweep

## Overview

This loop enforces the full inlined ruleset below across the F# domain layer and domain tests: `Wordfolio.Api/Wordfolio.Api.Domain` and `Wordfolio.Api/Wordfolio.Api.Domain.Tests`. The goal is complete rule compliance with no behavior changes, while preserving build/test stability.

The loop is feature-driven. Each implementation iteration must complete one full feature slice (domain + related tests) and fix all applicable rules in that feature before exiting.

## Specification

### Scope

- In-scope production code:
  - `Wordfolio.Api/Wordfolio.Api.Domain/Types.fs`
  - `Wordfolio.Api/Wordfolio.Api.Domain/Capabilities.fs`
  - `Wordfolio.Api/Wordfolio.Api.Domain/Operations.fs`
  - `Wordfolio.Api/Wordfolio.Api.Domain/Collections/*.fs`
  - `Wordfolio.Api/Wordfolio.Api.Domain/Vocabularies/*.fs`
  - `Wordfolio.Api/Wordfolio.Api.Domain/CollectionsHierarchy/*.fs`
  - `Wordfolio.Api/Wordfolio.Api.Domain/Entries/*.fs`
- In-scope tests:
  - `Wordfolio.Api/Wordfolio.Api.Domain.Tests/Collections/*.fs`
  - `Wordfolio.Api/Wordfolio.Api.Domain.Tests/Vocabularies/*.fs`
  - `Wordfolio.Api/Wordfolio.Api.Domain.Tests/CollectionsHierarchy/*.fs`
  - `Wordfolio.Api/Wordfolio.Api.Domain.Tests/Shared/*.fs`
  - `Wordfolio.Api/Wordfolio.Api.Domain.Tests/Entries/DraftOperations/*.fs`
  - `Wordfolio.Api/Wordfolio.Api.Domain.Tests/Entries/EntryOperations/*.fs`

### Pattern to Follow

- Follow repository conventions in `AGENTS.md`.
- Maintain no-behavior-change refactor intent.
- Keep compile order valid in:
  - `Wordfolio.Api/Wordfolio.Api.Domain/Wordfolio.Api.Domain.fsproj`
  - `Wordfolio.Api/Wordfolio.Api.Domain.Tests/Wordfolio.Api.Domain.Tests.fsproj`

### Inlined Ruleset (Authoritative)

#### Architecture Rules

- Use `Wordfolio.Api.Domain` as the only shared namespace for cross-feature domain code.
- Keep shared-level files at domain root as `Types.fs`, `Capabilities.fs`, and `Operations.fs`.
- Keep feature-level `Types.fs`, `Capabilities.fs`, `Operations.fs`, and `Errors.fs`, but do not use feature files as catch-all dumps.
- Features must not import other features. Cross-feature reuse must be extracted to the root-domain shared level.

#### Dependency Rules

- Allowed: `Feature -> Wordfolio.Api.Domain` (root shared contracts/types).
- Not allowed: `FeatureA -> FeatureB`.
- If a feature needs another feature concept, move that concept (type/capability/helper operation) to root-level shared files.

#### Capabilities Rules

- Capability methods must not use long unnamed tuple parameter lists.
- For any non-trivial method, introduce `...Parameters` record types.
- If a capability input record type name would otherwise match an operation parameter type name, keep the operation type as `...Parameters` and rename the capability type to `...Data` (for example, `CreateCollectionParameters` in operations and `CreateCollectionData` in capabilities). Never add `Capability` into the type name.
- Instead of tuples, use multiple named arguments.
- Capability-specific input record types must be declared in `Capabilities.fs` (place them at the top of the file before capability interfaces).

#### Operations Rules

- Every public operation must declare explicit parameter and return types.
- Operation signatures should be stable and readable; avoid inferred broad types.
- Do not add explicit type annotations for the `env` parameter in operation signatures; rely on F# inference for environment capability composition.
- Define operation parameter record types in the feature `Operations.fs` module, not in `Capabilities.fs`.
- All public operation functions must return `Task<Result<'a, 'err>>`. For operations that cannot fail, use `unit` as the error type and annotate the return type explicitly. Handlers should use `Result.defaultValue` rather than a dead match arm for unit-error operations.
- Transaction orchestration stays in the operations layer (consistent boundary ownership).
- Do not silently swallow failures into default values unless that is an explicit business rule.

#### Call-Site Qualification Rules

- Do not use fully-qualified namespace call sites in domain code or tests (for example, `Wordfolio.Api.Domain.Entries.DraftOperations.getDrafts ...`).
- Prefer module-qualified calls such as `DraftOperations.getDrafts ...`.
- If the function is already in scope, use the direct call form such as `getDrafts ...`.
- Resolve name collisions with local aliases or selective `open` statements, not long namespace chains.

#### Error Modeling Rules

- Errors must be operation-specific, not generic feature-wide unions.
- Use error DUs like `CreateEntryError`, `UpdateEntryError`, `DeleteEntryError` to avoid impossible cases in return types.
- Decorate all error DUs with `[<RequireQualifiedAccess>]` to enforce qualified access at every call site and prevent DU case shadowing.
- Keep error cases minimal and specific to actual outcomes of the operation.
- Normalize access/not-found semantics consistently per operation policy.

#### Type Design Rules

- Avoid misleading names. Type names must reflect payload depth and intent.
- Use clear suffixes when needed (`Overview`, `Details`, `WithEntryCount`, and so on).
- Prefer domain-specific value types where useful instead of primitive-heavy APIs.
- Do not alias or re-export root shared types inside feature namespaces (for example, no `type Collection = Wordfolio.Api.Domain.Collection` in `Wordfolio.Api.Domain.Collections`).

#### File and Module Organization Rules

- Shared root: `Types.fs`, `Capabilities.fs`, `Operations.fs`.
- Per feature: keep `Capabilities.fs`, `Operations.fs`, and `Errors.fs`; keep `Types.fs` only when the feature actually defines types in it.
- If a feature `Types.fs` is empty (or only a namespace shell), delete the file and remove it from `.fsproj`.
- Split large operation files by use case only when needed, but keep naming predictable.
- Keep `.fsproj` compile order aligned with dependency direction (shared root first, then features).
- File/directory layout must mirror namespace/module structure (tests included), with only `.Tests` inserted after `Wordfolio.Api.Domain` for test namespaces.

#### Refactoring Process Rules

- Refactor incrementally in compile-safe steps.
- First move and rename namespace structure, then update signatures (`...Parameters`), then split errors, then do naming cleanup.
- Update tests alongside each operation and error-contract change.
- Keep external behavior unchanged unless explicitly intended.

#### Test Rules

- Do not add compatibility wrappers in tests to reshape operation signatures; call operations directly with their real parameter records.
- Test dependency function signatures must match capability interface signatures exactly (parameter shape and return type).
- `...Calls` collections in tests must store exactly the parameter type of the method being tracked.
- For capability methods that use parameter records, store those same record types in `...Calls` collections.
- For capability methods that use tuple parameters, store those same tuple types in `...Calls` collections.
- Do not create duplicate test-only `...Call` records when capability parameter records already exist.
- In every test, assert the state of all `...Calls` collections (explicitly verify expected calls and explicit empties for non-used dependencies).
- Test module namespaces must mirror production namespaces with only `.Tests` inserted after `Wordfolio.Api.Domain` (for shared root operations, use `Wordfolio.Api.Domain.Tests.*`, not `Wordfolio.Api.Domain.Tests.Shared.*`).

## Implementation Steps

### Mandatory loop verification commands (run on every invocation)

- `dotnet fantomas .`
- `dotnet build "Wordfolio.Api/Wordfolio.Api/Wordfolio.Api.fsproj"`
- `dotnet test "Wordfolio.Api/Wordfolio.Api.Domain.Tests/Wordfolio.Api.Domain.Tests.fsproj"`
- `dotnet test "Wordfolio.Api/Wordfolio.Api.Domain.Tests/Wordfolio.Api.Domain.Tests.fsproj" --filter "FullyQualifiedName~Collections"`
- `dotnet test "Wordfolio.Api/Wordfolio.Api.Domain.Tests/Wordfolio.Api.Domain.Tests.fsproj" --filter "FullyQualifiedName~Vocabularies"`
- `dotnet test "Wordfolio.Api/Wordfolio.Api.Domain.Tests/Wordfolio.Api.Domain.Tests.fsproj" --filter "FullyQualifiedName~CollectionsHierarchy"`
- `dotnet test "Wordfolio.Api/Wordfolio.Api.Domain.Tests/Wordfolio.Api.Domain.Tests.fsproj" --filter "FullyQualifiedName~Entries"`
- `dotnet test`

### 1. Shared root compliance (`Types.fs`, `Capabilities.fs`, `Operations.fs`)
- [x] Implement: Audit and fix all applicable rules for `Wordfolio.Api/Wordfolio.Api.Domain/Types.fs`, `Wordfolio.Api/Wordfolio.Api.Domain/Capabilities.fs`, `Wordfolio.Api/Wordfolio.Api.Domain/Operations.fs`, and any directly impacted tests in `Wordfolio.Api/Wordfolio.Api.Domain.Tests/Shared/*.fs`.
- [x] Improve: Review for hidden feature-coupling and compile-order assumptions introduced by shared-type/capability movement.

### 2. Collections feature compliance (domain + tests)
- [x] Implement: In one iteration, fix all applicable rules for `Wordfolio.Api/Wordfolio.Api.Domain/Collections/*.fs` and `Wordfolio.Api/Wordfolio.Api.Domain.Tests/Collections/*.fs`.
- [x] Improve: Review test call-tracking completeness (`...Calls` type parity and full assertions per test).

### 3. Vocabularies feature compliance (domain + tests)
- [x] Implement: In one iteration, fix all applicable rules for `Wordfolio.Api/Wordfolio.Api.Domain/Vocabularies/*.fs` and `Wordfolio.Api/Wordfolio.Api.Domain.Tests/Vocabularies/*.fs`.
- [x] Improve: Review operation-specific error contracts and call-site qualification consistency.

### 4. CollectionsHierarchy feature compliance (domain + tests)
- [x] Implement: In one iteration, fix all applicable rules for `Wordfolio.Api/Wordfolio.Api.Domain/CollectionsHierarchy/*.fs` and `Wordfolio.Api/Wordfolio.Api.Domain.Tests/CollectionsHierarchy/*.fs`.
- [x] Improve: Review naming precision and cross-feature dependency compliance.

### 5. Entries feature compliance (domain + tests)
- [x] Implement: In one iteration, fix all applicable rules for `Wordfolio.Api/Wordfolio.Api.Domain/Entries/*.fs` and `Wordfolio.Api/Wordfolio.Api.Domain.Tests/Entries/**/*.fs`.
- [ ] Improve: Review capability signature parity, no wrapper regressions, and all `...Calls` assertions in every test.

### 6. Final full-scope compliance and closeout
- [ ] Implement: Re-audit all in-scope files and resolve any remaining rule drift across domain and tests.
- [ ] Improve: Review final diff for no-behavior-change compliance, consistency, and removal of fully-qualified domain call-sites.

## Progress Log

### Implement: Audit and fix all applicable rules for `Wordfolio.Api/Wordfolio.Api.Domain/Types.fs`, `Wordfolio.Api/Wordfolio.Api.Domain/Capabilities.fs`, `Wordfolio.Api/Wordfolio.Api.Domain/Operations.fs`, and any directly impacted tests in `Wordfolio.Api/Wordfolio.Api.Domain.Tests/Shared/*.fs`.
- Files changed: `docs/domain-rules-compliance-sweep/spec.md`
- What was done: Audited shared root domain files and related root-domain test coverage for rule compliance. No production or test code changes were required because existing signatures, capability parameter records, call-site qualification, and namespaces already satisfy the inlined ruleset.
- Issues encountered: None
- Learnings: Shared root operation `getOrCreateDefaultVocabulary` is intentionally internal and does not trigger the public-operation `Task<Result<_,_>>` contract rule; root-domain tests for it currently live in `Wordfolio.Api.Domain.Tests` rather than `Shared/`, so future spec steps should treat that file as the impacted coverage location.

### Improve: Review for hidden feature-coupling and compile-order assumptions introduced by shared-type/capability movement.
- Files changed: `docs/domain-rules-compliance-sweep/spec.md`
- What was done: Re-reviewed root shared domain contracts and operation wiring for cross-feature coupling and compile-order fragility. Confirmed the shared root files remain feature-agnostic and both domain/test `.fsproj` compile order already preserves root-before-feature dependency direction.
- Issues encountered: None
- Learnings: Shared root capability contracts (`IGetDefaultCollection`, `ICreateDefaultCollection`, `IGetDefaultVocabulary`, `ICreateDefaultVocabulary`) safely centralize default-vocabulary orchestration without requiring feature-to-feature imports, so future refactors should preserve this boundary by extending root contracts instead of adding cross-feature references.

### Implement: In one iteration, fix all applicable rules for `Wordfolio.Api/Wordfolio.Api.Domain/Collections/*.fs` and `Wordfolio.Api/Wordfolio.Api.Domain.Tests/Collections/*.fs`.
- Files changed: `Wordfolio.Api/Wordfolio.Api.Domain.Tests/Collections/CreateTests.fs`, `Wordfolio.Api/Wordfolio.Api.Domain.Tests/Collections/DeleteTests.fs`, `Wordfolio.Api/Wordfolio.Api.Domain.Tests/Collections/UpdateTests.fs`, `docs/domain-rules-compliance-sweep/spec.md`
- What was done: Updated collections tests to track capability calls using the exact capability parameter record types (`CreateCollectionData`, `UpdateCollectionData`) and removed duplicate test-only call records. Added explicit assertions in each test for all `...Calls` collections, including expected empties for dependencies that should not be invoked.
- Issues encountered: Initial compile errors from ambiguous record inference between operation and capability parameter records were resolved by explicitly typing expected call records in assertions.
- Learnings: Collections operations intentionally use separate operation parameter records and capability data records with overlapping fields, so tests should annotate expected record literals when asserting call lists to keep type parity explicit and unambiguous.

### Improve: Review test call-tracking completeness (`...Calls` type parity and full assertions per test).
- Files changed: `docs/domain-rules-compliance-sweep/spec.md`
- What was done: Audited all Collections domain tests (`Create`, `Update`, `Delete`, `GetById`, `GetByUserId`) for call-tracking parity and verified each test asserts every `...Calls` collection relevant to its test environment, including explicit empty assertions for non-invoked dependencies.
- Issues encountered: None
- Learnings: Collections tests consistently use exact capability parameter/call types (for example, `CreateCollectionData` and `UpdateCollectionData`) and complete call-list assertions across success and failure paths, so no additional production or test code changes were required in this improve pass.

### Implement: In one iteration, fix all applicable rules for `Wordfolio.Api/Wordfolio.Api.Domain/Vocabularies/*.fs` and `Wordfolio.Api/Wordfolio.Api.Domain.Tests/Vocabularies/*.fs`.
- Files changed: `Wordfolio.Api/Wordfolio.Api.Domain.Tests/Vocabularies/CreateTests.fs`, `Wordfolio.Api/Wordfolio.Api.Domain.Tests/Vocabularies/UpdateTests.fs`, `Wordfolio.Api/Wordfolio.Api.Domain.Tests/Vocabularies/DeleteTests.fs`, `Wordfolio.Api/Wordfolio.Api.Domain.Tests/Vocabularies/GetByIdTests.fs`, `Wordfolio.Api/Wordfolio.Api.Domain.Tests/Vocabularies/GetByCollectionIdTests.fs`, `docs/domain-rules-compliance-sweep/spec.md`
- What was done: Updated vocabularies tests to track capability calls with the exact capability input types (`CreateVocabularyData`, `UpdateVocabularyData`) instead of duplicate test-only call records. Added explicit assertions in every vocabularies test for all `...Calls` collections, including explicit empty assertions for non-invoked dependencies.
- Issues encountered: None
- Learnings: Vocabularies operations already matched the domain ruleset; the remaining compliance drift was in test call-tracking parity and assertion completeness, which are resolved by reusing capability parameter records directly in test environments.

### Improve: Review operation-specific error contracts and call-site qualification consistency.
- Files changed: `docs/domain-rules-compliance-sweep/spec.md`
- What was done: Audited vocabulary operation error unions and corresponding operation implementations/tests to confirm each public operation returns its own `[<RequireQualifiedAccess>]` DU with only relevant cases. Verified vocabulary domain/tests use module-qualified or in-scope operation calls and contain no fully-qualified `Wordfolio.Api.Domain...` call sites.
- Issues encountered: None
- Learnings: Vocabularies already follow operation-specific error modeling with explicit per-operation DUs (`GetVocabularyByIdError`, `GetVocabulariesByCollectionIdError`, `CreateVocabularyError`, `UpdateVocabularyError`, `DeleteVocabularyError`), so this improve pass required no production/test code edits beyond recording the compliance review.

### Implement: In one iteration, fix all applicable rules for `Wordfolio.Api/Wordfolio.Api.Domain/CollectionsHierarchy/*.fs` and `Wordfolio.Api/Wordfolio.Api.Domain.Tests/CollectionsHierarchy/*.fs`.
- Files changed: `Wordfolio.Api/Wordfolio.Api.Domain.Tests/CollectionsHierarchy/GetByUserIdTests.fs`, `docs/domain-rules-compliance-sweep/spec.md`
- What was done: Audited CollectionsHierarchy domain and test files for the inlined ruleset and kept production code unchanged because capability signatures, operation contracts, and module qualification already complied. Updated `GetByUserId` tests to assert all tracked dependency call collections in every test, including explicit non-empty call assertions for both dependencies.
- Issues encountered: None
- Learnings: For CollectionsHierarchy unit-error operations, compliance drift was in test assertion completeness rather than operation signatures; tests with multiple dependencies must always assert every `...Calls` collection to satisfy the loop's call-tracking rule.

### Improve: Review naming precision and cross-feature dependency compliance.
- Files changed: `docs/domain-rules-compliance-sweep/spec.md`
- What was done: Reviewed `CollectionsHierarchy` domain and test modules for naming precision and verified the public types accurately communicate payload depth (`CollectionWithVocabularies`, `CollectionWithVocabularyCount`, `VocabularyWithEntryCount`, `CollectionsHierarchyResult`) without ambiguous aliases. Re-audited dependencies to confirm the feature references only root shared contracts (`Wordfolio.Api.Domain`) and does not import other feature namespaces.
- Issues encountered: None
- Learnings: CollectionsHierarchy already preserves dependency boundaries by expressing cross-feature concepts through root shared value types (`CollectionId`, `VocabularyId`, `UserId`) rather than feature-to-feature imports; future naming refinements should keep this boundary intact.

### Implement: In one iteration, fix all applicable rules for `Wordfolio.Api/Wordfolio.Api.Domain/Entries/*.fs` and `Wordfolio.Api/Wordfolio.Api.Domain.Tests/Entries/**/*.fs`.
- Files changed: `Wordfolio.Api/Wordfolio.Api.Domain.Tests/Entries/EntryOperations/GetByVocabularyIdTests.fs`, `Wordfolio.Api/Wordfolio.Api.Domain.Tests/Entries/EntryOperations/GetByIdTests.fs`, `Wordfolio.Api/Wordfolio.Api.Domain.Tests/Entries/EntryOperations/DeleteTests.fs`, `Wordfolio.Api/Wordfolio.Api.Domain.Tests/Entries/EntryOperations/MoveTests.fs`, `Wordfolio.Api/Wordfolio.Api.Domain.Tests/Entries/EntryOperations/UpdateTests.fs`, `Wordfolio.Api/Wordfolio.Api.Domain.Tests/Entries/DraftOperations/GetDraftsTests.fs`, `Wordfolio.Api/Wordfolio.Api.Domain.Tests/Entries/DraftOperations/GetByVocabularyIdTests.fs`, `Wordfolio.Api/Wordfolio.Api.Domain.Tests/Entries/DraftOperations/GetByIdTests.fs`, `Wordfolio.Api/Wordfolio.Api.Domain.Tests/Entries/DraftOperations/DeleteTests.fs`, `Wordfolio.Api/Wordfolio.Api.Domain.Tests/Entries/DraftOperations/MoveTests.fs`, `Wordfolio.Api/Wordfolio.Api.Domain.Tests/Entries/DraftOperations/UpdateTests.fs`, `Wordfolio.Api/Wordfolio.Api.Domain.Tests/Entries/DraftOperations/CreateTests.fs`, `docs/domain-rules-compliance-sweep/spec.md`
- What was done: Removed fully-qualified `Wordfolio.Api.Domain.Entries.*` operation call sites across entries domain tests and switched them to direct in-scope operation calls (`create`, `update`, `move`, `delete`, `getById`, `getByVocabularyId`, `getDrafts`) to satisfy call-site qualification rules. Kept entries domain behavior unchanged and verified the full mandatory formatting/build/test suite passes.
- Issues encountered: None
- Learnings: Entries tests already import the operation modules, so replacing fully-qualified invocations with direct calls is compile-safe and reduces namespace noise without affecting operation contracts or behavior.
