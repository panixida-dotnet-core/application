using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PANiXiDA.Core.Application.Generators;

namespace PANiXiDA.Core.Application.UnitTests.Generators;

public sealed class SortingValidatorGeneratorTests
{
    private static readonly ImmutableArray<MetadataReference> References = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
        .Split(Path.PathSeparator)
        .Select(path => MetadataReference.CreateFromFile(path))
        .ToImmutableArray<MetadataReference>();

    [Fact(DisplayName = "Generator emits inherited and nested CLR paths without JSON aliases")]
    public void Generate_WhenModelHasNestedProperties_UsesScalarClrPaths()
    {
        var source = """
            namespace Example;
            public abstract record BaseModel(string Inherited) : IReadModel;
            public sealed record UserReadModel(string Name, Department? Department, Department Other) : BaseModel("base")
            {
                [System.Text.Json.Serialization.JsonPropertyName("display_name")]
                public string DisplayName { get; init; } = "";
                public new int Inherited { get; init; }
            }
            public sealed record Department(string Name, int? Number);
            """;

        var result = Generate(source);

        var generated = result.GeneratedSources.ShouldHaveSingleItem();
        generated.HintName.ShouldBe("Example.UserReadModelSortingValidator.g.cs");
        Fields(generated).ShouldBe(["Department.Name", "Department.Number", "DisplayName", "Inherited", "Name", "Other.Name", "Other.Number"]);
        generated.SourceText.ToString().ShouldContain("public sealed class UserReadModelSortingValidator");
        result.Diagnostics.ShouldBeEmpty();
    }

    [Theory(DisplayName = "Generator recognizes supported scalar types and nullable values")]
    [InlineData("bool")]
    [InlineData("char")]
    [InlineData("sbyte")]
    [InlineData("byte")]
    [InlineData("short")]
    [InlineData("ushort")]
    [InlineData("int")]
    [InlineData("uint")]
    [InlineData("long")]
    [InlineData("ulong")]
    [InlineData("float")]
    [InlineData("double")]
    [InlineData("decimal")]
    [InlineData("string")]
    [InlineData("System.Guid")]
    [InlineData("System.DateTime")]
    [InlineData("System.DateTimeOffset")]
    [InlineData("System.DateOnly")]
    [InlineData("System.TimeOnly")]
    [InlineData("System.TimeSpan")]
    [InlineData("System.DayOfWeek")]
    public void Generate_WhenPropertyIsScalar_IncludesProperty(string type)
    {
        var result = Generate($"public record Model({type} Value, {type}? Optional) : IReadModel;");

        Fields(result.GeneratedSources.ShouldHaveSingleItem()).ShouldBe(["Optional", "Value"]);
        result.Diagnostics.ShouldBeEmpty();
    }

    [Fact(DisplayName = "Generator skips unreadable properties, collections, and non-scalar system types")]
    public void Generate_WhenPropertiesCannotBeSorted_ExcludesThem()
    {
        var source = """
            public record BaseModel(string Hidden, string HiddenByField);
            public record Model : BaseModel, IReadModel
            {
                public Model() : base("", "") { }
                public string Name { get; init; } = "";
                private new string Hidden { get; init; } = "";
                public new string HiddenByField = "";
                public string Restricted { private get; set; } = "";
                public string WriteOnly { set { } }
                public static string Static { get; } = "";
                public string this[int index] { get { return ""; } }
                public string[] Array { get; } = [];
                public System.Collections.Generic.List<string> List { get; } = [];
                public System.Collections.Generic.IEnumerable<string>? Enumerable { get; }
                public System.Collections.IEnumerable? NonGenericEnumerable { get; }
                public System.Collections.Generic.Dictionary<string, string> Dictionary { get; } = [];
                public System.Uri? Uri { get; }
                public System.Text.StringBuilder? Builder { get; }
                public object? Object { get; }
                public dynamic? Dynamic { get; }
            }
            """;

        var result = Generate(source);

        Fields(result.GeneratedSources.ShouldHaveSingleItem()).ShouldBe(["Name"]);
        result.Diagnostics.ShouldBeEmpty();
    }

    [Fact(DisplayName = "Generator follows inherited interface properties and closed generic nested values")]
    public void Generate_WhenNestedTypeIsInterfaceOrGeneric_IncludesReadablePaths()
    {
        var result = Generate("""
            public interface IBase { string Name { get; } }
            public interface IDepartment : IBase { int Number { get; } }
            public record Value<T>(T Item);
            public abstract record Base<T>(T Inherited) : IReadModel;
            public sealed record Model(IDepartment Department, Value<int>? Value,
                PANiXiDA.Core.Application.Querying.Limiting.LimitParameters External) : Base<string>("");
            """);

        Fields(result.GeneratedSources.ShouldHaveSingleItem()).ShouldBe(["Department.Name", "Department.Number", "External.Limit", "Inherited", "Value.Item"]);
    }

    [Fact(DisplayName = "Generator terminates recursive and expanding generic paths")]
    public void Generate_WhenPropertiesAreRecursive_StopsAtRepeatedTypeDefinitions()
    {
        var result = Generate("""
            public record Node(string Name, Node? Parent);
            public record Recursive<T>(string Name, Recursive<System.Collections.Generic.List<T>>? Next);
            public record Model(string Name, Model? Parent, Node Node, Recursive<int> Recursive) : IReadModel;
            """);

        Fields(result.GeneratedSources.ShouldHaveSingleItem()).ShouldBe(["Name", "Node.Name", "Recursive.Name"]);
    }

