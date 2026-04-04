
using DotNetEnv;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OpenAI;

using System.ClientModel;
using TextGeneration_GitHub;


var endpoint = "https://models.github.ai/inference";

////Using .ENV file to load environment variables
//Env.Load();
//ApiKeyCredential credential = new ApiKeyCredential(Environment.GetEnvironmentVariable("GITHUB_TOKEN_openai_gpt_4o_mini")?? throw new InvalidOperationException("Missing configuration: GITHUB_TOKEN_openai_gpt_4o_mini."));

// get credentials from user secrets
IConfigurationRoot config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
ApiKeyCredential credential = new ApiKeyCredential(config["GITHUB_TOKEN_openai_gpt_4o_mini"] ?? throw new InvalidOperationException("Missing configuration: GITHUB_TOKEN_openai_gpt_4o_mini."));

var model = "openai/gpt-4o-mini";

var openAIOptions = new OpenAIClientOptions()
{
    Endpoint = new Uri(endpoint)

};

// Run the OpenAI client sample. This will execute a few different code samples demonstrating how to use the client.
OpenAIClientType.UsingOpenAiChat(credential, model, openAIOptions).Wait();

// Run the OpenAI chat ChatClient sample. This will execute a few different code samples demonstrating how to use the client.
//TextGeneration.OpenAiChatChatClient.UsingOpenAiChat(credential, model, openAIOptions).Wait();


