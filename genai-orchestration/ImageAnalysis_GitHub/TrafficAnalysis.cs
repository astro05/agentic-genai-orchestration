using ImageAnalysis_GitHub.Helpers;
using ImageAnalysis_GitHub.Models;
using Microsoft.Extensions.AI;
using System;
using System.Collections.Generic;
using System.Text;

namespace ImageAnalysis_GitHub
{
    public static class TrafficAnalysis
    {
        public static async Task AnalyzeTrafficAsync(IChatClient client)
        {
            // Define supported extensions
            string[] extensions = { "*.jpg", "*.jpeg", "*.png", "*.webp" };

            // Get all matching files using LINQ to flatten the lists
            var imageFiles = extensions.SelectMany(ext => Directory.GetFiles("images", ext));

            foreach (var imagePath in imageFiles)
            {
                var name = Path.GetFileNameWithoutExtension(imagePath);

                // Detect the correct MIME type (image/jpeg, image/png, etc.)
                string mimeType = MimeMapping.GetMimeType(imagePath);

                var message = new ChatMessage(ChatRole.User, $$"""
                        Extract information from this image from camera {{name}}.
                        Respond with a JSON object in this form:
                        {
                            "Status": string, // "Clear", "Flowing", "Congested", "Blocked"
                            "NumCars": number,
                            "NumTrucks": number
                        }
                        """);

                // Pass the dynamic mimeType here
                message.Contents.Add(new DataContent(File.ReadAllBytes(imagePath), mimeType));

                var response = await client.GetResponseAsync<TrafficCamResult>([message]);

                if (response.TryGetResult(out var result))
                {
                    Console.WriteLine($"{name} status: {result.Status} (cars: {result.NumCars}, trucks: {result.NumTrucks})");
                }
            }
        }
    }
}
