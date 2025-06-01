// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using System.Text.Json.Serialization;
using Microsoft.Data.Sqlite;
using SimpleCollabService.Data;
using SimpleCollabService.Endpoints;
using SimpleCollabService.Repository;
using SimpleCollabService.Repository.Abstractions;
using SimpleCollabService.Repository.Sqlite;

WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

builder.Services.AddTransient<SqliteConnection>(_ => new("Data Source=data.db"));

builder.Services.AddTransient<ISimpleCollabRepository, SqliteRepository>();

builder.Services.AddHostedService<MigrationService>();

WebApplication app = builder.Build();

RouteGroupBuilder api = app.MapGroup("/api");
RouteGroupBuilder v1 = api.MapGroup("/v1");
v1.MapGet("/server/info", ExampleEndpoints.GetServerInfo);
v1.MapPost("/identity", ExampleEndpoints.CreateIdentityAsync);

await app.RunAsync();

[JsonSerializable(typeof(ServerInfoResponse))]
[JsonSerializable(typeof(CreateIdentityRequest))]
[JsonSerializable(typeof(CreateIdentityResponse))]
[JsonSerializable(typeof(ErrorResponse))]
partial class AppJsonSerializerContext : JsonSerializerContext;
