using System;

namespace Further.Strapi;

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