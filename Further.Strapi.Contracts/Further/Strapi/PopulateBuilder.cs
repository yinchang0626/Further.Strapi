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
    /// 需要忽略的基本類型和值類型，這些類型不應該被視為導航屬性
    /// 
    /// 重要說明：
    /// - TimeOnly 和 DateOnly 是 .NET 6+ 引入的新時間類型
    /// - 在 Strapi 中，它們對應簡單的字符串值，不是關聯對象
    /// - 如果不將它們加入忽略清單，會被錯誤地當作導航屬性處理
    /// - 這會導致生成無效的 populate 查詢字符串
    /// 
    /// 包含所有原始類型、常用值類型和字串類型
    /// </summary>
    private static readonly HashSet<Type> IgnoredTypes = new()
    {
        // 字串類型
        typeof(string),
        
        // GUID 類型
        typeof(Guid),
        
        // 日期時間類型 - 重要：包含 .NET 6+ 的新類型
        typeof(DateTime),
        typeof(DateOnly),    // .NET 6+ 日期類型，在 Strapi 中為簡單字符串
        typeof(TimeOnly),    // .NET 6+ 時間類型，在 Strapi 中為簡單字符串
        typeof(DateTimeOffset),
        typeof(TimeSpan),
        
        // 數值類型 (除了原始類型之外的)
        typeof(decimal),
        
        // 其他常用值類型
        typeof(Uri),
        typeof(Version)
    };

    /// <summary>
    /// 根據型別自動產生 Populate
    /// </summary>
    public string GenerateForType<T>(int maxDepth = 5)
    {
        GeneratePopulateParts(typeof(T), maxDepth);
        return Build();
    }

    /// <summary>
    /// 自動填充指定類型的所有導航屬性
    /// </summary>
    public PopulateBuilder Auto<T>(int maxDepth = 5)
    {
        GeneratePopulateParts(typeof(T), maxDepth);
        return this;
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
    /// 手動添加欄位（Include 的別名方法）
    /// </summary>
    public PopulateBuilder Include(string field)
    {
        return Add(field);
    }

    /// <summary>
    /// 添加帶有巢狀填充的欄位
    /// </summary>
    public PopulateBuilder Include(string field, Action<PopulateBuilder> nestedPopulate)
    {
        Add(field);
        
        // 處理巢狀填充
        var nested = new PopulateBuilder();
        nestedPopulate(nested);
        
        // 將巢狀填充添加為子項目
        foreach (var nestedItem in nested._populateItems)
        {
            _populateItems.Add($"populate[{field}][{nestedItem}]");
        }
        
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

    private void GeneratePopulateParts(Type type, int maxDepth, int currentDepth = 0, string prefix = "", HashSet<Type>? visitedTypes = null)
    {
        if (currentDepth >= maxDepth) return;
        
        visitedTypes ??= new HashSet<Type>();
        if (visitedTypes.Contains(type)) return;
        
        visitedTypes.Add(type);

        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            // 檢查是否有 StrapiIgnorePopulateAttribute
            if (property.GetCustomAttribute<StrapiIgnorePopulateAttribute>() != null)
                continue;
                
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

    /// <summary>
    /// 判斷指定的類型是否為導航屬性
    /// 
    /// 核心邏輯說明：
    /// 1. 原始類型（int, bool, char 等）不是導航屬性
    /// 2. 枚舉類型不是導航屬性
    /// 3. IgnoredTypes 清單中的類型不是導航屬性
    /// 4. 可空版本的上述類型也不是導航屬性
    /// 5. 其他複雜對象類型才被視為導航屬性
    /// 
    /// 修復歷史：
    /// - 原本未正確處理 TimeOnly/DateOnly，導致它們被誤判為導航屬性
    /// - 透過集中式的 IgnoredTypes 管理，確保類型檢查的一致性
    /// </summary>
    /// <param name="type">要檢查的類型</param>
    /// <returns>
    /// 如果類型應該被視為導航屬性（需要 populate）則返回 true，否則返回 false
    /// </returns>
    /// <remarks>
    /// 以下類型不會被視為導航屬性：
    /// - 原始類型 (int, bool, char 等)
    /// - 枚舉類型
    /// - IgnoredTypes 清單中的類型 (string, Guid, DateTime, TimeOnly, DateOnly 等)
    /// - 上述類型的可空版本
    /// </remarks>
    private bool IsNavigationProperty(Type type)
    {
        // 檢查是否為原始類型 (int, bool, char, double, float 等)
        if (type.IsPrimitive)
            return false;

        // 檢查是否為枚舉類型
        if (type.IsEnum)
            return false;

        // 檢查是否在忽略類型清單中
        if (IgnoredTypes.Contains(type))
            return false;

        // 處理可空類型 (如 int?, DateTime?, Guid? 等)
        var underlyingType = Nullable.GetUnderlyingType(type);
        if (underlyingType != null)
            return IsNavigationProperty(underlyingType);

        // 其他類型被視為導航屬性（需要 populate）
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