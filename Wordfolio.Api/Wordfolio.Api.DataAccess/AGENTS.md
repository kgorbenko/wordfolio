# Data Access Layer Design Rules

**Scope:** `Wordfolio.Api.DataAccess` project only.

These rules govern the internal design of the data access layer: how modules are structured, how Dapper is used, how types are named, and how functions are organized. They complement but do not replace the project-wide conventions in the root `AGENTS.md`.

---

## Module Organization Rules

- One `.fs` file per entity or logical grouping (for example, `Collections.fs`, `CollectionsHierarchy.fs`).
- Declare each file as a top-level module: `module Wordfolio.Api.DataAccess.<Name>`.
- Keep `Dapper.fs` as shared infrastructure helpers only (no entity-specific logic).
- Keep `Schema.fs` as the single source of truth for table names and column name literals.

## Naming Rules

- Align data access type and function names with domain concepts when they represent the same business meaning.
- Public parameter record types use the `Create...Parameters`, `Update...Parameters`, `Move...Parameters` naming pattern.
- Internal Dapper materialization records are named `...Record` and declared `internal` (for example, `CollectionRecord`, `EntryRecord`).
- Internal insert-parameter records that differ from the public parameter type are named `...InsertParameters` and declared `internal` (for example, `CollectionInsertParameters`).
- All async functions are suffixed with `Async` and return `Task<'T>`.

## CLIMutable Rules

- Apply `[<CLIMutable>]` only to records that Dapper must materialize from query results (internal `...Record` types and internal `...InsertParameters` types used as output targets).
- Do not apply `[<CLIMutable>]` to public output records or public parameter records; they are constructed explicitly via `fromRecord` helpers or passed as input.

## Dapper and Table Declaration Rules

- Declare Dapper table bindings inline inside the function that uses them; do not hoist table declarations to module scope.
- Use `Schema.<Table>.Name` for all table name references; never use string literals directly.
- Use `Schema.Name` for all schema name references.

## Function Signature Rules

- Business parameters come first; infrastructure parameters (`connection`, `transaction`, `cancellationToken`) come last.
- Always annotate return types explicitly: `Task<int>` for affected-row counts, `Task<'a option>` for single lookups, `Task<'a list>` for multi-row results.
- Keep private mapping helpers (`fromRecord`) close to the record types they consume, before the first function that uses them.

## File Compile Order Rules

- Keep `.fsproj` compile order aligned with dependency direction: `Dapper.fs` first, `Schema.fs` second, then entity modules.
