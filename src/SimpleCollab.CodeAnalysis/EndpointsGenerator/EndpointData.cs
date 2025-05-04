// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

namespace SimpleCollab.CodeAnalysis.EndpointsGenerator;

readonly record struct EndpointData(
    string Namespace,
    string TypeType,
    string TypeName,
    string MethodName,
    string Pattern,
    string? Verb
);
