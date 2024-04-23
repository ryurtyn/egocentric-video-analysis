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
                // TODO: uncomment this line when you want the video indexer running again.
                //await UploadVideoAndIndex(name, accessToken, accountId, location, log, uri);
                // TODO: upload metadata to azure storage
                await SendToBlobStorage(name, uri, log);
                log.LogInformation("blob uploaded");
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

        // takes in the filename+UID and creates new blob storage folder inside the location rusycontainertest/processed-video-information
        // then creates a file called generalInfo.json in that folder that holds metadata (filename, video url, etc)
        // TODO: write this method
        public static async Task SendToBlobStorage(string filename, Uri uri, ILogger log)
        {
            // Create a BlobServiceClient
            string storageConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings:AzureBlobStorage");
            var blobServiceClient = new BlobServiceClient(storageConnectionString);

            // Create or reference an existing container
            var containerClient = blobServiceClient.GetBlobContainerClient("rusycontainertest");
            //await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

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
        }
    }
}

