# Handlers Refactoring (Vertical API Slices) — Autonomous Loop

You are an autonomous coding agent. Your job is to complete ONE unchecked step from the spec, then exit.

## Workflow

1. Read `docs/handlers-refactoring/spec.md`.
2. Read the **Progress Log** section first.
3. Read conventions and constraints before coding:
   - `AGENTS.md`
   - `Wordfolio.Api/Wordfolio.Api/AGENTS.md`
   - `docs/handlers-refactoring/rulebook.md`
4. Find the first unchecked step (`- [ ]`) in **Implementation Steps**.
5. Execute exactly that step:
   - **Implement** step: migrate only the named feature by following the spec and rulebook.
   - **Improve** step: review the preceding feature migration against the spec and rulebook, then apply targeted fixes.
6. If namespace relocation breaks API tests, you may make minimal test updates only in `Wordfolio.Api/Wordfolio.Api.Tests/*.fs`:
   - allowed: namespace/import/type-reference updates
   - not allowed: behavior/assertion changes
7. Run verification commands from spec.md:
   - `dotnet build && dotnet test`
   Fix failures and re-run until both pass. If stuck after 3 attempts, document the blocker in Progress Log.
8. Commit with a descriptive message that states what step was completed and why.
9. Check the completed step box in `docs/handlers-refactoring/spec.md` (`- [ ]` -> `- [x]`).
10. Append an entry to **Progress Log** in `docs/handlers-refactoring/spec.md`:
    ```
    ### {Step description}
    - Files changed: {list}
    - What was done: {1-2 sentences}
    - Issues encountered: {if any, otherwise "None"}
    - Learnings: {patterns or context useful for future steps}
    ```
11. Count remaining unchecked boxes (`- [ ]`) in **Implementation Steps**:
    - If zero, output this token as the very last line and nothing else after it:
      `<promise>COMPLETE</promise>`
    - If not zero, do not output the token.
12. Exit.

## Rules

- Complete exactly ONE step per invocation.
- Do not continue to the next step in the same invocation.
- Never commit code that fails verification.
- Never uncheck a previously checked box.
- Never modify `Overview`, `Specification`, or `Verification Commands` in `docs/handlers-refactoring/spec.md`.
- Only modify in `docs/handlers-refactoring/spec.md`:
  - checkboxes in **Implementation Steps**
  - **Progress Log**
- Do not introduce API breaking changes: keep routes, HTTP methods, auth requirements, status codes, and request/response contracts unchanged.
- Do not add API cross-feature imports (`Wordfolio.Api.Api.<FeatureA>` -> `Wordfolio.Api.Api.<FeatureB>` is forbidden).
- Ensure namespaces/modules match physical file locations.
- For unexpected `Result<_, unit>` branches, fail fast with explicit exception context; do not use `Result.defaultValue`.
- Iteration budget is 20 invocations:
  - if there are already 20 Progress Log step entries and unchecked boxes remain, append a blocker note in Progress Log and exit without further implementation.
- NEVER write, quote, or reference the completion token for any reason other than actual completion.
