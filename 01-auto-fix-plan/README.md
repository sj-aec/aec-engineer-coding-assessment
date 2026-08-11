# 01 — Safe BIM Auto-fix Planning

## Context

A BIM validator has produced parameter issues and suggested fixes. Before any Revit adapter writes to a model, the application must decide which suggestions are safe to automate and produce an auditable change plan.

Production inputs are not uniformly writable: elements may come from linked models, an element or parameter may be read-only, rules have different confidence levels, and some issue types are not approved for automatic repair.

This exercise only creates a plan. It must **not mutate the model**.

The starter contains a happy-path baseline that has not received a production safety review. It intentionally accepts unsafe suggestions. Run the tests, identify the unsafe paths, and make focused fixes rather than rewriting the module.

## Diagnose and fix

```csharp
AutoFixPlanResult AutoFixPlanner.CreatePlan(
    IReadOnlyCollection<ElementSnapshot> elements,
    IReadOnlyCollection<ValidationIssue> issues,
    AutoFixPolicy policy)
```

Domain types are in `src/AutoFixPlan/Domain.cs`.

## Input and policy validation

1. `policy.MinimumConfidence` must be finite and between `0` and `1`, inclusive. Invalid policy is a caller error and should throw `ArgumentOutOfRangeException` before planning.
2. Allowed issue codes use ordinal, case-sensitive comparison.
3. An issue with a confidence outside `[0, 1]`, NaN, or Infinity is skipped with `InvalidConfidence` so the malformed upstream result remains auditable.
4. Inputs and their dictionaries must not be mutated.

## Issue-level eligibility

Evaluate these conditions before conflict detection:

1. A missing or empty `SuggestedValue` is `NotActionable`.
2. An issue code not present in `policy.AllowedIssueCodes` is `IssueCodeNotAllowed`.
3. Confidence below `policy.MinimumConfidence` is `BelowMinimumConfidence`.
4. An unknown element is `UnknownElement`.
5. An element from `ModelKind.Linked` is `LinkedModel`.
6. An element with `IsEditable=false` is `ElementReadOnly`.
7. An unknown parameter is `UnknownParameter`.
8. A parameter with `IsReadOnly=true` is `ParameterReadOnly`.
9. A current value already equal to the suggestion using ordinal comparison is `AlreadyCorrect`.

Only suggestions that pass all issue-level checks participate in deduplication and conflict detection. For example, a rejected low-confidence suggestion must not conflict with an otherwise valid suggestion.

## Deduplication and conflicts

For eligible suggestions targeting the same `(ElementId, ParameterName)`:

1. Identical suggested values produce one `ParameterChange`.
2. The change records all contributing `RuleId` values once, sorted ordinally.
3. Repeated identical suggestions are recorded as `DuplicateSuggestion` audit entries.
4. Different eligible suggested values are `ConflictingSuggestions`; do not create a change for that target.
5. A conflict audit entry must identify the competing RuleIds or values in `Detail`.

Each planned change contains `ExpectedOldValue`. This is a precondition for the later transaction executor and must be the value observed in the supplied snapshot.

## Deterministic output

1. Order changes by `ElementId`, then `ParameterName`, using ordinal comparison.
2. Order skipped entries by `ElementId`, `ParameterName`, `RuleId`, and `Reason`.
3. Planning the same logical input in a different collection order must produce the same result.

## Working order

1. Run the existing tests and record the baseline.
2. Identify paths that could modify linked, read-only, low-confidence, or unapproved data.
3. Add regression tests for the safety rules you will fix.
4. Separate issue-level eligibility from target-level conflict handling.
5. Run focused tests and document any remaining policy assumptions.

## Acceptance

```bash
dotnet test
```

Prioritize tests for:

- linked-model elements;
- read-only elements and parameters;
- allowed issue codes and confidence thresholds;
- malformed confidence values;
- a rejected suggestion that must not create a false conflict;
- duplicate and conflicting eligible suggestions;
- deterministic output under reordered input;
- `ExpectedOldValue` and sorted RuleIds;
- input immutability.

## Out of scope

- Revit API calls
- Creating missing parameters
- Applying changes to a model
- Unit conversion or geometry processing
- UI

## Exercise-specific priorities

- Safety policy and writeability boundaries: 30%
- Conflict, deduplication, and audit behavior: 30%
- Determinism and preconditions: 20%
- Regression tests and design explanation: 20%

## Submission notes

Record the unsafe baseline behavior you found, your eligibility/conflict ordering, and any remaining assumptions about policy ownership or upstream validation.
