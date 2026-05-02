using TicketSystem.API.Models;
using TicketSystem.API.Repositories;

namespace TicketSystem.API.Services;

public class KnowledgeBaseService
{
    private readonly IKnowledgeArticleRepository _articleRepository;

    public KnowledgeBaseService(IKnowledgeArticleRepository articleRepository)
    {
        _articleRepository = articleRepository;
    }

    /// <summary>
    /// Top articles for a ticket: same category, general inquiry, plus keyword overlap.
    /// </summary>
    public async Task<List<KnowledgeArticle>> GetRelevantArticlesAsync(
        TicketCategory ticketCategory,
        string title,
        string description,
        int maxArticles = 5)
    {
        var all = await _articleRepository.GetAllAsync();

        var terms = Tokenize($"{title} {description}");
        var scored = all
            .Where(a =>
                a.Category == ticketCategory ||
                a.Category == TicketCategory.GeneralInquiry ||
                a.Category == TicketCategory.UncategorizedIssue)
            .Select(a => (Article: a, Score: ScoreArticle(a, terms, ticketCategory)))
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Article.Title)
            .Take(maxArticles)
            .Select(x => x.Article)
            .ToList();

        if (scored.Count > 0)
            return scored;

        return all.OrderBy(a => a.Title).Take(maxArticles).ToList();
    }

    private static int ScoreArticle(KnowledgeArticle a, HashSet<string> terms, TicketCategory ticketCategory)
    {
        var score = 0;
        if (a.Category == ticketCategory) score += 50;
        if (a.Category == TicketCategory.GeneralInquiry) score += 10;

        var blob = $"{a.Title} {a.Summary} {a.Body}".ToLowerInvariant();
        foreach (var t in terms)
        {
            if (t.Length < 3) continue;
            if (blob.Contains(t, StringComparison.Ordinal)) score += 3;
        }

        foreach (var kw in a.Keywords)
        {
            if (terms.Contains(kw.ToLowerInvariant())) score += 8;
        }

        return score;
    }

    private static HashSet<string> Tokenize(string text)
    {
        var separators = new[] { ' ', '\n', '\r', '\t', '.', ',', ';', ':', '!', '?', '(', ')', '[', ']' };
        return text
            .ToLowerInvariant()
            .Split(separators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(s => s.Length >= 2)
            .ToHashSet();
    }
}
