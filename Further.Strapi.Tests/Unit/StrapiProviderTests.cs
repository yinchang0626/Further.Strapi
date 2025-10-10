using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Shouldly;
using Further.Strapi.Tests;
using Further.Strapi.Tests.Models;

namespace Further.Strapi.Tests.Unit;

/// <summary>
/// CollectionTypeProvider 和 SingleTypeProvider 單元測試
/// 測試 Provider 類別的構造函數和基本功能
/// </summary>
public class StrapiProviderTests : StrapiIntegrationTestBase
{
    [Fact]
    public void CollectionTypeProvider_Constructor_WithValidDependencies_ShouldNotThrow()
    {
        // Arrange
        var httpClientFactory = GetRequiredService<IHttpClientFactory>();
        var jsonSerializer = GetRequiredService<Volo.Abp.Json.IJsonSerializer>();
        var writeSerializer = GetRequiredService<StrapiWriteSerializer>();

        // Act & Assert
        Should.NotThrow(() => new CollectionTypeProvider<Article>(
            httpClientFactory, 
            jsonSerializer, 
            writeSerializer));
    }

    [Fact]
    public void SingleTypeProvider_Constructor_WithValidDependencies_ShouldNotThrow()
    {
        // Arrange
        var httpClientFactory = GetRequiredService<IHttpClientFactory>();
        var jsonSerializer = GetRequiredService<Volo.Abp.Json.IJsonSerializer>();
        var writeSerializer = GetRequiredService<StrapiWriteSerializer>();

        // Act & Assert
        Should.NotThrow(() => new SingleTypeProvider<Global>(
            httpClientFactory, 
            jsonSerializer, 
            writeSerializer));
    }

    [Fact]
    public void MediaLibraryProvider_Constructor_WithValidDependencies_ShouldNotThrow()
    {
        // Arrange
        var httpClientFactory = GetRequiredService<IHttpClientFactory>();
        var jsonSerializer = GetRequiredService<Volo.Abp.Json.IJsonSerializer>();

        // Act & Assert
        Should.NotThrow(() => new MediaLibraryProvider(httpClientFactory, jsonSerializer));
    }

    [Fact]
    public void StrapiWriteSerializer_Constructor_WithValidDependencies_ShouldNotThrow()
    {
        // Arrange
        var jsonSerializer = GetRequiredService<Volo.Abp.Json.IJsonSerializer>();

        // Act & Assert
        Should.NotThrow(() => new StrapiWriteSerializer(jsonSerializer));
    }

    [Fact]
    public void CollectionTypeProvider_ShouldImplementInterface()
    {
        // Arrange
        var httpClientFactory = GetRequiredService<IHttpClientFactory>();
        var jsonSerializer = GetRequiredService<Volo.Abp.Json.IJsonSerializer>();
        var writeSerializer = GetRequiredService<StrapiWriteSerializer>();

        // Act
        var provider = new CollectionTypeProvider<Article>(httpClientFactory, jsonSerializer, writeSerializer);

        // Assert
        provider.ShouldBeAssignableTo<ICollectionTypeProvider<Article>>();
    }

    [Fact]
    public void SingleTypeProvider_ShouldImplementInterface()
    {
        // Arrange
        var httpClientFactory = GetRequiredService<IHttpClientFactory>();
        var jsonSerializer = GetRequiredService<Volo.Abp.Json.IJsonSerializer>();
        var writeSerializer = GetRequiredService<StrapiWriteSerializer>();

        // Act
        var provider = new SingleTypeProvider<Global>(httpClientFactory, jsonSerializer, writeSerializer);

        // Assert
        provider.ShouldBeAssignableTo<ISingleTypeProvider<Global>>();
    }

    [Fact]
    public void MediaLibraryProvider_ShouldImplementInterface()
    {
        // Arrange
        var httpClientFactory = GetRequiredService<IHttpClientFactory>();
        var jsonSerializer = GetRequiredService<Volo.Abp.Json.IJsonSerializer>();

        // Act
        var provider = new MediaLibraryProvider(httpClientFactory, jsonSerializer);

        // Assert
        provider.ShouldBeAssignableTo<IMediaLibraryProvider>();
    }

    [Fact]
    public void StrapiWriteSerializer_ShouldBeTransientDependency()
    {
        // Arrange
        var jsonSerializer = GetRequiredService<Volo.Abp.Json.IJsonSerializer>();

        // Act
        var serializer = new StrapiWriteSerializer(jsonSerializer);

        // Assert
        serializer.ShouldBeAssignableTo<Volo.Abp.DependencyInjection.ITransientDependency>();
    }

    [Fact]
    public void MediaLibraryProvider_ShouldBeTransientDependency()
    {
        // Arrange
        var httpClientFactory = GetRequiredService<IHttpClientFactory>();
        var jsonSerializer = GetRequiredService<Volo.Abp.Json.IJsonSerializer>();

        // Act
        var provider = new MediaLibraryProvider(httpClientFactory, jsonSerializer);

        // Assert
        provider.ShouldBeAssignableTo<Volo.Abp.DependencyInjection.ITransientDependency>();
    }

