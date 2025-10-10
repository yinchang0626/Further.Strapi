using Further.Strapi.Tests.Models;
using Further.Strapi.Tests.Services;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Further.Strapi.Tests.Services;

/// <summary>
/// ArticleService 測試類別 - 需要真實 Strapi 服務運行
/// </summary>
public class ArticleServiceTests : StrapiRealIntegrationTestBase
{
    private readonly ArticleService _articleService;
    private readonly ITestOutputHelper _output;

    public ArticleServiceTests(ITestOutputHelper output)
    {
        _output = output;
        _articleService = GetRequiredService<ArticleService>();
    }

    [Fact]
    public async Task ArticleService_CRUD_ShouldWork()
    {
        // Arrange
        var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        var testArticle = new Article
        {
            Title = $"ArticleService 測試文章-{timestamp}",
            Description = "通過 ArticleService 創建的測試文章",
            Slug = $"article-service-test-{timestamp}"
        };

        string documentId = null;

        try
        {
            // Act 1: 創建文章
            _output.WriteLine("📝 測試創建文章...");
            documentId = await _articleService.CreateArticleAsync(testArticle);

            // Assert 1: 驗證創建成功
            documentId.ShouldNotBeNullOrEmpty();
            _output.WriteLine($"✅ 成功創建文章，DocumentId: {documentId}");

            // Act 2: 讀取文章
            _output.WriteLine("📖 測試讀取文章...");
            var retrievedArticle = await _articleService.GetArticleAsync(documentId);

            // Assert 2: 驗證讀取成功
            retrievedArticle.ShouldNotBeNull();
            retrievedArticle.DocumentId.ShouldBe(documentId);
            retrievedArticle.Title.ShouldBe($"ArticleService 測試文章-{timestamp}");
            retrievedArticle.Description.ShouldBe("通過 ArticleService 創建的測試文章");
            retrievedArticle.Slug.ShouldBe($"article-service-test-{timestamp}");
            _output.WriteLine($"✅ 成功讀取文章: {retrievedArticle.Title}");

            // Act 3: 更新文章
            _output.WriteLine("📝 測試更新文章...");
            var updatedArticle = new Article
            {
                Title = $"已更新的 ArticleService 測試文章-{timestamp}",
                Description = "這是更新後的描述內容",
                Slug = $"updated-article-service-test-{timestamp}"
            };

            var updatedDocumentId = await _articleService.UpdateArticleAsync(documentId, updatedArticle);

            // Assert 3: 驗證更新成功
            updatedDocumentId.ShouldBe(documentId);
            _output.WriteLine($"✅ 成功更新文章");

            // 驗證更新結果
            var verifyUpdatedArticle = await _articleService.GetArticleAsync(documentId);
            verifyUpdatedArticle.ShouldNotBeNull();
            verifyUpdatedArticle.Title.ShouldBe($"已更新的 ArticleService 測試文章-{timestamp}");
            verifyUpdatedArticle.Description.ShouldBe("這是更新後的描述內容");
            verifyUpdatedArticle.Slug.ShouldBe($"updated-article-service-test-{timestamp}");
            _output.WriteLine($"✅ 確認文章更新成功: {verifyUpdatedArticle.Title}");

        }
        finally
        {
            // Cleanup: 刪除文章
            if (!string.IsNullOrEmpty(documentId))
            {
                try
                {
                    _output.WriteLine("🗑️ 測試刪除文章...");
                    await _articleService.DeleteArticleAsync(documentId);
                    _output.WriteLine($"✅ 成功刪除文章: {documentId}");

                    // 驗證刪除成功
                    var exception = await Should.ThrowAsync<Exception>(async () =>
                    {
                        await _articleService.GetArticleAsync(documentId);
                    });
                    _output.WriteLine($"✅ 確認文章已刪除，嘗試讀取時拋出例外: {exception.Message}");
                }
                catch (Exception cleanupEx)
                {
                    _output.WriteLine($"⚠️ 清理失敗: {cleanupEx.Message}");
                }
            }
        }
    }

