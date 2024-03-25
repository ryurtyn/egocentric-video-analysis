using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace hello_rusy.Data
{
    public class VideoIndexerList
    {
        [JsonPropertyName("results")]
        public List<VideoIndexerResult>? Results { get; set; }

        [JsonPropertyName("nextPage")]
        public NextPage? NextPage { get; set; }
    }

    public class VideoIndexerResult
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; } // filename

        [JsonPropertyName("state")]
        public string? State { get; set; } // processed/processing

        [JsonPropertyName("id")]
        public string? VideoId { get; set; } //video id

        [JsonPropertyName("videos")]
        public Video[]? Videos { get; set; }

        [JsonPropertyName("created")]
        public DateTimeOffset Created { get; set; }
    }

    public class NextPage
    {
        // Add properties as defined in your JSON, with JsonPropertyName attributes if necessary
    }

    public partial class Video
    {
        [JsonPropertyName("insights")]
        public Insights? Insights { get; set; }
    }

    public partial class Insights
    {

        [JsonPropertyName("duration")]
        public string? Duration { get; set; }

        [JsonPropertyName("transcript")]
        public List<Transcript>? Transcripts { get; set; }

        [JsonPropertyName("ocr")]
        public Ocr[]? Ocr { get; set; }

        [JsonPropertyName("keywords")]
        public Keyword[]? Keywords { get; set; }

        [JsonPropertyName("topics")]
        public Topic[]? Topics { get; set; }

        [JsonPropertyName("labels")]
        public Label[]? Labels { get; set; }

        [JsonPropertyName("shots")]
        public Shot[]? Shots { get; set; }

        [JsonPropertyName("sentiments")]
        public Sentiment[]? Sentiments { get; set; }

        [JsonPropertyName("detectedObjects")]
        public DetectedObject[] DetectedObjects { get; set; }
    }

    public partial class Transcript
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }

    public partial class Ocr
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("text")]
        public string? Text { get; set; }

        [JsonPropertyName("confidence")]
        public double Confidence { get; set; }

        [JsonPropertyName("left")]
        public long Left { get; set; }

        [JsonPropertyName("top")]
        public long Top { get; set; }

        [JsonPropertyName("width")]
        public long Width { get; set; }

        [JsonPropertyName("height")]
        public long Height { get; set; }

        [JsonPropertyName("angle")]
        public long Angle { get; set; }

        [JsonPropertyName("instances")]
        public Instance[]? Instances { get; set; }
    }


    public partial class Keyword
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("text")]
        public string? Text { get; set; }

        [JsonPropertyName("confidence")]
        public double Confidence { get; set; }

        [JsonPropertyName("instances")]
        public Instance[]? Instances { get; set; }

        [JsonPropertyName("speakerId")]
        public long? SpeakerId { get; set; }
    }


    public partial class Topic
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("referenceId")]
        public string? ReferenceId { get; set; }

        [JsonPropertyName("iptcName")]
        public string? IptcName { get; set; }

        [JsonPropertyName("confidence")]
        public double Confidence { get; set; }

        [JsonPropertyName("iabName")]
        public string? IabName { get; set; }

        [JsonPropertyName("instances")]
        public Instance[]? Instances { get; set; }
    }


    public partial class Label
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("instances")]
        public Instance[] Instances { get; set; }

        [JsonPropertyName("referenceId")]
        public string ReferenceId { get; set; }
    }

    public partial class Shot
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("keyFrames")]
        public Block[]? KeyFrames { get; set; }

        [JsonPropertyName("instances")]
        public Instance[] Instances { get; set; }

        [JsonPropertyName("tags")]
        public string[] Tags { get; set; }
    }

    public partial class Sentiment
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("averageScore")]
        public double AverageScore { get; set; }

        [JsonPropertyName("sentimentType")]
        public string SentimentType { get; set; }

        [JsonPropertyName("instances")]
        public Instance[] Instances { get; set; }
    }


    public partial class DetectedObject
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("thumbnailId")]
        public Guid ThumbnailId { get; set; }

        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; }

        [JsonPropertyName("wikiDataId")]
        public string WikiDataId { get; set; }

        [JsonPropertyName("instances")]
        public Instance[] Instances { get; set; }
    }

    public partial class Block
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("instances")]
        public Instance[] Instances { get; set; }
    }

    public partial class Instance
    {
        [JsonPropertyName("confidence")]
        public double? Confidence { get; set; }

        [JsonPropertyName("adjustedStart")]
        public string AdjustedStart { get; set; }

        [JsonPropertyName("adjustedEnd")]
        public string AdjustedEnd { get; set; }

        [JsonPropertyName("start")]
        public string Start { get; set; }

        [JsonPropertyName("end")]
        public string End { get; set; }

        [JsonPropertyName("thumbnailId")]
        public Guid? ThumbnailId { get; set; }
    }
}

