namespace Simantic.ChatAI.Configuration;

/// <summary>
/// Main application configuration that contains all provider configurations
/// </summary>
public class ChatAIConfiguration
{
    /// <summary>
    /// The default provider to use when none is specified
    /// </summary>
    public string DefaultProvider { get; set; } = "AzureOpenAI";

    /// <summary>
    /// Maximum number of messages to keep in chat history
    /// </summary>
    public int MaxChatHistoryMessages { get; set; } = 10;

    /// <summary>
    /// Default system message for all providers
    /// </summary>
    public string DefaultSystemMessage { get; set; } = "You are a friendly AI Assistant that answers in a friendly manner";

    /// <summary>
    /// Default temperature setting
    /// </summary>
    public float DefaultTemperature { get; set; } = 0.9f;

    /// <summary>
    /// Default maximum tokens
    /// </summary>
    public int DefaultMaxTokens { get; set; } = 1500;

    /// <summary>
    /// Azure OpenAI configuration
    /// </summary>
    public AzureOpenAIConfiguration? AzureOpenAI { get; set; }

    /// <summary>
    /// OpenAI configuration
    /// </summary>
    public OpenAIConfiguration? OpenAI { get; set; }

    /// <summary>
    /// HuggingFace configuration
    /// </summary>
    public HuggingFaceConfiguration? HuggingFace { get; set; }

    /// <summary>
    /// Ollama configuration
    /// </summary>
    public OllamaConfiguration? Ollama { get; set; }

    /// <summary>
    /// LM Studio configuration
    /// </summary>
    public LMStudioConfiguration? LMStudio { get; set; }

    /// <summary>
    /// Azure AI Inference configuration
    /// </summary>
    public AzureAIInferenceConfiguration? AzureAIInference { get; set; }

    /// <summary>
    /// Gets all configured providers
    /// </summary>
    /// <returns>Dictionary of provider configurations</returns>
    public Dictionary<string, LlmProviderConfiguration> GetAllProviders()
    {
        var providers = new Dictionary<string, LlmProviderConfiguration>();

        if (AzureOpenAI != null)
            providers["AzureOpenAI"] = AzureOpenAI;

        if (OpenAI != null)
            providers["OpenAI"] = OpenAI;

        if (HuggingFace != null)
            providers["HuggingFace"] = HuggingFace;

        if (Ollama != null)
            providers["Ollama"] = Ollama;

        if (LMStudio != null)
            providers["LMStudio"] = LMStudio;

        if (AzureAIInference != null)
            providers["AzureAIInference"] = AzureAIInference;

        return providers;
    }

    /// <summary>
    /// Gets all enabled providers
    /// </summary>
    /// <returns>Dictionary of enabled provider configurations</returns>
    public Dictionary<string, LlmProviderConfiguration> GetEnabledProviders()
    {
        return GetAllProviders()
            .Where(p => p.Value.IsEnabled && p.Value.IsValid())
            .ToDictionary(p => p.Key, p => p.Value);
    }

    /// <summary>
    /// Gets online providers only
    /// </summary>
    /// <returns>Dictionary of online provider configurations</returns>
    public Dictionary<string, LlmProviderConfiguration> GetOnlineProviders()
    {
        return GetEnabledProviders()
            .Where(p => p.Value.IsOnline)
            .ToDictionary(p => p.Key, p => p.Value);
    }

    /// <summary>
    /// Gets offline/local providers only
    /// </summary>
    /// <returns>Dictionary of offline provider configurations</returns>
    public Dictionary<string, LlmProviderConfiguration> GetOfflineProviders()
    {
        return GetEnabledProviders()
            .Where(p => !p.Value.IsOnline)
            .ToDictionary(p => p.Key, p => p.Value);
    }
}