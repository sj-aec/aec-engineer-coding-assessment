namespace RhinoGeometryBakeUpdate;

public sealed class RhinoGeometryBaker(IRhinoDocumentGateway document)
{
    public BakeResult BakeOrUpdate(
        IReadOnlyCollection<GeneratedObject> objects,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(objects);

        if (objects.Count == 0)
        {
            return BakeResult.Success([]);
        }

        document.VerifyWriteAccess();
        using var undoRecord = document.BeginUndoRecord("Bake generated geometry");
        var results = new List<BakeItemResult>(objects.Count);

        try
        {
            foreach (var generated in objects)
            {
                cancellationToken.ThrowIfCancellationRequested();

                document.EnsureLayer(generated.LayerPath);
                var attributes = new ObjectAttributes(
                    generated.Name,
                    generated.LayerPath,
                    new Dictionary<string, string>(generated.Metadata));
                var existing = document.FindBySourceId(generated.SourceId);

                if (existing.Count == 0)
                {
                    var objectId = document.AddObject(
                        generated.GeometryPayload,
                        attributes);
                    results.Add(new BakeItemResult(
                        generated.SourceId,
                        objectId,
                        BakeAction.Added));
                }
                else
                {
                    document.ReplaceObject(
                        existing[0].ObjectId,
                        generated.GeometryPayload,
                        attributes);
                    results.Add(new BakeItemResult(
                        generated.SourceId,
                        existing[0].ObjectId,
                        BakeAction.Updated));
                }
            }

            undoRecord.Commit();
            return BakeResult.Success(results);
        }
        catch (OperationCanceledException exception)
        {
            undoRecord.Rollback();
            return new BakeResult(
                BakeStatus.Cancelled,
                results.Count,
                results,
                "Bake was cancelled.",
                exception);
        }
        catch (Exception exception)
        {
            undoRecord.Rollback();
            return new BakeResult(
                BakeStatus.Failed,
                results.Count,
                results,
                exception.Message,
                exception);
        }
    }
}
