using JobSeeker.Domain.Models;

namespace JobSeeker.Application.Contracts;

public sealed record JobRankingResult(
    IReadOnlyList<MatchResult> Results,
    IReadOnlyList<string> Warnings);