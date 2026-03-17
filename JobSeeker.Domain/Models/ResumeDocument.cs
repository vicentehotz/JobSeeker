namespace JobSeeker.Domain.Models;

public sealed record ResumeDocument(
    string FileName,
    string ContentType,
    string ExtractedText,
    IReadOnlyList<string> Keywords);