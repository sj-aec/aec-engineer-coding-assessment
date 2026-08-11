using Xunit;

namespace AutoFixPlan.Tests;

public sealed class AutoFixPlannerTests
{
    private static readonly AutoFixPolicy Policy = new(
        MinimumConfidence: 0.8,
        AllowedIssueCodes: new HashSet<string>(StringComparer.Ordinal)
        {
            "InvalidBoolean",
            "InvalidValue"
        });

    [Fact]
    public void Creates_change_for_eligible_primary_model_issue()
    {
        var element = CreateElement(
            "E-001",
            ModelKind.Primary,
            isEditable: true,
            new ParameterSnapshot("yes"));
        var issue = CreateIssue(
            "E-001", "InvalidBoolean", "RULE-BOOL", 0.95, "true");

        var result = new AutoFixPlanner().CreatePlan([element], [issue], Policy);

        var change = Assert.Single(result.Changes);
        Assert.Equal("yes", change.ExpectedOldValue);
        Assert.Equal("true", change.NewValue);
        Assert.Equal(["RULE-BOOL"], change.RuleIds);
        Assert.Empty(result.Skipped);
    }

    [Fact]
    public void Does_not_plan_change_for_linked_model_element()
    {
        var element = CreateElement(
            "E-001",
            ModelKind.Linked,
            isEditable: true,
            new ParameterSnapshot("yes"));
        var issue = CreateIssue(
            "E-001", "InvalidBoolean", "RULE-BOOL", 0.95, "true");

        var result = new AutoFixPlanner().CreatePlan([element], [issue], Policy);

        Assert.Empty(result.Changes);
        Assert.Contains(result.Skipped,
            skipped => skipped.Reason == SkipReason.LinkedModel);
    }

    [Fact]
    public void Low_confidence_suggestion_does_not_conflict_with_eligible_suggestion()
    {
        var element = CreateElement(
            "E-001",
            ModelKind.Primary,
            isEditable: true,
            new ParameterSnapshot("unknown"));
        ValidationIssue[] issues =
        [
            CreateIssue("E-001", "InvalidValue", "RULE-LOW", 0.4, "1h"),
            CreateIssue("E-001", "InvalidValue", "RULE-HIGH", 0.95, "2h")
        ];

        var result = new AutoFixPlanner().CreatePlan([element], issues, Policy);

        var change = Assert.Single(result.Changes);
        Assert.Equal("2h", change.NewValue);
        Assert.Contains(result.Skipped,
            skipped => skipped.RuleId == "RULE-LOW" &&
                       skipped.Reason == SkipReason.BelowMinimumConfidence);
    }

    private static ElementSnapshot CreateElement(
        string elementId,
        ModelKind modelKind,
        bool isEditable,
        ParameterSnapshot parameter) =>
        new(
            elementId,
            modelKind == ModelKind.Primary ? "Main" : "Architecture-Link",
            modelKind,
            isEditable,
            new Dictionary<string, ParameterSnapshot>
            {
                ["IsExternal"] = parameter
            });

    private static ValidationIssue CreateIssue(
        string elementId,
        string code,
        string ruleId,
        double confidence,
        string suggestedValue) =>
        new(
            elementId,
            "IsExternal",
            code,
            ruleId,
            confidence,
            suggestedValue);
}
