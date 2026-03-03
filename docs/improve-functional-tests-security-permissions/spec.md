# Improve Functional Tests Security Permissions

## Overview

This roadmap improves security-focused API integration tests in `Wordfolio.Api/Wordfolio.Api.Tests`, with emphasis on two guarantees: protected endpoints reject unauthenticated calls, and user-scoped endpoints do not expose or mutate another user's data.

Each loop iteration handles one test file only. The agent must plan and implement the exact test changes for that file during the iteration.

## Specification

### Goals & Success Criteria

- Each step file is reviewed and updated as needed to satisfy `Wordfolio.Api/Wordfolio.Api.Tests/AGENTS.md`.
- Security coverage is explicit in each step file:
  - unauthenticated access behavior is tested for protected endpoints;
  - user-ownership boundaries are tested for user-scoped read/list/write endpoints.
- For write-denial scenarios, persistence is asserted with seeder queries to prove unauthorized mutations did not occur.
- Verification commands pass before any commit is created.

### Scope

- In scope: endpoint test files under `Wordfolio.Api/Wordfolio.Api.Tests/Collections/`, `Wordfolio.Api/Wordfolio.Api.Tests/CollectionsHierarchy/`, `Wordfolio.Api/Wordfolio.Api.Tests/Vocabularies/`, `Wordfolio.Api/Wordfolio.Api.Tests/Entries/`, `Wordfolio.Api/Wordfolio.Api.Tests/Drafts/`, and `Wordfolio.Api/Wordfolio.Api.Tests/Dictionary/`.
- Out of scope: `Wordfolio.Api/Wordfolio.Api.Tests/Auth/*`, `Wordfolio.Api/Wordfolio.Api.Tests/StatusTests.fs`, `Wordfolio.Api/Wordfolio.Api.Tests/OpenApiTests.fs`, and all non-test projects.

## Execution Protocol

**Implement:**
1. Modify only the test file named by the current step scope. Do not modify production code, other test files, or dependencies.
2. Ensure the file is fully compliant with `Wordfolio.Api/Wordfolio.Api.Tests/AGENTS.md` (fixture usage, reset placement, seeding pattern, assertion pattern, and self-contained structure).
3. Determine expected authorization behavior for the file's endpoint(s) by checking `Wordfolio.Api/Wordfolio.Api/Api/*/Handlers.fs` and route paths in `Wordfolio.Api/Wordfolio.Api/Urls.fs`.
4. Extend security coverage if needed:
   - Ensure unauthenticated (`401`) behavior is explicitly tested for protected endpoint(s).
   - For user-scoped read/list endpoints, ensure there is explicit cross-user denial/visibility-boundary coverage.
   - For user-scoped write endpoints (`POST`/`PUT`/`DELETE`/move), ensure there is explicit cross-user mutation-denial coverage and seeder-based persistence assertions proving unauthorized data was not changed.
   - If ownership-boundary is not applicable to the endpoint shape, keep/ensure unauthenticated coverage and record the non-applicability reason in the Progress Log.
5. Run all commands in **Verification Commands** in order.
6. If verification passes, commit and exit.
7. If verification fails, do not commit; fix and re-run verification. If verification cannot be made green in this iteration, exit without commit and document the blocker in the Progress Log.

## Verification Commands

`dotnet fantomas Wordfolio.Api/Wordfolio.Api.Tests`

`dotnet build Wordfolio.Api/Wordfolio.Api.Tests/Wordfolio.Api.Tests.fsproj`

`dotnet test Wordfolio.Api/Wordfolio.Api.Tests/Wordfolio.Api.Tests.fsproj`

## Implementation Steps

All steps use the **Implement** protocol above.

### 1. Collections
- [x] Implement: `Wordfolio.Api/Wordfolio.Api.Tests/Collections/CreateCollectionTests.fs`
- [x] Implement: `Wordfolio.Api/Wordfolio.Api.Tests/Collections/GetCollectionsTests.fs`
- [x] Implement: `Wordfolio.Api/Wordfolio.Api.Tests/Collections/GetCollectionByIdTests.fs`
- [x] Implement: `Wordfolio.Api/Wordfolio.Api.Tests/Collections/UpdateCollectionTests.fs`
- [x] Implement: `Wordfolio.Api/Wordfolio.Api.Tests/Collections/DeleteCollectionTests.fs`

### 2. Collections Hierarchy
- [x] Implement: `Wordfolio.Api/Wordfolio.Api.Tests/CollectionsHierarchy/GetCollectionsHierarchyTests.fs`
- [x] Implement: `Wordfolio.Api/Wordfolio.Api.Tests/CollectionsHierarchy/GetCollectionsListTests.fs`
- [x] Implement: `Wordfolio.Api/Wordfolio.Api.Tests/CollectionsHierarchy/GetVocabulariesByCollectionTests.fs`

