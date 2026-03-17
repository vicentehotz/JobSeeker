namespace JobSeeker.Infrastructure.Options;

public sealed class AiOptions
{
    public const string SectionName = "Ai";

    public bool Enabled { get; set; }

    public string Endpoint { get; set; } = "https://api.openai.com/v1/chat/completions";

    public string Model { get; set; } = "gpt-4.1-mini";

    public string ApiKey { get; set; } = string.Empty;

    public string ApiKeyHeaderName { get; set; } = "Authorization";

    public int MaxJobsPerRequest { get; set; } = 8;

    public double Temperature { get; set; } = 0.2;

    public int RequestTimeoutSeconds { get; set; } = 45;

    public bool AllowHeuristicFallback { get; set; } = true;
}