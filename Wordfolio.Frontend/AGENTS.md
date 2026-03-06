# Frontend Design Rules

**Scope:** `Wordfolio.Frontend` project only.

These rules govern frontend design and structure: feature boundaries, API contracts, mapping, routes, and mutation cache policy. They complement but do not replace the project-wide conventions in the root `AGENTS.md`.

---

## Architecture Rules

- Use a vertical, feature-first structure under `src/features/<feature>/`.
- Keep feature files focused: `api/`, `components/`, `hooks/`, `pages/`, `schemas/`, `routes.ts`, and `types.ts`.
- Keep route files in `src/routes/` as routing configuration only.
- Keep page-level orchestration in feature `pages/`; presentational rendering stays in feature `components/`.

## Dependency Rules

- Allowed: `Feature -> shared app modules` (`src/shared/`).
- Not allowed: `FeatureA -> FeatureB` direct dependencies.
- If multiple features need the same concept, extract it to `src/shared/`.
- Do not import another feature's `api`, `hooks`, `components`, `schemas`, `types`, or `pages` directly.
- Never duplicate code across features to sidestep a cross-feature import; extract the shared concept to `src/shared/` instead.

## Shared Module Rules

- All cross-feature shared code lives under `src/shared/`, organized into subdirectories that mirror feature subdirectory conventions: `api/`, `components/`, `contexts/`, `queries/`, `stores/`, `utils/`.
- Place code in `src/shared/` only when it is genuinely used by more than one feature. Feature-specific code stays inside the feature boundary even if it currently has no other consumers.
- Shared modules must never import from `src/features/`.
- Tests for shared modules mirror the `src/shared/` structure under `tests/shared/`.

## API Contracts Rules

- Define API transport contracts in feature `api` files.
- API request/response/query type names must match current server API type names exactly.
- Use explicit transport suffixes (`Request`, `Response`, `Query`) where applicable.
- Do not re-export (re-import) contracts from other modules or features.

## Mapper Rules

- Define DTO mapping logic only in feature `api/mappers.ts`.
- Mappers must be pure transformations (no network calls, no state updates, no navigation).
- UI layers (`pages`, `components`, `layouts`) must consume mapped feature types, not raw transport DTOs.
- Shared mapper helpers used by multiple features must be extracted to shared level.

## Mutation Cache Rules

- On every successful mutation, invalidate all React Query cache via `queryClient.invalidateQueries()`.
- Use global invalidation instead of selective key invalidation.

## Import and Naming Rules

- Import aliasing is allowed to resolve naming conflicts between libraries (e.g., `import { Link as MuiLink } from "@mui/material"`).
- Do not re-export symbols from one module through another as a passthrough wrapper.
- Do not create local type aliases that rename existing types without semantic change.

## Routing and Navigation Rules

- Keep route path definitions and path-builder helpers centralized in feature `routes.ts`.
- Use path builders for navigation; do not hardcode route strings in UI logic.
- Validate route params/search at route boundaries before page-level usage.

## Async UI and State Rules

- Every async screen must explicitly handle `Loading`, `Error`, and `Empty` states.
- Keep global cross-feature UI state in Zustand stores only when truly global.
- Keep feature-specific transient state inside feature hooks/components.

## Testing Rules

- Place tests under `Wordfolio.Frontend/tests/`, mirroring feature and component structure.
- Keep tests behavior-focused and aligned with feature boundaries.
- Avoid tests that rely on cross-feature internals.
