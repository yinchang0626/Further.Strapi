using Shouldly;
using System;
using System.Collections.Generic;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace Further.Strapi.Tests.Unit;

/// <summary>
/// JSON 序列化測試
/// 驗證 .NET 的 System.Text.Json 預設命名策略
/// </summary>
public class JsonSerializationTests
{
    private readonly ITestOutputHelper _output;

    public JsonSerializationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void SystemTextJson_DefaultNamingPolicy_ShouldMatchCSharpPropertyNames()
    {
        // Arrange
        var testObject = new StrapiMediaField
        {
            Id = 123,
            DocumentId = "test-doc-id",
            Name = "test-file.jpg",
            AlternativeText = "alt text",
            Caption = "caption text",
            Width = 800,
            Height = 600,
            CreatedAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 1, 2, 12, 0, 0, DateTimeKind.Utc),
            PublishedAt = new DateTime(2024, 1, 3, 12, 0, 0, DateTimeKind.Utc)
        };

        // Act - 使用預設選項序列化
        var jsonWithDefault = JsonSerializer.Serialize(testObject);
        _output.WriteLine("預設序列化結果:");
        _output.WriteLine(jsonWithDefault);

        // Act - 使用 camelCase 選項序列化
        var camelCaseOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        var jsonWithCamelCase = JsonSerializer.Serialize(testObject, camelCaseOptions);
        _output.WriteLine("\nCamelCase 序列化結果:");
        _output.WriteLine(jsonWithCamelCase);

        // Assert - 檢查預設行為是否符合 Strapi 的期望
        // 預設情況下，System.Text.Json 保持原始屬性名稱
        jsonWithDefault.ShouldContain("\"Id\":");
        jsonWithDefault.ShouldContain("\"DocumentId\":");
        jsonWithDefault.ShouldContain("\"AlternativeText\":");

