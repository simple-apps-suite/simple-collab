// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using System.Collections.Immutable;
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
                "SqlCommandAttributes.g.cs",
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

        if (sqlAttribute.ConstructorArguments.FirstOrDefault().Value is not string sqlString)
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

        (string Name, string Type, bool Constructor)[] resultFields = [];
        if (returnTypeInner?.IsRecord ?? false)
        {
            ImmutableArray<IParameterSymbol> constructorParameters = returnTypeInner
                .GetMembers(".ctor")
                .OfType<IMethodSymbol>()
                .FirstOrDefault()
                .Parameters;

            IEnumerable<(string Name, string Type, bool Constructor)> fieldsFromConstructor =
                constructorParameters.Select(p => (p.Name, p.Type.ToDisplayString(), true));

            IEnumerable<(string Name, string Type, bool Constructor)> fieldsFromProperties =
                returnTypeInner
                    .GetMembers()
                    .OfType<IPropertySymbol>()
                    .Where(p =>
                        p is { GetMethod: not null, SetMethod: not null }
                        && !constructorParameters.Any(p2 =>
                            p2.Name == p.Name
                            && SymbolEqualityComparer.Default.Equals(p2.Type, p.Type)
                        )
                    )
                    .Select(p => (p.Name, p.Type.ToDisplayString(), false));

            resultFields = [.. fieldsFromConstructor, .. fieldsFromProperties];
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
            returnTypeInner?.IsReferenceType ?? false,
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
            new(resultFields)
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
