using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using OllamaSharp;

namespace Ollama
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // Build and get configuration from appsettings.json, environment variables, and user secrets
            IConfiguration _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                 .AddUserSecrets<Program>()
                .Build();

            var endpoint = _configuration["Ollama:endpoint"];
            var modelId = _configuration["Ollama:modelid"];


            // Create chat history
            var history = new ChatHistory(systemMessage: "You are a friendly AI Assistant that answers in a friendly manner");

            // Get reference to chat completion service
            var chatCompletionService = new OllamaApiClient(endpoint, modelId).AsChatCompletionService();

            // Define settings for OpenAI prompt execution
            OllamaPromptExecutionSettings settings = new()
            {
                Temperature = 0.9f
            };

            // Display model and service information
            foreach (var attr in chatCompletionService.Attributes)
            {
                Console.WriteLine($"{attr.Key} \t \t {attr.Value}");
            }

            // Create a chat history truncation reducer
            var reducer = new ChatHistoryTruncationReducer(targetCount: 10);
            // var reducer = new ChatHistorySummarizationReducer(chatCompletionService, 2, 2);

            // Control loop for user interaction
            while (true)
            {
                // Get input from user
                Console.Write("\nEnter your prompt: ");
                var prompt = Console.ReadLine();

                // Exit if prompt is null or empty
                if (string.IsNullOrEmpty(prompt))
                    break;

                string fullMessage = "";

                history.AddUserMessage(prompt);
                // Get streaming response from chat completion service
                await foreach (StreamingChatMessageContent responseChunk in chatCompletionService.GetStreamingChatMessageContentsAsync(history, settings))
                {
                    // Print response to console
                    Console.Write(responseChunk.Content);
                    fullMessage += responseChunk.Content;
                }
                // Add response to chat history
                history.AddAssistantMessage(fullMessage);

                Console.WriteLine(); // Add a newline after the response

                // Reduce chat history if necessary
                var reduceMessages = await reducer.ReduceAsync(history);
                if (reduceMessages is not null)
                    history = new(reduceMessages);
            }
        }

    }
}
