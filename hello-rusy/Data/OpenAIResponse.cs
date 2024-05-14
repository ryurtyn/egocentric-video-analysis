using System;
using System.Text.Json.Serialization;
namespace hello_rusy.Data
{
    /// <summary>
    /// output to do list object 
    /// </summary>
    public class ToDoList
    {
        [JsonPropertyName("todos")]
        public List<ToDoItem> ToDos { get; set; }
    }

    public class ToDoItem
    {
        [JsonPropertyName("index")]
        public int index { get; set; }

        [JsonPropertyName("timestamp")]
        public string Timestamp { get; set; }

        [JsonPropertyName("task")]
        public string Task { get; set; }
    }

    /// <summary>
    /// Input for body to Chat Completion API Call 
    /// </summary>
    public class OpenAIRequest
	{
        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("messages")]
        public List<ChatMessage> Messages { get; set; }

    }

    public class ChatMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }
    }


    /// <summary>
    /// Response from API Call to Chat Completion
    /// </summary>    
    public partial class OpenAiResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("object")]
        public string Object { get; set; }

        [JsonPropertyName("created")]
        public long Created { get; set; }

        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("choices")]
        public Choice[] Choices { get; set; }

        [JsonPropertyName("usage")]
        public Usage Usage { get; set; }

        [JsonPropertyName("system_fingerprint")]
        public string SystemFingerprint { get; set; }
    }

    public partial class Choice
    {
        [JsonPropertyName("index")]
        public long Index { get; set; }

        [JsonPropertyName("message")]
        public ChatMessage Message { get; set; }

        [JsonPropertyName("logprobs")]
        public object Logprobs { get; set; }

        [JsonPropertyName("finish_reason")]
        public string FinishReason { get; set; }
    }

    public partial class Usage
    {
        [JsonPropertyName("prompt_tokens")]
        public long PromptTokens { get; set; }

        [JsonPropertyName("completion_tokens")]
        public long CompletionTokens { get; set; }

        [JsonPropertyName("total_tokens")]
        public long TotalTokens { get; set; }
    }
}

