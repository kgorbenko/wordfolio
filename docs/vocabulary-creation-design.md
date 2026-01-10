# Vocabulary Creation Workflow Design

## Goal
Design a quick, non-intrusive workflow for users to capture words/phrases they encounter while reading or watching content, with instant access to definitions.

## Core Requirements
- Minimal distraction from primary activity (reading/watching)
- Fast word/phrase entry
- Quick definition selection
- Support for multiple definitions per word
- Support for multiple translations per word (different learning approach)
- Ability to select specific definitions/translations or accept all
- Definitions for advanced learners (monolingual)
- Translations for beginners/intermediate learners (bilingual)

## Design Questions to Explore
1. Entry method preferences
2. Definition source and presentation
3. Selection workflow
4. Context preservation
5. Mobile vs desktop considerations

## Workflow Options
(To be populated based on user feedback)

## Key Design Decisions

### Platform
- **Web app first** (desktop + mobile responsive)
- Mobile native app as future consideration

### Entry Method
- **Floating action button (FAB)** - always visible, quick access
- Opens lightweight modal/overlay for word entry

### Context/Examples Structure
- **Multiple examples per definition** - not a single context field for the word
- Auto-generate one example sentence per definition/translation
- Users can select/deselect generated examples via checkbox
- Users can add custom examples (multiple per definition)
- Supports future exercise generation based on examples
- **Example limit: 3-5 total per definition** (1 auto + 2-4 custom)

### Examples UI Layout (Same for Both Sections)
- **Mobile:** Auto-generated examples shown with grayed-out styling, no indentation
- **Desktop:** Side-by-side layout (definition/translation | example)
- Auto-generated examples pre-selected by default
- When definition/translation is deselected, its examples are auto-deselected and disabled
- "+ Add Example" button per definition/translation, expands inline text field
- **Auto-generated examples are read-only** - cannot be edited
- **Example length limit: ~200 characters** (1-2 sentences)
- Future enhancement: Highlight target word in examples (bold/colored)

### Section Visual Design
- **Section headers with icons:** "📖 Definitions" and "🌐 Translations"
- **Selection count badges:** "(3/5 selected)" in each header
- **Auto-collapse empty:** If section has no results, collapse with "+ Add" button
- **Per-section controls:** Each section has own "Select All" / "Deselect All"

### Source Tracking
- **No source tracking at example level** - handled via Collection + Vocabulary hierarchy
- Example: Collection "Books" → Vocabulary "Catcher in the Rye" provides context
- Collection and Vocabulary names/descriptions serve as source metadata

### Two-Section Structure: Definitions + Translations

**Why Two Sections:**
- **Definitions (Monolingual):** Advanced learners learn in target language (English)
- **Translations (Bilingual):** Beginner/intermediate learners get native language help (Russian)

**MVP Language Pair:**
- User enters English word → gets English definitions + Russian translations
- Future: User language preference setting for other language pairs

**Section Behavior:**
- Both sections shown by default
- Both sections behave identically (definitions/translations + examples)
- All items pre-selected by default in both sections
- Auto-collapse empty sections (if API returns no translations, collapse that section)
- User must save at least 1 item total (from either section or both)

### Definition/Translation Selection
- **All items pre-selected by default** - one-click save
- Checkboxes allow deselection of unwanted items
- **Per-section buttons:** "Select All Definitions" / "Select All Translations"
- **Selection count in headers:** "Definitions (3/5 selected)", "Translations (2/2 selected)"

### Data Fetching
- **Auto-fetch as user types** - debounced search (e.g., 300-500ms delay)
- **Single API call** - returns both definitions AND translations
- Instant visual feedback, no extra "Search" button needed
- Loading indicators for both sections during fetch

### No Results Handling
- **Manual entry for both sections** - user can add custom definitions/translations
- If no definitions found: collapse section, show "+ Add Definition" button
- If no translations found: collapse section, show "+ Add Translation" button
- Supports slang, proper nouns, technical terms, etc.

### Inline Editing
- **Definitions:** Click on definition text to edit/simplify
- **Translations:** Click on translation text to edit/refine
- **Examples:** Custom examples editable, auto-generated read-only
- Useful for personalizing or shortening lengthy content

### Workflow Speed
- **Target: 2 steps total**
  1. Enter word (auto-fetches definitions)
  2. Save (all definitions pre-selected)

## UX Design Details

### Modal Presentation
- **Bottom sheet** - slides up from bottom of screen
- Mobile-friendly, easy thumb reach
- Works well on desktop too

### Input Field Features
- **Auto-focus** - cursor ready immediately when modal opens
- **Clear button** - quick 'X' to reset input
- **Paste detection** - detects when user pastes text
- Future enhancement: Auto-extract word from pasted sentence

### Loading State
- **Skeleton loaders** - show placeholder definition cards during fetch
- Smooth transition when real definitions appear
- Better perceived performance than spinner

