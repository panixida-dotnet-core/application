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

    [Fact(DisplayName = "With expression copies the derived read model")]
    public void WithExpression_WhenReadModelIsCopied_ReturnsDerivedCopy()
    {
        var readModel = new TestReadModel(Guid.NewGuid());
        var updatedId = Guid.NewGuid();

        var result = readModel with
        {
            Id = updatedId
        };

        result.Id.ShouldBe(updatedId);
    }

    private sealed record TestReadModel(Guid Id) : ReadModel;
}
