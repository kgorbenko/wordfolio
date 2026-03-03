# Setup development environment

1. Specify secrets for the AppHost:

    ```
    cd Wordfolio.AppHost
    dotnet user-secrets set Parameters:postgres-username <postgres-username>
    dotnet user-secrets set Parameters:postgres-password <postgres-password>
    dotnet user-secrets set Parameters:groq-api-key <groq-api-key>
    ```

# Build and run

```bash
dotnet build                            # Build all projects
dotnet test                             # Run all backend tests
cd Wordfolio.Frontend && npm test       # Run all frontend tests
cd Wordfolio.Frontend && npm run build  # Build frontend
dotnet run --project Wordfolio.AppHost  # Start the app (migrations run automatically)
```
