// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

namespace SimpleCollab.CodeAnalysis.EndpointsGenerator;

readonly record struct EndpointsGroupData(
    string Namespace,
    string TypeType,
    string TypeName,
    string Prefix
);
