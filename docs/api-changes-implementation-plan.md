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

### Create migration file: `20260110001_AddSystemCollectionsAndDefaultVocabularies.fs`

**Steps:**
1. Add `IsSystem bit NOT NULL DEFAULT 0` to Collections table
2. Add `IsDefault bit NOT NULL DEFAULT 0` to Vocabularies table
3. Add migration to `Wordfolio.Api.Migrations.fsproj`

**Run migration after creation.**

---

## Task 2: DataAccess Layer - Collections

### File: `Wordfolio.Api.DataAccess/Collections.fs`

**2.1 Add internal record with system flag**
```fsharp
[<CLIMutable>]
type internal CollectionRecordWithSystemFlag =
    { Id: int
      UserId: int
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: Nullable<DateTimeOffset>
      IsSystem: bool }
```

**2.2 Update `collectionsTable` to use `CollectionRecordWithSystemFlag`**

**2.3 Modify existing queries to filter out system collections:**
- `getCollectionsByUserIdAsync`: Add `WHERE IsSystem = 0`
- `getCollectionByIdAsync`: Add `WHERE IsSystem = 0`
- `createCollectionAsync`: Set `IsSystem = 0`

**2.4 Add new functions:**
- `getDefaultCollectionByUserIdAsync`: Returns "Unsorted" collection (IsSystem = 1)
- `createDefaultCollectionAsync`: Creates "Unsorted" collection with name "Unsorted" (IsSystem = 1)

**2.5 Add tree query:**
- `getCollectionsWithVocabulariesByUserIdAsync`:
  - Returns: `{ Collection: Collection; Vocabularies: Vocabulary list } list`
  - Filters: `IsSystem = 0` (collections) and `IsDefault = 0` (vocabularies)
  - JOIN with Vocabularies table to get nested vocabularies
  - Returns structured tree data for sidebar

---

## Task 3: DataAccess Layer - Vocabularies

### File: `Wordfolio.Api.DataAccess/Vocabularies.fs`

**3.1 Add internal record with default flag**
```fsharp
[<CLIMutable>]
type internal VocabularyRecordWithDefaultFlag =
    { Id: int
      CollectionId: int
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: Nullable<DateTimeOffset>
      IsDefault: bool }
```

**3.2 Update `vocabulariesTable` to use `VocabularyRecordWithDefaultFlag`**

**3.3 Modify existing queries to filter out default vocabularies:**
- `getVocabulariesByCollectionIdAsync`: Add `WHERE IsDefault = 0`
- `getVocabularyByIdAsync`: Add `WHERE IsDefault = 0`
- `createVocabularyAsync`: Set `IsDefault = 0`
- `updateVocabularyAsync`: Preserve `IsDefault` value
- `deleteVocabularyAsync`: Only allow deletion if not default

**3.4 Add new functions:**
- `getDefaultVocabularyByUserIdAsync`: Returns default vocabulary (IsDefault = 1)
  - JOIN with Collections to verify user owns the collection
- `createDefaultVocabularyAsync`: Creates default vocabulary in "Unsorted" collection with name "Unsorted" (IsDefault = 1)

**3.5 Add count function:**
- `getVocabularyEntryCountAsync(vocabularyId, connection, transaction, cancellationToken)`: Returns int
  - Used for populating `EntryCount` in responses

---

## Task 4: Domain Layer - Vocabularies Errors

### File: `Wordfolio.Api.Domain/Vocabularies/Errors.fs`

**4.1 Add new error types:**
```fsharp
type VocabularyError =
    | VocabularyNotFound of VocabularyId
    | VocabularyAccessDenied of UserId * VocabularyId
    | VocabularyNameRequired
    | VocabularyNameTooLong of int
    | VocabularyCollectionNotFound of CollectionId
    | DefaultVocabularyNotFound of UserId  // NEW
```

---

## Task 5: Domain Layer - Vocabularies Operations

### File: `Wordfolio.Api.Domain/Vocabularies/Operations.fs`

**5.1 Add new operation:**
```fsharp
let getDefaultOrCreateAsync
    (env: TransactionalEnv)
    (userId: UserId)
    : Task<Result<Vocabulary, VocabularyError>>
```

**Logic:**
1. Try to get existing default vocabulary for user
2. If found, return `Ok vocabulary`
3. If not found:
   - Check if "Unsorted" collection exists
   - If not, create it
   - Create default vocabulary in "Unsorted" collection
   - Return `Ok defaultVocabulary`
4. Handle errors appropriately (access denied, etc.)

---

## Task 6: Handler Layer - Collections

### File: `Wordfolio.Api/Handlers/Collections.fs`

**6.1 Add new response types:**
```fsharp
type VocabularySummaryResponse =
    { Id: int
      Name: string
      Description: string option }

type CollectionResponse =
    { Id: int
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset option
      VocabularyCount: int option
      Vocabularies: VocabularySummaryResponse list option }
```

**6.2 Modify `GET /collections`:**
- Accept optional query parameter `include: string[]` (e.g., `?include=vocabularies`)
- When `vocabularies` in include array:
  - Call `getCollectionsWithVocabulariesByUserIdAsync`
  - Populate `Vocabularies` and `VocabularyCount` for each collection
