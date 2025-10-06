using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureAIInference;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Connectors.Ollama;
using OllamaSharp;
using Simantic.ChatAI.Configuration;
using Simantic.ChatAI.Interfaces;
using Simantic.ChatAI.Models;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
namespace Simantic.ChatAI.Providers;
public class LlmProviderFactory : ILlmProviderFactory, IDisposable
{
    private readonly IConfigurationService _configurationService;
    private readonly ILogger<LlmProviderFactory> _logger;
    private readonly Dictionary<string, IChatCompletionService> _providerCache;
    private readonly HttpClient _httpClient;
    private bool _disposed;
    public LlmProviderFactory(
        IConfigurationService configurationService,
        ILogger<LlmProviderFactory> logger)
    {
        _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _providerCache = new Dictionary<string, IChatCompletionService>();
        // Create HttpClient with proper SSL handling for HuggingFace and other providers
        var handler = new HttpClientHandler();
        handler.ServerCertificateCustomValidationCallback = ValidateServerCertificate;
        _httpClient = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromMinutes(5)
        };
    }
    public async Task<IChatCompletionService> CreateChatCompletionServiceAsync(string providerId)
    {
        ArgumentNullException.ThrowIfNull(providerId);
        _logger.LogInformation("Creating chat completion service for provider: {ProviderId}", providerId);
        // Check cache first
        if (_providerCache.TryGetValue(providerId, out var cachedService))
        {
            _logger.LogDebug("Returning cached service for provider: {ProviderId}", providerId);
            return cachedService;
        }
        try
        {
            var config = _configurationService.GetConfiguration();
            var service = providerId.ToLowerInvariant() switch
            {
                "azureopenai" => await CreateAzureOpenAIServiceAsync(config.AzureOpenAI),
                "openai" => await CreateOpenAIServiceAsync(config.OpenAI),
                "huggingface" => await CreateHuggingFaceServiceAsync(config.HuggingFace),
                "ollama" => await CreateOllamaServiceAsync(config.Ollama),
                "lmstudio" => await CreateLMStudioServiceAsync(config.LMStudio),
                "azureaiinference" => await CreateAzureAIInferenceServiceAsync(config.AzureAIInference),
                _ => throw new NotSupportedException($"Provider '{providerId}' is not supported")
            };
            // Cache the service
            _providerCache[providerId] = service;
            _logger.LogInformation("Successfully created service for provider: {ProviderId}", providerId);
            return service;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create service for provider: {ProviderId}", providerId);
            throw;
        }
    }
    public IEnumerable<ProviderInfo> GetAvailableProviders()
    {
        var config = _configurationService.GetConfiguration();
        var providers = new List<ProviderInfo>();
        foreach (var (key, providerConfig) in config.GetAllProviders())
        {
            providers.Add(new ProviderInfo
            {
                Id = key,
                DisplayName = providerConfig.DisplayName,
                IsAvailable = providerConfig.IsEnabled && providerConfig.IsValid(),
                IsOnline = providerConfig.IsOnline,
                ModelId = providerConfig.DefaultModelId
            });
        }
        return providers;
    }
    public bool IsProviderAvailable(string providerId)
    {
        return _configurationService.IsProviderConfigured(providerId);
    }
    public string GetDefaultProviderId()
    {
        var config = _configurationService.GetConfiguration();
        return config.DefaultProvider;
    }
    private Task<IChatCompletionService> CreateAzureOpenAIServiceAsync(AzureOpenAIConfiguration? config)
    {
        if (config == null || !config.IsValid())
            throw new InvalidOperationException("Azure OpenAI configuration is invalid or missing");
        _logger.LogDebug("Creating Azure OpenAI service with endpoint: {Endpoint}", config.Endpoint);
        var builder = Kernel.CreateBuilder();
        builder.AddAzureOpenAIChatCompletion(config.DeploymentName, config.Endpoint, config.ApiKey);
        var kernel = builder.Build();
        return Task.FromResult(kernel.GetRequiredService<IChatCompletionService>());
    }
    private Task<IChatCompletionService> CreateOpenAIServiceAsync(OpenAIConfiguration? config)
    {
        if (config == null || !config.IsValid())
            throw new InvalidOperationException("OpenAI configuration is invalid or missing");
        _logger.LogDebug("Creating OpenAI service with model: {ModelId}", config.ModelId);
        var builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion(config.ModelId, config.ApiKey, config.OrganizationId);
        var kernel = builder.Build();
        return Task.FromResult(kernel.GetRequiredService<IChatCompletionService>());
    }
    private Task<IChatCompletionService> CreateHuggingFaceServiceAsync(HuggingFaceConfiguration? config)
    {
        if (config == null || !config.IsValid())
            throw new InvalidOperationException("HuggingFace configuration is invalid or missing");
        _logger.LogDebug("Creating HuggingFace service with model: {ModelId}", config.ModelId);
        var builder = Kernel.CreateBuilder();
        builder.AddHuggingFaceChatCompletion(config.ModelId, new Uri(config.Endpoint), config.ApiKey, httpClient: _httpClient);
        var kernel = builder.Build();
        return Task.FromResult(kernel.GetRequiredService<IChatCompletionService>());
    }
    private Task<IChatCompletionService> CreateOllamaServiceAsync(OllamaConfiguration? config)
    {
        if (config == null || !config.IsValid())
            throw new InvalidOperationException("Ollama configuration is invalid or missing");
        _logger.LogDebug("Creating Ollama service with endpoint: {Endpoint}, model: {ModelId}", config.Endpoint, config.ModelId);
        var ollamaClient = new OllamaApiClient(config.Endpoint, config.ModelId);
        return Task.FromResult(ollamaClient.AsChatCompletionService());
    }
    private Task<IChatCompletionService> CreateLMStudioServiceAsync(LMStudioConfiguration? config)
    {
        if (config == null || !config.IsValid())
            throw new InvalidOperationException("LM Studio configuration is invalid or missing");
        _logger.LogDebug("Creating LM Studio service with endpoint: {Endpoint}, model: {ModelId}", config.Endpoint, config.ModelId);
#pragma warning disable SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates
        return Task.FromResult<IChatCompletionService>(new OpenAIChatCompletionService(config.ModelId, new Uri(config.Endpoint)));
#pragma warning restore SKEXP0010
    }
    private Task<IChatCompletionService> CreateAzureAIInferenceServiceAsync(AzureAIInferenceConfiguration? config)
    {
        if (config == null || !config.IsValid())
            throw new InvalidOperationException("Azure AI Inference configuration is invalid or missing");
        _logger.LogDebug("Creating Azure AI Inference service with model: {ModelId}", config.ModelId);
        var builder = Kernel.CreateBuilder();
        builder.AddAzureAIInferenceChatCompletion(config.ModelId, config.ApiKey, new Uri(config.Endpoint));
        var kernel = builder.Build();
        return Task.FromResult(kernel.GetRequiredService<IChatCompletionService>());
    }
    private static bool ValidateServerCertificate(HttpRequestMessage message, X509Certificate2? cert, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
    {
        // For development purposes, allow certain SSL policy errors
        if (sslPolicyErrors == SslPolicyErrors.None)
            return true;
        // Handle RevocationStatusUnknown error specifically
        if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors && chain != null)
        {
            foreach (var status in chain.ChainStatus)
            {
                if (status.Status == X509ChainStatusFlags.RevocationStatusUnknown)
                {
                    continue;
                }
                if (status.Status != X509ChainStatusFlags.NoError)
                {
                    return false;
                }
            }
            return true;
        }
        return false;
    }
    public void Dispose()
    {
        if (!_disposed)
        {
            _httpClient?.Dispose();
            // Dispose cached services if they implement IDisposable
            foreach (var service in _providerCache.Values)
            {
                if (service is IDisposable disposable)
                {
                    try
                    {
                        disposable.Dispose();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error disposing chat completion service");
                    }
                }
            }
            _providerCache.Clear();
            _disposed = true;
        }
    }
}
