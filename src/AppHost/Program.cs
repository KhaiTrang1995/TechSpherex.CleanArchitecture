var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin()
    .WithLifetime(ContainerLifetime.Persistent);

var database = postgres.AddDatabase("TechSpherex-db");

var redis = builder.AddRedis("TechSpherex-cache")
    .WithRedisInsight()
    .WithLifetime(ContainerLifetime.Persistent);

builder.AddProject<Projects.TechSpherex_CleanArchitecture_Api>("api")
    .WithReference(database)
    .WaitFor(database)
    .WithReference(redis)
    .WaitFor(redis)
    .WithExternalHttpEndpoints();

builder.Build().Run();
