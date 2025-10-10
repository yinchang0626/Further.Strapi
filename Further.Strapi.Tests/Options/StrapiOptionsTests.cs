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

        // Assert - 驗證配置值是否正確讀取（不依賴具體的 token 值）
        options.StrapiUrl.ShouldBe("http://localhost:1337");
        options.StrapiToken.ShouldNotBeNullOrEmpty(); // 只驗證有 token，不驗證具體值
        options.StrapiToken.Length.ShouldBeGreaterThan(50); // 驗證 token 格式合理
    }

    [Fact]
    public void HttpClientFactory_WithBuilderConfiguration_ShouldCreateConfiguredClient()
    {
        // Arrange
        var httpClientFactory = GetRequiredService<IHttpClientFactory>();
        var options = _options.Value;

        // Act
        var strapiClient = httpClientFactory.CreateClient(StrapiOptions.HttpClientName);

        // Assert
        strapiClient.ShouldNotBeNull();
        strapiClient.BaseAddress?.ToString().ShouldBe("http://localhost:1337/");
        strapiClient.DefaultRequestHeaders.Authorization?.Scheme.ShouldBe("Bearer");
        strapiClient.DefaultRequestHeaders.Authorization?.Parameter.ShouldBe(options.StrapiToken); // 使用動態讀取的 token
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