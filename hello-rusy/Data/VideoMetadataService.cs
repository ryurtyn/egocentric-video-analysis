using System;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Text;
using System.Text.Json;
using System.IO;
using System.Threading.RateLimiting;

namespace hello_rusy.Data
{
    /// <summary>
    /// Service interacts with blob storage to save and load metadata files 
    /// </summary>
	public class VideoMetadataService
	{
        /// <summary>
        /// Uploads given file contents to blob storage under the specified filename
        /// </summary>
        /// <param name="videoName"> name of video in blob storage </param>
        /// <param name="metadataName"> name of destination metadata file </param>
        /// <param name="fileContents"> contents of metadata file </param>
        /// <param name="config"> configuration object </param>
        /// <returns></returns>
        public async Task UploadMetadata(string videoName, string metadataName, string fileContents, EgocentricVideoConfig config)
        {
            var blobServiceClient = new BlobServiceClient(config.dataFileConnectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(config.dataFileContainerName);
            string filename = videoName;
            string directoryPath = $"{filename}";
            string blobName = $"{directoryPath}/{metadataName}";
            byte[] byteArray = Encoding.UTF8.GetBytes(fileContents);
            using var ms = new MemoryStream(byteArray);
            var blobClient = containerClient.GetBlobClient(blobName);
            await blobClient.UploadAsync(ms, new BlobHttpHeaders { ContentType = "application/json" }, conditions: null);
        }

        /// <summary>
        /// Download contents of blob with specified filename 
        /// </summary>
        /// <param name="videoName"> name of video file in blob storage </param>
        /// <param name="metadataName"> name of metadata file in blob storage </param>
        /// <param name="config"> configuration object </param>
        /// <returns> metadata file contents </returns>
        public async Task<string> DownloadMetadata(string videoName, string metadataName, EgocentricVideoConfig config)
        {
            var blobServiceClient = new BlobServiceClient(config.dataFileConnectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(config.dataFileContainerName);
            string videoFilename = videoName;
            string blobName = $"{videoFilename}/{metadataName}";
            var blobClient = containerClient.GetBlobClient(blobName);
            BlobDownloadInfo download = await blobClient.DownloadAsync();
            using (var streamReader = new StreamReader(download.Content))
            {
                string json = await streamReader.ReadToEndAsync();
                return json;
            }
        }

        /// <summary>
        /// Updates the list of files and their associated titles with new file and title 
        /// </summary>
        /// <param name="videoName"> name of video in blob storage </param>
        /// <param name="summarizedTitle"> generated title for that video </param>
        /// <param name="config"> configuration object </param>
        /// <returns></returns>
        public async Task UpdateTitleMappings(string videoName, string summarizedTitle, EgocentricVideoConfig config)
        {
            var blobServiceClient = new BlobServiceClient(config.dataFileConnectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(config.dataFileContainerName);
            string blobName = $"titleMappings.json";
            var blobClient = containerClient.GetBlobClient(blobName);
            BlobDownloadInfo download = await blobClient.DownloadAsync();
            string json;
            using (var streamReader = new StreamReader(download.Content))
            {
                json = await streamReader.ReadToEndAsync();
            }
            TitleMappings titleMappings = JsonSerializer.Deserialize<TitleMappings>(json);
            foreach (Mapping mapping in titleMappings.filesList)
            {
                if (mapping.videoName.Equals(videoName)) {
                    mapping.summarizedTitle = summarizedTitle;
                }
            }
            string jsonContent = JsonSerializer.Serialize(titleMappings);
            byte[] byteArray = Encoding.UTF8.GetBytes(jsonContent);
            using var ms = new MemoryStream(byteArray);
            await blobClient.UploadAsync(ms, new BlobHttpHeaders { ContentType = "application/json" }, conditions: null);
        }

        /// <summary>
        /// Updates the "time last processed" in the title mappings for a given video 
        /// </summary>
        /// <param name="videoName"> name of video in blob storage </param>
        /// <param name="processedTime"> time of processing </param>
        /// <param name="config"> configuration object </param>
        /// <returns></returns>
        public async Task UpdateTitleMappingsProcessTime(string videoName, DateTime processedTime, EgocentricVideoConfig config)
        {
            var blobServiceClient = new BlobServiceClient(config.dataFileConnectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(config.dataFileContainerName);
            string blobName = $"titleMappings.json";
            var blobClient = containerClient.GetBlobClient(blobName);
            BlobDownloadInfo download = await blobClient.DownloadAsync();
            string json;
            // Deserialize the JSON content to an VideoMetadata object
            using (var streamReader = new StreamReader(download.Content))
            {
                json = await streamReader.ReadToEndAsync();
            }
            TitleMappings titleMappings = JsonSerializer.Deserialize<TitleMappings>(json);
            foreach (Mapping mapping in titleMappings.filesList)
            {
                if (mapping.videoName.Equals(videoName))
                {
                    mapping.ProcessedDate = processedTime;
                }
            }
            string jsonContent = JsonSerializer.Serialize(titleMappings);
            byte[] byteArray = Encoding.UTF8.GetBytes(jsonContent);
            using var ms = new MemoryStream(byteArray);
            await blobClient.UploadAsync(ms, new BlobHttpHeaders { ContentType = "application/json" }, conditions: null);
        }

        /// <summary>
        /// Download the current mappings from filename to title 
        /// </summary>
        /// <param name="config"> configuration object </param>
        /// <returns> title mappings json string </returns>
        public async Task<string> DownloadTitleMappings(EgocentricVideoConfig config)
        {
            var blobServiceClient = new BlobServiceClient(config.dataFileConnectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(config.dataFileContainerName);
            string blobName = $"titleMappings.json";
            Console.WriteLine("BLOB NAME: " + blobName);
            var blobClient = containerClient.GetBlobClient(blobName);
            BlobDownloadInfo download = await blobClient.DownloadAsync();
            using (var streamReader = new StreamReader(download.Content))
            {
                string json = await streamReader.ReadToEndAsync();
                return json;
            }
        }
    }
}
