var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.Wordfolio_Api>("apiservice")
    .WithHttpHealthCheck("/health");

builder.AddNpmApp("frontend", "../Wordfolio.Frontend")
    .WithReference(api)
    .WithEnvironment("BROWSER", "none")
    .WithHttpEndpoint(env: "VITE_PORT")
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

builder.Build().Run();