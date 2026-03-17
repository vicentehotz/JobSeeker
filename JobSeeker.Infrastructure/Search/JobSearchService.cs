using JobSeeker.Application.Contracts;
using JobSeeker.Application.Interfaces;
using JobSeeker.Domain.Models;
using JobSeeker.Infrastructure.Matching;

namespace JobSeeker.Infrastructure.Search;

public sealed class JobSearchService(
    IResumeTextExtractor resumeTextExtractor,
    IJobFeedReader jobFeedReader,
    IJobMatcher jobMatcher) : IJobSearchService
{
    private readonly IResumeTextExtractor _resumeTextExtractor = resumeTextExtractor;
    private readonly IJobFeedReader _jobFeedReader = jobFeedReader;
    private readonly IJobMatcher _jobMatcher = jobMatcher;

    public async Task<JobSearchResponse> SearchAsync(
        Stream content,
        string fileName,
        string? contentType,
        CancellationToken cancellationToken)
    {
        var resume = await _resumeTextExtractor.ExtractAsync(content, fileName, contentType, cancellationToken);
        var feedResult = await _jobFeedReader.ReadAsync(cancellationToken);

        var deduplicatedJobs = Deduplicate(feedResult.Listings);
        if (deduplicatedJobs.Count == 0)
        {
            if (feedResult.Warnings.Count > 0)
            {
                return new JobSearchResponse(
                    "error",
                    "We could not reach the configured job sources right now. Please try again shortly.",
                    0,
                    0,
                    0,
                    feedResult.Warnings,
                    []);
            }

            return new JobSearchResponse(
                "empty",
                "The configured job feeds are available, but there are no postings to rank right now.",
                0,
                0,
                0,
                [],
                []);
        }

        var shortlistedJobs = deduplicatedJobs
            .Select(job => new
            {
                Job = job,
                Score = KeywordMatchScorer.Score(resume, job, out _)
            })
            .Where(candidate => candidate.Score > 0)
            .OrderByDescending(candidate => candidate.Score)
            .Take(12)
            .Select(candidate => candidate.Job)
            .ToList();

        if (shortlistedJobs.Count == 0)
        {
            return new JobSearchResponse(
                "empty",
                "We found live jobs, but none of them matched your resume strongly enough yet.",
                deduplicatedJobs.Count,
                0,
                0,
                feedResult.Warnings,
                []);
        }

        var rankingResult = await _jobMatcher.RankAsync(resume, shortlistedJobs, cancellationToken);
        var warnings = feedResult.Warnings
            .Concat(rankingResult.Warnings)
            .Distinct()
            .ToList();

        if (rankingResult.Results.Count == 0)
        {
            return new JobSearchResponse(
                "empty",
                "We could not produce ranked matches from the current job feeds.",
                deduplicatedJobs.Count,
                shortlistedJobs.Count,
                0,
                warnings,
                []);
        }

        var message = rankingResult.Warnings.Count == 0
            ? "We found the strongest job matches for your resume."
            : "We found the strongest job matches for your resume using fallback ranking while the AI service is unavailable.";

        return new JobSearchResponse(
            "success",
            message,
            deduplicatedJobs.Count,
            shortlistedJobs.Count,
            rankingResult.Results.Count,
            warnings,
            rankingResult.Results);
    }

    private static IReadOnlyList<JobListing> Deduplicate(IReadOnlyList<JobListing> listings)
    {
        var results = new List<JobListing>();
        var seenKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var listing in listings)
        {
            var deduplicationKey = BuildDeduplicationKey(listing);
            if (seenKeys.Add(deduplicationKey))
            {
                results.Add(listing);
            }
        }

        return results;
    }

    private static string BuildDeduplicationKey(JobListing listing)
    {
        if (Uri.TryCreate(listing.DestinationUrl, UriKind.Absolute, out var uri))
        {
            return $"{uri.Host}{uri.AbsolutePath}".ToLowerInvariant();
        }

        return $"{listing.SourceName}|{listing.Title}".ToLowerInvariant();
    }
}