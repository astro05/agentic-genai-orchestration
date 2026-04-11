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

        /// <summary>Drafts a suggested agent reply using ticket context + knowledge snippets.</summary>
        public async Task<ReplyAssistResponseDto> DraftReplyAssistAsync(
            string ticketTitle,
            string ticketDescription,
            string categoryName,
            IReadOnlyList<(string Id, string Title, string Summary)> knowledgeSnippets)
        {
            var sources = knowledgeSnippets
                .Select(k => new ReplyAssistSourceDto { ArticleId = k.Id, Title = k.Title })
                .ToList();

            if (knowledgeSnippets.Count == 0)
            {
                return new ReplyAssistResponseDto
                {
                    Draft =
                        "Hello,\n\nThank you for contacting support. I've reviewed your request and will help you resolve this shortly. " +
                        "If you have any additional details (screenshots, steps to reproduce, or account email), please reply here.\n\nBest regards,\nSupport",
                    Sources = sources
                };
            }

            var kbBlock = string.Join("\n\n", knowledgeSnippets.Select((k, i) =>
                $"[{i + 1}] {k.Title}\n{k.Summary}"));

            var systemPrompt = """
                You are a professional customer support agent. Draft a concise, empathetic reply to the customer.
                Use the knowledge base excerpts only as factual support; do not invent policy.
                Output ONLY the email body text (no subject line, no JSON). Keep it under 400 words.
                """;

            var userPrompt = $"""
                Ticket category: {categoryName}
                Title: {ticketTitle}
                Description: {ticketDescription}

                Knowledge base:
                {kbBlock}
                """;

            var requestBody = new
            {
                model = _model,
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt }
                },
                max_tokens = 600,
                temperature = 0.35
            };

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_endpoint}/chat/completions");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            request.Content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json");

            try
            {
                var response = await _httpClient.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return FallbackDraft(ticketTitle, knowledgeSnippets, sources);

                using var doc = JsonDocument.Parse(responseBody);
                var content = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString()?
                    .Trim() ?? "";

                if (string.IsNullOrWhiteSpace(content))
                    return FallbackDraft(ticketTitle, knowledgeSnippets, sources);

                return new ReplyAssistResponseDto { Draft = content, Sources = sources };
            }
            catch
            {
                return FallbackDraft(ticketTitle, knowledgeSnippets, sources);
            }
        }

        private static ReplyAssistResponseDto FallbackDraft(
            string ticketTitle,
            IReadOnlyList<(string Id, string Title, string Summary)> knowledgeSnippets,
            List<ReplyAssistSourceDto> sources)
        {
            var first = knowledgeSnippets[0];
            var draft =
                $"Hello,\n\nThank you for reaching out about \"{ticketTitle}\".\n\n" +
                $"Based on our knowledge base article \"{first.Title}\": {first.Summary}\n\n" +
                "If this does not fully address your situation, please reply with any extra details and we'll continue assisting.\n\nBest regards,\nSupport";
            return new ReplyAssistResponseDto { Draft = draft, Sources = sources };
        }
    }
}