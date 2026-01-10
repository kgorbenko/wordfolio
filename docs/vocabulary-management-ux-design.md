# Wordfolio UX Design Plan - Vocabulary Management

## Overview

Design a responsive vocabulary management UX for desktop and mobile browsers. The app manages **Collections → Vocabularies → Entries** hierarchy with a focus on the quick word-entry workflow (5-15 seconds).

## Key Decisions

1. **Default vocabulary:** A global "quick add" button creates entries in "Unsorted" vocabulary (auto-created if needed). The word entry form allows selecting a different vocabulary.
2. **Implementation priority:** Word entry first, then collection/vocabulary management
3. **Desktop sidebar:** Expandable tree (collections expand to show vocabularies inline)

---

## Navigation Architecture

### Route Structure

```
/collections                           # Collections list (home for authenticated users)
/collections/:collectionId             # Vocabularies within a collection
/collections/:collectionId/:vocabId    # Entries list + FAB for word entry
/entries/:entryId                      # Entry detail/edit view
/settings                              # User preferences
```

### Layout Shell

**Mobile (< 900px):** Bottom navigation bar with global "+" button
```
┌─────────────────────────────────────┐
│          Page Content               │
│                                     │
├─────────────────────────────────────┤
│ [📚]    [🔍]    [+]    [⚙️]         │
│ Home   Search   Add   Settings      │
└─────────────────────────────────────┘
```
- The **[+] Add** button is always visible and opens word entry
- Defaults to "Unsorted" vocabulary, with option to select another

**Desktop (≥ 900px):** Left sidebar with expandable tree + global FAB
```
┌────────────┬──────────────────────────────────┐
│ Wordfolio  │  Collections > Books > Catcher   │
├────────────┼──────────────────────────────────┤
│ ▼ Books    │                                  │
│   Catcher  │      Page Content                │
│   1984     │                                  │
│ ▶ Movies   │                                  │
│ ▶ Work     │                         [+ FAB]  │
│ ─────────  │                                  │
│ Settings   │                                  │
└────────────┴──────────────────────────────────┘
```
- Collections expand inline to show vocabularies (accordion)
- FAB visible globally, opens word entry with vocabulary selector

---

## Page Designs

### 1. Collections List (`/collections`)

- Grid of collection cards (name, description, vocabulary count)
- FAB to create new collection
- **Empty state:** "Create your first collection to organize words by topic, book, or course"

### 2. Collection Detail (`/collections/:collectionId`)

- Breadcrumb: Collections > [Name]
- Collection name/description (editable)
- Grid of vocabulary cards (name, entry count)
- FAB to create new vocabulary
- **Empty state:** "Add your first vocabulary - a book, movie, or any source of new words"

### 3. Vocabulary Detail (`/collections/:collectionId/:vocabId`) - PRIMARY VIEW

The main working screen where users spend most time.

- Breadcrumb: Collections > [Collection] > [Vocabulary]
- Scrollable entry list with search/filter
- **Persistent FAB** for adding new words
- Entry list item:
  ```
  ┌─────────────────────────────────────────┐
  │ "serendipity"                           │
  │ n. the occurrence of events by chance   │
  │ счастливая случайность                  │
  │                             2 days ago  │
  └─────────────────────────────────────────┘
  ```
- **Empty state:** Pulsing FAB with "Tap + to add your first word"

### 4. Word Entry Bottom Sheet (Modal)

Triggered by global FAB or bottom nav "+" button. Follows `docs/vocabulary-creation-design.md`:

- **Mobile:** Bottom sheet (~85% height)
- **Desktop:** Centered modal (max-width 600px)

**Header:**
- Vocabulary selector dropdown (defaults to "Unsorted" or current vocabulary if in context)
- Auto-creates "Unsorted" collection/vocabulary on first use

**Body:**
- Auto-focused word input with debounced LLM lookup (500ms)
- Two sections: Definitions + Translations (all pre-selected)
- Skeleton loaders during fetch
- Inline editing, "+ Add Example" buttons
- Save button with count: "Save (5 items)"

**Keyboard:** Enter=save, Escape=close

### 5. Entry Detail (`/entries/:entryId`)

- Full view of word with all definitions, translations, examples
- Edit mode toggle for modifications
- Delete entry option

---

## Mobile vs Desktop Differences

