# Plan: Move Vocabulary List Sorting/Filtering to Client-Side (Server-Side Changes)

GitHub issue: #179

## Context

The `GET /collections-hierarchy/collections/{collectionId}/vocabularies` endpoint currently accepts `search`, `sortBy`, and `sortDirection` query parameters and performs server-side ILIKE filtering with dynamic ORDER BY. Since users are expected to have few vocabularies per collection, this overhead is unnecessary. The server should return all vocabularies for a collection (with entry counts) and let the client handle sorting/filtering.

The existing `searchCollectionVocabulariesAsync` data access function is replaced by a new simpler `getVocabulariesWithEntryCountByCollectionIdAsync`. The other vocabulary list endpoint (`GET /collections/{collectionId}/vocabularies`, the `Vocabularies` handler) is **not changed**.

`SortDirection`, `CollectionSortBy`, `SearchUserCollectionsQuery`, `SortDirectionRequest`, `CollectionSortByRequest` are **kept** — they are still used by the collections search endpoint (`searchUserCollectionsAsync`), which is not changed in this issue.

**Response fields:** `id`, `name`, `description`, `entryCount`, `createdAt`, `updatedAt` — **no** `collectionId`.

## Changes by Layer

### 1. Data Access — `Wordfolio.Api/Wordfolio.Api.DataAccess/CollectionsHierarchy.fs`

**Remove:**
- `VocabularySortBy` DU (lines 229-233)
- `SearchCollectionVocabulariesQuery` record (lines 235-238)
- `searchCollectionVocabulariesAsync` function (lines 240-310)

**Keep** (used by existing collections search):
- `SortDirection`
- `CollectionSortBy`
- `SearchUserCollectionsQuery`
- `escapeLikeWildcards`

**Add** `getVocabulariesWithEntryCountByCollectionIdAsync`:
- Signature: `int -> int -> IDbConnection -> IDbTransaction -> CancellationToken -> Task<VocabularyWithEntryCount list>`
- Parameters: `userId`, `collectionId`, then infrastructure params
- SQL with fixed sort order `UpdatedAt DESC NULLS LAST, Id`:
```sql
SELECT
    v."Id", v."Name", v."Description", v."CreatedAt", v."UpdatedAt", v."IsDefault",
    COALESCE(e."EntryCount", 0) as "EntryCount"
FROM wordfolio."Vocabularies" v
INNER JOIN wordfolio."Collections" c ON c."Id" = v."CollectionId"
LEFT JOIN (
    SELECT "VocabularyId", COUNT(*) as "EntryCount"
    FROM wordfolio."Entries"
    GROUP BY "VocabularyId"
) e ON e."VocabularyId" = v."Id"
WHERE c."UserId" = @UserId AND c."Id" = @CollectionId
  AND c."IsSystem" = false AND v."IsDefault" = false
ORDER BY v."UpdatedAt" DESC NULLS LAST, v."Id"
```
- Returns `VocabularyWithEntryCount list` via the existing `VocabularyRecord` internal type

### 2. Domain Types — `Wordfolio.Api/Wordfolio.Api.Domain/CollectionsHierarchy/Types.fs`

**Remove:**
- `VocabularySortBy` DU (lines 50-54)
- `SearchCollectionVocabulariesQuery` record (lines 56-59)

**Keep** all other types including `SortDirection`, `CollectionSortBy`, `SearchUserCollectionsQuery`.

### 3. Domain Capabilities — `Wordfolio.Api/Wordfolio.Api.Domain/CollectionsHierarchy/Capabilities.fs`

**Remove:**
- `SearchCollectionVocabulariesData` record (lines 11-14)
- `ISearchCollectionVocabularies` interface (lines 22-23)
- `searchCollectionVocabularies` module function (line 34)

**Add:**
- `IGetVocabulariesWithEntryCountByCollectionId` interface:
  ```fsharp
  abstract GetVocabulariesWithEntryCountByCollectionId: UserId -> CollectionId -> Task<VocabularyWithEntryCount list>
  ```
- `getVocabulariesWithEntryCountByCollectionId` module function:
  ```fsharp
  let getVocabulariesWithEntryCountByCollectionId (env: #IGetVocabulariesWithEntryCountByCollectionId) userId collectionId =
      env.GetVocabulariesWithEntryCountByCollectionId(userId, collectionId)
  ```

Note: The capability uses **separate curried parameters** (`UserId -> CollectionId -> ...`), not a data record, because there are only two simple inputs and no `...Data` record is needed.

### 4. Domain Operations — `Wordfolio.Api/Wordfolio.Api.Domain/CollectionsHierarchy/Operations.fs`

**Remove:**
- `SearchCollectionVocabulariesParameters` record (lines 15-18)
- `searchCollectionVocabularies` operation (lines 51-65)

**Add:**
- `GetVocabulariesWithEntryCountByCollectionIdParameters` record:
  ```fsharp
  type GetVocabulariesWithEntryCountByCollectionIdParameters =
      { UserId: UserId
        CollectionId: CollectionId }
  ```
- `getVocabulariesWithEntryCountByCollectionId` operation:
  ```fsharp
  let getVocabulariesWithEntryCountByCollectionId
      env
      (parameters: GetVocabulariesWithEntryCountByCollectionIdParameters)
      : Task<Result<VocabularyWithEntryCount list, unit>> =
      runInTransaction env (fun appEnv ->
          task {
              let! vocabularies =
                  getVocabulariesWithEntryCountByCollectionId
                      appEnv
                      parameters.UserId
                      parameters.CollectionId

              return (Ok vocabularies)
          })
  ```

