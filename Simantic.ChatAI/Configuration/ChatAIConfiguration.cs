namespace Simantic.ChatAI.Configuration;
public class ChatAIConfiguration
{
    public string DefaultProvider { get; set; } = "AzureOpenAI";
    public int MaxChatHistoryMessages { get; set; } = 10;
    public string DefaultSystemMessage { get; set; } = "You are a friendly AI Assistant that answers in a friendly manner";
    public float DefaultTemperature { get; set; } = 0.9f;
    public int DefaultMaxTokens { get; set; } = 1500;
    public AzureOpenAIConfiguration? AzureOpenAI { get; set; }
    public OpenAIConfiguration? OpenAI { get; set; }
    public HuggingFaceConfiguration? HuggingFace { get; set; }
    public OllamaConfiguration? Ollama { get; set; }
    public LMStudioConfiguration? LMStudio { get; set; }
    public AzureAIInferenceConfiguration? AzureAIInference { get; set; }
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
    public Dictionary<string, LlmProviderConfiguration> GetEnabledProviders()
    {
        return GetAllProviders()
            .Where(p => p.Value.IsEnabled && p.Value.IsValid())
            .ToDictionary(p => p.Key, p => p.Value);
    }
    public Dictionary<string, LlmProviderConfiguration> GetOnlineProviders()
    {
        return GetEnabledProviders()
            .Where(p => p.Value.IsOnline)
            .ToDictionary(p => p.Key, p => p.Value);
    }
    public Dictionary<string, LlmProviderConfiguration> GetOfflineProviders()
    {
        return GetEnabledProviders()
            .Where(p => !p.Value.IsOnline)
            .ToDictionary(p => p.Key, p => p.Value);
    }
}
