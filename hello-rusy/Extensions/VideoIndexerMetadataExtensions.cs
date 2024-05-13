using System;
using Azure.Core;
using Azure.Storage.Blobs.Models;
using hello_rusy.Data;

namespace hello_rusy.Extensions
{
	public static class VideoIndexerMetadataExtensions
    {
        public static VideoIndexerMetadata ConvertToVideoIndexerMetadata(this VideoIndexerResult videoIndexerResult, EgocentricVideoConfig config)
		{
            // Get transcript text
            List<string> transcriptTexts = GetTranscriptText(videoIndexerResult);

            // Get transcript times
            List<string> transcriptTimes = GetTranscriptTimes(videoIndexerResult);


            List<List<string>> keyframeShots = GetVideoKeyframesByShot(videoIndexerResult, config);
            ////keyframeUrls = VideoIndexerServiceInstance.GetVideoKeyframes(videoInformation, accessToken, accountId, location);

            List<string> labels = GetLabels(videoIndexerResult);

            List<string> topics = GetTopics(videoIndexerResult);

            List<string> keywords = GetKeyWords(videoIndexerResult);

            return new VideoIndexerMetadata()
            {
                Timestamps = transcriptTimes,
                Transcripts = transcriptTexts,
                KeyframeShots = keyframeShots,
                Labels = labels,
                Topics = topics,
                Keywords = keywords
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
        private static string GetKeyFrameUrl(string thumbnailId, string videoId, EgocentricVideoConfig config )
        {
            string url = $"https://api.videoindexer.ai/{config.videoIndexerLocation}/Accounts/{config.videoIndexerAccountId}/Videos/{videoId}/Thumbnails/{thumbnailId}?accessToken={config.videoIndexerApiKey}";
            return url;
        }

        private static List<List<string>> GetVideoKeyframesByShot(VideoIndexerResult videoIndexerResult, EgocentricVideoConfig config)
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
                                            thumbnailUrl = GetKeyFrameUrl(thumbnailId, videoId, config);
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

        public static List<string> GetLabels(VideoIndexerResult videoIndexerResult)
        {
            List<string> labels = new List<string>();
            if (videoIndexerResult.Videos != null)
            {
                foreach (var video in videoIndexerResult.Videos)
                {
                    if (video.Insights?.Labels != null)
                    {
                        foreach (var label in video.Insights.Labels)
                        {
                            if (label.Name != null)
                            {
                                labels.Add(label.Name);
                            }
                        }
                    }
                }
            }
            return labels;
        }

        public static List<string> GetTopics(VideoIndexerResult videoIndexerResult)
        {
            List<string> topics = new List<string>();
            if (videoIndexerResult.Videos != null)
            {
                foreach (var video in videoIndexerResult.Videos)
                {
                    if (video.Insights?.Topics != null)
                    {
                        foreach (var topic in video.Insights.Topics)
                        {
                            if ((topic.Name != null))
                            {
                                topics.Add(topic.Name);
                            }
                        }
                    }
                }
            }
            return topics;
        }

        public static List<string> GetKeyWords(VideoIndexerResult videoIndexerResult)
        {
            List<string> keywords = new List<string>();
            if (videoIndexerResult.Videos != null)
            {
                foreach (var video in videoIndexerResult.Videos)
                {
                    if (video.Insights?.Keywords != null)
                    {
                        foreach (var keyword in video.Insights.Keywords)
                        {
                            if (keyword.Text != null)
                            {
                                keywords.Add(keyword.Text);
                            }
                        }
                    }
                }
            }
            return keywords;
        }
        // TODO: put all the rest of the data conversion stuff from video indexer service in here instead 
    }
}