    [Fact(DisplayName = "Generator emits one validator for partial records and ignores non-read-model types")]
    public void Generate_WhenDeclarationsArePartial_DeduplicatesSymbols()
    {
        var result = Generate("""
            public partial record Model(string Name) : IReadModel;
            public partial record Model { public int Age { get; init; } }
            public abstract record AbstractModel : IReadModel;
            public interface ICustomReadModel : IReadModel;
            public record Unrelated(string Name);
            """);

        Fields(result.GeneratedSources.ShouldHaveSingleItem()).ShouldBe(["Age", "Name"]);
    }

    [Fact(DisplayName = "Generator emits no output for unrelated declarations")]
    public void Generate_WhenNoReadModelsExist_ProducesNoSource()
    {
        var result = Generate("public class Unrelated { }");

        result.GeneratedSources.ShouldBeEmpty();
        result.Diagnostics.ShouldBeEmpty();
    }

    [Fact(DisplayName = "Generator respects namespaces, nested model names, and accessibility")]
    public void Generate_WhenModelNamesRepeatInDifferentScopes_EmitsDistinctValidators()
    {
        var result = Generate("""
            namespace One { public record Model(string Name) : IReadModel; }
            namespace Two { internal readonly record struct Model(int Number) : IReadModel; }
            namespace @event
            {
                public class Outer
                {
                    private record Model(string @class) : IReadModel;
                }
            }
            """);

        result.GeneratedSources.Length.ShouldBe(3);
        result.GeneratedSources[0].SourceText.ToString().ShouldContain("public sealed class ModelSortingValidator");
        result.GeneratedSources[1].SourceText.ToString().ShouldContain("internal sealed class ModelSortingValidator");
        result.GeneratedSources[2].SourceText.ToString().ShouldContain("internal sealed class Outer_ModelSortingValidator");
        Fields(result.GeneratedSources[2]).ShouldBe(["class"]);
        result.Diagnostics.ShouldBeEmpty();
    }

    [Theory(DisplayName = "Generator reports generic models instead of silently generating incomplete validators")]
    [InlineData("public record Model<T>(T Value) : IReadModel;")]
    [InlineData("public class Outer<T> { public record Model(string Name) : IReadModel; }")]
    public void Generate_WhenModelIsGeneric_ReportsDiagnostic(string source)
    {
        var result = Generate(source);

        result.GeneratedSources.ShouldBeEmpty();
        result.Diagnostics.ShouldHaveSingleItem().Id.ShouldBe("PANSG001");
    }

    [Theory(DisplayName = "Generator reports sorting validator name collisions")]
    [InlineData("public record Model(string Name) : IReadModel; public class ModelSortingValidator { }", 1)]
    [InlineData("public class Outer { public record Model(string Name) : IReadModel; } public record Outer_Model(string Name) : IReadModel;", 2)]
    public void Generate_WhenValidatorNameIsTaken_ReportsDiagnostic(string source, int count)
    {
        var result = Generate(source);

        result.GeneratedSources.ShouldBeEmpty();
        result.Diagnostics.Length.ShouldBe(count);
        result.Diagnostics.ShouldAllBe(diagnostic => diagnostic.Id == "PANSG002");
    }

    [Theory(DisplayName = "Generator rejects paths that differ only by case")]
    [InlineData("public record Model(string Name, string name) : IReadModel;")]
    [InlineData("public record Nested(string Name); public record Model(Nested Department, Nested department) : IReadModel;")]
    public void Generate_WhenFieldNamesAreAmbiguous_ReportsDiagnostic(string source)
    {
        var result = Generate(source);

        result.GeneratedSources.ShouldBeEmpty();
        result.Diagnostics.ShouldHaveSingleItem().Id.ShouldBe("PANSG003");
    }

    [Fact(DisplayName = "Generator updates the field set after read model properties change")]
    public void Generate_WhenPropertyIsRenamed_ReplacesSupportedPath()
    {
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new SortingValidatorGenerator());
        driver = driver.RunGenerators(CreateCompilation("public record Model(string Name) : IReadModel;"), TestContext.Current.CancellationToken);

        driver = driver.RunGenerators(CreateCompilation("public record Model(string DisplayName) : IReadModel;"), TestContext.Current.CancellationToken);

        var result = driver.GetRunResult().Results.ShouldHaveSingleItem();
        Fields(result.GeneratedSources.ShouldHaveSingleItem()).ShouldBe(["DisplayName"]);
    }

    private static GeneratorRunResult Generate(string source)
    {
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new SortingValidatorGenerator());
        driver = driver.RunGeneratorsAndUpdateCompilation(CreateCompilation(source), out var compilation, out _, TestContext.Current.CancellationToken);
        compilation.GetDiagnostics(TestContext.Current.CancellationToken).Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();
        var result = driver.GetRunResult().Results.ShouldHaveSingleItem();
        result.Exception.ShouldBeNull();
        return result;
    }

    private static CSharpCompilation CreateCompilation(string source)
    {
        var tree = CSharpSyntaxTree.ParseText("#nullable enable\nusing PANiXiDA.Core.Application.Querying;\n" + source,
            cancellationToken: TestContext.Current.CancellationToken);
        return CSharpCompilation.Create("GeneratorTests", [tree], References, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    private static string[] Fields(GeneratedSourceResult source)
    {
        return source.SyntaxTree.GetRoot(TestContext.Current.CancellationToken).DescendantNodes()
            .OfType<LiteralExpressionSyntax>().Where(literal => literal.IsKind(SyntaxKind.StringLiteralExpression))
            .Select(literal => literal.Token.ValueText).ToArray();
    }
}
