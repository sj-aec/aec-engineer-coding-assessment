namespace AutoFixPlan;

public enum ModelKind
{
    Primary,
    Linked
}

public sealed record ParameterSnapshot(
    string? Value,
    bool IsReadOnly = false);

public sealed record ElementSnapshot(
    string ElementId,
    string SourceModel,
    ModelKind ModelKind,
    bool IsEditable,
    IReadOnlyDictionary<string, ParameterSnapshot> Parameters);

public sealed record ValidationIssue(
    string ElementId,
    string ParameterName,
    string Code,
    string RuleId,
    double Confidence,
    string? SuggestedValue = null);

public sealed record AutoFixPolicy(
    double MinimumConfidence,
    IReadOnlySet<string> AllowedIssueCodes);

public sealed record ParameterChange(
    string ElementId,
    string ParameterName,
    string? ExpectedOldValue,
    string NewValue,
    IReadOnlyList<string> RuleIds);

public enum SkipReason
{
    NotActionable,
    InvalidConfidence,
    IssueCodeNotAllowed,
    BelowMinimumConfidence,
    UnknownElement,
    LinkedModel,
    ElementReadOnly,
    UnknownParameter,
    ParameterReadOnly,
    AlreadyCorrect,
    DuplicateSuggestion,
    ConflictingSuggestions
}

public sealed record SkippedChange(
    string ElementId,
    string ParameterName,
    string RuleId,
    SkipReason Reason,
    string? Detail = null);

public sealed record AutoFixPlanResult(
    IReadOnlyList<ParameterChange> Changes,
    IReadOnlyList<SkippedChange> Skipped);
