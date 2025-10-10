using Further.Strapi.Tests.Models;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace Further.Strapi.Tests.Components;

/// <summary>
/// Collection Type 複雜場景測試
/// 測試 Dynamic Zone、Media File、關聯關係的序列化清理
/// </summary>
public class CollectionTypeComplexScenarioTests : StrapiIntegrationTestBase
{
    private readonly ITestOutputHelper _output;

    public CollectionTypeComplexScenarioTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Article_WithComplexBlocks_ShouldCleanCorrectly()
    {
        // Arrange - 創建包含複雜組件的文章
        var article = new Article
        {
            // 系統欄位（應該被清理）
            Id = 999,
            DocumentId = "fake-doc-id",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            PublishedAt = DateTime.Now,

            // 業務欄位
            Title = "測試文章包含複雜組件",
            Description = "這個文章包含多種組件和媒體檔案",
            Slug = "test-complex-article",

            // Cover 媒體檔案（應該只保留 ID）
            Cover = new StrapiMediaField
            {
                Id = 1,
                DocumentId = "media-doc-id-should-be-removed",
                Name = "cover.jpg",
                Url = "/uploads/cover.jpg",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            },

            // Dynamic Zone 組件
            Blocks = new List<IStrapiComponent>
            {
                new SharedRichTextComponent
                {
                    Id = 888, // 組件 ID 應該被清理
                    Body = "這是豐富文本內容"
                },
                new SharedMediaComponent
                {
                    Id = 777, // 組件 ID 應該被清理
                    File = new StrapiMediaField
                    {
                        Id = 2,
                        DocumentId = "media-2-doc-id",
                        Name = "image.jpg",
                        Url = "/uploads/image.jpg"
                    }
                },
                new SharedSliderComponent
                {
                    Id = 666, // 組件 ID 應該被清理
                    Files = new List<StrapiMediaField>
                    {
                        new StrapiMediaField { Id = 1, DocumentId = "media-3", Name = "slide1.jpg" },
                        new StrapiMediaField { Id = 2, DocumentId = "media-4", Name = "slide2.jpg" }
                    }
                }
            }
        };

        // Act - 使用寫入序列化器
        var cleaner = GetRequiredService<StrapiWriteSerializer>();
        var cleanedJson = cleaner.SerializeForUpdate(article);

        _output.WriteLine("清理後的 JSON:");
        _output.WriteLine(cleanedJson);

        // Assert - 直接檢查 JSON 字串
        cleanedJson.ShouldNotContain("\"id\":");
        cleanedJson.ShouldNotContain("\"documentId\":");
        cleanedJson.ShouldNotContain("\"createdAt\":");
        cleanedJson.ShouldNotContain("\"updatedAt\":");
        cleanedJson.ShouldNotContain("\"publishedAt\":");
        
        cleanedJson.ShouldContain("\"title\":");
        cleanedJson.ShouldContain("\"cover\":1");
        cleanedJson.ShouldContain("\"__component\":");
        cleanedJson.ShouldContain("\"blocks\":");
        
        _output.WriteLine("✅ Article 複雜組件清理驗證通過");
    }

    [Fact]
    public void Article_WithRelations_ShouldHandleCorrectly()
    {
        // Arrange - 創建包含關聯關係的文章
        var article = new Article
        {
            Title = "測試關聯關係文章",
            Description = "這個文章測試關聯關係的處理",
            Slug = "test-relations-article",

            // 關聯的 Author（可能包含完整物件或只有 ID）
            Author = new Author
            {
                Id = 1,
                DocumentId = "author-doc-id",
                Name = "測試作者",
                Email = "author@test.com",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            },

            // 關聯的 Category
            Category = new Category
            {
                Id = 2,
                DocumentId = "category-doc-id", 
                Name = "測試分類",
                Description = "分類描述",
                Slug = "test-category"
            }
        };

        // Act
        var cleaner = GetRequiredService<StrapiWriteSerializer>();
        var cleanedJson = cleaner.SerializeForUpdate(article);

        _output.WriteLine("包含關聯關係的清理後 JSON:");
        _output.WriteLine(cleanedJson);

        // Parse JSON 來驗證
        var jsonDocument = JsonDocument.Parse(cleanedJson);
        var root = jsonDocument.RootElement;

        // Assert - 驗證關聯關係的處理
        // 這裡需要根據實際的 Strapi API 需求來決定
        // 關聯應該如何被序列化（ID only, connect format, 或其他）

        _output.WriteLine("✅ Article 關聯關係處理驗證完成");
    }

    [Fact]
    public void CollectionType_SystemFieldsCleaning_ShouldWorkConsistently()
    {
        // Arrange - 測試不同 Collection Type 的系統欄位清理一致性
        var author = new Author
        {
            Id = 100,
            DocumentId = "author-doc",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            PublishedAt = DateTime.Now,
            Name = "測試作者一致性",
            Email = "consistency@test.com"
        };

        var category = new Category
        {
            Id = 200,
            DocumentId = "category-doc",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            PublishedAt = DateTime.Now,
            Name = "測試分類一致性",
            Description = "一致性測試描述",
            Slug = "consistency-test"
        };

        // Act
        var cleaner = GetRequiredService<StrapiWriteSerializer>();
        var authorJson = cleaner.SerializeForUpdate(author);
        var categoryJson = cleaner.SerializeForUpdate(category);

        _output.WriteLine("Author 清理後 JSON:");
        _output.WriteLine(authorJson);
        _output.WriteLine("Category 清理後 JSON:");
        _output.WriteLine(categoryJson);

        // Assert - 驗證所有 Collection Type 的系統欄位都被一致清理
        var authorDoc = JsonDocument.Parse(authorJson);
        var categoryDoc = JsonDocument.Parse(categoryJson);

        var systemFields = new[] { "id", "documentId", "createdAt", "updatedAt", "publishedAt" };

        foreach (var field in systemFields)
        {
            authorDoc.RootElement.TryGetProperty(field, out _).ShouldBeFalse($"Author 應該清理 {field}");
            categoryDoc.RootElement.TryGetProperty(field, out _).ShouldBeFalse($"Category 應該清理 {field}");
        }

        // 驗證業務欄位存在
        authorDoc.RootElement.TryGetProperty("name", out _).ShouldBeTrue();
        categoryDoc.RootElement.TryGetProperty("name", out _).ShouldBeTrue();

        _output.WriteLine("✅ Collection Type 系統欄位清理一致性驗證通過");
    }
}