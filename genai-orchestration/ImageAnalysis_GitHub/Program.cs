using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using System.ClientModel;

// get credentials from user secrets
IConfigurationRoot config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

var gitHubToken = "GITHUB_TOKEN_openai_gpt_4o_mini";
var gitHubModel = "openai/gpt-4o-mini";
var credential = new ApiKeyCredential(config[gitHubToken] ?? throw new InvalidOperationException($"Missing configuration: {gitHubToken}."));
var options = new OpenAIClientOptions()
{
    Endpoint = new Uri("https://models.github.ai/inference")
};

// create a chat client
IChatClient client =
    new OpenAIClient(credential, options).GetChatClient(gitHubModel).AsIChatClient();


// user prompts
var promptDescribe = "Describe the image";
var promptAnalyze = "How many red cars are in the picture? and what other car colors are there?";

// prompts
string systemPrompt = "You are a useful assistant that describes images using a direct style.";
var userPrompt = promptAnalyze;

List<ChatMessage> messages =
[
    new ChatMessage(ChatRole.System, systemPrompt),
    new ChatMessage(ChatRole.User, userPrompt),
];

// read the image bytes, create a new image content part and add it to the messages
// Define your file path
var imageFileName = "traffic.jpg"; // Or "traffic.jpg"
string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "images", imageFileName);

// Detect the MIME type dynamically
string mimeType = GetMimeType(imageFileName);

// Create the DataContent using the detected type
AIContent aic = new DataContent(File.ReadAllBytes(imagePath), mimeType);

var message = new ChatMessage(ChatRole.User, [aic]);
messages.Add(message);


// send the messages to the assistant
var response = await client.GetResponseAsync(messages);
Console.WriteLine($"Prompt: {userPrompt}");
Console.WriteLine($"Image: {imageFileName}");
Console.WriteLine($"Response: {response.Messages[0]}");

static string GetMimeType(string fileName)
{
    string extension = Path.GetExtension(fileName).ToLowerInvariant();
    return extension switch
    {
        ".png" => "image/png",
        ".jpg" or ".jpeg" => "image/jpeg",
        ".gif" => "image/gif",
        ".webp" => "image/webp",
        _ => "application/octet-stream" // Fallback for unknown types
    };
}