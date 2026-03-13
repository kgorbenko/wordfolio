# Plan: Move Collections List Sorting/Filtering to Client-Side (Server-Side)

GitHub issue: #178

## Context

The `GET /collections-hierarchy/collections` endpoint currently accepts `search`, `sortBy`, and `sortDirection` query parameters and performs server-side ILIKE filtering with dynamic ORDER BY. Since users are expected to have fewer than 50 collections, this overhead is unnecessary. The server should return all collections for a user (with vocabulary counts) and let the client handle sorting/filtering.

**No existing data access method** returns collections with just a vocabulary count — a new simpler function is needed to replace `searchUserCollectionsAsync`.

## Changes by Layer

### 1. Data Access — `Wordfolio.Api/Wordfolio.Api.DataAccess/CollectionsHierarchy.fs`

**Remove:**
- `CollectionSortBy` DU (lines 36-40)
- `SearchUserCollectionsQuery` record (lines 46-49)
- `searchUserCollectionsAsync` function (lines 155-227)

**Keep** (shared with vocabularies search):
- `SortDirection` (lines 42-44)
- `escapeLikeWildcards` (lines 152-153)
- `CollectionWithVocabularyCount` + `CollectionWithVocabularyCountRecord` — reused by replacement

**Add** `getCollectionsWithVocabularyCountByUserIdAsync`:
- Signature: `int -> IDbConnection -> IDbTransaction -> CancellationToken -> Task<CollectionWithVocabularyCount list>`
- SQL with fixed sort order `UpdatedAt DESC NULLS LAST, Id`:
```sql
SELECT
    c."Id", c."UserId", c."Name", c."Description", c."CreatedAt", c."UpdatedAt",
    COALESCE(v_counts."VocabularyCount", 0) AS "VocabularyCount"
FROM wordfolio."Collections" c
LEFT JOIN (
    SELECT v."CollectionId", COUNT(*) AS "VocabularyCount"
    FROM wordfolio."Vocabularies" v
    WHERE v."IsDefault" = false
    GROUP BY v."CollectionId"
) v_counts ON v_counts."CollectionId" = c."Id"
WHERE c."UserId" = @UserId AND c."IsSystem" = false
ORDER BY c."UpdatedAt" DESC NULLS LAST, c."Id"
```

### 2. Domain Types — `Wordfolio.Api/Wordfolio.Api.Domain/CollectionsHierarchy/Types.fs`

**Remove:**
- `CollectionSortBy` (lines 31-35)
- `SearchUserCollectionsQuery` (lines 41-44)

**Keep:** `SortDirection` (shared with `SearchCollectionVocabulariesQuery`), all other types

### 3. Domain Capabilities — `Wordfolio.Api/Wordfolio.Api.Domain/CollectionsHierarchy/Capabilities.fs`

**Remove:**
- `SearchUserCollectionsData` record (lines 7-9)
- `ISearchUserCollections` interface (lines 19-20)
- `searchUserCollections` module function (line 32)

**Add:**
- `IGetCollectionsWithVocabularyCount` interface: `UserId -> Task<CollectionWithVocabularyCount list>`
- `getCollectionsWithVocabularyCount` module function

### 4. Domain Operations — `Wordfolio.Api/Wordfolio.Api.Domain/CollectionsHierarchy/Operations.fs`

**Remove:**
- `SearchUserCollectionsParameters` (lines 11-13)
- `searchUserCollections` operation (lines 36-49)

**Add:**
- `getCollectionsWithVocabularyCount` operation taking `userId: UserId` directly (no parameter record)
- Wraps capability in `runInTransaction`, returns `Ok collections`
- Return type: `Task<Result<CollectionWithVocabularyCount list, unit>>`

### 5. AppEnv — `Wordfolio.Api/Wordfolio.Api/Infrastructure/Environment.fs`

**Remove:**
- `toCollectionSortByDataAccess` (lines 148-154)
- `toSearchUserCollectionsQueryDataAccess` (lines 161-166)
- `ISearchUserCollections` implementation (lines 636-651)

**Rename:** `toCollectionOverviewDomain` → `toCollectionWithVocabularyCountDomain` (lines 138-146)

**Keep:** `toSortDirectionDataAccess` (used by vocabularies)

**Add:** `IGetCollectionsWithVocabularyCount` implementation — calls new data access function, maps via renamed `toCollectionWithVocabularyCountDomain`

### 6. API Types — `Wordfolio.Api/Wordfolio.Api/Api/CollectionsHierarchy/Types.fs`

**Remove:** `CollectionSortByRequest` enum (lines 5-9)

**Keep:** `SortDirectionRequest`, `VocabularySortByRequest` (used by vocabularies), `CollectionWithVocabularyCountResponse`

### 7. API Mappers — `Wordfolio.Api/Wordfolio.Api/Api/CollectionsHierarchy/Mappers.fs`

**Remove:**
- `toCollectionSortByDomain` (lines 45-51)
- `toSearchQuery` (lines 59-70)

**Keep:** `toCollectionWithVocabularyCountResponse`, `toSortDirectionDomain`, `toVocabularySortByDomain`, `toCollectionVocabulariesQuery`

### 8. API Handler — `Wordfolio.Api/Wordfolio.Api/Api/CollectionsHierarchy/Handlers.fs`

**Modify** the collections list endpoint (lines 56-87):
- Remove `search`, `sortBy`, `sortDirection` from `Func` signature → `Func<ClaimsPrincipal, NpgsqlDataSource, CancellationToken, _>`
- Replace `searchUserCollections` call with `getCollectionsWithVocabularyCount`
- Remove `toSearchQuery` call

### 9. Tests

#### Data Access Tests
- **Delete** `Wordfolio.Api/Wordfolio.Api.DataAccess.Tests/CollectionsHierarchy/SearchUserCollectionsTests.fs`
- **Create** `CollectionsHierarchy/GetCollectionsWithVocabularyCountByUserIdTests.fs` with tests:
  1. Returns collections with correct vocabulary counts
  2. Excludes system collections
  3. Excludes default vocabularies from count
  4. Returns empty list when no collections
  5. Does not return other users' collections
  6. Returns zero count for collections with no vocabularies
  7. Returns collections sorted by UpdatedAt desc (nulls last), then by Id
- **Update** `Wordfolio.Api.DataAccess.Tests.fsproj` line 34: replace compile include

#### Domain Tests
- **Delete** `Wordfolio.Api/Wordfolio.Api.Domain.Tests/CollectionsHierarchy/SearchUserCollectionsTests.fs`
- **Create** `CollectionsHierarchy/GetCollectionsWithVocabularyCountTests.fs` with:
  - `TestEnv` implementing `IGetCollectionsWithVocabularyCount` + `ITransactional<TestEnv>`
  - Test: returns collections from capability, asserts calls
- **Update** `Wordfolio.Api.Domain.Tests.fsproj` line 34: replace compile include

#### API Integration Tests
- **Rewrite** `Wordfolio.Api/Wordfolio.Api.Tests/CollectionsHierarchy/GetCollectionsListTests.fs`:
  - Remove `CollectionsQueryOptions` record and `CollectionsUrlHelpers` module
  - URL becomes `Urls.CollectionsHierarchy.collections()` (no query string)
  - Keep existing test scenarios but remove search/sort assertions

## Verification

```bash
dotnet build                      # Verify compilation
dotnet test                       # Run all tests
dotnet fantomas .                 # Format F# code
```
