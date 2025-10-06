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
        // Set console encoding to ASCII
        Console.OutputEncoding = Encoding.ASCII;
        Console.InputEncoding = Encoding.ASCII;
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
                logging.SetMinimumLevel(LogLevel.Error); // Only show errors by default
                
                // Allow only specific application logs (ChatApplication only)
                logging.AddFilter("Simantic.ChatAI.ChatApplication", LogLevel.Information);
                
                // Suppress ALL other application components
                logging.AddFilter("Simantic.ChatAI.Providers", LogLevel.None);
                logging.AddFilter("Simantic.ChatAI.Services", LogLevel.None);
                logging.AddFilter("Simantic.ChatAI.Configuration", LogLevel.None);
                
                // Suppress all Microsoft and System logs
                logging.AddFilter("Microsoft", LogLevel.None);
                logging.AddFilter("System", LogLevel.None);
                logging.AddFilter("Azure", LogLevel.None);
                
                // Suppress specific noisy components
                logging.AddFilter("System.Net.Http.HttpClient", LogLevel.None);
                logging.AddFilter("Microsoft.Extensions.Http", LogLevel.None);
                logging.AddFilter("Microsoft.Extensions.Hosting", LogLevel.None);
                logging.AddFilter("Microsoft.Extensions.DependencyInjection", LogLevel.None);
                logging.AddFilter("Microsoft.SemanticKernel", LogLevel.None);
                logging.AddFilter("Azure.Core", LogLevel.None);
                logging.AddFilter("Azure.AI", LogLevel.None);
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
        Console.WriteLine("[AI] Welcome to Simantic.ChatAI - Unified LLM Provider Interface");
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
            Console.WriteLine("[WARNING] No providers are currently configured and available.");
            return;
        }
        Console.WriteLine("\nAvailable Providers:");
        Console.WriteLine("====================");
        
        foreach (var provider in providers)
        {
            var type = provider.IsOnline ? "Online" : "Local ";
            var current = provider.Id.Equals(_chatService.CurrentProvider, StringComparison.OrdinalIgnoreCase) ? " <- CURRENT" : "";
            
            // Display provider name and ID with padding
            Console.Write($"  - {provider.DisplayName,-20} ({provider.Id,-15}) ");
            
            // Display type with fixed width
            Console.Write($"[{type,-6}] ");
            
            // Color only the status text
            Console.Write("Status: ");
            if (provider.IsAvailable)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("Available");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("Not Available");
            }
            Console.ResetColor();
            
            // Add current indicator in yellow
            if (!string.IsNullOrEmpty(current))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(current);
                Console.ResetColor();
            }
            
            Console.WriteLine();
            
            // Display model info with proper indentation
            if (!string.IsNullOrEmpty(provider.ModelId))
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"    | Model: {provider.ModelId}");
                Console.ResetColor();
            }
            
            Console.WriteLine(); // Add spacing between providers
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
                Console.WriteLine("Goodbye!");
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
                    Console.WriteLine("[OK] Chat history cleared.");
                    break;
                case "/history":
                    DisplayChatHistory();
                    break;
                case "/help":
                    DisplayWelcomeMessage();
                    break;
                default:
                    Console.WriteLine($"[ERROR] Unknown command: {cmd}");
                    Console.WriteLine("Type /help to see available commands.");
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling command: {Command}", command);
            Console.WriteLine($"[ERROR] Error executing command: {ex.Message}");
        }
    }
    private async Task HandleSwitchCommand(string? providerId)
    {
        if (string.IsNullOrWhiteSpace(providerId))
        {
            Console.WriteLine("[ERROR] Please specify a provider ID. Example: /switch OpenAI");
            return;
        }
        try
        {
            await _chatService.SwitchProviderAsync(providerId);
            Console.WriteLine($"[OK] Switched to provider: {providerId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to switch to provider '{providerId}': {ex.Message}");
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
        Console.WriteLine("\n[HISTORY] Chat History:");
        Console.WriteLine("================");
        foreach (var message in history)
        {
            var prefix = message.Role.Equals("user", StringComparison.OrdinalIgnoreCase) ? "[USER] You" : "[AI] Assistant";
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
                Console.WriteLine($"\n[STATS] Token Usage: {tokenUsage}");
            }
            Console.WriteLine(); // Add spacing after response
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing chat message");
            Console.WriteLine($"[ERROR] Error: {ex.Message}");
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
