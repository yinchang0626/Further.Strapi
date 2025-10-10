using System;
using System.Collections.Generic;
using Xunit;
using Shouldly;
using Further.Strapi.Tests.Models;

namespace Further.Strapi.Tests.Unit;

/// <summary>
/// 錯誤處理和邊緣情況測試 - 純單元測試，無外部依賴
/// 測試各種異常狀況的處理
/// </summary>
public class ErrorHandlingTests
{
    [Fact]
    public void FilterBuilder_WithNullValue_ShouldHandleGracefully()
    {
        // Arrange
        var builder = new FilterBuilder();

        // Act
        builder.Where("title", FilterOperator.Equals, null);
        var result = builder.Build();

        // Assert
        result.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void PaginationBuilder_WithValidValues_ShouldWork()
    {
        // Arrange
        var builder = new PaginationBuilder();

        // Act & Assert - These should NOT throw
        Should.NotThrow(() => builder.Page(1, 10));
        Should.NotThrow(() => builder.Page(2, 25));
        Should.NotThrow(() => builder.Page(1, 100));
        
        // Build should work
        var result = builder.Build();
        result.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void SortBuilder_WithValidFields_ShouldWork()
    {
        // Arrange
        var builder = new SortBuilder();

        // Act & Assert - These should NOT throw
        Should.NotThrow(() => builder.Ascending("title"));
        Should.NotThrow(() => builder.Descending("createdAt"));
        
        // Build should work
        var result = builder.Build();
        result.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void StrapiProtocol_WithValidTypes_ShouldWork()
    {
        // Act & Assert - These should NOT throw
        Should.NotThrow(() => StrapiProtocol.Paths.CollectionType<Article>());
        Should.NotThrow(() => StrapiProtocol.Paths.SingleType<Global>());
    }

    [Fact]
    public void FilterBuilder_WithMultipleConditions_ShouldWork()
    {
        // Arrange
        var builder = new FilterBuilder();

        // Act
        var result = Should.NotThrow(() => 
            builder.Where("title", FilterOperator.Contains, "test")
                   .And("status", FilterOperator.Equals, "published")
                   .Build());

        // Assert
        result.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void SortBuilder_WithMultipleSorts_ShouldWork()
    {
        // Arrange
        var builder = new SortBuilder();

        // Act
        var result = Should.NotThrow(() => 
            builder.Ascending("title")
                   .Descending("createdAt")
                   .Build());

        // Assert
        result.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void PaginationBuilder_WithDifferentModes_ShouldWork()
    {
        // Arrange
        var builder1 = new PaginationBuilder();
        var builder2 = new PaginationBuilder();

        // Act & Assert
        Should.NotThrow(() => builder1.Page(1, 10).Build());
        Should.NotThrow(() => builder2.StartLimit(0, 20).Build());
        
        // Verify results
        var result1 = builder1.Build();
        var result2 = builder2.Build();
        
        result1.ShouldNotBeNullOrEmpty();
        result2.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void Builders_ShouldReturnValidQueryStrings()
    {
        // Arrange & Act
        var filter = new FilterBuilder().Where("title", FilterOperator.Contains, "test").Build();
        var sort = new SortBuilder().Descending("createdAt").Build();
        var pagination = new PaginationBuilder().Page(1, 10).Build();

        // Assert
        filter.ShouldNotBeNullOrEmpty();
        sort.ShouldNotBeNullOrEmpty();
        pagination.ShouldNotBeNullOrEmpty();
    }
}