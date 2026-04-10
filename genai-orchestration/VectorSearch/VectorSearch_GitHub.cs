using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel.Connectors.InMemory;
using OpenAI;
using System.ClientModel;
using VectorSearch.Models;

namespace VectorSearch;

public static class VectorSearch_GitHub
{
    public static async Task VectorSearchGitHub()
    {
        var token = "text_embedding_3_large";
        var model = "openai/text-embedding-3-large";
        var endpoint = "https://models.github.ai/inference";

        IConfigurationRoot config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

        var credential = new ApiKeyCredential(config[token] ?? throw new InvalidOperationException($"Missing configuration: {token}."));
        var options = new OpenAIClientOptions()
        {
            Endpoint = new Uri(endpoint)
        };

        // Create an embedding generator (text-embedding-3-large is an example)
        IEmbeddingGenerator<string, Embedding<float>> generator =
            new OpenAIClient(credential, options)
                .GetEmbeddingClient(model)
                .AsIEmbeddingGenerator();

        // Create and populate the vector store
        var vectorStore = new InMemoryVectorStore();

        var restaurantsStore = vectorStore.GetCollection<int, Restaurant>("restaurants");

        await restaurantsStore.EnsureCollectionExistsAsync();

        foreach (var restaurant in RestaurantData.Restaurants)
        {
            restaurant.Vector = await generator.GenerateVectorAsync(restaurant.Description);
            await restaurantsStore.UpsertAsync(restaurant);
        }

        Console.WriteLine("=== Restaurant Search Examples ===\n");

        //Console.WriteLine("Query 1: 'I want a casual place for dinner with my family'");
        //Console.WriteLine("Query 2: 'Looking for a romantic spot for date night'");
        //Console.WriteLine("Query 3: 'I want budget-friendly lunch options'");
        Console.WriteLine("Query 4: 'Where can I get authentic Italian pasta?'\n");

        // Uncomment the query you want to test
        // var query = "I want a casual place for dinner with my family";
        // var query = "Looking for a romantic spot for date night";
        // var query = "I want budget-friendly lunch options";
        var query = "Where can I get authentic Italian pasta?";

        Console.WriteLine($"Searching for: \"{query}\"\n");
        Console.WriteLine("Top 3 recommendations:\n");

        var queryEmbedding = await generator.GenerateVectorAsync(query);

        var searchResults = restaurantsStore.SearchAsync(queryEmbedding, top: 3);

        await foreach (var result in searchResults)
        {
            Console.WriteLine($"Name: {result.Record.Name}");
            Console.WriteLine($"Description: {result.Record.Description}");
            Console.WriteLine($"Cuisine: {result.Record.CuisineType}");
            Console.WriteLine($"Price: {result.Record.PriceRange}");
            Console.WriteLine($"Location: {result.Record.Location}");
            Console.WriteLine($"Similarity Score: {result.Score:F3}");
            Console.WriteLine(new string('-', 70));
        }
    }
}
