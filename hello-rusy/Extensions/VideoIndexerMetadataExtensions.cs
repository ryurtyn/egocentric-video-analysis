using System;
using hello_rusy.Data;

namespace hello_rusy.Extensions
{
	public static class VideoIndexerMetadataExtensions
	{

		public static VideoIndexerMetadata ConvertToVideoIndexerMetadata(this VideoIndexerResult videoIndexerResult)
		{
            // Get transcript text
            List<string> transcriptTexts = GetTranscriptText(videoIndexerResult);

            // Get transcript times
            List<string> transcriptTimes = GetTranscriptTimes(videoIndexerResult);
            

            return new VideoIndexerMetadata()
            {
                Timestamps = transcriptTimes,
                Transcripts = transcriptTexts
			};
		}

        private static List<string> GetTranscriptText(VideoIndexerResult videoIndexerResult)
        {
            List<string> transcriptTexts = new List<string>();

            if (videoIndexerResult.Videos != null)
            {
                foreach (var video in videoIndexerResult.Videos)
                {
                    if (video.Insights?.Transcripts != null)
                    {
                        foreach (var transcriptItem in video.Insights.Transcripts)
                        {
                            if (transcriptItem.Text != null)
                            {
                                transcriptTexts.Add(transcriptItem.Text);
                            }
                        }
                    }
                }
            }
            return transcriptTexts;
        }

        private static List<string> GetTranscriptTimes(VideoIndexerResult videoIndexerResult)
        {
            List<string> transcriptTimes = new List<string>();

            if (videoIndexerResult.Videos != null)
            {
                foreach (var video in videoIndexerResult.Videos)
                {
                    if (video.Insights?.Transcripts != null)
                    {
                        foreach (var transcriptItem in video.Insights.Transcripts)
                        {
                            if (transcriptItem.Instances != null)
                            {
                                transcriptTimes.Add(transcriptItem.Instances[0].Start);
                                //transcriptTexts.Add(transcriptItem.Text);

                            }
                        }
                    }
                }
            }
            return transcriptTimes;
        }


        // TODO: put all the rest of the data conversion stuff from video indexer service in here instead 
    }
}

