namespace Further.Strapi;

public static class StrapiDbProperties
{
    public static string DbTablePrefix { get; set; } = "Strapi";

    public static string? DbSchema { get; set; } = null;

    public const string ConnectionStringName = "Strapi";
}
