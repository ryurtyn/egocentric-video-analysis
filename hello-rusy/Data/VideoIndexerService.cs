using System;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Core;
using Azure.Storage.Blobs.Models;

namespace hello_rusy.Data
{
    /// <summary>
    /// Video indexer service interacts with the video indexer API to generate video insights 
    /// </summary>
	public class VideoIndexerService
	{
        /// <summary>
        /// searches through all videos to find the target video. Then requests the indexer result of that video.
        /// </summary>
        /// <param name="targetName"> desired video name </param>
        /// <param name="config"> configuration object </param>
        /// <returns> video indexer result object </returns>
        public async Task<VideoIndexerResult> GetVideoInsights(string targetName, EgocentricVideoConfig config)
        {
            VideoIndexerList listOfResults = await GetAllVideos(config);
            VideoIndexerResult? foundResult = null;
            VideoIndexerResult? resultIndex = null;

            foreach (var result in listOfResults.Results)
            {
                if (result.Name == targetName)
                {
                    foundResult = result;
                    break;
                }
            }
            if (foundResult != null)
            {
                resultIndex = await RequestResult(foundResult.VideoId!, config);
            }
            return resultIndex;
        }

        /// <summary>
        /// Calls video indexer API to list all videos 
        /// </summary>
        /// <param name="config"> configuration object </param>
        /// <returns> video indexer result object </returns>
        public async Task<VideoIndexerList> GetAllVideos(EgocentricVideoConfig config)
        {
            HttpClient client = new HttpClient();
            string url = $"https://api.videoindexer.ai/{config.videoIndexerLocation}/Accounts/{config.videoIndexerAccountId}/Videos?accessToken={config.videoIndexerApiKey}";
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", config.videoIndexerSubscriptionKey);
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", config.videoIndexerApiKey);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            string jsonContent = await response.Content.ReadAsStringAsync();
            VideoIndexerList listResult = JsonSerializer.Deserialize<VideoIndexerList>(jsonContent)!;
            return listResult;
        }

        /// <summary>
        /// Calls video indexer API to retrieve insights for a single video
        /// </summary>
        /// <param name="videoId"> video indexer video ID </param>
        /// <param name="config"> configuration object </param>
        /// <returns> video indexer result object </returns>
        private async Task<VideoIndexerResult> RequestResult(string videoId, EgocentricVideoConfig config)
        {
            HttpClient client = new HttpClient();
            string url = $"https://api.videoindexer.ai/{config.videoIndexerLocation}/Accounts/{config.videoIndexerAccountId}/Videos/{videoId}/Index?accessToken={config.videoIndexerApiKey}";
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", config.videoIndexerSubscriptionKey);
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", config.videoIndexerApiKey);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            string jsonContent = await response.Content.ReadAsStringAsync();
            VideoIndexerResult indexResult = JsonSerializer.Deserialize<VideoIndexerResult>(jsonContent)!;
            return indexResult;
        }
    }
}

