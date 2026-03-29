var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .AddDatabase("booklydb", "bookly");

var api = builder.AddProject<Projects.Bookly_Api>("bookly-api")
    .WithReference(postgres)
    .WaitFor(postgres);

builder.AddProject<Projects.Bookly_Ui>("bookly-ui")
    .WithReference(api)
    .WaitFor(api);

// Bookly.Cli is a standalone console app, not an Aspire service.
// Run it separately: dotnet run --project src/Bookly.Cli -- add 3 5

builder.Build().Run();
