namespace LargeModelValidation;

public sealed record ModelElement(
    string ElementId,
    string SourceModel,
    string Category,
    IReadOnlyDictionary<string, string?> Parameters);

public sealed record ValidationRule(
    string Category,
    string ParameterName,
    bool Required,
    IReadOnlySet<string>? AllowedValues = null);

public enum ValidationIssueCode
{
    MissingRequiredParameter,
    ValueNotAllowed
}

public sealed record ValidationIssue(
    string ElementId,
    string SourceModel,
    string ParameterName,
    ValidationIssueCode Code,
    string? ActualValue);

public sealed record ValidationProgress(long ProcessedElements);
