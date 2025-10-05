using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace LMStudio
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

            var modelid = _configuration["LMStudio:modelid"];
            var endpoint = _configuration["LMStudio:endpoint"];

            //var chatCompletionService = new OllamaApiClient(config["ollama:endpoint"], config["ollama:modelid"]).AsChatCompletionService();
#pragma warning disable SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            OpenAIChatCompletionService chatCompletionService = new(modelid, new Uri(endpoint));
#pragma warning restore SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

            // Create chat history
            var history = new ChatHistory(systemMessage: "You are a friendly AI Assistant that answers in a friendly manner");


            // Define settings for OpenAI prompt execution
            AzureOpenAIPromptExecutionSettings settings = new()
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
                OpenAI.Chat.ChatTokenUsage usage = null;

                history.AddUserMessage(prompt);
                // Get streaming response from chat completion service
                await foreach (StreamingChatMessageContent responseChunk in chatCompletionService.GetStreamingChatMessageContentsAsync(history, settings))
                {
                    // Print response to console
                    Console.Write(responseChunk.Content);
                    fullMessage += responseChunk.Content;
                    usage = ((OpenAI.Chat.StreamingChatCompletionUpdate)responseChunk.InnerContent).Usage;
                }
                // Add response to chat history
                history.AddAssistantMessage(fullMessage);


                //get non-streaming result from chat completion setvice
                //var response = await chatCompletionService.GetChatMessageContentAsync(history, settings);
                //add response to chat history
                //history.Add(response);

                // Display number of tokens used (model specific)
                Console.WriteLine($"\n\tInput Tokens: \t{usage?.InputTokenCount}");
                Console.WriteLine($"\tOutput Tokens: \t{usage?.OutputTokenCount}");
                Console.WriteLine($"\tTotal Tokens: \t{usage?.TotalTokenCount}");

                // Reduce chat history if necessary
                var reduceMessages = await reducer.ReduceAsync(history);
                if (reduceMessages is not null)
                    history = new(reduceMessages);
            }
        }
    }
}
