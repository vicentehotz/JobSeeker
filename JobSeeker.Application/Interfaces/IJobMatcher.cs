using JobSeeker.Application.Contracts;
using JobSeeker.Domain.Models;

namespace JobSeeker.Application.Interfaces;

public interface IJobMatcher
{
    Task<JobRankingResult> RankAsync(
        ResumeDocument resume,
        IReadOnlyList<JobListing> jobs,
        CancellationToken cancellationToken);
}