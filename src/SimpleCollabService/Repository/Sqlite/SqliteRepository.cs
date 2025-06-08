// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using Microsoft.Data.Sqlite;
using SimpleCollabService.Repository.Abstractions;

namespace SimpleCollabService.Repository.Sqlite;

public class SqliteRepository(SqliteConnection connection) : ISimpleCollabRepository
{
    /// <inheritdoc/>
    public async ValueTask ApplyMigrationsAsync(CancellationToken cancellationToken = default) =>
        await SqliteHelper.ApplyMigrationsAsync(connection, cancellationToken).ConfigureAwait(false);
}
