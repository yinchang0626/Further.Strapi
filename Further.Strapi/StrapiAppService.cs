using Further.Strapi.Localization;
using Volo.Abp.Application.Services;

namespace Further.Strapi;

public abstract class StrapiAppService : ApplicationService
{
    protected StrapiAppService()
    {
        LocalizationResource = typeof(StrapiResource);
        ObjectMapperContext = typeof(StrapiModule);
    }
}
