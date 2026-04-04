using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using OpenAI;
using System;
using System.ClientModel;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using TextGeneration_GitHub.Models;

namespace TextGeneration_GitHub
{
    public static class OpenAIClientType
    {
        public static async Task UsingOpenAiChat(ApiKeyCredential credential, string model, OpenAIClientOptions openAIOptions)
        {
            // create a chat client
            IChatClient client = new OpenAIClient(credential, openAIOptions).GetChatClient(model).AsIChatClient();

            #region  Run a basic code sample

            string prompt = "What is the capital of France?";
            Console.WriteLine($"user >>> {prompt}");

            ChatResponse response = await client.GetResponseAsync(prompt);

            Console.WriteLine($"assistant >>> {response}");
            Console.WriteLine($"Tokens used: in={response.Usage?.InputTokenCount}, out={response.Usage?.OutputTokenCount}");

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

            //var jobListings = new[]
            //{
            //        "We are hiring a Senior Software Engineer with 5+ years of experience in .NET, Azure, and microservices. Remote role with flexible hours. Salary range $90,000 - $120,000. Contact hr@techwave.com.",

            //        "Looking for a Junior Data Analyst. Must know Excel, SQL, and basic Python. Entry-level position, onsite in Dhaka office. Salary $500 - $800 per month. Apply at careers@datahub.com.",

            //        "Product Manager needed for fast-growing startup. Requires experience in agile teams, product lifecycle management, and UI/UX collaboration. Hybrid role. Salary $100,000 - $130,000 annually.",

            //        "Mid-level Backend Developer required with experience in ASP.NET Core, APIs, and cloud deployment. Remote position. Salary $70,000 - $95,000. Email jobs@backendpro.com."
            //    };

            //foreach (var jobText in jobListings)
            //{
            //    var response = await client.GetResponseAsync<JobDetails>(
            //        $"""
            //            Convert the following job listing into a JSON object matching this C# schema:

            //            Title: job role title
            //            Level: Junior, Mid, Senior
            //            Type: Remote, Onsite, Hybrid
            //            MinSalary: integer only
            //            MaxSalary: integer only
            //            Skills: array of key required skills
            //            TenWordSummary: exactly ten words

            //            Here is the job listing:
            //            {jobText}
            //            """
            //        );

            //    if (response.TryGetResult(out var info))
            //    {
            //        Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(
            //            info,
            //            new System.Text.Json.JsonSerializerOptions { WriteIndented = true }
            //        ));
            //    }
            //    else
            //    {
            //        Console.WriteLine("Response was not in the expected format.");
            //    }
            //}

            #endregion

            #region ChatBot

            //// Initialize chat history with system instructions
            //List<ChatMessage> chatHistory = new()
            //{
            //    new ChatMessage(ChatRole.System, """
            //                    You are a motivating fitness coach who helps people improve their health and fitness.
            //                    You introduce yourself when first saying hello. Your name is Syedur.

            //                    When assisting users, you always ask for:

            //                    1. Their fitness goal (weight loss, muscle gain, endurance)
            //                    2. Their current fitness level (beginner, intermediate, advanced)
            //                    3. How many days per week they can exercise

            //                    After collecting this information, you create a simple weekly workout plan.
            //                    Include exercises, duration, and rest suggestions.

            //                    Also provide one health tip related to their goal.

            //                    End your response by asking if they need a diet plan or progress tracking tips.
            //                    """
            //                    )
            //};

            //while (true)
            //{
            //    // Get user input and add to chat history
            //    Console.WriteLine("Your prompt:");
            //    var userPrompt = Console.ReadLine();
            //    chatHistory.Add(new ChatMessage(ChatRole.User, userPrompt));

            //    // Get streaming response from the model and display in real-time
            //    Console.WriteLine("Bot Response:");
            //    var response = "";
            //    await foreach (var item in
            //        client.GetStreamingResponseAsync(chatHistory))
            //    {
            //        Console.Write(item.Text);
            //        response += item.Text;
            //    }
            //    chatHistory.Add(new ChatMessage(ChatRole.Assistant, response));
            //    Console.WriteLine();
            //}

            #endregion


        }


        /// <summary>
        /// Enables the AI model to invoke predefined functions (tools) during a conversation.
        /// Instead of generating only text responses, the model can decide to call a registered
        /// function with structured arguments, execute external logic, and use the result to
        /// produce more accurate and context-aware responses.
        /// 
        /// This allows integration with real-world systems such as APIs, databases, or custom
        /// business logic, turning the AI from a text generator into an action-capable agent.
        /// </summary>
        public static async Task UsingOpenAiFunctionCall(ApiKeyCredential credential, string model, OpenAIClientOptions openAIOptions)
        {
            //Create a client with function calling capabilities
            IChatClient client = new ChatClientBuilder(new OpenAIClient(credential, openAIOptions).GetChatClient(model).AsIChatClient())
                    .UseFunctionInvocation()
                    .Build();

            //Fitness tool (instead of weather)
            var chatOptions = new ChatOptions
            {
                Tools = [AIFunctionFactory.Create((string goal, string level, int daysPerWeek) =>
                            {
                                // Simulated workout generator (replace with real logic if needed)
                                return $"""
                                        Workout Plan:
                                        Goal: {goal}
                                        Level: {level}
                                        Days per week: {daysPerWeek}

                                        Day 1: Full Body (Squats, Push-ups, Plank)
                                        Day 2: Cardio (Running or Cycling)
                                        Day 3: Upper Body (Pull-ups, Shoulder Press)
                                        Day 4: Rest or Light Stretching
                                        Day 5: Lower Body (Lunges, Deadlifts)
                                        """;
                             },
                            "generate_workout_plan",
                            "Generate a weekly workout plan based on fitness goal, level, and schedule")
                         ]
            };

            //Chat history
            List<ChatMessage> chatHistory = new()
                    {
                        new(ChatRole.System, """
                            You are a friendly and motivating fitness coach.
                            You help users build workout routines based on their goals.

                            Always ask for:
                            1. Fitness goal (weight loss, muscle gain, endurance)
                            2. Fitness level (beginner, intermediate, advanced)
                            3. Days available per week

                            Use the workout generator tool when appropriate.
                            """
                           )
                    };

            //User input
            chatHistory.Add(new(ChatRole.User, """
                                    I want to lose weight. I'm a beginner and can work out 3 days a week.
                                    Can you suggest a workout plan?
                                    """)
                            );

            Console.WriteLine($"{chatHistory.Last().Role} >>> {chatHistory.Last()}");

            //AI response (will call function automatically)
            ChatResponse response = await client.GetResponseAsync(chatHistory, chatOptions);

            // Save response
            chatHistory.Add(new(ChatRole.Assistant, response.Text));

            // Print response
            Console.WriteLine($"{chatHistory.Last().Role} >>> {chatHistory.Last()}");

        }
    } 
    
}
