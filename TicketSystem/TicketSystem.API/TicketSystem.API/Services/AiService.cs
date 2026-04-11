using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace TicketSystem.API.Services;

public class AiService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;

    public AiService(IConfiguration config)
    {
        _config = config;
        _http = new HttpClient();
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", config["GitHubModels:ApiKey"]);
    }

    public async Task<(string Category, string Priority)> ClassifyTicketAsync(string description)
    {
        var endpoint = _config["GitHubModels:Endpoint"]!;
        var model = _config["GitHubModels:Model"]!;

        var prompt = $"""
            You are a support ticket classifier. Given the ticket description below, 
            respond ONLY with a JSON object with two fields:
            - "category": one of [Billing, Technical, General, Account, Other]
            - "priority": one of [Low, Medium, High]

            Ticket: {description}

            JSON:
            """;

        var body = new
        {
            model,
            messages = new[]
            {
                new { role = "user", content = prompt }
            },
            max_tokens = 100
        };

        var request = new HttpRequestMessage(HttpMethod.Post, $"{endpoint}/chat/completions")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json")
        };

        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var content = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? "{}";

        // Clean possible markdown fences
        content = content.Replace("```json", "").Replace("```", "").Trim();

        using var result = JsonDocument.Parse(content);
        var category = result.RootElement.GetProperty("category").GetString() ?? "General";
        var priority = result.RootElement.GetProperty("priority").GetString() ?? "Low";

        return (category, priority);
    }
}