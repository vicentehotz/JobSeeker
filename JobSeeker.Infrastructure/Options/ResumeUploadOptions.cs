namespace JobSeeker.Infrastructure.Options;

public sealed class ResumeUploadOptions
{
    public const string SectionName = "ResumeUpload";

    public int MaxFileSizeInMegabytes { get; set; } = 5;

    public List<string> AllowedExtensions { get; set; } = [".pdf", ".docx"];
}