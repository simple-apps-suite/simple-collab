// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

namespace SimpleCollabService.Endpoints;

static class ExampleEndpoints
{
    public static async Task HelloWorld(HttpContext context)
    {
        await context
            .Response.WriteAsync("Hello, world!", context.RequestAborted)
            .ConfigureAwait(false);
    }
}
