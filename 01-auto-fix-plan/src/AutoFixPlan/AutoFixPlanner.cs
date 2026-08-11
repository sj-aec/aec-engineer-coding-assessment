namespace AutoFixPlan;

public sealed class AutoFixPlanner
{
    public AutoFixPlanResult CreatePlan(
        IReadOnlyCollection<ElementSnapshot> elements,
        IReadOnlyCollection<ValidationIssue> issues,
        AutoFixPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(elements);
        ArgumentNullException.ThrowIfNull(issues);
        ArgumentNullException.ThrowIfNull(policy);

        // This baseline handles the simple happy path, but has not yet applied
        // production safety policy, writeability checks, or target-level conflict handling.
        _ = policy;
        var elementsById = elements.ToDictionary(
            element => element.ElementId,
            StringComparer.Ordinal);
        var changes = new List<ParameterChange>();
        var skipped = new List<SkippedChange>();

        foreach (var issue in issues)
        {
            if (string.IsNullOrEmpty(issue.SuggestedValue))
            {
                skipped.Add(new SkippedChange(
                    issue.ElementId,
                    issue.ParameterName,
                    issue.RuleId,
                    SkipReason.NotActionable));
                continue;
            }

            if (!elementsById.TryGetValue(issue.ElementId, out var element))
            {
                skipped.Add(new SkippedChange(
                    issue.ElementId,
                    issue.ParameterName,
                    issue.RuleId,
                    SkipReason.UnknownElement));
                continue;
            }

            if (!element.Parameters.TryGetValue(issue.ParameterName, out var parameter))
            {
                skipped.Add(new SkippedChange(
                    issue.ElementId,
                    issue.ParameterName,
                    issue.RuleId,
                    SkipReason.UnknownParameter));
                continue;
            }

            if (string.Equals(
                    parameter.Value,
                    issue.SuggestedValue,
                    StringComparison.Ordinal))
            {
                skipped.Add(new SkippedChange(
                    issue.ElementId,
                    issue.ParameterName,
                    issue.RuleId,
                    SkipReason.AlreadyCorrect));
                continue;
            }

            changes.Add(new ParameterChange(
                issue.ElementId,
                issue.ParameterName,
                parameter.Value,
                issue.SuggestedValue,
                [issue.RuleId]));
        }

        return new AutoFixPlanResult(changes, skipped);
    }
}
