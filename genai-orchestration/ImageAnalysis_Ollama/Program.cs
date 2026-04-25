using ImageAnalysis_Ollama;
using Microsoft.Extensions.AI;
using OllamaSharp;
using OllamaSharp.Models.Chat;

//// create a chat client
//IChatClient client =
//    new OllamaApiClient(new Uri("http://localhost:11434"), "llava");

// Create the HttpClient with an extended or infinite timeout
var httpClient = new HttpClient
{
    BaseAddress = new Uri("http://localhost:11434"),
    Timeout = Timeout.InfiniteTimeSpan // Recommended for LLM/Image tasks
};

// Initialize the client using the custom HttpClient
IChatClient client = new OllamaApiClient(httpClient, "llava");


//// analyze the imagese
//await ImageAnalysis.AnalyzeImage(client);

// analyze the traffic cam images
await TrafficAnalysis.AnalyzeTrafficAsync(client);

Console.ReadLine();