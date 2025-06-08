// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace SimpleCollab.CodeAnalysis.Utility;

static partial class StringExtensions
{
    static readonly Regex s_newlines = new(@"\r?\n", RegexOptions.Compiled);

    public static string ReplaceNewLines(this string str, string newLine = "\r\n") => s_newlines.Replace(str, newLine);

    public static string EscapeXml(this string str)
    {
        string escaped = new XElement("x", str).ToString();
        return escaped.Substring("<x>".Length, escaped.Length - "<x>".Length - "</x>".Length);
    }
}
