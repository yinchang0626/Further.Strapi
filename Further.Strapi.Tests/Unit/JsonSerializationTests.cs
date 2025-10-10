using Shouldly;
using System;
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