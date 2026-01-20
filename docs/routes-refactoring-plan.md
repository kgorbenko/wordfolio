# Routes Directory Refactoring Plan

## Executive Summary

The `src/routes/` directory has two fundamental problems:

1. **Architecture violation**: Route files contain full page implementations (up to 485 lines) instead of being thin configuration files. This violates CLAUDE.md principle.

2. **Massive code duplication**: Common UI patterns (containers, headers, breadcrumbs, loading states, error states) are duplicated 8-38+ times across route files.

**Goals:**
- Eliminate duplicated code through shared components
- Split massive components (200+ lines) into focused, single-responsibility pieces
- Establish clear component APIs: data in via props, events out via callbacks
- Ensure all orchestration (navigation, mutations, dialogs) happens at page level only

---

## 1. Principles for Refactoring

### P1: Route Files Are Configuration Only
Route files should contain **only**:
- Route path definition via `createFileRoute()`
- Search param validation schemas (Zod)
- `beforeLoad` guards for auth/redirects
- Component import and reference

Route files should **never** contain:
- React component definitions
- Data fetching hooks (useQuery, useMutation)
- Business logic or event handlers
- JSX rendering beyond the component reference

### P2: Eliminate Duplication via Shared Components
Extract repeated patterns into reusable components in `src/components/common/`:

| Pattern | Occurrences | Extract To |
|---------|-------------|------------|
| `<Container maxWidth={false} sx={{ py: 4 }}>` | 27+ | `<PageContainer>` |
| Page header Typography (h4, fontWeight 600) | 12+ | `<PageHeader>` |
| Breadcrumb navigation | 38+ | `<BreadcrumbNav>` |
| Loading skeletons | 8 files | `<PageSkeleton variant="cards|list|form">` |
| Error state with retry | 8 files | `<RetryOnError>` |
| Card grid layout | 4+ | `<CardGrid>` |

### P3: Clear Component API - Data In, Events Out
Components must have explicit contracts with `readonly` properties:

```tsx
// GOOD: Clear props API with callbacks and readonly types
interface CollectionCardProps {
  readonly collection: CollectionSummary;
  readonly onClick: () => void;        // Event callback - page handles navigation
  readonly onDelete: () => void;       // Event callback - page handles mutation
}

// BAD: Component with internal navigation/mutation
const CollectionCard = ({ collection }) => {
  const navigate = useNavigate();           // No internal navigation
  const deleteMutation = useDeleteMutation(); // No internal mutations
  // ...
}
```

**Rules:**
- Components receive **data** via props (never fetch their own data)
- Components emit **events** via callbacks (never navigate or mutate directly)
- Components may have **local UI state** only (open/closed, hover, focus)
- All interface properties must be `readonly`

### P4: Page-Level Orchestration Only
Pages are the **only** components that may:
- Call `useNavigate()` for navigation
- Call mutation hooks (`useCreateMutation`, `useDeleteMutation`, etc.)
- Call `useConfirmDialog()` for confirmations
- Call notification context (`openErrorNotification`, etc.)
- Coordinate multiple queries and their loading/error states
- Use `assertNonNullable` for strict null checks on data that must exist

**Current Violation:** `WordEntrySheet` calls `useCreateEntryMutation` internally. This must be refactored so the parent page/layout orchestrates the mutation.

### P5: Feature-Based Vertical Slices
Organize code by **feature** under `src/features/<feature>/`:
```
src/features/collections/
├── api/              # API clients + mappers
├── components/       # UI components + their SCSS modules
├── hooks/            # Query and mutation hooks
├── pages/            # Page components + their SCSS modules
├── schemas/          # Zod validation schemas
└── types.ts          # Client-side types
```

### P6: Respect 200-Line Threshold
Components exceeding 200 lines should be decomposed. Extract:
- Repeated sections (definitions list, translations list)
- Complex sub-forms (add definition dialog, add translation dialog)
- Reusable patterns (item editor, example list)

### P7: Colocation Over Global Directories
Feature-specific code (queries, mutations, schemas, components) should live within the feature directory, not in global directories.

### P8: Client-Side Types with Mapping Layer
Components should not depend directly on API response types. Instead:
- Define **client-side types** (interfaces) that components use
- Create a **mapping layer** to transform API types → client types
- Handle serialization artifacts in the mapping layer (e.g., `string` → `Date`)
- Mark unavailable fields with TODOs in mappers (e.g., `entryCount` in detail views)

