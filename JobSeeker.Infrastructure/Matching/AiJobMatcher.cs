using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using JobSeeker.Application.Contracts;
using JobSeeker.Application.Interfaces;
using JobSeeker.Domain.Models;
using JobSeeker.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JobSeeker.Infrastructure.Matching;

public sealed class AiJobMatcher(
    HttpClient httpClient,
    IOptions<AiOptions> options,
    ILogger<AiJobMatcher> logger) : IJobMatcher
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly AiOptions _options = options.Value;
    private readonly ILogger<AiJobMatcher> _logger = logger;

    public async Task<JobRankingResult> RankAsync(
        ResumeDocument resume,
        IReadOnlyList<JobListing> jobs,
        CancellationToken cancellationToken)
    {
        var batch = jobs.Take(Math.Max(1, _options.MaxJobsPerRequest)).ToList();
        if (batch.Count == 0)
        {
            return new JobRankingResult([], []);
        }

        if (!_options.Enabled || string.IsNullOrWhiteSpace(_options.ApiKey) || string.IsNullOrWhiteSpace(_options.Endpoint))
        {
            return BuildFallback(resume, batch, "AI ranking is not configured yet, so the current results use heuristic matching.");
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, _options.Endpoint);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (string.Equals(_options.ApiKeyHeaderName, "Authorization", StringComparison.OrdinalIgnoreCase))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
            }
            else
            {
                request.Headers.TryAddWithoutValidation(_options.ApiKeyHeaderName, _options.ApiKey);
            }

            request.Content = JsonContent.Create(new
            {
                model = _options.Model,
                temperature = _options.Temperature,
                response_format = new { type = "json_object" },
                messages = new object[]
                {
                    new
                    {
                        role = "system",
                        content = "You rank job listings for a candidate. Return strict JSON with this shape only: {\"results\":[{\"jobId\":\"string\",\"score\":0,\"reasoning\":\"string\"}]}. Scores must be integers from 0 to 100 and results must be sorted from best to worst."
                    },
                    new
                    {
                        role = "user",
                        content = BuildPrompt(resume, batch)
                    }
                }
            });

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "AI ranking request failed with status {StatusCode}: {ResponseBody}",
                    response.StatusCode,
                    responseBody);

                return BuildFallback(resume, batch, "AI ranking is temporarily unavailable, so the current results use heuristic matching.");
            }

            var rankedResults = ParseRankedResults(responseBody, batch);
            if (rankedResults.Count == 0)
            {
                return BuildFallback(resume, batch, "AI ranking returned an invalid response, so the current results use heuristic matching.");
            }

            return new JobRankingResult(rankedResults, []);
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or JsonException)
        {
            _logger.LogWarning(exception, "AI ranking failed and will fall back to heuristic matching.");
            return BuildFallback(resume, batch, "AI ranking is temporarily unavailable, so the current results use heuristic matching.");
        }
    }

    private static string BuildPrompt(ResumeDocument resume, IReadOnlyList<JobListing> jobs)
    {
        var resumeExcerpt = resume.ExtractedText.Length > 3000
            ? resume.ExtractedText[..3000]
            : resume.ExtractedText;

        var payload = jobs.Select(job => new
        {
            jobId = job.Id,
            title = job.Title,
            source = job.SourceName,
            summary = job.Summary,
            description = job.Description,
            url = job.DestinationUrl
        });

        return $"Resume excerpt:\n{resumeExcerpt}\n\nCandidate keywords:\n{string.Join(", ", resume.Keywords.Take(20))}\n\nRank these jobs and return JSON only:\n{JsonSerializer.Serialize(payload)}";
    }

    private static IReadOnlyList<MatchResult> ParseRankedResults(string responseBody, IReadOnlyList<JobListing> jobs)
    {
        using var responseDocument = JsonDocument.Parse(responseBody);
        var content = ExtractContent(responseDocument.RootElement);
        if (string.IsNullOrWhiteSpace(content))
        {
            return [];
        }

        var jsonPayload = ExtractJsonPayload(content);
        using var rankingDocument = JsonDocument.Parse(jsonPayload);

        if (!rankingDocument.RootElement.TryGetProperty("results", out var resultsElement) || resultsElement.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var jobsById = jobs.ToDictionary(job => job.Id, StringComparer.OrdinalIgnoreCase);
        var parsedResults = new List<MatchResult>();

        foreach (var result in resultsElement.EnumerateArray())
        {
            if (!result.TryGetProperty("jobId", out var jobIdElement))
            {
                continue;
            }

            var jobId = jobIdElement.GetString();
            if (string.IsNullOrWhiteSpace(jobId) || !jobsById.TryGetValue(jobId, out var job))
            {
                continue;
            }

            var score = result.TryGetProperty("score", out var scoreElement) && scoreElement.TryGetDouble(out var parsedScore)
                ? Math.Clamp(Math.Round(parsedScore, 1, MidpointRounding.AwayFromZero), 0d, 100d)
                : 0d;

            var reasoning = result.TryGetProperty("reasoning", out var reasoningElement)
                ? reasoningElement.GetString() ?? string.Empty
                : string.Empty;

            parsedResults.Add(new MatchResult(job, 0, score, string.IsNullOrWhiteSpace(reasoning)
                ? "The AI ranked this job highly based on the overlap between the resume and the job description."
                : reasoning.Trim()));
        }

        return parsedResults
            .OrderByDescending(result => result.Score)
            .Select((result, index) => result with { Rank = index + 1 })
            .ToList();
    }

    private static string? ExtractContent(JsonElement rootElement)
    {
        if (!rootElement.TryGetProperty("choices", out var choicesElement) || choicesElement.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        var firstChoice = choicesElement.EnumerateArray().FirstOrDefault();
        if (!firstChoice.TryGetProperty("message", out var messageElement))
        {
            return null;
        }

        if (!messageElement.TryGetProperty("content", out var contentElement))
        {
            return null;
        }

        return contentElement.ValueKind switch
        {
            JsonValueKind.String => contentElement.GetString(),
            JsonValueKind.Array => string.Join(
                string.Empty,
                contentElement
                    .EnumerateArray()
                    .Where(item => item.TryGetProperty("text", out _))
                    .Select(item => item.GetProperty("text").GetString() ?? string.Empty)),
            _ => null
        };
    }

    private static string ExtractJsonPayload(string content)
    {
        var start = content.IndexOf('{');
        var end = content.LastIndexOf('}');

        return start >= 0 && end > start
            ? content[start..(end + 1)]
            : content;
    }

    private static JobRankingResult BuildFallback(
        ResumeDocument resume,
        IReadOnlyList<JobListing> jobs,
        string warning)
    {
        var results = jobs
            .Select(job =>
            {
                var score = KeywordMatchScorer.Score(resume, job, out var matches);
                return new MatchResult(job, 0, score, KeywordMatchScorer.BuildReasoning(matches, job));
            })
            .OrderByDescending(result => result.Score)
            .Select((result, index) => result with { Rank = index + 1 })
            .ToList();

        return new JobRankingResult(results, [warning]);
    }
}