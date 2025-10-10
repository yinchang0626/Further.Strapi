using System;

namespace Further.Strapi;

/// <summary>
/// 指定 Strapi 組件的名稱標識符
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class StrapiComponentNameAttribute : Attribute
{
    /// <summary>
    /// 組件名稱 (例如: "shared.quote", "shared.rich-text")
    /// </summary>
    public string ComponentName { get; }

    public StrapiComponentNameAttribute(string componentName)
    {
        if (string.IsNullOrWhiteSpace(componentName))
            throw new ArgumentException("Component name cannot be null or empty.", nameof(componentName));

        ComponentName = componentName;
    }
}

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class StrapiCollectionNameAttribute : Attribute
{
    public string CollectionName { get; }

    public StrapiCollectionNameAttribute(string collectionName)
    {
        CollectionName = collectionName;
    }
}

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class StrapiSingleTypeNameAttribute : Attribute
{
    public string TypeName { get; }

    public StrapiSingleTypeNameAttribute(string typeName)
    {
        TypeName = typeName;
    }
}


/// <summary>
/// 標記屬性，指示該屬性不應該被 populate，因為它是內建屬性
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class StrapiIgnorePopulateAttribute : Attribute
{
}
