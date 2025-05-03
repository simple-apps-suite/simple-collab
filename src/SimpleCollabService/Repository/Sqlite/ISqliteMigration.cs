// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using Microsoft.Data.Sqlite;

namespace SimpleCollabService.Repository.Sqlite;

interface ISqliteMigration
{
    static abstract Task Apply(SqliteConnection connection, CancellationToken cancellationToken);
}