### Post-Save Behavior
- **Close modal + success toast** - "Added to vocabulary"
- Returns user to their content immediately
- Future enhancement: "Add Another Word" button for batch adding

### Keyboard Shortcuts
- **Enter to save** - quick save from word input field
- **Escape to close** - dismiss modal
- Future enhancement: Tab navigation for checkboxes (accessibility)

### Duplicate Handling
- **Warn on duplicates** - if word exists in current vocabulary, show warning
- Allow proceeding (word can exist in multiple vocabularies)
- Offer to view existing entry

### Collection/Vocabulary Selection
- **Context-aware** - user is already viewing a Vocabulary when adding words
- Words are added to currently active Vocabulary
- If no vocabularies exist, save to default "Unsorted" or "My Words" vocabulary
- Same word can exist in multiple vocabularies with different examples/context

## Final Recommended Workflow

### Step-by-Step User Journey

**Context:** User is reading a book/watching a movie and encounters an unfamiliar word.

#### 1. Initiate Word Entry (2 seconds)
- User clicks **floating action button (FAB)** on screen
- **Bottom sheet modal** slides up from bottom
- **Word input field** auto-focuses with cursor ready
- Features: Clear button (X), paste detection

#### 2. Enter Word (3-5 seconds)
- User types or pastes word/phrase
- **Debounced auto-search** triggers after 300-500ms pause
- **Skeleton loaders** appear immediately, showing placeholder definition cards

#### 3. Review Definitions & Translations (5-10 seconds)
- Both sections load and appear with smooth transition
- **All definitions + translations + examples pre-selected** by default
- **Layout:**
  - **Mobile:** Stacked sections, items with examples below (grayed out styling)
  - **Desktop:** Side-by-side within each item (definition/translation | example)

**Section 1: 📖 Definitions (3/5 selected)**
- Each definition shows:
  - ☑ Definition text (inline editable)
  - ☑ Auto-generated example (~200 chars, read-only, grayed out)
  - "+ Add Example" button (expands inline text field)
- Section controls: "Select All Definitions" / "Deselect All"

**Section 2: 🌐 Translations (2/2 selected)**
- Each translation shows:
  - ☑ Translation text in Russian (inline editable)
  - ☑ Auto-generated example in Russian (~200 chars, read-only, grayed out)
  - "+ Add Example" button (expands inline text field)
- Section controls: "Select All Translations" / "Deselect All"

**User Actions:**
- Deselect unwanted definitions/translations (auto-deselects their examples)
- Edit definition/translation text inline (click to edit)
- Add custom examples (up to 4 per definition/translation, 200 char limit)
- Collapse/expand sections if needed (empty sections auto-collapse)

#### 4. Handle Edge Cases (as needed)
- **No definitions found:** Section collapses, "+ Add Definition" button shown
- **No translations found:** Section collapses, "+ Add Translation" button shown
- **Duplicate detected:** Warning shown, option to view existing or proceed
- **No vocabularies exist:** Word saved to default "Unsorted" vocabulary

#### 5. Save (1 second)
- User clicks **Save** button (or presses Enter)
- Modal closes with smooth animation
- **Success toast** appears: "Added to vocabulary"
- User returns to their book/movie

### Total Time: 10-15 seconds for typical workflow

### Workflow Variations

**Fastest path (power users):**
1. Click FAB → Type word → Press Enter
2. All defaults accepted (all definitions + translations + examples)
3. ~5 seconds total

**Careful curation:**
1. Click FAB → Type word → Review
2. Deselect unwanted definitions/translations
3. Add 2-3 custom examples per item
4. Edit definitions/translations for clarity → Save
5. ~30-45 seconds total

**Definitions-only path (advanced learners):**
1. Click FAB → Type word → Review
2. "Deselect All Translations" (keep only definitions)
3. Save
4. ~10 seconds total

**Translations-only path (beginners):**
1. Click FAB → Type word → Review
2. "Deselect All Definitions" (keep only translations)
3. Save
4. ~10 seconds total

## Future Enhancements (Not MVP)

1. **Multi-language support** - user language preference setting (Russian, Spanish, German, etc.)
2. **Auto-extract word from pasted sentence** - paste full sentence, app extracts target word
3. **"Add Another Word" button** - batch add multiple words without closing modal
4. **Tab navigation for accessibility** - keyboard-only navigation through checkboxes
5. **Highlight target word in examples** - bold/colored emphasis of the word in context
6. **Voice input** - speak the word instead of typing
7. **Recent words dropdown** - quick access to recently searched words
8. **Smart example selection** - if >5 definitions/translations, auto-deselect some examples
9. **Offline mode** - cache recent LLM results for offline word addition
10. **Section persistence** - remember user's preference (definitions-only vs both)

## Data Model Implications

