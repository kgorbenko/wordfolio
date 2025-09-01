using Microsoft.Extensions.Configuration;
using Wordfolio.AppHost;

const string databaseName = "wordfoliodb";

var builder = DistributedApplication.CreateBuilder(args);

var postgresUsername = builder.AddParameter("postgres-username", secret: true);
var postgresPassword = builder.AddParameter("postgres-password", secret: true);

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

var api = builder.AddProject<Projects.Wordfolio_Api>("apiservice")
    .WithReference(postgresDatabase)
    .WaitFor(postgresDatabase)
    .WithHttpHealthCheck("/health");

builder.AddNpmApp("frontend", "../Wordfolio.Frontend")
    .WithReference(api)
    .WithEnvironment("BROWSER", "none")
    .WithHttpEndpoint(env: "VITE_PORT")
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

builder.Build().Run();