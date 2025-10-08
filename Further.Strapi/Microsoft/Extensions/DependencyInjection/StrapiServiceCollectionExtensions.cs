using Further.Strapi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Strapi 服務配置擴展
/// </summary>
public static class StrapiServiceCollectionExtensions
{
    /// <summary>
    /// 註冊 Strapi 服務
    /// </summary>
    /// <param name="services">服務集合</param>
    /// <param name="configure">配置建構器 (可為 null，表示使用 appsettings.json 設定)</param>
    /// <remarks>
    /// 支援重複調用，後面的配置會覆蓋前面的配置 (符合 ABP 模組拓樸依賴優先順序)
    /// </remarks>
    public static IServiceCollection AddStrapi(this IServiceCollection services, Action<StrapiServiceBuilder>? configure = null)
    {
        var builder = new StrapiServiceBuilder(services);
        configure?.Invoke(builder);
        ApplyHttpClientConfigurations(services, builder.GetHttpClientActions(), builder.GetHttpClientBuilderActions());
        
        return services;
    }

    /// <summary>
    /// 應用 HttpClient 配置 (共用函式)
    /// </summary>
    internal static void ApplyHttpClientConfigurations(
        IServiceCollection services,
        IList<Action<IServiceProvider, HttpClient>> httpClientActions,
        IList<Action<IHttpClientBuilder>> httpClientBuilderActions)
    {
        // 註冊 HttpClient 工廠配置
        var clientBuilder = services.AddHttpClient(StrapiOptions.HttpClientName, (serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<StrapiOptions>>().Value;
            
            // 設定基礎 URL
            client.BaseAddress = new Uri(options.StrapiUrl.TrimEnd('/') + "/");
            
            // 設定認證 Header
            if (!string.IsNullOrEmpty(options.StrapiToken))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.StrapiToken);
            }
            
            // 設定預設 Headers
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // 應用自訂配置
            foreach (var action in httpClientActions)
            {
                action(serviceProvider, client);
            }
        });

        // 應用 HttpClientBuilder 配置
        foreach (var action in httpClientBuilderActions)
        {
            action(clientBuilder);
        }
    }
}

/// <summary>
/// Strapi 服務建構器
/// </summary>
public class StrapiServiceBuilder
{
    public IServiceCollection Services { get; }
    private readonly List<Action<IServiceProvider, HttpClient>> _httpClientActions = new();
    private readonly List<Action<IHttpClientBuilder>> _httpClientBuilderActions = new();

    public StrapiServiceBuilder(IServiceCollection services)
    {
        Services = services;
    }

    /// <summary>
    /// 配置 Strapi 選項
    /// </summary>
    public StrapiServiceBuilder ConfigureOptions(Action<StrapiOptions> configure)
    {
        Services.Configure(configure);
        return this;
    }

    /// <summary>
    /// 配置 HttpClient
    /// </summary>
    public StrapiServiceBuilder ConfigureHttpClient(Action<IServiceProvider, HttpClient> configure)
    {
        _httpClientActions.Add(configure);
        return this;
    }

    /// <summary>
    /// 配置 HttpClientBuilder (用於 Polly 等進階配置)
    /// </summary>
    public StrapiServiceBuilder ConfigureHttpClientBuilder(Action<IHttpClientBuilder> configure)
    {
        _httpClientBuilderActions.Add(configure);
        return this;
    }

    internal IList<Action<IServiceProvider, HttpClient>> GetHttpClientActions() => _httpClientActions;
    internal IList<Action<IHttpClientBuilder>> GetHttpClientBuilderActions() => _httpClientBuilderActions;
}