using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Further.Strapi;

public class PopulateBuilder
{
    private readonly List<string> _populateItems = new();
    private int _index = 0;

    /// <summary>
    /// 根據型別自動產生 Populate
    /// </summary>
    public string GenerateForType<T>(int maxDepth = 2)
    {
        GeneratePopulateParts(typeof(T), maxDepth);
        return Build();
    }

    /// <summary>
    /// 手動添加欄位
    /// </summary>
    public PopulateBuilder Add(string field)
    {
        _populateItems.Add($"populate[{_index}]={field}");
        _index++;
        return this;
    }

    /// <summary>
    /// 添加巢狀欄位
    /// </summary>
    public PopulateBuilder AddNested(string field, string nestedField)
    {
        _populateItems.Add($"populate[{field}][populate][{_index}]={nestedField}");
        _index++;
        return this;
    }

    /// <summary>
    /// 添加欄位過濾
    /// </summary>
    public PopulateBuilder AddWithFields(string field, params string[] fields)
    {
        for (int i = 0; i < fields.Length; i++)
        {
            _populateItems.Add($"populate[{field}][fields][{i}]={fields[i]}");
        }
        _index++;
        return this;
    }

    /// <summary>
    /// 建構最終查詢字串
    /// </summary>
    public string Build()
    {
        return string.Join("&", _populateItems);
    }

    private void GeneratePopulateParts(Type type, int maxDepth, int currentDepth = 0, string prefix = "", HashSet<Type> visitedTypes = null)
    {
        if (currentDepth >= maxDepth) return;
        
        visitedTypes ??= new HashSet<Type>();
        if (visitedTypes.Contains(type)) return;
        
        visitedTypes.Add(type);

        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            if (!IsNavigationProperty(property.PropertyType)) continue;

            string propertyName = ToCamelCase(property.Name);
            string fullPath = string.IsNullOrEmpty(prefix) ? propertyName : $"{prefix}.{propertyName}";

            Add(fullPath);

            // 遞迴處理巢狀物件
            Type elementType = GetElementType(property.PropertyType);
            GeneratePopulateParts(elementType, maxDepth, currentDepth + 1, fullPath, visitedTypes);
        }

        visitedTypes.Remove(type);
    }

    private bool IsNavigationProperty(Type type)
    {
        if (type.IsPrimitive || type == typeof(string) || type == typeof(Guid) || 
            type == typeof(DateTime) || type.IsEnum || type == typeof(decimal))
            return false;

        var underlyingType = Nullable.GetUnderlyingType(type);
        if (underlyingType != null)
            return IsNavigationProperty(underlyingType);

        return true;
    }

    private Type GetElementType(Type type)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            return type.GetGenericArguments()[0];
        
        if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(ICollection<>) || 
                                   type.GetGenericTypeDefinition() == typeof(List<>)))
            return type.GetGenericArguments()[0];

        return type;
    }

    private string ToCamelCase(string input)
    {
        if (string.IsNullOrEmpty(input) || input.Length < 2)
            return input;

        return char.ToLowerInvariant(input[0]) + input.Substring(1);
    }
}