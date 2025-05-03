// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using System.Text.Json.Serialization;
using Microsoft.Data.Sqlite;
using SimpleCollabService.Repository.Abstractions;
using SimpleCollabService.Repository.Sqlite;

WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

builder.Services.AddTransient<SqliteConnection>(_ => new("Data Source=data.db"));

builder.Services.AddTransient<ISimpleCollabRepository, SqliteRepository>();

builder.Services.AddHostedService<RepositoryMigratorService>();

WebApplication app = builder.Build();

app.MapGet("/", () => "Hello World!");

await app.RunAsync();

[JsonSerializable(typeof(Todo[]))]
[JsonSerializable(typeof(Test))]
partial class AppJsonSerializerContext : JsonSerializerContext;

class Todo();

class Test();

class RepositoryMigratorService(IServiceProvider services) : IHostedService
{
    class Executor(ISimpleCollabRepository repository, ILogger<Executor> logger)
    {
        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Applying migrations...");

            await repository.ApplyMigrationsAsync(cancellationToken).ConfigureAwait(false);

            logger.LogInformation("All migrations applied.");
        }
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using AsyncServiceScope scope = services.CreateAsyncScope();

        Executor executor = ActivatorUtilities.CreateInstance<Executor>(scope.ServiceProvider);

        await executor.ExecuteAsync(cancellationToken);
    }

    Task IHostedService.StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
