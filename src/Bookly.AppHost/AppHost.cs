var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume("bookly-postgres-data");

postgres
    .WithPgAdmin(containerName: "pgmyadmin")
    .WithLifetime(ContainerLifetime.Persistent);

var booklyDatabase = postgres
    .AddDatabase("booklydb", "bookly");

var api = builder.AddProject<Projects.Bookly_Api>("bookly-api")
    .WithReference(booklyDatabase)
    .WaitFor(booklyDatabase);

builder.AddProject<Projects.Bookly_Ui>("bookly-ui")
    .WithReference(api)
    .WaitFor(api);

// Bookly.Cli is a standalone console app, not an Aspire service.
// Run it separately: dotnet run --project src/Bookly.Cli -- add 3 5

builder.Build().Run();
