using System;
using System.Text.Json.Serialization;

namespace hello_rusy.Data
{
	public class VideoIndexerMetadata
	{
        [JsonPropertyName("transcripts")]
        public List<string> Transcripts { get; set; }

        [JsonPropertyName("timestamps")]
        public List<string> Timestamps { get; set; }

        [JsonPropertyName("keyframeShots")]
        public List<List<string>> KeyframeShots { get; set; }


    }
}

