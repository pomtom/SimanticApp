# Semantic.WebApp - AI Assistant

A modern Blazor Server application that provides an AI-powered chat interface using Semantic Kernel and Azure OpenAI. This application demonstrates clean plugin architecture, responsive UI design, and seamless AI integration without any JavaScript dependencies in the core functionality.

## 🌟 Features

- **Pure Blazor Implementation**: No JavaScript required for core functionality - fully vanilla Blazor
- **Modern, Responsive UI**: Clean and friendly chat interface with Bootstrap 5 and Font Awesome icons
- **Plugin Architecture**: Extensible plugin system inspired by the Chat application
- **Real-time Chat**: Interactive conversation with AI assistant
- **Typing Indicators**: Visual feedback when AI is processing responses
- **Markdown Support**: Formatted responses with basic markdown rendering
- **Keyboard Shortcuts**: Ctrl+Enter to send messages
- **Auto-scroll**: Automatic scrolling to latest messages

## 🏗️ Architecture

### Plugin System
The application uses Semantic Kernel's plugin architecture with the following built-in plugins:

1. **GetDateTime**: Provides current date and time information
2. **PersonalInfo**: Returns user profile information
3. **GetWeather**: Fetches 7-day weather forecasts using NOAA API
4. **GetGeoCoordinates**: Geocodes addresses to latitude/longitude coordinates

### Project Structure
```
Semantic.WebApp/
├── Models/
│   └── ChatModel.cs          # Chat conversation model
├── Services/
│   └── ChatService.cs        # AI chat service implementation
├── Plugins/
│   ├── GetDateTime.cs        # Date/time plugin
│   ├── PersonalInfo.cs       # User information plugin
│   ├── GetWeather.cs         # Weather forecast plugin
│   └── GetGeoCoordinates.cs  # Address geocoding plugin
├── Pages/
│   ├── Index.razor           # Main chat interface
│   ├── _Host.cshtml          # Host page
│   └── _Layout.cshtml        # Layout template
├── Shared/
│   └── MainLayout.razor      # Application layout
└── wwwroot/
    ├── css/site.css          # Custom styles and animations
    └── js/site.js            # Minimal JavaScript utilities
```

## 🚀 Getting Started

### Prerequisites
- .NET 9.0 SDK
- Azure OpenAI Service account
- Geocoding API key (for address lookup functionality)

### Configuration
1. Update `appsettings.json` with your API credentials:
```json
{
  "AZURE_OPENAI_CHAT_DEPLOYMENT": "your-gpt-deployment",
  "AZURE_OPENAI_ENDPOINT": "your-azure-endpoint",
  "AZURE_OPENAI_KEY": "your-api-key",
  "GEOCODING_API_KEY": "your-geocoding-key"
}
```

### Running the Application
```bash
cd Semantic.WebApp
dotnet run
```

Navigate to `https://localhost:7111` to access the application.

## 💡 Key Design Principles

### 1. Vanilla Blazor Approach
- No external JavaScript frameworks
- Pure server-side Blazor components
- Minimal JavaScript only for DOM utilities (scrolling)

### 2. Plugin Extensibility
- Easy to add new AI capabilities
- Clean separation of concerns
- Semantic Kernel integration

### 3. Responsive Design
- Mobile-friendly interface
- Adaptive layout
- Smooth animations and transitions

### 4. User Experience
- Clear visual indicators for AI processing
- Intuitive chat interface
- Keyboard shortcuts for power users

## 🎨 UI Components

### Chat Interface
- **Message Bubbles**: Distinct styling for user and AI messages
- **Typing Indicator**: Animated dots showing AI is thinking
- **Auto-scroll**: Smooth scrolling to new messages
- **Clear Chat**: Reset conversation button

### Input Area
- **Multi-line Input**: Textarea with resize capability
- **Send Button**: Disabled state management
- **Validation**: Form validation with error messages
- **Keyboard Support**: Ctrl+Enter to submit

## 🔧 Customization

### Adding New Plugins
1. Create a new class in the `Plugins` folder
2. Implement Semantic Kernel attributes:
   ```csharp
   [KernelFunction("function_name")]
   [Description("Function description")]
   public async Task<string> YourFunction(string parameter)
   {
       // Implementation
   }
   ```
3. Register the plugin in `Program.cs`:
   ```csharp
   kernelBuilder.Plugins.AddFromType<YourPlugin>();
   ```

### Customizing UI
- Modify `wwwroot/css/site.css` for styling changes
- Update `Pages/Index.razor` for layout modifications
- Customize animations and colors as needed

## 🛠️ Technical Details

### Dependencies
- **Microsoft.SemanticKernel**: AI orchestration and plugin management
- **Microsoft.SemanticKernel.Plugins.OpenApi**: OpenAPI plugin support
- **Bootstrap 5**: CSS framework for responsive design
- **Font Awesome**: Icon library

### Browser Compatibility
- Modern browsers supporting Blazor Server
- SignalR for real-time communication
- WebSocket support required

## 📝 Comparison with Chat Application

The Semantic.WebApp builds upon the plugin architecture from the original Chat application with these improvements:

1. **Modern UI**: Updated to Blazor with responsive design
2. **No JavaScript Dependencies**: Pure Blazor implementation
3. **Better UX**: Typing indicators, animations, and keyboard shortcuts
4. **Cleaner Architecture**: Separated services and improved organization
5. **Enhanced Styling**: Modern CSS with animations and better visuals

## 🤝 Contributing

Feel free to extend this application with additional plugins, UI improvements, or new features. The modular architecture makes it easy to add new capabilities while maintaining clean separation of concerns.

---

*Built with ❤️ using Blazor Server and Semantic Kernel*