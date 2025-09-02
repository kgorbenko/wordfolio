# Project Guidelines

- Coding guidelines
  - Prefer short, single-word variable names.
  - Do not write comments.
  - Prefer using `open` statements instead of referencing namespaces explicitly.
  - F#
    - Prefer modules and functions, small files grouped by responsibility.
    - Use explicit types for public APIs. Keep pipelines readable.
  - csproj|fsproj
    - Use double spaces for indentation.
    - `PropertyGroup`s and `ItemGroup`s should be separated by a blank line.