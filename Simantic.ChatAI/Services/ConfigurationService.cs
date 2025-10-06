using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AzureAIInference;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Simantic.ChatAI.Configuration;
using Simantic.ChatAI.Interfaces;

namespace Simantic.ChatAI.Services;

/// <summary>
/// Service for managing application configuration
/// </summary>
public class ConfigurationService : IConfigurationService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ConfigurationService> _logger;
    private readonly Lazy<ChatAIConfiguration> _chatAIConfig;

    public ConfigurationService(IConfiguration configuration, ILogger<ConfigurationService> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _chatAIConfig = new Lazy<ChatAIConfiguration>(LoadConfiguration);
    }

    /// <summary>
    /// Gets the current chat AI configuration
    /// </summary>
    /// <returns>Configuration instance</returns>
    public ChatAIConfiguration GetConfiguration()
    {
        return _chatAIConfig.Value;
    }

    /// <summary>
    /// Validates if a provider is properly configured
    /// </summary>
    /// <param name="providerId">Provider identifier</param>
    /// <returns>True if valid, false otherwise</returns>
    public bool IsProviderConfigured(string providerId)
    {
        try
        {
            var config = GetConfiguration();
            var providers = config.GetAllProviders();
            
            if (!providers.TryGetValue(providerId, out var providerConfig))
            {
                _logger.LogWarning("Provider {ProviderId} not found in configuration", providerId);
                return false;
            }

            var isValid = providerConfig.IsEnabled && providerConfig.IsValid();
            _logger.LogDebug("Provider {ProviderId} configuration valid: {IsValid}", providerId, isValid);
            
            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating provider configuration for {ProviderId}", providerId);
            return false;
        }
    }

    /// <summary>
    /// Gets provider-specific execution settings
    /// </summary>
    /// <param name="providerId">Provider identifier</param>
    /// <returns>Prompt execution settings</returns>
    public PromptExecutionSettings GetExecutionSettings(string providerId)
    {
        var config = GetConfiguration();
        
        return providerId.ToLowerInvariant() switch
        {
            "azureopenai" => new OpenAIPromptExecutionSettings
            {
                Temperature = config.DefaultTemperature,
                MaxTokens = config.DefaultMaxTokens,
                ChatSystemPrompt = config.DefaultSystemMessage
            },
            "openai" => new OpenAIPromptExecutionSettings
            {
                Temperature = config.DefaultTemperature,
                MaxTokens = config.DefaultMaxTokens,
                ChatSystemPrompt = config.DefaultSystemMessage
            },
            "huggingface" => new HuggingFacePromptExecutionSettings
            {
                Temperature = config.DefaultTemperature,
                MaxTokens = config.DefaultMaxTokens
            },
            "ollama" => new OllamaPromptExecutionSettings
            {
                Temperature = config.DefaultTemperature
            },
            "lmstudio" => new OpenAIPromptExecutionSettings
            {
                Temperature = config.DefaultTemperature
            },
            "azureaiinference" => new AzureAIInferencePromptExecutionSettings
            {
                Temperature = config.DefaultTemperature,
                MaxTokens = config.DefaultMaxTokens
            },
            _ => throw new NotSupportedException($"Provider '{providerId}' is not supported")
        };
    }

    private ChatAIConfiguration LoadConfiguration()
    {
        try
        {
            _logger.LogInformation("Loading ChatAI configuration");

            var chatConfig = new ChatAIConfiguration();
            
            // Load main settings
            _configuration.GetSection("ChatAI").Bind(chatConfig);

            // Load provider configurations
            LoadProviderConfigurations(chatConfig);

            _logger.LogInformation("Configuration loaded successfully. Default provider: {DefaultProvider}", chatConfig.DefaultProvider);
            
            return chatConfig;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load configuration");
            throw;
        }
    }

    private void LoadProviderConfigurations(ChatAIConfiguration chatConfig)
    {
        // Load Azure OpenAI configuration
        var azureOpenAISection = _configuration.GetSection("AzureOpenAI");
        if (azureOpenAISection.Exists())
        {
            chatConfig.AzureOpenAI = new AzureOpenAIConfiguration
            {
                ProviderId = "AzureOpenAI",
                DisplayName = "Azure OpenAI",
                Endpoint = azureOpenAISection["Endpoint"] ?? string.Empty,
                ApiKey = azureOpenAISection["ApiKey"] ?? string.Empty,
                DeploymentName = azureOpenAISection["DeploymentName"] ?? string.Empty
            };
            _logger.LogDebug("Loaded Azure OpenAI configuration");
        }

        // Load OpenAI configuration
        var openAISection = _configuration.GetSection("OpenAI");
        if (openAISection.Exists())
        {
            chatConfig.OpenAI = new OpenAIConfiguration
            {
                ProviderId = "OpenAI",
                DisplayName = "OpenAI",
                ApiKey = openAISection["ApiKey"] ?? string.Empty,
                ModelId = openAISection["ModelId"] ?? string.Empty,
                OrganizationId = openAISection["OrganizationId"]
            };
            _logger.LogDebug("Loaded OpenAI configuration");
        }

        // Load HuggingFace configuration
        var huggingFaceSection = _configuration.GetSection("HuggingFace");
        if (huggingFaceSection.Exists())
        {
            chatConfig.HuggingFace = new HuggingFaceConfiguration
            {
                ProviderId = "HuggingFace",
                DisplayName = "Hugging Face",
                ApiKey = huggingFaceSection["ApiKey"] ?? string.Empty,
                ModelId = huggingFaceSection["ModelId"] ?? string.Empty,
                Endpoint = huggingFaceSection["Endpoint"] ?? string.Empty
            };
            _logger.LogDebug("Loaded Hugging Face configuration");
        }

        // Load Ollama configuration
        var ollamaSection = _configuration.GetSection("Ollama");
        if (ollamaSection.Exists())
        {
            chatConfig.Ollama = new OllamaConfiguration
            {
                ProviderId = "Ollama",
                DisplayName = "Ollama (Local)",
                Endpoint = ollamaSection["Endpoint"] ?? string.Empty,
                ModelId = ollamaSection["ModelId"] ?? string.Empty
            };
            _logger.LogDebug("Loaded Ollama configuration");
        }

        // Load LM Studio configuration
        var lmStudioSection = _configuration.GetSection("LMStudio");
        if (lmStudioSection.Exists())
        {
            chatConfig.LMStudio = new LMStudioConfiguration
            {
                ProviderId = "LMStudio",
                DisplayName = "LM Studio (Local)",
                Endpoint = lmStudioSection["Endpoint"] ?? string.Empty,
                ModelId = lmStudioSection["ModelId"] ?? string.Empty
            };
            _logger.LogDebug("Loaded LM Studio configuration");
        }

        // Load Azure AI Inference configuration
        var azureAIInferenceSection = _configuration.GetSection("AzureAIInference");
        if (azureAIInferenceSection.Exists())
        {
            chatConfig.AzureAIInference = new AzureAIInferenceConfiguration
            {
                ProviderId = "AzureAIInference",
                DisplayName = "Azure AI Inference",
                ApiKey = azureAIInferenceSection["ApiKey"] ?? string.Empty,
                ModelId = azureAIInferenceSection["ModelId"] ?? string.Empty,
                Endpoint = azureAIInferenceSection["Endpoint"] ?? string.Empty
            };
            _logger.LogDebug("Loaded Azure AI Inference configuration");
        }
    }
}