        // CamelCase 策略應該轉換為小寫開頭
        jsonWithCamelCase.ShouldContain("\"id\":");
        jsonWithCamelCase.ShouldContain("\"documentId\":");
        jsonWithCamelCase.ShouldContain("\"alternativeText\":");
    }

    [Fact]
    public void SystemTextJson_DeserializeFromStrapiResponse_ShouldWork()
    {
        // Arrange - 實際的 Strapi API 回應格式
        var strapiJson = """
        {
            "id": 4,
            "documentId": "vuyzqv9x269tp034kjwzpiaz",
            "name": "test-upload-20251010-074330.txt",
            "alternativeText": null,
            "caption": null,
            "width": null,
            "height": null,
            "formats": null,
            "hash": "test_upload_20251010_074330_2a0a6ac571",
            "ext": ".txt",
            "mime": "text/plain",
            "size": 0.04,
            "url": "/uploads/test_upload_20251010_074330_2a0a6ac571.txt",
            "previewUrl": null,
            "provider": "local",
            "provider_metadata": null,
            "createdAt": "2025-10-09T23:44:18.769Z",
            "updatedAt": "2025-10-09T23:44:18.769Z",
            "publishedAt": "2025-10-09T23:44:18.769Z"
        }
        """;

        // Act - 使用 camelCase 策略反序列化（因為 Strapi 使用 camelCase）
        var camelCaseOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        try
        {
            var result = JsonSerializer.Deserialize<StrapiMediaField>(strapiJson, camelCaseOptions);

            // Assert
            result.ShouldNotBeNull();
            result.Id.ShouldBe(4);
            result.DocumentId.ShouldBe("vuyzqv9x269tp034kjwzpiaz");
            result.Name.ShouldBe("test-upload-20251010-074330.txt");
            result.Hash.ShouldBe("test_upload_20251010_074330_2a0a6ac571");
            result.Ext.ShouldBe(".txt");
            result.Mime.ShouldBe("text/plain");
            result.Size.ShouldBe(0.04m); // 使用 decimal 字面值
            result.Url.ShouldBe("/uploads/test_upload_20251010_074330_2a0a6ac571.txt");
            result.Provider.ShouldBe("local");

            _output.WriteLine("✅ 反序列化成功!");
            _output.WriteLine($"   ID: {result.Id}");
            _output.WriteLine($"   DocumentId: {result.DocumentId}");
            _output.WriteLine($"   Name: {result.Name}");
            _output.WriteLine($"   MimeType: {result.Mime}");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"❌ 反序列化失敗: {ex.Message}");
            throw;
        }
    }

    #region 驗證拿掉 [JsonConverter] 後讀取仍正常

    /// <summary>
    /// 【核心測試】驗證讀取 Strapi 資料並轉換 JSON 時，結果須包含 StrapiMediaField 等完整結構
    ///
    /// 此測試證明：
    /// 1. 從 Strapi API 讀取 JSON → 反序列化為 C# 物件 → StrapiMediaField 等欄位完整保留
    /// 2. 將 C# 物件重新序列化 → JSON 輸出包含完整的 StrapiMediaField 結構
    /// 3. 不需要 [JsonConverter] 屬性也能正確處理讀取
    /// </summary>
    [Fact]
    public void ReadStrapiData_AndConvertToJson_ShouldContainFullStrapiMediaFieldStructure()
    {
        // Arrange - 模擬從 Strapi API 讀取的完整 JSON 回應
        var strapiApiResponse = """
        {
            "id": 1,
            "documentId": "article-doc-123",
            "title": "Test Article",
            "description": "This is a test article",
            "cover": {
                "id": 42,
                "documentId": "media-doc-456",
                "name": "cover.jpg",
                "alternativeText": "Cover image",
                "caption": "Article cover",
                "url": "/uploads/cover.jpg",
                "mime": "image/jpeg",
                "size": 1024.5,
                "width": 800,
                "height": 600
            },
            "author": {
                "id": 10,
                "documentId": "author-doc-789",
                "name": "John Doe",
                "email": "john@example.com"
            }
        }
        """;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        // Act 1 - 從 Strapi API 讀取 JSON 並反序列化為 C# 物件
        var article = JsonSerializer.Deserialize<ArticleWithoutConverterForReadTest>(strapiApiResponse, options);

        // Assert 1 - 驗證反序列化後，StrapiMediaField 的完整結構被保留
        article.ShouldNotBeNull();
        article.Cover.ShouldNotBeNull();
        article.Cover!.Id.ShouldBe(42);
        article.Cover.DocumentId.ShouldBe("media-doc-456");
        article.Cover.Name.ShouldBe("cover.jpg");
        article.Cover.AlternativeText.ShouldBe("Cover image");
        article.Cover.Caption.ShouldBe("Article cover");
        article.Cover.Url.ShouldBe("/uploads/cover.jpg");
        article.Cover.Mime.ShouldBe("image/jpeg");
        article.Cover.Size.ShouldBe(1024.5m);
        article.Cover.Width.ShouldBe(800);
        article.Cover.Height.ShouldBe(600);

        // Act 2 - 將 C# 物件重新序列化為 JSON
        var outputJson = JsonSerializer.Serialize(article, options);

        _output.WriteLine("=== 讀取 Strapi 資料後重新序列化的 JSON ===");
        _output.WriteLine(outputJson);

        // Assert 2 - 驗證 JSON 輸出包含完整的 StrapiMediaField 結構
        outputJson.ShouldContain("\"cover\":");
        outputJson.ShouldContain("\"id\": 42");
        outputJson.ShouldContain("\"documentId\": \"media-doc-456\"");
        outputJson.ShouldContain("\"name\": \"cover.jpg\"");
        outputJson.ShouldContain("\"alternativeText\": \"Cover image\"");
        outputJson.ShouldContain("\"caption\": \"Article cover\"");
        outputJson.ShouldContain("\"url\": \"/uploads/cover.jpg\"");
        outputJson.ShouldContain("\"mime\": \"image/jpeg\"");
        outputJson.ShouldContain("\"width\": 800");
        outputJson.ShouldContain("\"height\": 600");

        _output.WriteLine("\n✅ 讀取 Strapi 資料並轉換 JSON，結果包含 StrapiMediaField 完整結構！");
    }

    /// <summary>
    /// 驗證：從 Strapi API 讀取完整 JSON 物件時，不需要 [JsonConverter] 也能正確反序列化
    /// 這個測試證明 TypeAwareConverter 的改動不會影響讀取功能
    /// </summary>
    [Fact]
    public void Deserialize_WithoutJsonConverterAttribute_ShouldReadFullJsonCorrectly()
    {
        // Arrange - 模擬 Strapi API 回傳的完整 Article JSON（包含巢狀的 Media 和 Relation）
        var strapiResponseJson = """
        {
            "id": 1,
            "documentId": "article-doc-123",
            "title": "Test Article",
            "description": "This is a test article",
            "cover": {
                "id": 42,
                "documentId": "media-doc-456",
                "name": "cover.jpg",
                "alternativeText": "Cover image",
                "url": "/uploads/cover.jpg",
                "mime": "image/jpeg",
                "size": 1024.5
            },
            "author": {
                "id": 10,
                "documentId": "author-doc-789",
                "name": "John Doe",
                "email": "john@example.com"
            },
            "createdAt": "2025-01-01T00:00:00.000Z",
            "updatedAt": "2025-01-02T00:00:00.000Z"
        }
        """;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        // Act - 使用沒有 [JsonConverter] 標註的類別進行反序列化
        var result = JsonSerializer.Deserialize<ArticleWithoutConverterForReadTest>(strapiResponseJson, options);

        // Assert - 驗證所有欄位都能正確讀取
        result.ShouldNotBeNull();

        // 基本欄位
        result.Id.ShouldBe(1);
        result.DocumentId.ShouldBe("article-doc-123");
        result.Title.ShouldBe("Test Article");
        result.Description.ShouldBe("This is a test article");

        // Media 欄位 - 完整物件
        result.Cover.ShouldNotBeNull();
        result.Cover!.Id.ShouldBe(42);
        result.Cover.DocumentId.ShouldBe("media-doc-456");
        result.Cover.Name.ShouldBe("cover.jpg");
        result.Cover.Url.ShouldBe("/uploads/cover.jpg");

        // Relation 欄位 - 完整物件
        result.Author.ShouldNotBeNull();
        result.Author!.Id.ShouldBe(10);
        result.Author.DocumentId.ShouldBe("author-doc-789");
        result.Author.Name.ShouldBe("John Doe");

        _output.WriteLine("✅ 不需要 [JsonConverter] 也能正確讀取完整 JSON 物件！");
    }

    /// <summary>
    /// 驗證：讀取包含 Media 陣列的 JSON 時，不需要 [JsonConverter] 也能正確反序列化
    /// </summary>
    [Fact]
    public void Deserialize_MediaList_WithoutJsonConverterAttribute_ShouldReadCorrectly()
    {
        // Arrange - 模擬包含多個 Media 的 Gallery JSON
        var strapiResponseJson = """
        {
            "title": "Photo Gallery",
            "images": [
                {
                    "id": 1,
                    "documentId": "img-1",
                    "name": "photo1.jpg",
                    "url": "/uploads/photo1.jpg"
                },
                {
                    "id": 2,
                    "documentId": "img-2",
                    "name": "photo2.jpg",
                    "url": "/uploads/photo2.jpg"
                },
                {
                    "id": 3,
                    "documentId": "img-3",
                    "name": "photo3.jpg",
                    "url": "/uploads/photo3.jpg"
                }
            ]
        }
        """;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        // Act
        var result = JsonSerializer.Deserialize<GalleryWithoutConverterForReadTest>(strapiResponseJson, options);

        // Assert
        result.ShouldNotBeNull();
        result.Title.ShouldBe("Photo Gallery");
        result.Images.ShouldNotBeNull();
        result.Images!.Count.ShouldBe(3);

        result.Images[0].Id.ShouldBe(1);
        result.Images[0].Name.ShouldBe("photo1.jpg");
        result.Images[1].Id.ShouldBe(2);
        result.Images[2].Id.ShouldBe(3);

        _output.WriteLine("✅ 不需要 [JsonConverter] 也能正確讀取 Media 陣列！");
    }

    /// <summary>
    /// 驗證：讀取包含 Relation 陣列的 JSON 時，不需要 [JsonConverter] 也能正確反序列化
    /// </summary>
    [Fact]
    public void Deserialize_RelationList_WithoutJsonConverterAttribute_ShouldReadCorrectly()
    {
        // Arrange - 模擬包含多個 Category 的 Post JSON
        var strapiResponseJson = """
        {
            "title": "My Blog Post",
            "categories": [
                {
                    "id": 1,
                    "documentId": "cat-tech",
                    "name": "Technology"
                },
                {
                    "id": 2,
                    "documentId": "cat-dev",
                    "name": "Development"
                }
            ]
        }
        """;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        // Act
        var result = JsonSerializer.Deserialize<PostWithCategoriesForReadTest>(strapiResponseJson, options);

        // Assert
        result.ShouldNotBeNull();
        result.Title.ShouldBe("My Blog Post");
        result.Categories.ShouldNotBeNull();
        result.Categories!.Count.ShouldBe(2);

        result.Categories[0].DocumentId.ShouldBe("cat-tech");
        result.Categories[0].Name.ShouldBe("Technology");
        result.Categories[1].DocumentId.ShouldBe("cat-dev");

        _output.WriteLine("✅ 不需要 [JsonConverter] 也能正確讀取 Relation 陣列！");
    }

    #endregion

    [Fact]
    public void JsonNamingPolicies_Comparison()
    {
        // Arrange
        var testObject = new { DocumentId = "test", AlternativeText = "alt", CreatedAt = DateTime.UtcNow };

        // Act & Assert
        var defaultJson = JsonSerializer.Serialize(testObject);
        _output.WriteLine($"預設策略: {defaultJson}");
        defaultJson.ShouldContain("\"DocumentId\"");

        var camelCaseJson = JsonSerializer.Serialize(testObject, new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
        });
        _output.WriteLine($"CamelCase 策略: {camelCaseJson}");
        camelCaseJson.ShouldContain("\"documentId\"");

        var snakeCaseJson = JsonSerializer.Serialize(testObject, new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower 
        });
        _output.WriteLine($"SnakeCase 策略: {snakeCaseJson}");
        snakeCaseJson.ShouldContain("\"document_id\"");
    }
}

