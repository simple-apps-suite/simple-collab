// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using System.Data.Common;
using Microsoft.Data.Sqlite;
using SimpleCollabService.Repository.Sqlite.Migrations;
using SimpleCollabService.Utility;

namespace SimpleCollabService.Repository.Sqlite;

static partial class SqliteHelper
{
    delegate Task SqliteMigrationAsync(
        SqliteConnection connection,
        CancellationToken cancellationToken
    );

    static readonly (string Name, SqliteMigrationAsync MigrateAsync)[] s_migrations =
    [
        ("M0_FirstVersion", ApplyMigration<M0_FirstVersion>),
    ];

    public static async Task ApplyMigrationsAsync(
        SqliteConnection connection,
        CancellationToken cancellationToken = default
    )
    {
        await connection.OpenAsync(cancellationToken);

        HashSet<string> unappliedMigrations = [.. s_migrations.Select(m => m.Name)];

        if (
            await SqliteQueries
                .TableExistsAsync(connection, "migrations", cancellationToken)
                .ConfigureAwait(false)
        )
        {
            await foreach (
                string appliedMigration in SqliteQueries
                    .GetMigrationNamesAsync(connection, cancellationToken)
                    .ConfigureAwait(false)
            )
                unappliedMigrations.Remove(appliedMigration);
        }

        foreach ((string name, SqliteMigrationAsync? migrateAsync) in s_migrations)
        {
            if (unappliedMigrations.Contains(name))
            {
                using DbTransaction transaction = await connection
                    .BeginTransactionAsync(cancellationToken)
                    .ConfigureAwait(false);

                await migrateAsync(connection, cancellationToken).ConfigureAwait(false);

                await SqliteQueries
                    .InsertMigrationAsync(
                        connection,
                        name,
                        DateTime.UtcNow.ToUnixTimeSeconds(),
                        cancellationToken
                    )
                    .ConfigureAwait(false);

                await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }

    static Task ApplyMigration<TMigration>(
        SqliteConnection connection,
        CancellationToken cancellationToken
    )
        where TMigration : ISqliteMigration => TMigration.Apply(connection, cancellationToken);
}
