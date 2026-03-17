using JobSeeker.Application.Interfaces;
using JobSeeker.Infrastructure.Feed;
using JobSeeker.Infrastructure.Matching;
using JobSeeker.Infrastructure.Options;
using JobSeeker.Infrastructure.Resume;
using JobSeeker.Infrastructure.Search;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace JobSeeker.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddJobSearchInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<FeedCollectionOptions>(configuration.GetSection(FeedCollectionOptions.SectionName));
        services.Configure<ResumeUploadOptions>(configuration.GetSection(ResumeUploadOptions.SectionName));
        services.Configure<AiOptions>(configuration.GetSection(AiOptions.SectionName));

        services.AddHttpClient<IJobFeedReader, RssJobFeedReader>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(20);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("JobSeeker/1.0");
        });

        services.AddHttpClient<IJobMatcher, AiJobMatcher>((serviceProvider, client) =>
        {
            var aiOptions = serviceProvider.GetRequiredService<IOptions<AiOptions>>().Value;
            client.Timeout = TimeSpan.FromSeconds(Math.Max(5, aiOptions.RequestTimeoutSeconds));
        });

        services.AddScoped<IResumeTextExtractor, ResumeTextExtractor>();
        services.AddScoped<IJobSearchService, JobSearchService>();

        return services;
    }
}