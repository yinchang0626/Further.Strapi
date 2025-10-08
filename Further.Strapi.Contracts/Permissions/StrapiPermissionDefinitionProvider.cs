using Further.Strapi.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace Further.Strapi.Permissions;

public class StrapiPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(StrapiPermissions.GroupName, L("Permission:Strapi"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<StrapiResource>(name);
    }
}
