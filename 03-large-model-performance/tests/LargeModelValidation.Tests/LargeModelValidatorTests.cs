using System.Runtime.CompilerServices;
using Xunit;

namespace LargeModelValidation.Tests;

public sealed class LargeModelValidatorTests
{
    [Fact]
    public async Task Reports_missing_required_parameter_and_disallowed_value()
    {
        ModelElement[] elements =
        [
            new(
                "E-001",
                "Main",
                "Wall",
                new Dictionary<string, string?> { ["FireRating"] = "4h" })
        ];
        ValidationRule[] rules =
        [
            new("Wall", "IsExternal", true),
            new("Wall", "FireRating", true, new HashSet<string> { "1h", "2h", "3h" })
        ];

        var issues = await new LargeModelValidator()
            .ValidateAsync(ToAsync(elements), rules)
            .ToListAsync();

        Assert.Collection(
            issues,
            issue => Assert.Equal(ValidationIssueCode.MissingRequiredParameter, issue.Code),
            issue => Assert.Equal(ValidationIssueCode.ValueNotAllowed, issue.Code));
    }

    [Fact]
    public async Task Includes_elements_from_linked_models()
    {
        ModelElement[] elements =
        [
            new("L-001", "Architecture-Link", "Door", new Dictionary<string, string?>())
        ];
        ValidationRule[] rules = [new("Door", "Mark", true)];

        var issues = await new LargeModelValidator()
            .ValidateAsync(ToAsync(elements), rules)
            .ToListAsync();

        Assert.Equal("Architecture-Link", Assert.Single(issues).SourceModel);
    }

    private static async IAsyncEnumerable<ModelElement> ToAsync(
        IEnumerable<ModelElement> elements,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var element in elements)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return element;
            await Task.Yield();
        }
    }
}

internal static class AsyncEnumerableTestExtensions
{
    public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> source)
    {
        var result = new List<T>();
        await foreach (var item in source)
        {
            result.Add(item);
        }

        return result;
    }
}
