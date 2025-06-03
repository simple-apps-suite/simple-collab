// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using Microsoft.Data.Sqlite;
using SimpleCollab.CodeAnalysis.SqlGenerator;
using SimpleCollabService.Repository.Sqlite.Data;

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

    [SqlQuery("SELECT 1 FROM identities WHERE hash = $hash LIMIT 1")]
    public static partial Task<bool> IdentityExistsAsync(
        SqliteConnection connection,
        byte[] hash,
        CancellationToken cancellationToken = default
    );

    [SqlQuery("SELECT 1 FROM users WHERE username = $username COLLATE NOCASE LIMIT 1")]
    public static partial Task<bool> UsernameExistsAsync(
        SqliteConnection connection,
        string username,
        CancellationToken cancellationToken = default
    );

    [SqlCommand(
        """
            INSERT INTO users (username) VALUES ($username)
            WHERE NOT EXISTS (SELECT 1 FROM users WHERE username = $username)
            """
    )]
    public static partial Task<long> InsertUserIfNotExistsAsync(
        SqliteConnection connection,
        string username,
        CancellationToken cancellationToken = default
    );

    [SqlQuery("SELECT id FROM users WHERE username = $username COLLATE NOCASE LIMIT 1")]
    public static partial Task<long?> GetUserIdByUsernameAsync(
        SqliteConnection connection,
        string username,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Associate identity to user if the identity is not already associated
    /// withanother user, and there are no other identities associated with
    /// this user.
    /// </summary>
    [SqlQuery(
        """
            WITH usr(id) AS (SELECT id FROM users WHERE username = $username)
            UPDATE identities
            SET user_id = (SELECT id FROM usr)
            WHERE hash = $identity_hash
              AND (user_id IS NULL OR user_id = (SELECT id FROM usr)) -- No other user for the identity.
              AND NOT EXISTS (SELECT 1 FROM identities AS i INNER JOIN users AS u ON i.user_id = u.id AND hash <> $identity_hash) -- No other identity for the user.
            RETURNING 1;
            """
    )]
    public static partial Task<bool> AssociateIdentityToUserAsync(
        SqliteConnection connection,
        string username,
        byte[] identity_hash,
        CancellationToken cancellationToken = default
    );

    [SqlQuery(
        """
            SELECT u.username
            FROM identities AS i
            LEFT JOIN users AS u ON i.user_id = u.id
            WHERE i.hash = $identity_hash AND i.user_id IS NOT NULL
            """
    )]
    public static partial Task<string> GetIdentityUsernameAsync(
        SqliteConnection connection,
        byte[] identity_hash,
        CancellationToken cancellationToken = default
    );

    [SqlQuery(
        """
            -- TODO
            SELECT u.username, 123456789, 123456789
            FROM identities AS i
            LEFT JOIN users AS u ON i.user_id = u.id
            WHERE i.hash = $identity_hash AND i.user_id IS NOT NULL
            LIMIT 1
            """
    )]
    public static partial Task<UserInfo> GetUserInfo(
        SqliteConnection connection,
        byte[] identity_hash,
        CancellationToken cancellationToken = default
    );
}
