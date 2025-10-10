using Further.Strapi.Tests.Models;
using Further.Strapi.Tests.Services;
using NSubstitute;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Further.Strapi.Tests.Unit;

/// <summary>
/// ArticleService 單元測試 - 使用 Mock，不需要真實 Strapi 環境
/// </summary>
public class ArticleServiceUnitTests
{
    private readonly ICollectionTypeProvider<Article> _mockArticleProvider;
    private readonly ArticleService _articleService;

    public ArticleServiceUnitTests()
    {
        _mockArticleProvider = Substitute.For<ICollectionTypeProvider<Article>>();
        _articleService = new ArticleService(_mockArticleProvider);
    }

    [Fact]
    public async Task CreateArticleAsync_ShouldCallProvider()
    {
        // Arrange
        var article = new Article
        {
            Title = "Test Article",
            Description = "Test Description",
            Slug = "test-article"
        };
        var expectedDocumentId = "test-document-id";
        
        _mockArticleProvider.CreateAsync(article).Returns(expectedDocumentId);

        // Act
        var result = await _articleService.CreateArticleAsync(article);

        // Assert
        result.ShouldBe(expectedDocumentId);
        await _mockArticleProvider.Received(1).CreateAsync(article);
    }

    [Fact]
    public async Task GetArticleAsync_ShouldCallProvider()
    {
        // Arrange
        var documentId = "test-document-id";
        var expectedArticle = new Article
        {
            DocumentId = documentId,
            Title = "Test Article",
            Description = "Test Description"
        };
        
        _mockArticleProvider.GetAsync(documentId).Returns(expectedArticle);

        // Act
        var result = await _articleService.GetArticleAsync(documentId);

        // Assert
        result.ShouldBe(expectedArticle);
        await _mockArticleProvider.Received(1).GetAsync(documentId);
    }

    [Fact]
    public async Task UpdateArticleAsync_ShouldCallProvider()
    {
        // Arrange
        var documentId = "test-document-id";
        var article = new Article
        {
            Title = "Updated Article",
            Description = "Updated Description"
        };
        
        _mockArticleProvider.UpdateAsync(documentId, article).Returns(documentId);

        // Act
        var result = await _articleService.UpdateArticleAsync(documentId, article);

        // Assert
        result.ShouldBe(documentId);
        await _mockArticleProvider.Received(1).UpdateAsync(documentId, article);
    }

    [Fact]
    public async Task DeleteArticleAsync_ShouldCallProvider()
    {
        // Arrange
        var documentId = "test-document-id";

        // Act
        await _articleService.DeleteArticleAsync(documentId);

        // Assert
        await _mockArticleProvider.Received(1).DeleteAsync(documentId);
    }

    [Fact]
    public async Task GetPublishedArticlesAsync_ShouldCallProviderWithCorrectParameters()
    {
        // Arrange
        var expectedArticles = new List<Article>
        {
            new Article { DocumentId = "1", Title = "Article 1" },
            new Article { DocumentId = "2", Title = "Article 2" }
        };

        _mockArticleProvider.GetListAsync(
            Arg.Any<Action<FilterBuilder>>(),
            Arg.Any<Action<PopulateBuilder>>(),
            Arg.Any<Action<SortBuilder>>(),
            Arg.Any<Action<PaginationBuilder>>())
            .Returns(expectedArticles);

        // Act
        var result = await _articleService.GetPublishedArticlesAsync();

        // Assert
        result.ShouldBe(expectedArticles);
        await _mockArticleProvider.Received(1).GetListAsync(
            Arg.Any<Action<FilterBuilder>>(),
            Arg.Any<Action<PopulateBuilder>>(),
            Arg.Any<Action<SortBuilder>>(),
            Arg.Any<Action<PaginationBuilder>>());
    }

    [Fact]
    public async Task GetArticlesWithPaginationAsync_ShouldCallProviderWithCorrectParameters()
    {
        // Arrange
        var page = 1;
        var pageSize = 10;
        var expectedArticles = new List<Article>
        {
            new Article { DocumentId = "1", Title = "Article 1" }
        };

        _mockArticleProvider.GetListAsync(
            Arg.Any<Action<FilterBuilder>>(),
            Arg.Any<Action<PopulateBuilder>>(),
            Arg.Any<Action<SortBuilder>>(),
            Arg.Any<Action<PaginationBuilder>>())
            .Returns(expectedArticles);

        // Act
        var result = await _articleService.GetArticlesWithPaginationAsync(page, pageSize);

        // Assert
        result.ShouldBe(expectedArticles);
        await _mockArticleProvider.Received(1).GetListAsync(
            Arg.Any<Action<FilterBuilder>>(),
            Arg.Any<Action<PopulateBuilder>>(),
            Arg.Any<Action<SortBuilder>>(),
            Arg.Any<Action<PaginationBuilder>>());
    }

    [Fact]
    public async Task GetArticlesWithOffsetAsync_ShouldCallProviderWithCorrectParameters()
    {
        // Arrange
        var start = 0;
        var limit = 20;
        var expectedArticles = new List<Article>
        {
            new Article { DocumentId = "1", Title = "Article 1" }
        };

        _mockArticleProvider.GetListAsync(
            Arg.Any<Action<FilterBuilder>>(),
            Arg.Any<Action<PopulateBuilder>>(),
            Arg.Any<Action<SortBuilder>>(),
            Arg.Any<Action<PaginationBuilder>>())
            .Returns(expectedArticles);

        // Act
        var result = await _articleService.GetArticlesWithOffsetAsync(start, limit);

        // Assert
        result.ShouldBe(expectedArticles);
        await _mockArticleProvider.Received(1).GetListAsync(
            Arg.Any<Action<FilterBuilder>>(),
            Arg.Any<Action<PopulateBuilder>>(),
            Arg.Any<Action<SortBuilder>>(),
            Arg.Any<Action<PaginationBuilder>>());
    }

    [Fact]
    public async Task GetArticlesCompatibilityAsync_ShouldCallProviderWithCorrectParameters()
    {
        // Arrange
        var paginationInput = new PaginationInput { Page = 1, PageSize = 5 };
        var expectedArticles = new List<Article>
        {
            new Article { DocumentId = "1", Title = "Article 1" }
        };

        _mockArticleProvider.GetListAsync(
            Arg.Any<Action<FilterBuilder>>(),
            Arg.Any<Action<PopulateBuilder>>(),
            Arg.Any<Action<SortBuilder>>(),
            Arg.Any<Action<PaginationBuilder>>())
            .Returns(expectedArticles);

        // Act
        var result = await _articleService.GetArticlesCompatibilityAsync(paginationInput);

        // Assert
        result.ShouldBe(expectedArticles);
        await _mockArticleProvider.Received(1).GetListAsync(
            Arg.Any<Action<FilterBuilder>>(),
            Arg.Any<Action<PopulateBuilder>>(),
            Arg.Any<Action<SortBuilder>>(),
            Arg.Any<Action<PaginationBuilder>>());
    }
}