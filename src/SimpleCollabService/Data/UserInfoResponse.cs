// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

namespace SimpleCollabService.Data;

record UserInfoResponse(long Quota, long Used, long Expiration);
