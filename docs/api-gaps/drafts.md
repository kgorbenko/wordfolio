# Drafts: API Gaps & Inconsistencies

## Inconsistencies

### 1. Two different empty states with different text

Two distinct empty state paths exist:

- `DraftsEmptyState` component: shown when `data` is `null` (no default vocabulary exists). Text: *"Your drafts will appear here when you add your first word. Tap the + button to get started."*
- `DraftsContent` inline empty state: shown when `data.entries` is empty (default vocabulary exists but has no entries). Text: *"Tap the + button to add your first word to drafts."*

Both represent "no drafts yet" to the user but use different descriptions.

### 2. No success notification on draft entry edit

`DraftsEntryEditPage` does not show a success notification on successful update. `CreateDraftPage` does show one.

## Notes

### Drafts vs Vocabulary entries data asymmetry

The Drafts endpoint (`GET /drafts`) returns entries WITH full hierarchy (definitions, translations, examples), while the regular entry list endpoint (`GET /vocabularies/{vid}/entries`) returns entries WITHOUT hierarchy. This means `EntryListItem` shows definition/translation previews on the Drafts page but not on the Vocabulary Detail page. See `entries.md` gap #1 for details.
