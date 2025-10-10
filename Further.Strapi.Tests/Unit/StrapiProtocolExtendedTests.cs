using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Shouldly;
using Further.Strapi.Tests.Models;

namespace Further.Strapi.Tests.Unit;

/// <summary>
/// StrapiProtocol 單元測試
/// 測試路徑建構、查詢字串生成等邏輯
/// </summary>
public class StrapiProtocolExtendedTests
{
    [Fact]
    public void Paths_CollectionType_WithoutDocumentId_ShouldGenerateCorrectPath()
    {
        // Act
        var result = StrapiProtocol.Paths.CollectionType<Article>();

        // Assert
        result.ShouldStartWith("api/articles");
    }

    [Fact]
    public void Paths_CollectionType_WithDocumentId_ShouldGenerateCorrectPath()
    {
        // Act
        var result = StrapiProtocol.Paths.CollectionType<Article>("doc-123");

        // Assert
        result.ShouldStartWith("api/articles");
        result.ShouldContain("doc-123");
    }

    [Fact]
    public void Paths_SingleType_ShouldGenerateCorrectPath()
    {
        // Act
        var result = StrapiProtocol.Paths.SingleType<Global>();

        // Assert
        result.ShouldStartWith("api/global");
    }

    [Fact]
    public void Populate_Auto_WithSimpleModel_ShouldGenerateBasicPopulate()
    {
        // Act
        var result = StrapiProtocol.Populate.Auto<Author>();

        // Assert
        result.ShouldNotBeNullOrEmpty();
        // Should include articles relationship
        result.ShouldContain("articles");
    }

    [Fact]
    public void Populate_Auto_WithComplexModel_ShouldHandleNestedRelations()
    {
        // Act
        var result = StrapiProtocol.Populate.Auto<Article>();

        // Assert
        result.ShouldNotBeNullOrEmpty();
        // Should include author, category, cover relationships
        result.ShouldContain("author");
        result.ShouldContain("category");
        result.ShouldContain("cover");
    }

    [Fact]
    public void FilterBuilder_WithBasicConditions_ShouldGenerateCorrectFilter()
    {
        // Arrange
        var builder = new FilterBuilder();
        
        // Act
        builder.Where("title", FilterOperator.Contains, "test")
               .And("publishedAt", FilterOperator.NotNull);

        var result = builder.Build();

        // Assert
        result.ShouldNotBeNullOrEmpty();
        result.ShouldContain("filters");
    }

    [Fact]
    public void SortBuilder_WithMultipleFields_ShouldGenerateCorrectSort()
    {
        // Arrange
        var builder = new SortBuilder();
        
        // Act
        builder.Ascending("createdAt")
               .Descending("title");

        var result = builder.Build();

        // Assert
        result.ShouldNotBeNullOrEmpty();
        result.ShouldContain("sort");
    }

    [Fact]
    public void PaginationBuilder_WithPageAndSize_ShouldGenerateCorrectPagination()
    {
        // Arrange
        var builder = new PaginationBuilder();
        
        // Act
        builder.Page(1, 10);

        var result = builder.Build();

        // Assert
        result.ShouldNotBeNullOrEmpty();
        result.ShouldContain("pagination");
    }
}