namespace JobSeeker.Domain.Models;

public sealed record JobListing(
    string Id,
    string Title,
    string SourceName,
    string SourceUrl,
    string DestinationUrl,
    string Summary,
    string Description,
    DateTimeOffset? PublishedAt);