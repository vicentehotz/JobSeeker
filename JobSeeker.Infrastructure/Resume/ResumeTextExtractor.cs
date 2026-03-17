using System.IO.Compression;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using JobSeeker.Application.Interfaces;
using JobSeeker.Domain.Models;
using JobSeeker.Infrastructure.Matching;
using JobSeeker.Infrastructure.Options;
using Microsoft.Extensions.Options;
using UglyToad.PdfPig;

namespace JobSeeker.Infrastructure.Resume;

public sealed partial class ResumeTextExtractor(IOptions<ResumeUploadOptions> options) : IResumeTextExtractor
{
    private readonly ResumeUploadOptions _options = options.Value;

    public async Task<ResumeDocument> ExtractAsync(
        Stream content,
        string fileName,
        string? contentType,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new InvalidDataException("Please upload a PDF or DOCX resume.");
        }

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!_options.AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidDataException("Unsupported file format. Please upload a PDF or DOCX resume.");
        }

        await using var memoryStream = new MemoryStream();
        await content.CopyToAsync(memoryStream, cancellationToken);

        if (memoryStream.Length == 0)
        {
            throw new InvalidDataException("The uploaded resume is empty. Please choose a valid PDF or DOCX file.");
        }

        var maxFileSizeBytes = _options.MaxFileSizeInMegabytes * 1024L * 1024L;
        if (memoryStream.Length > maxFileSizeBytes)
        {
            throw new InvalidDataException($"The resume is too large. Please upload a file smaller than {_options.MaxFileSizeInMegabytes} MB.");
        }

        memoryStream.Position = 0;

        var extractedText = extension switch
        {
            ".pdf" => ExtractPdfText(memoryStream),
            ".docx" => ExtractDocxText(memoryStream),
            _ => string.Empty
        };

        extractedText = WhitespacePattern().Replace(extractedText, " ").Trim();
        if (string.IsNullOrWhiteSpace(extractedText))
        {
            throw new InvalidDataException("The resume could not be read. Please try a different PDF or DOCX file.");
        }

        return new ResumeDocument(
            fileName,
            contentType ?? "application/octet-stream",
            extractedText,
            KeywordMatchScorer.ExtractKeywords(extractedText));
    }

    private static string ExtractPdfText(Stream stream)
    {
        stream.Position = 0;

        using var document = PdfDocument.Open(stream);
        return string.Join(' ', document.GetPages().Select(page => page.Text));
    }

    private static string ExtractDocxText(Stream stream)
    {
        stream.Position = 0;

        try
        {
            using var document = WordprocessingDocument.Open(stream, false);
            return string.Join(
                ' ',
                document.MainDocumentPart?
                    .Document
                    .Body?
                    .Descendants<Text>()
                    .Select(text => text.Text)
                    .Where(text => !string.IsNullOrWhiteSpace(text))
                ?? []);
        }
        catch (InvalidDataException exception) when (exception.InnerException is InvalidDataException or IOException)
        {
            throw new InvalidDataException("The DOCX resume appears to be corrupted. Please upload a valid file.", exception);
        }
        catch (OpenXmlPackageException exception)
        {
            throw new InvalidDataException("The DOCX resume appears to be corrupted. Please upload a valid file.", exception);
        }
    }

    [GeneratedRegex(@"\s+", RegexOptions.Compiled)]
    private static partial Regex WhitespacePattern();
}