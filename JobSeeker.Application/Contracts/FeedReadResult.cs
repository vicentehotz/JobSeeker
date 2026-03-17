using JobSeeker.Domain.Models;

namespace JobSeeker.Application.Contracts;

public sealed record FeedReadResult(
    IReadOnlyList<JobListing> Listings,
    IReadOnlyList<string> Warnings);