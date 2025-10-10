using Further.Strapi.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Volo.Abp.Json.SystemTextJson;
using Volo.Abp.Modularity;
using Volo.Abp.Reflection;

namespace Further.Strapi;

/// <summary>
/// Strapi Shared Module for automatic component polymorphism
/// </summary>
[DependsOn(typeof(AbpJsonSystemTextJsonModule))]
public class StrapiSharedModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {        
        // 配置 ABP JSON 序列化器選項
        context.Services.Configure<AbpSystemTextJsonSerializerOptions>(options =>
        {
            // 將在後續使用 IPostConfigureOptions 來設置 TypeInfoResolver
        });
        
        // 使用 PostConfigure 來確保 DI 容器已完全構建
        context.Services.AddSingleton<IPostConfigureOptions<AbpSystemTextJsonSerializerOptions>, 
            StrapiJsonSerializerOptionsPostConfigureOptions>();
    }
}

/// <summary>
/// 後配置選項類，用於設置 TypeInfoResolver
/// </summary>
public class StrapiJsonSerializerOptionsPostConfigureOptions : IPostConfigureOptions<AbpSystemTextJsonSerializerOptions>
{
    private readonly StrapiPolymorphicTypeResolver _typeResolver;

    public StrapiJsonSerializerOptionsPostConfigureOptions(StrapiPolymorphicTypeResolver typeResolver)
    {
        _typeResolver = typeResolver;
    }

    public void PostConfigure(string? name, AbpSystemTextJsonSerializerOptions options)
    {
        // 設定 TypeInfoResolver 用於組件多型序列化
        options.JsonSerializerOptions.TypeInfoResolver = _typeResolver;
    }
}