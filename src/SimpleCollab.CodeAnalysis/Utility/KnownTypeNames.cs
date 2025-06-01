// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

namespace SimpleCollab.CodeAnalysis.Utility;

static class KnownTypeNames
{
    public const string AsyncEnumerable1 = "System.Collections.Generic.IAsyncEnumerable<T>";
    public const string DbConnection = "System.Data.Common.DbConnection";
    public const string EnumMemberAttribute = "System.Runtime.Serialization.EnumMemberAttribute";
    public const string JsonConverter = "System.Text.Json.Serialization.JsonConverter<T>";
    public const string Task1 = "System.Threading.Tasks.Task<TResult>";
    public const string CancellationToken = "System.Threading.CancellationToken";
}
