# Wordfolio

Wordfolio is a full-stack application built with .NET 9.0 backend (F# and C#) and React + TypeScript frontend.

## CI/CD

The project uses separate GitHub Actions workflows for backend and frontend:

- **Backend CI** (`.github/workflows/backend.yml`) - Runs on changes to backend code
  - Triggers on changes to `Wordfolio.Api/**`, `Wordfolio.AppHost/**`, `Wordfolio.Common/**`, `Wordfolio.ServiceDefaults/**`, and solution files
  - Restores dependencies, builds, runs tests, and checks F# formatting
  
- **Frontend CI** (`.github/workflows/frontend.yml`) - Runs on changes to frontend code
  - Triggers on changes to `Wordfolio.Frontend/**`
  - Installs dependencies, runs linter, executes tests, and builds the application

This separation allows for faster, more efficient CI/CD pipelines that only run when relevant code changes.

## Setup development environment

1. Specify username and password for the database:

    `Wordfolio.AppHost> dotnet user-secrets set Parameters:postgres-username <postgres-username>`

    `Wordfolio.AppHost> dotnet user-secrets set Parameters:postgres-password <postgres-password>`

2. Execute identity migrations:

    `dotnet ef database update --startup-project .\Wordfolio.Api\Wordfolio.Api\Wordfolio.Api.fsproj --connection "Host=localhost;Port=5432;Database=wordfoliodb;User ID=<postgres-username>;Password=<postgres-password>"`

3. Execute wordfolio migrations:

    ` dotnet fm migrate -p PostgreSQL15_0 -a ".\Wordfolio.Api\Wordfolio.Api.Migrations\bin\Debug\net9.0\Wordfolio.Api.Migrations.dll" -c "Host=localhost;Port=5432;Database=wordfoliodb;User ID=<postgres-username>;Password=<postgres-password>"`

## Development Commands

### Backend
- `dotnet restore` - Restore dependencies
- `dotnet build` - Build the solution
- `dotnet test` - Run tests
- `dotnet fantomas .` - Format F# code

### Frontend
- `npm install` - Install dependencies (in `Wordfolio.Frontend/`)
- `npm run dev` - Start development server
- `npm run build` - Build for production
- `npm run lint` - Run ESLint
- `npm test` - Run tests
- `npm run test:watch` - Run tests in watch mode