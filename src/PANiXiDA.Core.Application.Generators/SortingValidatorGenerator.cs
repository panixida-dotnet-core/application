using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace PANiXiDA.Core.Application.Generators;

/// <summary>
/// Generates sorting validators from concrete read model properties at compile time.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class SortingValidatorGenerator : IIncrementalGenerator
{
    private static readonly DiagnosticDescriptor GenericModel = new(
        "PANSG001", "Sorting requires a concrete read model",
        "Read model '{0}' cannot generate a sorting validator because it or a containing type is generic",
        "Sorting", DiagnosticSeverity.Error, true);

    private static readonly DiagnosticDescriptor ValidatorCollision = new(
        "PANSG002", "Sorting validator name is ambiguous",
        "Read model '{0}' generates a sorting validator name '{1}' that is already in use",
        "Sorting", DiagnosticSeverity.Error, true);

    private static readonly DiagnosticDescriptor FieldCollision = new(
        "PANSG003", "Sorting field names are ambiguous",
        "Read model '{0}' contains sorting paths that differ only by case: '{1}'",
        "Sorting", DiagnosticSeverity.Error, true);

    /// <summary>
    /// Registers generation for source types implementing the application read model contract.
    /// </summary>
    /// <param name="context">The incremental generator initialization context.</param>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var models = context.SyntaxProvider.CreateSyntaxProvider(
            static (node, _) => node is TypeDeclarationSyntax,
            static (syntax, cancellationToken) => syntax.SemanticModel.GetDeclaredSymbol(syntax.Node, cancellationToken) as INamedTypeSymbol)
            .Where(static model => model is { IsAbstract: false }
                && model.AllInterfaces.Any(contract => contract.ToDisplayString() == "PANiXiDA.Core.Application.Querying.IReadModel"));

        context.RegisterSourceOutput(models.Collect(), static (output, symbols) => Generate(output, symbols));
    }

    private static void Generate(SourceProductionContext context, ImmutableArray<INamedTypeSymbol?> symbols)
    {
        var models = symbols.OfType<INamedTypeSymbol>().Distinct<INamedTypeSymbol>(SymbolEqualityComparer.Default).ToArray();
        var names = models.GroupBy(GetQualifiedValidatorName).ToDictionary(group => group.Key, group => group.Count());

        foreach (var model in models)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            var containingTypes = GetContainingTypes(model).ToArray();
            if (containingTypes.Any(type => type.IsGenericType))
            {
                context.ReportDiagnostic(Diagnostic.Create(GenericModel, model.Locations[0], model.ToDisplayString()));
                continue;
            }

            var name = GetValidatorName(model);
            if (names[GetQualifiedValidatorName(model)] > 1 || model.ContainingNamespace.GetTypeMembers(name).Length > 0)
            {
                context.ReportDiagnostic(Diagnostic.Create(ValidatorCollision, model.Locations[0], model.ToDisplayString(), name));
                continue;
            }

            var paths = new List<string>();
            CollectPaths(model, "", new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default), paths, context.CancellationToken);
            var collision = paths.GroupBy(path => path, StringComparer.OrdinalIgnoreCase).FirstOrDefault(group => group.Count() > 1);
            if (collision is not null)
            {
                context.ReportDiagnostic(Diagnostic.Create(FieldCollision, model.Locations[0], model.ToDisplayString(), string.Join(", ", collision)));
                continue;
            }

            var source = new StringBuilder();
            if (!model.ContainingNamespace.IsGlobalNamespace)
            {
                source.Append("namespace ").Append(model.ContainingNamespace.ToDisplayString()).AppendLine(";").AppendLine();
            }

            source.AppendLine("/// <summary>Validates sorting against the read model's public scalar property paths.</summary>");
            source.Append(containingTypes.All(type => type.DeclaredAccessibility == Accessibility.Public) ? "public" : "internal")
                .Append(" sealed class ").Append(name)
                .AppendLine(" : global::PANiXiDA.Core.Application.Querying.Sorting.SortingParametersValidator")
                .AppendLine("{")
                .AppendLine("    private static readonly global::System.Collections.Generic.IReadOnlySet<string> SupportedFields =")
                .AppendLine("        global::System.Collections.Frozen.FrozenSet.ToFrozenSet(new string[]")
                .AppendLine("        {");

            foreach (var path in paths.OrderBy(path => path, StringComparer.Ordinal))
            {
                source.Append("            ").Append(SymbolDisplay.FormatLiteral(path, true)).AppendLine(",");
            }

            source.AppendLine("        }, global::System.StringComparer.OrdinalIgnoreCase);")
                .AppendLine()
                .AppendLine("    /// <summary>Creates a sorting validator for this read model.</summary>")
                .AppendLine("    /// <param name=\"maxFields\">The positive maximum number of sorting criteria.</param>")
                .AppendLine("    /// <exception cref=\"global::System.ArgumentOutOfRangeException\">The maximum is not positive.</exception>")
                .Append("    public ").Append(name).AppendLine("(int maxFields = DefaultMaxFields)")
                .AppendLine("        : base(SupportedFields, maxFields)")
                .AppendLine("    {")
                .AppendLine("    }")
                .AppendLine("}");

            context.AddSource(GetQualifiedValidatorName(model).Replace("@", "") + ".g.cs", SourceText.From(source.ToString(), Encoding.UTF8));
        }
    }

    private static void CollectPaths(INamedTypeSymbol model, string prefix, HashSet<INamedTypeSymbol> ancestors,
        List<string> paths, CancellationToken cancellationToken)
    {
        if (!ancestors.Add(model.OriginalDefinition))
        {
            return;
        }

        foreach (var property in GetProperties(model))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (property.IsStatic || property.IsIndexer || property.GetMethod?.DeclaredAccessibility != Accessibility.Public)
            {
                continue;
            }

            var type = property.Type;
            if (type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullable)
            {
                type = nullable.TypeArguments[0];
            }

            var path = prefix + property.Name;
            if (IsScalar(type))
            {
                paths.Add(path);
            }
            else if (type is INamedTypeSymbol nested)
            {
                var ns = nested.ContainingNamespace.ToDisplayString();
                if (nested.SpecialType != SpecialType.System_Collections_IEnumerable
                    && !nested.AllInterfaces.Any(contract => contract.SpecialType == SpecialType.System_Collections_IEnumerable)
                    && ns != "System" && !ns.StartsWith("System.", StringComparison.Ordinal))
                {
                    CollectPaths(nested, path + ".", ancestors, paths, cancellationToken);
                }
            }
        }

        ancestors.Remove(model.OriginalDefinition);
    }

    private static IEnumerable<IPropertySymbol> GetProperties(INamedTypeSymbol model)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var types = model.TypeKind == TypeKind.Interface ? new[] { model }.Concat(model.AllInterfaces) : GetBaseTypes(model);
        foreach (var type in types)
        {
            foreach (var member in type.GetMembers())
            {
                if (seen.Add(member.Name) && member is IPropertySymbol property)
                {
                    yield return property;
                }
            }
        }
    }

    private static IEnumerable<INamedTypeSymbol> GetBaseTypes(INamedTypeSymbol model)
    {
        for (var type = model; type is not null; type = type.BaseType)
        {
            yield return type;
        }
    }

    private static bool IsScalar(ITypeSymbol type)
    {
        return type.TypeKind == TypeKind.Enum
            || type.SpecialType is >= SpecialType.System_Boolean and <= SpecialType.System_Double
                or SpecialType.System_String or SpecialType.System_DateTime
            || type.ToDisplayString() is "System.Guid" or "System.DateTimeOffset" or "System.DateOnly" or "System.TimeOnly" or "System.TimeSpan";
    }

    private static IEnumerable<INamedTypeSymbol> GetContainingTypes(INamedTypeSymbol model)
    {
        for (var type = model; type is not null; type = type.ContainingType)
        {
            yield return type;
        }
    }

    private static string GetValidatorName(INamedTypeSymbol model)
    {
        return string.Join("_", GetContainingTypes(model).Reverse().Select(type => type.Name)) + "SortingValidator";
    }

    private static string GetQualifiedValidatorName(INamedTypeSymbol model)
    {
        return model.ContainingNamespace.IsGlobalNamespace
            ? GetValidatorName(model)
            : model.ContainingNamespace.ToDisplayString() + "." + GetValidatorName(model);
    }
}
