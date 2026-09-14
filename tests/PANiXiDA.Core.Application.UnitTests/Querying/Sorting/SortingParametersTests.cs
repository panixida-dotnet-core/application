using PANiXiDA.Core.Application.Querying.Sorting;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using SortDirection = PANiXiDA.Core.Application.Querying.Sorting.SortDirection;

namespace PANiXiDA.Core.Application.UnitTests.Querying.Sorting;

public sealed class SortingParametersTests
{
    [Fact(DisplayName = "Default sorting contains no implicit fields")]
    public void Default_WhenCalled_ReturnsEmptySorting()
    {
        var parameters = SortingParameters.Default();

        parameters.Fields.ShouldBeEmpty();
        parameters.HasSorting.ShouldBeFalse();
        SortingParameters.None.Fields.ShouldBeEmpty();
        new SortingParameters([]).Fields.ShouldBeEmpty();
    }

    [Fact(DisplayName = "Positional record stores and deconstructs the supplied criteria array")]
    public void Constructor_WhenArrayIsProvided_PreservesArrayReference()
    {
        var first = new SortField("department.name", SortDirection.Desc);
        var second = new SortField("name");
        var fields = new[] { first, second };
        var parameters = new SortingParameters(fields);

        parameters.Deconstruct(out var actualFields);

        parameters.Fields.ShouldBeSameAs(fields);
        actualFields.ShouldBeSameAs(fields);
        parameters.Fields.ShouldBe([first, second]);
        parameters.HasSorting.ShouldBeTrue();
    }

    [Fact(DisplayName = "With replaces the criteria array without changing the original record")]
    public void With_WhenReplacingFields_PreservesOriginalCriteria()
    {
        var parameters = SortingParameters.Of(new SortField("department.name", SortDirection.Desc), new SortField("name"));
        var replacement = new[] { new SortField("createdAt") };

        var copy = parameters with { Fields = replacement };

        copy.ShouldNotBeSameAs(parameters);
        copy.Fields.ShouldBeSameAs(replacement);
        copy.HasSorting.ShouldBeTrue();
        parameters.Fields.ShouldBe([new SortField("department.name", SortDirection.Desc), new SortField("name")]);
    }

    [Fact(DisplayName = "Factories preserve criterion order and direction")]
    public void Factories_WhenCalled_PreserveFieldAndDirection()
    {
        var ascending = SortingParameters.Ascending("name");
        var descending = SortingParameters.Descending("department.name");
        var multiple = SortingParameters.Of(descending.Fields[0], ascending.Fields[0]);

        ascending.Fields.ShouldHaveSingleItem().ShouldBe(new SortField("name", SortDirection.Asc));
        descending.Fields.ShouldHaveSingleItem().ShouldBe(new SortField("department.name", SortDirection.Desc));
        multiple.Fields.ShouldBe([new SortField("department.name", SortDirection.Desc), new SortField("name")]);
        SortingParameters.Of().Fields.ShouldBeEmpty();
    }

    [Fact(DisplayName = "WithDefault retains explicit precedence and appends only missing default fields")]
    public void WithDefault_WhenFieldsOverlap_MergesWithoutChangingInputs()
    {
        var parameters = SortingParameters.Of(new SortField("NAME"), new SortField("department.name", SortDirection.Desc));
        var defaults = SortingParameters.Of(new SortField("name", SortDirection.Desc), new SortField("createdAt", SortDirection.Desc));

        var combined = parameters.WithDefault(defaults);

        combined.Fields.ShouldBe([
            new SortField("NAME"),
            new SortField("department.name", SortDirection.Desc),
            new SortField("createdAt", SortDirection.Desc)
        ]);
        parameters.Fields.Length.ShouldBe(2);
        defaults.Fields[0].Order.ShouldBe(SortDirection.Desc);
    }

    [Fact(DisplayName = "WithDefault accepts missing defaults and uses defaults when explicit sorting is empty")]
    public void WithDefault_WhenOneSideIsEmpty_PreservesAvailableCriteria()
    {
        var defaults = SortingParameters.Descending("name");

        var combined = SortingParameters.None.WithDefault(defaults);

        combined.Fields.ShouldBe(defaults.Fields);
        defaults.WithDefault(null).ShouldBeSameAs(defaults);
        defaults.WithDefault(SortingParameters.None).ShouldBeSameAs(defaults);
        defaults.WithDefault(SortingParameters.Ascending("NAME")).ShouldBeSameAs(defaults);
        SortingParameters.None.WithDefault(null).Fields.ShouldBeEmpty();
    }

    [Theory(DisplayName = "SortDirection has localized display names")]
    [InlineData(SortDirection.Asc, "По возрастанию")]
    [InlineData(SortDirection.Desc, "По убыванию")]
    public void DisplayName_WhenSortDirectionIsProvided_ReturnsLocalizedName(
        SortDirection sortDirection,
        string expectedDisplayName)
    {
        var member = typeof(SortDirection).GetMember(sortDirection.ToString()).Single();
        var displayAttribute = member.GetCustomAttribute<DisplayAttribute>();

        displayAttribute.ShouldNotBeNull();
        displayAttribute.Name.ShouldBe(expectedDisplayName);
    }
}
