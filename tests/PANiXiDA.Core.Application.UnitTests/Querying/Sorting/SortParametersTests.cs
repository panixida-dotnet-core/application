using PANiXiDA.Core.Application.Querying.Sorting;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace PANiXiDA.Core.Application.UnitTests.Querying.Sorting;

public sealed class SortParametersTests
{
    [Fact(DisplayName = "Default returns ascending sorting without a field")]
    public void Default_WhenCalled_ReturnsAscendingSortingWithoutField()
    {
        var parameters = SortParameters.Default();

        parameters.Field.Should().BeNull();
        parameters.Order.Should().Be(SortOrder.Ascending);
    }

    [Fact(DisplayName = "Constructor stores sorting values")]
    public void Constructor_WhenValuesAreProvided_StoresValues()
    {
        var parameters = new SortParameters(Field: "name", Order: SortOrder.Descending);

        parameters.Field.Should().Be("name");
        parameters.Order.Should().Be(SortOrder.Descending);
    }

    [Fact(DisplayName = "With expression copies and updates sorting parameters")]
    public void WithExpression_WhenSortValuesAreChanged_CopiesAndUpdatesParameters()
    {
        var parameters = new SortParameters(Field: "name", Order: SortOrder.Ascending);

        var updated = parameters with
        {
            Field = "createdAt",
            Order = SortOrder.Descending
        };

        updated.Field.Should().Be("createdAt");
        updated.Order.Should().Be(SortOrder.Descending);
    }

    [Theory(DisplayName = "SortOrder has localized display names")]
    [InlineData(SortOrder.Ascending, "По возрастанию")]
    [InlineData(SortOrder.Descending, "По убыванию")]
    public void DisplayName_WhenSortOrderIsProvided_ReturnsLocalizedName(
        SortOrder sortOrder,
        string expectedDisplayName)
    {
        var member = typeof(SortOrder).GetMember(sortOrder.ToString()).Single();
        var displayAttribute = member.GetCustomAttribute<DisplayAttribute>();

        displayAttribute.Should().NotBeNull();
        displayAttribute!.Name.Should().Be(expectedDisplayName);
    }
}
