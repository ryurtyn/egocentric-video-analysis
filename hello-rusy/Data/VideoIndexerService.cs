using System;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Core;
using Azure.Storage.Blobs.Models;

namespace hello_rusy.Data
{
	public class VideoIndexerService
	{
        public async Task<VideoIndexerResult> GetVideoInsights(string targetName, EgocentricVideoConfig config)
        {
            VideoIndexerList listOfResults = await GetAllVideos(config);


            VideoIndexerResult? foundResult = null;
            VideoIndexerResult? resultIndex = null;

            foreach (var result in listOfResults.Results)
            {
                if (result.Name == targetName)
                {
                    foundResult = result;
                    break;
                }
            }
            if (foundResult != null)
            {
                resultIndex = await RequestResult(foundResult.VideoId!, config);
            }
            return resultIndex;
        }

        public async Task<VideoIndexerList> GetAllVideos(EgocentricVideoConfig config)
        {
            HttpClient client = new HttpClient();
            string url = $"https://api.videoindexer.ai/{config.videoIndexerLocation}/Accounts/{config.videoIndexerAccountId}/Videos?accessToken={config.videoIndexerApiKey}";
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", config.videoIndexerSubscriptionKey);
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", config.videoIndexerApiKey);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            string jsonContent = await response.Content.ReadAsStringAsync();
            VideoIndexerList listResult = JsonSerializer.Deserialize<VideoIndexerList>(jsonContent)!;
            return listResult;
        }

        private async Task<VideoIndexerResult> RequestResult(string videoId, EgocentricVideoConfig config)
        {
            HttpClient client = new HttpClient();
            string url = $"https://api.videoindexer.ai/{config.videoIndexerLocation}/Accounts/{config.videoIndexerAccountId}/Videos/{videoId}/Index?accessToken={config.videoIndexerApiKey}";
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", config.videoIndexerSubscriptionKey);
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", config.videoIndexerApiKey);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            string jsonContent = await response.Content.ReadAsStringAsync();

            VideoIndexerResult indexResult = JsonSerializer.Deserialize<VideoIndexerResult>(jsonContent)!;
            return indexResult;
        }


        //// Methods to manipulate video indexer result 

        //public List<string> ExtractTranscriptTexts(VideoIndexerResult videoIndexerResult)
        //{
        //    List<string> transcriptTexts = new List<string>();

        //    if (videoIndexerResult.Videos != null)
        //    {
        //        foreach (var video in videoIndexerResult.Videos)
        //        {
        //            if (video.Insights?.Transcripts != null)
        //            {
        //                foreach (var transcriptItem in video.Insights.Transcripts)
        //                {
        //                    if (transcriptItem.Text != null)
        //                    {
        //                        transcriptTexts.Add(transcriptItem.Text);
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    return transcriptTexts;
        //}

        //public List<string> ExtractTranscriptTimestamps(VideoIndexerResult videoIndexerResult)
        //{
        //    List<string> transcriptTimes = new List<string>();

        //    if (videoIndexerResult.Videos != null)
        //    {
        //        foreach (var video in videoIndexerResult.Videos)
        //        {
        //            if (video.Insights?.Transcripts != null)
        //            {
        //                foreach (var transcriptItem in video.Insights.Transcripts)
        //                {
        //                    if (transcriptItem.Instances != null)
        //                    {
        //                        transcriptTimes.Add(transcriptItem.Instances[0].Start);
        //                        //transcriptTexts.Add(transcriptItem.Text);
                                
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    return transcriptTimes;
        //}

        //public string GetKeyFrameUrl(string thumbnailId, string videoId, string accessToken, string accountId, string location)
        //{
        //    string url = $"https://api.videoindexer.ai/{location}/Accounts/{accountId}/Videos/{videoId}/Thumbnails/{thumbnailId}?accessToken={accessToken}";
        //    return url;
        //}

        //public List<string> GetVideoKeyframes(VideoIndexerResult videoIndexerResult, string accessToken, string accountId, string location)
        //{
        //    string thumbnailId;
        //    string thumbnailUrl;
        //    string videoId = videoIndexerResult.VideoId!;
        //    List<string> keyFrameUrls = new List<string>();

        //    if (videoIndexerResult.Videos != null)
        //    {
        //        foreach (var video in videoIndexerResult.Videos)
        //        {
        //            if (video.Insights?.Shots != null)
        //            {
        //                foreach (var shot in video.Insights.Shots)
        //                {
        //                    if ((shot.KeyFrames != null))
        //                    {
        //                        foreach (var keyFrame in shot.KeyFrames)
        //                        {
        //                            if (keyFrame.Instances != null)
        //                            {
        //                                foreach (var currentInstance in keyFrame.Instances)
        //                                {
        //                                    thumbnailId = currentInstance.ThumbnailId.ToString()!;
        //                                    thumbnailUrl = GetKeyFrameUrl(thumbnailId, videoId, accessToken, accountId, location);
        //                                    keyFrameUrls.Add(thumbnailUrl);
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    return keyFrameUrls;
        //}

