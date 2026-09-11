using PANiXiDA.Core.Application.Querying.Limiting;

namespace PANiXiDA.Core.Application.UnitTests.Querying.Limiting;

public sealed class LimitParametersTests
{
    [Fact(DisplayName = "Limit defaults to twenty when no value is supplied")]
    public void Constructor_WhenLimitIsOmitted_DefaultsToTwenty()
    {
        var parameters = new LimitParameters();

        parameters.Limit.ShouldBe(20);
    }
}
