# Handlers Refactoring Rulebook

## Objective

Refactor `Wordfolio.Api` handlers into vertical feature slices with explicit API contracts and mappings, while preserving existing endpoint behavior.

## Target Structure

```text
Wordfolio.Api/Wordfolio.Api/
├── Api/
│   ├── Types.fs
│   ├── Mappers.fs
│   ├── Helpers.fs
│   ├── Auth/
│   │   ├── Types.fs
│   │   ├── Mappers.fs
│   │   └── Handlers.fs
│   ├── Collections/
│   │   ├── Types.fs
│   │   ├── Mappers.fs
│   │   └── Handlers.fs
│   ├── CollectionsHierarchy/
│   │   ├── Types.fs
│   │   ├── Mappers.fs
│   │   └── Handlers.fs
│   ├── Vocabularies/
│   │   ├── Types.fs
│   │   ├── Mappers.fs
│   │   └── Handlers.fs
│   ├── Entries/
│   │   ├── Types.fs
│   │   ├── Mappers.fs
│   │   └── Handlers.fs
│   ├── Drafts/
│   │   ├── Types.fs
│   │   ├── Mappers.fs
│   │   └── Handlers.fs
│   └── Dictionary/
│       ├── Types.fs
│       ├── Mappers.fs
│       └── Handlers.fs
├── Urls.fs
├── OpenApi.fs
└── Program.fs
```

## Refactoring Principles

1. API types are defined separately from handlers.
2. Mappers are defined separately from handlers and API types.
3. No API feature may import another API feature.
4. Shared cross-feature API concerns are extracted to root `Api/Types.fs`, `Api/Mappers.fs`, or `Api/Helpers.fs`.
5. Domain error DU to HTTP response mapping is allowed in feature `Handlers.fs`.
6. API type names should match current domain naming.
7. Shared auth and helper logic (including `getUserId`) lives in `Api/Helpers.fs`.
8. Unexpected `Result<_, unit>` error paths must fail fast (throw) with operation context.
9. Route-parameter cleanup is out of scope for this refactor.
10. API breaking changes are forbidden: keep routes, HTTP methods, auth requirements, status codes, and request or response contracts unchanged.
11. All namespaces must match physical file location on disk.

## Scope and Non-Goals

### In Scope

- File and module structure migration to vertical slices.
- Type extraction from handlers.
- Mapper extraction from handlers.
- Removal of cross-handler and cross-feature dependencies in API layer.
- Shared helper extraction (`getUserId`, fail-fast helper).

### Out of Scope

- Behavioral endpoint changes.
- Route template redesign.
- Fixing unused route parameters.
- Domain logic changes.

## Compile Order Contract

1. `Api/Types.fs`
2. `Api/Mappers.fs`
3. `Api/Helpers.fs`
4. Per-feature `Types.fs`
5. Per-feature `Mappers.fs`
6. Per-feature `Handlers.fs`
7. `OpenApi.fs`
8. `Program.fs`
