using System;
using Azure.Core;
using Azure.Storage.Blobs.Models;
using hello_rusy.Data;

namespace hello_rusy.Extensions
{
    /// <summary>
    /// Class to interact with video indexer result class. Extracts needed information and converts to video indexer metadata format 
    /// </summary>
	public static class VideoIndexerMetadataExtensions
    {
        /// <summary>
        /// Converter from Video Indexer Result class to Video Indexer Metadata class 
        /// </summary>
        /// <param name="videoIndexerResult"> Video Indexer Result object </param>
        /// <param name="config"> configuration object </param>
        /// <returns> Video Indexer Metadata object </returns>
        public static VideoIndexerMetadata ConvertToVideoIndexerMetadata(this VideoIndexerResult videoIndexerResult, EgocentricVideoConfig config)
		{
            List<string> transcriptTexts = GetTranscriptText(videoIndexerResult);
            List<string> transcriptTimes = GetTranscriptTimes(videoIndexerResult);
            List<string> keywords = GetKeyWords(videoIndexerResult);
            return new VideoIndexerMetadata()
            {
                Timestamps = transcriptTimes,
                Transcripts = transcriptTexts,
                Keywords = keywords
            };
		}

        /// <summary>
        /// Retrives list of transcripts from video indexer result object 
        /// </summary>
        /// <param name="videoIndexerResult"> video indexer result object </param>
        /// <returns> list of transcript strings </returns>
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

        /// <summary>
        /// Retrives list of timestamps corresponding to each transcript object from video indexer result object 
        /// </summary>
        /// <param name="videoIndexerResult"> video indexer result object </param>
        /// <returns> list of timestamps corresponding to each transcript item </returns>
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

        /// <summary>
        /// Constructs url for key frame image 
        /// Not secure and should not be used in final product 
        /// </summary>
        /// <param name="thumbnailId"></param>
        /// <param name="videoId"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        private static string GetKeyFrameUrl(string thumbnailId, string videoId, EgocentricVideoConfig config )
        {
            string url = $"https://api.videoindexer.ai/{config.videoIndexerLocation}/Accounts/{config.videoIndexerAccountId}/Videos/{videoId}/Thumbnails/{thumbnailId}?accessToken={config.videoIndexerApiKey}";
            return url;
        }

        /// <summary>
        /// extracts key frame url from video indexer result object 
        /// </summary>
        /// <param name="videoIndexerResult"> video indexer result object </param>
        /// <param name="config"> configuration object </param>
        /// <returns> list of video key frame urls split by shot </returns>
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

        /// <summary>
        /// Gets labels from video indexer result object 
        /// </summary>
        /// <param name="videoIndexerResult"> video indexer result object </param>
        /// <returns> list of labels </returns>
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

        /// <summary>
        /// Gets topics from video indexer result object 
        /// </summary>
        /// <param name="videoIndexerResult"> video indexer result object </param>
        /// <returns> list of topics </returns>
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

        /// <summary>
        /// Gets key words from video indexer result object 
        /// </summary>
        /// <param name="videoIndexerResult"> video indexer result object </param>
        /// <returns> list of key words </returns>
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

        /// <summary>
        ///  Gets OCR information from video indexer result object 
        /// </summary>
        /// <param name="videoIndexerResult"> video indexer result object </param>
        /// <returns> list of OCR strings </returns>
        public static List<string> GetOcr(VideoIndexerResult videoIndexerResult)
        {
            List<string> ocrs = new List<string>();
            if (videoIndexerResult.Videos != null)
            {
                foreach (var video in videoIndexerResult.Videos)
                {
                    if (video.Insights?.Ocr != null)
                    {
                        foreach (var ocr in video.Insights.Ocr)
                        {
                            if (ocr.Text != null)
                            {
                                ocrs.Add(ocr.Text);
                            }
                        }
                    }
                }
            }
            return ocrs;
        }

        /// <summary>
        /// Gets detected objects from video indexer result object 
        /// </summary>
        /// <param name="videoIndexerResult"> video indexer result object </param>
        /// <returns> list of detected objects </returns>
        public static List<string> GetDetectedObjects(VideoIndexerResult videoIndexerResult)
        {
            List<string> detectedObjects = new List<string>();
            if (videoIndexerResult.Videos != null)
            {
                foreach (var video in videoIndexerResult.Videos)
                {
                    if (video.Insights?.DetectedObjects != null)
                    {
                        foreach (var detectedObject in video.Insights.DetectedObjects)
                        {
                            if (detectedObject.DisplayName != null)
                            {
                                detectedObjects.Add(detectedObject.DisplayName);
                            }
                        }
                    }
                }
            }
            return detectedObjects;
        }
    }
}

