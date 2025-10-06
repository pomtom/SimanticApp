using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Simantic.ChatAI.Interfaces;
using Simantic.ChatAI.Models;
using System.Runtime.CompilerServices;

namespace Simantic.ChatAI.Services;

/// <summary>
/// Main chat service that handles conversations across different LLM providers
/// </summary>
public class ChatService : IChatService
{
    private readonly ILlmProviderFactory _providerFactory;
    private readonly IConfigurationService _configurationService;
    private readonly ILogger<ChatService> _logger;
    private readonly ChatHistory _chatHistory;
    private readonly ChatHistoryTruncationReducer _historyReducer;
    
    private IChatCompletionService? _currentService;
    private string _currentProvider;
    private bool _disposed;

    public ChatService(
        ILlmProviderFactory providerFactory,
        IConfigurationService configurationService,
        ILogger<ChatService> logger)
    {
        _providerFactory = providerFactory ?? throw new ArgumentNullException(nameof(providerFactory));
        _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var config = _configurationService.GetConfiguration();
        _chatHistory = new ChatHistory(config.DefaultSystemMessage);
        _historyReducer = new ChatHistoryTruncationReducer(targetCount: config.MaxChatHistoryMessages);
        
        _currentProvider = _providerFactory.GetDefaultProviderId();
        _logger.LogInformation("ChatService initialized with default provider: {Provider}", _currentProvider);
    }

    /// <summary>
    /// Current provider being used
    /// </summary>
    public string CurrentProvider => _currentProvider;

