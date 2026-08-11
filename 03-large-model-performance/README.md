# 03 — Large-model Validation

## Context

A validator must process elements from a primary model and linked models:

- up to 1,000,000 elements;
- up to 2,000 validation rules;
- elements read asynchronously and sequentially from a host API;
- a host reader that is not thread-safe.

The starter baseline has useful output behavior and passing basic tests, but scans every rule for every element: `O(E × R)`. Preserve ordering, streaming, cancellation, and progress behavior while removing this production bottleneck. Do not load all elements or issues into memory.

## Optimize

```csharp
IAsyncEnumerable<ValidationIssue> LargeModelValidator.ValidateAsync(
    IAsyncEnumerable<ModelElement> elements,
    IReadOnlyCollection<ValidationRule> rules,
    IProgress<ValidationProgress>? progress = null,
    CancellationToken cancellationToken = default)
```

Domain types are in `src/LargeModelValidation`.

## Validation behavior

For every rule matching an element's `Category`:

1. When `Required=true` and the parameter is absent or empty, emit `MissingRequiredParameter`.
2. When a value exists and is not in `AllowedValues`, emit `ValueNotAllowed`.
3. Compare categories, parameter names, and allowed values using ordinal, case-sensitive comparison.
4. Preserve element input order. For one element, preserve rule input order.
5. One element may produce multiple issues.

## Production constraints

1. Preprocess rules at most once and group them by Category.
2. Consume elements as a stream. Do not call `ToList`, `ToArray`, or enumerate the source twice.
3. Do not use `Task.Run` or enumerate the host source in parallel; treat it as non-thread-safe.
4. Respect `CancellationToken`, including while reading the source.
5. Report progress every 10,000 processed elements. At completion, report the final count only if that count has not already been reported. An empty source reports `0`.
6. Memory use should depend primarily on rule count and issues waiting to be consumed, not total element count.
7. `SourceModel` is report metadata. Do not filter linked-model elements.

This exercise does not require a sophisticated algorithm. A straightforward Category lookup and careful streaming implementation are sufficient.

## Performance scenario

`tools/PerformanceScenario` provides generated input and timing. Only about 0.1% of elements produce issues so issue allocation does not dominate rule lookup.

Record a comparable baseline and optimized result with:

```bash
dotnet run --project tools/PerformanceScenario --configuration Release -- 100000 2000
```

After optimization, run the target scale:

```bash
dotnet run --project tools/PerformanceScenario --configuration Release -- 1000000 2000
```

If the baseline does not finish in a reasonable time, stop it and record the observed size, elapsed time, and bottleneck. You are not required to wait for an obviously inefficient run.

Do not add a machine-specific timing threshold to the tests. Record:

- machine context;
- element and rule counts;
- elapsed time;
- issue count;
- observed memory and scaling characteristics.

The expected shape is close to `O(R + E × Rc)`, where `Rc` is the number of rules for the current Category. This describes the production bottleneck; it is not an algorithms puzzle.

## Working order

1. Run existing tests and a smaller performance scenario.
2. Explain the bottleneck and intended production improvement.
3. Add tests protecting ordering, single enumeration, cancellation, and progress.
4. Make a focused optimization and record comparable before/after evidence.
5. Explain why the host source was not enumerated in parallel.

## Acceptance

```bash
dotnet test
```

Add coverage for:

- required parameters and disallowed values;
- a Category with no rules;
- linked-model elements;
- output ordering;
- single enumeration of the source;
- cancellation;
- the 10,000 boundary and final progress notification;
- many unrelated Categories without relying on fragile millisecond assertions.

## Out of scope

- Parallel Revit API access
- Automatic fixes
- Collecting every issue in memory
- UI

## Exercise-specific priorities

- Preserving output behavior: 25%
- Streaming and large-model scaling: 30%
- Performance measurement and evidence: 20%
- Cancellation, progress, and tests: 25%

## Submission notes

Record the design, before/after performance evidence, and any AI recommendations you modified or rejected.