    [Fact]
    public async Task ArticleService_CreateWithComponents_ShouldWork()
    {
        // Arrange
        var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        var articleWithComponents = new Article
        {
            Title = $"包含組件的文章-{timestamp}",
            Description = "這個文章包含動態組件",
            Slug = $"article-with-components-{timestamp}",
            Blocks = new List<IStrapiComponent>
            {
                new SharedRichTextComponent
                {
                    Body = "這是一段豐富的文本內容，通過 ArticleService 創建。"
                },
                new SharedQuoteComponent
                {
                    Title = "重要提示",
                    Body = "這是通過 ArticleService 創建的引言組件。"
                }
            }
        };

        string documentId = null;

        try
        {
            // Act
            _output.WriteLine("📝 測試創建包含組件的文章...");
            documentId = await _articleService.CreateArticleAsync(articleWithComponents);

            // Assert
            documentId.ShouldNotBeNullOrEmpty();
            _output.WriteLine($"✅ 成功創建包含組件的文章，DocumentId: {documentId}");

            // 讀取並驗證組件
            var retrievedArticle = await _articleService.GetArticleAsync(documentId);
            retrievedArticle.ShouldNotBeNull();
            retrievedArticle.Title.ShouldBe($"包含組件的文章-{timestamp}");
            
            if (retrievedArticle.Blocks != null && retrievedArticle.Blocks.Count > 0)
            {
                _output.WriteLine($"✅ 文章包含 {retrievedArticle.Blocks.Count} 個組件");
                
                // 檢查 RichText 組件
                var richTextComponent = retrievedArticle.Blocks[0] as SharedRichTextComponent;
                if (richTextComponent != null)
                {
                    richTextComponent.Body.ShouldContain("ArticleService");
                    _output.WriteLine($"✅ RichText 組件內容正確");
                }

                // 檢查 Quote 組件
                if (retrievedArticle.Blocks.Count > 1)
                {
                    var quoteComponent = retrievedArticle.Blocks[1] as SharedQuoteComponent;
                    if (quoteComponent != null)
                    {
                        quoteComponent.Title.ShouldBe("重要提示");
                        quoteComponent.Body.ShouldContain("ArticleService");
                        _output.WriteLine($"✅ Quote 組件內容正確");
                    }
                }
            }
            else
            {
                _output.WriteLine("⚠️ 文章的組件為空");
            }
        }
        finally
        {
            // Cleanup
            if (!string.IsNullOrEmpty(documentId))
            {
                try
                {
                    await _articleService.DeleteArticleAsync(documentId);
                    _output.WriteLine($"🗑️ 已清理測試資料: {documentId}");
                }
                catch (Exception cleanupEx)
                {
                    _output.WriteLine($"⚠️ 清理失敗: {cleanupEx.Message}");
                }
            }
        }
    }

    [Fact]
    public async Task ArticleService_CreateWithAuthor_ShouldWork()
    {
        // Arrange - 先創建作者，再創建包含作者關聯的文章
        var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        var authorProvider = GetRequiredService<ICollectionTypeProvider<Author>>();
        string authorDocumentId = null;
        string documentId = null;

        try
        {
            // 步驟 1: 先創建作者
            _output.WriteLine("📝 步驟 1: 創建測試作者");
            var author = new Author
            {
                Name = $"測試作者-{timestamp}",
                Email = $"author-{timestamp}@test.com"
            };
            
            authorDocumentId = await authorProvider.CreateAsync(author);
            authorDocumentId.ShouldNotBeNullOrEmpty();
            _output.WriteLine($"✅ 成功創建作者，DocumentId: {authorDocumentId}");

            // 步驟 2: 創建包含作者關聯的文章
            var articleWithAuthor = new Article
            {
                Title = $"包含作者的文章-{timestamp}",
                Description = "這個文章包含作者關聯",
                Slug = $"article-with-author-{timestamp}",
                Author = new Author
                {
                    DocumentId = authorDocumentId // 使用動態創建的 DocumentId
                }
            };

            // Act
            _output.WriteLine("📝 步驟 2: 測試創建包含作者的文章...");
            documentId = await _articleService.CreateArticleAsync(articleWithAuthor);

            // Assert
            documentId.ShouldNotBeNullOrEmpty();
            _output.WriteLine($"✅ 成功創建包含作者的文章，DocumentId: {documentId}");

            // 讀取並驗證作者關聯
            var retrievedArticle = await _articleService.GetArticleAsync(documentId);
            retrievedArticle.ShouldNotBeNull();
            retrievedArticle.Title.ShouldBe($"包含作者的文章-{timestamp}");

            if (retrievedArticle.Author != null)
            {
                _output.WriteLine($"✅ 文章作者: {retrievedArticle.Author.Name} (DocumentId: {retrievedArticle.Author.DocumentId})");
                
                // 驗證作者資料是否正確
                retrievedArticle.Author.DocumentId.ShouldBe(authorDocumentId);
                retrievedArticle.Author.Name.ShouldBe($"測試作者-{timestamp}");
            }
            else
            {
                _output.WriteLine("⚠️ 文章的作者關聯為空 - 可能需要 populate 參數");
            }
        }
        finally
        {
            // Cleanup - 先清理文章，再清理作者
            if (!string.IsNullOrEmpty(documentId))
            {
                try
                {
                    await _articleService.DeleteArticleAsync(documentId);
                    _output.WriteLine($"🗑️ 已清理測試文章: {documentId}");
                }
                catch (Exception cleanupEx)
                {
                    _output.WriteLine($"⚠️ 清理文章失敗: {cleanupEx.Message}");
                }
            }
            
            if (!string.IsNullOrEmpty(authorDocumentId))
            {
                try
                {
                    await authorProvider.DeleteAsync(authorDocumentId);
                    _output.WriteLine($"🗑️ 已清理測試作者: {authorDocumentId}");
                }
                catch (Exception cleanupEx)
                {
                    _output.WriteLine($"⚠️ 清理作者失敗: {cleanupEx.Message}");
                }
            }
        }
    }

