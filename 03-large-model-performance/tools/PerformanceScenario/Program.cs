using System.Diagnostics;
using System.Runtime.CompilerServices;
using LargeModelValidation;

var elementCount = args.Length > 0 ? int.Parse(args[0]) : 100_000;
var ruleCount = args.Length > 1 ? int.Parse(args[1]) : 2_000;
var rules = CreateRules(ruleCount);
var validator = new LargeModelValidator();
var issueCount = 0L;
var stopwatch = Stopwatch.StartNew();

await foreach (var _ in validator.ValidateAsync(CreateElements(elementCount), rules))
{
    issueCount++;
}

stopwatch.Stop();
Console.WriteLine($"Elements: {elementCount:N0}");
Console.WriteLine($"Rules:    {ruleCount:N0}");
Console.WriteLine($"Issues:   {issueCount:N0}");
Console.WriteLine($"Elapsed:  {stopwatch.Elapsed}");

static ValidationRule[] CreateRules(int count) =>
    Enumerable.Range(0, count)
        .Select(index => new ValidationRule(
            $"Category-{index % 100}",
            $"Parameter-{index}",
            Required: index < 100))
        .ToArray();

static async IAsyncEnumerable<ModelElement> CreateElements(
    int count,
    [EnumeratorCancellation] CancellationToken cancellationToken = default)
{
    for (var index = 0; index < count; index++)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var categoryIndex = index % 100;
        IReadOnlyDictionary<string, string?> parameters = index % 1_000 == 0
            ? new Dictionary<string, string?>()
            : new Dictionary<string, string?>
            {
                [$"Parameter-{categoryIndex}"] = "present"
            };

        yield return new ModelElement(
            $"E-{index}",
            index % 10 == 0 ? "Linked" : "Main",
            $"Category-{categoryIndex}",
            parameters);

        if (index % 10_000 == 0)
        {
            await Task.Yield();
        }
    }
}
