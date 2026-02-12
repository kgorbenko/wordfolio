# Vocabularies: API Gaps & Inconsistencies

## Data Gaps

### 1. Entry count always 0 on collection detail vocabulary cards

`GET /api/collections/{id}/vocabularies` returns `VocabularyResponse` without `entryCount`. The frontend mapper in `features/collections/api/mappers.ts` hardcodes `entryCount: 0`. The `VocabularyCard` component displays this count, so every vocabulary always shows "0" on the collection detail page.

Meanwhile, `GET /api/collections-hierarchy` returns `VocabularySummaryResponse` which includes `EntryCount`. So the sidebar/collections list shows correct counts, but the collection detail page does not.

- **Backend**: `VocabularyResponse` lacks `entryCount`; `VocabularySummaryResponse` has it
- **Frontend**: `mapVocabularyResponse` hardcodes `entryCount: 0` with a TODO comment
- **Impact**: Users see "0 entries" for every vocabulary on the collection detail page

### 2. No entry count in vocabulary detail page header

The Vocabulary Detail page displays entries but never shows a total count in the header/title area. The data is available client-side (`entries.length`), but it is not rendered.

### 3. `createdAt`/`updatedAt` never displayed for vocabularies

The API returns timestamps for vocabularies, but neither the vocabulary cards nor the Vocabulary Detail page displays them.

- **Backend**: Both list and detail endpoints return `createdAt`/`updatedAt`
- **Frontend**: Timestamps are mapped but never rendered

## Sorting & Filtering

### 4. No sorting or filtering on entry list within vocabulary detail

Entries on the Vocabulary Detail page are rendered in whatever order the API returns. The backend `getEntriesByVocabularyIdAsync` has **no ORDER BY clause**, making the sort order undefined. Compare with the Drafts endpoint which explicitly sorts `ORDER BY CreatedAt DESC`.

## Inconsistencies

### 5. No success notification on vocabulary create

`CreateVocabularyPage` does not show a success notification on successful creation (only error notification). `CreateCollectionPage` and `CreateEntryPage` both show success notifications.

### 6. No success notification on vocabulary edit

`EditVocabularyPage` does not show a success notification on successful update. `EditCollectionPage` does show one.

### 7. `VocabularyCard` hides description when empty

`VocabularyCard` simply omits the description section when it's empty/null. This differs from `CollectionCard` which shows `"No description"` text.

### 8. Duplicate `Vocabulary` type definitions with different shapes

- `features/collections/types.ts` defines `Vocabulary` with `entryCount`
- `features/vocabularies/types.ts` defines `Vocabulary` without `entryCount`, plus a separate `VocabularyDetail` with `collectionName`

Two types with the same name but different shapes across feature modules.
