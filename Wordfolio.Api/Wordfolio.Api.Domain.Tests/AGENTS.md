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
- Do not construct test inputs by calling other production functions whose output the tested function consumes. Build inputs from explicit literal values (records, JSON strings, and so on). This prevents matching bugs in paired operations (for example, `serialize` and `deserialize`) from masking each other.
- Assert against complete expected objects using `Assert.Equivalent` or `Assert.Equal`. Do not assert individual properties in isolation when the operation's output is deterministic; partial assertions silently miss regressions in fields the test forgot to cover. When the output is genuinely non-deterministic (for example, when only a subset of inputs is selected), use the strongest partial assertion the contract allows.
