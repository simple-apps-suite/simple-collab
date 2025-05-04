// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using SimpleCollab.CodeAnalysis.Utility;

namespace SimpleCollab.CodeAnalysis.EndpointsGenerator;

readonly record struct EndpointsGroupAndEndpointsData(
    EndpointsGroupData Group,
    EquatableArray<EndpointData> Endpoints
);
