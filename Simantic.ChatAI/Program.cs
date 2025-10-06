using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Simantic.ChatAI.Interfaces;
using Simantic.ChatAI.Providers;
using Simantic.ChatAI.Services;
using System.Text;
namespace Simantic.ChatAI;
internal class Program
{
    private static async Task Main(string[] args)
    {
        // Set console encoding to support Unicode characters
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;
        try
        {
            // Build host with dependency injection
            using var host = CreateHostBuilder(args).Build();
            // Start the application
            var app = host.Services.GetRequiredService<ChatApplication>();
            await app.RunAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Application failed to start: {ex.Message}");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory())
                      .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                      .AddUserSecrets<Program>(optional: true)
                      .AddEnvironmentVariables()
                      .AddCommandLine(args);
            })
            .ConfigureLogging((context, logging) =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Information);
                // Reduce noise from HTTP client and other internal logs
                logging.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);
                logging.AddFilter("Microsoft.Extensions.Http", LogLevel.Warning);
            })
            .ConfigureServices((context, services) =>
            {
                // Register application services
                services.AddSingleton<IConfigurationService, ConfigurationService>();
                services.AddSingleton<ILlmProviderFactory, LlmProviderFactory>();
                services.AddScoped<IChatService, ChatService>();
                services.AddTransient<ChatApplication>();
                // Register HttpClient
                services.AddHttpClient();
            });
}
public class ChatApplication
{
    private readonly IChatService _chatService;
    private readonly ILogger<ChatApplication> _logger;
    public ChatApplication(IChatService chatService, ILogger<ChatApplication> logger)
    {
        _chatService = chatService ?? throw new ArgumentNullException(nameof(chatService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    public async Task RunAsync()
    {
        _logger.LogInformation("Starting Simantic.ChatAI application");
        DisplayWelcomeMessage();
        DisplayAvailableProviders();
        try
        {
            await RunChatLoopAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in chat loop");
            Console.WriteLine($"\nAn error occurred: {ex.Message}");
        }
        finally
        {
            _chatService.Dispose();
            _logger.LogInformation("Simantic.ChatAI application stopped");
        }
    }
    private void DisplayWelcomeMessage()
    {
        Console.WriteLine("🤖 Welcome to Simantic.ChatAI - Unified LLM Provider Interface");
        Console.WriteLine("================================================================");
        Console.WriteLine();
        Console.WriteLine($"Current Provider: {_chatService.CurrentProvider}");
        Console.WriteLine();
        Console.WriteLine("Available Commands:");
        Console.WriteLine("  /switch <provider>  - Switch to a different provider");
        Console.WriteLine("  /providers          - List available providers");
        Console.WriteLine("  /clear              - Clear chat history");
        Console.WriteLine("  /history            - Show chat history");
        Console.WriteLine("  /help               - Show this help message");
        Console.WriteLine("  /quit or /exit      - Exit the application");
        Console.WriteLine("  <empty line>        - Exit the application");
        Console.WriteLine();
    }
    private void DisplayAvailableProviders()
    {
        var providers = _chatService.GetAvailableProviders().ToList();
        if (!providers.Any())
        {
            Console.WriteLine("⚠️  No providers are currently configured and available.");
            return;
        }
        Console.WriteLine("Available Providers:");
        Console.WriteLine("-------------------");
        foreach (var provider in providers)
        {
            var status = provider.IsAvailable ? "✅ Available" : "❌ Not Available";
            var type = provider.IsOnline ? "Online" : "Local";
            var current = provider.Id.Equals(_chatService.CurrentProvider, StringComparison.OrdinalIgnoreCase) ? " (Current)" : "";
            Console.WriteLine($"  {provider.DisplayName} ({provider.Id}) - {type} - {status}{current}");
            if (!string.IsNullOrEmpty(provider.ModelId))
            {
                Console.WriteLine($"    Model: {provider.ModelId}");
            }
        }
        Console.WriteLine();
    }
    private async Task RunChatLoopAsync()
    {
        while (true)
        {
            Console.Write("Enter your message: ");
            var input = Console.ReadLine();
            // Exit on empty input or quit commands
            if (string.IsNullOrWhiteSpace(input) || 
                input.Equals("/quit", StringComparison.OrdinalIgnoreCase) ||
                input.Equals("/exit", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Goodbye! 👋");
                break;
            }
            // Handle commands
            if (input.StartsWith('/'))
            {
                await HandleCommandAsync(input);
                continue;
            }
            // Send message to LLM
            await HandleChatMessageAsync(input);
        }
    }
    private async Task HandleCommandAsync(string command)
    {
        var parts = command.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var cmd = parts[0].ToLowerInvariant();
        var arg = parts.Length > 1 ? parts[1] : null;
        try
        {
            switch (cmd)
            {
                case "/switch":
                    await HandleSwitchCommand(arg);
                    break;
                case "/providers":
                    DisplayAvailableProviders();
                    break;
                case "/clear":
                    _chatService.ClearHistory();
                    Console.WriteLine("✅ Chat history cleared.");
                    break;
                case "/history":
                    DisplayChatHistory();
                    break;
                case "/help":
                    DisplayWelcomeMessage();
                    break;
                default:
                    Console.WriteLine($"❌ Unknown command: {cmd}");
                    Console.WriteLine("Type /help to see available commands.");
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling command: {Command}", command);
            Console.WriteLine($"❌ Error executing command: {ex.Message}");
        }
    }
    private async Task HandleSwitchCommand(string? providerId)
    {
        if (string.IsNullOrWhiteSpace(providerId))
        {
            Console.WriteLine("❌ Please specify a provider ID. Example: /switch OpenAI");
            return;
        }
        try
        {
            await _chatService.SwitchProviderAsync(providerId);
            Console.WriteLine($"✅ Switched to provider: {providerId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to switch to provider '{providerId}': {ex.Message}");
        }
    }
    private void DisplayChatHistory()
    {
        var history = _chatService.GetHistory();
        if (!history.Any())
        {
            Console.WriteLine("No chat history available.");
            return;
        }
        Console.WriteLine("\n📋 Chat History:");
        Console.WriteLine("================");
        foreach (var message in history)
        {
            var prefix = message.Role.Equals("user", StringComparison.OrdinalIgnoreCase) ? "👤 You" : "🤖 Assistant";
            Console.WriteLine($"{prefix}: {message.Content}");
            Console.WriteLine();
        }
    }
    private async Task HandleChatMessageAsync(string message)
    {
        try
        {
            Console.WriteLine(); // Add spacing before response
            var tokenUsage = await ProcessStreamingResponseAsync(message);
            // Display token usage if available
            if (tokenUsage != null)
            {
                Console.WriteLine($"\n📊 Token Usage: {tokenUsage}");
            }
            Console.WriteLine(); // Add spacing after response
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing chat message");
            Console.WriteLine($"❌ Error: {ex.Message}");
        }
    }
    private async Task<Models.TokenUsage?> ProcessStreamingResponseAsync(string message)
    {
        Models.TokenUsage? finalUsage = null;
        await foreach (var chunk in _chatService.SendMessageStreamingAsync(message))
        {
            if (!string.IsNullOrEmpty(chunk.Content))
            {
                Console.Write(chunk.Content);
            }
            if (chunk.IsComplete)
            {
                finalUsage = chunk.TokenUsage;
                break;
            }
            // Update usage as we receive chunks (for providers that send intermediate usage)
            if (chunk.TokenUsage != null)
            {
                finalUsage = chunk.TokenUsage;
            }
        }
        return finalUsage;
    }
}
