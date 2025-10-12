using Further.Strapi.Data;
using Further.Strapi.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Text;
using System.Text.Json.Serialization.Metadata;
using Volo.Abp.Application;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AutoMapper;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Json.SystemTextJson;
using Volo.Abp.Modularity;

namespace Further.Strapi;

[DependsOn(
    typeof(StrapiSharedModule),
    typeof(StrapiContractsModule),
    typeof(AbpDddApplicationModule),
    typeof(AbpAutoMapperModule),
    typeof(AbpAspNetCoreMvcModule),
    typeof(AbpEntityFrameworkCoreModule)
)]
public class StrapiModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        PreConfigure<IMvcBuilder>(mvcBuilder =>
        {
            mvcBuilder.AddApplicationPartIfNotExists(typeof(StrapiModule).Assembly);
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAutoMapperObjectMapper<StrapiModule>();
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<StrapiModule>(validate: true);
        });

        context.Services.AddAbpDbContext<StrapiDbContext>(options =>
        {
            options.AddDefaultRepositories<IStrapiDbContext>(includeAllEntities: true);

            /* Add custom repositories here. Example:
             * options.AddRepository<Question, EfCoreQuestionRepository>();
             */
        });
        context.Services.AddStrapi();
        
        // 註冊 Provider 服務
        context.Services.AddTransient(typeof(ICollectionTypeProvider<>), typeof(CollectionTypeProvider<>));
        context.Services.AddTransient(typeof(ISingleTypeProvider<>), typeof(SingleTypeProvider<>));
        
        // 配置 JSON 序列化選項（高階配置）
        context.Services.AddSingleton<IPostConfigureOptions<AbpSystemTextJsonSerializerOptions>, 
            StrapiJsonSerializerOptionsPostConfigureOptions>();
    }
}

/// <summary>
/// Strapi JSON 序列化選項後配置
/// 負責配置 TypeInfoResolver 以支援多型序列化
/// </summary>
public class StrapiJsonSerializerOptionsPostConfigureOptions : IPostConfigureOptions<AbpSystemTextJsonSerializerOptions>
{
    private readonly StrapiPolymorphicTypeResolver _polymorphicTypeResolver;

    public StrapiJsonSerializerOptionsPostConfigureOptions(
        StrapiPolymorphicTypeResolver polymorphicTypeResolver)
    {
        _polymorphicTypeResolver = polymorphicTypeResolver;
    }

    public void PostConfigure(string? name, AbpSystemTextJsonSerializerOptions options)
    {
        // 配置多型序列化解析器
        // 所有 JsonConverter 都採用手動標註方式，保持設計一致性
        options.JsonSerializerOptions.TypeInfoResolver = _polymorphicTypeResolver;
    }
}
