using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Simantic.ChatAI.Models;

namespace Simantic.ChatAI.Interfaces;

/// <summary>
/// Interface for LLM provider factory
/// </summary>
public interface ILlmProviderFactory
{
    /// <summary>
    /// Creates a chat completion service for the specified provider
    /// </summary>
    /// <param name="providerId">The provider identifier</param>
    /// <returns>Chat completion service instance</returns>
    Task<IChatCompletionService> CreateChatCompletionServiceAsync(string providerId);

    /// <summary>
    /// Gets available providers
    /// </summary>
    /// <returns>List of available provider information</returns>
    IEnumerable<ProviderInfo> GetAvailableProviders();

    /// <summary>
    /// Checks if a provider is available and configured
    /// </summary>
    /// <param name="providerId">The provider identifier</param>
    /// <returns>True if available, false otherwise</returns>
    bool IsProviderAvailable(string providerId);

    /// <summary>
    /// Gets the default provider ID
    /// </summary>
    /// <returns>Default provider identifier</returns>
    string GetDefaultProviderId();
}

/// <summary>
/// Interface for chat service that handles conversations
/// </summary>
public interface IChatService : IDisposable
{
    /// <summary>
    /// Current provider being used
    /// </summary>
    string CurrentProvider { get; }

    /// <summary>
    /// Switches to a different provider
    /// </summary>
    /// <param name="providerId">The provider identifier</param>
    Task SwitchProviderAsync(string providerId);

    /// <summary>
    /// Sends a message and gets streaming response
    /// </summary>
    /// <param name="message">User message</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Streaming chat response</returns>
    IAsyncEnumerable<ChatResponseChunk> SendMessageStreamingAsync(string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a message and gets complete response
    /// </summary>
    /// <param name="message">User message</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Complete chat response</returns>
    Task<ChatResponse> SendMessageAsync(string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears the chat history
    /// </summary>
    void ClearHistory();

    /// <summary>
    /// Gets the current chat history
    /// </summary>
    /// <returns>List of chat messages</returns>
    IReadOnlyList<ChatMessage> GetHistory();

    /// <summary>
    /// Gets available providers
    /// </summary>
    /// <returns>List of provider information</returns>
    IEnumerable<ProviderInfo> GetAvailableProviders();
}

/// <summary>
/// Interface for managing application settings and configuration
/// </summary>
public interface IConfigurationService
{
    /// <summary>
    /// Gets the current chat AI configuration
    /// </summary>
    /// <returns>Configuration instance</returns>
    Configuration.ChatAIConfiguration GetConfiguration();

    /// <summary>
    /// Validates if a provider is properly configured
    /// </summary>
    /// <param name="providerId">Provider identifier</param>
    /// <returns>True if valid, false otherwise</returns>
    bool IsProviderConfigured(string providerId);

    /// <summary>
    /// Gets provider-specific execution settings
    /// </summary>
    /// <param name="providerId">Provider identifier</param>
    /// <returns>Prompt execution settings</returns>
    PromptExecutionSettings GetExecutionSettings(string providerId);
}