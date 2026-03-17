using System.Text.RegularExpressions;
using JobSeeker.Domain.Models;

namespace JobSeeker.Infrastructure.Matching;

internal static partial class KeywordMatchScorer
{
    private static readonly HashSet<string> StopWords =
    [
        "about", "after", "again", "also", "and", "been", "being", "between", "build", "candidate",
        "company", "could", "from", "have", "into", "just", "more", "must", "only", "role",
        "skills", "that", "their", "there", "they", "this", "with", "work", "years", "your"
    ];

    public static IReadOnlyList<string> ExtractKeywords(string text, int maxKeywords = 40)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return [];
        }

        return TokenPattern()
            .Matches(text.ToLowerInvariant())
            .Select(match => match.Value.Trim())
            .Where(token => token.Length >= 3 && !StopWords.Contains(token))
            .GroupBy(token => token)
            .OrderByDescending(group => group.Count())
            .ThenByDescending(group => group.Key.Length)
            .Take(maxKeywords)
            .Select(group => group.Key)
            .ToList();
    }

    public static double Score(ResumeDocument resume, JobListing job, out IReadOnlyList<string> matches)
    {
        var searchableContent = string.Join(
            ' ',
            new[]
            {
                job.Title,
                job.Summary,
                job.Description,
                job.SourceName
            }.Where(value => !string.IsNullOrWhiteSpace(value)))
            .ToLowerInvariant();

        var title = job.Title.ToLowerInvariant();

        var matchedKeywords = resume.Keywords
            .Where(keyword => searchableContent.Contains(keyword, StringComparison.Ordinal))
            .Distinct(StringComparer.Ordinal)
            .Take(6)
            .ToList();

        var titleMatches = matchedKeywords.Count(keyword => title.Contains(keyword, StringComparison.Ordinal));
        var remoteBonus = searchableContent.Contains("remote", StringComparison.Ordinal)
            && resume.ExtractedText.Contains("remote", StringComparison.OrdinalIgnoreCase)
            ? 6d
            : 0d;

        matches = matchedKeywords;

        if (matchedKeywords.Count == 0)
        {
            return 0;
        }

        var score = (matchedKeywords.Count * 12d) + (titleMatches * 8d) + remoteBonus;
        return Math.Min(100d, Math.Round(score, 1, MidpointRounding.AwayFromZero));
    }

    public static string BuildReasoning(IReadOnlyList<string> matches, JobListing job)
    {
        if (matches.Count == 0)
        {
            return $"This role looks relevant based on the overall overlap between your resume and the {job.SourceName} posting.";
        }

        var highlightedMatches = string.Join(", ", matches.Take(3));
        return $"Strong overlap around {highlightedMatches}, with the clearest signal coming from the job title and summary.";
    }

    [GeneratedRegex(@"[a-z0-9+#\.\-]{3,}", RegexOptions.Compiled)]
    private static partial Regex TokenPattern();
}