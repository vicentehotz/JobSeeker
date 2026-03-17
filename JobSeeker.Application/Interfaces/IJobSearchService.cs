using JobSeeker.Application.Contracts;

namespace JobSeeker.Application.Interfaces;

public interface IJobSearchService
{
    Task<JobSearchResponse> SearchAsync(
        Stream content,
        string fileName,
        string? contentType,
        CancellationToken cancellationToken);
}