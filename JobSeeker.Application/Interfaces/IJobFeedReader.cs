using JobSeeker.Application.Contracts;

namespace JobSeeker.Application.Interfaces;

public interface IJobFeedReader
{
    Task<FeedReadResult> ReadAsync(CancellationToken cancellationToken);
}