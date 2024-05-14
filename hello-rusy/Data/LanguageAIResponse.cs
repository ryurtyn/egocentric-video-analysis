using System;
using System.Formats.Asn1;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace hello_rusy.Data
{
    /// <summary>
    /// Input to the Language service API 
    /// </summary>
    public partial class SummarizeTextInput
    {
        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; }

        [JsonPropertyName("analysisInput")]
        public AnalysisInput AnalysisInput { get; set; }

        [JsonPropertyName("tasks")]
        public LanguageTask[] Tasks { get; set; }
    }

    public partial class AnalysisInput
    {
        [JsonPropertyName("conversations")]
        public ConversationInput[] Conversations { get; set; }
    }

    public partial class ConversationInput
    {
        [JsonPropertyName("conversationItems")]
        public List<ConversationItem> ConversationItems { get; set; }

        [JsonPropertyName("modality")]
        public string Modality { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("language")]
        public string Language { get; set; }
    }

    public partial class ConversationItem
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonPropertyName("participantId")]
        public string ParticipantId { get; set; }
    }

    public partial class LanguageTask
    {
        [JsonPropertyName("taskName")]
        public string TaskName { get; set; }

        [JsonPropertyName("kind")]
        public string Kind { get; set; }

        [JsonPropertyName("parameters")]
        public Parameters Parameters { get; set; }
    }

    public partial class Parameters
    {
        [JsonPropertyName("summaryAspects")]
        public string[] SummaryAspects { get; set; }
    }


    /// <summary>
    /// Output from language service API call 
    /// </summary>
    public partial class TextSummarizerResult
    {
        [JsonPropertyName("jobId")]
        public Guid JobId { get; set; }

        [JsonPropertyName("lastUpdatedDateTime")]
        public DateTimeOffset LastUpdatedDateTime { get; set; }

        [JsonPropertyName("createdDateTime")]
        public DateTimeOffset CreatedDateTime { get; set; }

        [JsonPropertyName("expirationDateTime")]
        public DateTimeOffset ExpirationDateTime { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("errors")]
        public object[] Errors { get; set; }

        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; }

        [JsonPropertyName("tasks")]
        public Tasks Tasks { get; set; }
    }

    public partial class Tasks
    {
        [JsonPropertyName("completed")]
        public long Completed { get; set; }

        [JsonPropertyName("failed")]
        public long Failed { get; set; }

        [JsonPropertyName("inProgress")]
        public long InProgress { get; set; }

        [JsonPropertyName("total")]
        public long Total { get; set; }

        [JsonPropertyName("items")]
        public Item[] Items { get; set; }
    }

    public partial class Item
    {
        [JsonPropertyName("kind")]
        public string Kind { get; set; }

        [JsonPropertyName("taskName")]
        public string TaskName { get; set; }

        [JsonPropertyName("lastUpdateDateTime")]
        public DateTimeOffset LastUpdateDateTime { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("results")]
        public Results Results { get; set; }
    }

    public partial class Results
    {
        [JsonPropertyName("conversations")]
        public Conversation[] Conversations { get; set; }

        [JsonPropertyName("errors")]
        public object[] Errors { get; set; }

        [JsonPropertyName("modelVersion")]
        public string ModelVersion { get; set; }
    }

    public partial class Conversation
    {
        [JsonPropertyName("summaries")]
        public Summary[] Summaries { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("warnings")]
        public object[] Warnings { get; set; }
    }

    public partial class Summary
    {
        [JsonPropertyName("aspect")]
        public string Aspect { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("contexts")]
        public Context[] Contexts { get; set; }
    }

    public partial class Context
    {
        [JsonPropertyName("conversationItemId")]
        public string ConversationItemId { get; set; }

        [JsonPropertyName("offset")]
        public long Offset { get; set; }

        [JsonPropertyName("length")]
        public long Length { get; set; }
    }



    
}

