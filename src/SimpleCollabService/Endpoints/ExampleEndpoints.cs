// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using SimpleCollab.CodeAnalysis.EndpointsGenerator;

namespace SimpleCollabService.Endpoints
{
    [EndpointsGroup("")]
    partial class ExampleEndpoints
    {
        [Endpoint("/", Verb = "GET")]
        public static async Task HelloWorld(HttpContext context)
        {
            await context
                .Response.WriteAsync("Hello, world!", context.RequestAborted)
                .ConfigureAwait(false);
        }
    }
}
