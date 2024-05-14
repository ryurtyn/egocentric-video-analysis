using System;
using System.Text.Json.Serialization;

namespace hello_rusy.Data
{
    /// <summary>
    /// Metadata format to save video indexer results 
    /// </summary>
	public class VideoIndexerMetadata
	{
        [JsonPropertyName("transcripts")]
        public List<string> Transcripts { get; set; }

        [JsonPropertyName("timestamps")]
        public List<string> Timestamps { get; set; }

        [JsonPropertyName("keyframeShots")]
        public List<List<string>> KeyframeShots { get; set; }

        [JsonPropertyName("labels")]
        public List<string> Labels { get; set; }

        [JsonPropertyName("topics")]
        public List<string> Topics { get; set; }

        [JsonPropertyName("keywords")]
        public List<string> Keywords { get; set; }
    }
}

