using PANiXiDA.Core.Application.Querying.Filtering;

namespace PANiXiDA.Core.Application.UnitTests.Querying.Filtering;

public sealed class FilterParametersTests
{
    [Fact(DisplayName = "FilterParameters can be used as a read filter base type")]
    public void FilterParameters_WhenDerived_CanBeUsedAsBaseType()
    {
        FilterParameters parameters = new TestFilterParameters("active");

        parameters.Should().BeOfType<TestFilterParameters>();
    }

    [Fact(DisplayName = "With expression copies filter parameters")]
    public void WithExpression_WhenFilterValuesAreChanged_CopiesFilterParameters()
    {
        var parameters = new TestFilterParameters("active");

        var updated = parameters with
        {
            Status = "inactive"
        };

        updated.Status.Should().Be("inactive");
    }

    private sealed record TestFilterParameters(string Status) : FilterParameters;
}
