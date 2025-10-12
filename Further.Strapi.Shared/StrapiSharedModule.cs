using Volo.Abp.Json.SystemTextJson;
using Volo.Abp.Modularity;

namespace Further.Strapi;

/// <summary>
/// Strapi Shared Module
/// 提供基礎的序列化器和轉換器，不包含高階配置邏輯
/// </summary>
[DependsOn(typeof(AbpJsonSystemTextJsonModule))]
public class StrapiSharedModule : AbpModule
{
    // 基礎共享模組，不需要額外配置
    // 所有高階配置邏輯移至 StrapiModule
}