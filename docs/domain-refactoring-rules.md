# Domain Refactoring Rules

## Architecture Rules

- Use `Wordfolio.Api.Domain` as the only shared namespace for cross-feature domain code.
- Keep shared-level files at domain root as `Types.fs`, `Capabilities.fs`, and `Operations.fs`.
- Keep feature-level `Types.fs`, `Capabilities.fs`, `Operations.fs`, and `Errors.fs`, but do not use feature files as catch-all dumps.
- Features must not import other features. Cross-feature reuse must be extracted to the root-domain shared level.

## Dependency Rules

- Allowed: `Feature -> Wordfolio.Api.Domain` (root shared contracts/types).
- Not allowed: `FeatureA -> FeatureB`.
- If a feature needs another feature concept, move that concept (type/capability/helper operation) to root-level shared files.

## Capabilities Rules

- Capability methods must not use long unnamed tuple parameter lists.
- For any non-trivial method, introduce `...Parameters` record types.
- Use named tuple elements only for truly simple methods; otherwise prefer `...Parameters`.
- Naming convention: `CreateCollectionParameters`, `UpdateEntryParameters`, `MoveEntryParameters`, and so on (no `...Command`).

## Operations Rules

- Every public operation must declare explicit parameter and return types.
- Operation signatures should be stable and readable; avoid inferred broad types.
- Transaction orchestration stays in the operations layer (consistent boundary ownership).
- Do not silently swallow failures into default values unless that is an explicit business rule.

## Error Modeling Rules

- Errors must be operation-specific, not generic feature-wide unions.
- Use error DUs like `CreateEntryError`, `UpdateEntryError`, `DeleteEntryError` to avoid impossible cases in return types.
- Keep error cases minimal and specific to actual outcomes of the operation.
- Normalize access/not-found semantics consistently per operation policy.

## Type Design Rules

- Avoid misleading names. Type names must reflect payload depth and intent.
- Use clear suffixes when needed (`Overview`, `Details`, `WithEntryCount`, and so on).
- Prefer domain-specific value types where useful instead of primitive-heavy APIs.
- Do not alias or re-export root shared types inside feature namespaces (for example, no `type Collection = Wordfolio.Api.Domain.Collection` in `Wordfolio.Api.Domain.Collections`).

## File and Module Organization Rules

- Shared root: `Types.fs`, `Capabilities.fs`, `Operations.fs`.
- Per feature: at minimum `Types.fs`, `Capabilities.fs`, `Operations.fs`, `Errors.fs`.
- Split large operation files by use case only when needed, but keep naming predictable.
- Keep `.fsproj` compile order aligned with dependency direction (shared root first, then features).

## Refactoring Process Rules

- Refactor incrementally in compile-safe steps.
- First move and rename namespace structure, then update signatures (`...Parameters`), then split errors, then do naming cleanup.
- Update tests alongside each operation and error-contract change.
- Keep external behavior unchanged unless explicitly intended.
