namespace TicketSystem.API.Settings
{
    public class MongoDbSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
        public string UsersCollection { get; set; } = "users";
        public string TicketsCollection { get; set; } = "tickets";
        public string KnowledgeBaseCollection { get; set; } = "knowledgeArticles";
    }
}