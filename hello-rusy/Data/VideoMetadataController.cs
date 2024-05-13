using System;
using Azure.Core;
using Azure.Storage.Blobs.Models;
using hello_rusy.Extensions;
using System.Text.Json;
using Microsoft.Extensions.Options;
using System.Text.Json.Serialization;


namespace hello_rusy.Data
{
    /// <summary>
    /// Collects Video Metadata and puts it in the format to be uploaded (JSON format) 
    /// </summary>
	public class VideoMetadataController
    {
        private VideoIndexerService videoIndexerServiceInstance;
        private OpenAIService openAIServiceInstance;
        private VideoMetadataService videoMetadataServiceInstance;
        private LanguageAIService languageAIServiceInstance;

        public VideoMetadataController(VideoIndexerService videoIndexerServiceInstance, OpenAIService openAIServiceInstance, VideoMetadataService videoMetadataServiceInstance, LanguageAIService languageAIServiceInstance)
        {
            this.videoIndexerServiceInstance = videoIndexerServiceInstance;
            this.openAIServiceInstance = openAIServiceInstance;
            this.videoMetadataServiceInstance = videoMetadataServiceInstance;
            this.languageAIServiceInstance = languageAIServiceInstance;
        }

        /// <summary>
        /// Calls all insights services to generate metadata
        /// </summary>
        /// <param name="videoName"> name of video in blob storage</param>
        /// <param name="config"> configuration object </param>
        /// <returns></returns>
        public async Task GenerateAllMetadata(string videoName, EgocentricVideoConfig config)
        {
            VideoIndexerMetadata videoIndexerMetadata = await GenerateVideoIndexerMetadata(videoName, config);
            List<string> transcripts = videoIndexerMetadata.Transcripts;
            List<string> timestamps = videoIndexerMetadata.Timestamps;
            ToDoList openAIMetadata = await GenerateOpenAIMetadata(videoName, transcripts, timestamps, config);
            string textSummaryResult = await GenerateTitleSummary(videoName, transcripts, config);
            VideoMetadata videoMetadata = await GenerateGeneralMetadata(videoName, config);
        }

        /// <summary>
        /// Calls the video indexer service to generate video indexer insights 
        /// </summary>
        /// <param name="videoName"> name of video in blob storage </param>
        /// <param name="config"> configuration object </param>
        /// <returns> video indexer metadata object </returns>
        public async Task<VideoIndexerMetadata> GenerateVideoIndexerMetadata(string videoName, EgocentricVideoConfig config)
        {
            VideoIndexerResult videoIndexerResult = await videoIndexerServiceInstance.GetVideoInsights(videoName, config);
            VideoIndexerMetadata videoIndexerMetadata = videoIndexerResult.ConvertToVideoIndexerMetadata(config);
            string videoIndexerJson = JsonSerializer.Serialize(videoIndexerMetadata);
            await videoMetadataServiceInstance.UploadMetadata(videoName, "videoIndexerMetadata.json", videoIndexerJson, config);
            return videoIndexerMetadata;
        }

        /// <summary>
        /// Retrieves video indexer metadata from blob storage 
        /// </summary>
        /// <param name="videoName"> name of video in blob storage </param>
        /// <param name="config"> configuration object </param>
        /// <returns> video indexer metadata object </returns>
        public async Task<VideoIndexerMetadata> RetrieveVideoIndexerMetadata(string videoName, EgocentricVideoConfig config) 
        {
            string jsonMetadata = await videoMetadataServiceInstance.DownloadMetadata(videoName, "videoIndexerMetadata.json", config);
            VideoIndexerMetadata videoIndexerMetadata = JsonSerializer.Deserialize<VideoIndexerMetadata>(jsonMetadata);
            return videoIndexerMetadata;
        }

        /// <summary>
        /// Calls the video indexer service to generate openAI metadata 
        /// </summary>
        /// <param name="videoName"> name of video in blob storage </param>
        /// <param name="transcripts"> list of video transcript strings </param>
        /// <param name="timestamps"> list of timestamps corresponding to video transcript items </param>
        /// <param name="config"> configuration object </param>
        /// <returns> to do list metadata object </returns>
        public async Task<ToDoList> GenerateOpenAIMetadata(string videoName, List<string> transcripts, List<string> timestamps, EgocentricVideoConfig config)
        {
            ToDoList todos = await openAIServiceInstance.RequestChatResponse(transcripts, timestamps, config);
            string todosJson = JsonSerializer.Serialize(todos);
            await videoMetadataServiceInstance.UploadMetadata(videoName, "openAIMetadata.json", todosJson, config);
            return todos;
        }

