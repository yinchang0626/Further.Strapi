using Further.Strapi.Tests.Models;
using System.Collections.Generic;
using Volo.Abp.Json;
using Xunit;

namespace Further.Strapi.Tests.JsonConverters;

/// <summary>
/// 測試全域 JSON 轉換器設定
/// 驗證 StrapiMediaFile 的智能轉換器在 ABP 框架中正常運作
/// </summary>
public class GlobalJsonConverterTests : StrapiIntegrationTestBase
{
    private readonly IJsonSerializer _jsonSerializer;

    public GlobalJsonConverterTests()
    {
        _jsonSerializer = GetRequiredService<IJsonSerializer>();
    }

    [Fact]
    public void MediaComponent_SerializeWithAbpJsonSerializer_ShouldUseSmartConverter()
    {
        // Arrange
        var component = new SharedMediaComponent
        {
            Id = 1,
            File = new StrapiMediaField
            {
                Id = 123,
                DocumentId = "doc123",
                Name = "test.jpg",
                Url = "http://example.com/test.jpg",
                Mime = "image/jpeg"
            }
        };

        // Act
        var json = _jsonSerializer.Serialize(component);

        // Assert
        Assert.Contains("\"id\":1", json);
        Assert.Contains("\"file\":123", json);
    }

    [Fact]
    public void SliderComponent_SerializeMultipleFiles_ShouldUseSmartArrayConverter()
    {
        // Arrange
        var component = new SharedSliderComponent
        {
            Id = 1,
            Files = new List<StrapiMediaField>
            {
                new StrapiMediaField { Id = 123, DocumentId = "doc123" },
                new StrapiMediaField { Id = 456, DocumentId = "doc456" }
            }
        };

        // Act
        var json = _jsonSerializer.Serialize(component);

        // Assert
        Assert.Contains("\"id\":1", json);
        Assert.Contains("\"files\":[123,456]", json);
    }

    [Fact]
    public void MediaFile_DeserializeFullObject_ShouldCreateCompleteObject()
    {
        // Arrange
        var json = """
        {
            "id": 123,
            "documentId": "doc123",
            "name": "test-image.jpg",
            "url": "http://example.com/test.jpg",
            "mime": "image/jpeg",
            "size": 1024.5,
            "width": 800,
            "height": 600
        }
        """;

        // Act
        var result = _jsonSerializer.Deserialize<StrapiMediaField>(json);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(123, result.Id);
        Assert.Equal("doc123", result.DocumentId);
        Assert.Equal("test-image.jpg", result.Name);
        Assert.Equal("http://example.com/test.jpg", result.Url);
        Assert.Equal("image/jpeg", result.Mime);
        Assert.Equal(1024.5m, result.Size);
        Assert.Equal(800, result.Width);
        Assert.Equal(600, result.Height);
    }
}