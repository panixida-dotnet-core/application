using PANiXiDA.Core.Application.Querying;

namespace PANiXiDA.Core.Application.UnitTests.Querying;

public sealed class IReadModelTests
{
    [Fact(DisplayName = "IReadModel can identify a read result record")]
    public void IReadModel_WhenImplementedByRecord_IdentifiesReadResult()
    {
        IReadModel readModel = new TestReadModel(Guid.NewGuid());

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

    private sealed record TestReadModel(Guid Id) : IReadModel;
}
