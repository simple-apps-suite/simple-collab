// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using System.ComponentModel.DataAnnotations;

namespace SimpleCollabService.Data;

record IdentityLinkRequest(
    [property: Required] long Timestamp,
    [property: Required] string CurrentIdentity,
    [property: Required] string NewIdentity,
    [property: Required] string Username,
    [property: Required] string CurrentSignature,
    [property: Required] string NewSignature
);
