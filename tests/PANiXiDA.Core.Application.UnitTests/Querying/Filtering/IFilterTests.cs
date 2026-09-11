using PANiXiDA.Core.Application.Querying.Filtering;

namespace PANiXiDA.Core.Application.UnitTests.Querying.Filtering;

public sealed class IFilterTests
{
    [Fact(DisplayName = "IFilter can identify an application query filter record")]
    public void IFilter_WhenImplementedByRecord_IdentifiesQueryFilter()
    {
        IFilter filter = new TestFilter("active");

        filter.ShouldBeOfType<TestFilter>();
    }

    [Fact(DisplayName = "With expression copies filters")]
    public void WithExpression_WhenFilterValuesAreChanged_CopiesFilter()
    {
        var filter = new TestFilter("active");

        var updated = filter with
        {
            Status = "inactive"
        };

        updated.Status.ShouldBe("inactive");
    }

    private sealed record TestFilter(string Status) : IFilter;
}