    /// <summary>
    /// Switches to a different provider
    /// </summary>
    /// <param name="providerId">The provider identifier</param>
    public async Task SwitchProviderAsync(string providerId)
    {
        ArgumentNullException.ThrowIfNull(providerId);

        if (_currentProvider.Equals(providerId, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogDebug("Already using provider: {ProviderId}", providerId);
            return;
        }

        if (!_providerFactory.IsProviderAvailable(providerId))
        {
            throw new InvalidOperationException($"Provider '{providerId}' is not available or not configured");
        }

        try
        {
            _logger.LogInformation("Switching from provider {CurrentProvider} to {NewProvider}", _currentProvider, providerId);
            
            if (_currentService is IDisposable disposableService)
                disposableService.Dispose();
            _currentService = await _providerFactory.CreateChatCompletionServiceAsync(providerId);
            _currentProvider = providerId;
            
            _logger.LogInformation("Successfully switched to provider: {Provider}", providerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to switch to provider: {ProviderId}", providerId);
            throw;
        }
    }

    /// <summary>
    /// Sends a message and gets streaming response
    /// </summary>
    /// <param name="message">User message</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Streaming chat response</returns>
    public async IAsyncEnumerable<ChatResponseChunk> SendMessageStreamingAsync(
        string message, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        var service = await EnsureServiceAsync();
        var settings = _configurationService.GetExecutionSettings(_currentProvider);

        _logger.LogDebug("Sending streaming message to {Provider}: {Message}", _currentProvider, message[..Math.Min(message.Length, 50)]);

        _chatHistory.AddUserMessage(message);
        string fullResponse = string.Empty;
        TokenUsage? finalUsage = null;

        // Stream the response chunks
        await foreach (var chunk in GetStreamingResponseAsync(service, settings, cancellationToken))
        {
            if (!chunk.IsComplete)
            {
                fullResponse += chunk.Content;
                if (chunk.TokenUsage != null)
                    finalUsage = chunk.TokenUsage;
            }
            
            yield return chunk;
            
            if (chunk.IsComplete)
                break;
        }

        // Add response to history
        _chatHistory.AddAssistantMessage(fullResponse);

        // Reduce history if necessary
        await ReduceHistoryIfNeededAsync();

        _logger.LogDebug("Completed streaming response from {Provider}. Length: {Length}", _currentProvider, fullResponse.Length);
    }

    private async IAsyncEnumerable<ChatResponseChunk> GetStreamingResponseAsync(
        IChatCompletionService service,
        PromptExecutionSettings settings,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        TokenUsage? finalUsage = null;

        await foreach (var chunk in service.GetStreamingChatMessageContentsAsync(_chatHistory, settings, null, cancellationToken))
        {
            var content = chunk.Content;

            // Extract token usage if available
            TokenUsage? chunkUsage = null;
            if (chunk.InnerContent is OpenAI.Chat.StreamingChatCompletionUpdate update)
            {
                chunkUsage = TokenUsage.FromOpenAIUsage(update.Usage);
                if (chunkUsage != null)
                    finalUsage = chunkUsage;
            }

            yield return new ChatResponseChunk
            {
                Content = content,
                IsComplete = false,
                TokenUsage = chunkUsage,
                Provider = _currentProvider,
                ModelId = GetModelIdFromService(service)
            };
        }

        // Send final chunk
        yield return new ChatResponseChunk
        {
            Content = null,
            IsComplete = true,
            TokenUsage = finalUsage,
            Provider = _currentProvider,
            ModelId = GetModelIdFromService(service)
        };
    }

    /// <summary>
    /// Sends a message and gets complete response
    /// </summary>
    /// <param name="message">User message</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Complete chat response</returns>
    public async Task<ChatResponse> SendMessageAsync(string message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        var service = await EnsureServiceAsync();
        var settings = _configurationService.GetExecutionSettings(_currentProvider);

        _logger.LogDebug("Sending message to {Provider}: {Message}", _currentProvider, message[..Math.Min(message.Length, 50)]);

        _chatHistory.AddUserMessage(message);

        try
        {
            var response = await service.GetChatMessageContentAsync(_chatHistory, settings, null, cancellationToken);
            var content = response.Content ?? string.Empty;

            // Extract token usage if available
            TokenUsage? usage = null;
            if (response.InnerContent is OpenAI.Chat.ChatCompletion completion)
            {
                usage = TokenUsage.FromOpenAIUsage(completion.Usage);
            }

            _chatHistory.AddAssistantMessage(content);

            // Reduce history if necessary
            await ReduceHistoryIfNeededAsync();

            var chatResponse = new ChatResponse
            {
                Content = content,
                Provider = _currentProvider,
                ModelId = GetModelIdFromService(service),
                TokenUsage = usage,
                IsStreamed = false
            };

            _logger.LogDebug("Completed response from {Provider}. Length: {Length}", _currentProvider, content.Length);
            
            return chatResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during chat completion with {Provider}", _currentProvider);
            throw;
        }
    }

    /// <summary>
    /// Clears the chat history
    /// </summary>
    public void ClearHistory()
    {
        var config = _configurationService.GetConfiguration();
        _chatHistory.Clear();
        _chatHistory.AddSystemMessage(config.DefaultSystemMessage);
        _logger.LogInformation("Chat history cleared");
    }

    /// <summary>
    /// Gets the current chat history
    /// </summary>
    /// <returns>List of chat messages</returns>
    public IReadOnlyList<ChatMessage> GetHistory()
    {
        return _chatHistory
            .Where(m => m.Role != AuthorRole.System) // Exclude system messages from display
            .Select(m => new ChatMessage
            {
                Role = m.Role.ToString(),
                Content = m.Content ?? string.Empty,
                Timestamp = DateTime.UtcNow // Note: ChatHistory doesn't store timestamps
            })
            .ToList();
    }

    /// <summary>
    /// Gets available providers
    /// </summary>
    /// <returns>List of provider information</returns>
    public IEnumerable<ProviderInfo> GetAvailableProviders()
    {
        return _providerFactory.GetAvailableProviders();
    }

    private async Task<IChatCompletionService> EnsureServiceAsync()
    {
        if (_currentService == null)
        {
            _currentService = await _providerFactory.CreateChatCompletionServiceAsync(_currentProvider);
        }
        return _currentService;
    }

    private static string? GetModelIdFromService(IChatCompletionService service)
    {
        // Try to extract model ID from service attributes
        if (service.Attributes.TryGetValue("ModelId", out var modelId))
        {
            return modelId?.ToString();
        }

        // Try deployment name for Azure OpenAI
        if (service.Attributes.TryGetValue("DeploymentName", out var deploymentName))
        {
            return deploymentName?.ToString();
        }

        return null;
    }

    private async Task ReduceHistoryIfNeededAsync()
    {
        try
        {
            var service = await EnsureServiceAsync();
            var reducedMessages = await _historyReducer.ReduceAsync(_chatHistory);
            
            if (reducedMessages != null)
            {
                var config = _configurationService.GetConfiguration();
                _chatHistory.Clear();
                _chatHistory.AddSystemMessage(config.DefaultSystemMessage);
                
                foreach (var message in reducedMessages.Skip(1)) // Skip system message
                {
                    _chatHistory.Add(message);
                }
                
                _logger.LogDebug("Chat history reduced to {Count} messages", _chatHistory.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to reduce chat history");
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_currentService is IDisposable disposableService)
                disposableService.Dispose();
            _disposed = true;
            _logger.LogDebug("ChatService disposed");
        }
    }
}