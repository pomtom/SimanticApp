# SimanticApp

A .NET console application that demonstrates chat completion using Microsoft Semantic Kernel with Azure OpenAI integration. This application provides an interactive chat interface with streaming responses and token usage tracking.

## Features

- **Interactive Chat Interface**: Console-based chat with Azure OpenAI
- **Streaming Responses**: Real-time streaming of AI responses
- **Token Usage Tracking**: Monitor input, output, and total token consumption
- **Chat History Management**: Automatic chat history truncation to manage context
- **Configurable Settings**: Easy configuration through `appsettings.json` and user secrets
- **Microsoft Semantic Kernel Integration**: Leverages the latest Semantic Kernel framework

## Prerequisites

- .NET 9.0 SDK
- Azure OpenAI resource with a deployed model
- Visual Studio 2022 or Visual Studio Code (optional)

## Configuration

### 1. Azure OpenAI Setup

1. Create an Azure OpenAI resource in the Azure portal
2. Deploy a chat completion model (e.g., GPT-3.5-turbo or GPT-4)
3. Note down your:
   - Endpoint URL
   - API Key
   - Deployment Name

### 2. Application Configuration

Update the `appsettings.json` file with your Azure OpenAI credentials:

```json
{
  "AzureOpenAI": {
    "Endpoint": "https://your-resource-name.openai.azure.com/",
    "ApiKey": "your-api-key-here",
    "DeploymentName": "your-deployment-name"
  }
}
```

**Security Note**: For production use, consider using Azure Key Vault or user secrets instead of storing the API key in `appsettings.json`.

### 3. Using User Secrets (Recommended)

For development, use user secrets to store sensitive information:

```bash
dotnet user-secrets init
dotnet user-secrets set "AzureOpenAI:ApiKey" "your-actual-api-key"
dotnet user-secrets set "AzureOpenAI:Endpoint" "https://your-resource-name.openai.azure.com/"
dotnet user-secrets set "AzureOpenAI:DeploymentName" "your-deployment-name"
```

## Installation and Setup

1. **Clone the repository**:
   ```bash
   git clone <repository-url>
   cd SimanticApp
   ```

2. **Navigate to the project directory**:
   ```bash
   cd AzureOpenAI
   ```

3. **Restore NuGet packages**:
   ```bash
   dotnet restore
   ```

4. **Configure your Azure OpenAI settings** (see Configuration section above)

5. **Build the application**:
   ```bash
   dotnet build
   ```

## Usage

1. **Run the application**:
   ```bash
   dotnet run
   ```

2. **Start chatting**:
   - Enter your prompts when prompted
   - The AI will respond with streaming text
   - Token usage information is displayed after each response
   - Press Enter with an empty prompt to exit

### Example Interaction

```
Enter your prompt: Hello, how are you today?

Hello! I'm doing well, thank you for asking. I'm here and ready to help you with any questions or tasks you might have. How are you doing today? Is there anything specific I can assist you with?

        Input Tokens:      15
        Output Tokens:     45
        Total Tokens:      60

Enter your prompt: 
```

## Key Components

### Dependencies

- **Microsoft.SemanticKernel**: Core framework for AI orchestration
- **Microsoft.SemanticKernel.Connectors.AzureOpenAI**: Azure OpenAI connector
- **Microsoft.Extensions.Configuration**: Configuration management
- **Microsoft.Extensions.DependencyInjection**: Dependency injection
- **Microsoft.Extensions.Hosting**: Application hosting
- **Microsoft.Extensions.Logging**: Logging framework

### Chat History Management

The application includes chat history truncation to manage token limits:
- **ChatHistoryTruncationReducer**: Keeps the most recent 10 messages
- Alternative summarization reducer available (commented out)

### OpenAI Settings

- **Temperature**: 0.9 (controls response creativity)
- **MaxTokens**: 1500 (maximum response length)
- **System Prompt**: Configured for friendly AI assistant behavior

## Project Structure

```
AzureOpenAI/
├── Program.cs              # Main application entry point
├── AzureOpenAI.csproj     # Project file with dependencies
├── appsettings.json       # Configuration file
└── bin/                   # Build output directory
```

## Troubleshooting

### Common Issues

1. **Authentication Error**: Verify your API key and endpoint URL
2. **Model Not Found**: Ensure your deployment name matches the actual deployment
3. **Rate Limiting**: Check your Azure OpenAI quota and usage limits
4. **Network Issues**: Verify connectivity to Azure OpenAI endpoint

### Logging

The application uses Microsoft.Extensions.Logging for diagnostics. Check the console output for detailed error messages.

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Submit a pull request

## License

This project is licensed under the terms specified in the LICENSE.txt file.

## Support

For issues and questions:
- Check the troubleshooting section above
- Review Azure OpenAI documentation
- Consult Microsoft Semantic Kernel documentation

## Version History

- **Current**: Initial release with basic chat functionality and streaming responses