using JobSeeker.Domain.Models;

namespace JobSeeker.Application.Contracts;

public sealed record JobSearchResponse(
    string Status,
    string Message,
    int TotalFetched,
    int TotalConsidered,
    int TotalReturned,
    IReadOnlyList<string> Warnings,
    IReadOnlyList<MatchResult> Results);