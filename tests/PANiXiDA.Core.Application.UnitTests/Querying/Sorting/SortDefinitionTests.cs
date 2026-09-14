using PANiXiDA.Core.Application.Querying.Sorting;

namespace PANiXiDA.Core.Application.UnitTests.Querying.Sorting;

public sealed class SortDefinitionTests
{
    [Fact(DisplayName = "Definition snapshots field names and performs case-insensitive lookups")]
    public void Constructor_WhenInputChanges_PreservesDefinition()
    {
        var fields = new[] { "name", "department.name" };
        var definition = new SortDefinition<TestReadModel>(fields);

        fields[0] = "id";

        definition.Fields.Contains("NAME").ShouldBeTrue();
        definition.Fields.Contains("Department.Name").ShouldBeTrue();
        definition.Fields.Contains("id").ShouldBeFalse();
        definition.Fields.Count.ShouldBe(2);
    }

    [Fact(DisplayName = "Definition rejects a null field array")]
    public void Constructor_WhenArrayIsNull_Throws()
    {
        var action = () => new SortDefinition<TestReadModel>(null!);

        action.ShouldThrow<ArgumentNullException>().ParamName.ShouldBe("fields");
    }

    [Theory(DisplayName = "Definition rejects malformed field names")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("department..name")]
    [InlineData("name:asc")]
    public void Constructor_WhenFieldIsInvalid_Throws(string? field)
    {
        var action = () => new SortDefinition<TestReadModel>([field!]);

        action.ShouldThrow<ArgumentException>().ParamName.ShouldBe("fields");
    }

    [Fact(DisplayName = "Definition rejects case-insensitive field name collisions")]
    public void Constructor_WhenNamesCollide_Throws()
    {
        var action = () => new SortDefinition<TestReadModel>("name", "NAME");

        action.ShouldThrow<ArgumentException>().Message.ShouldStartWith("Sorting field 'NAME' is defined more than once.");
    }

    private sealed record TestReadModel(string Name);
}
