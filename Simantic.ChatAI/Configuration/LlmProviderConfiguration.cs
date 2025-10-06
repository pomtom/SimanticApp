using System.ComponentModel.DataAnnotations;

namespace Simantic.ChatAI.Configuration;

/// <summary>
/// Base configuration class for all LLM providers
/// </summary>
public abstract class LlmProviderConfiguration
{
    /// <summary>
    /// Unique identifier for the provider
    /// </summary>
    [Required]
    public required string ProviderId { get; set; }

    /// <summary>
    /// Display name for the provider
    /// </summary>
    [Required]
    public required string DisplayName { get; set; }

    /// <summary>
    /// Whether this provider is enabled
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Whether this provider requires an internet connection
    /// </summary>
    public abstract bool IsOnline { get; }

    /// <summary>
    /// Default model ID for this provider
    /// </summary>
    public string? DefaultModelId { get; set; }

    /// <summary>
    /// Validates the configuration
    /// </summary>
    /// <returns>True if valid, false otherwise</returns>
    public abstract bool IsValid();
}

/// <summary>
/// Configuration for Azure OpenAI provider
/// </summary>
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

/// <summary>
/// Configuration for OpenAI provider
/// </summary>
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

/// <summary>
/// Configuration for HuggingFace provider
/// </summary>
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

/// <summary>
/// Configuration for Ollama provider (local)
/// </summary>
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

/// <summary>
/// Configuration for LM Studio provider (local)
/// </summary>
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

/// <summary>
/// Configuration for Azure AI Inference provider
/// </summary>
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