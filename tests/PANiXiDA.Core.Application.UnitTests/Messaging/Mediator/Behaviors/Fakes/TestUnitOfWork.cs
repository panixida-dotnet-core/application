using PANiXiDA.Core.Application.Persistence;

namespace PANiXiDA.Core.Application.UnitTests.Messaging.Mediator.Behaviors.Fakes;

internal sealed class TestUnitOfWork : IUnitOfWork
{
    public int SaveChangesCalls { get; private set; }
    public int ExecuteInTransactionCalls { get; private set; }
    public int BeginTransactionCalls { get; private set; }
    public int CommitTransactionCalls { get; private set; }
    public int RollbackTransactionCalls { get; private set; }
    public int DisposeTransactionCalls { get; private set; }
    public CancellationToken LastCancellationToken { get; private set; }
    public bool HasActiveTransaction { get; set; }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveChangesCalls++;
        LastCancellationToken = cancellationToken;

        return Task.CompletedTask;
    }

    public async Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> action,
        CancellationToken cancellationToken)
    {
        ExecuteInTransactionCalls++;
        LastCancellationToken = cancellationToken;
        await action(cancellationToken);
    }

    public Task BeginTransactionAsync(CancellationToken cancellationToken)
    {
        BeginTransactionCalls++;
        LastCancellationToken = cancellationToken;
        HasActiveTransaction = true;

        return Task.CompletedTask;
    }

    public Task CommitTransactionAsync(CancellationToken cancellationToken)
    {
        CommitTransactionCalls++;
        LastCancellationToken = cancellationToken;

        return Task.CompletedTask;
    }

    public Task RollbackTransactionAsync(CancellationToken cancellationToken)
    {
        RollbackTransactionCalls++;
        LastCancellationToken = cancellationToken;

        return Task.CompletedTask;
    }

    public ValueTask DisposeTransactionAsync()
    {
        DisposeTransactionCalls++;
        HasActiveTransaction = false;

        return ValueTask.CompletedTask;
    }
}
