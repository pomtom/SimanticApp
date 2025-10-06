# Simantic.ChatAI - Unified LLM Provider Integration

A comprehensive .NET 9 console application that provides a unified interface to interact with multiple Large Language Model (LLM) providers through a single, modular architecture.

## 🎯 Features

- **Unified Interface**: Single application to chat with multiple LLM providers
- **Factory Pattern**: Clean provider instantiation and management
- **Runtime Provider Switching**: Switch between providers during chat sessions
- **Streaming Support**: Real-time streaming responses from all supported providers
- **Token Usage Tracking**: Monitor token consumption where available
- **Chat History Management**: Automatic history truncation and management
- **Configuration-Driven**: Easy provider setup through `appsettings.json` and user secrets
- **Dependency Injection**: Clean, testable architecture with DI container
- **Comprehensive Logging**: Structured logging throughout the application
- **Error Handling**: Robust error handling with graceful degradation

## 🔌 Supported Providers

### Online Providers
- **Azure OpenAI** - Microsoft's Azure-hosted OpenAI models
- **OpenAI** - Direct OpenAI API integration
- **Hugging Face** - Hugging Face Inference API
- **Azure AI Inference** - GitHub Models and other Azure AI services

### Local/Offline Providers
- **Ollama** - Local LLM hosting platform
- **LM Studio** - Local model inference server

## 🏗️ Architecture

The application follows Clean Architecture principles with clear separation of concerns:

```
Simantic.ChatAI/
├── Configuration/          # Provider and app configuration classes
├── Interfaces/             # Service contracts and abstractions
├── Models/                 # Data models and DTOs
├── Providers/              # LLM provider factory implementation
├── Services/               # Core business logic services
└── Program.cs              # Application entry point and DI setup
```

### Key Components

- **ILlmProviderFactory**: Factory for creating provider-specific services
- **IChatService**: Main chat interface with provider switching
- **IConfigurationService**: Configuration management and validation
- **ChatApplication**: Console UI and user interaction handling

## 🚀 Getting Started

### Prerequisites

- .NET 9.0 SDK
- At least one configured LLM provider (see Configuration section)

### Installation

1. **Clone and navigate to the project**:
   ```bash
   cd Simantic.ChatAI
   ```

2. **Restore packages**:
   ```bash
   dotnet restore
   ```

3. **Configure providers** (see Configuration section below)

4. **Build and run**:
   ```bash
   dotnet run
   ```

## ⚙️ Configuration

### Application Settings

Configure the main application settings in `appsettings.json`:

```json
{
  "ChatAI": {
    "DefaultProvider": "AzureOpenAI",
    "MaxChatHistoryMessages": 10,
    "DefaultSystemMessage": "You are a friendly AI Assistant",
    "DefaultTemperature": 0.9,
    "DefaultMaxTokens": 1500
  }
}
```

### Provider Configuration

Each provider has its own configuration section:

#### Azure OpenAI
```json
{
  "AzureOpenAI": {
    "Endpoint": "https://your-resource.openai.azure.com/",
    "ApiKey": "your-api-key",
    "DeploymentName": "gpt-4o"
  }
}
```

#### OpenAI
```json
{
  "OpenAI": {
    "ApiKey": "your-api-key",
    "ModelId": "gpt-4o-mini",
    "OrganizationId": "optional-org-id"
  }
}
```

#### Hugging Face
```json
{
  "HuggingFace": {
    "ApiKey": "your-api-key",
    "ModelId": "microsoft/DialoGPT-medium",
    "Endpoint": "https://api-inference.huggingface.co"
  }
}
```

#### Ollama (Local)
```json
{
  "Ollama": {
    "Endpoint": "http://localhost:11434",
    "ModelId": "llama2"
  }
}
```

#### LM Studio (Local)
```json
{
  "LMStudio": {
    "Endpoint": "http://localhost:1234/v1",
    "ModelId": "local-model"
  }
}
```

#### Azure AI Inference
```json
{
  "AzureAIInference": {
    "ApiKey": "your-api-key",
    "ModelId": "gpt-4o",
    "Endpoint": "https://models.github.ai/inference"
  }
}
```

### 🔐 Using User Secrets (Recommended)

For security, store API keys in user secrets:

```bash
dotnet user-secrets init
dotnet user-secrets set "AzureOpenAI:ApiKey" "your-actual-api-key"
dotnet user-secrets set "OpenAI:ApiKey" "your-openai-api-key"
dotnet user-secrets set "HuggingFace:ApiKey" "your-hf-token"
```

## 💬 Usage

### Interactive Commands

Once running, you can use these commands:

- **Chat**: Simply type your message and press Enter
- **/switch <provider>**: Switch to a different provider
- **/providers**: List all available providers
- **/clear**: Clear chat history
- **/history**: Display current chat history
- **/help**: Show help information
- **/quit** or **/exit**: Exit the application

### Example Session

```
🤖 Welcome to Simantic.ChatAI - Unified LLM Provider Interface
================================================================

Current Provider: AzureOpenAI

Available Providers:
-------------------
  Azure OpenAI (AzureOpenAI) - Online - ✅ Available (Current)
  OpenAI (OpenAI) - Online - ✅ Available
  Ollama (Ollama) - Local - ❌ Not Available

Enter your message: Hello, how are you today?

Hello! I'm doing well, thank you for asking. I'm here and ready to help you with any questions or tasks you might have. How are you doing today?

📊 Token Usage: Input: 15, Output: 34, Total: 49

Enter your message: /switch OpenAI
✅ Switched to provider: OpenAI

Enter your message: What's the weather like?
```

## 🧪 Testing Different Providers

The application makes it easy to test and compare different providers:

1. Configure multiple providers in your settings
2. Start a conversation with one provider
3. Use `/switch <provider>` to change providers mid-conversation
4. Compare responses, performance, and token usage

## 🏛️ Architecture Details

### Factory Pattern Implementation

The `LlmProviderFactory` implements a clean factory pattern that:
- Manages provider lifecycle and caching
- Handles provider-specific initialization
- Provides consistent error handling
- Supports dependency injection

### Configuration Validation

Each provider configuration includes:
- Required field validation
- Connection endpoint validation
- Runtime availability checking
- Graceful error handling for missing configurations

### Memory Management

The application properly manages resources:
- Disposes HTTP clients and services
- Implements IDisposable pattern throughout
- Caches provider instances to avoid recreation
- Manages chat history to prevent memory leaks

## 🔧 Troubleshooting

### Common Issues

1. **Provider Not Available**: Check configuration and network connectivity
2. **API Key Errors**: Verify API keys in user secrets or appsettings.json
3. **Local Provider Issues**: Ensure Ollama/LM Studio is running locally
4. **SSL Certificate Errors**: The app handles common SSL issues for development

### Logging

Enable debug logging by updating `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Simantic.ChatAI": "Debug"
    }
  }
}
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Follow the existing architectural patterns
4. Add appropriate tests
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the LICENSE.txt file for details.

## 🙏 Acknowledgments

- Microsoft Semantic Kernel for the unified LLM abstraction
- All the amazing LLM providers for their APIs and services
- The .NET community for excellent tooling and libraries