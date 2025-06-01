// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using Microsoft.Data.Sqlite;
using SimpleCollab.CodeAnalysis.SqlGenerator;

namespace SimpleCollabService.Repository.Sqlite;

static partial class SqliteQueries
{
    [SqlQuery("SELECT 1 FROM sqlite_master WHERE type = 'table' AND name = $name")]
    public static partial Task<bool> TableExistsAsync(
        SqliteConnection connection,
        string name,
        CancellationToken cancellationToken = default
    );

    [SqlQuery("SELECT name FROM migrations")]
    public static partial IAsyncEnumerable<string> GetMigrationNamesAsync(
        SqliteConnection connection,
        CancellationToken cancellationToken = default
    );

    [SqlCommand("INSERT INTO migrations (name, date) VALUES ($name, $date)")]
    public static partial Task InsertMigrationAsync(
        SqliteConnection connection,
        string name,
        long date,
        CancellationToken cancellationToken = default
    );

    [SqlCommand("INSERT OR IGNORE INTO identities (hash, public_key) VALUES ($hash, $public_key)")]
    public static partial Task InsertIdentityAsync(
        SqliteConnection connection,
        byte[] hash,
        byte[] public_key,
        CancellationToken cancellationToken = default
    );

    [SqlQuery("SELECT public_key FROM identities WHERE hash = $hash LIMIT 1")]
    public static partial Task<byte[]?> GetPublicKeyByHashAsync(
        SqliteConnection connection,
        byte[] hash,
        CancellationToken cancellationToken = default
    );
}
