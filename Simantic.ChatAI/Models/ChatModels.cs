namespace Simantic.ChatAI.Models;
public class ProviderInfo
{
    public required string Id { get; set; }
    public required string DisplayName { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsOnline { get; set; }
    public string? ModelId { get; set; }
    public Dictionary<string, object?> Attributes { get; set; } = new();
}
public class ChatMessage
{
    public required string Role { get; set; }
    public required string Content { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public TokenUsage? TokenUsage { get; set; }
}
public class ChatResponse
{
    public required string Content { get; set; }
    public required string Provider { get; set; }
    public string? ModelId { get; set; }
    public TokenUsage? TokenUsage { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public bool IsStreamed { get; set; }
}
public class ChatResponseChunk
{
    public string? Content { get; set; }
    public bool IsComplete { get; set; }
    public TokenUsage? TokenUsage { get; set; }
    public string? Provider { get; set; }
    public string? ModelId { get; set; }
}
public class TokenUsage
{
    public int? InputTokens { get; set; }
    public int? OutputTokens { get; set; }
    public int? TotalTokens { get; set; }
    public static TokenUsage? FromOpenAIUsage(OpenAI.Chat.ChatTokenUsage? usage)
    {
        if (usage == null) return null;
        return new TokenUsage
        {
            InputTokens = usage.InputTokenCount,
            OutputTokens = usage.OutputTokenCount,
            TotalTokens = usage.TotalTokenCount
        };
    }
    public override string ToString()
    {
        if (InputTokens.HasValue && OutputTokens.HasValue && TotalTokens.HasValue)
        {
            return $"Input: {InputTokens}, Output: {OutputTokens}, Total: {TotalTokens}";
        }
        if (TotalTokens.HasValue)
        {
            return $"Total: {TotalTokens}";
        }
        return "Token usage not available";
    }
}