#region 測試用類別 - 沒有 [JsonConverter] 標註

/// <summary>
/// 測試用 Article 類別 - 沒有任何 [JsonConverter] 標註
/// 用於驗證讀取時不需要 JsonConverter 也能正確反序列化
/// </summary>
public class ArticleWithoutConverterForReadTest
{
    public int? Id { get; set; }
    public string? DocumentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // 沒有 [JsonConverter] - 直接使用 StrapiMediaField 類型
    public StrapiMediaField? Cover { get; set; }

    // 沒有 [JsonConverter] - 直接使用 AuthorForReadTest 類型
    public AuthorForReadTest? Author { get; set; }
}

/// <summary>
/// 測試用 Author 類別
/// </summary>
public class AuthorForReadTest
{
    public int? Id { get; set; }
    public string? DocumentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
}

/// <summary>
/// 測試用 Gallery 類別 - 包含 Media 列表
/// </summary>
public class GalleryWithoutConverterForReadTest
{
    public string Title { get; set; } = string.Empty;

    // 沒有 [JsonConverter] - 直接使用 List<StrapiMediaField>
    public List<StrapiMediaField>? Images { get; set; }
}

/// <summary>
/// 測試用 Post 類別 - 包含 Category 列表
/// </summary>
public class PostWithCategoriesForReadTest
{
    public string Title { get; set; } = string.Empty;

    // 沒有 [JsonConverter] - 直接使用 List<CategoryForReadTest>
    public List<CategoryForReadTest>? Categories { get; set; }
}

/// <summary>
/// 測試用 Category 類別
/// </summary>
public class CategoryForReadTest
{
    public int? Id { get; set; }
    public string? DocumentId { get; set; }
    public string Name { get; set; } = string.Empty;
}

#endregion