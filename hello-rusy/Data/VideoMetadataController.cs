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
        /// Should call all 3 or 4 services and get them in JSON format 
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="blobConnectionString"></param>
        /// <returns></returns>
        public async Task GenerateAllMetadata(string videoName, EgocentricVideoConfig config)
        {
            VideoIndexerMetadata videoIndexerMetadata = await GenerateVideoIndexerMetadata(videoName, config);
            List<string> transcripts = videoIndexerMetadata.Transcripts;
            List<string> timestamps = videoIndexerMetadata.Timestamps;
            // GenerateOpenAIMetadata
            ToDoList openAIMetadata = await GenerateOpenAIMetadata(videoName, transcripts, timestamps, config);
            // TODO: GenerateGeneralMetadata
            //bool generalSuccess = await GenerateGeneralMetadata(videoName, blobConnectionString);
            // TODO: GenerateLanguageServiceMetadata
            string textSummaryResult = await GenerateTitleSummary(videoName, transcripts, config);
            // TODO: call update videos list with the updated summarized title to update 
            // return ??? maybe a json of all the jsons? or have a separate public class for each that will be called by the uploader
            VideoMetadata videoMetadata = await GenerateGeneralMetadata(videoName, config);
        }

        //public async Task<string> RetrieveDataToDisplay(string videoName, string blobConnectionString)
        //{
        //    VideoMetadata videoMetadata = await RetrieveGeneralMetadata(videoName, blobConnectionString);
        //    string videoUrl = videoMetadata.VideoUrl;
        //    ToDoList toDoList = await RetrieveOpenAIMetadata(videoName, blobConnectionString);

        //}

        public async Task<VideoIndexerMetadata> GenerateVideoIndexerMetadata(string videoName, EgocentricVideoConfig config)
        {
            VideoIndexerResult videoIndexerResult = await videoIndexerServiceInstance.GetVideoInsights(videoName, config);
            VideoIndexerMetadata videoIndexerMetadata = videoIndexerResult.ConvertToVideoIndexerMetadata(config);
            string videoIndexerJson = JsonSerializer.Serialize(videoIndexerMetadata);
            await videoMetadataServiceInstance.UploadMetadata(videoName, "videoIndexerMetadata.json", videoIndexerJson, config);
            Console.WriteLine("VIDEO NAME: " + videoName);
            return videoIndexerMetadata;
        }

        public async Task<VideoIndexerMetadata> RetrieveVideoIndexerMetadata(string videoName, EgocentricVideoConfig config) 
        {
            string jsonMetadata = await videoMetadataServiceInstance.DownloadMetadata(videoName, "videoIndexerMetadata.json", config);
            Console.WriteLine("METADATA: " + jsonMetadata);
            VideoIndexerMetadata videoIndexerMetadata = JsonSerializer.Deserialize<VideoIndexerMetadata>(jsonMetadata);

            Console.WriteLine("METADATA CONVERTED: " + videoIndexerMetadata);
            return videoIndexerMetadata;
        }


        public async Task<ToDoList> GenerateOpenAIMetadata(string videoName, List<string> transcripts, List<string> timestamps, EgocentricVideoConfig config)
        {
            ToDoList todos = await openAIServiceInstance.RequestChatResponse(transcripts, timestamps, config);
            // I don't really thing I need it to be converted to another format ?? at least not for now. It's a to do list format
            string todosJson = JsonSerializer.Serialize(todos);
            await videoMetadataServiceInstance.UploadMetadata(videoName, "openAIMetadata.json", todosJson, config);
            return todos;
        }

        public async Task<ToDoList> RetrieveOpenAIMetadata(string videoName, EgocentricVideoConfig config)
        {
            string jsonMetadata = await videoMetadataServiceInstance.DownloadMetadata(videoName, "openAIMetadata.json", config);
            Console.WriteLine("METADATA: " + jsonMetadata);
            ToDoList toDoList = JsonSerializer.Deserialize<ToDoList>(jsonMetadata);

            Console.WriteLine("METADATA CONVERTED: " + toDoList);
            return toDoList;
        }

        public async Task<VideoMetadata> GenerateGeneralMetadata(string videoName, EgocentricVideoConfig config)
        {
            VideoMetadata videoMetadata = await RetrieveGeneralMetadata(videoName, config);
            videoMetadata.ProcessedDate = DateTime.Now;
            string generalMetadataJson = JsonSerializer.Serialize(videoMetadata);
            await videoMetadataServiceInstance.UploadMetadata(videoName, "generalInfo.json", generalMetadataJson, config);

            await videoMetadataServiceInstance.UpdateTitleMappingsProcessTime(videoName, videoMetadata.ProcessedDate, config);

            return videoMetadata;
        }

        public async Task<VideoMetadata> RetrieveGeneralMetadata(string videoName, EgocentricVideoConfig config)
        {
            string jsonMetadata = await videoMetadataServiceInstance.DownloadMetadata(videoName, "generalInfo.json", config);
            VideoMetadata videoMetadata = JsonSerializer.Deserialize<VideoMetadata>(jsonMetadata);
            return videoMetadata;
        }

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

        public async Task<TitleMappings> RetrieveTitleMappings(EgocentricVideoConfig config)
        {

            string jsonMappings = await videoMetadataServiceInstance.DownloadTitleMappings(config);
            TitleMappings titleMappings = JsonSerializer.Deserialize<TitleMappings>(jsonMappings);
            return titleMappings;
        }

    }
}

