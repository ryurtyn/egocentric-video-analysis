using System;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Text;
using System.Text.Json;
using System.IO;
using System.Threading.RateLimiting;

namespace hello_rusy.Data
{
	public class VideoMetadataService
	{

        public async Task UploadMetadata(string videoName, string metadataName, string fileContents, EgocentricVideoConfig config)
        {
            // Create a BlobServiceClient
            var blobServiceClient = new BlobServiceClient(config.dataFileConnectionString);

            // Create or reference an existing container
            var containerClient = blobServiceClient.GetBlobContainerClient(config.dataFileContainerName);

            string filename = videoName;

            // Define the directory path and the full blob name
            string directoryPath = $"{filename}";
            string blobName = $"{directoryPath}/{metadataName}";
            Console.WriteLine("BLOB UPLOAD NAME: " + blobName);


            //string jsonContent = JsonSerializer.Serialize(fileContents);
            byte[] byteArray = Encoding.UTF8.GetBytes(fileContents);
            using var ms = new MemoryStream(byteArray);

            // Upload the generalInfo.json file to the directory
            var blobClient = containerClient.GetBlobClient(blobName);
            await blobClient.UploadAsync(ms, new BlobHttpHeaders { ContentType = "application/json" }, conditions: null);
        }

        public async Task<string> DownloadMetadata(string videoName, string metadataName, EgocentricVideoConfig config)
        {
            // Create a BlobServiceClient
            var blobServiceClient = new BlobServiceClient(config.dataFileConnectionString);

            // Create or reference an existing container
            var containerClient = blobServiceClient.GetBlobContainerClient(config.dataFileContainerName);

            string videoFilename = videoName;

            string blobName = $"{videoFilename}/{metadataName}";
            
            Console.WriteLine("BLOB NAME: " + blobName);
            var blobClient = containerClient.GetBlobClient(blobName);

            // Download the blob's content
            BlobDownloadInfo download = await blobClient.DownloadAsync();

            // Deserialize the JSON content to an VideoMetadata object
            using (var streamReader = new StreamReader(download.Content))
            {
                string json = await streamReader.ReadToEndAsync();
                return json;
            }
        }

        public async Task UpdateTitleMappings(string videoName, string summarizedTitle, EgocentricVideoConfig config)
        {
            // get the videos <-> summaries mapping json from blob storage
            // Create a BlobServiceClient
            var blobServiceClient = new BlobServiceClient(config.dataFileConnectionString);

            // Create or reference an existing container
            var containerClient = blobServiceClient.GetBlobContainerClient(config.dataFileContainerName);

            // Construct the path to the generalInfo.json file
            string blobName = $"titleMappings.json";
            Console.WriteLine("BLOB NAME: " + blobName);
            var blobClient = containerClient.GetBlobClient(blobName);

            // Download the blob's content
            BlobDownloadInfo download = await blobClient.DownloadAsync();

            string json;
            // Deserialize the JSON content to an VideoMetadata object
            using (var streamReader = new StreamReader(download.Content))
            {
                json = await streamReader.ReadToEndAsync();
            }

            // serialize it as whatever structure you give it
            TitleMappings titleMappings = JsonSerializer.Deserialize<TitleMappings>(json);
            // add to the (front of) list of pairings
            foreach (Mapping mapping in titleMappings.filesList)
            {
                if (mapping.videoName.Equals(videoName)) {
                    mapping.summarizedTitle = summarizedTitle;
                }
            }

            // upload to update the file
            string jsonContent = JsonSerializer.Serialize(titleMappings);
            byte[] byteArray = Encoding.UTF8.GetBytes(jsonContent);
            using var ms = new MemoryStream(byteArray);

            // Upload the generalInfo.json file to the directory
            await blobClient.UploadAsync(ms, new BlobHttpHeaders { ContentType = "application/json" }, conditions: null);
        }

        public async Task UpdateTitleMappingsProcessTime(string videoName, DateTime processedTime, EgocentricVideoConfig config)
        {
            // get the videos <-> summaries mapping json from blob storage
            // Create a BlobServiceClient
            var blobServiceClient = new BlobServiceClient(config.dataFileConnectionString);

            // Create or reference an existing container
            var containerClient = blobServiceClient.GetBlobContainerClient(config.dataFileContainerName);

            // Construct the path to the generalInfo.json file
            string blobName = $"titleMappings.json";
            Console.WriteLine("BLOB NAME: " + blobName);
            var blobClient = containerClient.GetBlobClient(blobName);

            // Download the blob's content
            BlobDownloadInfo download = await blobClient.DownloadAsync();

            string json;
            // Deserialize the JSON content to an VideoMetadata object
            using (var streamReader = new StreamReader(download.Content))
            {
                json = await streamReader.ReadToEndAsync();
            }

            // serialize it as whatever structure you give it
            TitleMappings titleMappings = JsonSerializer.Deserialize<TitleMappings>(json);
            // add to the (front of) list of pairings
            foreach (Mapping mapping in titleMappings.filesList)
            {
                if (mapping.videoName.Equals(videoName))
                {
                    mapping.ProcessedDate = processedTime;
                }
            }

            // upload to update the file
            string jsonContent = JsonSerializer.Serialize(titleMappings);
            byte[] byteArray = Encoding.UTF8.GetBytes(jsonContent);
            using var ms = new MemoryStream(byteArray);

            // Upload the generalInfo.json file to the directory
            await blobClient.UploadAsync(ms, new BlobHttpHeaders { ContentType = "application/json" }, conditions: null);
        }


        public async Task<string> DownloadTitleMappings(EgocentricVideoConfig config)
        {
            // Create a BlobServiceClient
            var blobServiceClient = new BlobServiceClient(config.dataFileConnectionString);

            // Create or reference an existing container
            var containerClient = blobServiceClient.GetBlobContainerClient(config.dataFileContainerName);

            // TODO: can make this a configuration later (processed-video-information) 
            string blobName = $"titleMappings.json";
            Console.WriteLine("BLOB NAME: " + blobName);
            var blobClient = containerClient.GetBlobClient(blobName);

            // Download the blob's content
            BlobDownloadInfo download = await blobClient.DownloadAsync();

            // Deserialize the JSON content to an VideoMetadata object
            using (var streamReader = new StreamReader(download.Content))
            {
                string json = await streamReader.ReadToEndAsync();
                return json;
            }
        }


        

    }
}

