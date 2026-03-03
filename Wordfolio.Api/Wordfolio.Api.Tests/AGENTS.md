# API Integration Tests Design Rules

**Scope:** `Wordfolio.Api.Tests` project only.

These rules govern how API integration tests are written and structured. They complement but do not replace the project-wide conventions in the root `AGENTS.md`.

---

## File and Module Organization Rules

- Group test files by endpoint area directory (for example, `Collections/`, `Entries/`, `Drafts/`, `Dictionary/`).
- Keep one test file per endpoint behavior/route variant, named `<EndpointBehavior>Tests.fs` (for example, `CreateCollectionTests.fs`, `GetCollectionByIdTests.fs`, `DeleteCollectionTests.fs`).
- Do not use an `Async` suffix in test file names.
- Namespace declarations must match physical file locations: files under `Wordfolio.Api.Tests/<Area>/` use `namespace Wordfolio.Api.Tests.<Area>`; nested folders append namespace segments.
- Keep `.fsproj` compile includes explicit and ordered: shared test infrastructure files first, then infrastructure tests, then endpoint test files grouped by area.

## Fixture Rules

- Every endpoint test class receives `WordfolioIdentityTestFixture` via constructor injection and implements `IClassFixture<WordfolioIdentityTestFixture>`.
- Call `fixture.ResetDatabaseAsync()` as the first line of every test body.
- Create and dispose `WebApplicationFactory` and `HttpClient` in each test (`use` / `use!`), not as shared mutable state.
- Create authenticated clients via `factory.CreateAuthenticatedClientAsync(identityUser)`.
- Create identity users via `factory.CreateUserAsync(...)`, then seed corresponding Wordfolio users through `Seeder.addUsers` before calling protected business endpoints.

## Seed and Assertion Rules

- Seed prerequisite Wordfolio data exclusively through `Entities.make*` helpers and `Seeder.add*` pipeline functions followed by `Seeder.saveChangesAsync`.
- Assert write side effects via seeder queries (`Seeder.get*` / `Seeder.getAll*`); do not rely on another HTTP endpoint call as the only persistence assertion.
- Assert complete expected DTO/entity objects using `Assert.Equal` or `Assert.Equivalent`; do not assert isolated individual fields only.
- For write endpoints, verify the targeted change and verify non-targeted rows remain unchanged.

## Endpoint Coverage Rules

- Keep every test file fully self-contained: fixture setup, seed setup, invocation, and assertions all in the same file.
- For each protected endpoint, cover success, unauthenticated (`401`), and not-found or ownership-boundary behavior (`404`) when applicable.
- For input-bound endpoints, cover key validation failures (`400`) and key conflict/constraint failures (`409`) when applicable.
- For list endpoints, cover non-empty and empty responses, and filtering/sorting behavior when the endpoint supports query options.
- For update/delete endpoints, verify resulting persistence state, including cascade side effects for related rows where relevant.
- For user-scoped endpoints, include tests proving one user cannot read or mutate another user's data.