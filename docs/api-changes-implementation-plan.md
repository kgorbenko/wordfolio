# API Changes Implementation Plan

This document outlines all API changes required to support the vocabulary management UX design (see `vocabulary-management-ux-design.md`).

---

## Overview

| Change Type | Description |
|-------------|-------------|
| Database | Add `IsSystem` and `IsDefault` columns |
| DataAccess | Filter system/default entities, add tree queries |
| Domain | Add default vocabulary operations |
| Handlers | Add DELETE endpoint, modify POST, add query parameter |
| Tests | Update/add tests for new behavior |

---

## Task 1: Database Migration

- [x] Create migration file: `20260110001_AddSystemCollectionsAndDefaultVocabularies.fs`
- [x] Add `IsSystem bit NOT NULL DEFAULT 0` to Collections table
- [x] Add `IsDefault bit NOT NULL DEFAULT 0` to Vocabularies table
- [x] Add migration to `Wordfolio.Api.Migrations.fsproj`
- [x] Run migration

---

## Task 2: DataAccess Layer - Collections

### File: `Wordfolio.Api.DataAccess/Collections.fs`

- [x] 2.1 Update `CollectionRecord` to include `IsSystem: bool`
- [x] 2.2 Update `CollectionInsertParameters` to include `IsSystem: bool`
- [x] 2.3 Modify existing queries to filter out system collections:
  - [x] `getCollectionsByUserIdAsync`: Add `WHERE IsSystem = false`
  - [x] `getCollectionByIdAsync`: Add `WHERE IsSystem = false`
  - [x] `createCollectionAsync`: Set `IsSystem = false`
  - [x] `updateCollectionAsync`: Add `WHERE IsSystem = false`
  - [x] `deleteCollectionAsync`: Add `WHERE IsSystem = false`
- [x] 2.4 Add new functions:
  - [x] `getDefaultCollectionByUserIdAsync`: Returns "Unsorted" collection (IsSystem = true)
  - [x] `createDefaultCollectionAsync`: Creates "Unsorted" collection with name "Unsorted" (IsSystem = true)
- [x] 2.5 Add tree query:
  - [x] `getCollectionsWithVocabulariesByUserIdAsync`: Returns `{ Collection: Collection; Vocabularies: Vocabulary list } list`

---

## Task 3: DataAccess Layer - Vocabularies

### File: `Wordfolio.Api.DataAccess/Vocabularies.fs`

- [x] 3.1 Update `VocabularyRecord` to include `IsDefault: bool`
- [x] 3.2 Update `VocabularyInsertParameters` to include `IsDefault: bool`
- [x] 3.3 Add `CollectionRecord` with `IsSystem: bool` for join queries
- [x] 3.4 Modify existing queries to filter out default vocabularies AND vocabularies in system collections:
  - [x] `getVocabularyByIdAsync`: Add join with Collections, filter `IsDefault = false AND IsSystem = false`
  - [x] `getVocabulariesByCollectionIdAsync`: Add join with Collections, filter `IsDefault = false AND IsSystem = false`
  - [x] `getVocabularyByIdAndUserIdAsync`: Add filter `IsDefault = false AND IsSystem = false`
  - [x] `createVocabularyAsync`: Set `IsDefault = false`
  - [x] `updateVocabularyAsync`: Use `NOT EXISTS` subquery to filter out default vocabularies and system collections
  - [x] `deleteVocabularyAsync`: Use `NOT EXISTS` subquery to filter out default vocabularies and system collections
- [x] 3.5 Add new functions:
  - [x] `getDefaultVocabularyByUserIdAsync`: Returns default vocabulary (IsDefault = true)
  - [x] `createDefaultVocabularyAsync`: Creates default vocabulary in "Unsorted" collection with name "Unsorted" (IsDefault = true)
- [x] ~~3.6 Add count function~~ (REMOVED - not needed)
  - ~~`getVocabularyEntryCountAsync(vocabularyId, connection, transaction, cancellationToken)`: Returns int~~

---

## Task 4: Domain Layer - Vocabularies Errors

### File: `Wordfolio.Api.Domain/Vocabularies/Errors.fs`

- [ ] 4.1 Add new error type: `DefaultVocabularyNotFound of UserId`

---

## Task 5: Domain Layer - Vocabularies Operations

### File: `Wordfolio.Api.Domain/Vocabularies/Operations.fs`

- [ ] 5.1 Add new operation: `getDefaultOrCreateAsync`
  - Try to get existing default vocabulary for user
  - If not found, check if "Unsorted" collection exists (create if not)
  - Create default vocabulary in "Unsorted" collection
  - Return vocabulary

---

## Task 6: Handler Layer - Collections

### File: `Wordfolio.Api/Handlers/Collections.fs`

- [ ] 6.1 Add new response types: `VocabularySummaryResponse`, `CollectionWithVocabulariesResponse`
- [ ] 6.2 Modify `GET /collections`: Accept optional query parameter `include=vocabularies`

---

## ~~Task 7: Handler Layer - Vocabularies~~ (REMOVED)

### File: `Wordfolio.Api/Handlers/Vocabularies.fs`

