using System;
using System.Collections.Generic;
using System.Text.Json;
using Volo.Abp.Json.SystemTextJson;
using Volo.Abp.Json;
using Microsoft.Extensions.Options;
using Xunit;

namespace Further.Strapi.Tests.Protocol;

/// <summary>
/// StrapiProtocol 路徑建構測試
/// </summary>
public class StrapiProtocolPathsTests
{
    [Fact]
    public void Paths_CollectionType_WithoutDocumentId_ShouldReturnCorrectPath()
    {
        // Arrange
        // (假設有一個測試用的模型)

        // Act
        var path = StrapiProtocol.Paths.CollectionType<TestArticle>();

        // Assert
        Assert.Equal("api/test-articles", path);
    }

    [Fact]
    public void Paths_CollectionType_WithDocumentId_ShouldReturnCorrectPath()
    {
        // Arrange
        var documentId = "abc123def456";

        // Act
        var path = StrapiProtocol.Paths.CollectionType<TestArticle>(documentId);

        // Assert
        Assert.Equal($"api/test-articles/{documentId}", path);
    }

    [Fact]
    public void Paths_CollectionType_WithAttribute_ShouldUseAttributeName()
    {
        // Act
        var path = StrapiProtocol.Paths.CollectionType<TestRestaurant>();

        // Assert
        Assert.Equal("api/restaurants", path);
    }

    [Fact]
    public void Paths_SingleType_ShouldReturnCorrectPath()
    {
        // Act
        var path = StrapiProtocol.Paths.SingleType<TestGlobalSetting>();

        // Assert
        Assert.Equal("api/global-setting", path);
    }

    [Theory]
    [InlineData(null, "api/upload")]
    [InlineData("123", "api/upload/files/123")]
    [InlineData("abc456def", "api/upload/files/abc456def")]
    public void Paths_Media_WithValidInput_ShouldReturnCorrectPath(string fileId, string expectedPath)
    {
        // Act
        var path = StrapiProtocol.Paths.Media(fileId);

        // Assert
        Assert.Equal(expectedPath, path);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Paths_Media_WithEmptyOrWhitespaceFileId_ShouldThrowException(string invalidFileId)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => StrapiProtocol.Paths.Media(invalidFileId));
    }
}

/// <summary>
/// StrapiProtocol 查詢建構測試
/// </summary>
public class StrapiProtocolPopulateTests
{
    [Fact]
    public void Populate_Auto_ShouldGenerateCorrectQuery()
    {
        // Act
        var query = StrapiProtocol.Populate.Auto<TestArticle>();

        // Assert
        Assert.Contains("author", query);
        Assert.Contains("categories", query);
    }

    [Fact]
    public void Populate_All_ShouldReturnAsterisk()
    {
        // Act
        var query = StrapiProtocol.Populate.All();

        // Assert
        Assert.Equal("populate=*", query);
    }

    [Fact]
    public void Populate_Deep_ShouldReturnDoubleAsterisk()
    {
        // Act
        var query = StrapiProtocol.Populate.Deep();

        // Assert
        Assert.Equal("populate=**", query);
    }
}

/// <summary>
/// StrapiProtocol 請求序列化測試
/// </summary>
public class StrapiProtocolRequestTests
{
    private readonly IJsonSerializer _jsonSerializer;

    public StrapiProtocolRequestTests()
    {
        // 建立 ABP 的 JSON 序列化器
        var options = Microsoft.Extensions.Options.Options.Create(new AbpSystemTextJsonSerializerOptions());
        _jsonSerializer = new AbpSystemTextJsonSerializer(options);
    }
}

// 測試用模型
public class TestArticle
{
    public string DocumentId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public TestAuthor Author { get; set; } = new();
    public List<TestCategory> Categories { get; set; } = new();
}

[StrapiCollectionName("restaurants")]
public class TestRestaurant
{
    public string DocumentId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

[StrapiSingleTypeName("global-setting")]
public class TestGlobalSetting
{
    public string DocumentId { get; set; } = string.Empty;
    public string SiteName { get; set; } = string.Empty;
}

public class TestAuthor
{
    public string DocumentId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class TestCategory
{
    public string DocumentId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}