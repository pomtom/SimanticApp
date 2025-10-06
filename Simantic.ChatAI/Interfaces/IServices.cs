using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Simantic.ChatAI.Models;
namespace Simantic.ChatAI.Interfaces;
public interface ILlmProviderFactory
{
    Task<IChatCompletionService> CreateChatCompletionServiceAsync(string providerId);
    IEnumerable<ProviderInfo> GetAvailableProviders();
    bool IsProviderAvailable(string providerId);
    string GetDefaultProviderId();
}
public interface IChatService : IDisposable
{
    string CurrentProvider { get; }
    Task SwitchProviderAsync(string providerId);
    IAsyncEnumerable<ChatResponseChunk> SendMessageStreamingAsync(string message, CancellationToken cancellationToken = default);
    Task<ChatResponse> SendMessageAsync(string message, CancellationToken cancellationToken = default);
    void ClearHistory();
    IReadOnlyList<ChatMessage> GetHistory();
    IEnumerable<ProviderInfo> GetAvailableProviders();
}
public interface IConfigurationService
{
    Configuration.ChatAIConfiguration GetConfiguration();
    bool IsProviderConfigured(string providerId);
    PromptExecutionSettings GetExecutionSettings(string providerId);
}
