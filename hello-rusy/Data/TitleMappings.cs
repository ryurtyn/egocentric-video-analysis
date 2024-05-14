using System;
using System.Text.Json.Serialization;

namespace hello_rusy.Data
{
    /// <summary>
    /// object that maps files to summarized titles 
    /// </summary>
	public class TitleMappings
	{
        [JsonPropertyName("FilesList")]
        public List<Mapping> filesList { get; set; }

    }

    public class Mapping
    {
        [JsonPropertyName("VideoName")]
        public string videoName { get; set; }

        [JsonPropertyName("SummarizedTitle")]
        public string summarizedTitle { get; set; }

        [JsonPropertyName("IsProcessing")]
        public bool isProcessing { get; set; }

        [JsonPropertyName("CreatedDate")]
        public DateTime CreatedDate { get; set; }

        [JsonPropertyName("ProcessedDate")]
        public DateTime ProcessedDate { get; set; }
    }
}

