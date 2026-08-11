namespace RhinoGeometryBakeUpdate;

public sealed record GeneratedObject(
    string SourceId,
    string LayerPath,
    string Name,
    string GeometryPayload,
    IReadOnlyDictionary<string, string> Metadata);

public sealed record DocumentObject(
    Guid ObjectId,
    string SourceId,
    bool ManagedByBaker);

public sealed record ObjectAttributes(
    string Name,
    string LayerPath,
    IReadOnlyDictionary<string, string> UserStrings);

public enum BakeAction
{
    Added,
    Updated
}

public sealed record BakeItemResult(
    string SourceId,
    Guid ObjectId,
    BakeAction Action);

public enum BakeStatus
{
    Succeeded,
    Failed,
    Cancelled
}

public sealed record BakeResult(
    BakeStatus Status,
    int AppliedCount,
    IReadOnlyList<BakeItemResult> Items,
    string? ErrorMessage = null,
    Exception? Error = null,
    Exception? RollbackError = null)
{
    public static BakeResult Success(IReadOnlyList<BakeItemResult> items) =>
        new(BakeStatus.Succeeded, items.Count, items);
}
