var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.TechSpherex_CleanArchitecture_Api>("api")
    .WithExternalHttpEndpoints();

// --UseInMemoryStores=true (or scripts/start.ps1 -InMemory) runs the API with an in-memory
// database and cache: no PostgreSQL/Redis containers, so Docker is not required.
// All data is lost when the app stops.
if (string.Equals(builder.Configuration["UseInMemoryStores"], "true", StringComparison.OrdinalIgnoreCase))
{
    api.WithEnvironment("Database__UseInMemory", "true");
}
else
{
    var postgres = builder.AddPostgres("postgres")
        .WithPgAdmin()
        .WithLifetime(ContainerLifetime.Persistent);

    var database = postgres.AddDatabase("TechSpherex-db");

    var redis = builder.AddRedis("TechSpherex-cache")
        .WithRedisInsight()
        .WithLifetime(ContainerLifetime.Persistent);

    api.WithReference(database)
        .WaitFor(database)
        .WithReference(redis)
        .WaitFor(redis);
}

await builder.Build().RunAsync();
