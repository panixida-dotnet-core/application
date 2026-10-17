using PANiXiDA.Core.Application.Persistence.Errors;

namespace PANiXiDA.Core.Application.UnitTests.Persistence.Errors;

public sealed class ConcurrencyExceptionTests
{
    [Fact(DisplayName = "ConcurrencyException provides a stable default message")]
    public void Constructor_WhenCalledWithoutArguments_UsesDefaultMessage()
    {
        var exception = new ConcurrencyException();

        exception.Message.ShouldBe("The entity was modified or deleted by another operation.");
        exception.InnerException.ShouldBeNull();
    }

    [Fact(DisplayName = "ConcurrencyException preserves a custom diagnostic message")]
    public void Constructor_WhenGivenMessage_PreservesMessage()
    {
        const string message = "The expected order version no longer exists.";

        var exception = new ConcurrencyException(message);

        exception.Message.ShouldBe(message);
        exception.InnerException.ShouldBeNull();
    }

    [Fact(DisplayName = "ConcurrencyException preserves the original provider exception")]
    public void Constructor_WhenGivenInnerException_PreservesInstance()
    {
        const string message = "The order could not be updated.";
        var providerException = new InvalidOperationException("Provider diagnostics.");

        var exception = new ConcurrencyException(message, providerException);

        exception.Message.ShouldBe(message);
        exception.InnerException.ShouldBeSameAs(providerException);
    }

    [Fact(DisplayName = "ConcurrencyException accepts a null inner exception")]
    public void Constructor_WhenInnerExceptionIsNull_PreservesMessage()
    {
        var exception = new ConcurrencyException("Version mismatch.", null);

        exception.Message.ShouldBe("Version mismatch.");
        exception.InnerException.ShouldBeNull();
    }

    [Theory(DisplayName = "ConcurrencyException rejects a null message in both message constructors")]
    [InlineData(false)]
    [InlineData(true)]
    public void Constructor_WhenMessageIsNull_ThrowsArgumentNullException(bool includeInnerException)
    {
        var innerException = new InvalidOperationException("Provider diagnostics.");

        var exception = Should.Throw<ArgumentNullException>(() =>
            includeInnerException
                ? new ConcurrencyException(null!, innerException)
                : new ConcurrencyException(null!));

        exception.ParamName.ShouldBe("message");
    }

    [Theory(DisplayName = "ConcurrencyException rejects blank messages in both message constructors")]
    [InlineData("", false)]
    [InlineData(" ", false)]
    [InlineData("\t\r\n", false)]
    [InlineData("", true)]
    [InlineData(" ", true)]
    [InlineData("\t\r\n", true)]
    public void Constructor_WhenMessageIsBlank_ThrowsArgumentException(string message, bool includeInnerException)
    {
        var innerException = new InvalidOperationException("Provider diagnostics.");

        var exception = Should.Throw<ArgumentException>(() =>
            includeInnerException
                ? new ConcurrencyException(message, innerException)
                : new ConcurrencyException(message));

        exception.ParamName.ShouldBe("message");
    }
}
