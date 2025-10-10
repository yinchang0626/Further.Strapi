using Microsoft.Extensions.Options;
using Shouldly;
using System.Net.Http;
using Volo.Abp.Testing;
using Xunit;

namespace Further.Strapi.Tests.Options;

/// <summary>
/// 測試 StrapiOptions 在 Builder 配置下的行為
/// </summary>
public class StrapiOptionsBuilderTests : StrapiIntegrationTestBase
{
    private readonly IOptions<StrapiOptions> _options;

    public StrapiOptionsBuilderTests()
    {
        _options = GetRequiredService<IOptions<StrapiOptions>>();
    }

    [Fact]
    public void StrapiOptions_WithBuilderConfiguration_ShouldUseBuilderValues()
    {
        // Arrange & Act
        var options = _options.Value;

        // Assert - 驗證 builder 設定的值
        options.StrapiUrl.ShouldBe("http://localhost:1337/");
        options.StrapiToken.ShouldBe("bd8cdd66daecf5db8dbdfbeccbbf4e4adba0a38834f72a66700e31b1ad5864051a3ede3c0f028aae7621ef3077cc2226ed6e61e8c68c80f58d53d70d2ac64f8c401ca0c004378a8d62480ea78190eb9c505571c1b538f659a4a06e8c39e5a1c39ede18dfb600b11511b4f61c84b9fbe92e77a90340122a79e98d8ef9915fb5f1");
    }

    [Fact]
    public void HttpClientFactory_WithBuilderConfiguration_ShouldCreateConfiguredClient()
    {
        // Arrange
        var httpClientFactory = GetRequiredService<IHttpClientFactory>();

        // Act
        var strapiClient = httpClientFactory.CreateClient(StrapiOptions.HttpClientName);

        // Assert
        strapiClient.ShouldNotBeNull();
        strapiClient.BaseAddress?.ToString().ShouldBe("http://localhost:1337/");
        strapiClient.DefaultRequestHeaders.Authorization?.Scheme.ShouldBe("Bearer");
        strapiClient.DefaultRequestHeaders.Authorization?.Parameter.ShouldBe("bd8cdd66daecf5db8dbdfbeccbbf4e4adba0a38834f72a66700e31b1ad5864051a3ede3c0f028aae7621ef3077cc2226ed6e61e8c68c80f58d53d70d2ac64f8c401ca0c004378a8d62480ea78190eb9c505571c1b538f659a4a06e8c39e5a1c39ede18dfb600b11511b4f61c84b9fbe92e77a90340122a79e98d8ef9915fb5f1");
    }

    [Fact]
    public void StrapiServices_WithBuilderConfiguration_ShouldBeResolvable()
    {
        // Arrange & Act - 測試所有 Strapi 服務都能正確解析
        var collectionProvider = GetRequiredService<ICollectionTypeProvider<object>>();
        var singleProvider = GetRequiredService<ISingleTypeProvider<object>>();
        var mediaProvider = GetRequiredService<IMediaLibraryProvider>();

        // Assert
        collectionProvider.ShouldNotBeNull();
        singleProvider.ShouldNotBeNull();
        mediaProvider.ShouldNotBeNull();
    }

    [Fact]
    public void StrapiOptions_HttpClientName_ShouldBeConstant()
    {
        // Arrange & Act
        var httpClientName = StrapiOptions.HttpClientName;

        // Assert
        httpClientName.ShouldBe("StrapiClient");
        httpClientName.ShouldNotBeNullOrWhiteSpace();
    }
}