        //public List<List<string>> GetVideoKeyframesByShot(VideoIndexerResult videoIndexerResult, string accessToken, string accountId, string location)
        //{
        //    string thumbnailId;
        //    string thumbnailUrl;
        //    string videoId = videoIndexerResult.VideoId!;
        //    List<List<string>> keyFrameUrls = new List<List<string>>();

        //    if (videoIndexerResult.Videos != null)
        //    {
        //        foreach (var video in videoIndexerResult.Videos)
        //        {
        //            if (video.Insights?.Shots != null)
        //            {
        //                foreach (var shot in video.Insights.Shots)
        //                {
        //                    List<string> shotList = new List<string>();
        //                    if ((shot.KeyFrames != null))
        //                    {
        //                        foreach (var keyFrame in shot.KeyFrames)
        //                        {
        //                            if (keyFrame.Instances != null)
        //                            {
        //                                foreach (var currentInstance in keyFrame.Instances)
        //                                {
        //                                    thumbnailId = currentInstance.ThumbnailId.ToString()!;
        //                                    thumbnailUrl = GetKeyFrameUrl(thumbnailId, videoId, accessToken, accountId, location);
        //                                    shotList.Add(thumbnailUrl);
        //                                }
        //                            }
        //                        }
        //                    }
        //                    keyFrameUrls.Add(shotList);
        //                }
        //            }
        //        }
        //    }

        //    return keyFrameUrls;
        //}

        public List<string> GetOcr(VideoIndexerResult videoIndexerResult)
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


        //public List<string> GetKeyWords(VideoIndexerResult videoIndexerResult)
        //{
        //    List<string> keywords = new List<string>();
        //    if (videoIndexerResult.Videos != null)
        //    {
        //        foreach (var video in videoIndexerResult.Videos)
        //        {
        //            if (video.Insights?.Keywords != null)
        //            {
        //                foreach (var keyword in video.Insights.Keywords)
        //                {
        //                    if (keyword.Text != null)
        //                    {
        //                        keywords.Add(keyword.Text);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    return keywords;
        //}


        //public List<string> GetTopics(VideoIndexerResult videoIndexerResult)
        //{
        //    List<string> topics = new List<string>();
        //    if (videoIndexerResult.Videos != null)
        //    {
        //        foreach (var video in videoIndexerResult.Videos)
        //        {
        //            if (video.Insights?.Topics != null)
        //            {
        //                foreach (var topic in video.Insights.Topics)
        //                {
        //                    if ((topic.Name != null))
        //                    {
        //                        topics.Add(topic.Name);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    return topics;
        //}

        public List<string> GetFaces(VideoIndexerResult videoIndexerResult)
        {
            return new List<string>();
        }

        //public List<string> GetLabels(VideoIndexerResult videoIndexerResult)
        //{
        //    List<string> labels = new List<string>();
        //    if (videoIndexerResult.Videos != null)
        //    {
        //        foreach (var video in videoIndexerResult.Videos)
        //        {
        //            if (video.Insights?.Labels != null)
        //            {
        //                foreach (var label in video.Insights.Labels)
        //                {
        //                    if (label.Name != null)
        //                    {
        //                        labels.Add(label.Name);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    return labels;
        //}

        public List<string> GetScenes(VideoIndexerResult videoIndexerResult)
        {
            return new List<string>();
        }

        public List<string> GetShots(VideoIndexerResult videoIndexerResult)
        {
            return new List<string>();
        }

        public List<string> GetDetectedObjects(VideoIndexerResult videoIndexerResult)
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

        public List<string> GetAudioEffects(VideoIndexerResult videoIndexerResult)
        {
            return new List<string>();
        }

        public List<string> GetSentiments(VideoIndexerResult videoIndexerResult)
        {
            List<string> sentiments = new List<string>();
            if (videoIndexerResult.Videos != null)
            {
                foreach (var video in videoIndexerResult.Videos)
                {
                    if (video.Insights?.Sentiments != null)
                    {
                        foreach (var sentiment in video.Insights.Sentiments)
                        {
                            if (sentiment.SentimentType != null)
                            {
                                sentiments.Add(sentiment.SentimentType);
                            }
                        }
                    }
                }
            }
            return sentiments;
        }

        public List<string> GetSpeakers(VideoIndexerResult videoIndexerResult)
        {
            return new List<string>();
        }

        public List<string> GetStatistics(VideoIndexerResult videoIndexerResult)
        {
            return new List<string>();
        }
    }
}

