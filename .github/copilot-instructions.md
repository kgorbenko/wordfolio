# Copilot Instructions

Follow these concise rules when generating code or suggestions for this repository.

- Tech Stack
  - Backend: F# and C# with .NET 9.0
  - Frontend: TypeScript with React and Vite
  - Database: PostgreSQL (via Entity Framework Core)
  - Build tool: dotnet CLI, npm for frontend

- Project Structure
  - `Wordfolio.Api/` - Main API backend projects
    - `Wordfolio.Api/` - Web API project (F#)
    - `Wordfolio.Api.DataAccess/` - Data access layer (F#)
    - `Wordfolio.Api.Identity/` - Identity management (C#)
    - `Wordfolio.Api.Migrations/` - Database migrations (F#)
  - `Wordfolio.Frontend/` - React frontend application
  - `Wordfolio.Common/` - Shared F# code
  - `Wordfolio.ServiceDefaults/` - Service defaults configuration
  - `Wordfolio.AppHost/` - Application host for orchestration

- Commands
  - Build: `dotnet build`
  - Test: `dotnet test`
  - Format: `dotnet fantomas .`
  - Frontend build: `cd Wordfolio.Frontend && npm run build`
  - Frontend lint: `cd Wordfolio.Frontend && npm run lint`

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
  - Keep commits small and atomic. If you need to make multiple independent changes, split them into separate commits.