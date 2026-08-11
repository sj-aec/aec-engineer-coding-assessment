namespace RhinoGeometryBakeUpdate;

public interface IRhinoDocumentGateway
{
    void VerifyWriteAccess();

    IReadOnlyList<DocumentObject> FindBySourceId(string sourceId);

    void EnsureLayer(string layerPath);

    Guid AddObject(string geometryPayload, ObjectAttributes attributes);

    void ReplaceObject(
        Guid objectId,
        string geometryPayload,
        ObjectAttributes attributes);

    IRhinoUndoRecord BeginUndoRecord(string name);
}

public interface IRhinoUndoRecord : IDisposable
{
    void Commit();

    void Rollback();
}
