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
    }
}

