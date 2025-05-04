// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using SimpleCollabService.Repository.Abstractions;

namespace SimpleCollabService.Repository;

class RepositoryMigrationService(IServiceProvider services) : IHostedService
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