### 5. AppEnv — `Wordfolio.Api/Wordfolio.Api/Infrastructure/Environment.fs`

**Remove:**
- `toVocabularySortByDataAccess` (lines 168-173)
- `toSearchCollectionVocabulariesQueryDataAccess` (lines 175-180)
- `ISearchCollectionVocabularies` implementation (lines 653-669)

**Keep:** `toSortDirectionDataAccess`, `toCollectionSortByDataAccess`, `toSearchUserCollectionsQueryDataAccess` (all used by collections search), `toVocabularyWithEntryCountDomain` (reused by new impl).

**Add** `IGetVocabulariesWithEntryCountByCollectionId` implementation:
```fsharp
interface IGetVocabulariesWithEntryCountByCollectionId with
    member _.GetVocabulariesWithEntryCountByCollectionId(UserId userId, CollectionId collectionId) =
        task {
            let! results =
                Wordfolio.Api.DataAccess.CollectionsHierarchy.getVocabulariesWithEntryCountByCollectionIdAsync
                    userId
                    collectionId
                    connection
                    transaction
                    cancellationToken

            return results |> List.map toVocabularyWithEntryCountDomain
        }
```

### 6. API Types — `Wordfolio.Api/Wordfolio.Api/Api/CollectionsHierarchy/Types.fs`

**Remove:** `VocabularySortByRequest` enum (lines 11-15)

**Keep:** `CollectionSortByRequest`, `SortDirectionRequest`, all response types.

### 7. API Mappers — `Wordfolio.Api/Wordfolio.Api/Api/CollectionsHierarchy/Mappers.fs`

**Remove:**
- `toVocabularySortByDomain` (lines 37-43)
- `toCollectionVocabulariesQuery` (lines 72-83)

**Keep:** `toVocabularyWithEntryCountResponse`, `toCollectionWithVocabulariesResponse`, `toCollectionWithVocabularyCountResponse`, `toCollectionSortByDomain`, `toSortDirectionDomain`, `toSearchQuery`.

### 8. API Handler — `Wordfolio.Api/Wordfolio.Api/Api/CollectionsHierarchy/Handlers.fs`

**Modify** the `VocabulariesByCollectionPath` endpoint (lines 89-121):
- Remove `search`, `sortBy`, `sortDirection` from `Func` signature
- New signature: `Func<int, ClaimsPrincipal, NpgsqlDataSource, CancellationToken, _>`
- Remove `toCollectionVocabulariesQuery` call
- Replace `searchCollectionVocabularies` call with `getVocabulariesWithEntryCountByCollectionId`:
  ```fsharp
  let! result =
      getVocabulariesWithEntryCountByCollectionId
          env
          { UserId = UserId userId
            CollectionId = CollectionId collectionId }
  ```

### 9. Tests

#### Data Access Tests
- **Delete** `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/CollectionsHierarchy/SearchCollectionVocabulariesTests.fs`
- **Create** `CollectionsHierarchy/GetVocabulariesWithEntryCountByCollectionIdTests.fs` (written from scratch):
  1. Returns vocabularies with correct entry counts (excludes default vocabulary)
  2. Returns empty list for non-existent collection
  3. Returns empty list for another user's collection
  4. Returns empty list for system collection
  5. Returns empty list when collection has no vocabularies
  6. Returns vocabularies sorted by `UpdatedAt DESC NULLS LAST, Id` (the sorting test)
- **Update** `.fsproj` line 35: replace `SearchCollectionVocabulariesTests.fs` → `GetVocabulariesWithEntryCountByCollectionIdTests.fs`

#### Domain Tests
- **Delete** `Wordfolio.Api/Wordfolio.Api.Domain.Tests/CollectionsHierarchy/SearchCollectionVocabulariesTests.fs`
- **Create** `CollectionsHierarchy/GetVocabulariesWithEntryCountByCollectionIdTests.fs` (written from scratch):
  - `TestEnv` implementing `IGetVocabulariesWithEntryCountByCollectionId` + `ITransactional<TestEnv>`
  - Tracks calls as `(UserId * CollectionId) list`
  - Test: passes correct userId and collectionId to capability, returns Ok vocabularies, asserts calls
- **Update** `.fsproj` line 35: replace `SearchCollectionVocabulariesTests.fs` → `GetVocabulariesWithEntryCountByCollectionIdTests.fs`

#### API Integration Tests
- **Rewrite** `Wordfolio.Api/Wordfolio.Api.Tests/CollectionsHierarchy/GetVocabulariesByCollectionTests.fs`:
  - Remove `VocabulariesQueryOptions` record and `VocabulariesUrlHelpers` module
  - URL becomes plain `Urls.CollectionsHierarchy.vocabulariesByCollection collectionId` (no query string)
  - Keep tests: returns vocabularies with entry counts, returns empty for other user's collection, returns empty for system collection, returns 401 without authentication
  - Remove tests: supports search filtering, supports sorting (these are now client-side concerns)
  - No `.fsproj` change needed (same file name)

## Verification

```bash
dotnet build       # Verify compilation
dotnet test        # Run all tests
dotnet fantomas .  # Format F# code
```
