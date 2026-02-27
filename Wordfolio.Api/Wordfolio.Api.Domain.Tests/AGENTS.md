# Domain Tests Design Rules

**Scope:** `Wordfolio.Api.Domain.Tests` project only.

These rules govern how domain operation tests are written and structured. They complement but do not replace the project-wide conventions in the root `AGENTS.md`.

---

## Test Rules

- Do not add compatibility wrappers in tests to reshape operation signatures; call operations directly with their real parameter records.
- Test dependency function signatures must match capability interface signatures exactly (parameter shape and return type).
- `...Calls` collections in tests must store exactly the parameter type of the method being tracked.
- For capability methods that use parameter records, store those same record types in `...Calls` collections.
- For capability methods that use tuple parameters, store those same tuple types in `...Calls` collections.
- Do not create duplicate test-only `...Call` records when capability parameter records already exist.
- In every test, assert the state of all `...Calls` collections (explicitly verify expected calls and explicit empties for non-used dependencies).
- Test module namespaces must mirror production namespaces with only `.Tests` inserted after `Wordfolio.Api.Domain` (for shared root operations, use `Wordfolio.Api.Domain.Tests.*`, not `Wordfolio.Api.Domain.Tests.Shared.*`).
