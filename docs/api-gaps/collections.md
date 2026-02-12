# Collections: API Gaps & Inconsistencies

## Data Gaps

### 1. Vocabulary count hardcoded to 0 on collection detail

`mapCollectionDetail` in `features/collections/api/mappers.ts` hardcodes `vocabularyCount: 0` because `GET /api/collections/{id}` does not return a vocabulary count. The hierarchy endpoint provides this via the `vocabularies` array, but the detail endpoint does not.

- **Backend**: `GET /api/collections/{id}` returns `CollectionResponse` without `vocabularyCount`
- **Frontend**: `mapCollectionDetail` sets `vocabularyCount: 0`
- **Impact**: The `Collection` type carries a `vocabularyCount` field that is always 0 when fetched from the detail endpoint. Not currently displayed on the detail page, but is a data integrity issue in the type.

### 2. `createdAt`/`updatedAt` never displayed

The API returns `createdAt` and `updatedAt` for collections, but neither the Collections List page nor the Collection Detail page displays these dates anywhere.

- **Backend**: Both `GET /api/collections` and `GET /api/collections/{id}` return timestamps
- **Frontend**: Timestamps are mapped into the `Collection` type but never rendered

## Sorting & Filtering

### 3. No sorting or filtering on collections list

Collections are displayed in whatever order the hierarchy endpoint returns them (`ORDER BY Id ASC`). There are no user-facing sort controls (e.g., by name, by date, by vocabulary count) and no search/filter.

### 4. Inconsistent sorting source for vocabulary list within collection detail

Vocabularies on Collection Detail come from `GET /api/collections/{id}/vocabularies`, not from hierarchy. This endpoint has no explicit `ORDER BY`, so ordering is undefined and can differ from `GET /api/collections-hierarchy` (which orders by `Id`).

- **Collection Detail**: `/api/collections/{id}/vocabularies` (no explicit sort)
- **Sidebar / Collections overview**: `/api/collections-hierarchy` (`ORDER BY c."Id", v."Id"`)
- **Impact**: Users may see a different vocabulary order between list/sidebar and collection detail.

## Inconsistencies

### 5. `CollectionCard` shows "No description" for empty descriptions

`CollectionCard` renders the text `"No description"` when a collection has no description. This differs from `VocabularyCard`, which simply hides the description section entirely.

### 6. Navigation patterns mixed across collection pages

- `CreateCollectionPage` uses hardcoded string paths (`to: "/collections"`) instead of the `collectionsPath()` helper
- `EditCollectionPage` uses `useParams` with a raw string route ID
- `CollectionDetailPage` uses typed `collectionDetailRouteApi.useParams()`

### 7. Delete confirmation message lacks consequence warning

Delete collection dialog says `Are you sure you want to delete "X"?` without mentioning that all vocabularies and entries within it will also be deleted (cascade). Compare with delete vocabulary which says `This will also delete all entries within it.`

### 8. Form button labels inconsistent with other features

Collection forms use `"Create Collection"` / `"Save Changes"`, while vocabulary forms use `"Create"` / `"Save"` and entry forms use `"Save"` / `"Save"`.
