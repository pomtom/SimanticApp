using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Semantic.WebApp.Plugins;
using Semantic.WebApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add ServiceDefaults
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Add HttpClient factory for plugins
builder.Services.AddHttpClient();

// Add Semantic Kernel
var kernelBuilder = builder.Services.AddKernel();

// Register plugins
kernelBuilder.Plugins.AddFromType<GetDateTime>();
kernelBuilder.Plugins.AddFromType<GetWeather>();
kernelBuilder.Plugins.AddFromType<GetGeoCoordinates>();
kernelBuilder.Plugins.AddFromType<PersonalInfo>();

// Add Azure OpenAI Service
builder.Services.AddAzureOpenAIChatCompletion(
    deploymentName: builder.Configuration.GetValue<string>("AZURE_OPENAI_CHAT_DEPLOYMENT") ?? "gpt-4",
    endpoint: builder.Configuration.GetValue<string>("AZURE_OPENAI_ENDPOINT") ?? "",
    apiKey: builder.Configuration.GetValue<string>("AZURE_OPENAI_KEY") ?? "");

// Configure prompt execution settings
builder.Services.AddTransient<PromptExecutionSettings>(_ => new OpenAIPromptExecutionSettings
{
    Temperature = 0.7,
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(options: new FunctionChoiceBehaviorOptions 
    { 
        AllowConcurrentInvocation = false 
    })
});

// Register chat service
builder.Services.AddScoped<IChatService, ChatService>();

var app = builder.Build();

// Map default endpoints for service discovery
app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
