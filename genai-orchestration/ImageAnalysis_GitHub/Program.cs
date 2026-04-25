using ImageAnalysis_GitHub;
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


// analyze the imagese
await ImageAnalysis.AnalyzeImage(client);



