# Endpoint Integration Plan

## Implementation Outline

- API client updates
  - Add missing CRUD methods for collections and vocabularies in `src/api/vocabulariesApi.ts`
  - Add update entry method in `src/api/entriesApi.ts`
  - Remove N+1 helpers (`getAllVocabularies`, `getOrCreateDefaultVocabulary`) and rely on collections hierarchy

- Query hooks (TanStack Query)
  - `useCollectionsQuery` (GET /api/collections)
  - `useCollectionQuery` (GET /api/collections/{id})
  - `useVocabulariesQuery` (GET /api/collections/{collectionId}/vocabularies)
  - `useVocabularyQuery` (GET /api/collections/{collectionId}/vocabularies/{id})
  - `useEntriesQuery` (GET /api/vocabularies/{vocabularyId}/entries)
  - `useEntryQuery` (GET /api/entries/{id})

- Mutation hooks (TanStack Query)
  - `useCreateCollectionMutation`, `useUpdateCollectionMutation`, `useDeleteCollectionMutation`
  - `useCreateVocabularyMutation`, `useUpdateVocabularyMutation`, `useDeleteVocabularyMutation`
  - `useCreateEntryMutation`, `useUpdateEntryMutation`, `useDeleteEntryMutation`
  - Invalidate collections, vocabularies, entries, and collections-hierarchy as needed per mutation

- Component wiring and stub removal
  - `WordEntrySheet.tsx`: use collections hierarchy for vocab selector; create entry mutation
  - `dashboard.tsx`: use collections hierarchy + entries query
  - `collections/index.tsx`: collections queries and mutations
  - `collections/$collectionId/index.tsx`: collection + vocabularies queries and vocabulary mutations
  - `collections/$collectionId/$vocabularyId.tsx`: collection/vocabulary + entries queries
  - `entries/$entryId.tsx`: entry query + update/delete mutations

## TODO Task List

1. [x] Integrate Collections List (GET /api/collections)
   - Add query hook
   - Replace stub data in collections list route
   - Ensure empty state and loading state render correctly

2. [x] Integrate Collection CRUD (GET/POST/PUT/DELETE /api/collections)
   - Add API client methods for get/update/delete
   - Add mutation hooks for create/update/delete
   - Wire create/edit/delete dialogs in collections list route
   - Invalidate collections + collections-hierarchy queries

3. [x] Integrate Collection Detail (GET /api/collections/{id})
   - Add query hook
   - Replace stub collection data in collection detail route
   - Keep breadcrumb + header in sync with fetched data

4. [x] Integrate Vocabularies List (GET /api/collections/{collectionId}/vocabularies)
   - Add query hook
   - Replace stub vocabularies data in collection detail route
   - Verify empty state and counts

5. [x] Integrate Vocabulary CRUD (POST/PUT/DELETE /api/collections/{collectionId}/vocabularies)
   - Add API client methods for get/update/delete
   - Add mutation hooks for create/update/delete
   - Wire create/edit/delete dialogs in collection detail route
   - Invalidate vocabularies + collections-hierarchy queries

6. [x] Integrate Vocabulary Detail (GET /api/collections/{collectionId}/vocabularies/{id})
   - Add query hook
   - Replace stub vocabulary data in vocabulary detail route
   - Keep breadcrumb + header in sync with fetched data

7. [x] Integrate Entries List (GET /api/vocabularies/{vocabularyId}/entries)
   - Add query hook
   - Replace stub entries data in vocabulary detail route
   - Ensure search/filter uses fetched data

8. [x] Integrate Entry Create (POST /api/entries)
   - Add create entry mutation hook
   - Replace manual entry creation in WordEntrySheet
   - Use collections-hierarchy query to populate vocabulary selector
   - Invalidate entries + collections-hierarchy queries on success

9. [x] Integrate Entry Detail (GET /api/entries/{id})
   - Add query hook
   - Replace stub entry data in entry detail route

10. [x] Integrate Entry Update (PUT /api/entries/{id})
    - Add update entry API client + mutation hook
    - Wire edit/save flow in entry edit route (separate navigation route)
    - Invalidate entry + entries + collections-hierarchy queries

11. [x] Integrate Entry Delete (DELETE /api/entries/{id})
    - Add delete entry mutation hook
    - Wire delete dialog in entry detail route
    - Invalidate entries + collections-hierarchy queries

12. [ ] Integrate Dashboard (GET /api/collections-hierarchy + entries)
    - Replace manual data loading with queries
    - Use default vocabulary from collections-hierarchy
    - Ensure recent entries list uses entries query

13. [ ] Cleanup and verification
    - Remove getAllVocabularies and getOrCreateDefaultVocabulary from vocabulariesApi
    - Remove remaining stub data from routes
    - Run frontend format/lint/build/tests
