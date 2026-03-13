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

var useFixedFrontendPort = builder.Configuration.GetValue<bool>("WORDFOLIO_FIXED_FRONTEND_PORT");

var fixedFrontendPortOptions =
    builder.Configuration
        .GetRequiredSection(Configuration.FixedFrontendPortOptionsSection)
        .Get<FixedFrontendPortOptions>()!;

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
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithEnvironment("GroqApi__ApiKey", groqApiKey);

var frontend = builder.AddViteApp("frontend", "../Wordfolio.Frontend")
    .WithReference(api)
    .WithEnvironment("BROWSER", "none")
    .PublishAsDockerFile();

if (useFixedFrontendPort)
    frontend.WithEndpoint("http", endpoint =>
    {
        endpoint.Port = fixedFrontendPortOptions.FrontendPort;
        endpoint.TargetHost = "0.0.0.0";
    });

builder.Build().Run();
