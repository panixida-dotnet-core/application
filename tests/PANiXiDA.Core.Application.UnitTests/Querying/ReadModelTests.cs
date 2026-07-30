using PANiXiDA.Core.Application.Querying;

namespace PANiXiDA.Core.Application.UnitTests.Querying;

public sealed class ReadModelTests
{
    [Fact(DisplayName = "ReadModel can be used as a read result base type")]
    public void ReadModel_WhenDerived_CanBeUsedAsBaseType()
    {
        ReadModel readModel = new TestReadModel(Guid.NewGuid());

        readModel.ShouldBeOfType<TestReadModel>();
    }

    private sealed record TestReadModel(Guid Id) : ReadModel;
}
