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
        public async Task Run([BlobTrigger("%AzureBLobStorageInputVideoFilesContainerName%/{name}.mp4", Connection = "AzureBlobStorageInputVideoFilesConnectionString")] Stream myBlob, string name, ILogger log, Uri uri)
        {
            // try to read the uploaded file and log something (some metadata or something)
            // want to log the name, and maybe the length/size of the video
            log.LogInformation($"Blob URI: {uri}");
            log.LogInformation($"C# Blob trigger function got blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

            // upload file to video indexer & start indexing


            ProcessVideoFileConfig config = new ProcessVideoFileConfig(
                apiKey: Environment.GetEnvironmentVariable("VideoIndexerApiKey"),
                accountId: Environment.GetEnvironmentVariable("VideoIndexerAccountId"),
                accountName: Environment.GetEnvironmentVariable("VideoIndexerAccountName"),
                bearerAuthKey: Environment.GetEnvironmentVariable("BearerAuthKey"),
                location: Environment.GetEnvironmentVariable("VideoIndexerLocation"),
                resourceGroupName: Environment.GetEnvironmentVariable("VideoIndexerResourceGroup"),
                subscriptionId: Environment.GetEnvironmentVariable("VideoIndexerSubscriptionId"),
                subscriptionKey: Environment.GetEnvironmentVariable("VideoIndexerSubscriptionKey"),
                inputVideoConnectionString: Environment.GetEnvironmentVariable("AzureBlobStorageInputVideoFilesConnectionString"),
                inputVideoContainerName: Environment.GetEnvironmentVariable("AzureBlobStorageInputVideoFilesContainerName"),
                dataFileConnectionString: Environment.GetEnvironmentVariable("AzureBlobStorageDataFilesConnectionString"),
                dataFileContainerName: Environment.GetEnvironmentVariable("AzureBlobStorageDataFilesContainerName")
                );

            //string bearerAuthKey = Environment.GetEnvironmentVariable("BearerAuthKey");
            try
            {
                // Generate an ARM access token 
                var accessToken = await GetAccessToken(config, log);
                log.LogInformation("access token generated");
                log.LogInformation($"f 1");
                await UploadVideoAndIndex(config, name, accessToken, log, uri);
                // Create metadata skeleton and upload to blob storage
                log.LogInformation($"f 2");
                await SendToBlobStorage(config, name, uri, log);
                // Update the videos list to contain the new video
                log.LogInformation($"f 3");
                await UpdateVideosList(config, name, log);
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

        private static async Task<string> GetAccessToken(ProcessVideoFileConfig config, ILogger log)
        {
            
            string url = $"https://management.azure.com/subscriptions/{config.subscriptionId}/resourceGroups/{config.resourceGroupName}/providers/Microsoft.VideoIndexer/accounts/{config.accountName}/generateAccessToken?api-version=2024-01-01";
            string old_url = "https://management.azure.com/subscriptions/2a2133fc-9811-4822-bf15-8c3c74f5973c/resourceGroups/hello-rusy-resource-group/providers/Microsoft.VideoIndexer/accounts/videoIndexerTester/generateAccessToken?api-version=2024-01-01";
            // TODO: need to swap out the bearer thing to put in the api key from appsettings
            // https://learn.microsoft.com/en-us/rest/api/videoindexer/generate/access-token?view=rest-videoindexer-2024-01-01&tabs=HTTP#code-try-0
            // TODO: will need to update this every few hours
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Authorization", config.bearerAuthKey); // this is ARM token 
            log.LogInformation($"get access token API URL: {url}");
            log.LogInformation($"get access token old URL: {old_url}");
            log.LogInformation($"line 1");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = new StringContent("{permissionType: \"Contributor\",scope: \"Account\"}",
                                                Encoding.UTF8,
                                                "application/json");//CONTENT-TYPE header

            var response = await client.SendAsync(request);

            log.LogInformation($"line 2");
            response.EnsureSuccessStatusCode();

            string jsonContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(jsonContent);
            string accessToken = tokenResponse.accessToken;

            return accessToken.Trim('"');
        }

        private static async Task UploadVideoAndIndex(ProcessVideoFileConfig config, string fileName, string accessToken, ILogger log, Uri uri)
        {
            string url = $"https://api.videoindexer.ai/{config.location}/Accounts/{config.accountId}/Videos?name={fileName}&videoUrl={uri}&accessToken={accessToken}";
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", config.subscriptionKey);

            // Add the Authorization header with the access token.
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            log.LogInformation($"Upload URL: {url}");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            log.LogInformation($"Video uploaded and indexing started. Response: {responseBody}");
        }

        public static async Task SendToBlobStorage(ProcessVideoFileConfig config ,string filePath, Uri uri, ILogger log)
        {
            // Create a BlobServiceClient

            string storageConnectionString = config.dataFileConnectionString;
            var blobServiceClient = new BlobServiceClient(storageConnectionString);

            // Create or reference an existing container
            string containerName = config.dataFileContainerName;
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName); // rusycontainertest --> processsedvideoinfo --> filename  | processed-video-information/filename

            // Split the full path by the '/' character
            string filename = filePath;
            // Define the directory path and the full blob name
            string directoryPath = $"{filename}"; // processed-video-information/filename
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
            log.LogInformation($"blob client {blobClient.Name}, {blobClient.BlobContainerName}");
            await blobClient.UploadAsync(ms, new BlobHttpHeaders { ContentType = "application/json" }, conditions: null);
            log.LogInformation("File uploaded to Blob Storage.");

            ////One time, upload new mappings
            //TitleMappings newTitleMapping = new TitleMappings()
            //{
            //    filesList = new List<Mapping>()
            //};
            //await UploadNewMappings(config, newTitleMapping, log);
            ////Update the title mappings to include the new video
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

            [JsonPropertyName("CreatedDate")]
            public DateTime CreatedDate { get; set; }

            [JsonPropertyName("ProcessedDate")]
            public DateTime ProcessedDate { get; set; }
        }


        public static async Task UpdateVideosList(ProcessVideoFileConfig config, string filePath, ILogger log)
        {
            TitleMappings titleMappings = await DownloadCurrentMappings(config, log);
            log.LogInformation("title mappings downloaded");
            Mapping newMapping = new Mapping()
            {
                videoName = filePath,
                summarizedTitle = "",
                CreatedDate = DateTime.Now
            };
            // might be inefficient call 
            titleMappings.filesList.Insert(0, newMapping);

            await UploadNewMappings(config, titleMappings, log);

        }

        public static async Task<TitleMappings> DownloadCurrentMappings(ProcessVideoFileConfig config, ILogger log)
        {
            // Create a BlobServiceClient
            string storageConnectionString = config.dataFileConnectionString;
            //string storageConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings:AzureBlobStorage");
            var blobServiceClient = new BlobServiceClient(storageConnectionString);

            // Create or reference an existing container
            var containerClient = blobServiceClient.GetBlobContainerClient(config.dataFileContainerName);

            string blobName = $"titleMappings.json";//$"processed-video-information/titleMappings.json";
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


        public static async Task UploadNewMappings(ProcessVideoFileConfig config, TitleMappings updatedMappings, ILogger log)
        {
            // Create a BlobServiceClient
            string storageConnectionString = config.dataFileConnectionString; // Environment.GetEnvironmentVariable("ConnectionStrings:AzureBlobStorage");
            var blobServiceClient = new BlobServiceClient(storageConnectionString);

            // Create or reference an existing container
            var containerClient = blobServiceClient.GetBlobContainerClient(config.dataFileContainerName); //"rusycontainertest"

            string jsonContent = JsonSerializer.Serialize(updatedMappings);
            byte[] byteArray = Encoding.UTF8.GetBytes(jsonContent);
            using var ms = new MemoryStream(byteArray);

            string blobName = $"titleMappings.json"; // processed-video-information
            var blobClient = containerClient.GetBlobClient(blobName);
            await blobClient.UploadAsync(ms, new BlobHttpHeaders { ContentType = "application/json" }, conditions: null);
        }
    }

    public class ProcessVideoFileConfig
    {
        public ProcessVideoFileConfig(
            string apiKey,
            string accountId,
            string accountName,
            string bearerAuthKey,
            string location,
            string resourceGroupName,
            string subscriptionId,
            string subscriptionKey,
            string inputVideoConnectionString,
            string inputVideoContainerName,
            string dataFileConnectionString,
            string dataFileContainerName
            )
        {
            this.apiKey = apiKey;
            this.accountId = accountId;
            this.accountName = accountName;
            this.bearerAuthKey = bearerAuthKey;
            this.location = location;
            this.resourceGroupName = resourceGroupName;
            this.subscriptionId = subscriptionId;
            this.subscriptionKey = subscriptionKey;
            this.inputVideoConnectionString = inputVideoConnectionString;
            this.inputVideoContainerName = inputVideoContainerName;
            this.dataFileConnectionString = dataFileConnectionString;
            this.dataFileContainerName = dataFileContainerName;
        }

        public string apiKey { get; }
        public string accountId { get;}
        public string accountName { get; }
        public string bearerAuthKey { get; }
        public string location { get; }
        public string resourceGroupName { get;}
        public string subscriptionId { get; }
        public string subscriptionKey { get; }
        public string inputVideoConnectionString { get; }
        public string inputVideoContainerName { get; }
        public string dataFileConnectionString { get; }
        public string dataFileContainerName { get; }

    }
}

