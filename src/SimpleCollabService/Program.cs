// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Data.Sqlite;
using SimpleCollabService.Data;
using SimpleCollabService.Endpoints;
using SimpleCollabService.Repository;
using SimpleCollabService.Repository.Abstractions;
using SimpleCollabService.Repository.Sqlite;
using SimpleCollabService.Utility;

WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
    options.SerializerOptions.TypeInfoResolverChain.Add(AppJsonSerializerContext.Default);
});

builder.Services.AddTransient(services => new SqliteConnection("Data Source=data.db").WithServiceProvider(services));

builder.Services.AddTransient<ISimpleCollabRepository, SqliteRepository>();

builder.Services.AddHostedService<MigrationService>();

WebApplication app = builder.Build();

app.UseExceptionHandler(app => app.Run(ExampleEndpoints.HandleExceptionAsync));

RouteGroupBuilder api = app.MapGroup("/api");
RouteGroupBuilder v1 = api.MapGroup("/v1");
v1.MapGet("/server/info", ExampleEndpoints.ServerInfo);
v1.MapPost("/identity", ExampleEndpoints.RegisterIdentityAsync);
v1.MapGet("/identity/{hash}", ExampleEndpoints.IdentityAsync);
v1.MapPost("/user", ExampleEndpoints.RegisterUserAsync);
v1.MapPost("/user/info", ExampleEndpoints.UserInfoAsync);
v1.MapFallback(ExampleEndpoints.InvalidApiEndpoint);

await app.RunAsync();

[JsonSerializable(typeof(ErrorCode))]
[JsonSerializable(typeof(ErrorResponse))]
[JsonSerializable(typeof(IdentityResponse))]
[JsonSerializable(typeof(RegisterIdentityRequest))]
[JsonSerializable(typeof(RegisterIdentityResponse))]
[JsonSerializable(typeof(RegisterUserRequest))]
[JsonSerializable(typeof(RegisterUserResponse))]
[JsonSerializable(typeof(ServerInfoResponse))]
[JsonSerializable(typeof(UserInfoRequest))]
[JsonSerializable(typeof(UserInfoResponse))]
partial class AppJsonSerializerContext : JsonSerializerContext;

class ServiceProviderComponent(IServiceProvider services) : IComponent
{
    EventHandler? _disposed;

    public IServiceProvider ServiceProvider => services;

    ISite? IComponent.Site { get; set; }

    event EventHandler? IComponent.Disposed
    {
        add => _disposed += value;
        remove => _disposed -= value;
    }

    void IDisposable.Dispose() => _disposed?.Invoke(this, EventArgs.Empty);
}
