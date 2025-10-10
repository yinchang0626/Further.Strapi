using Further.Strapi.Tests.Models;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Further.Strapi.Tests.Integration;

/// <summary>
/// CollectionTypeProvider 整合測試 - 測試真實的 Strapi API
/// </summary>
public class CollectionTypeProviderIntegrationTests : StrapiIntegrationTestBase
{
    private readonly ICollectionTypeProvider<Article> _articleProvider;
    private readonly ICollectionTypeProvider<Author> _authorProvider;
    private readonly ICollectionTypeProvider<Category> _categoryProvider;
    private readonly ITestOutputHelper _output;

    public CollectionTypeProviderIntegrationTests(ITestOutputHelper output)
    {
        _output = output;
        _articleProvider = GetRequiredService<ICollectionTypeProvider<Article>>();
        _authorProvider = GetRequiredService<ICollectionTypeProvider<Author>>();
        _categoryProvider = GetRequiredService<ICollectionTypeProvider<Category>>();
    }

    [Fact]
    public async Task CreateAuthor_ShouldWork()
    {
        // Arrange
        var newAuthor = new Author
        {
            Name = "Test Author",
            Email = "test@example.com"
        };

        try
        {
            // Act
            var documentId = await _authorProvider.CreateAsync(newAuthor);

            // Assert
            documentId.ShouldNotBeNullOrEmpty();
            _output.WriteLine($"Created author with DocumentId: {documentId}");

            // Verify we can retrieve it
            var retrievedAuthor = await _authorProvider.GetAsync(documentId);
            retrievedAuthor.ShouldNotBeNull();
            retrievedAuthor.Name.ShouldBe("Test Author");
            retrievedAuthor.Email.ShouldBe("test@example.com");
            retrievedAuthor.DocumentId.ShouldBe(documentId);

            _output.WriteLine($"Retrieved author: {retrievedAuthor.Name} ({retrievedAuthor.Email})");

            // Cleanup
            await _authorProvider.DeleteAsync(documentId);
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Test failed: {ex.Message}");
            throw;
        }
    }

    [Fact]
    public async Task CreateCategory_ShouldWork()
    {
        // Arrange
        var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        var newCategory = new Category
        {
            Name = "Test Category",
            Description = "This is a test category",
            Slug = $"test-category-{timestamp}"
        };

        try
        {
            // Act
            var documentId = await _categoryProvider.CreateAsync(newCategory);

            // Assert
            documentId.ShouldNotBeNullOrEmpty();
            _output.WriteLine($"Created category with DocumentId: {documentId}");

            // Verify we can retrieve it
            var retrievedCategory = await _categoryProvider.GetAsync(documentId);
            retrievedCategory.ShouldNotBeNull();
            retrievedCategory.Name.ShouldBe("Test Category");
            retrievedCategory.Description.ShouldBe("This is a test category");
            retrievedCategory.Slug.ShouldBe($"test-category-{timestamp}");
            retrievedCategory.DocumentId.ShouldBe(documentId);

            _output.WriteLine($"Retrieved category: {retrievedCategory.Name} - {retrievedCategory.Description}");

            // Cleanup
            await _categoryProvider.DeleteAsync(documentId);
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Test failed: {ex.Message}");
            throw;
        }
    }

    [Fact]
    public async Task CreateArticle_WithRelations_ShouldWork()
    {
        // First create author and category
        var author = new Author
        {
            Name = "Article Author",
            Email = "author@example.com"
        };
        var authorDocumentId = await _authorProvider.CreateAsync(author);

        var category = new Category
        {
            Name = "Article Category",
            Description = "Category for testing articles",
            Slug = $"article-category-{DateTime.Now:yyyyMMdd-HHmmss}"
        };
        var categoryDocumentId = await _categoryProvider.CreateAsync(category);

        try
        {
            // Arrange
            var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
            var newArticle = new Article
            {
                Title = "Test Article",
                Description = "This is a test article for integration testing",
                Slug = $"test-article-{timestamp}",
                Blocks = new List<IStrapiComponent>
                {
                    new SharedRichTextComponent 
                    { 
                        Body = "This is the article content in rich text format."
                    },
                    new SharedQuoteComponent
                    {
                        Title = "Important Quote",
                        Body = "This is an important quote within the article."
                    }
                }
            };

            // Act
            var articleDocumentId = await _articleProvider.CreateAsync(newArticle);

            // Assert
            articleDocumentId.ShouldNotBeNullOrEmpty();
            _output.WriteLine($"Created article with DocumentId: {articleDocumentId}");

            // Verify we can retrieve it
            var retrievedArticle = await _articleProvider.GetAsync(articleDocumentId);
            retrievedArticle.ShouldNotBeNull();
            retrievedArticle.Title.ShouldBe("Test Article");
            retrievedArticle.Description.ShouldBe("This is a test article for integration testing");
            retrievedArticle.Slug.ShouldBe($"test-article-{timestamp}");
            retrievedArticle.DocumentId.ShouldBe(articleDocumentId);

            // Check blocks
            retrievedArticle.Blocks.ShouldNotBeNull();
            retrievedArticle.Blocks.Count.ShouldBe(2);

            var richTextBlock = retrievedArticle.Blocks[0] as SharedRichTextComponent;
            richTextBlock.ShouldNotBeNull();
            richTextBlock.Body.ShouldBe("This is the article content in rich text format.");

            var quoteBlock = retrievedArticle.Blocks[1] as SharedQuoteComponent;
            quoteBlock.ShouldNotBeNull();
            quoteBlock.Title.ShouldBe("Important Quote");
            quoteBlock.Body.ShouldBe("This is an important quote within the article.");

            _output.WriteLine($"Retrieved article: {retrievedArticle.Title}");
            _output.WriteLine($"Article has {retrievedArticle.Blocks.Count} blocks");

            // Cleanup
            await _articleProvider.DeleteAsync(articleDocumentId);
        }
        finally
        {
            // Cleanup author and category
            await _authorProvider.DeleteAsync(authorDocumentId);
            await _categoryProvider.DeleteAsync(categoryDocumentId);
        }
    }

    [Fact]
    public async Task UpdateArticle_ShouldWork()
    {
        // First create an article
        var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        var newArticle = new Article
        {
            Title = "Original Title",
            Description = "Original description",
            Slug = $"original-slug-{timestamp}"
        };

        var documentId = await _articleProvider.CreateAsync(newArticle);

        try
        {
            // Act - Update the article
            var updatedArticle = new Article
            {
                Title = "Updated Title",
                Description = "Updated description with new content",
                Slug = $"updated-slug-{timestamp}"
            };

            var updatedDocumentId = await _articleProvider.UpdateAsync(documentId, updatedArticle);

            // Assert
            updatedDocumentId.ShouldBe(documentId); // Document ID should remain the same

            // Verify the changes
            var retrievedArticle = await _articleProvider.GetAsync(documentId);
            retrievedArticle.ShouldNotBeNull();
            retrievedArticle.Title.ShouldBe("Updated Title");
            retrievedArticle.Description.ShouldBe("Updated description with new content");
            retrievedArticle.Slug.ShouldBe($"updated-slug-{timestamp}");

            _output.WriteLine($"Successfully updated article: {retrievedArticle.Title}");
        }
        finally
        {
            // Cleanup
            await _articleProvider.DeleteAsync(documentId);
        }
    }
}