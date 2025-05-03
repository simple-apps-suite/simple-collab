// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using Microsoft.Data.Sqlite;
using SimpleCollab.CodeAnalysis.SqlGenerator;

namespace SimpleCollabService.Repository.Sqlite.Migrations;

public partial class M0_FirstVersion : ISqliteMigration
{
    /// <inheritdoc/>
    public static async Task Apply(SqliteConnection connection, CancellationToken cancellationToken)
    {
        await CreateTableMigrations(connection, cancellationToken);
    }

    [SqlCommand(
        """
            CREATE TABLE migrations (
                id INTEGER PRIMARY KEY,
                name TEXT NOT NULL,
                date TEXT NOT NULL
            );
            """
    )]
    private static partial Task<int> CreateTableMigrations(
        SqliteConnection connection,
        CancellationToken cancellationToken = default
    );
}
