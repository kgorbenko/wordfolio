# Copilot Instructions

Follow these concise rules when generating code or suggestions for this repository.

- Formatting
  - Run: `dotnet fantomas .` to format generated F# code.
  - For csproj/fsproj files use double spaces for indentation.
  - Separate `PropertyGroup`s and `ItemGroup`s by a blank line.

- Naming
  - Use descriptive variable and type names.
  - Prefer short names only if they remain descriptive.
  - Avoid abbreviations.

- Comments
  - Do not add comments in generated code; prefer self-explanatory names and clear structure.

- Imports (F#)
  - Prefer `open` statements instead of referencing namespaces inline.
  - Remove unused `open` statements.
  - Sort `open` statements into groups with a blank line between groups:
    - System imports first.
    - Third-party imports second.
    - Local imports last.
  - Within each group, sort imports alphabetically.

- F# style
  - Prefer modules and small functions; group files by responsibility.
  - Use explicit types for public APIs.
  - Keep pipelines readable.

- General
  - Keep changes minimal and focused.
  - Follow repository conventions when suggesting edits.