```
API Response (string dates, nullable fields)
    ↓ mapping function
Client Type (Date objects, required fields with defaults)
    ↓
Component (receives clean, typed data)
```

**Benefits:**
- Components are decoupled from API changes
- Date parsing, null handling happen in one place
- Future OpenAPI client generation won't require component changes
- Components are easier to test with plain objects

**Example:**
```tsx
// API type (from server or generated client)
interface CollectionResponse {
  readonly id: number;
  readonly name: string;
  readonly createdAt: string;  // ISO string from API
}

// Client type (used by components)
interface Collection {
  readonly id: number;
  readonly name: string;
  readonly createdAt: Date;    // Proper Date object
}

// Mapping function (in features/collections/api/mappers.ts)
const mapCollection = (response: CollectionResponse): Collection => ({
  id: response.id,
  name: response.name,
  createdAt: new Date(response.createdAt),
});
```

### P9: SCSS Over Inline Styles
Strictly use SCSS modules for all styling and layout:
- **Use SCSS** for layout (margins, padding, flex/grid), spacing, typography, colors.
- **Avoid `sx`** props entirely unless absolutely necessary for dynamic values not achievable with CSS vars.
- **MUI Props**: Use MUI specific props (like `color="primary"`, `variant="contained"`) but control their positioning and sizing via classes.

**Rationale:**
- Separates styling concerns from component logic
- Makes JSX easier to scan and understand
- SCSS provides better tooling (autocomplete, linting)
- Keeps MUI-specific styling minimal and intentional

### P10: Mandatory Breadcrumbs
All pages must render breadcrumbs for consistent navigation:
- Breadcrumbs are rendered at **page level**, not in shared components
- Static parts render immediately (no loading state needed)
- Dynamic parts (e.g., collection name) show skeleton while loading

### P11: Page + Content Pattern with renderContent
Pages call queries and handle all async states, Content components render data only:

```
Page (orchestration layer)
├── useQuery() calls
├── useMutation() calls
├── renderContent() - switches on loading/error/data
├── BreadcrumbNav (can use query data for dynamic parts)
├── PageHeader (can use query data)
├── {renderContent()}
│   ├── loading → ContentSkeleton
│   ├── error → RetryOnError
│   └── data → Content component (data only)
```

**Responsibility split:**
- **Page**: Queries, mutations, loading/error handling, navigation, shell
- **Content**: Renders data only (no loading/error states)

**Why queries at Page level:**
- Page can access loading state for dynamic breadcrumbs (e.g., show "Loading..." or skeleton)
- Page controls all async state transitions in one place
- Content components are pure data renderers - easier to test
- Consistent pattern: all orchestration at page level

**renderContent pattern:**
```tsx
const renderContent = useCallback(() => {
  if (isLoading) return <ContentSkeleton variant="cards" />;
  if (isError || !data) return <RetryOnError title="..." onRetry={refetch} />;
  return <CollectionsContent collections={data} onCollectionClick={...} />;
}, [isLoading, isError, data, ...]);
```

### P12: UI Standards
- **Creation Actions**: Place "Create" buttons in the `PageHeader` actions area (not FAB) for list pages. Use `variant="contained"`.
- **Edit/Delete Actions**: Use `IconButton` with `color="primary"` for Edit and `color="error"` for Delete.
- **Page Layout**: Use `PageContainer` without explicit `maxWidth` for standard pages to ensure consistent fluid layout.

---

## 2. Issues Sorted by Severity

### Critical (Architecture Violations)

| Issue | Location | Lines | Description |
|-------|----------|-------|-------------|
| Page in route file | `collections/$collectionId/$vocabularyId/entries/$entryId/index.tsx` | 485 | Entry detail page with 3 queries, mutations, confirmation dialogs |
| Page in route file | `collections/$collectionId/$vocabularyId/index.tsx` | 304 | Vocabulary detail page embedded in route |
| Page in route file | `collections/$collectionId/index.tsx` | 301 | Collection detail page embedded in route |
| Page in route file | `collections/$collectionId/$vocabularyId/entries/$entryId/edit.tsx` | 263 | Entry edit page embedded in route |
| Page in route file | `collections/$collectionId/$vocabularyId/edit.tsx` | 217 | Vocabulary edit page embedded in route |
| Orchestration violation | `components/word-entry/WordEntrySheet.tsx` | 72-80 | Component calls `useCreateEntryMutation` internally instead of receiving callback |

### High (Code Duplication)

