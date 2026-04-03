using DotNetEnv;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

var endpoint = "https://models.github.ai/inference";

////Using .ENV file to load environment variables
//Env.Load();
//ApiKeyCredential credential = new ApiKeyCredential(Environment.GetEnvironmentVariable("GITHUB_TOKEN")?? throw new InvalidOperationException("Missing configuration: GITHUB_TOKEN."));

// get credentials from user secrets
IConfigurationRoot config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
ApiKeyCredential credential = new ApiKeyCredential(config["GITHUB_TOKEN"] ?? throw new InvalidOperationException("Missing configuration: GITHUB_TOKEN."));

var model = "openai/gpt-4o";

var openAIOptions = new OpenAIClientOptions()
{
    Endpoint = new Uri(endpoint)

};

// Create a client
ChatClient client = new ChatClient(model, credential, openAIOptions);

List<ChatMessage> messages = new List<ChatMessage>()
{
    new SystemChatMessage("You are a helpful assistant."),
    new UserChatMessage("What is the capital of France?"),
};

var requestOptions = new ChatCompletionOptions()
{
    Temperature = 1.0f,
    TopP = 1.0f,
    MaxOutputTokenCount = 1000
};

ClientResult<ChatCompletion> response = client.CompleteChat(messages, requestOptions);
System.Console.WriteLine(response.Value.Content[0].Text);