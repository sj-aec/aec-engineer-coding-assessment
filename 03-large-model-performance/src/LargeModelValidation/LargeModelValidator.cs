using System.Runtime.CompilerServices;

namespace LargeModelValidation;

public sealed class LargeModelValidator
{
    public async IAsyncEnumerable<ValidationIssue> ValidateAsync(
        IAsyncEnumerable<ModelElement> elements,
        IReadOnlyCollection<ValidationRule> rules,
        IProgress<ValidationProgress>? progress = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(elements);
        ArgumentNullException.ThrowIfNull(rules);

        long processedElements = 0;

        await foreach (var element in elements.WithCancellation(cancellationToken))
        {
            // This baseline is functionally useful, but scans every rule for every
            // element. Diagnose and remove the large-model bottleneck without
            // changing output ordering or streaming behavior.
            foreach (var rule in rules)
            {
                if (!string.Equals(
                        element.Category,
                        rule.Category,
                        StringComparison.Ordinal))
                {
                    continue;
                }

                element.Parameters.TryGetValue(rule.ParameterName, out var value);
                var isMissing = string.IsNullOrEmpty(value);

                if (rule.Required && isMissing)
                {
                    yield return new ValidationIssue(
                        element.ElementId,
                        element.SourceModel,
                        rule.ParameterName,
                        ValidationIssueCode.MissingRequiredParameter,
                        value);
                    continue;
                }

                if (!isMissing &&
                    rule.AllowedValues is not null &&
                    !rule.AllowedValues.Any(
                        allowed => string.Equals(allowed, value, StringComparison.Ordinal)))
                {
                    yield return new ValidationIssue(
                        element.ElementId,
                        element.SourceModel,
                        rule.ParameterName,
                        ValidationIssueCode.ValueNotAllowed,
                        value);
                }
            }

            processedElements++;
            if (processedElements % 10_000 == 0)
            {
                progress?.Report(new ValidationProgress(processedElements));
            }
        }

        if (processedElements == 0 || processedElements % 10_000 != 0)
        {
            progress?.Report(new ValidationProgress(processedElements));
        }
    }
}
