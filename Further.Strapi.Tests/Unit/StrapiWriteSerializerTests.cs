using System;
using System.Threading.Tasks;
using Xunit;
using Shouldly;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Text;
using Volo.Abp.Json;
using Further.Strapi.Tests.Models;

namespace Further.Strapi.Tests.Unit;

/// <summary>
/// StrapiWriteSerializer 單元測試
/// 測試序列化器的各種序列化模式和邊緣情況
/// </summary>
public class StrapiWriteSerializerTests : StrapiIntegrationTestBase
{
    private readonly StrapiWriteSerializer _serializer;
    private readonly IJsonSerializer _jsonSerializer;

    public StrapiWriteSerializerTests()
    {
        _serializer = GetRequiredService<StrapiWriteSerializer>();
        _jsonSerializer = GetRequiredService<IJsonSerializer>();
    }

    [Fact]
    public void SerializeForUpdate_WithSimpleObject_ShouldReturnCorrectFormat()
    {
        // Arrange
        var data = new { Title = "Test", Description = "Test Description" };

        // Act
        var result = _serializer.SerializeForUpdate(data);

        // Assert
        result.ShouldNotBeNullOrEmpty();
        result.ShouldContain("Test");
    }

    [Fact]
    public void SerializeForUpdate_WithNullObject_ShouldReturnNull()
    {
        // Act
        var result = _serializer.SerializeForUpdate(null);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void SerializeForUpdate_WithSystemFields_ShouldExcludeSystemFields()
    {
        // Arrange
        var data = new 
        {
            Id = 123,
            DocumentId = "doc-123",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            PublishedAt = DateTime.Now,
            Title = "Test Article",
            Description = "Test Description"
        };

        // Act
        var result = _serializer.SerializeForUpdate(data);

        // Assert
        result.ShouldNotBeNullOrEmpty();
        result.ShouldContain("Test Article");
        result.ShouldNotContain("\"Id\"");
        result.ShouldNotContain("\"DocumentId\"");
        result.ShouldNotContain("\"CreatedAt\"");
        result.ShouldNotContain("\"UpdatedAt\"");
        result.ShouldNotContain("\"PublishedAt\"");
    }

    [Fact]
    public void SerializeForUpdate_WithComplexObject_ShouldHandleNestedProperties()
    {
        // Arrange
        var article = new Article
        {
            Id = 123,
            DocumentId = "article-123",
            Title = "Complex Test Article",
            Description = "Test Description",
            Author = new Author 
            { 
                DocumentId = "author-123",
                Name = "Test Author"
            }
        };

        // Act
        var result = _serializer.SerializeForUpdate(article);

        // Assert
        result.ShouldNotBeNullOrEmpty();
        result.ShouldContain("Complex Test Article");
        result.ShouldContain("author-123"); // Author should be serialized as DocumentId
        result.ShouldNotContain("\"Id\"");
        result.ShouldNotContain("\"DocumentId\"");
    }

    [Fact]
    public void SerializeForUpdate_WithEmptyObject_ShouldReturnEmptyStructure()
    {
        // Arrange
        var data = new { };

        // Act
        var result = _serializer.SerializeForUpdate(data);

        // Assert
        result.ShouldNotBeNullOrEmpty();
        result.ShouldBe("{}");
    }
}