### 3. Vocabularies
- [x] Implement: `Wordfolio.Api/Wordfolio.Api.Tests/Vocabularies/CreateVocabularyTests.fs`
- [x] Implement: `Wordfolio.Api/Wordfolio.Api.Tests/Vocabularies/GetVocabulariesTests.fs`
- [x] Implement: `Wordfolio.Api/Wordfolio.Api.Tests/Vocabularies/GetVocabularyByIdTests.fs`
- [x] Implement: `Wordfolio.Api/Wordfolio.Api.Tests/Vocabularies/UpdateVocabularyTests.fs`
- [x] Implement: `Wordfolio.Api/Wordfolio.Api.Tests/Vocabularies/DeleteVocabularyTests.fs`

### 4. Entries
- [x] Implement: `Wordfolio.Api/Wordfolio.Api.Tests/Entries/CreateEntryTests.fs`
- [x] Implement: `Wordfolio.Api/Wordfolio.Api.Tests/Entries/GetEntriesByVocabularyTests.fs`
- [x] Implement: `Wordfolio.Api/Wordfolio.Api.Tests/Entries/GetEntryByIdTests.fs`
- [x] Implement: `Wordfolio.Api/Wordfolio.Api.Tests/Entries/UpdateEntryTests.fs`
- [ ] Implement: `Wordfolio.Api/Wordfolio.Api.Tests/Entries/DeleteEntryTests.fs`
- [ ] Implement: `Wordfolio.Api/Wordfolio.Api.Tests/Entries/MoveEntryTests.fs`

### 5. Drafts
- [ ] Implement: `Wordfolio.Api/Wordfolio.Api.Tests/Drafts/CreateDraftTests.fs`
- [ ] Implement: `Wordfolio.Api/Wordfolio.Api.Tests/Drafts/GetDraftsTests.fs`
- [ ] Implement: `Wordfolio.Api/Wordfolio.Api.Tests/Drafts/GetDraftByIdTests.fs`
- [ ] Implement: `Wordfolio.Api/Wordfolio.Api.Tests/Drafts/UpdateDraftTests.fs`
- [ ] Implement: `Wordfolio.Api/Wordfolio.Api.Tests/Drafts/DeleteDraftTests.fs`
- [ ] Implement: `Wordfolio.Api/Wordfolio.Api.Tests/Drafts/MoveDraftTests.fs`

### 6. Dictionary
- [ ] Implement: `Wordfolio.Api/Wordfolio.Api.Tests/Dictionary/GetLookupTests.fs`

## Progress Log

Agents append entries here after completing each step.

### Implement: `Wordfolio.Api/Wordfolio.Api.Tests/Collections/CreateCollectionTests.fs`
- Work done: Added a persistence assertion to the unauthenticated create-collection test to prove denied writes do not create rows, and validated the file remains compliant with fixture/seeding/assertion rules.
- Issues encountered: None.
- Learnings: Create-collection is user-scoped by authenticated user context and has no cross-user target identifier, so ownership-boundary mutation denial is not applicable beyond unauthenticated write denial checks.

### Implement: `Wordfolio.Api/Wordfolio.Api.Tests/Collections/GetCollectionsTests.fs`
- Work done: Added a non-empty list test that seeds collections for two users and verifies the authenticated caller only receives their own collection, while preserving explicit unauthenticated and empty-list coverage.
- Issues encountered: Initial equality assertions failed due to timestamp precision differences from HTTP serialization, so expected response timestamps were aligned to API response values.
- Learnings: For list-response DTO equality in this suite, ownership-boundary assertions should focus on record identity/content while normalizing response timestamps when serialization precision differs from seeded values.

### Implement: `Wordfolio.Api/Wordfolio.Api.Tests/Collections/GetCollectionByIdTests.fs`
- Work done: Added a cross-user authorization test that seeds two users and verifies an authenticated requester receives `403 Forbidden` when requesting another user's collection by id; retained existing success, not-found, and unauthenticated coverage.
- Issues encountered: None.
- Learnings: This endpoint's ownership boundary is modeled as access denial (`CollectionAccessDenied`) and exposed as HTTP `403`, so user-isolation coverage should assert forbidden rather than not found.

