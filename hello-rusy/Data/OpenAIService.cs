using System;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Azure.Core;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace hello_rusy.Data
{
    public class OpenAIService
    {
        public async Task<ToDoList> RequestChatResponse(List<string> transcripts, List<string> timestamps, string apiKey)
        {

            //string message =
            //    "Extract a list of to do items from the following transcript. Respond in a JSON format. " +
            //    "Every line of the transcript includes a timestamp. Make sure the extracted to do items have a corresponding timestamp. " +
            //    "Here is a sample response: " +
            //    " {todos: [{index: 0, timestamp: 0:00:26:93, task: 'This is the given task.'}, {index: 1, timestamp: 0:00:45:23, task: 'This is the second task.'}]}. " +
            //    "Make sure every task in the transcript is displayed in your result.";
            string message = ConstructTranscriptMessage(transcripts, timestamps);
            OpenAiResponse chatResponse = await CallChatCompletion(message, apiKey);
            string responseMessage = extractResponseString(chatResponse);
            Console.WriteLine(responseMessage);
            ToDoList toDoList = JsonSerializer.Deserialize<ToDoList>(responseMessage)!;
            Console.WriteLine(toDoList);
            //var numbers = Enumerable.Range(1, 10) // Generates a sequence of numbers from 1 to 10
            //    .Select(n => $"{n}.") // Transforms each number into a string followed by a period
            //    .ToArray();
            //foreach (var number in numbers)
            //{
            //    responseMessage = responseMessage.Replace(number, "SPLIT " + number);

            //}
            //string[] todos = responseMessage.Split("SPLIT");
            return toDoList;
        }

        private string ConstructTranscriptMessage(List<string> transcripts, List<string> timestamps)
        {
            string message =
            "Extract a list of to do items from the following transcript. Respond in a JSON format. " +
            "Every line of the transcript includes a timestamp. Make sure the extracted to do items have a corresponding timestamp. " +
            "Here is a sample response: " +
            " {todos: [{index: 0, timestamp: 0:00:26:93, task: 'This is the given task.'}, {index: 1, timestamp: 0:00:45:23, task: 'This is the second task.'}]}. " +
            "Make sure every task in the transcript is displayed in your result.";

            if (transcripts.Count != timestamps.Count)
            {
                Console.WriteLine("Lists are not of the same length.");
            }

            // StringBuilder to accumulate the result
            StringBuilder result = new StringBuilder();
            result.AppendLine(message);

            // Loop through the lists
            for (int i = 0; i < transcripts.Count; i++)
            {
                // Append transcript followed by timestamp
                result.AppendLine($"{transcripts[i]} ({timestamps[i]})");
            }
            return result.ToString();
        }

        public string ExtractToDos(ToDoList toDos)
        {


            // List<string> transcriptTexts = new List<string>();

            //if (videoIndexerResult.Videos != null)
            //{
            //    foreach (var video in videoIndexerResult.Videos)
            //    {
            //        if (video.Insights?.Transcripts != null)
            //        {
            //            foreach (var transcriptItem in video.Insights.Transcripts)
            //            {
            //                if (transcriptItem.Text != null)
            //                {
            //                    transcriptTexts.Add(transcriptItem.Text);
            //                }
            //            }
            //        }
            //    }
            //}

            //return transcriptTexts;
            return "hi";
        }

        public async Task<OpenAiResponse> CallChatCompletion(string message, string apiKey)
        {
            HttpClient client = new HttpClient();
            string url = "https://api.openai.com/v1/chat/completions";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);

            OpenAIRequest requestBody = createRequestBody("gpt-3.5-turbo", message);
            string body = JsonSerializer.Serialize(requestBody);
            //string body = @"{
            //    ""model"": ""gpt-3.5-turbo"",
            //    ""messages"": [
            //        {
            //        ""role"": ""system"",
            //        ""content"": ""You are a helpful assistant.""
            //        },
            //        {
            //        ""role"": ""user"",
            //        ""content"": ""What is 2 + 2? ""
            //        }]
            //}";
            request.Content = new StringContent(body,
                                    Encoding.UTF8,
                                    "application/json");//CONTENT-TYPE header
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
            var response = await client.SendAsync(request);

            response.EnsureSuccessStatusCode();

            string? jsonContent = await response.Content.ReadAsStringAsync();
            OpenAiResponse chatResponse = JsonSerializer.Deserialize<OpenAiResponse>(jsonContent)!;

            return chatResponse!;
        }

        public OpenAIRequest createRequestBody(string model, string message)
        {
            OpenAIRequest requestBody = new OpenAIRequest()
            {
                Model = model,
                Messages = new List<ChatMessage>()
                {
                    new ChatMessage()
                    {
                        Role = "user",
                        Content = message
                    }
                }
            };
            return requestBody;
        }

        public string extractResponseString(OpenAiResponse chatResponse)
        {
            if (chatResponse.Choices != null)
            {
                Choice choice = chatResponse.Choices[0];
                if (choice.Message != null)
                {
                    ChatMessage message = choice.Message;
                    return message.Content;
                }
            }
            return "error in parsing response object";
        }
    }

}
