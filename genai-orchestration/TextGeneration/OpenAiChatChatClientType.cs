using OpenAI;
using OpenAI.Chat;
using System;
using System.ClientModel;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using TextGeneration_GitHub.Models;

namespace TextGeneration_GitHub
{
    public static class OpenAiChatChatClientType
    {
        public static async Task UsingOpenAiChat(ApiKeyCredential credential, string model, OpenAIClientOptions openAIOptions)
        {
            // Create a client
            ChatClient client = new ChatClient(model, credential, openAIOptions);

            #region  Run a basic code sample

            //List<ChatMessage> messages = new List<ChatMessage>()
            //{
            //    new SystemChatMessage("You are a helpful assistant."),
            //    new UserChatMessage("What is the capital of France?"),
            //};

            //var requestOptions = new ChatCompletionOptions()
            //{
            //    Temperature = 1.0f,
            //    TopP = 1.0f,
            //    MaxOutputTokenCount = 1000
            //};

            //ClientResult<ChatCompletion> response = client.CompleteChat(messages, requestOptions);
            //System.Console.WriteLine(response.Value.Content[0].Text);

            #endregion

            #region Run a multi-turn conversation

            //List<ChatMessage> messages = new List<ChatMessage>()
            //{
            //    new SystemChatMessage("You are a helpful assistant."),
            //    new UserChatMessage("What is the capital of France?"),
            //};

            //var response = client.CompleteChat(messages);
            //System.Console.WriteLine(response.Value.Content[0].Text);
            //// Append the model response to the chat history.
            //messages.Add(new AssistantChatMessage(response.Value.Content[0].Text));
            //// Append new user question.
            //messages.Add(new UserChatMessage("What about Spain?"));

            //response = client.CompleteChat(messages);
            //System.Console.WriteLine(response.Value.Content[0].Text);

            #endregion

            #region StreamingAsync

            //string prompt = "What is LLM ? in 100 words.";
            //Console.WriteLine($"user >>> {prompt}");

            //AsyncCollectionResult<StreamingChatCompletionUpdate> responseStream = client.CompleteChatStreamingAsync(prompt);
            //await foreach (var update in responseStream)
            //{
            //    if (update.ContentUpdate != null)
            //    {
            //        foreach (var content in update.ContentUpdate)
            //        {
            //            Console.Write(content.Text);
            //        }
            //    }
            //}

            #endregion

            #region Stream the output

            //List<ChatMessage> messages = new List<ChatMessage>()
            //{
            //    new SystemChatMessage("You are a helpful assistant."),
            //    new UserChatMessage("Give me 5 good reasons why I should exercise every day."),
            //};

            //var response = client.CompleteChatStreaming(messages);

            //ChatTokenUsage usage = null;
            //foreach (StreamingChatCompletionUpdate update in response)
            //{
            //    foreach (ChatMessageContentPart updatePart in update.ContentUpdate)
            //    {
            //        System.Console.Write(updatePart.Text);
            //    }
            //    if (update.Usage != null)
            //    {
            //        usage = update.Usage;
            //    }
            //}

            //System.Console.WriteLine("");
            //if (usage != null)
            //{
            //    System.Console.WriteLine($"Output tokens: {usage.OutputTokenCount}");
            //    System.Console.WriteLine($"Input tokens: {usage.InputTokenCount}");
            //    System.Console.WriteLine($"Total tokens: {usage.TotalTokenCount}");
            //}

            #endregion

            #region Text Summarization

            //var summaryPrompt = """
            //Summarize the following text in 1 concise sentence:

            //"Artificial Intelligence is transforming industries by automating tasks, improving decision-making, and enabling new capabilities. However, it also raises concerns about job displacement, data privacy, and ethical use, making responsible development and regulation essential."
            //""";

            //Console.WriteLine($"user >>> {summaryPrompt}");

            //var responseStream = client.CompleteChatStreamingAsync(summaryPrompt);

            //Console.Write("assistant >>> ");

            //await foreach (var update in responseStream)
            //{
            //    if (update.ContentUpdate != null)
            //    {
            //        foreach (var content in update.ContentUpdate)
            //        {
            //            Console.Write(content.Text);
            //        }
            //    }
            //}
            //Console.WriteLine();

            #endregion

            #region Structured output

            var jobListings = new[]
            {
    "We are hiring a Senior Software Engineer with 5+ years of experience in .NET and cloud technologies. Remote role with flexible hours. Salary range $90,000 - $120,000. Contact hr@techwave.com.",

    "Looking for a Junior Data Analyst. Must know Excel, SQL, and basic Python. Entry-level position, office-based in Dhaka. Salary $500 - $800 per month. Apply at careers@datahub.com.",

    "Product Manager needed for fast-growing startup. Requires experience in agile teams and product lifecycle management. Hybrid work model. Salary $100,000 annually."
};

            foreach (var jobText in jobListings)
            {
                var messages = new List<ChatMessage>
    {
        new SystemChatMessage("""
You are a strict JSON generator.
Return ONLY valid JSON. No explanation.
"""),

        new UserChatMessage($"""
Convert this job listing into JSON with this schema:

Title: job role
Level: Junior, Mid, Senior
Type: Remote, Onsite, Hybrid
MinSalary: integer
MaxSalary: integer
Skills: array of key skills
TenWordSummary: exactly 10 words

Listing:
{jobText}
""")
    };

                var response = await client.CompleteChatAsync(messages);

                var json = response.Value.Content[0].Text;

                try
                {
                    var cleaned = json
                        .Replace("```json", "")
                        .Replace("```", "")
                        .Trim();

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        Converters = { new JsonStringEnumConverter() }
                    };

                    var info = JsonSerializer.Deserialize<JobDetails>(cleaned, options);

                    Console.WriteLine(JsonSerializer.Serialize(info, new JsonSerializerOptions
                    {
                        WriteIndented = true
                    }));
                }
                catch
                {
                    Console.WriteLine("Failed to parse JSON:");
                    Console.WriteLine(json);
                }
            }

            #endregion
        }

    }
}
