using JobSeeker.Domain.Models;

namespace JobSeeker.Application.Interfaces;

public interface IResumeTextExtractor
{
    Task<ResumeDocument> ExtractAsync(
        Stream content,
        string fileName,
        string? contentType,
        CancellationToken cancellationToken);
}