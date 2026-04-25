

namespace ImageAnalysis_Ollama.Helpers
{
    public static class MimeMapping
    {
        public static string GetMimeType(string fileName)
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
    }
}
