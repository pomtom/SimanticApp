namespace Simantic.ChatAI.Models;

/// <summary>
/// Information about an LLM provider
/// </summary>
public class ProviderInfo
{
    /// <summary>
    /// Unique identifier for the provider
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// Display name for the provider
    /// </summary>
    public required string DisplayName { get; set; }

    /// <summary>
    /// Whether the provider is currently available
    /// </summary>
    public bool IsAvailable { get; set; }

    /// <summary>
    /// Whether the provider requires internet connection
    /// </summary>
    public bool IsOnline { get; set; }

    /// <summary>
    /// Current model being used
    /// </summary>
    public string? ModelId { get; set; }

    /// <summary>
    /// Additional provider attributes
    /// </summary>
    public Dictionary<string, object?> Attributes { get; set; } = new();
}

/// <summary>
/// Represents a chat message
/// </summary>
public class ChatMessage
{
    /// <summary>
    /// Role of the message sender
    /// </summary>
    public required string Role { get; set; }

    /// <summary>
    /// Content of the message
    /// </summary>
    public required string Content { get; set; }

    /// <summary>
    /// Timestamp when the message was created
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Token usage information (if available)
    /// </summary>
    public TokenUsage? TokenUsage { get; set; }
}

/// <summary>
/// Represents a complete chat response
/// </summary>
public class ChatResponse
{
    /// <summary>
    /// The response content
    /// </summary>
    public required string Content { get; set; }

    /// <summary>
    /// Provider that generated the response
    /// </summary>
    public required string Provider { get; set; }

    /// <summary>
    /// Model used to generate the response
    /// </summary>
    public string? ModelId { get; set; }

    /// <summary>
    /// Token usage information
    /// </summary>
    public TokenUsage? TokenUsage { get; set; }

    /// <summary>
    /// Response generation timestamp
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Whether the response was streamed
    /// </summary>
    public bool IsStreamed { get; set; }
}

/// <summary>
/// Represents a chunk of streaming chat response
/// </summary>
public class ChatResponseChunk
{
    /// <summary>
    /// Content chunk
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// Whether this is the final chunk
    /// </summary>
    public bool IsComplete { get; set; }

    /// <summary>
    /// Token usage (usually only available on final chunk)
    /// </summary>
    public TokenUsage? TokenUsage { get; set; }

    /// <summary>
    /// Provider that generated this chunk
    /// </summary>
    public string? Provider { get; set; }

    /// <summary>
    /// Model used to generate this chunk
    /// </summary>
    public string? ModelId { get; set; }
}

/// <summary>
/// Token usage information
/// </summary>
public class TokenUsage
{
    /// <summary>
    /// Number of input tokens
    /// </summary>
    public int? InputTokens { get; set; }

    /// <summary>
    /// Number of output tokens
    /// </summary>
    public int? OutputTokens { get; set; }

    /// <summary>
    /// Total number of tokens
    /// </summary>
    public int? TotalTokens { get; set; }

    /// <summary>
    /// Creates a token usage instance from OpenAI token usage
    /// </summary>
    /// <param name="usage">OpenAI token usage</param>
    /// <returns>TokenUsage instance</returns>
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

    /// <summary>
    /// String representation of token usage
    /// </summary>
    /// <returns>Formatted token usage string</returns>
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