### Database Schema (conceptual)

```
Word/Phrase Entry
├─ word_text (string)
├─ vocabulary_id (FK)
├─ created_at (timestamp)
├─ definitions (array)
│  ├─ Definition 1
│  │  ├─ definition_text (string, editable)
│  │  ├─ source ('llm' | 'manual')
│  │  └─ examples (array, limit 5)
│  │     ├─ Example 1 (auto-generated, read-only)
│  │     ├─ Example 2 (custom)
│  │     └─ Example 3 (custom)
│  └─ Definition 2
│     └─ ...
└─ translations (array)
   ├─ Translation 1
   │  ├─ translation_text (string, Russian, editable)
   │  ├─ source ('llm' | 'manual')
   │  ├─ target_language ('ru' for MVP)
   │  └─ examples (array, limit 5)
   │     ├─ Example 1 (auto-generated Russian, read-only)
   │     ├─ Example 2 (custom Russian)
   │     └─ Example 3 (custom Russian)
   └─ Translation 2
      └─ ...
```

### Key Attributes
- **Definition/Translation.source:** 'llm' | 'manual' (track if LLM-generated or user-created)
- **Translation.target_language:** 'ru' for MVP (future: user preference)
- **Example.source:** 'llm' | 'custom' (track generation method)
- **Example.is_editable:** false for auto-generated, true for custom
- **Example.max_length:** 200 characters
- **Examples_per_item.limit:** 5 total (1 auto + 4 custom)
- **Minimum_to_save:** At least 1 definition OR 1 translation (or both)

## LLM Integration

### LLM-Powered Word Lookup
Instead of traditional dictionary/translation APIs, we use an LLM (Large Language Model) to generate definitions and translations. This approach provides:

1. **Unified response** - single LLM call returns both definitions AND translations with examples
2. **Context-aware definitions** - LLM understands word usage nuances better than static dictionaries
3. **Natural example sentences** - high-quality, contextual examples generated on-the-fly
4. **Flexible output** - handles slang, idioms, and phrases that traditional APIs may miss
5. **Consistent format** - structured JSON output for reliable parsing

### Implementation Details

**Streaming Response:**
- Uses Server-Sent Events (SSE) for real-time streaming
- Two-part output: human-readable text first, then structured JSON
- User sees definitions appearing progressively (better perceived performance)

**Prompt Structure:**
- Input: English word or phrase
- Output Part 1: Formatted text with definitions and Russian translations
- Output Part 2: JSON with structured data for saving

**JSON Schema:**
```json
{
  "definitions": [
    {
      "definition": "...",
      "partOfSpeech": "verb|noun|adj|adv|null",
      "exampleSentences": ["..."]
    }
  ],
  "translations": [
    {
      "translation": "...",
      "partOfSpeech": "verb|noun|adj|adv|null",
      "examples": [
        { "russian": "...", "english": "..." }
      ]
    }
  ]
}
```

**LLM Parameters:**
- Temperature: 0.1 (low for consistent, factual output)
- Max tokens: 4096 (sufficient for comprehensive definitions)

### Advantages Over Traditional APIs

| Aspect | Traditional APIs | LLM Approach |
|--------|------------------|---------------|
| Coverage | Limited to dictionary entries | Handles any word/phrase |
| Examples | Often missing or limited | Always generates relevant examples |
| Translations | Separate API needed | Included in same request |
| Phrases/Idioms | Poor support | Excellent understanding |
| Context | Static definitions | Contextually appropriate |
| Maintenance | Multiple API integrations | Single LLM integration |

### Fallback Strategy
- If LLM fails or times out: allow manual entry for both sections
- If word not recognized: LLM will indicate this, user can add manually
- Network errors: show error state with retry option

## Technical Considerations (High-Level)

### Performance Targets
- **Modal open time:** <100ms
- **LLM first token:** <500ms (streaming starts quickly)
- **LLM full response:** 2-4s (acceptable with streaming UX)
- **Debounce delay:** 500ms (balance responsiveness vs LLM calls)
- **Animation duration:** 200-300ms (smooth but fast)

### Mobile Optimizations
- **Touch targets:** Min 44x44px for buttons/checkboxes
- **Thumb reach:** Bottom sheet keeps controls in easy reach
- **Keyboard handling:** Auto-show/hide keyboard, scroll to keep input visible
- **Network awareness:** Show clear loading states, handle slow connections

### Desktop Enhancements
- **Keyboard shortcuts:** Enter, Escape, Tab navigation
- **Side-by-side layout:** Better use of horizontal space
- **Hover states:** Clear interactive feedback
- **Larger content area:** Show more definitions/examples at once

## Success Metrics

### Speed (Primary Goal)
- Average time to add word: <15 seconds
- Bounce rate on modal: <5% (users complete the action)

