// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using System.ComponentModel.DataAnnotations;

namespace SimpleCollabService.Data;

record RegisterUserRequest(
    [property: Required] long Timestamp,
    [property: Required] string Identity,
    [property: Required] string Username,
    [property: Required] string Signature
);
