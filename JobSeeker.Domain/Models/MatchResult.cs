namespace JobSeeker.Domain.Models;

public sealed record MatchResult(JobListing Job, int Rank, double Score, string Reasoning);