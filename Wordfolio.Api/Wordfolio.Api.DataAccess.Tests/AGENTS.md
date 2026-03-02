# Data Access Tests Design Rules

**Scope:** `Wordfolio.Api.DataAccess.Tests` project only.

These rules govern how data access integration tests are written and structured. They complement but do not replace the project-wide conventions in the root `AGENTS.md`.

---

## File and Module Organization Rules

- One test file per data access function, named `<FunctionNameInPascalCase>Tests.fs` (for example, `CreateCollectionTests.fs`, `GetDefaultCollectionByUserIdTests.fs`).
- Place test files under a subdirectory that matches the data access module name (for example, `Collections/`, `Entries/`, `Vocabularies/`).
- Declare each test file as a namespace: `namespace Wordfolio.Api.DataAccess.Tests.<Module>` (for example, `namespace Wordfolio.Api.DataAccess.Tests.Collections`).
- Keep `SqlErrorCodes.fs` at the project root as the single location for PostgreSQL error code constants.
- Keep `.fsproj` compile includes explicit and ordered: `SqlErrorCodes.fs` first, then test files grouped by module in dependency order.

## Fixture Rules

- Every test class receives `WordfolioTestFixture` via constructor injection and implements `IClassFixture<WordfolioTestFixture>`.
- Call `fixture.ResetDatabaseAsync()` as the first line of every test body to guarantee a clean database state.
- Use `fixture.WithConnectionAsync` to pass `connection`, `transaction`, and `cancellationToken` to data access functions under test.
- Use `fixture.Seeder` (accessed as `Seeder.*` after `open`) for all seed and assertion queries.

## Seed and Assertion Rules

- Seed test data exclusively through `Entities.make*` helpers and `Seeder.add*` pipeline functions followed by `Seeder.saveChangesAsync`. Never use the function under test to create prerequisite data.
- Assert writes by querying via `Seeder.get*` or `Seeder.getAll*` functions; never re-call the function under test to read back written data.
- Assert against complete expected objects using `Assert.Equivalent` or `Assert.Equal`; do not assert individual properties in isolation.
- For constraint violation tests, use `Assert.ThrowsAsync` and assert the specific `SqlState` from `SqlErrorCodes`.

## Test Coverage Rules

- Cover the success path, the not-found / empty path, and key constraint violation paths for every function.
- For write operations, include a test that verifies only the targeted row was affected (isolation of side effects).
- Keep every test file fully self-contained: fixture setup, seed setup, invocation, and assertions all in the same file.
