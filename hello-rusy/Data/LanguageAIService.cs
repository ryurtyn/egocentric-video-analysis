﻿using System;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Azure;
using Azure.Core;

namespace hello_rusy.Data
{
    /// <summary>
    /// Class interacts with language service API to generate summarized title 
    /// </summary>
	public class LanguageAIService
    {
        /// <summary>
        /// Convert list of transcripts to input format for OpenAI API call 
        /// </summary>
        /// <param name="transcripts"> list of transcript strings </param>
        /// <returns> summarize text input object </returns>
        public SummarizeTextInput MapVideoTranscriptToSummarizerInput(List<string> transcripts)
        {
            SummarizeTextInput summarizerInput = new SummarizeTextInput()
            {
                DisplayName = "my task",
                AnalysisInput = new AnalysisInput()
                {
                    Conversations = new ConversationInput[]
                    {
                        new ConversationInput()
                        {
                            ConversationItems = transcripts.Select(
                            (item, index)  => new ConversationItem()
                            {
                                Text = item,
                                Id = (index + 1).ToString(),
                                Role = "Agent",
                                ParticipantId = "1" // note: if your transcript has different speakers you will need to update this later to include that 
                            }
                            ).ToList(),
                            Modality = "text",
                            Id = "conversation1",
                            Language = "en"
                        }
                    }
                },
                Tasks = new LanguageTask[1]
                {
                    new LanguageTask()
                    {
                        TaskName = "Conversation Task",
                        Kind = "ConversationalSummarizationTask",
                        Parameters = new Parameters()
                        {
                            SummaryAspects = new string[]{"chapterTitle"}
                        }
                    }
                }
            };
            return summarizerInput;
        }

        /// <summary>
        /// waits until text summary has returned a result and returns the result 
        /// </summary>
        /// <param name="config"> configuration object </param>
        /// <param name="transcripts"> list of transcript strings</param>
        /// <returns> text summarizer result object </returns>
        /// <exception cref="Exception"></exception>
        public async Task<TextSummarizerResult> getTextSummary(EgocentricVideoConfig config, List<string> transcripts)
        {
            SummarizeTextInput inputBody = MapVideoTranscriptToSummarizerInput(transcripts);
            string operationLocation = await getOperationLocation(config.languageServiceApiKey, inputBody);

            bool finished = false;
            int maxRetries = 20; // For example, to avoid infinite loop
            int attempts = 0;
            TextSummarizerResult summaryResult = null;

            while (!finished && attempts < maxRetries)
            {
                // Assuming getSummaryResponse checks the status and returns null if not completed
                summaryResult = await getSummaryResponse(config.languageServiceApiKey, operationLocation);
                if (summaryResult.Status.Equals("succeeded") || summaryResult.Status.Equals("failed")|| summaryResult.Status.Equals("canceled"))
                {
                    finished = true;
                } else
                {
                    // Use Task.Delay in an async method instead of Thread.Sleep
                    await Task.Delay(500); // Wait for half a second before trying again
                    attempts++;
                }
            }

            if (!finished)
            {
                throw new Exception("Operation did not complete within the maximum number of attempts.");
            }
            return summaryResult;


        }

        /// <summary>
        /// calls Language service API to get a summary response 
        /// </summary>
        /// <param name="subscriptionKey"> language service subscription key </param>
        /// <param name="operationLocation"> language service operation location </param>
        /// <returns> text summarizer result </returns>
        private async Task<TextSummarizerResult> getSummaryResponse(string subscriptionKey, string operationLocation)
        {
            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, operationLocation);
            request.Content = new StringContent("application/json");//CONTENT-TYPE header
            request.Headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            string jsonContent = await response.Content.ReadAsStringAsync();
            TextSummarizerResult summaryResponse = JsonSerializer.Deserialize<TextSummarizerResult>(jsonContent)!;
            return summaryResponse;
        }

        /// <summary>
        /// calls language service API to retrieve operation location
        /// </summary>
        /// <param name="subscriptionKey"> language service subscription key </param>
        /// <param name="requestObject"> api request object </param>
        /// <returns> operation location string </returns>
        private async Task<string> getOperationLocation(string subscriptionKey, SummarizeTextInput requestObject)
        {
            HttpClient client = new HttpClient();
            string requestBody = JsonSerializer.Serialize(requestObject); 
            
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://egocentrictextsummarizer.cognitiveservices.azure.com/language/analyze-conversations/jobs?api-version=2023-11-15-preview");
            request.Content = new StringContent(requestBody,
                                    Encoding.UTF8,
                                    "application/json");//CONTENT-TYPE header
            request.Headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
            var response = await client.SendAsync(request);

            response.EnsureSuccessStatusCode();

            string? operationLocation = response.Headers.GetValues("operation-location").FirstOrDefault();
            
            return operationLocation!;

        }

        /// <summary>
        /// extracts summarized title from text summarizer result 
        /// </summary>
        /// <param name="response"> text summarizer result from language service api </param>
        /// <returns> summarized title string </returns>
        public string GetSummarizedTitle(TextSummarizerResult response)
        {
            List<string> summaryTexts = new List<string>();
            string status = response.Status;
            if (status.Equals("succeeded")) {
                if ((response.Tasks != null) && (response.Tasks.Items != null))
                {
                   foreach (var item in response.Tasks.Items)
                    {
                        if ((item.Results != null) && (item.Results.Conversations != null))
                        {
                            foreach (var conversation in item.Results.Conversations)
                            {
                                if (conversation.Summaries != null)
                                {
                                    foreach (var summary in conversation.Summaries)
                                    {
                                        string currText = summary.Text;
                                        summaryTexts.Add(currText);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (summaryTexts != null)
            {
                string summary = String.Join(" ", summaryTexts);
                return summary;
            } else
            {
                return "no summaries found";
            }
        }
    }
}

