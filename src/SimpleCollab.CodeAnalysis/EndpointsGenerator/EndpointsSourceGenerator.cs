// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using SimpleCollab.CodeAnalysis.Utility;

namespace SimpleCollab.CodeAnalysis.EndpointsGenerator;

[Generator]
class EndpointsSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx =>
            ctx.AddSource(
                "EndpointsAttributes.g.cs",
                SourceText.From(EndpointsHelper.AttributesSource, Encoding.UTF8)
            )
        );

        IncrementalValuesProvider<EndpointData?> endpoints = context
            .SyntaxProvider.ForAttributeWithMetadataName(
                EndpointsHelper.EndpointAttributeFullName,
                predicate: static (_, _) => true,
                transform: static (ctx, cancellationToken) =>
                    GetEndpointData(ctx.SemanticModel, ctx.TargetNode, cancellationToken)
            )
            .Where(c => c is not null);

        IncrementalValuesProvider<EndpointsGroupData?> endpointsGroups = context
            .SyntaxProvider.ForAttributeWithMetadataName(
                EndpointsHelper.EndpointsGroupAttributeFullName,
                predicate: static (_, _) => true,
                transform: static (ctx, cancellationToken) =>
                    GetEndpointsGroupData(ctx.SemanticModel, ctx.TargetNode, cancellationToken)
            )
            .Where(c => c is not null);

        IncrementalValueProvider<EquatableArray<EndpointsGroupAndEndpointsData>> groupedEndpoints =
            endpoints
                .Collect()
                .Combine(endpointsGroups.Collect())
                .Select(
                    (data, CancellationToken) =>
                    {
                        var (unfilteredEndpoints, unfilteredEndpointsGroups) = data;

                        var endpointsByGroup = unfilteredEndpoints
                            .Where(e => e.HasValue)
                            .Select(e => e!.Value)
                            .ToLookup(e => (e.Namespace, e.TypeName));

                        var grouped = unfilteredEndpointsGroups
                            .Where(g => g.HasValue)
                            .Select(g => g!.Value)
                            .Select(group => new EndpointsGroupAndEndpointsData(
                                group,
                                new([.. endpointsByGroup[(group.Namespace, group.TypeName)]])
                            ));

                        return new EquatableArray<EndpointsGroupAndEndpointsData>([.. grouped]);
                    }
                );

        context.RegisterSourceOutput(
            endpoints,
            static (spc, source) => GenerateEndpointSource(source, spc)
        );

        context.RegisterSourceOutput(
            groupedEndpoints,
            static (spc, source) =>
            {
                foreach (EndpointsGroupAndEndpointsData endpointsGroupWithEndpoints in source)
                    GenerateEndpointsGroupSource(endpointsGroupWithEndpoints, spc);
            }
        );
    }

    static EndpointData? GetEndpointData(
        SemanticModel semanticModel,
        SyntaxNode syntaxNode,
        CancellationToken cancellationToken
    )
    {
        if (
            semanticModel.GetDeclaredSymbol(syntaxNode, cancellationToken)
            is not IMethodSymbol methodSymbol
        )
            return null;

        AttributeData? endpointAttribute = methodSymbol
            .GetAttributes()
            .FirstOrDefault(attr =>
                attr.AttributeClass?.ToDisplayString() is EndpointsHelper.EndpointAttributeFullName
            );

        if (endpointAttribute is null)
            return null;

        if (
            endpointAttribute.ConstructorArguments.FirstOrDefault().Value
            is not string patternString
        )
            return null;

        string? verb =
            endpointAttribute.NamedArguments.FirstOrDefault(kvp => kvp.Key is "Verb").Value.Value
            as string;

        return new EndpointData(
            methodSymbol.ContainingNamespace.ToDisplayString(),
            methodSymbol.ContainingType.TypeKind.ToCSharpTypeKind(),
            methodSymbol.ContainingType.Name,
            methodSymbol.Name,
            patternString,
            verb
        );
    }

    static EndpointsGroupData? GetEndpointsGroupData(
        SemanticModel semanticModel,
        SyntaxNode syntaxNode,
        CancellationToken cancellationToken
    )
    {
        if (
            semanticModel.GetDeclaredSymbol(syntaxNode, cancellationToken)
            is not ITypeSymbol typeSymbol
        )
            return null;

        AttributeData? endpointsGroupAttribute = typeSymbol
            .GetAttributes()
            .FirstOrDefault(attr =>
                attr.AttributeClass?.ToDisplayString()
                is EndpointsHelper.EndpointsGroupAttributeFullName
            );

        if (endpointsGroupAttribute is null)
            return null;

        if (
            endpointsGroupAttribute.ConstructorArguments.FirstOrDefault().Value
            is not string prefixString
        )
            return null;

        return new EndpointsGroupData(
            typeSymbol.ContainingNamespace.ToDisplayString(),
            typeSymbol.TypeKind.ToCSharpTypeKind(),
            typeSymbol.Name,
            prefixString
        );
    }

    static void GenerateEndpointSource(EndpointData? endpoint, SourceProductionContext context)
    {
        if (endpoint is not { } value)
            return;

        string result = EndpointsHelper.GenerateEndpointSource(value);
        context.AddSource(
            $"Endpoint.{value.Namespace}.{value.TypeName}.{value.MethodName}.g.cs",
            SourceText.From(result, Encoding.UTF8)
        );
    }

    static void GenerateEndpointsGroupSource(
        EndpointsGroupAndEndpointsData endpointsGroupWithEndpoints,
        SourceProductionContext context
    )
    {
        string result = EndpointsHelper.GenerateEndpointsGroupSource(endpointsGroupWithEndpoints);
        context.AddSource(
            $"EndpointsGroup.{endpointsGroupWithEndpoints.Group.Namespace}.{endpointsGroupWithEndpoints.Group.TypeName}.g.cs",
            SourceText.From(result, Encoding.UTF8)
        );
    }
}