- ~~[ ] 7.1 Modify `VocabularyResponse`: Add `EntryCount: int option`~~ (REMOVED - not needed)
- ~~[ ] 7.2 Modify `GET /collections/{collectionId}/vocabularies`: Populate `EntryCount`~~ (REMOVED - not needed)
- ~~[ ] 7.3 Modify `GET /collections/{collectionId}/vocabularies/{id}`: Populate `EntryCount`~~ (REMOVED - not needed)

---

## Task 8: Handler Layer - Entries

### File: `Wordfolio.Api/Handlers/Entries.fs`

- [ ] 8.1 Modify `CreateEntryRequest`: Make `VocabularyId` optional
- [ ] 8.2 Modify `POST /entries`: Use default vocabulary when `VocabularyId` is None
- [ ] 8.3 Add new endpoint `DELETE /entries/{id}`
- [ ] 8.4 Update error mapping in `toErrorResponse`

---

## ~~Task 9: DataAccess Layer - Add Missing Count Functions~~ (REMOVED)

### File: `Wordfolio.Api.DataAccess/Collections.fs`

- [x] ~~Add `getVocabularyCountByCollectionIdAsync`: Count vocabularies in collection (excluding default)~~ (REMOVED - not needed)

---

## Task 10: Tests

### 10.1 DataAccess Tests - Collections
- [x] Test `createCollectionAsync` sets `IsSystem = false`
- [x] Test `getCollectionByIdAsync` returns None for system collections
- [x] Test `getCollectionsByUserIdAsync` filters out system collections
- [x] Test `updateCollectionAsync` returns 0 for system collections
- [x] Test `deleteCollectionAsync` returns 0 for system collections
- [x] Test `getDefaultCollectionByUserIdAsync`
- [x] Test `createDefaultCollectionAsync`
- [x] Test `getCollectionsWithVocabulariesByUserIdAsync`
- [x] ~~Test `getVocabularyCountByCollectionIdAsync`~~ (REMOVED - function removed)

### 10.2 DataAccess Tests - Vocabularies
- [x] Test `createVocabularyAsync` sets `IsDefault = false`
- [x] Test `getVocabularyByIdAsync` returns None for default vocabularies
- [x] Test `getVocabularyByIdAsync` returns None for vocabularies in system collections
- [x] Test `getVocabulariesByCollectionIdAsync` filters out default vocabularies
- [x] Test `getVocabulariesByCollectionIdAsync` returns empty for system collections
- [x] Test `getVocabularyByIdAndUserIdAsync` returns None for default vocabularies
- [x] Test `getVocabularyByIdAndUserIdAsync` returns None for vocabularies in system collections
- [x] Test `updateVocabularyAsync` returns 0 for default vocabularies
- [x] Test `updateVocabularyAsync` returns 0 for vocabularies in system collections
- [x] Test `deleteVocabularyAsync` returns 0 for default vocabularies
- [x] Test `deleteVocabularyAsync` returns 0 for vocabularies in system collections
- [x] Test `getDefaultVocabularyByUserIdAsync`
- [x] Test `createDefaultVocabularyAsync`
- [x] ~~Test `getVocabularyEntryCountAsync`~~ (REMOVED - function removed)

### 10.3 Domain Tests
- [ ] Test `getDefaultOrCreateAsync` when default vocabulary exists
- [ ] Test `getDefaultOrCreateAsync` when default vocabulary doesn't exist (should create)
- [ ] Test `getDefaultOrCreateAsync` error cases

### 10.4 Integration Tests
- [ ] Test `GET /collections?include=vocabularies` returns tree
- [ ] Test `GET /collections?include=vocabularies` excludes system collections
- [ ] Test `POST /entries` with null `VocabularyId` creates in default vocabulary
- [ ] Test `POST /entries` auto-creates default vocabulary if needed
- [ ] Test `DELETE /entries/{id}` works correctly

---

## Task 11: Run Verification Commands

```bash
# Build
dotnet build

# Run tests
dotnet test

# Format F# code
dotnet fantomas .

# Format C# code
dotnet format
```

---

## Implementation Order

1. **Database Migration** (Task 1) - COMPLETED
2. **DataAccess Layer - Basic Filtering** (Tasks 2.1-2.3, 3.1-3.4) - COMPLETED
3. **DataAccess Layer - New Functions** (Tasks 2.4-2.5, 3.5) - COMPLETED (Task 3.6, 9 removed)
4. **Domain Layer** (Tasks 4, 5) - TODO
5. **Handler Layer** (Tasks 6, 8) - TODO (Task 7 removed)
6. **Remaining Tests** (Task 10) - DataAccess tests COMPLETED, Domain/Integration tests TODO
7. **Verification** (Task 11) - Run after each step

---

## Notes

- **No changes to public API models** for `IsSystem` and `IsDefault` - these are internal only
- **System collections and default vocabularies are filtered out by default** in all queries
- **Only accessible via specific functions** when deliberately loading them
- **Unsorted collection and default vocabulary are created automatically** when first needed
- **Frontend will fetch tree data** via `GET /collections?include=vocabularies` for sidebar
- **Word entry without specifying vocabulary** uses default vocabulary automatically
- **Vocabulary queries also filter by collection's IsSystem** to prevent accessing vocabularies in system collections
