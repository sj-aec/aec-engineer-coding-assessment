using Xunit;

namespace TransactionExecution.Tests;

public sealed class TransactionalChangeExecutorTests
{
    [Fact]
    public void Commits_once_after_all_changes_succeed()
    {
        var model = new FakeModel(new Dictionary<(string, string), string?>
        {
            [("E-001", "Mark")] = "A"
        });
        var transaction = new FakeTransaction();
        var executor = new TransactionalChangeExecutor(
            model, new FakeTransactionFactory(transaction));

        var result = executor.Execute(
            [new ParameterChange("E-001", "Mark", "A", "B")]);

        Assert.Equal(ExecutionStatus.Succeeded, result.Status);
        Assert.Equal(1, result.AppliedCount);
        Assert.Equal("B", model.Values[("E-001", "Mark")]);
        Assert.Equal(1, transaction.CommitCount);
        Assert.Equal(0, transaction.RollbackCount);
        Assert.True(transaction.IsDisposed);
    }

    [Fact]
    public void Rolls_back_when_plan_is_stale()
    {
        var model = new FakeModel(new Dictionary<(string, string), string?>
        {
            [("E-001", "Mark")] = "changed-by-another-user"
        });
        var transaction = new FakeTransaction();
        var executor = new TransactionalChangeExecutor(
            model, new FakeTransactionFactory(transaction));

        var result = executor.Execute(
            [new ParameterChange("E-001", "Mark", "A", "B")]);

        Assert.Equal(ExecutionStatus.Failed, result.Status);
        Assert.Equal(0, transaction.CommitCount);
        Assert.Equal(1, transaction.RollbackCount);
        Assert.True(transaction.IsDisposed);
    }

    private sealed class FakeModel(
        Dictionary<(string Element, string Parameter), string?> values)
        : IModelGateway
    {
        public Dictionary<(string Element, string Parameter), string?> Values { get; } = values;

        public string? GetParameter(string elementId, string parameterName) =>
            Values[(elementId, parameterName)];

        public void SetParameter(string elementId, string parameterName, string newValue) =>
            Values[(elementId, parameterName)] = newValue;
    }

    private sealed class FakeTransactionFactory(FakeTransaction transaction)
        : IModelTransactionFactory
    {
        public IModelTransaction Begin(string name) => transaction;
    }

    private sealed class FakeTransaction : IModelTransaction
    {
        public int CommitCount { get; private set; }
        public int RollbackCount { get; private set; }
        public bool IsDisposed { get; private set; }

        public void Commit() => CommitCount++;
        public void Rollback() => RollbackCount++;
        public void Dispose() => IsDisposed = true;
    }
}
