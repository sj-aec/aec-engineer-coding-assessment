# 02 — Transaction Execution

## Context

The system has produced a collection of `ParameterChange` records and must now apply them through a Revit-like host API. All writes must occur inside a host transaction. The batch is **all-or-nothing**: commit only when every change succeeds; otherwise roll back.

The host is represented by interfaces, so Revit is not required.

The starter contains an implementation that handles the basic happy path but has not received a production review. It intentionally contains reliability defects. Characterize the existing behavior with tests, diagnose it, and make focused fixes rather than rewriting the module.

## Diagnose and fix

```csharp
ExecutionResult TransactionalChangeExecutor.Execute(
    IReadOnlyCollection<ParameterChange> changes,
    CancellationToken cancellationToken = default)
```

Interfaces and domain types are in `src/TransactionExecution`.

In your submission notes, identify the problems you found, their priority, and any AI recommendations you accepted, modified, or rejected.

## Required behavior

1. An empty plan succeeds without opening a transaction.
2. Before opening a transaction, reject duplicate `(ElementId, ParameterName)` targets without writing to the model.
3. Check cancellation before each change.
4. Read the live value through `IModelGateway.GetParameter` before writing.
5. The live value must match `ExpectedOldValue` using ordinal comparison. Otherwise the plan is stale and the entire batch fails.
6. Call `Commit()` exactly once after every change succeeds.
7. If a read, validation, or write fails, call `Rollback()` once and stop processing.
8. Cancellation must also roll back and return `Cancelled`.
9. Preserve host API errors in `ExecutionResult`; do not swallow them.
10. Dispose the transaction even when commit, rollback, or a host call throws.

Assume the transaction is active after `Begin()` succeeds.

## Important failure cases

- Change N may fail after changes 1 through N-1 were written; rollback is responsible for restoring consistency.
- `Commit()` may throw.
- `Rollback()` may also throw and must not replace the original failure.
- The plan may be old, so `ExpectedOldValue` cannot be trusted without a live read.

## Working order

1. Run the existing tests and record the baseline.
2. Review the implementation and list risks before rewriting anything.
3. Add characterization or regression tests for the failures you will fix.
4. Make focused fixes and run focused tests.
5. Record host transaction assumptions that remain unverified.

## Acceptance

```bash
dotnet test
```

Add coverage for:

- an empty plan;
- successful commit;
- a mid-batch write failure and rollback;
- a stale plan;
- cancellation before and during execution;
- duplicate targets;
- commit and rollback exceptions;
- transaction disposal on every path.

Use fakes or mocks. Do not connect to real BIM software.

## Out of scope

- Automatic retries
- Multiple independent transactions
- UI or progress reporting
- Change-plan generation

## Exercise-specific priorities

- Transaction, exception, and rollback semantics: 35%
- Evidence used to diagnose existing defects: 25%
- Regression tests: 20%
- Scope control and design explanation: 20%

## Submission notes

Add 3–8 lines here describing the risks you found and the precedence you chose for original errors, rollback errors, and cancellation.