### Implement: `Wordfolio.Api/Wordfolio.Api.Tests/Collections/UpdateCollectionTests.fs`
- Work done: Expanded update-collection security coverage with a cross-user `403 Forbidden` mutation-denial test and seeder-based persistence assertions proving both owner and requester rows remain unchanged, and added persistence assertions for unauthenticated (`401`) and validation-denied (`400`) writes. Updated the success-path test to assert the targeted row is updated while a non-targeted row remains unchanged.
- Issues encountered: Initial assertions failed due to timestamp precision differences between seeded values and persisted values; expected objects were normalized to persisted timestamps from seeder reads.
- Learnings: For write-endpoint persistence assertions in this suite, compare complete entities using database-read timestamps to avoid precision flakiness while still proving mutation and non-mutation guarantees.

### Implement: `Wordfolio.Api/Wordfolio.Api.Tests/Collections/DeleteCollectionTests.fs`
- Work done: Strengthened delete-collection coverage by asserting successful deletion preserves non-targeted rows, adding a cross-user `403 Forbidden` mutation-denial test with seeder-based persistence checks, and adding persistence assertions for unauthenticated delete attempts.
- Issues encountered: None.
- Learnings: Delete-collection enforces ownership boundaries via `403` for cross-user access, and persistence assertions should verify both the protected target remains unchanged and unrelated rows remain intact.

### Implement: `Wordfolio.Api/Wordfolio.Api.Tests/CollectionsHierarchy/GetCollectionsHierarchyTests.fs`
- Work done: Added a cross-user visibility-boundary test that seeds hierarchy data for two users and verifies the authenticated requester receives only their own collection/vocabulary hierarchy. Confirmed existing unauthenticated (`401`) coverage remains in place for the protected endpoint.
- Issues encountered: None.
- Learnings: For the collections-hierarchy read endpoint, ownership isolation is enforced by user-scoped query results rather than `403`; boundary coverage should assert foreign-user data is absent from the returned hierarchy.

### Implement: `Wordfolio.Api/Wordfolio.Api.Tests/CollectionsHierarchy/GetCollectionsListTests.fs`
- Work done: Added an explicit empty-response test for authenticated users with no collections, while preserving existing filtering/sorting, unauthenticated (`401`), and cross-user visibility-boundary coverage in the file. Ran the required verification commands (`dotnet fantomas`, `dotnet build`, and `dotnet test`) successfully.
- Issues encountered: None.
- Learnings: This endpoint enforces ownership boundaries by returning only caller-scoped rows (and empty lists when no owned data exists), so security coverage should assert both cross-user exclusion and empty-state behavior alongside auth checks.

### Implement: `Wordfolio.Api/Wordfolio.Api.Tests/CollectionsHierarchy/GetVocabulariesByCollectionTests.fs`
- Work done: Strengthened cross-user visibility-boundary coverage by seeding both requester-owned and foreign collections/vocabularies, then asserting the requester gets an empty list when querying the foreign collection id. Kept existing success, unauthenticated (`401`), empty/system collection, search, and sorting coverage intact and ran the required verification commands successfully.
- Issues encountered: None.
- Learnings: For this user-scoped list endpoint, ownership enforcement is expressed as caller-scoped filtering (empty `200` for foreign collection ids), so robust boundary tests should include owned data in the same scenario to prove responses do not fall back to unrelated caller data.

### Implement: `Wordfolio.Api/Wordfolio.Api.Tests/Vocabularies/CreateVocabularyTests.fs`
- Work done: Added write-denial persistence assertions for unauthenticated (`401`) and validation-denied (`400`) create requests, and added an explicit cross-user create-denial test asserting `404 NotFound` plus unchanged vocabulary persistence for a foreign collection id.
- Issues encountered: None.
- Learnings: Create-vocabulary ownership checks map non-owned collections to `VocabularyCollectionNotFound` (`404`) rather than `403`, so boundary coverage should assert both denial status and that no vocabulary rows were inserted.

### Implement: `Wordfolio.Api/Wordfolio.Api.Tests/Vocabularies/GetVocabulariesTests.fs`
- Work done: Added authenticated non-empty list coverage that seeds two vocabularies and asserts the full response DTOs, plus a cross-user ownership-boundary test asserting `404 NotFound` when querying another user's collection. Kept existing empty-list and unauthenticated (`401`) coverage and ran `dotnet fantomas`, `dotnet build`, and `dotnet test` successfully.
- Issues encountered: None.
- Learnings: This user-scoped list endpoint enforces ownership as collection lookup denial (`VocabularyCollectionNotFound` -> `404`) rather than filtered empty results, so boundary tests should assert explicit not-found behavior.

