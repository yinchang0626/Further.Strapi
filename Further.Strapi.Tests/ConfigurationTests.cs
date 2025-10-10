using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Further.Strapi.Tests;

/// <summary>
/// 專門測試配置讀取的測試類別
/// </summary>
public class ConfigurationTests : StrapiIntegrationTestBase
{
    private readonly ITestOutputHelper _output;
    private readonly IOptions<StrapiOptions> _options;
    private readonly IConfiguration _configuration;

    public ConfigurationTests(ITestOutputHelper output)
    {
        _output = output;
        _options = GetRequiredService<IOptions<StrapiOptions>>();
        _configuration = GetRequiredService<IConfiguration>();
    }

    [Fact]
    public void Configuration_StrapiSection_ShouldExist()
    {
        // 檢查 Strapi 配置區段是否存在
        var strapiSection = _configuration.GetSection("Strapi");
        strapiSection.Exists().ShouldBeTrue();
        
        var url = strapiSection["StrapiUrl"];
        var token = strapiSection["StrapiToken"];
        
        _output.WriteLine($"Strapi URL: {url}");
        _output.WriteLine($"Strapi Token: {(string.IsNullOrEmpty(token) ? "null" : token.Substring(0, Math.Min(20, token.Length)) + "...")}");
        
        url.ShouldNotBeNullOrEmpty();
        token.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void StrapiOptions_ShouldBeCorrectlyBound()
    {
        // 檢查 StrapiOptions 是否正確綁定
        var options = _options.Value;
        
        _output.WriteLine($"Options URL: {options.StrapiUrl}");
        _output.WriteLine($"Options Token: {(string.IsNullOrEmpty(options.StrapiToken) ? "null" : options.StrapiToken.Substring(0, Math.Min(20, options.StrapiToken.Length)) + "...")}");
        
        options.StrapiUrl.ShouldBe("http://localhost:1337");
        options.StrapiToken.ShouldNotBeNullOrEmpty();
        options.StrapiToken.Length.ShouldBeGreaterThan(50);
    }

    [Fact]
    public void Configuration_AllSources_ShouldBeVisible()
    {
        // 列出所有配置來源，幫助調試
        _output.WriteLine("Configuration Sources:");
        var root = _configuration as IConfigurationRoot;
        if (root != null)
        {
            foreach (var provider in root.Providers)
            {
                _output.WriteLine($"- {provider.GetType().Name}");
            }
        }
        
        // 檢查所有 Strapi 相關的配置值
        _output.WriteLine("\nAll Strapi Configuration Values:");
        foreach (var item in _configuration.AsEnumerable())
        {
            if (item.Key.Contains("Strapi", StringComparison.OrdinalIgnoreCase))
            {
                var value = item.Key.Contains("Token") && !string.IsNullOrEmpty(item.Value) 
                    ? item.Value.Substring(0, Math.Min(20, item.Value.Length)) + "..."
                    : item.Value;
                _output.WriteLine($"  {item.Key} = {value}");
            }
        }
    }
}