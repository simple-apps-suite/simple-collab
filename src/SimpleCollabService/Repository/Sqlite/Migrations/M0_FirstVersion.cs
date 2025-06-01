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
        await CreateTables(connection, cancellationToken);
    }

    [SqlCommand(
        """
            CREATE TABLE migrations (
                id INTEGER PRIMARY KEY,
                name TEXT NOT NULL,
                date DATETIME NOT NULL
            );

            CREATE TABLE users (
                id INTEGER PRIMARY KEY,
                username TEXT NOT NULL
            );

            CREATE TABLE identities (
                id INTEGER PRIMARY KEY,
                hash BLOB NOT NULL,
                public_key BLOB NOT NULL,
                user_id INTEGER,
                FOREIGN KEY(user_id) REFERENCES users(id)
            );

            CREATE TABLE documents (
                id INTEGER PRIMARY KEY,
                hash BLOB NOT NULL,
                type BLOB NOT NULL,
                data BLOB NOT NULL
            );

            CREATE TABLE rents (
                id INTEGER PRIMARY KEY,
                target_id INTEGER NOT NULL,
                by_id INTEGER NOT NULL,
                for_id INTEGER NOT NULL,
                expiration DATETIME,
                public BOOLEAN NOT NULL,
                signature BLOB NOT NULL,
                FOREIGN KEY(target_id) REFERENCES documents(id),
                FOREIGN KEY(by_id) REFERENCES identities(id),
                FOREIGN KEY(for_id) REFERENCES identities(id)
            );
            """
    )]
    private static partial Task CreateTables(
        SqliteConnection connection,
        CancellationToken cancellationToken = default
    );
}
