using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace SCAuditStudio
{
    static class AISort
    {
        const string key = "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";
        const string url = "https://api.openai.com/v1/chat/completions";
        public static async Task<string> AskGPT(string input)
        {
            // Initialise the chat by describing the assistant
            var messages = new List<dynamic>
            {
            new {role = "system", content = "You are ChatGPT, a large language " + "model trained by OpenAI. " + "Answer only shortly"}
            };

            var userMessageS = input;
            messages.Add(new { role = "user", content = userMessageS });
            var request = new
            {
                messages,
                model = "gpt-3.5-turbo",
                max_tokens = 300,
                temperature = 0.2,
            };

            // Send the request and capture the response
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {key}");
            var requestJson = JsonConvert.SerializeObject(request);
            var requestContent = new StringContent(requestJson, System.Text.Encoding.UTF8, "application/json");
            var httpResponseMessage = await httpClient.PostAsync(url, requestContent);
            var jsonString = await httpResponseMessage.Content.ReadAsStringAsync();
            var responseObject = JsonConvert.DeserializeAnonymousType(jsonString, new
            {
                choices = new[] { new { message = new { role = string.Empty, content = string.Empty } } },
                error = new { message = string.Empty }
            });
            if (!string.IsNullOrEmpty(responseObject?.error?.message))  // Check for errors
            {
                return responseObject?.error.message;
            }
            else  // Add the message object to the message collection
            {
                var messageObject = responseObject?.choices[0].message;
                return messageObject.content;
            }

        }
        
    }
}
