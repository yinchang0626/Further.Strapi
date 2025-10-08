using Volo.Abp.Reflection;

namespace Further.Strapi.Permissions;

public class StrapiPermissions
{
    public const string GroupName = "Strapi";

    public static string[] GetAll()
    {
        return ReflectionHelper.GetPublicConstantsRecursively(typeof(StrapiPermissions));
    }
}
