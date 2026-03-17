using System.Net;
using System.Security.Cryptography;
using System.ServiceModel.Syndication;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using JobSeeker.Application.Contracts;
using JobSeeker.Application.Interfaces;
using JobSeeker.Domain.Models;
using JobSeeker.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JobSeeker.Infrastructure.Feed;

public sealed partial class RssJobFeedReader(
    HttpClient httpClient,
    IOptions<FeedCollectionOptions> options,
    ILogger<RssJobFeedReader> logger) : IJobFeedReader
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly FeedCollectionOptions _options = options.Value;
    private readonly ILogger<RssJobFeedReader> _logger = logger;

    public async Task<FeedReadResult> ReadAsync(CancellationToken cancellationToken)
    {
        if (_options.Sources.Count == 0)
        {
            return new FeedReadResult([], ["No job feeds are configured."]);
        }

        var tasks = _options.Sources.Select(source => ReadSourceAsync(source, cancellationToken));
        var results = await Task.WhenAll(tasks);

        var listings = results.SelectMany(result => result.Listings).ToList();
        var warnings = results.SelectMany(result => result.Warnings).Distinct().ToList();

        return new FeedReadResult(listings, warnings);
    }

    private async Task<FeedReadResult> ReadSourceAsync(
        FeedSourceOptions source,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(source.Url) || string.IsNullOrWhiteSpace(source.Name))
        {
            return new FeedReadResult([], ["One of the configured feed sources is missing a name or URL."]);
        }

        try
        {
            await using var stream = await _httpClient.GetStreamAsync(source.Url, cancellationToken);
            using var reader = XmlReader.Create(stream, new XmlReaderSettings { Async = false });
            var feed = SyndicationFeed.Load(reader);

            if (feed is null)
            {
                return new FeedReadResult([], [$"{source.Name} did not return a readable feed."]);
            }

            var listings = feed.Items
                .Take(_options.MaxItemsPerFeed)
                .Select(item => MapListing(item, source))
                .Where(item => item is not null)
                .Cast<JobListing>()
                .ToList();

            return new FeedReadResult(listings, []);
        }
        catch (Exception exception) when (exception is HttpRequestException or XmlException or InvalidOperationException)
        {
            _logger.LogWarning(exception, "Failed to read feed {FeedName} from {FeedUrl}", source.Name, source.Url);
            return new FeedReadResult([], [$"{source.Name} is temporarily unavailable."]);
        }
    }

    private static JobListing? MapListing(SyndicationItem item, FeedSourceOptions source)
    {
        var destinationUrl = item.Links.FirstOrDefault(link => link.Uri is not null)?.Uri?.ToString();
        var title = CleanText(item.Title?.Text);
        var summary = CleanText(item.Summary?.Text);
        var description = !string.IsNullOrWhiteSpace(summary)
            ? summary
            : CleanText(item.Content switch
            {
                TextSyndicationContent textContent => textContent.Text,
                _ => string.Empty
            });

        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(destinationUrl))
        {
            return null;
        }

        return new JobListing(
            CreateStableId(source.Name, destinationUrl, title),
            title,
            source.Name,
            source.Url,
            destinationUrl,
            summary,
            description,
            item.PublishDate == DateTimeOffset.MinValue ? null : item.PublishDate);
    }

    private static string CleanText(string? rawText)
    {
        if (string.IsNullOrWhiteSpace(rawText))
        {
            return string.Empty;
        }

        var withoutTags = HtmlTagPattern().Replace(rawText, " ");
        var decoded = WebUtility.HtmlDecode(withoutTags);
        return WhitespacePattern().Replace(decoded, " ").Trim();
    }

    private static string CreateStableId(string sourceName, string destinationUrl, string title)
    {
        var seed = $"{sourceName}|{destinationUrl}|{title}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(seed));
        return Convert.ToHexString(hash)[..12].ToLowerInvariant();
    }

    [GeneratedRegex("<[^>]+>", RegexOptions.Compiled)]
    private static partial Regex HtmlTagPattern();

    [GeneratedRegex(@"\s+", RegexOptions.Compiled)]
    private static partial Regex WhitespacePattern();
}