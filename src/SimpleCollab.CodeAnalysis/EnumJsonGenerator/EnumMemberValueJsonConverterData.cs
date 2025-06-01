// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using SimpleCollab.CodeAnalysis.Utility;

namespace SimpleCollab.CodeAnalysis.EnumJsonGenerator;

readonly record struct EnumMemberValueJsonConverterData(
    string Namespace,
    string TypeType,
    string TypeName,
    string EnumTypeName,
    EquatableArray<(string Name, string Value)> EnumMemberValues
);
