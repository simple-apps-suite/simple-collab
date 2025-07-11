// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

namespace SimpleCollabService.Repository.Sqlite.Data;

record UserInfo(string Username, long Expiration, long UsedBytes);
