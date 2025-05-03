// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using Microsoft.CodeAnalysis;

namespace SimpleCollab.CodeAnalysis.Utility;

static class AccessibilityExtensions
{
    public static string? ToCSharpAccessibilityModifier(this Accessibility accessibility) =>
        accessibility switch
        {
            Accessibility.NotApplicable => null,
            Accessibility.Private => "private",
            Accessibility.ProtectedAndInternal => "private protected",
            Accessibility.Protected => "protected",
            Accessibility.Internal => "internal",
            Accessibility.ProtectedOrInternal => "protected internal",
            Accessibility.Public => "public",
            _ => throw new NotSupportedException(),
        };
}