        /// <summary>
        /// Retrieves openAI metadata from blob storage 
        /// </summary>
        /// <param name="videoName"> name of video in blob storage </param>
        /// <param name="config"> configuration object </param>
        /// <returns> to do list metadata object </returns>
        public async Task<ToDoList> RetrieveOpenAIMetadata(string videoName, EgocentricVideoConfig config)
        {
            string jsonMetadata = await videoMetadataServiceInstance.DownloadMetadata(videoName, "openAIMetadata.json", config);
            ToDoList toDoList = JsonSerializer.Deserialize<ToDoList>(jsonMetadata);
            return toDoList;
        }

        /// <summary>
        /// Updates general metadata 
        /// </summary>
        /// <param name="videoName"> name of video in blob storage </param>
        /// <param name="config"> configuration object </param>
        /// <returns> video metadata object </returns>
        public async Task<VideoMetadata> GenerateGeneralMetadata(string videoName, EgocentricVideoConfig config)
        {
            VideoMetadata videoMetadata = await RetrieveGeneralMetadata(videoName, config);
            videoMetadata.ProcessedDate = DateTime.Now;
            string generalMetadataJson = JsonSerializer.Serialize(videoMetadata);
            await videoMetadataServiceInstance.UploadMetadata(videoName, "generalInfo.json", generalMetadataJson, config);
            await videoMetadataServiceInstance.UpdateTitleMappingsProcessTime(videoName, videoMetadata.ProcessedDate, config);
            return videoMetadata;
        }

        /// <summary>
        /// Retrieves general metadata from blob storage 
        /// </summary>
        /// <param name="videoName"> name of video in blob storage </param>
        /// <param name="config"> configuration object </param>
        /// <returns> video metadata object </returns>
        public async Task<VideoMetadata> RetrieveGeneralMetadata(string videoName, EgocentricVideoConfig config)
        {
            string jsonMetadata = await videoMetadataServiceInstance.DownloadMetadata(videoName, "generalInfo.json", config);
            VideoMetadata videoMetadata = JsonSerializer.Deserialize<VideoMetadata>(jsonMetadata);
            return videoMetadata;
        }

        /// <summary>
        /// Calls the language service to generate a title for a video
        /// </summary>
        /// <param name="videoName"> name of video in blob storage </param>
        /// <param name="transcripts"> list of video transcript strings </param>
        /// <param name="config"> configuration object </param>
        /// <returns> extracted title </returns>
        public async Task<string> GenerateTitleSummary(string videoName, List<string> transcripts, EgocentricVideoConfig config)
        {
            string summarizedTitle;
            if (transcripts.Count > 0)
            {
                TextSummarizerResult textSummaryResult = await languageAIServiceInstance.getTextSummary(config, transcripts);
                summarizedTitle = languageAIServiceInstance.GetSummarizedTitle(textSummaryResult);
                
            } else
            {
                 summarizedTitle = "No Title Found";
            }
            await videoMetadataServiceInstance.UpdateTitleMappings(videoName, summarizedTitle, config);
            VideoMetadata generalMetadata = await RetrieveGeneralMetadata(videoName, config);
            generalMetadata.SummarizedTitle = summarizedTitle;
            string generalMetadataJson = JsonSerializer.Serialize(generalMetadata);
            await videoMetadataServiceInstance.UploadMetadata(videoName, "generalInfo.json", generalMetadataJson, config);

            return summarizedTitle;

        }

        /// <summary>
        /// Retries the video title from storage 
        /// </summary>
        /// <param name="config"> configuration object </param>
        /// <returns> current title mappings </returns>
        public async Task<TitleMappings> RetrieveTitleMappings(EgocentricVideoConfig config)
        {
            string jsonMappings = await videoMetadataServiceInstance.DownloadTitleMappings(config);
            TitleMappings titleMappings = JsonSerializer.Deserialize<TitleMappings>(jsonMappings);
            return titleMappings;
        }
    }
}

