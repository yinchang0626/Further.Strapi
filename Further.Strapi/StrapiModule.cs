using Further.Strapi.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Text;
using Volo.Abp.Application;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AutoMapper;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace Further.Strapi;

[DependsOn(
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
    }
}
