using System.ComponentModel.DataAnnotations;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Semantic.WebApp.Models
{
    public class ChatModel
    {
        public ChatModel()
        {
            ChatHistory = new ChatHistory("You are a friendly AI assistant that helps users with their questions. Always format your responses using markdown and be helpful and informative.");
        }

        public ChatModel(string systemMessage)
        {
            ChatHistory = new ChatHistory(systemMessage);
        }

        public ChatHistory ChatHistory { get; set; } = new();

        [Required(ErrorMessage = "Please enter a prompt")]
        [Display(Name = "Your Message")]
        public string Prompt { get; set; } = string.Empty;

        public bool IsProcessing { get; set; } = false;
    }
}