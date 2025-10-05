using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace HuggingFace
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

            var apiKey = _configuration["HuggingFace:ApiKey"];
            var modelId = _configuration["HuggingFace:ModelId"];
            var endpoint = _configuration["HuggingFace:Endpoint"];

            // Validate configuration
            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(modelId) || string.IsNullOrEmpty(endpoint))
            {
                Console.WriteLine("Error: HuggingFace configuration is missing. Please check your appsettings.json or user secrets.");
                return;
            }

            // Create custom HttpClient with SSL certificate handling
            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) =>
            {
                // For development purposes, you can bypass SSL validation
                // In production, implement proper certificate validation
                if (sslPolicyErrors == SslPolicyErrors.None)
                    return true;
                
                // Handle RevocationStatusUnknown error specifically
                if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors)
                {
                    foreach (X509ChainStatus status in chain!.ChainStatus)
                    {
                        if (status.Status == X509ChainStatusFlags.RevocationStatusUnknown)
                        {
                            // Allow connections when revocation status is unknown
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
            };

            var httpClient = new HttpClient(httpClientHandler);
            
            // Set timeout for the HttpClient
            httpClient.Timeout = TimeSpan.FromMinutes(5);

            // Create a kernel builder and add HuggingFace chat completion service
            var builder = Kernel.CreateBuilder();
            builder.AddHuggingFaceChatCompletion(modelId, new Uri(endpoint), apiKey, httpClient: httpClient);

            // Build the kernel
            Kernel kernel = builder.Build();

            // Create chat history
            var history = new ChatHistory("You are a friendly AI Assistant that answers in a friendly manner");

            // Get reference to chat completion service
            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

            // Define settings for OpenAI prompt execution
            HuggingFacePromptExecutionSettings settings = new()
            {
                Temperature = 0.9f,
                MaxTokens = 1500,
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
                OpenAI.Chat.ChatTokenUsage? usage = null;

                history.AddUserMessage(prompt);
                // Get streaming response from chat completion service
                await foreach (StreamingChatMessageContent responseChunk in chatCompletionService.GetStreamingChatMessageContentsAsync(history, settings))
                {
                    // Print response to console
                    Console.Write(responseChunk.Content);
                    fullMessage += responseChunk.Content;
                    
                    // Try to get usage information if available
                    if (responseChunk.InnerContent is OpenAI.Chat.StreamingChatCompletionUpdate update)
                    {
                        usage = update.Usage;
                    }
                }
                // Add response to chat history
                history.AddAssistantMessage(fullMessage);


                //get non-streaming result from chat completion setvice
                //var response = await chatCompletionService.GetChatMessageContentAsync(history, settings);
                //add response to chat history
                //history.Add(response);

                // Display number of tokens used (model specific) - only if usage is available
                if (usage != null)
                {
                    Console.WriteLine($"\n\tInput Tokens: \t{usage.InputTokenCount}");
                    Console.WriteLine($"\tOutput Tokens: \t{usage.OutputTokenCount}");
                    Console.WriteLine($"\tTotal Tokens: \t{usage.TotalTokenCount}");
                }
                else
                {
                    Console.WriteLine("\n\tToken usage information not available for this model.");
                }

                // Reduce chat history if necessary
                var reduceMessages = await reducer.ReduceAsync(history);
                if (reduceMessages is not null)
                    history = new(reduceMessages);
            }
        }
    }
}
