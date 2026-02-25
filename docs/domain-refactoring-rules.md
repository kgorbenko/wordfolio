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
- Capability-specific input record types must be declared in `Capabilities.fs` (place them at the top of the file before capability interfaces).

## Operations Rules

- Every public operation must declare explicit parameter and return types.
- Operation signatures should be stable and readable; avoid inferred broad types.
- Do not add explicit type annotations for the `env` parameter in operation signatures; rely on F# inference for environment capability composition.
- Define operation parameter record types in the feature `Operations.fs` module, not in `Capabilities.fs`.
- All public operation functions must return `Task<Result<'a, 'err>>`. For operations that cannot fail, use `unit` as the error type and annotate the return type explicitly. Handlers should use `Result.defaultValue` rather than a dead match arm for unit-error operations.
- Transaction orchestration stays in the operations layer (consistent boundary ownership).
- Do not silently swallow failures into default values unless that is an explicit business rule.

## Error Modeling Rules

- Errors must be operation-specific, not generic feature-wide unions.
- Use error DUs like `CreateEntryError`, `UpdateEntryError`, `DeleteEntryError` to avoid impossible cases in return types.
- Decorate all error DUs with `[<RequireQualifiedAccess>]` to enforce qualified access at every call site and prevent DU case shadowing.
- Keep error cases minimal and specific to actual outcomes of the operation.
- Normalize access/not-found semantics consistently per operation policy.

## Type Design Rules

- Avoid misleading names. Type names must reflect payload depth and intent.
- Use clear suffixes when needed (`Overview`, `Details`, `WithEntryCount`, and so on).
- Prefer domain-specific value types where useful instead of primitive-heavy APIs.
- Do not alias or re-export root shared types inside feature namespaces (for example, no `type Collection = Wordfolio.Api.Domain.Collection` in `Wordfolio.Api.Domain.Collections`).

## File and Module Organization Rules

- Shared root: `Types.fs`, `Capabilities.fs`, `Operations.fs`.
- Per feature: keep `Capabilities.fs`, `Operations.fs`, and `Errors.fs`; keep `Types.fs` only when the feature actually defines types in it.
- If a feature `Types.fs` is empty (or only a namespace shell), delete the file and remove it from `.fsproj`.
- Split large operation files by use case only when needed, but keep naming predictable.
- Keep `.fsproj` compile order aligned with dependency direction (shared root first, then features).

## Refactoring Process Rules

- Refactor incrementally in compile-safe steps.
- First move and rename namespace structure, then update signatures (`...Parameters`), then split errors, then do naming cleanup.
- Update tests alongside each operation and error-contract change.
- Keep external behavior unchanged unless explicitly intended.