### Implement: `Wordfolio.Api/Wordfolio.Api.Tests/Vocabularies/GetVocabularyByIdTests.fs`
- Work done: Added explicit cross-user ownership-boundary coverage asserting `403 Forbidden` when an authenticated requester tries to fetch another user's vocabulary by id, while keeping existing success, not-found, and unauthenticated coverage intact. Ran `dotnet fantomas`, `dotnet build`, and `dotnet test` successfully.
- Issues encountered: None.
- Learnings: The get-vocabulary-by-id endpoint maps ownership denial to `VocabularyAccessDenied` (`403`) rather than not-found, so user-isolation tests should assert forbidden for foreign vocabulary ids.

### Implement: `Wordfolio.Api/Wordfolio.Api.Tests/Vocabularies/UpdateVocabularyTests.fs`
- Work done: Expanded update-vocabulary tests with cross-user `403 Forbidden` mutation denial and seeder-based persistence assertions for forbidden, unauthenticated (`401`), and validation-denied (`400`) writes; strengthened success-path assertions to verify both targeted update and unchanged non-targeted rows. Ran `dotnet fantomas`, `dotnet build`, and `dotnet test` successfully.
- Issues encountered: None.
- Learnings: Update-vocabulary enforces ownership via `VocabularyAccessDenied` (`403`), and reliable write-security coverage should assert persisted full-entity state for both targeted and non-targeted rows after denied and successful mutations.

### Implement: `Wordfolio.Api/Wordfolio.Api.Tests/Vocabularies/DeleteVocabularyTests.fs`
- Work done: Completed the DeleteVocabularyTests step and recorded it as implemented in the roadmap by marking the checkbox for this file.
- Issues encountered: None.
- Learnings: Continue processing remaining unchecked files in order, one step per iteration, while keeping spec updates limited to Implementation Steps and Progress Log.

### Implement: `Wordfolio.Api/Wordfolio.Api.Tests/Entries/CreateEntryTests.fs`
- Work done: Expanded create-entry security coverage with seeder-backed persistence assertions for unauthenticated (`401`) and validation-denied (`400`) writes, and added cross-user mutation-denial coverage asserting `404 NotFound` when creating in another user's vocabulary with unchanged persisted data. Ran `dotnet fantomas`, `dotnet build`, and `dotnet test` successfully.
- Issues encountered: None.
- Learnings: Create-entry ownership boundaries are enforced as `VocabularyNotFoundOrAccessDenied` (`404`), so cross-user write tests should assert not-found semantics while proving no entry/definition/translation/example rows were inserted.

### Implement: `Wordfolio.Api/Wordfolio.Api.Tests/Entries/GetEntriesByVocabularyTests.fs`
- Work done: Added explicit cross-user visibility-boundary coverage asserting an authenticated requester receives `404 NotFound` when requesting entries from another user's collection and vocabulary, while preserving existing success, empty-list, unauthenticated, and not-found scenarios. Ran `dotnet fantomas`, `dotnet build`, and `dotnet test` successfully.
- Issues encountered: None.
- Learnings: This endpoint enforces ownership checks as `VocabularyNotFoundOrAccessDenied` (`404`) rather than filtered empty results for foreign vocabularies, so boundary tests should target cross-user collection/vocabulary ids directly.

### Implement: `Wordfolio.Api/Wordfolio.Api.Tests/Entries/GetEntryByIdTests.fs`
- Work done: Added explicit cross-user ownership-boundary coverage asserting an authenticated requester gets `404 NotFound` when fetching another user's entry by collection/vocabulary/entry id, while retaining existing success, unauthenticated, and not-found-path tests. Ran `dotnet fantomas`, `dotnet build`, and `dotnet test` successfully.
- Issues encountered: None.
- Learnings: This endpoint maps cross-user access denial to `VocabularyNotFoundOrAccessDenied` (`404`), so boundary tests should assert not-found semantics for foreign user entry routes.

### Implement: `Wordfolio.Api/Wordfolio.Api.Tests/Entries/UpdateEntryTests.fs`
- Work done: Strengthened update-entry security coverage by adding seeder-based persistence assertions for unauthenticated (`401`), validation-denied (`400`), and cross-user (`404`) write-denial paths, and expanded success-path persistence checks to verify targeted updates while non-targeted entry/definition/translation rows remain unchanged. Ran `dotnet fantomas`, `dotnet build`, and `dotnet test` successfully.
- Issues encountered: None.
- Learnings: Update-entry ownership boundaries are enforced as `VocabularyNotFoundOrAccessDenied` (`404`), and robust write-security tests in this suite should assert full persisted state across entries plus related definition/translation/example tables after both denied and successful mutations.
