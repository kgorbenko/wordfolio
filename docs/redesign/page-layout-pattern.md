# Page Layout Pattern

## Structure

Every list/detail page is composed of three independent sibling sections stacked vertically:

```
Page Header       — title, description (detail pages), and secondary actions
Search Toolbar    — search field + primary action button
Grid              — table header + data rows
```

These are **separate nodes**, not nested inside each other.
### Semantic split

- **Page Header** owns identity and meta-actions: *what this page is* and *what you can do to it* (Edit, Delete)
- **Search Toolbar** owns content actions: *what you can do with its contents* (search and create items)

---

## Page Header Variants

### List pages (Collections)
- Title only — no buttons in the header
- `paddingBottom: 16px`, `marginBottom: 24px`

### Detail pages (Collection Detail)
- Row: title (`flex-grow: 1`) + secondary action buttons (Edit, Delete) top-right (`align-items: flex-start`)
- Description text below (`font-size: 13px`, `color: #888888`)
- `paddingBottom: 16px`, `marginBottom: 24px`, `gap: 8px` between title row and description

---

## Spacing (Desktop)

| Gap | Mechanism | Value |
|---|---|---|
| Page Header → Search field | `marginBottom` on Page Header + `paddingBottom` inside Page Header + `paddingTop` on Search Toolbar | 24px + 16px + 6px |
| Search field → column headers | `paddingBottom` on Search Toolbar + `paddingBlock` on Table Header | 6px + 8px |

---

## 32px Height Rhythm

Two elements share an explicit `height: 32px` with `flexShrink: 0` on both desktop and mobile:

1. **Search field** — inside the Search Toolbar
2. **Table Header** — column labels row

The primary action button in the Search Toolbar also uses `height: 32px` to align flush with the search field.

---

## Search Toolbar

- `paddingBlock: 6px` (equal top and bottom), `gap: 8px` between children
- Search field: `flex: 1 1 0`, `height: 32px`, `border-radius: 8px`, `background: #2E2E2E`, `border: 1px solid #555555`, `padding: 0 10px`, `gap: 8px`
  - Contains: search icon (13×13, `#666666`) | placeholder text (`#555555`) | clear icon (11×11)
- Primary action button: `height: 32px`, `flex-shrink: 0`, on the **right** side

---

## Button Styles

- Primary: `background: #444444`, `border: 1px solid #555555`, white text
- Neutral: border-only `#555555`, `color: #BBBBBB`
- Destructive: border-only `#AA5555`, `color: #AA5555`
- All buttons: `border-radius: 8px`, `padding: 6px 16px`, `font-size: 13px`

---

## Form Pages

Form pages (Create / Edit) share the same Page Header as detail pages — title + description — but have no Search Toolbar or Grid.

### Page Header (Form pages)
- Title only row (no buttons) — `font-size: 26px`
- Description text below (`font-size: 13px`, `color: #888888`) — contextual hint about the form
- `paddingBottom: 16px`, `marginBottom: 24px`, `gap: 8px`

### Form Body
- `display: flex`, `flex-direction: column`, `gap: 20px` between field groups

### Field Labels
- Same style as Grid column headers: `font-size: 10px`, `letter-spacing: 0.08em`, `text-transform: uppercase`, `color: #888888`
- `gap: 6px` between label and input

### Text Inputs
- `height: 32px`, `border-radius: 8px`, `padding: 0 12px`
- `background: #2E2E2E`, `border: 1px solid #555555`
- Placeholder text: `color: #888888`, `font-size: 13px`

### Textareas
- Explicit `height` as needed (e.g. `96px`), `align-items: flex-start`, `padding: 10px 12px`
- `border-radius: 8px`, `background: #2E2E2E`, `border: 1px solid #555555`

### Form Buttons
- Grouped in a `flex` row with `justify-content: flex-end`, `gap: 8px`, `margin-top: 8px`
- Primary (Submit): `background: #444444`, `border: 1px solid #555555`, white text
- Neutral (Cancel): border-only `#555555`, `color: #BBBBBB`
- All form buttons: `height: 32px`, `border-radius: 8px`, `padding: 0 16px`, `font-size: 13px`

---

## Grid

### Table Header
- `height: 32px`, `flexShrink: 0`, `paddingInline: 16px`
- Column labels: `font-size: 10px`, `letter-spacing: 0.08em`, `text-transform: uppercase`, `color: #888888`
- Sort indicator: `↓` character appended inline, `color: #AAAAAA`

### Rows
- `padding: 10px 16px`
- Striped backgrounds: odd rows `#323232`, even rows `#2C2C2C`
- Border-radius: first row `8px 8px 2px 2px`, last row `2px 2px 8px 8px`, middle rows `2px`
- Name column: `flex-grow: 1`, `min-width: 0` — stacked name (14px white) + description (11px `#666666`) with `gap: 3px`
- Fixed-width columns: Created At `100px`, Updated At `115px`, count column `80–100px` — all `flex-shrink: 0`, `text-align: right`, `color: #888888`

---

## Column Layout

| Column | Width | Notes |
|---|---|---|
| Name / Vocabulary | `flex-grow: 1` | Stacked name + description, truncates on overflow |
| Created At | `100px` | Date string |
| Updated At | `115px` | Date string + sort indicator in header |
| Count (Vocabularies / Entries) | `80–100px` | Numeric |

---

## Mobile Variant (390×844px)

### Differences from desktop

| Property | Desktop | Mobile |
|---|---|---|
| Container | centered 800px column | `padding: 28px 16px 0 16px`, `width: 100%` |
| Title font size | 26px | 22px |
| Search field height | 32px | 32px |
| Table Header height | 32px | 32px |
| Row padding | `10px 16px` | `10px 12px` |
| Table Header padding | `paddingInline: 16px` | `paddingInline: 12px` |

### Mobile Page Header variants

**List pages (Collections):** Title only, `marginBottom: 20px`. No buttons in header.

**Detail pages (Collection Detail):** Title + `⋮` overflow menu button (`width: 32px`, `height: 32px`, `border-radius: 8px`, `border: 1px solid #555555`) on the right. Edit and Delete are hidden behind the overflow menu. Description below. `marginBottom: 20px`, `gap: 4px`.

### Mobile Search Toolbar
- No `paddingBlock` — spacing to grid handled by `marginBottom: 12px` on the toolbar frame
- Search field: `height: 32px`, `border-radius: 8px`, `padding: 0 12px`
- Primary action button: `height: 32px`, same right-side placement as desktop
