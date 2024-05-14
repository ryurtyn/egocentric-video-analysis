using System;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Azure.Core;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace hello_rusy.Data
{
    /// <summary>
    /// Service interacts with OpenAI API to generate tasks 
    /// </summary>
    public class OpenAIService
    {
        /// <summary>
        /// given transcripts and timestamps, outputs a list of tasks with associated timestamps 
        /// </summary>
        /// <param name="transcripts"> list of transcript items </param>
        /// <param name="timestamps"> list of timestamps associated with each transcript item</param>
        /// <param name="config"> configuration object </param>
        /// <returns> to do list object </returns>
        public async Task<ToDoList> RequestChatResponse(List<string> transcripts, List<string> timestamps, EgocentricVideoConfig config)
        {
            string message = ConstructTranscriptMessage(transcripts, timestamps);
            OpenAiResponse chatResponse = await CallChatCompletion(message, config.openAIApiKey);
            string responseMessage = extractResponseString(chatResponse);
            Console.WriteLine(responseMessage);
            ToDoList toDoList = JsonSerializer.Deserialize<ToDoList>(responseMessage)!;
            Console.WriteLine(toDoList);
            return toDoList;
        }

        /// <summary>
        /// constructs input message to be used in OpenAI API call 
        /// </summary>
        /// <param name="transcripts"> list of transcript items </param>
        /// <param name="timestamps"> list of timestamps associated with each transcript item </param>
        /// <returns> message string </returns>
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
                result.AppendLine($"{transcripts[i]} ({timestamps[i]})");
            }
            return result.ToString();
        }

        /// <summary>
        /// Calls OpenAI API chat completion
        /// </summary>
        /// <param name="message"></param>
        /// <param name="apiKey"></param>
        /// <returns></returns>
        public async Task<OpenAiResponse> CallChatCompletion(string message, string apiKey)
        {
            HttpClient client = new HttpClient();
            string url = "https://api.openai.com/v1/chat/completions";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
            OpenAIRequest requestBody = createRequestBody("gpt-3.5-turbo", message);
            string body = JsonSerializer.Serialize(requestBody);
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

        /// <summary>
        /// Create request body to give to openAI input 
        /// </summary>
        /// <param name="model"> model name </param>
        /// <param name="message"> input message </param>
        /// <returns> openAI Request object </returns>
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

        /// <summary>
        /// Extracts response from openAI Response object 
        /// </summary>
        /// <param name="chatResponse"> OpenAIResponse object </param>
        /// <returns> response message string </returns>
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