| Issue | Occurrences | Impact |
|-------|-------------|--------|
| Container pattern duplicated | 27+ times | `<Container maxWidth={false} sx={{ py: 4 }}>` repeated in every route |
| Breadcrumb pattern duplicated | 38+ times | Same breadcrumb structure with NavigateNextIcon, styling, hover effects |
| Page header duplicated | 12+ times | `<Typography variant="h4" fontWeight={600}>` with identical styling |
| Loading skeleton duplicated | 8 files | Same Skeleton patterns for cards/lists repeated |
| Error state duplicated | 8 files | Same RetryOnError + Container wrapper pattern |
| Grid layout duplicated | 4+ times | Same responsive grid template columns |

### High (Oversized Components)

| Issue | Location | Lines | Decomposition Needed |
|-------|----------|-------|---------------------|
| EntryForm | `components/entries/EntryForm.tsx` | 788 | Definitions/translations sections are duplicated code; 3 dialogs should be 1 generic |
| Sidebar | `components/layouts/Sidebar.tsx` | 565 | Extract CollectionTreeItem, VocabularyTreeItem |
| WordEntrySheet | `components/word-entry/WordEntrySheet.tsx` | 423 | Extract WordLookupInput, LookupResultsDisplay |

### Medium (Inconsistencies)

| Issue | Location | Description |
|-------|----------|-------------|
| Layout with mutations | `_authenticated.tsx` | Layout orchestrates WordEntrySheet mutations via uiStore |
| Page in route file | `collections/$collectionId/edit.tsx` | 179 lines - Edit collection page in route |
| Page in route file | `collections/$collectionId/vocabularies/new.tsx` | 178 lines - New vocabulary page in route |
| Page in route file | `collections/index.tsx` | 129 lines - Collections list page |
| Page in route file | `collections/new.tsx` | 78 lines - New collection page |
| Unused feature directory | `src/features/collections/` | Directory structure exists but is empty |
| Global queries | `src/queries/` | Feature queries should be colocated |
| Global mutations | `src/mutations/` | Feature mutations should be colocated |

### Low (Code Quality)

| Issue | Description |
|-------|-------------|
| Deep relative imports | `../../../../../../../contexts/` in nested routes |
| Inconsistent patterns | Auth pages in `src/pages/`, feature pages in routes |
| Multi-query aggregation | Repeated `isLoading = isLoading1 \|\| isLoading2` pattern in 4+ files |

---

## 3. Collections Feature Refactoring Example (Reference)

### 3.1 Target Directory Structure

```
src/
├── components/common/               # Shared components to eliminate duplication
│   ├── PageContainer.tsx            # Replaces 27+ Container patterns
│   ├── PageHeader.tsx               # Replaces 12+ Typography h4 patterns
│   ├── ContentSkeleton.tsx          # Content-only skeleton (no container)
│   ├── RetryOnError.tsx             # (existing, used directly)
│   ├── BreadcrumbNav.tsx            # Replaces 38+ breadcrumb patterns
│   ├── CardGrid.tsx                 # Replaces 4+ grid patterns
│   └── EmptyState.tsx               # (existing)
│
├── routes/_authenticated/collections/
│   ├── index.tsx                    # ~6 lines - route config only
│   ├── new.tsx                      # ~6 lines - route config only
│   └── $collectionId/
│       ├── index.tsx                # ~6 lines - route config only
│       └── edit.tsx                 # ~6 lines - route config only
│
└── features/collections/
    ├── api/
    │   ├── collectionsApi.ts        # HTTP client (API types)
    │   └── mappers.ts               # API → Client type transformations
    ├── types.ts                     # Client-side types (used by components)
    ├── components/
    │   ├── CollectionCard.tsx
    │   ├── CollectionCard.module.scss
    │   ├── CollectionForm.tsx
    │   ├── CollectionForm.module.scss
    │   ├── CollectionsContent.tsx        # Renders collection grid (data only)
    │   └── CollectionDetailContent.tsx   # Renders collection details (data only)
    ├── hooks/
    │   ├── useCollectionQuery.ts
    │   ├── useCollectionsQuery.ts
    │   ├── useCollectionsHierarchyQuery.ts
    │   ├── useCreateCollectionMutation.ts
    │   ├── useDeleteCollectionMutation.ts
    │   └── useUpdateCollectionMutation.ts
    ├── pages/
    │   ├── CollectionsPage.tsx           # Query + shell + orchestration
    │   ├── CollectionsPage.module.scss
    │   ├── CollectionDetailPage.tsx
    │   ├── CollectionDetailPage.module.scss
    │   ├── CreateCollectionPage.tsx
    │   └── EditCollectionPage.tsx
    └── schemas/
        └── collectionSchemas.ts
```