- When `vocabularies` NOT in include:
  - Call `getCollectionsByUserIdAsync`
  - Optionally populate `VocabularyCount` (count query per collection, or modify query to include counts)
  - Set `Vocabularies` to `None`

**6.3 Modify `GET /collections/{id}`:**
- Add `VocabularyCount` to response
- Get count from `Vocabularies.getVocabularyCountByCollectionIdAsync` (need to add this function)

---

## Task 7: Handler Layer - Vocabularies

### File: `Wordfolio.Api/Handlers/Vocabularies.fs`

**7.1 Modify `VocabularyResponse`:**
```fsharp
type VocabularyResponse =
    { Id: int
      CollectionId: int
      Name: string
      Description: string option
      CreatedAt: DateTimeOffset
      UpdatedAt: DateTimeOffset option
      EntryCount: int option }
```

**7.2 Modify `GET /collections/{collectionId}/vocabularies`:**
- For each vocabulary, populate `EntryCount` using `getVocabularyEntryCountAsync`
- Note: This endpoint excludes default vocabularies (IsDefault = 0 filter in data layer)

**7.3 Modify `GET /collections/{collectionId}/vocabularies/{id}`:**
- Populate `EntryCount` field
- Note: This endpoint excludes default vocabularies (IsDefault = 0 filter in data layer)

---

## Task 8: Handler Layer - Entries

### File: `Wordfolio.Api/Handlers/Entries.fs`

**8.1 Modify `CreateEntryRequest`:**
```fsharp
type CreateEntryRequest =
    { VocabularyId: int option  // Made optional (was required)
      EntryText: string
      Definitions: DefinitionRequest list
      Translations: TranslationRequest list }
```

**8.2 Modify `POST /entries`:**
- If `VocabularyId` is `None`:
  - Call `getDefaultOrCreateAsync` to get or create default vocabulary
  - Use returned vocabulary ID
  - Return entry with `VocabularyId` populated
- If `VocabularyId` is `Some`:
  - Validate vocabulary exists and is accessible
  - Use existing logic

**8.3 Add new endpoint `DELETE /entries/{id}`:**
```fsharp
group.MapDelete(
    UrlTokens.ById,
    Func<int, ClaimsPrincipal, NpgsqlDataSource, CancellationToken, _>
        (fun id user dataSource cancellationToken ->
            task {
                match getUserId user with
                | None -> return Results.Unauthorized()
                | Some userId ->
                    let env = TransactionalEnv(dataSource, cancellationToken)
                    let! result = delete env (UserId userId) (EntryId id)

                    return
                        match result with
                        | Ok() -> Results.NoContent()
                        | Error error -> toErrorResponse error
            })
)
```

**8.4 Update error mapping in `toErrorResponse`:**
- Ensure all `EntryError` cases are handled

---

## Task 9: DataAccess Layer - Add Missing Count Functions

### File: `Wordfolio.Api.DataAccess/Collections.fs`

**Add function:**
```fsharp
let getVocabularyCountByCollectionIdAsync
    (collectionId: int)
    (connection: IDbConnection)
    (transaction: IDbTransaction)
    (cancellationToken: CancellationToken)
    : Task<int>
```

- Count vocabularies in collection (excluding default: `WHERE CollectionId = @collectionId AND IsDefault = 0`)

---

## Task 10: Update Tests

### 10.1 Database Tests
- Test new columns in migrations

### 10.2 DataAccess Tests
- Test filtering of system collections and default vocabularies
- Test `getCollectionsWithVocabulariesByUserIdAsync` returns correct tree structure
- Test `getDefaultVocabularyByUserIdAsync`
- Test `createDefaultCollectionAsync` and `createDefaultVocabularyAsync`
- Test count functions

### 10.3 Domain Tests
- Test `getDefaultOrCreateAsync` operation
  - Test when default vocabulary exists
  - Test when default vocabulary doesn't exist (should create)
  - Test error cases

### 10.4 Integration Tests
- Test `GET /collections?include=vocabularies` returns tree
- Test `GET /collections?include=vocabularies` excludes system collections
- Test `POST /entries` with null `VocabularyId` creates in default vocabulary
- Test `POST /entries` auto-creates default vocabulary if needed
- Test `DELETE /entries/{id}` works correctly
- Test `EntryCount` is populated in responses

---

## Task 11: Run Verification Commands

### Backend
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

1. **Database Migration** (Task 1)
2. **DataAccess Layer** (Tasks 2, 3, 9) - depends on migration
3. **Domain Layer** (Tasks 4, 5) - depends on DataAccess
4. **Handler Layer** (Tasks 6, 7, 8) - depends on Domain
5. **Tests** (Task 10) - can be done alongside development
6. **Verification** (Task 11)

---

## Notes

- **No changes to public API models** for `IsSystem` and `IsDefault` - these are internal only
- **System collections and default vocabularies are filtered out by default** in all queries
- **Only accessible via specific functions** when deliberately loading them
- **Unsorted collection and default vocabulary are created automatically** when first needed
- **Frontend will fetch tree data** via `GET /collections?include=vocabularies` for sidebar
- **Word entry without specifying vocabulary** uses default vocabulary automatically
