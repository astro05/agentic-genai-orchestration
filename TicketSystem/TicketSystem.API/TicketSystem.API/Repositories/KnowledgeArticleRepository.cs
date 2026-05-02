using MongoDB.Driver;
using TicketSystem.API.Data;
using TicketSystem.API.Models;

namespace TicketSystem.API.Repositories;

public class KnowledgeArticleRepository : IKnowledgeArticleRepository
{
    private readonly IMongoCollection<KnowledgeArticle> _articles;

    public KnowledgeArticleRepository(MongoDbContext dbContext)
    {
        _articles = dbContext.KnowledgeArticles;
    }

    public Task<List<KnowledgeArticle>> GetAllAsync() =>
        _articles.Find(_ => true).ToListAsync();

    public Task InsertManyAsync(IEnumerable<KnowledgeArticle> articles) =>
        _articles.InsertManyAsync(articles);

    public Task<long> CountAllAsync() =>
        _articles.CountDocumentsAsync(_ => true);
}