    [Fact]
    public void StrapiProtocol_Paths_CollectionType_ShouldGenerateCorrectPath()
    {
        // Act
        var pathWithId = StrapiProtocol.Paths.CollectionType<Article>("doc123");
        var pathWithoutId = StrapiProtocol.Paths.CollectionType<Article>();

        // Assert
        pathWithId.ShouldContain("api/articles/doc123");
        pathWithoutId.ShouldContain("api/articles");
        pathWithoutId.ShouldNotContain("/doc123");
    }

    [Fact]
    public void StrapiProtocol_Paths_SingleType_ShouldGenerateCorrectPath()
    {
        // Act
        var globalPath = StrapiProtocol.Paths.SingleType<Global>();
        var aboutPath = StrapiProtocol.Paths.SingleType<About>();

        // Assert
        globalPath.ShouldContain("api/global");
        aboutPath.ShouldContain("api/about");
    }

    [Fact]
    public void StrapiProtocol_Paths_Media_ShouldGenerateCorrectPath()
    {
        // Act
        var mediaWithId = StrapiProtocol.Paths.Media("123");
        var mediaWithoutId = StrapiProtocol.Paths.Media();

        // Assert
        mediaWithId.ShouldContain("api/upload/files/123");
        mediaWithoutId.ShouldContain("api/upload");
    }

    [Fact]
    public void StrapiProtocol_Populate_Auto_ShouldGeneratePopulateQuery()
    {
        // Act
        var articlePopulate = StrapiProtocol.Populate.Auto<Article>();
        var authorPopulate = StrapiProtocol.Populate.Auto<Author>();

        // Assert
        articlePopulate.ShouldNotBeNullOrEmpty();
        authorPopulate.ShouldNotBeNullOrEmpty();
        
        // Should contain relationship fields
        articlePopulate.ShouldContain("author");
        articlePopulate.ShouldContain("category");
        authorPopulate.ShouldContain("articles");
    }

    [Fact]
    public void StrapiProtocol_Populate_All_ShouldReturnAsterisk()
    {
        // Act
        var result = StrapiProtocol.Populate.All();

        // Assert
        result.ShouldBe("populate=*");
    }

    [Fact]
    public void StrapiProtocol_Populate_Deep_ShouldReturnDeepPopulate()
    {
        // Act
        var result = StrapiProtocol.Populate.Deep();

        // Assert
        result.ShouldBe("populate=**");
    }

    [Fact]
    public void StrapiProtocol_Populate_Manual_ShouldReturnBuilder()
    {
        // Act
        var builder = StrapiProtocol.Populate.Manual();

        // Assert
        builder.ShouldNotBeNull();
        builder.ShouldBeOfType<PopulateBuilder>();
    }

    [Fact]
    public void PopulateBuilder_ShouldBuildCorrectQuery()
    {
        // Act
        var result = StrapiProtocol.Populate.Manual()
            .Add("author")
            .Add("category")
            .Build();

        // Assert
        result.ShouldNotBeNullOrEmpty();
        result.ShouldContain("populate[0]=author");
        result.ShouldContain("populate[1]=category");
    }

    [Fact]
    public void StrapiSingleResponse_ShouldHaveDataProperty()
    {
        // Arrange & Act
        var response = new StrapiSingleResponse<Article>
        {
            Data = new Article { Title = "Test Article" }
        };

        // Assert
        response.Data.ShouldNotBeNull();
        response.Data.Title.ShouldBe("Test Article");
    }

    [Fact]
    public void StrapiCollectionResponse_ShouldHaveDataAndMeta()
    {
        // Arrange & Act
        var response = new StrapiCollectionResponse<Article>
        {
            Data = new[] { new Article { Title = "Test Article" } }.ToList(),
            Meta = new StrapiMeta
            {
                Pagination = new StrapiPagination { Total = 1 }
            }
        };

        // Assert
        response.Data.ShouldNotBeNull();
        response.Data.Count.ShouldBe(1);
        response.Meta.ShouldNotBeNull();
        response.Meta.Pagination.ShouldNotBeNull();
        response.Meta.Pagination.Total.ShouldBe(1);
    }

    [Fact]
    public void StrapiPagination_AllProperties_ShouldBeSettable()
    {
        // Arrange & Act
        var pagination = new StrapiPagination
        {
            Page = 2,
            PageSize = 10,
            PageCount = 5,
            Total = 50
        };

        // Assert
        pagination.Page.ShouldBe(2);
        pagination.PageSize.ShouldBe(10);
        pagination.PageCount.ShouldBe(5);
        pagination.Total.ShouldBe(50);
    }
}