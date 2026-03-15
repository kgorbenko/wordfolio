# Sidebar Collection Tree — Interaction & Visual States

## Structure

Collections and vocabularies form a two-level tree in the sidebar:

```
Collections
├── English Vocabulary          (collection, expandable)
│   ├── Common Words            (vocabulary)
│   ├── Academic Terms          (vocabulary)
│   └── Idioms & Phrases        (vocabulary)
├── Spanish Basics              (collection, no children)
└── Academic Writing            (collection, no children)
```

- Collections that have children show an expand/collapse chevron (`>` / `v`).
- Collections without children show no chevron.
- Expanded collections reveal a vertical tree line on the left of the children block.
- All items (collections and vocabularies) use the same `ListItemButton` component with identical padding, border-radius, and margin.

## Visual States

Three independent visual channels, each signaling a different thing:

| Channel        | Signals         | Style                                                  |
|----------------|-----------------|--------------------------------------------------------|
| Background     | Scope           | Selected background covers the entire expanded group   |
| Text color     | Active item     | Accent color (`#B5F507`) on the one active item        |
| Underline      | Hover / cursor  | Text underline on the item under the cursor            |

### Background — Scope

When a collection is expanded, its selected background extends to cover the **entire hierarchy block**: the collection item itself and all its child vocabularies. This visually groups them as one cohesive unit.

```
                                        ┌─────────────────────────┐
  📁 English Vocabulary           v     │  selected background    │
     Common Words              156      │  covers this entire     │
     Academic Terms             89      │  block                  │
     Idioms & Phrases           42      │                         │
                                        └─────────────────────────┘
  📁 Spanish Basics                        (no background)
  📁 Academic Writing                      (no background)
```

Collapsed collections have no selected background (unless they themselves are the active item — see below).

### Text Color — Active Item

Exactly **one** item in the sidebar has accent-colored text at any time. The accent moves down the hierarchy as the user drills deeper:

1. **Browsing collections** (no vocabulary selected): the expanded collection name is in accent color.
2. **Vocabulary selected**: the accent moves to the vocabulary. The collection reverts to white text (but stays expanded with the group background).

This avoids double-accent — there is never more than one accent-colored item.

| Scenario                          | Collection text | Vocabulary text |
|-----------------------------------|-----------------|-----------------|
| Collection expanded, no vocab     | Accent          | White           |
| Collection expanded, vocab active | White           | Accent          |
| Collection collapsed              | White           | —               |

### Underline — Hover

Hover is indicated by a **text underline** on the item under the cursor. No background change on hover.

This keeps hover visually distinct from both the scope background and the active text color, and avoids conflicting with the group background that covers expanded hierarchies.

## State Combinations

| Item state                         | Background       | Text color | Underline |
|------------------------------------|------------------|------------|-----------|
| Default                            | None             | White      | No        |
| Hover                              | None             | White      | Yes       |
| Expanded collection (no vocab)     | Group background | Accent     | No        |
| Expanded collection (vocab active) | Group background | White      | No        |
| Expanded collection + hover        | Group background | Accent/White | Yes     |
| Selected vocabulary                | Group background | Accent     | No        |
| Selected vocabulary + hover        | Group background | Accent     | Yes       |
| Collapsed collection               | None             | White      | No        |

## Expand/Collapse Behavior

- Clicking a collection toggles its expanded state.
- The vertical tree line runs the full height of the children block (no horizontal branches).
- The tree line color matches the border/divider token (`#555555`).
- The chevron icon is `ExpandMore` (expanded) / `ChevronRight` (collapsed), colored `#888888`.
