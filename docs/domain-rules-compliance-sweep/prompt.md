# Domain Rules Compliance Sweep — Autonomous Loop

You are an autonomous coding agent. Your job is to complete ONE unchecked step
from the spec, then exit.

## Workflow

1. Read `docs/domain-rules-compliance-sweep/spec.md`.
2. Read `AGENTS.md` before editing code.
3. Read the **Progress Log** section in `docs/domain-rules-compliance-sweep/spec.md` first — previous iterations may include blockers or conventions.
4. Find the first unchecked step (`- [ ]`) in `docs/domain-rules-compliance-sweep/spec.md`.
5. Execute exactly that one step:
   - **Implement**: apply only the changes needed for that step.
   - **Improve**: review the implementation against the stated concerns and make targeted fixes.
6. Run the mandatory loop verification suite from `docs/domain-rules-compliance-sweep/spec.md` on every invocation:
   - `dotnet fantomas .`
   - `dotnet build "Wordfolio.Api/Wordfolio.Api/Wordfolio.Api.fsproj"`
   - `dotnet test "Wordfolio.Api/Wordfolio.Api.Domain.Tests/Wordfolio.Api.Domain.Tests.fsproj"`
   - `dotnet test "Wordfolio.Api/Wordfolio.Api.Domain.Tests/Wordfolio.Api.Domain.Tests.fsproj" --filter "FullyQualifiedName~Collections"`
   - `dotnet test "Wordfolio.Api/Wordfolio.Api.Domain.Tests/Wordfolio.Api.Domain.Tests.fsproj" --filter "FullyQualifiedName~Vocabularies"`
   - `dotnet test "Wordfolio.Api/Wordfolio.Api.Domain.Tests/Wordfolio.Api.Domain.Tests.fsproj" --filter "FullyQualifiedName~CollectionsHierarchy"`
   - `dotnet test "Wordfolio.Api/Wordfolio.Api.Domain.Tests/Wordfolio.Api.Domain.Tests.fsproj" --filter "FullyQualifiedName~Entries"`
   - `dotnet test`
   If any command fails, fix and rerun. If stuck after 3 attempts, document blocker details in Progress Log and exit.
7. Check the box for the completed step (`- [x]`).
8. Append an entry at the bottom of **Progress Log** in `docs/domain-rules-compliance-sweep/spec.md`:

   ```
   ### {Step description}
   - Files changed: {list}
   - What was done: {1-2 sentences}
   - Issues encountered: {if any, otherwise "None"}
   - Learnings: {patterns or context useful for future steps}
   ```

9. Commit with a descriptive message that follows repository style.
10. If all checkboxes are complete, output exactly:
    `<promise>COMPLETE</promise>`
11. Exit.