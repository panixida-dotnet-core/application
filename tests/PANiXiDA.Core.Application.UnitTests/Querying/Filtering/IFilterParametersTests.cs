using PANiXiDA.Core.Application.Querying.Filtering;

namespace PANiXiDA.Core.Application.UnitTests.Querying.Filtering;

public sealed class IFilterParametersTests
{
    [Fact(DisplayName = "IFilterParameters can identify a read filter record")]
    public void IFilterParameters_WhenImplementedByRecord_IdentifiesReadFilter()
    {
        IFilterParameters parameters = new TestFilterParameters("active");

        parameters.ShouldBeOfType<TestFilterParameters>();
    }

    [Fact(DisplayName = "With expression copies filter parameters")]
    public void WithExpression_WhenFilterValuesAreChanged_CopiesFilterParameters()
    {
        var parameters = new TestFilterParameters("active");

        var updated = parameters with
        {
            Status = "inactive"
        };

        updated.Status.ShouldBe("inactive");
    }

    private sealed record TestFilterParameters(string Status) : IFilterParameters;
}
