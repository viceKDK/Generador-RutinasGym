using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace GymRoutineGenerator.Infrastructure.Exercises;

internal static class ExerciseNameNormalizer
{
    private static readonly Regex NonAlphanumeric = new("[^a-z0-9]+", RegexOptions.Compiled);

    public static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var ch in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (category != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(char.ToLowerInvariant(ch));
            }
        }

        var collapsed = NonAlphanumeric.Replace(builder.ToString(), " ").Trim();
        return collapsed;
    }
}