    [Fact]
    public async Task ArticleService_GetListAsync_WithFilters_ShouldWork()
    {
        // Arrange
        var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        var testArticles = new List<Article>();
        var documentIds = new List<string>();

        try
        {
            // 創建測試文章
            _output.WriteLine("📝 創建測試文章...");
            for (int i = 1; i <= 3; i++)
            {
                var article = new Article
                {
                    Title = $"GetList測試文章-{i}-{timestamp}",
                    Description = $"第 {i} 篇測試文章",
                    Slug = $"getlist-test-{i}-{timestamp}"
                };
                testArticles.Add(article);
                
                var documentId = await _articleService.CreateArticleAsync(article);
                documentIds.Add(documentId);
                _output.WriteLine($"✅ 創建文章 {i}: {documentId}");
            }

            // Act 1: 測試 GetPublishedArticlesAsync (帶篩選)
            _output.WriteLine("🔍 測試帶篩選的文章列表查詢...");
            var publishedArticles = await _articleService.GetPublishedArticlesAsync();
            
            // Assert 1: 驗證列表查詢
            publishedArticles.ShouldNotBeNull();
            _output.WriteLine($"✅ 成功取得 {publishedArticles.Count} 篇已發布文章");

            // Act 2: 測試 GetArticlesWithPaginationAsync (分頁查詢)
            _output.WriteLine("📄 測試分頁查詢...");
            var pagedArticles = await _articleService.GetArticlesWithPaginationAsync(1, 5);
            
            // Assert 2: 驗證分頁查詢
            pagedArticles.ShouldNotBeNull();
            pagedArticles.Count.ShouldBeLessThanOrEqualTo(5);
            _output.WriteLine($"✅ 成功取得第1頁文章，共 {pagedArticles.Count} 篇");

            // Act 3: 測試 GetArticlesWithOffsetAsync (偏移查詢)
            _output.WriteLine("⏭️ 測試偏移查詢...");
            var offsetArticles = await _articleService.GetArticlesWithOffsetAsync(0, 3);
            
            // Assert 3: 驗證偏移查詢
            offsetArticles.ShouldNotBeNull();
            offsetArticles.Count.ShouldBeLessThanOrEqualTo(3);
            _output.WriteLine($"✅ 成功取得偏移文章，共 {offsetArticles.Count} 篇");

            // Act 4: 測試 GetArticlesCompatibilityAsync (相容性查詢)
            _output.WriteLine("🔄 測試相容性分頁查詢...");
            var paginationInput = new PaginationInput { Page = 1, PageSize = 2 };
            var compatibilityArticles = await _articleService.GetArticlesCompatibilityAsync(paginationInput);
            
            // Assert 4: 驗證相容性查詢
            compatibilityArticles.ShouldNotBeNull();
            compatibilityArticles.Count.ShouldBeLessThanOrEqualTo(2);
            _output.WriteLine($"✅ 成功取得相容性分頁文章，共 {compatibilityArticles.Count} 篇");

        }
        finally
        {
            // Cleanup: 清理測試文章
            _output.WriteLine("🗑️ 清理測試文章...");
            foreach (var documentId in documentIds)
            {
                try
                {
                    await _articleService.DeleteArticleAsync(documentId);
                    _output.WriteLine($"✅ 成功刪除文章: {documentId}");
                }
                catch (Exception cleanupEx)
                {
                    _output.WriteLine($"⚠️ 清理失敗 {documentId}: {cleanupEx.Message}");
                }
            }
        }
    }

    [Fact]
    public async Task ArticleService_GetListAsync_EmptyFilters_ShouldWork()
    {
        // Act: 測試不帶參數的列表查詢
        _output.WriteLine("📋 測試不帶篩選的文章列表查詢...");
        var allArticles = await _articleService.GetArticlesWithPaginationAsync(1, 20);

        // Assert: 驗證基本查詢
        allArticles.ShouldNotBeNull();
        _output.WriteLine($"✅ 成功取得文章列表，共 {allArticles.Count} 篇文章");

        // 驗證每篇文章都有基本資訊
        foreach (var article in allArticles)
        {
            article.ShouldNotBeNull();
            article.DocumentId.ShouldNotBeNullOrEmpty();
            article.Title.ShouldNotBeNullOrEmpty();
            _output.WriteLine($"📄 文章: {article.Title} (ID: {article.DocumentId})");
        }
    }
}