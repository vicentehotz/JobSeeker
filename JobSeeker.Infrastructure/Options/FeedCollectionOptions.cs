namespace JobSeeker.Infrastructure.Options;

public sealed class FeedCollectionOptions
{
    public const string SectionName = "JobFeeds";

    public int MaxItemsPerFeed { get; set; } = 20;

    public List<FeedSourceOptions> Sources { get; set; } = [];
}

public sealed class FeedSourceOptions
{
    public string Name { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;
}