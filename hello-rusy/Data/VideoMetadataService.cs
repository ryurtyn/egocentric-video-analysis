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

        public async Task UploadMetadata(string videoName, string metadataName, string fileContents, string blobConnectionString)
        {
            // Create a BlobServiceClient
            var blobServiceClient = new BlobServiceClient(blobConnectionString);

            // Create or reference an existing container
            var containerClient = blobServiceClient.GetBlobContainerClient("rusycontainertest");

            // Split the full path by the '/' character
            string[] parts = videoName.Split('/');

            // The part after the '/' will be the last element of the array
            string filename = "";
            if (parts.Length > 1)
            {
                filename = parts[1]; // returns the second part, after the '/'
            }
            else
            {
                Console.WriteLine("FILENAME ERROR: " + videoName); // returns the original path if no '/' found
            }
            // Define the directory path and the full blob name
            string directoryPath = $"processed-video-information/{filename}";
            string blobName = $"{directoryPath}/{metadataName}";
            Console.WriteLine("BLOB UPLOAD NAME: " + blobName);


            //string jsonContent = JsonSerializer.Serialize(fileContents);
            byte[] byteArray = Encoding.UTF8.GetBytes(fileContents);
            using var ms = new MemoryStream(byteArray);

            // Upload the generalInfo.json file to the directory
            var blobClient = containerClient.GetBlobClient(blobName);
            await blobClient.UploadAsync(ms, new BlobHttpHeaders { ContentType = "application/json" }, conditions: null);
        }

        public async Task<string> DownloadMetadata(string videoName, string metadataName, string blobConnectionString)
        {
            // Create a BlobServiceClient
            var blobServiceClient = new BlobServiceClient(blobConnectionString);

            // Create or reference an existing container
            var containerClient = blobServiceClient.GetBlobContainerClient("rusycontainertest");

            // Split the full path by the '/' character
            string[] parts = videoName.Split('/');

            // The part after the '/' will be the last element of the array
            string videoFilename = "";
            if (parts.Length > 1)
            {
                videoFilename = parts[1]; // returns the second part, after the '/'
            }
            else
            {
                Console.WriteLine("FILENAME ERROR: " + videoName); // returns the original path if no '/' found
            }

            // Construct the path to the generalInfo.json file
            // TODO: can make this a configuration later (processed-video-information) 
            string blobName = $"processed-video-information/{videoFilename}/{metadataName}";
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

        public async Task UpdateTitleMappings(string videoName, string summarizedTitle, string blobConnectionString)
        {
            // get the videos <-> summaries mapping json from blob storage
            // Create a BlobServiceClient
            var blobServiceClient = new BlobServiceClient(blobConnectionString);

            // Create or reference an existing container
            var containerClient = blobServiceClient.GetBlobContainerClient("rusycontainertest");

            // Construct the path to the generalInfo.json file
            // TODO: can make this a configuration later (processed-video-information) 
            string blobName = $"processed-video-information/titleMappings.json";
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

        public async Task<string> DownloadTitleMappings(string blobConnectionString)
        {
            // Create a BlobServiceClient
            var blobServiceClient = new BlobServiceClient(blobConnectionString);

            // Create or reference an existing container
            var containerClient = blobServiceClient.GetBlobContainerClient("rusycontainertest");

            // TODO: can make this a configuration later (processed-video-information) 
            string blobName = $"processed-video-information/titleMappings.json";
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

