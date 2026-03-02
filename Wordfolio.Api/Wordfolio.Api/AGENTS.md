# API Layer Design Rules

**Scope:** `Wordfolio.Api/Wordfolio.Api` project only.

These rules govern API-layer design: endpoint handlers, API contracts, API mappers, and shared handler helpers.
They complement but do not replace the root `AGENTS.md`.

---

## Architecture Rules

- Use a vertical, feature-first API structure under `Api/`.
- Keep shared root-level API files as `Api/Types.fs`, `Api/Mappers.fs`, and `Api/Helpers.fs`.
- Keep feature-level files as `Api/<Feature>/Types.fs`, `Api/<Feature>/Mappers.fs`, and `Api/<Feature>/Handlers.fs`.
- Do not define API request/response/query types inside `Handlers.fs`.
- Do not use feature files as catch-all dumps.

## Dependency Rules

- Allowed: `Api.<Feature> -> Wordfolio.Api.Api` root shared modules (`Types`, `Mappers`, `Helpers`).
- Allowed: `Api.<Feature> -> Wordfolio.Api.Domain.*` and required infrastructure for endpoint orchestration.
- Not allowed: `Api.<FeatureA> -> Api.<FeatureB>`.
- Not allowed: handler-to-handler dependencies across features.
- If multiple features need a concept, move it to root `Api/Types.fs` or `Api/Mappers.fs`.

## API Types Rules

- Define API request/response/query contracts only in `Types.fs` files.
- Type names must follow current domain naming terminology.
- Use explicit transport suffixes (`Request`, `Response`, `Query`) where applicable.
- Avoid aliases or re-exports of another feature's API types.

## Mapper Rules

- Define mapping logic only in `Mappers.fs` files.
- Mappers must be pure transformations (no DB calls, no network calls, no service resolution).
- Shared mapping helpers used across features belong in root `Api/Mappers.fs`.
- Mapping domain error DUs to HTTP responses is allowed directly in `Handlers.fs`.

## Helpers Rules

- Shared handler-level helpers must be placed in `Api/Helpers.fs`.
- Shared auth and claims helpers should be centralized in `Api/Helpers.fs`.
- For unexpected `Result<_, unit>` error cases, fail fast by throwing with context.
- Do not use `Result.defaultValue` for `Result<_, unit>` flows.

## Handler Rules

- `Handlers.fs` must focus on endpoint composition: auth extraction, env creation, operation invocation, and HTTP response assembly.
- Keep handlers thin; business logic remains in domain operations.
- Domain error DU to HTTP response mapping can live inside `Handlers.fs`.
- Do not introduce API breaking changes unless explicitly requested.

## File and Compile Order Rules

- `.fsproj` compile order must follow dependency direction:
  1. `Api/Types.fs`
  2. `Api/Mappers.fs`
  3. `Api/Helpers.fs`
  4. For each feature: `Types.fs -> Mappers.fs -> Handlers.fs`
  5. `Program.fs`
- Keep file and directory layout aligned with namespace and module structure.
