using Further.Strapi.Localization;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Application;
using Volo.Abp.Authorization;
using Volo.Abp.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Modularity;
using Volo.Abp.Validation;
using Volo.Abp.Validation.Localization;
using Volo.Abp.VirtualFileSystem;

namespace Further.Strapi;

[DependsOn(
    typeof(AbpDddApplicationContractsModule),
    typeof(AbpValidationModule),
    typeof(AbpAuthorizationModule)
)]
[DependsOn(typeof(Further.Strapi.StrapiSharedModule))]
public class StrapiContractsModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();

        // 註冊 Strapi 配置選項
        context.Services.Configure<StrapiOptions>(configuration.GetSection("Strapi"));

        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<StrapiContractsModule>("Further.Strapi");
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<StrapiResource>("en")
                .AddBaseTypes(typeof(AbpValidationResource))
                .AddVirtualJson("/Localization/Strapi");
        });

        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("Strapi", typeof(StrapiResource));
        });
    }
}
