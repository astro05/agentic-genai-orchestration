using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Embeddings;
using System;
using System.ClientModel;
using System.ClientModel.Primitives;
using System.Collections.Generic;
using System.Numerics.Tensors;
using System.Text;

namespace VectorSearch
{
    /// <summary>
    /// Provides example methods for generating text embeddings using the OpenAI API and demonstrates how to compare
    /// embedding similarities.
    /// </summary>
    /// <remarks>This class includes sample code for authenticating with the OpenAI service, generating
    /// embeddings for input text, handling exceptions during asynchronous operations, and comparing the similarity of
    /// multiple embeddings using cosine similarity. It is intended as a reference for developers integrating embedding
    /// generation and similarity comparison into their applications.</remarks>
    public static class EmbeddingsExample
    {
        public static async Task EmbeddingAsync()
        {
            var token = "text_embedding_3_large";
            var model = "openai/text-embedding-3-large";
            var endpoint = "https://models.github.ai/inference";

            // get credentials from user secrets
            IConfigurationRoot config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

            ApiKeyCredential credential = new ApiKeyCredential(config[token] ?? throw new InvalidOperationException($"Missing configuration: {token}"));
            var openAIOptions = new OpenAIClientOptions()
            {
                Endpoint = new Uri(endpoint)
            };

            #region Run a basic code sample

            //EmbeddingClient client = new(model, credential, openAIOptions);
            //OpenAIEmbeddingCollection response = client.GenerateEmbeddings(
            //    new List<string>{"first phrase", "second phrase", "third phrase"}
            //);

            //foreach (OpenAIEmbedding embedding in response)
            //{
            //    ReadOnlyMemory<float> vector = embedding.ToFloats();
            //    int length = vector.Length;
            //    System.Console.Write($"data[{embedding.Index}]: length={length}, ");
            //    System.Console.Write($"[{vector.Span[0]}, {vector.Span[1]}, ..., ");
            //    System.Console.WriteLine($"{vector.Span[length - 2]}, {vector.Span[length - 1]}]");
            //}

            #endregion
            #region Run a more detailed code sample with error handling and similarity comparison

            // Initialize the generator
            IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator =
                new OpenAIClient(credential, openAIOptions)
                    .GetEmbeddingClient(model)
                    .AsIEmbeddingGenerator();

            try
            {
                Console.WriteLine("Generating embedding...");

                // 1: Generate a single embedding
                // Added a CancellationToken for safety so it doesn't hang forever during testing
                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                var embedding = await embeddingGenerator.GenerateVectorAsync("Hello, world!", cancellationToken: cts.Token);

                Console.WriteLine($"Embedding dimensions: {embedding.Length}");

                // Use a small sample to avoid flooding the console
                for (int i = 0; i < 10; i++)
                {
                    Console.Write("{0:0.00}, ", embedding.Span[i]);
                }
                Console.WriteLine("...");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            // 2: Compare multiple embeddings using Cosine Similarity
            Console.WriteLine("Compare multiple embeddings using Cosine Similarity");
            var catVector = await embeddingGenerator.GenerateVectorAsync("cat");
            var dogVector = await embeddingGenerator.GenerateVectorAsync("dog");
            var kittenVector = await embeddingGenerator.GenerateVectorAsync("kitten");

            Console.WriteLine($"cat-dog similarity: {TensorPrimitives.CosineSimilarity(catVector.Span, dogVector.Span):F2}");
            Console.WriteLine($"cat-kitten similarity: {TensorPrimitives.CosineSimilarity(catVector.Span, kittenVector.Span):F2}");
            Console.WriteLine($"dog-kitten similarity: {TensorPrimitives.CosineSimilarity(dogVector.Span, kittenVector.Span):F2}");

            #endregion
        }
    }
}
