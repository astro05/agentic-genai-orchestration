using Microsoft.Extensions.AI;
using OpenAI;
using System;
using System.ClientModel;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using TextGeneration.Models;

namespace TextGeneration
{
    public static class OpenAIClientType
    {
        public static async Task UsingOpenAiChat(ApiKeyCredential credential, string model, OpenAIClientOptions openAIOptions)
        {
            // create a chat client
            IChatClient client =
                new OpenAIClient(credential, openAIOptions).GetChatClient(model).AsIChatClient();

            #region  Run a basic code sample

            //string prompt = "What is the capital of France?";
            //Console.WriteLine($"user >>> {prompt}");

            //ChatResponse response = await client.GetResponseAsync(prompt);

            //Console.WriteLine($"assistant >>> {response}");
            //Console.WriteLine($"Tokens used: in={response.Usage?.InputTokenCount}, out={response.Usage?.OutputTokenCount}");

            #endregion

            #region StreamingAsync

            //string prompt = "What is LLM ? in 100 words.";
            //Console.WriteLine($"user >>> {prompt}");

            //var responseStream = client.GetStreamingResponseAsync(prompt);
            //await foreach (var message in responseStream)
            //{
            //    Console.Write(message.Text);
            //}

            #endregion

            #region Text Summarization

            //var summaryPrompt = """
            //Summarize the following text in 1 concise sentence:

            //"Artificial Intelligence is transforming industries by automating tasks, improving decision-making, and enabling new capabilities. However, it also raises concerns about job displacement, data privacy, and ethical use, making responsible development and regulation essential."
            //""";

            //Console.WriteLine($"user >>> {summaryPrompt}");

            //ChatResponse summaryResponse = await client.GetResponseAsync(summaryPrompt);

            //Console.WriteLine($"assistant >>> \n{summaryResponse}");

            #endregion

            #region Structured output

            var jobListings = new[]
            {
                    "We are hiring a Senior Software Engineer with 5+ years of experience in .NET, Azure, and microservices. Remote role with flexible hours. Salary range $90,000 - $120,000. Contact hr@techwave.com.",

                    "Looking for a Junior Data Analyst. Must know Excel, SQL, and basic Python. Entry-level position, onsite in Dhaka office. Salary $500 - $800 per month. Apply at careers@datahub.com.",

                    "Product Manager needed for fast-growing startup. Requires experience in agile teams, product lifecycle management, and UI/UX collaboration. Hybrid role. Salary $100,000 - $130,000 annually.",

                    "Mid-level Backend Developer required with experience in ASP.NET Core, APIs, and cloud deployment. Remote position. Salary $70,000 - $95,000. Email jobs@backendpro.com."
                };

            foreach (var jobText in jobListings)
            {
                var response = await client.GetResponseAsync<JobDetails>(
                    $"""
                        Convert the following job listing into a JSON object matching this C# schema:

                        Title: job role title
                        Level: Junior, Mid, Senior
                        Type: Remote, Onsite, Hybrid
                        MinSalary: integer only
                        MaxSalary: integer only
                        Skills: array of key required skills
                        TenWordSummary: exactly ten words

                        Here is the job listing:
                        {jobText}
                        """
                    );

                if (response.TryGetResult(out var info))
                {
                    Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(
                        info,
                        new System.Text.Json.JsonSerializerOptions { WriteIndented = true }
                    ));
                }
                else
                {
                    Console.WriteLine("Response was not in the expected format.");
                }
            }
        }

            #endregion
    

    }
    
}
