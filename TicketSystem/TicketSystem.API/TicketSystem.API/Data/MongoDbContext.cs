using MongoDB.Driver;
using TicketSystem.API.Models;
using TicketSystem.API.Settings;

namespace TicketSystem.API.Data;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;
    private readonly MongoDbSettings _settings;

    public MongoDbContext(MongoDbSettings settings)
    {
        var client = new MongoClient(settings.ConnectionString);
        _database = client.GetDatabase(settings.DatabaseName);
        _settings = settings;
    }

    public IMongoCollection<User> Users => _database.GetCollection<User>(_settings.UsersCollection);

    public IMongoCollection<Ticket> Tickets => _database.GetCollection<Ticket>(_settings.TicketsCollection);

    public IMongoCollection<KnowledgeArticle> KnowledgeArticles =>
        _database.GetCollection<KnowledgeArticle>(_settings.KnowledgeBaseCollection);
}
