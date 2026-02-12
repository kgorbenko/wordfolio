# Entries: API Gaps & Inconsistencies

## Data Gaps

### 1. Entry list endpoint returns entries WITHOUT definitions/translations

`GET /vocabularies/{vocabularyId}/entries` returns entries with empty definition and translation arrays. The AppEnv maps entries with `toEntryDomain(e, [], [])`.

The frontend `EntryListItem` component renders first definition and first translation previews, but these are always empty when sourced from this endpoint. Only the Drafts endpoint (`GET /drafts`) returns the full hierarchy.

- **Vocabulary Detail Page**: entry list items show no definition/translation previews
- **Drafts Page**: entry list items correctly show definition/translation previews
- **Impact**: Users on the vocabulary page see entries with no preview of their content

### 2. Definition/translation `source` (API vs Manual) not displayed

Each definition and translation carries a `source` field (`"Api"` or `"Manual"`) indicating AI-generated vs user-created. This is available in the data model and returned by the API, but never shown in the Entry Detail page UI (`EntryDetailContent`, `AnnotatedItemCard`).

### 3. `updatedAt` not shown in entry list items

`EntryListItem` receives and displays `createdAt` as a relative time chip, but `updatedAt` is available on each entry and is neither passed to nor displayed by `EntryListItem`.

## Sorting & Filtering

### 4. Undefined sort order on entry list endpoint

The data access function `getEntriesByVocabularyIdAsync` has no `ORDER BY` clause. The sort order of entries on the Vocabulary Detail page is undefined. Compare with the Drafts endpoint which explicitly uses `ORDER BY CreatedAt DESC`.

## Inconsistencies

### 5. No success notification on entry edit

`EditEntryPage` does not show a success notification on successful update. `CreateEntryPage` does show one.

### 6. Delete confirmation message differs from other entities

- Delete collection: `Are you sure you want to delete "X"?`
- Delete vocabulary: `...delete "X"? This will also delete all entries within it.`
- Delete entry: `...delete "X"? This action cannot be undone.`

Only entry mentions irreversibility, only vocabulary mentions cascade, collection mentions neither.
