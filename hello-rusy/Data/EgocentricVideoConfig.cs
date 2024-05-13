using System;
namespace hello_rusy.Data
{
    /// <summary>
    /// Configuration settings needed to process the video, including API keys and endpoint URLs
    /// </summary>
    public class EgocentricVideoConfig
	{
		public EgocentricVideoConfig(
			string videoIndexerApiKey,
			string videoIndexerAccountId,
			string videoIndexerLocation,
			string videoIndexerSubscriptionKey,
			string dataFileConnectionString,
			string dataFileContainerName,
			string openAIApiKey,
			string languageServiceApiKey
            )
		{
			this.videoIndexerApiKey = videoIndexerApiKey;
			this.videoIndexerAccountId = videoIndexerAccountId;
			this.videoIndexerLocation = videoIndexerLocation;
			this.videoIndexerSubscriptionKey = videoIndexerSubscriptionKey;
            this.dataFileConnectionString = dataFileConnectionString;
            this.dataFileContainerName = dataFileContainerName;
			this.openAIApiKey = openAIApiKey;
			this.languageServiceApiKey = languageServiceApiKey;
        }

		public string videoIndexerApiKey { get; }
		public string videoIndexerAccountId { get; }
		public string videoIndexerLocation { get; }
		public string videoIndexerSubscriptionKey { get; }
		public string dataFileConnectionString { get; }
		public string dataFileContainerName { get; }
		public string openAIApiKey { get; }
		public string languageServiceApiKey { get; }

    }
}

