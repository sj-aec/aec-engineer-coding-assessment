namespace TransactionExecution;

public sealed record ParameterChange(
    string ElementId,
    string ParameterName,
    string? ExpectedOldValue,
    string NewValue);

public enum ExecutionStatus
{
    Succeeded,
    Failed,
    Cancelled
}

public sealed record ExecutionResult(
    ExecutionStatus Status,
    int AppliedCount,
    string? ErrorMessage = null,
    Exception? Error = null,
    Exception? RollbackError = null)
{
    public static ExecutionResult Success(int appliedCount) =>
        new(ExecutionStatus.Succeeded, appliedCount);
}
