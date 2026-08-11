# 04 — Rhino Geometry Bake and Update

## Context

A Grasshopper or computational-design workflow has produced a collection of geometry results. A plug-in must Bake those results into Rhino and update the objects it previously Baked on later runs. It must not continually create duplicates or overwrite objects created manually by a user.

This exercise does not require geometry algorithms or a Rhino installation. `GeometryPayload` is an opaque placeholder for geometry data. The starter interfaces represent the RhinoCommon and Rhino Document boundary. The focus is idempotent Bake/update behavior, Layers, User Strings, Undo, failure recovery, and testability.

The starter handles a simple Add/Update happy path but has not received a production review. Run the tests and inspect the implementation before making focused fixes. Do not rewrite the entire module.

## Diagnose and fix

```csharp
BakeResult RhinoGeometryBaker.BakeOrUpdate(
    IReadOnlyCollection<GeneratedObject> objects,
    CancellationToken cancellationToken = default)
```

Types are in `src/RhinoGeometryBakeUpdate`.

## Pre-Bake validation

Validate the entire input before the first Rhino Document write:

1. `SourceId`, `LayerPath`, `Name`, and `GeometryPayload` must not be null, empty, or whitespace.
2. `SourceId` comparison is ordinal and case-sensitive. IDs must be unique within one batch.
3. Metadata keys must not be empty.
4. Input cannot overwrite these reserved User Strings:
   - `AecBake.SourceId`
   - `AecBake.Owner`
5. An empty batch succeeds without opening an Undo record or accessing the Rhino Document.
6. Validation failure returns `Failed` with `AppliedCount=0` and performs no Document writes.

## Rhino Document preflight

After pure input validation:

1. Call `VerifyWriteAccess()` once to make the Rhino UI-thread/document-context boundary explicit.
2. Before opening an Undo record, use `FindBySourceId` for every target.
3. No existing object means the operation will Add a new object.
4. Exactly one object managed by this baker means the operation will Replace it.
5. A user-created object, or multiple existing objects for one SourceId, rejects the entire batch before writes begin.
6. Cancellation during preflight must not open an Undo record.

One lookup per SourceId is acceptable. No indexing algorithm or geometry comparison is required.

## Bake and update rules

1. Perform all Add/Replace operations inside one Undo record so the batch is all-or-nothing.
2. Call `EnsureLayer` at most once per distinct LayerPath in a batch.
3. Preserve input Metadata and add these User Strings:

   ```text
   AecBake.SourceId = <SourceId>
   AecBake.Owner    = AecCodingAssessment
   ```

4. Update a managed object with `ReplaceObject`, preserving its `ObjectId`.
5. Bake a new object with `AddObject`.
6. Preserve input order in result items and distinguish `Added` from `Updated`.
7. Check cancellation before every write.
8. Call `Commit()` exactly once after every operation succeeds.
9. On an Add, Replace, Commit, or cancellation failure, stop processing and attempt `Rollback()`.
10. A rollback failure must not replace the original error. Preserve both in `BakeResult`.
11. Always dispose the Undo record.

## Working order

1. Run the existing tests and record the baseline.
2. Identify paths that may overwrite user objects, duplicate Baked objects, or leave partial results.
3. Add characterization or regression tests for the behavior you will fix.
4. Implement pure validation and Document preflight before changing the write flow.
5. Run focused tests and record assumptions that still require verification in Rhino.

## Acceptance

```bash
dotnet test
```

Prioritize tests for:

- an empty batch;
- first Bake and idempotent later update;
- duplicate SourceIds in one batch;
- a user-object conflict;
- duplicate SourceIds already in the Document;
- Layer deduplication;
- preservation of Metadata and addition of reserved User Strings;
- cancellation during preflight and writing;
- Add, Replace, and Commit failures;
- an additional Rollback failure;
- Undo record disposal on every path.

## Rhino/Grasshopper discussion prompts (optional)

Briefly explain:

- how a real adapter would find and identify Baked objects with Rhino object User Strings;
- which operations must run on the Rhino UI thread;
- how a Rhino Undo record differs from a database transaction and how an adapter could compensate for partial writes;
- how to avoid duplicate Bake during Grasshopper recompute, document close, and repeated component execution;
- how to report progress and support cancellation for a large Bake batch.

## Out of scope

- Parsing or comparing real Brep, Mesh, or Curve geometry
- Geometry algorithms
- Rhino command registration, UI, or `.rhp` packaging
- Parallel Rhino Document writes
- Matching existing objects by geometric similarity

## Exercise-specific priorities

- Idempotent Bake and user-object protection: 30%
- Rhino Document/Undo lifecycle and recovery: 30%
- Preflight, cancellation, and User Strings: 20%
- Regression tests and Rhino adapter explanation: 20%

## Submission notes

Record the defects you found, completed scope, assumptions about a real Rhino adapter, and risks or next steps for incomplete work.
