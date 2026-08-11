namespace TransactionExecution;

public sealed class TransactionalChangeExecutor(
    IModelGateway model,
    IModelTransactionFactory transactions)
{
    public ExecutionResult Execute(
        IReadOnlyCollection<ParameterChange> changes,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(changes);

        if (changes.Count == 0)
        {
            return ExecutionResult.Success(0);
        }

        using var transaction = transactions.Begin("Apply parameter changes");
        var appliedCount = 0;

        try
        {
            foreach (var change in changes)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var currentValue = model.GetParameter(
                    change.ElementId,
                    change.ParameterName);

                if (!string.Equals(
                        currentValue,
                        change.ExpectedOldValue,
                        StringComparison.Ordinal))
                {
                    transaction.Rollback();
                    return new ExecutionResult(
                        ExecutionStatus.Failed,
                        appliedCount,
                        $"Stale value for {change.ElementId}.{change.ParameterName}.");
                }

                model.SetParameter(
                    change.ElementId,
                    change.ParameterName,
                    change.NewValue);
                appliedCount++;
            }

            transaction.Commit();
            return ExecutionResult.Success(appliedCount);
        }
        catch (OperationCanceledException exception)
        {
            transaction.Rollback();
            return new ExecutionResult(
                ExecutionStatus.Cancelled,
                appliedCount,
                "Execution was cancelled.",
                exception);
        }
        catch (Exception exception)
        {
            transaction.Rollback();
            return new ExecutionResult(
                ExecutionStatus.Failed,
                appliedCount,
                exception.Message,
                exception);
        }
    }
}