### 3.2 Shared Components to Create

#### PageContainer
```tsx
// src/components/common/PageContainer.tsx
import styles from "./PageContainer.module.scss";

interface PageContainerProps {
  readonly children: React.ReactNode;
  readonly maxWidth?: number | false;
}

export const PageContainer = ({ children, maxWidth }: PageContainerProps) => (
  <Container
    maxWidth={false}
    className={styles.container}
    sx={maxWidth ? { maxWidth } : undefined}
  >
    {children}
  </Container>
);
```

#### PageHeader
```tsx
// src/components/common/PageHeader.tsx
import styles from "./PageHeader.module.scss";

interface PageHeaderProps {
  readonly title: string;
  readonly description?: string;
  readonly actions?: React.ReactNode;
}

export const PageHeader = ({ title, description, actions }: PageHeaderProps) => (
  <Box className={styles.container}>
    <Box className={styles.topRow}>
      <Typography variant="h4" fontWeight={600} className={styles.title}>
        {title}
      </Typography>
      {actions}
    </Box>
    {description && (
      <Typography variant="body1" color="text.secondary" className={styles.description}>
        {description}
      </Typography>
    )}
  </Box>
);
```

#### BreadcrumbNav
Wrap in a `div` to ensure margins are applied correctly.
```tsx
// src/components/common/BreadcrumbNav.tsx
export const BreadcrumbNav = ({ items }: BreadcrumbNavProps) => (
  <div className={styles.breadcrumbs}>
    <Breadcrumbs ...>
      ...
    </Breadcrumbs>
  </div>
);
```

### 3.3 Route File (Configuration Only)

```tsx
// src/routes/_authenticated/collections/index.tsx (6 lines)
import { createFileRoute } from "@tanstack/react-router";
import { CollectionsPage } from "../../../features/collections/pages/CollectionsPage";

export const Route = createFileRoute("/_authenticated/collections/")({
  component: CollectionsPage,
});
```

### 3.4 Client-Side Types

```tsx
// src/features/collections/types.ts - Client-side types (used by components)
export interface Collection {
  readonly id: number;
  readonly name: string;
  readonly description: string | null;
  readonly vocabularyCount: number;
  readonly createdAt: Date;
  readonly updatedAt: Date | null;
}
```

### 3.5 Mapping Layer

```tsx
// src/features/collections/api/mappers.ts - Transforms API → Client types
import { CollectionResponse } from "./collectionsApi";
import { Collection } from "../types";

export const mapCollection = (response: CollectionResponse): Collection => ({
  id: response.id,
  name: response.name,
  description: response.description,
  vocabularyCount: response.vocabularies.length,
  createdAt: new Date(response.createdAt),
  updatedAt: response.updatedAt ? new Date(response.updatedAt) : null,
});

export const mapCollections = (responses: CollectionResponse[]): Collection[] =>
  responses.map(mapCollection);
```

### 3.6 Query Hook (Flexible Options)

```tsx
// src/features/collections/hooks/useCollectionsQuery.ts
import { useQuery, UseQueryOptions } from "@tanstack/react-query";
import { collectionsApi } from "../api/collectionsApi";
import { mapCollections } from "../api/mappers";
import { Collection } from "../types";

export const useCollectionsQuery = (options?: Partial<UseQueryOptions<Collection[]>>) =>
  useQuery({
    queryKey: ["collections"],
    queryFn: async () => {
      const response = await collectionsApi.getAll();
      return mapCollections(response);
    },
    ...options,
  });
```

### 3.7 Page Component (Query + Shell + Orchestration)

Page calls queries, handles loading/error states, renders shell and content:

