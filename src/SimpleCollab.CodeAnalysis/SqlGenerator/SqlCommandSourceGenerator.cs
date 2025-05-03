// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using SimpleCollab.CodeAnalysis.Utility;

namespace SimpleCollab.CodeAnalysis.SqlGenerator;

[Generator]
class SqlCommandSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx =>
            ctx.AddSource(
                "SqlCommandAttribute.g.cs",
                SourceText.From(SqlCommandHelper.AttributesSource, Encoding.UTF8)
            )
        );

        IncrementalValuesProvider<SqlCommandData?> sqlCommands = context
            .SyntaxProvider.ForAttributeWithMetadataName(
                SqlCommandHelper.SqlCommandAttributeFullName,
                predicate: static (_, _) => true,
                transform: static (ctx, cancellationToken) =>
                    GetData(ctx.SemanticModel, ctx.TargetNode, cancellationToken)
            )
            .Where(c => c is not null);

        IncrementalValuesProvider<SqlCommandData?> sqlQueries = context
            .SyntaxProvider.ForAttributeWithMetadataName(
                SqlCommandHelper.SqlQueryAttributeFullName,
                predicate: static (_, _) => true,
                transform: static (ctx, cancellationToken) =>
                    GetData(ctx.SemanticModel, ctx.TargetNode, cancellationToken)
            )
            .Where(q => q is not null);

        context.RegisterSourceOutput(
            sqlCommands,
            static (spc, source) => GenerateSource(source, spc)
        );

        context.RegisterSourceOutput(
            sqlQueries,
            static (spc, source) => GenerateSource(source, spc)
        );
    }

    static SqlCommandData? GetData(
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

        AttributeData? sqlAttribute = methodSymbol
            .GetAttributes()
            .FirstOrDefault(attr =>
                attr.AttributeClass?.ToDisplayString()
                    is SqlCommandHelper.SqlQueryAttributeFullName
                        or SqlCommandHelper.SqlCommandAttributeFullName
            );

        if (sqlAttribute is null)
            return null;

        bool isQuery =
            sqlAttribute.AttributeClass?.ToDisplayString()
            is SqlCommandHelper.SqlQueryAttributeFullName;

        string? sqlString = sqlAttribute.ConstructorArguments.FirstOrDefault().Value as string;
        if (sqlString is null)
            return null;

        bool returnsAsyncEnumerable = false;
        ITypeSymbol? returnTypeInner = null;
        if (methodSymbol.ReturnType is INamedTypeSymbol namedTypeSymbol)
        {
            returnsAsyncEnumerable =
                namedTypeSymbol.ConstructedFrom.OriginalDefinition?.ToDisplayString()
                is KnownTypeNames.AsyncEnumerable1;
            returnTypeInner = namedTypeSymbol.TypeArguments.FirstOrDefault();
        }

        return new SqlCommandData(
            isQuery,
            returnsAsyncEnumerable,
            methodSymbol.ContainingNamespace.ToDisplayString(),
            methodSymbol.ContainingType.TypeKind.ToCSharpTypeKind(),
            methodSymbol.ContainingType.Name,
            methodSymbol.DeclaredAccessibility.ToCSharpAccessibilityModifier(),
            methodSymbol.Name,
            sqlString,
            methodSymbol.ReturnType.ToDisplayString(),
            returnTypeInner?.ToDisplayString(),
            new(
                [
                    .. methodSymbol.Parameters.Select(p =>
                        (
                            p.Name,
                            p.Type.ToDisplayString(),
                            new EquatableArray<int>(
                                p.Name is "connection" or "cancellationToken" ? [] : [0]
                            ) // TODO
                        )
                    ),
                ]
            ),
            methodSymbol.Parameters.First(p => IsDbConnection(p.Type)).Name,
            new([])
        );
    }

    static void GenerateSource(SqlCommandData? sqlCommand, SourceProductionContext context)
    {
        if (sqlCommand is not { } value)
            return;

        string result = SqlCommandHelper.GenerateSource(value);
        context.AddSource(
            $"SqlCommand.{value.Namespace}.{value.TypeName}.{value.MethodName}.g.cs",
            SourceText.From(result, Encoding.UTF8)
        );
    }

    static bool IsDbConnection(ITypeSymbol type)
    {
        ITypeSymbol? candidate = type;
        while (candidate is not null)
        {
            if (candidate.ToDisplayString() is KnownTypeNames.DbConnection)
                return true;

            candidate = candidate.BaseType;
        }

        return false;
    }
}
