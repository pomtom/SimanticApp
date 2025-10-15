using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Semantic.WebApp.Models;

namespace Semantic.WebApp.Services
{
    public interface IChatService
    {
        Task<string> GetResponseAsync(ChatModel chatModel);
    }

    public class ChatService : IChatService
    {
        private readonly Kernel _kernel;
        private readonly PromptExecutionSettings _promptSettings;

        public ChatService(Kernel kernel, PromptExecutionSettings promptSettings)
        {
            _kernel = kernel;
            _promptSettings = promptSettings;
        }

        public async Task<string> GetResponseAsync(ChatModel chatModel)
        {
            var chatService = _kernel.GetRequiredService<IChatCompletionService>();
            chatModel.ChatHistory.AddUserMessage(chatModel.Prompt);
            
            var history = new ChatHistory(chatModel.ChatHistory);
            var response = await chatService.GetChatMessageContentAsync(history, _promptSettings, _kernel);
            
            chatModel.ChatHistory.Add(response);
            
            return response.Content ?? "I'm sorry, I couldn't generate a response.";
        }
    }
}