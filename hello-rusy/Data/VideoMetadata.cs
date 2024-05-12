using System;
using System.Text.Json.Serialization;

namespace hello_rusy.Data
{
	public class VideoMetadata
	{
        [JsonPropertyName("FileName")]
        public string Filename { get; set; }

        [JsonPropertyName("VideoUrl")]
        public string VideoUrl { get; set; }

        [JsonPropertyName("SummarizedTitle")]
        public string SummarizedTitle { get; set; }

        [JsonPropertyName("CreatedDate")]
        public DateTime CreatedDate { get; set; }

        [JsonPropertyName("ProcessedDate")]
        public DateTime ProcessedDate { get; set; }

    }
}

