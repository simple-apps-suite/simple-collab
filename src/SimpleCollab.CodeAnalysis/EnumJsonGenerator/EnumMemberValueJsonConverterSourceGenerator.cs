// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using SimpleCollab.CodeAnalysis.Utility;

namespace SimpleCollab.CodeAnalysis.EnumJsonGenerator;

[Generator]
class EnumMemberValueJsonConverterSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx =>
            ctx.AddSource(
                "EnumMemberValueJsonConverterAttributes.g.cs",
                SourceText.From(EnumMemberValueJsonConverterHelper.AttributesSource, Encoding.UTF8)
            )
        );

        IncrementalValuesProvider<EnumMemberValueJsonConverterData?> jsonConverters = context
            .SyntaxProvider.ForAttributeWithMetadataName(
                EnumMemberValueJsonConverterHelper.AttributeFullName,
                predicate: static (_, _) => true,
                transform: static (ctx, cancellationToken) =>
                    GetData(ctx.SemanticModel, ctx.TargetNode, cancellationToken)
            )
            .Where(c => c is not null);

        context.RegisterSourceOutput(
            jsonConverters,
            static (spc, source) => GenerateSource(source, spc)
        );
    }

    static EnumMemberValueJsonConverterData? GetData(
        SemanticModel semanticModel,
        SyntaxNode syntaxNode,
        CancellationToken cancellationToken
    )
    {
        if (
            semanticModel.GetDeclaredSymbol(syntaxNode, cancellationToken)
            is not INamedTypeSymbol typeSymbol
        )
            return null;

        AttributeData? attribute = typeSymbol
            .GetAttributes()
            .FirstOrDefault(attr =>
                attr.AttributeClass?.ToDisplayString()
                is EnumMemberValueJsonConverterHelper.AttributeFullName
            );

        if (attribute is null)
            return null;

        ITypeSymbol? enumTypeSymbol = null;
        INamedTypeSymbol? candidate = typeSymbol;
        while ((candidate = candidate?.BaseType) is not null)
        {
            if (candidate.OriginalDefinition?.ToDisplayString() is KnownTypeNames.JsonConverter)
            {
                enumTypeSymbol = candidate.TypeArguments.FirstOrDefault();
                break;
            }
        }

        if (enumTypeSymbol is null)
            return null;

        return new EnumMemberValueJsonConverterData(
            typeSymbol.ContainingNamespace.ToDisplayString(),
            typeSymbol.TypeKind.ToCSharpTypeKind(),
            typeSymbol.Name,
            enumTypeSymbol.Name,
            new(
                [
                    .. enumTypeSymbol
                        .GetMembers()
                        .OfType<IFieldSymbol>()
                        .Select(f =>
                            (
                                f.Name,
                                Value: f.GetAttributes()
                                    .FirstOrDefault(a =>
                                        a.AttributeClass?.ToDisplayString()
                                        is KnownTypeNames.EnumMemberAttribute
                                    )
                                    ?.NamedArguments.FirstOrDefault(n => n.Key is "Value")
                                    .Value.ToCSharpString()
                            )
                        )
                        .Where(f => f.Value is not null)
                        .OfType<(string Name, string Value)>(),
                ]
            )
        );
    }

    static void GenerateSource(
        EnumMemberValueJsonConverterData? data,
        SourceProductionContext context
    )
    {
        if (data is not { } value)
            return;

        string result = EnumMemberValueJsonConverterHelper.GenerateSource(value);
        context.AddSource(
            $"EnumJsonGenerator.{value.Namespace}.{value.TypeName}.g.cs",
            SourceText.From(result, Encoding.UTF8)
        );
    }
}
