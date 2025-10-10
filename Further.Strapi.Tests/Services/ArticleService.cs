using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Further.Strapi.Tests.Models;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;

namespace Further.Strapi.Tests.Services;

/// <summary>
/// Article 應用服務，展示如何使用 Further.Strapi 進行操作
/// </summary>
public class ArticleService : ApplicationService, ITransientDependency
{
    private readonly ICollectionTypeProvider<Article> _articleProvider;

    public ArticleService(ICollectionTypeProvider<Article> articleProvider)
    {
        _articleProvider = articleProvider;
    }

    /// <summary>
    /// 範例：取得已發布的文章列表，依發布時間排序
    /// </summary>
    public async Task<List<Article>> GetPublishedArticlesAsync()
    {
        return await _articleProvider.GetListAsync(
            filter: f => f
                .Where("title", FilterOperator.ContainsInsensitive, "技術")
                .And("publishedAt", FilterOperator.NotNull),
            populate: p => p
                .Include("author")
                .Include("blocks"),
            sort: s => s.Descending("publishedAt"),
            pagination: p => p.Page(1, 10)
        );
    }

    /// <summary>
    /// 範例：取得文章列表（使用傳統分頁模式）
    /// </summary>
    public async Task<List<Article>> GetArticlesWithPaginationAsync(int page = 1, int pageSize = 10)
    {
        return await _articleProvider.GetListAsync(
            filter: f => f.Where("publishedAt", FilterOperator.NotNull),
            populate: p => p
                .Include("author")
                .Include("category"),
            sort: s => s.Descending("createdAt"),
            pagination: p => p.Page(page, pageSize)
        );
    }

    /// <summary>
    /// 範例：取得文章列表（使用偏移分頁，不計算總數以提升效能）
    /// </summary>
    public async Task<List<Article>> GetArticlesWithOffsetAsync(int start = 0, int limit = 20)
    {
        return await _articleProvider.GetListAsync(
            filter: f => f.Where("publishedAt", FilterOperator.NotNull),
            populate: p => p.Include("author"),
            sort: s => s.Descending("publishedAt"),
            pagination: p => p
                .StartLimit(start, limit)
                .WithCount(false) // 不計算總數，提升效能
        );
    }

    /// <summary>
    /// 範例：使用 PaginationInput 的向下相容性支援
    /// </summary>
    public async Task<List<Article>> GetArticlesCompatibilityAsync(PaginationInput paginationInput)
    {
        return await _articleProvider.GetListAsync(
            filter: f => f.Where("publishedAt", FilterOperator.NotNull),
            populate: p => p.Include("author").Include("category"),
            sort: s => s.Descending("createdAt"),
            pagination: p => p.Page(paginationInput.Page, paginationInput.PageSize)
        );
    }

    /// <summary>
    /// 根據 DocumentId 取得單一文章
    /// </summary>
    public async Task<Article> GetArticleAsync(string documentId)
    {
        return await _articleProvider.GetAsync(documentId);
    }

    /// <summary>
    /// 創建新文章
    /// </summary>
    public async Task<string> CreateArticleAsync(Article article)
    {
        return await _articleProvider.CreateAsync(article);
    }

    /// <summary>
    /// 更新文章
    /// </summary>
    public async Task<string> UpdateArticleAsync(string documentId, Article article)
    {
        return await _articleProvider.UpdateAsync(documentId, article);
    }

    /// <summary>
    /// 刪除文章
    /// </summary>
    public async Task DeleteArticleAsync(string documentId)
    {
        await _articleProvider.DeleteAsync(documentId);
    }
}