using System.ComponentModel.DataAnnotations;
namespace Simantic.ChatAI.Configuration;
public abstract class LlmProviderConfiguration
{
    [Required]
    public required string ProviderId { get; set; }
    [Required]
    public required string DisplayName { get; set; }
    public bool IsEnabled { get; set; } = true;
    public abstract bool IsOnline { get; }
    public string? DefaultModelId { get; set; }
    public abstract bool IsValid();
}
public class AzureOpenAIConfiguration : LlmProviderConfiguration
{
    public override bool IsOnline => true;
    [Required]
    public required string Endpoint { get; set; }
    [Required]
    public required string ApiKey { get; set; }
    [Required]
    public required string DeploymentName { get; set; }
    public override bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Endpoint) &&
               !string.IsNullOrWhiteSpace(ApiKey) &&
               !string.IsNullOrWhiteSpace(DeploymentName) &&
               Uri.TryCreate(Endpoint, UriKind.Absolute, out _);
    }
}
public class OpenAIConfiguration : LlmProviderConfiguration
{
    public override bool IsOnline => true;
    [Required]
    public required string ApiKey { get; set; }
    [Required]
    public required string ModelId { get; set; }
    public string? OrganizationId { get; set; }
    public override bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(ApiKey) &&
               !string.IsNullOrWhiteSpace(ModelId);
    }
}
public class HuggingFaceConfiguration : LlmProviderConfiguration
{
    public override bool IsOnline => true;
    [Required]
    public required string ApiKey { get; set; }
    [Required]
    public required string ModelId { get; set; }
    [Required]
    public required string Endpoint { get; set; }
    public override bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(ApiKey) &&
               !string.IsNullOrWhiteSpace(ModelId) &&
               !string.IsNullOrWhiteSpace(Endpoint) &&
               Uri.TryCreate(Endpoint, UriKind.Absolute, out _);
    }
}
public class OllamaConfiguration : LlmProviderConfiguration
{
    public override bool IsOnline => false;
    [Required]
    public required string Endpoint { get; set; }
    [Required]
    public required string ModelId { get; set; }
    public override bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Endpoint) &&
               !string.IsNullOrWhiteSpace(ModelId) &&
               Uri.TryCreate(Endpoint, UriKind.Absolute, out _);
    }
}
public class LMStudioConfiguration : LlmProviderConfiguration
{
    public override bool IsOnline => false;
    [Required]
    public required string Endpoint { get; set; }
    [Required]
    public required string ModelId { get; set; }
    public override bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Endpoint) &&
               !string.IsNullOrWhiteSpace(ModelId) &&
               Uri.TryCreate(Endpoint, UriKind.Absolute, out _);
    }
}
public class AzureAIInferenceConfiguration : LlmProviderConfiguration
{
    public override bool IsOnline => true;
    [Required]
    public required string ApiKey { get; set; }
    [Required]
    public required string ModelId { get; set; }
    [Required]
    public required string Endpoint { get; set; }
    public override bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(ApiKey) &&
               !string.IsNullOrWhiteSpace(ModelId) &&
               !string.IsNullOrWhiteSpace(Endpoint) &&
               Uri.TryCreate(Endpoint, UriKind.Absolute, out _);
    }
}
