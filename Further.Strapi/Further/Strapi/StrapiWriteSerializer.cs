using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp.Json;

namespace Further.Strapi;

/// <summary>
/// Strapi 寫入操作序列化器 - 處理新增和更新時的物件簡化
/// 處理系統欄位排除和 Component 的 __component 欄位邏輯
/// 重構版本：提取方法來分離欄位過濾邏輯
/// </summary>
public class StrapiWriteSerializer : Volo.Abp.DependencyInjection.ITransientDependency
{
    private readonly IJsonSerializer _jsonSerializer;
    
    // 常數定義 - 避免魔術字串
    private const string COMPONENT_FIELD = "__component";
    
    /// <summary>
    /// 需要排除的系統欄位名稱
    /// </summary>
    private static readonly HashSet<string> SystemFieldNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "id", "documentId", "createdAt", "updatedAt", "publishedAt"
    };

    public StrapiWriteSerializer(IJsonSerializer jsonSerializer)
    {
        _jsonSerializer = jsonSerializer;
    }

    public string? SerializeForUpdate(object? obj)
    {
        if (obj == null) return null;

        // 使用 ABP 的 JsonSerializer 序列化為 JSON
        var json = _jsonSerializer.Serialize(obj);

        // 解析為 JsonDocument，然後獲取 RootElement
        using var jsonDocument = JsonDocument.Parse(json);
        var jsonElement = jsonDocument.RootElement;

        // 清理並重建 - 處理系統欄位排除和 __component 邏輯
        var cleaned = CleanJsonElement(jsonElement, isInDynamicZone: false);
        return cleaned != null ? _jsonSerializer.Serialize(cleaned) : string.Empty;
    }

    /// <summary>
    /// 清理 JSON 元素 - 遞迴處理不同類型的 JSON 結構
    /// 
    /// 為什麼需要遞迴？
    /// 因為 JSON 可能有巢狀結構：
    /// {
    ///   "title": "文章",
    ///   "author": { "id": 1, "name": "作者" },  // 巢狀物件需要遞迴清理
    ///   "components": [...]                     // 陣列也需要遞迴處理
    /// }
    /// </summary>
    private object? CleanJsonElement(JsonElement element, bool isInDynamicZone = false)
    {
        // 本地函式：檢查陣列是否為 Component 陣列
        static bool IsArrayOfComponents(JsonElement array)
        {
            if (array.ValueKind != JsonValueKind.Array)
                return false;
                
            // 檢查第一個元素是否有 __component
            foreach (var item in array.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.Object && 
                    item.TryGetProperty(COMPONENT_FIELD, out _))
                {
                    return true;
                }
                break; // 只檢查第一個
            }
            return false;
        }

        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                return CleanJsonObject(element, isInDynamicZone);
                
            case JsonValueKind.Array:
                // 檢查是否為 Dynamic Zone (陣列中的物件有 __component)
                bool isDynamicZone = IsArrayOfComponents(element);
                return element.EnumerateArray()
                    .Select(item => CleanJsonElement(item, isInDynamicZone: isDynamicZone))
                    .ToArray();
                    
            case JsonValueKind.String:
                return element.GetString() ?? string.Empty;
                
            case JsonValueKind.Number:
                if (element.TryGetInt32(out var intValue))
                    return intValue;
                if (element.TryGetDecimal(out var decimalValue))
                    return decimalValue;
                return element.GetDouble();
                
            case JsonValueKind.True:
                return true;
                
            case JsonValueKind.False:
                return false;
                
            case JsonValueKind.Null:
                return null;
                
            default:
                return element.ToString();
        }
    }

    /// <summary>
    /// 清理 JSON 物件 - 重構版本，透過方法提取來分離邏輯
    /// 
    /// CleanJsonElement vs CleanJsonObject 的差別：
    /// - CleanJsonElement: 處理任何類型的 JSON 元素 (物件、陣列、字串、數字等)
    /// - CleanJsonObject: 只處理物件類型，專門負責欄位過濾邏輯
    /// </summary>
    private Dictionary<string, object> CleanJsonObject(JsonElement obj, bool isInDynamicZone = false)
    {
        // 本地函式：判斷是否應該跳過某個欄位 - 統一的判斷入口
        static bool ShouldSkipField(string fieldName, bool isComponent, bool isInDynamicZone)
        {
            if (isComponent)
            {
                return ShouldSkipComponentField(fieldName, isInDynamicZone);
            }
            else
            {
                return ShouldSkipRegularField(fieldName);
            }
        }

        // 本地函式：Component 物件的欄位過濾邏輯
        static bool ShouldSkipComponentField(string fieldName, bool isInDynamicZone)
        {
            // Component 的 id 總是要移除
            if (fieldName.Equals("id", StringComparison.OrdinalIgnoreCase))
                return true;
                
            // __component 欄位的處理：在 Dynamic Zone 中保留，否則移除
            if (fieldName.Equals(COMPONENT_FIELD, StringComparison.OrdinalIgnoreCase))
                return !isInDynamicZone;
                
            // 其他欄位保留
            return false;
        }

        // 本地函式：一般物件的欄位過濾邏輯
        static bool ShouldSkipRegularField(string fieldName)
        {
            return SystemFieldNames.Contains(fieldName);
        }

        var result = new Dictionary<string, object>();
        
        // 檢查這個物件是否是 Component
        bool isComponent = obj.TryGetProperty(COMPONENT_FIELD, out _);
        
        foreach (var property in obj.EnumerateObject())
        {
            // 簡化的條件判斷：透過方法提取來分離邏輯
            if (ShouldSkipField(property.Name, isComponent, isInDynamicZone))
                continue;
            
            // 遞迴處理屬性值 - 因為巢狀結構也需要清理
            var cleanedValue = CleanJsonElement(property.Value, isInDynamicZone: false);
            
            if (cleanedValue != null)
            {
                result[property.Name] = cleanedValue;
            }
        }

        return result;
    }
}