// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using System.Text;
using Microsoft.CodeAnalysis;

namespace SimpleCollab.CodeAnalysis.Utility;

static class TypeKindExtensions
{
    public static string ToCSharpTypeKind(this TypeKind typeKind) =>
        typeKind switch
        {
            TypeKind.Class => "class",
            TypeKind.Delegate => "delegate",
            TypeKind.Enum => "enum",
            TypeKind.Interface => "interface",
            TypeKind.Struct => "struct",
            _ => throw new NotSupportedException(),
        };
}
