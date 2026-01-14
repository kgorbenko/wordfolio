using Microsoft.Extensions.Configuration;
using Wordfolio.AppHost;

const string databaseName = "wordfoliodb";

var builder = DistributedApplication.CreateBuilder(args);

var postgresUsername = builder.AddParameter("postgres-username", secret: true);
var postgresPassword = builder.AddParameter("postgres-password", secret: true);
var groqApiKey = builder.AddParameter("groq-api-key", secret: true);

var databaseOptions =
    builder.Configuration
        .GetRequiredSection(Configuration.DatabaseOptionsSection)
        .Get<DatabaseOptions>()!;

var postgres =
    builder.AddPostgres("postgres", postgresUsername, postgresPassword)
        .WithEndpoint(name: "postgresendpoint", scheme: "tcp", port: databaseOptions.Port, targetPort: 5432, isProxied: false)
        .WithLifetime(ContainerLifetime.Persistent)
        .WithDataBindMount(databaseOptions.DataBindMount);

var postgresDatabase = postgres.AddDatabase(databaseName);

var migrationService = builder.AddProject<Projects.Wordfolio_MigrationRunner>("migrationservice")
    .WithReference(postgresDatabase)
    .WaitFor(postgresDatabase);

var api = builder.AddProject<Projects.Wordfolio_Api>("apiservice")
    .WithReference(postgresDatabase)
    .WaitForCompletion(migrationService)
    .WithHttpHealthCheck("/health")
    .WithEnvironment("GroqApi__ApiKey", groqApiKey);

builder.AddViteApp("frontend", "../Wordfolio.Frontend")
    .WithReference(api)
    .WithEnvironment("BROWSER", "none")
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

builder.Build().Run();
