using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TicketSystem.API.DTOs;
using TicketSystem.API.Models;

namespace TicketSystem.API.Services
{
    public class AIService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _endpoint;
        private readonly string _model;

        public AIService(IConfiguration config, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _apiKey = config["GitHubModels:ApiKey"] ?? throw new Exception("GitHub Models API key missing");
            _endpoint = config["GitHubModels:Endpoint"] ?? "https://models.inference.ai.azure.com";
            _model = config["GitHubModels:Model"] ?? "gpt-4o-mini";
        }

        public async Task<AIClassifyResponse> ClassifyTicketAsync(string description)
        {
            var categories = string.Join(", ", Enum.GetNames<TicketCategory>());
            var priorities = string.Join(", ", Enum.GetNames<TicketPriority>());

            var systemPrompt = $$"""
                You are a customer support ticket classifier. Analyze the ticket description and respond ONLY with a valid JSON object.
                Choose Category from: {{categories}}
                Choose Priority from: {{priorities}}
                Respond exactly in this format: {"category":"ChosenCategory","priority":"ChosenPriority"}
                """;

            var requestBody = new
            {
                model = _model,
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = description }
                },
                max_tokens = 100,
                temperature = 0.1
            };

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_endpoint}/chat/completions");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            request.Content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            try
            {
                var response = await _httpClient.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return new AIClassifyResponse { Category = "UncategorizedIssue", Priority = "Medium" };

                using var doc = JsonDocument.Parse(responseBody);
                var content = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString() ?? "{}";

                using var resultDoc = JsonDocument.Parse(content.Trim());
                var category = resultDoc.RootElement.GetProperty("category").GetString() ?? "UncategorizedIssue";
                var priority = resultDoc.RootElement.GetProperty("priority").GetString() ?? "Medium";

                // Validate enums
                if (!Enum.TryParse<TicketCategory>(category, true, out _)) category = "UncategorizedIssue";
                if (!Enum.TryParse<TicketPriority>(priority, true, out _)) priority = "Medium";

                return new AIClassifyResponse { Category = category, Priority = priority };
            }
            catch
            {
                return new AIClassifyResponse { Category = "UncategorizedIssue", Priority = "Medium" };
            }
        }
    }
}