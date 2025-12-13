using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Volo.Abp.DependencyInjection;

namespace Further.Strapi.Serialization;

/// <summary>
/// 類型識別轉換器介面
/// </summary>
public interface ITypeAwareConverter
{
    /// <summary>
    /// 將物件轉換為寫入格式
    /// - StrapiMediaField → Id (數字)
    /// - 有 [StrapiCollectionName] 的類型 → DocumentId (字串)
    /// - 有 [StrapiSingleTypeName] 的類型 → DocumentId (字串)
    /// - 有 [StrapiComponentName] 的類型 → 加入 __component 欄位
    /// </summary>
    /// <param name="obj">要轉換的物件</param>
    /// <returns>轉換後的字典表示</returns>
    object? ConvertForWrite(object? obj);

    /// <summary>
    /// 檢查類型是否有 [StrapiCollectionName] 屬性
    /// </summary>
    bool HasStrapiCollectionName(Type type);

    /// <summary>
    /// 檢查類型是否有 [StrapiSingleTypeName] 屬性
    /// </summary>
    bool HasStrapiSingleTypeName(Type type);

    /// <summary>
    /// 檢查類型是否有 [StrapiComponentName] 屬性
    /// </summary>
    bool HasStrapiComponentName(Type type);
}

/// <summary>
/// 類型識別轉換器 - 自動識別 Media 和 Relation 欄位並轉換為適當格式
///
/// 核心邏輯：
/// - StrapiMediaField → 轉為 Id (數字)
/// - 有 [StrapiCollectionName] 的類型 → 轉為 DocumentId (字串)
/// - 有 [StrapiSingleTypeName] 的類型 → 轉為 DocumentId (字串)
/// - 有 [StrapiComponentName] 的類型 → 加入 __component 欄位
///
/// 這個類別的目的是讓 Model 定義更簡潔，不需要在每個欄位標註 [JsonConverter]
/// </summary>
public class TypeAwareConverter : ITypeAwareConverter, ITransientDependency
{
    /// <summary>
    /// 將物件轉換為寫入格式
    /// 這是主要入口點，處理「根物件」- 不會將根物件本身轉換為 Id/DocumentId
    /// </summary>
    /// <param name="obj">要轉換的物件</param>
    /// <returns>轉換後的字典表示</returns>
    public object? ConvertForWrite(object? obj)
    {
        if (obj == null)
        {
            return null;
        }

        var type = obj.GetType();

        // 根物件（即使有 [StrapiCollectionName]）應該處理其屬性，不是轉換為 DocumentId
        // 只有當作為「屬性值」時才會轉換
        if (type.IsClass && type != typeof(string) && !IsListType(type))
        {
            return PreprocessObject(obj);
        }

        // 處理基本類型和其他
        return obj;
    }

    /// <summary>
    /// 檢查類型是否有 [StrapiCollectionName] 屬性
    /// </summary>
    public bool HasStrapiCollectionName(Type type)
    {
        return type.GetCustomAttribute<StrapiCollectionNameAttribute>() != null;
    }

    /// <summary>
    /// 檢查類型是否有 [StrapiSingleTypeName] 屬性
    /// </summary>
    public bool HasStrapiSingleTypeName(Type type)
    {
        return type.GetCustomAttribute<StrapiSingleTypeNameAttribute>() != null;
    }

    /// <summary>
    /// 檢查類型是否有 [StrapiComponentName] 屬性
    /// </summary>
    public bool HasStrapiComponentName(Type type)
    {
        return type.GetCustomAttribute<StrapiComponentNameAttribute>() != null;
    }

    #region 私有方法

    /// <summary>
    /// 處理作為屬性值的物件 - 會將 Media/Relation 轉換為 Id/DocumentId
    /// </summary>
    private object? ProcessAsPropertyValue(object? value, Type propertyType)
    {
        if (value == null)
        {
            return null;
        }

        var type = value.GetType();

        // 處理 StrapiMediaField
        if (type == typeof(StrapiMediaField))
        {
            return ConvertMediaToId((StrapiMediaField)value);
        }

        // 處理 List<StrapiMediaField>
        if (IsListOfType<StrapiMediaField>(type))
        {
            return ConvertMediaListToIds((IEnumerable)value);
        }

        // 處理有 [StrapiCollectionName] 的類型 → 轉為 DocumentId
        if (HasStrapiCollectionName(type))
        {
            return ConvertRelationToDocumentId(value);
        }

        // 處理 List<T> where T has [StrapiCollectionName]
        if (IsListOfStrapiCollectionType(type, out _))
        {
            return ConvertRelationListToDocumentIds((IEnumerable)value);
        }

        // 處理有 [StrapiSingleTypeName] 的類型 → 轉為 DocumentId
        if (HasStrapiSingleTypeName(type))
        {
            return ConvertRelationToDocumentId(value);
        }

        // 處理 Component 類型 - 遞迴處理（加入 __component）
        if (HasStrapiComponentName(type))
        {
            return PreprocessObject(value, addComponentName: true);
        }

        // 處理列表（非 Media/Relation）
        if (IsListType(type) && value is IEnumerable enumerable)
        {
            return ProcessList(enumerable);
        }

        // 其他類型直接返回
        return value;
    }

    /// <summary>
    /// 處理列表類型
    /// </summary>
    private object? ProcessList(IEnumerable enumerable)
    {
        var list = new List<object?>();
        foreach (var item in enumerable)
        {
            if (item == null)
            {
                list.Add(null);
                continue;
            }

            var itemType = item.GetType();

            // 檢查列表元素是否為 Component（加入 __component）
            if (HasStrapiComponentName(itemType))
            {
                list.Add(PreprocessObject(item, addComponentName: true));
            }
            else
            {
                list.Add(item);
            }
        }
        return list;
    }

