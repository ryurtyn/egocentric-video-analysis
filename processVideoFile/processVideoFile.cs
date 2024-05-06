using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace processVideoFile
{
    public class processVideoFile
    {
        private static readonly HttpClient client = new HttpClient();

        // Function is triggered when a file is uploaded to the blob storage container 
        [FunctionName("processVideoFile")]
        public async Task Run([BlobTrigger("rusycontainertest/{name}.mp4", Connection = "ConnectionStrings:AzureBlobStorage")] Stream myBlob, string name, ILogger log, Uri uri)
        {
            // try to read the uploaded file and log something (some metadata or something)
            // want to log the name, and maybe the length/size of the video
            log.LogInformation($"Blob URI: {uri}");
            log.LogInformation($"C# Blob trigger function got blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

            // upload file to video indexer & start indexing
            string apiKey = Environment.GetEnvironmentVariable("VideoIndexerApiKey");
            string accountId = Environment.GetEnvironmentVariable("VideoIndexerAccountId");
            string location = Environment.GetEnvironmentVariable("VideoIndexerLocation"); // For example, "trial" or "westus2"
            //string bearerAuthKey = Environment.GetEnvironmentVariable("BearerAuthKey");
            try
            {
                // Generate an ARM access token 
                var accessToken = await GetAccessToken(apiKey, accountId, location, log);
                log.LogInformation("access token generated");
                // Upload video to Video Indexer
                await UploadVideoAndIndex(name, accessToken, accountId, location, log, uri);
                // Create metadata skeleton and upload to blob storage
                await SendToBlobStorage(name, uri, log);
                // Update the videos list to contain the new video
                await UpdateVideosList(name, log);
            }
            catch (Exception ex)
            {
                log.LogError($"Error processing video file: {ex.Message}");
            }
        }

        public class TokenResponse
        {
            public string accessToken { get; set; }
        }

        private static async Task<string> GetAccessToken(string apiKey, string accountId, string location, ILogger log)
        {
            string url = "https://management.azure.com/subscriptions/2a2133fc-9811-4822-bf15-8c3c74f5973c/resourceGroups/hello-rusy-resource-group/providers/Microsoft.VideoIndexer/accounts/videoIndexerTester/generateAccessToken?api-version=2024-01-01";
            // TODO: need to swap out the bearer thing to put in the api key from appsettings
            // https://learn.microsoft.com/en-us/rest/api/videoindexer/generate/access-token?view=rest-videoindexer-2024-01-01&tabs=HTTP#code-try-0
            // TODO: will need to update this every few hours
            // TODO: put accesstoken in environment variable in the function
            client.DefaultRequestHeaders.Clear();
            string authorizationkey = Environment.GetEnvironmentVariable("BearerAuthKey");
            client.DefaultRequestHeaders.Add("Authorization", authorizationkey); // this is ARM token 

            log.LogInformation($"line 1");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = new StringContent("{permissionType: \"Contributor\",scope: \"Account\"}",
                                                Encoding.UTF8,
                                                "application/json");//CONTENT-TYPE header

            var response = await client.SendAsync(request);

            //log.LogInformation($"line 2");
            response.EnsureSuccessStatusCode();

            string jsonContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(jsonContent);
            string accessToken = tokenResponse.accessToken;

            return accessToken.Trim('"');
        }

        private static async Task UploadVideoAndIndex(string fileName, string accessToken, string accountId, string location, ILogger log, Uri uri)
        {
            string url = $"https://api.videoindexer.ai/{location}/Accounts/{accountId}/Videos?name={fileName}&videoUrl={uri}&accessToken={accessToken}";
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Environment.GetEnvironmentVariable("VideoIndexerSubscriptionKey"));

            // Add the Authorization header with the access token.
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            log.LogInformation($"Upload URL: {url}");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            log.LogInformation($"Video uploaded and indexing started. Response: {responseBody}");
        }

        public static async Task SendToBlobStorage(string filePath, Uri uri, ILogger log)
        {
            // Create a BlobServiceClient
            string storageConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings:AzureBlobStorage");
            var blobServiceClient = new BlobServiceClient(storageConnectionString);

            // Create or reference an existing container
            var containerClient = blobServiceClient.GetBlobContainerClient("rusycontainertest");

            // Split the full path by the '/' character
            string[] parts = filePath.Split('/');

            // The part after the '/' will be the last element of the array
            string filename = "";
            if (parts.Length > 1)
            {
                filename = parts[1]; // returns the second part, after the '/'
            }
            else
            {
                Console.WriteLine("FILENAME ERROR: " + filePath); // returns the original path if no '/' found
            }
            // Define the directory path and the full blob name
            string directoryPath = $"processed-video-information/{filename}";
            string blobName = $"{directoryPath}/generalInfo.json";
            log.LogInformation("Blob Name: " + blobName);
            // Prepare the file content as JSON
            var metadataContent = new
            {
                FileName = filename,
                VideoUrl = uri
            };

            string jsonContent = JsonSerializer.Serialize(metadataContent);
            log.LogInformation("Metadata: " + jsonContent);
            byte[] byteArray = Encoding.UTF8.GetBytes(jsonContent);
            using var ms = new MemoryStream(byteArray);

            // Upload the generalInfo.json file to the directory
            var blobClient = containerClient.GetBlobClient(blobName);
            await blobClient.UploadAsync(ms, new BlobHttpHeaders { ContentType = "application/json" }, conditions: null);
            log.LogInformation("File uploaded to Blob Storage.");

            // One time, upload new mappings
            //TitleMappings newTitleMapping = new TitleMappings()
            //{
            //    filesList = new List<Mapping>()
            //};
            //await UploadNewMappings(newTitleMapping, log);
            // Update the title mappings to include the new video
        }

        /// <summary>
        /// Probably need to restructure this stuff ! maybe put it in its own file
        /// also this is a copy of the TitleMappings in hello rusy .data 
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

            [JsonPropertyName("Created")]
            public string created { get; set; }
        }


        public static async Task UpdateVideosList(string filePath, ILogger log)
        {
            TitleMappings titleMappings = await DownloadCurrentMappings(log);
            Mapping newMapping = new Mapping()
            {
                videoName = filePath,
                summarizedTitle = ""
            };
            // might be inefficient call 
            titleMappings.filesList.Insert(0, newMapping);

            await UploadNewMappings(titleMappings, log);

        }

        public static async Task<TitleMappings> DownloadCurrentMappings(ILogger log)
        {
            // Create a BlobServiceClient
            string storageConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings:AzureBlobStorage");
            var blobServiceClient = new BlobServiceClient(storageConnectionString);

            // Create or reference an existing container
            var containerClient = blobServiceClient.GetBlobContainerClient("rusycontainertest");

            string blobName = $"processed-video-information/titleMappings.json";
            var blobClient = containerClient.GetBlobClient(blobName);
            BlobDownloadInfo download = await blobClient.DownloadAsync();

            string json;
            // Deserialize the JSON content to an VideoMetadata object
            using (var streamReader = new StreamReader(download.Content))
            {
                json = await streamReader.ReadToEndAsync();
            }

            // serialize it as whatever structure you give it
            TitleMappings titleMappings = JsonSerializer.Deserialize<TitleMappings>(json);
            return titleMappings;
        }


        public static async Task UploadNewMappings(TitleMappings updatedMappings, ILogger log)
        {
            // Create a BlobServiceClient
            string storageConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings:AzureBlobStorage");
            var blobServiceClient = new BlobServiceClient(storageConnectionString);

            // Create or reference an existing container
            var containerClient = blobServiceClient.GetBlobContainerClient("rusycontainertest");

            string jsonContent = JsonSerializer.Serialize(updatedMappings);
            byte[] byteArray = Encoding.UTF8.GetBytes(jsonContent);
            using var ms = new MemoryStream(byteArray);

            string blobName = $"processed-video-information/titleMappings.json";
            var blobClient = containerClient.GetBlobClient(blobName);
            await blobClient.UploadAsync(ms, new BlobHttpHeaders { ContentType = "application/json" }, conditions: null);
        }
    }
}

