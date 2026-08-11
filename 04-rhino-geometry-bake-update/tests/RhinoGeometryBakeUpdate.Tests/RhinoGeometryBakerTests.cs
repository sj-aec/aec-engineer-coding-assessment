using Xunit;

namespace RhinoGeometryBakeUpdate.Tests;

public sealed class RhinoGeometryBakerTests
{
    [Fact]
    public void Bakes_new_object_and_commits()
    {
        var document = new FakeDocument();
        var generated = CreateGenerated("panel-001");

        var result = new RhinoGeometryBaker(document).BakeOrUpdate([generated]);

        Assert.Equal(BakeStatus.Succeeded, result.Status);
        Assert.Equal(BakeAction.Added, Assert.Single(result.Items).Action);
        Assert.Equal(1, document.AddCount);
        Assert.Equal(1, document.Undo.CommitCount);
        Assert.True(document.Undo.IsDisposed);
    }

    [Fact]
    public void Updates_existing_managed_object()
    {
        var objectId = Guid.NewGuid();
        var document = new FakeDocument();
        document.Existing["panel-001"] =
            [new DocumentObject(objectId, "panel-001", ManagedByBaker: true)];

        var result = new RhinoGeometryBaker(document)
            .BakeOrUpdate([CreateGenerated("panel-001")]);

        var item = Assert.Single(result.Items);
        Assert.Equal(BakeAction.Updated, item.Action);
        Assert.Equal(objectId, item.ObjectId);
        Assert.Equal(1, document.ReplaceCount);
    }

    [Fact]
    public void Does_not_overwrite_user_object_with_same_source_id()
    {
        var document = new FakeDocument();
        document.Existing["panel-001"] =
            [new DocumentObject(Guid.NewGuid(), "panel-001", ManagedByBaker: false)];

        var result = new RhinoGeometryBaker(document)
            .BakeOrUpdate([CreateGenerated("panel-001")]);

        Assert.Equal(BakeStatus.Failed, result.Status);
        Assert.Equal(0, result.AppliedCount);
        Assert.Equal(0, document.ReplaceCount);
        Assert.Equal(0, document.BeginUndoCount);
    }

    private static GeneratedObject CreateGenerated(string sourceId) =>
        new(
            sourceId,
            "AEC::Panels",
            $"Panel {sourceId}",
            "opaque-geometry",
            new Dictionary<string, string> { ["Level"] = "02" });

    private sealed class FakeDocument : IRhinoDocumentGateway
    {
        public Dictionary<string, IReadOnlyList<DocumentObject>> Existing { get; } = [];
        public FakeUndoRecord Undo { get; } = new();
        public int AddCount { get; private set; }
        public int ReplaceCount { get; private set; }
        public int BeginUndoCount { get; private set; }

        public void VerifyWriteAccess()
        {
        }

        public IReadOnlyList<DocumentObject> FindBySourceId(string sourceId) =>
            Existing.TryGetValue(sourceId, out var objects) ? objects : [];

        public void EnsureLayer(string layerPath)
        {
        }

        public Guid AddObject(string geometryPayload, ObjectAttributes attributes)
        {
            AddCount++;
            return Guid.NewGuid();
        }

        public void ReplaceObject(
            Guid objectId,
            string geometryPayload,
            ObjectAttributes attributes) => ReplaceCount++;

        public IRhinoUndoRecord BeginUndoRecord(string name)
        {
            BeginUndoCount++;
            return Undo;
        }
    }

    private sealed class FakeUndoRecord : IRhinoUndoRecord
    {
        public int CommitCount { get; private set; }
        public int RollbackCount { get; private set; }
        public bool IsDisposed { get; private set; }

        public void Commit() => CommitCount++;
        public void Rollback() => RollbackCount++;
        public void Dispose() => IsDisposed = true;
    }
}