    /// <summary>
    /// 遞迴處理物件的所有屬性
    /// </summary>
    private Dictionary<string, object?> PreprocessObject(object obj, bool addComponentName = false)
    {
        var result = new Dictionary<string, object?>();
        var type = obj.GetType();

        // 如果是 Component 且需要加入 __component，從 Attribute 取得名稱
        if (addComponentName)
        {
            var componentAttr = type.GetCustomAttribute<StrapiComponentNameAttribute>();
            if (componentAttr != null)
            {
                result["__component"] = componentAttr.ComponentName;
            }
        }

        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            if (!property.CanRead)
            {
                continue;
            }

            var value = property.GetValue(obj);
            var propertyType = property.PropertyType;

            // 自動識別並轉換
            var processedValue = ProcessAsPropertyValue(value, propertyType);
            result[GetJsonPropertyName(property)] = processedValue;
        }

        return result;
    }

    #endregion

    #region 類型檢查輔助方法

    /// <summary>
    /// 檢查類型是否為 List&lt;T&gt;
    /// </summary>
    private static bool IsListType(Type type)
    {
        return type.IsGenericType &&
               (type.GetGenericTypeDefinition() == typeof(List<>) ||
                type.GetGenericTypeDefinition() == typeof(IList<>) ||
                type.GetGenericTypeDefinition() == typeof(IEnumerable<>) ||
                type.GetGenericTypeDefinition() == typeof(ICollection<>));
    }

    /// <summary>
    /// 檢查類型是否為 List&lt;T&gt; 且 T 是指定類型
    /// </summary>
    private static bool IsListOfType<T>(Type type)
    {
        if (!type.IsGenericType)
        {
            return false;
        }

        var genericDef = type.GetGenericTypeDefinition();
        if (genericDef != typeof(List<>) &&
            genericDef != typeof(IList<>) &&
            genericDef != typeof(IEnumerable<>) &&
            genericDef != typeof(ICollection<>))
        {
            return false;
        }

        var elementType = type.GetGenericArguments()[0];
        return elementType == typeof(T);
    }

    /// <summary>
    /// 檢查類型是否為 List&lt;T&gt; 且 T 有 [StrapiCollectionName]
    /// </summary>
    private bool IsListOfStrapiCollectionType(Type type, out Type? elementType)
    {
        elementType = null;

        if (!type.IsGenericType)
        {
            return false;
        }

        var genericDef = type.GetGenericTypeDefinition();
        if (genericDef != typeof(List<>) &&
            genericDef != typeof(IList<>) &&
            genericDef != typeof(IEnumerable<>) &&
            genericDef != typeof(ICollection<>))
        {
            return false;
        }

        elementType = type.GetGenericArguments()[0];
        return HasStrapiCollectionName(elementType) || HasStrapiSingleTypeName(elementType);
    }

    /// <summary>
    /// 取得 JSON 屬性名稱（考慮 JsonPropertyName 標註）
    /// </summary>
    private static string GetJsonPropertyName(PropertyInfo property)
    {
        var jsonPropertyName = property.GetCustomAttribute<System.Text.Json.Serialization.JsonPropertyNameAttribute>();
        if (jsonPropertyName != null)
        {
            return jsonPropertyName.Name;
        }

        // 使用 camelCase
        var name = property.Name;
        return char.ToLowerInvariant(name[0]) + name.Substring(1);
    }

    #endregion

    #region 轉換方法

    /// <summary>
    /// 將 StrapiMediaField 轉換為 Id
    /// </summary>
    private static object? ConvertMediaToId(StrapiMediaField media)
    {
        if (media.Id > 0)
        {
            return media.Id;
        }
        return null;
    }

    /// <summary>
    /// 將 List&lt;StrapiMediaField&gt; 轉換為 List&lt;Id&gt;
    /// </summary>
    private static object? ConvertMediaListToIds(IEnumerable mediaList)
    {
        var ids = new List<int>();
        foreach (var item in mediaList)
        {
            if (item is StrapiMediaField media && media.Id > 0)
            {
                ids.Add(media.Id);
            }
        }
        return ids.Count > 0 ? ids : null;
    }

    /// <summary>
    /// 將有 [StrapiCollectionName] 的物件轉換為 DocumentId
    /// </summary>
    private static object? ConvertRelationToDocumentId(object relation)
    {
        var type = relation.GetType();
        var documentIdProp = type.GetProperty("DocumentId");

        if (documentIdProp != null)
        {
            var documentId = documentIdProp.GetValue(relation) as string;
            if (!string.IsNullOrWhiteSpace(documentId))
            {
                return documentId;
            }
        }

        return null;
    }

    /// <summary>
    /// 將 List&lt;T&gt; where T has [StrapiCollectionName] 轉換為 List&lt;DocumentId&gt;
    /// </summary>
    private static object? ConvertRelationListToDocumentIds(IEnumerable relationList)
    {
        var documentIds = new List<string>();
        foreach (var item in relationList)
        {
            if (item == null)
            {
                continue;
            }

            var type = item.GetType();
            var documentIdProp = type.GetProperty("DocumentId");

            if (documentIdProp != null)
            {
                var documentId = documentIdProp.GetValue(item) as string;
                if (!string.IsNullOrWhiteSpace(documentId))
                {
                    documentIds.Add(documentId);
                }
            }
        }
        return documentIds.Count > 0 ? documentIds : null;
    }

    #endregion
}