```tsx
// src/features/collections/pages/CollectionsPage.tsx
import { useCallback } from "react";
import { useNavigate } from "@tanstack/react-router";
import { Button } from "@mui/material";
import AddIcon from "@mui/icons-material/Add";

import { PageContainer } from "../../../components/common/PageContainer";
import { PageHeader } from "../../../components/common/PageHeader";
import { BreadcrumbNav } from "../../../components/common/BreadcrumbNav";
import { ContentSkeleton } from "../../../components/common/ContentSkeleton";
import { RetryOnError } from "../../../components/common/RetryOnError";
import { CollectionsContent } from "../components/CollectionsContent";
import { useCollectionsQuery } from "../hooks/useCollectionsQuery";

export const CollectionsPage = () => {
  const navigate = useNavigate();

  // PAGE CALLS QUERY
  const { data: collections, isLoading, isError, refetch } = useCollectionsQuery();

  // PAGE ORCHESTRATES: navigation callbacks
  const handleCollectionClick = useCallback((id: number) => {
    void navigate({ to: "/collections/$collectionId", params: { collectionId: String(id) } });
  }, [navigate]);

  const handleCreateClick = useCallback(() => {
    void navigate({ to: "/collections/new" });
  }, [navigate]);

  // PAGE HANDLES: loading/error/data switching
  const renderContent = useCallback(() => {
    if (isLoading) return <ContentSkeleton variant="cards" />;
    
    if (isError || !collections) {
      return (
        <RetryOnError
          title="Failed to Load Collections"
          description="..."
          onRetry={() => void refetch()}
        />
      );
    }

    return (
      <CollectionsContent
        collections={collections}
        onCollectionClick={handleCollectionClick}
        onCreateClick={handleCreateClick}
      />
    );
  }, [isLoading, isError, collections, refetch, handleCollectionClick, handleCreateClick]);

  return (
    <PageContainer>
      <BreadcrumbNav items={[{ label: "Collections" }]} />
      <PageHeader
        title="Collections"
        actions={
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={handleCreateClick}
          >
            Create Collection
          </Button>
        }
      />

      {renderContent()}
    </PageContainer>
  );
};
```

---

## 4. Implementation Order

### Phase 1: Create Shared Components (Foundation)
**Completed.**
- `PageContainer` (SCSS + standard layout)
- `PageHeader` (SCSS + description support)
- `BreadcrumbNav` (SCSS + wrapper div)
- `ContentSkeleton`
- `RetryOnError` (replaces ContentError)
- `CardGrid`

### Phase 2: Collections Feature (Reference Implementation)
**Completed.**
- Structure established in `src/features/collections/`
- Strict separation of Page (orchestration) and Content (display)
- SCSS modules for all styling
- Readonly interfaces
- UI standards applied (Header actions, colored buttons)

### Phase 3: Vocabularies Feature
**Completed.**
- Created `src/features/vocabularies/` structure.
- Defined readonly `types.ts` and mappers.
- Migrated API and Hooks.
- Created strict SCSS-styled components.
- Refactored pages to use `PageContainer`, `PageHeader`, `BreadcrumbNav`, and `RetryOnError`.
- Ensured "Create" buttons are in Header.

### Phase 4: Entries Feature
Apply same pattern to entries routes and related code.
**Special attention:** Decompose `EntryForm.tsx` (788 lines):
- Extract `DefinitionItemEditor` / `TranslationItemEditor` (eliminate duplication)
- Extract generic `AddItemDialog` (replace 3 similar dialogs)
- Extract `ExamplesList` component
- Ensure strict null safety with `assertNonNullable` in delete/update handlers.

### Phase 5: WordEntrySheet Refactoring
Fix orchestration violation:
1. Move `useCreateEntryMutation` call to `_authenticated.tsx` layout
2. Pass `onSave` callback to `WordEntrySheet`
3. Extract `WordLookupInput` and `LookupResultsDisplay` sub-components

### Phase 6: Cleanup
1. Remove empty global directories (`src/queries/`, `src/mutations/`) after migration
2. Update all imports
3. Run linter and formatter

---

## 5. Verification

After each phase:
1. `npm run build` - should compile without errors
2. `npm test` - all tests should pass
3. `npm run lint` - no warnings
4. `npm run format` - code formatted

Final architecture checks:
- [ ] All route files under 15 lines (config only)
- [ ] No page components defined in `src/routes/`
- [ ] All interface properties are `readonly`
- [ ] All custom styling uses SCSS modules (no inline `sx` for layout)
- [ ] All pages have mandatory breadcrumbs
- [ ] All pages use renderContent pattern (queries at page level)
- [ ] Content components render data only (no loading/error states)
- [ ] No `useNavigate()` or `useMutation` calls in non-page components
- [ ] No FABs for creation actions on list pages (use Header buttons)
- [ ] Edit/Delete buttons use proper semantic colors
- [ ] All duplicated patterns use shared components
- [ ] No component exceeds 200 lines
