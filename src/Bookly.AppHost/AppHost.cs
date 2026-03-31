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

var ui = builder.AddProject<Projects.Bookly_Ui>("bookly-ui")
    .WithReference(api)
    .WaitFor(api);

// Dev Tunnel (opt-in): expose bookly-ui externally via Microsoft Dev Tunnels CLI.
// Activate by setting DevTunnel:Enabled=true in user secrets or appsettings.Development.json.
// Prerequisite: install devtunnel CLI and run `devtunnel login` once.
if (builder.Configuration["DevTunnel:Enabled"] == "true")
{
    builder.AddExecutable("dev-tunnel", "devtunnel", ".",
            "host", "--allow-anonymous", "-p", "5044")
        .WaitFor(ui);
}

// Bookly.Cli is a standalone console app, not an Aspire service.
// Run it separately: dotnet run --project src/Bookly.Cli -- add 3 5

builder.Build().Run();
