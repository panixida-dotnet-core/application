using PANiXiDA.Core.Application.Querying.Sorting;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace PANiXiDA.Core.Application.UnitTests.Querying.Sorting;

public sealed class SortParametersTests
{
    [Fact(DisplayName = "Default sorting contains no implicit fields")]
    public void Default_WhenCalled_ReturnsEmptySorting()
    {
        var parameters = SortParameters.Default();

        parameters.Fields.ShouldBeEmpty();
        parameters.HasSorting.ShouldBeFalse();
        SortParameters.None.Fields.ShouldBeEmpty();
        new SortParameters().Fields.ShouldBeEmpty();
    }

    [Fact(DisplayName = "Constructor preserves criterion order and takes an immutable snapshot")]
    public void Constructor_WhenArrayChanges_PreservesOriginalCriteria()
    {
        var first = new SortField("department.name", SortOrder.Desc);
        var second = new SortField("name");
        var fields = new[] { first, second };
        var parameters = new SortParameters(fields);

        fields[0] = new SortField("changed");

        parameters.Fields.ShouldBe([first, second]);
        parameters.HasSorting.ShouldBeTrue();
    }

    [Fact(DisplayName = "Copying sorting parameters preserves immutable criteria")]
    public void With_WhenCopying_PreservesCriteria()
    {
        var parameters = SortParameters.Of(new SortField("department.name", SortOrder.Desc), new SortField("name"));

        var copy = parameters with { };

        copy.ShouldNotBeSameAs(parameters);
        copy.Fields.ShouldBe(parameters.Fields);
        copy.HasSorting.ShouldBeTrue();
    }

    [Fact(DisplayName = "Constructor rejects null arrays and null criteria")]
    public void Constructor_WhenInputIsNull_ThrowsArgumentException()
    {
        var nullArray = () => new SortParameters(null!);
        var nullCriterion = () => new SortParameters([null!]);

        nullArray.ShouldThrow<ArgumentNullException>().ParamName.ShouldBe("fields");
        nullCriterion.ShouldThrow<ArgumentException>().ParamName.ShouldBe("fields");
    }

    [Fact(DisplayName = "Factories preserve criterion order and direction")]
    public void Factories_WhenCalled_PreserveFieldAndDirection()
    {
        var ascending = SortParameters.Ascending("name");
        var descending = SortParameters.Descending("department.name");
        var multiple = SortParameters.Of(descending.Fields[0], ascending.Fields[0]);

        ascending.Fields.ShouldHaveSingleItem().ShouldBe(new SortField("name", SortOrder.Asc));
        descending.Fields.ShouldHaveSingleItem().ShouldBe(new SortField("department.name", SortOrder.Desc));
        multiple.Fields.ShouldBe([new SortField("department.name", SortOrder.Desc), new SortField("name")]);
        SortParameters.Of().Fields.ShouldBeEmpty();
    }

    [Fact(DisplayName = "WithDefault retains explicit precedence and appends only missing default fields")]
    public void WithDefault_WhenFieldsOverlap_MergesWithoutChangingInputs()
    {
        var parameters = new SortParameters(new SortField("NAME"), new SortField("department.name", SortOrder.Desc));
        var defaults = new SortParameters(new SortField("name", SortOrder.Desc), new SortField("createdAt", SortOrder.Desc));

        var combined = parameters.WithDefault(defaults);

        combined.Fields.ShouldBe([
            new SortField("NAME"),
            new SortField("department.name", SortOrder.Desc),
            new SortField("createdAt", SortOrder.Desc)
        ]);
        parameters.Fields.Length.ShouldBe(2);
        defaults.Fields[0].Order.ShouldBe(SortOrder.Desc);
    }

    [Fact(DisplayName = "WithDefault accepts missing defaults and uses defaults when explicit sorting is empty")]
    public void WithDefault_WhenOneSideIsEmpty_PreservesAvailableCriteria()
    {
        var defaults = SortParameters.Descending("name");

        var combined = SortParameters.None.WithDefault(defaults);

        combined.Fields.ShouldBe(defaults.Fields);
        defaults.WithDefault(null).ShouldBeSameAs(defaults);
        defaults.WithDefault(SortParameters.None).ShouldBeSameAs(defaults);
        defaults.WithDefault(SortParameters.Ascending("NAME")).ShouldBeSameAs(defaults);
        SortParameters.None.WithDefault(null).Fields.ShouldBeEmpty();
    }

    [Theory(DisplayName = "SortOrder has localized display names")]
    [InlineData(SortOrder.Asc, "По возрастанию")]
    [InlineData(SortOrder.Desc, "По убыванию")]
    public void DisplayName_WhenSortOrderIsProvided_ReturnsLocalizedName(
        SortOrder sortOrder,
        string expectedDisplayName)
    {
        var member = typeof(SortOrder).GetMember(sortOrder.ToString()).Single();
        var displayAttribute = member.GetCustomAttribute<DisplayAttribute>();

        displayAttribute.ShouldNotBeNull();
        displayAttribute.Name.ShouldBe(expectedDisplayName);
    }
}
