# Shared / Cross-Cutting: API Gaps & Inconsistencies

## Duplicate API Layers

### 1. Two parallel API client implementations

Two API layers exist side-by-side:

- **Legacy**: `src/api/vocabulariesApi.ts` with hooks in `src/queries/` and `src/mutations/`
- **Feature-based**: `src/features/*/api/` with hooks in `src/features/*/hooks/`

Both have their own `getAuthHeaders`, `ApiError` type, and methods for overlapping endpoints. For example, `useCreateCollectionMutation` exists in both `src/mutations/` and `src/features/collections/hooks/`.

### 2. Query key invalidation mismatch between API layers

- Feature-based `useCreateCollectionMutation` invalidates `["collections"]`
- Legacy `useCreateCollectionMutation` invalidates both `["collections"]` AND `["collections-hierarchy"]`
- Entry mutations invalidate `["collections-hierarchy"]`, but feature-layer collection mutations do not

This can cause stale data in the sidebar or move dialog after collection CRUD operations.

## No Pagination

### 3. All list endpoints return all records

No pagination support exists on any endpoint:

- `GET /collections` returns all collections
- `GET /collections-hierarchy` returns all collections with all vocabularies
- `GET /collections/{id}/vocabularies` returns all vocabularies in a collection
- `GET /vocabularies/{vocabularyId}/entries` returns all entries in a vocabulary
- `GET /drafts` returns all draft entries

## No Search

### 4. No search or filter support on any list endpoint

The only "search" functionality is the duplicate check during entry creation (exact text match within a vocabulary). No endpoint supports text search or filtering.
