using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Simantic.MultiModel
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            IConfiguration _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                 .AddUserSecrets<Program>()
                .Build();


            string deploymentName = _configuration["AzureOpenAI:DeploymentName"]!;
            string apiKey = _configuration["AzureOpenAI:ApiKey"]!;
            string endpoint = _configuration["AzureOpenAI:Endpoint"]!;
            string modelId = _configuration["AzureOpenAI:ModelId"]!;

            AzureOpenAIChatCompletionService chatCompletionService = new AzureOpenAIChatCompletionService(
                deploymentName: deploymentName,
                apiKey: apiKey,
                endpoint: endpoint,
                modelId: modelId
            );

            var promptExecutionSettings = new OpenAIPromptExecutionSettings
            {
                ResponseFormat = typeof(CameraAnalysis)
            };

            var imageFiles = Directory.GetFiles("images", "*.jpg");

            foreach (var imageFile in imageFiles)
            {
                var imageBytes = File.ReadAllBytes(imageFile);
                ChatHistory history = new ChatHistory(@"you are a traffic analyzer AI that monitors traffic congestion images and congestion level.
Heavy congestion level is when there is very little room between cars and vehicles are breaking.
Medium congestion is when there is a lot of cars but they are not braking. Low traffic is when there are few cars on the road
In addition, attempt to determine if the image was taken with a malfunctioning camera by looking for distorted image or missing content.");

                history.AddUserMessage([
                       new ImageContent(imageBytes, "image/jpeg"),
                        new TextContent("Analyze the image and determine the traffic congestion level. Also determine if the camera is malfunctioning")
                   ]);

                // Get the chat message content from the chat completion service
                var response = await chatCompletionService.GetChatMessageContentAsync(history, promptExecutionSettings);

                var options = new JsonSerializerOptions
                {
                    // Configure the JSON serializer to convert strings to enums when needed
                    Converters = { new JsonStringEnumConverter() }
                };

                CameraAnalysis result = JsonSerializer.Deserialize<CameraAnalysis>(response.Content, options);

                // Display the results with appropriate console colors
                Console.ForegroundColor = result.IsBroken ? ConsoleColor.Red : ConsoleColor.Green;
                Console.WriteLine($"Image Name: {imageFile}");
                Console.WriteLine($"Image Visibility: {result.Visibility}");
                Console.WriteLine($"IsBroken: {result.IsBroken}");
                Console.WriteLine($"TrafficCongestionLevel: {result.TrafficCongestionLevel}");
                Console.WriteLine($"Analysis: {result.Analysis}");
                Console.ResetColor();
                Console.WriteLine(new string('-', 40));

                //artificial delay
                await Task.Delay(1000);
            }
        }

        public class CameraAnalysis
        {
            public bool IsBroken { get; set; }
            public TrafficCongestionLevel TrafficCongestionLevel { get; set; }
            public string Analysis { get; set; }

            public VisibilityLevel Visibility { get; set; }

        }

        public enum TrafficCongestionLevel
        {
            Light,
            Moderate,
            Heavy,
            Unknown
        }

        public enum VisibilityLevel
        {
            Clear,
            Foggy,
            Rainy,
            Snowy,
            Sunlight,
            Unknown
        }
    }
}
