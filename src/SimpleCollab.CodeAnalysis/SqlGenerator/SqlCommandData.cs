// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using SimpleCollab.CodeAnalysis.Utility;

namespace SimpleCollab.CodeAnalysis.SqlGenerator;

readonly record struct SqlCommandData(
    bool IsQuery,
    bool ReturnsAsyncEnumerable,
    string Namespace,
    string TypeType,
    string TypeName,
    string? MethodVisibility,
    string MethodName,
    string Sql,
    string? ResultType,
    string? ResultTypeInner,
    EquatableArray<(string Name, string Type, EquatableArray<int> SqlIndices)> Parameters,
    string DbConnectionParameter,
    EquatableArray<(string Name, string Type)> ResultFields
);