### Engagement
- **Section usage:** % using definitions-only vs translations-only vs both
- **Items per word:** Average number of definitions + translations saved per word
- **Custom examples:** % of entries with user-added examples
- **Duplicate warnings:** How often users proceed vs cancel
- **Section collapse:** How often users manually collapse sections

### Quality
- **Words with examples:** % of entries that include at least 1 example
- **Example length distribution:** Are users hitting the 200 char limit?
- **Edit frequency:** How often users edit auto-generated definitions/translations
- **Manual additions:** % of definitions/translations added manually (not from API)

### Learning Path Insights
- **Beginner behavior:** Do users who use translations-only add more custom examples?
- **Advanced learner behavior:** Do definitions-only users edit more frequently?
- **Mixed approach:** What % of users keep both sections enabled?

## Design Mockup Notes (for UI designer)

### Mobile Layout
```
┌──────────────────────────────────────┐
│  [Word input field]           [X]    │
│  ↑ Auto-focus, paste                 │
├──────────────────────────────────────┤
│ 📖 Definitions (3/5 selected)        │
│ [Select All] [Deselect All]          │
├──────────────────────────────────────┤
│ ☑ Definition 1                       │
│   Editable text...                   │
│   ☑ Auto example (gray)              │
│   [+ Add Example]                    │
├──────────────────────────────────────┤
│ ☑ Definition 2                       │
│   ...                                │
├──────────────────────────────────────┤
│ 🌐 Translations (2/2 selected)       │
│ [Select All] [Deselect All]          │
├──────────────────────────────────────┤
│ ☑ Translation 1 (Russian)            │
│   Editable text...                   │
│   ☑ Auto example RU (gray)           │
│   [+ Add Example]                    │
├──────────────────────────────────────┤
│ ☑ Translation 2 (Russian)            │
│   ...                                │
├──────────────────────────────────────┤
│              [Save (5 items)]        │
└──────────────────────────────────────┘
```

### Desktop Layout
```
┌───────────────────────────────────────────────────────┐
│  [Word input field]                            [X]    │
├───────────────────────────────────────────────────────┤
│ 📖 Definitions (3/5 selected)                         │
│ [Select All Definitions] [Deselect All]               │
├───────────────────────────────────────────────────────┤
│ ☑ Definition 1         │ ☑ Auto example              │
│   Editable text...     │   Read-only (gray)          │
│   [+ Add Example]      │ ☐ Custom example            │
│                        │   [editable field]           │
├───────────────────────────────────────────────────────┤
│ ☑ Definition 2         │ ☑ Auto example              │
│   ...                  │   ...                        │
├───────────────────────────────────────────────────────┤
│ 🌐 Translations (2/2 selected)                        │
│ [Select All Translations] [Deselect All]              │
├───────────────────────────────────────────────────────┤
│ ☑ Translation 1 (RU)   │ ☑ Auto example (RU)         │
│   Editable text...     │   Read-only (gray)          │
│   [+ Add Example]      │ ☐ Custom example (RU)       │
│                        │   [editable field]           │
├───────────────────────────────────────────────────────┤
│                                    [Save (5 items)]   │
└───────────────────────────────────────────────────────┘
```

### States to Design
1. **Empty state** - input field ready, no content
2. **Loading state** - skeleton loaders for both sections
3. **Loaded state** - definitions + translations + examples visible
4. **Partial results** - definitions loaded, translations section collapsed (or vice versa)
5. **No results state** - both sections collapsed, manual entry mode
6. **Duplicate warning** - overlay with options
7. **Success state** - toast notification with count

## Conclusion

This workflow is optimized for **speed and minimal distraction** while maintaining **flexibility** for users who want to curate their vocabulary carefully. The key innovations are:

1. **Two-section structure** - supports both monolingual (definitions) and bilingual (translations) learning
2. **Auto-fetch + pre-selection** - 90% of users can save with one click
3. **Examples per definition/translation** - enables future exercise generation
4. **Inline editing** - customize without extra screens
5. **Context-aware saving** - no dropdowns, word goes to current vocabulary
6. **Progressive disclosure** - advanced features available but not required
7. **Flexible learning paths** - users can use definitions-only, translations-only, or both

The design balances the needs of:
- **Beginners** - translations + examples in native language (Russian)
- **Advanced learners** - definitions-only for immersive learning
- **Intermediate learners** - both sections for comprehensive understanding
- **Casual users** - fast, one-click save with all defaults
- **Power users** - full control over definitions/translations/examples
- **Future features** - foundation for exercises, spaced repetition, multi-language support

### Key Success Factors
- **Speed:** 5-15 seconds per word keeps users engaged
- **Flexibility:** Both learning approaches in one workflow
- **Scalability:** Easy to add more languages in future
- **Data quality:** Examples enable rich exercise generation
- **User choice:** Everything is optional except saving at least 1 item
