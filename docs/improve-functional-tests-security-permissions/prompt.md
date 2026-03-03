# Improve Functional Tests Security Permissions — Autonomous Loop

You are an autonomous agent. Your job is to complete ONE unchecked step
from the spec, then exit.

## Workflow

1. Read `docs/improve-functional-tests-security-permissions/spec.md` in full.
2. Read the **Progress Log** section — previous iterations may have documented
   patterns or blockers relevant to your step.
3. Read the **Agent Instructions** section — follow all constraints stated there.
4. Find the first unchecked step (`- [ ]`) in **Implementation Steps**.
5. Execute it exactly as described in the step.
6. Check the box: change `- [ ]` to `- [x]` for the completed step.
7. Append an entry to the **Progress Log** in spec.md:
   ```
   ### {Step description}
   - Work done: {1-2 sentences summarizing what was accomplished}
   - Issues encountered: {if any, otherwise "None"}
   - Learnings: {context useful for future steps}
   ```
8. Count the remaining unchecked boxes (`- [ ]`) in the Implementation Steps
   section. If the count is **zero**, output the following token as your very
   last line and nothing else after it: `<promise>COMPLETE</promise>`
   If any unchecked boxes remain, do nothing — proceed directly to step 9.
9. Exit.

## Rules

- Complete exactly ONE step per invocation. Do not continue to the next step.
- Never uncheck a previously checked box.
- Never modify **Overview**, **Specification**, **Agent Instructions**,
  **Execution Protocol**, or **Verification Commands** in spec.md.
- Only modify in spec.md: checkboxes in **Implementation Steps** and the
  **Progress Log**.
- NEVER write, quote, or reference the completion token for any reason other
  than actual completion. Do NOT explain why you are not emitting it. If the
  job is not done, simply exit — silence is correct.
