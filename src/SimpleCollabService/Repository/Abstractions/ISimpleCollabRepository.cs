// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

namespace SimpleCollabService.Repository.Abstractions;

public interface ISimpleCollabRepository
{
    /// <summary>
    /// Applies any pending migrations to the database.
    /// This method should be called before interacting with the repository,
    /// to ensure that the database schema is up to date.
    /// </summary>
    /// <returns></returns>
    ValueTask ApplyMigrationsAsync(CancellationToken cancellationToken = default);
}