| Aspect | Mobile | Desktop |
|--------|--------|---------|
| Navigation | Bottom bar | Left sidebar |
| Lists | Single column, swipe actions | Multi-column grid, context menus |
| Word entry | Full-screen bottom sheet | Centered modal dialog |
| Entry detail | Full page | Optional split view (list + detail panel) |

---

## Component Structure

```
src/components/
├── layouts/
│   ├── AuthenticatedLayout.tsx    # Navigation shell
│   ├── MobileNavigation.tsx       # Bottom nav
│   └── DesktopSidebar.tsx         # Left sidebar
├── collections/
│   ├── CollectionCard.tsx
│   └── CollectionForm.tsx
├── vocabularies/
│   ├── VocabularyCard.tsx
│   └── VocabularyForm.tsx
├── entries/
│   ├── EntryListItem.tsx
│   └── EntryDetail.tsx
├── word-entry/
│   ├── WordEntrySheet.tsx         # Bottom sheet/modal container
│   ├── WordInput.tsx              # Auto-focused input
│   ├── DefinitionsSection.tsx     # With examples
│   ├── TranslationsSection.tsx    # With examples
│   └── WordEntrySkeleton.tsx
└── common/
    ├── EmptyState.tsx
    ├── FloatingActionButton.tsx
    └── ConfirmDialog.tsx
```

---

## Key User Flows

### Quick Word Entry (5-15 seconds)
```
Vocabulary Detail → Tap FAB → Type word → Auto-fetch →
All items pre-selected → Tap Save → Success toast → Back to list
```

### New User Onboarding
```
Login → Empty Collections → Create Collection →
Empty Collection → Create Vocabulary →
Empty Vocabulary (pulsing FAB) → Add first word
```

---

## Technical Considerations

- **Virtualized lists** for entries (performance with large vocabularies)
- **Optimistic updates** for mutations
- **Debounced search** (500ms for LLM lookup)
- **Skeleton loaders** for perceived performance
- **Touch targets** min 44x44px
- **Keyboard shortcuts** (Ctrl+N for new word, Escape to close)

---

## Implementation Phases

### Phase 1: Word Entry (Priority)
Focus on the core word capture workflow:

1. **Authenticated layout shell** with global FAB
2. **Word entry bottom sheet** with vocabulary selector
3. **Dictionary lookup API** integration (LLM-powered)
4. **Default vocabulary** auto-creation ("Unsorted")
5. **Entry list view** (basic, in vocabulary context)

### Phase 2: Navigation & Management
Add full collection/vocabulary management:

1. **Desktop sidebar** with expandable tree
2. **Mobile bottom navigation**
3. **Collections CRUD** (list, create, edit, delete)
4. **Vocabularies CRUD** (list, create, edit, delete)
5. **Entry detail/edit** page

### Phase 3: Polish
- Search functionality
- Entry editing with full definition/translation management
- Settings page
- Keyboard shortcuts
- Performance optimizations (virtualized lists)

---

## Files to Create/Modify

### Phase 1 Files
- `src/routes/_authenticated.tsx` - Protected layout with FAB
- `src/routes/_authenticated/index.tsx` - Redirect or basic home
- `src/components/word-entry/WordEntrySheet.tsx` - Bottom sheet/modal
- `src/components/word-entry/WordInput.tsx`
- `src/components/word-entry/DefinitionsSection.tsx`
- `src/components/word-entry/TranslationsSection.tsx`
- `src/api/dictionaryApi.ts` - LLM lookup
- `src/api/entriesApi.ts` - Entry CRUD
- `src/api/vocabulariesApi.ts` - For vocabulary selector
- `src/mutations/useCreateEntry.ts`
- `src/queries/useVocabularies.ts`

### Phase 2 Files
- `src/routes/_authenticated/collections/` - Collection routes
- `src/components/layouts/MobileNavigation.tsx`
- `src/components/layouts/DesktopSidebar.tsx`
- `src/components/collections/CollectionCard.tsx`
- `src/components/vocabularies/VocabularyCard.tsx`
- Full CRUD queries/mutations for collections/vocabularies

### State Management
- `src/stores/uiStore.ts` - UI preferences (sidebar state)
- `src/stores/wordEntryStore.ts` - Temporary word entry state

---

## Verification

1. **Navigation:** Verify routing works on mobile and desktop
2. **Responsiveness:** Test at 375px (mobile), 768px (tablet), 1280px (desktop)
3. **Word entry workflow:** Complete flow in under 15 seconds
4. **Empty states:** Verify all empty states render correctly
5. **CRUD operations:** Test create/read/update/delete for all entities
