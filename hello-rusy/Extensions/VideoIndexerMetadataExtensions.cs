using System;
using Azure.Core;
using Azure.Storage.Blobs.Models;
using hello_rusy.Data;

namespace hello_rusy.Extensions
{
	public static class VideoIndexerMetadataExtensions
    {
        public static VideoIndexerMetadata ConvertToVideoIndexerMetadata(this VideoIndexerResult videoIndexerResult, string accessToken, string accountId, string location)
		{
            // Get transcript text
            List<string> transcriptTexts = GetTranscriptText(videoIndexerResult);

            // Get transcript times
            List<string> transcriptTimes = GetTranscriptTimes(videoIndexerResult);


            List<List<string>> keyframeShots = GetVideoKeyframesByShot(videoIndexerResult, accessToken, accountId, location);
            ////keyframeUrls = VideoIndexerServiceInstance.GetVideoKeyframes(videoInformation, accessToken, accountId, location);

            return new VideoIndexerMetadata()
            {
                Timestamps = transcriptTimes,
                Transcripts = transcriptTexts,
                KeyframeShots = keyframeShots
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

        // For getting key frames 
        private static string GetKeyFrameUrl(string thumbnailId, string videoId, string accessToken, string accountId, string location)
        {
            string url = $"https://api.videoindexer.ai/{location}/Accounts/{accountId}/Videos/{videoId}/Thumbnails/{thumbnailId}?accessToken={accessToken}";
            return url;
        }

        private static List<List<string>> GetVideoKeyframesByShot(VideoIndexerResult videoIndexerResult, string accessToken, string accountId, string location)
        {
            string thumbnailId;
            string thumbnailUrl;
            string videoId = videoIndexerResult.VideoId!;
            List<List<string>> keyFrameUrls = new List<List<string>>();

            if (videoIndexerResult.Videos != null)
            {
                foreach (var video in videoIndexerResult.Videos)
                {
                    if (video.Insights?.Shots != null)
                    {
                        foreach (var shot in video.Insights.Shots)
                        {
                            List<string> shotList = new List<string>();
                            if ((shot.KeyFrames != null))
                            {
                                foreach (var keyFrame in shot.KeyFrames)
                                {
                                    if (keyFrame.Instances != null)
                                    {
                                        foreach (var currentInstance in keyFrame.Instances)
                                        {
                                            thumbnailId = currentInstance.ThumbnailId.ToString()!;
                                            thumbnailUrl = GetKeyFrameUrl(thumbnailId, videoId, accessToken, accountId, location);
                                            shotList.Add(thumbnailUrl);
                                        }
                                    }
                                }
                            }
                            keyFrameUrls.Add(shotList);
                        }
                    }
                }
            }

            return keyFrameUrls;
        }

        // TODO: put all the rest of the data conversion stuff from video indexer service in here instead 
    }
}

