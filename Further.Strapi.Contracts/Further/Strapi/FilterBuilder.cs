using System;
using System.Collections.Generic;
using System.Linq;

namespace Further.Strapi;

public class FilterBuilder
{
    private readonly List<string> _filters = new();

    /// <summary>
    /// 使用 FilterOperator 枚舉添加篩選條件
    /// </summary>
    public FilterBuilder Where(string field, FilterOperator op, object? value = null)
    {
        var filter = op switch
        {
            FilterOperator.Equals => $"filters[{field}][$eq]={EncodeValue(value)}",
            FilterOperator.NotEquals => $"filters[{field}][$ne]={EncodeValue(value)}",
            FilterOperator.GreaterThan => $"filters[{field}][$gt]={EncodeValue(value)}",
            FilterOperator.GreaterThanOrEqual => $"filters[{field}][$gte]={EncodeValue(value)}",
            FilterOperator.LessThan => $"filters[{field}][$lt]={EncodeValue(value)}",
            FilterOperator.LessThanOrEqual => $"filters[{field}][$lte]={EncodeValue(value)}",
            FilterOperator.Contains => $"filters[{field}][$contains]={EncodeValue(value)}",
            FilterOperator.ContainsInsensitive => $"filters[{field}][$containsi]={EncodeValue(value)}",
            FilterOperator.NotContains => $"filters[{field}][$notContains]={EncodeValue(value)}",
            FilterOperator.StartsWith => $"filters[{field}][$startsWith]={EncodeValue(value)}",
            FilterOperator.EndsWith => $"filters[{field}][$endsWith]={EncodeValue(value)}",
            FilterOperator.IsNull => $"filters[{field}][$null]=true",
            FilterOperator.NotNull => $"filters[{field}][$notNull]=true",
            _ => throw new ArgumentException($"不支援的操作符: {op}")
        };
        
        _filters.Add(filter);
        return this;
    }

    /// <summary>
    /// 使用 And 連接器添加篩選條件
    /// </summary>
    public FilterBuilder And(string field, FilterOperator op, object? value = null)
    {
        return Where(field, op, value);
    }

    // 基本比較操作
    public FilterBuilder Equal(string field, object value)
    {
        _filters.Add($"filters[{field}][$eq]={EncodeValue(value)}");
        return this;
    }

    public FilterBuilder NotEqual(string field, object value)
    {
        _filters.Add($"filters[{field}][$ne]={EncodeValue(value)}");
        return this;
    }

    public FilterBuilder GreaterThan(string field, object value)
    {
        _filters.Add($"filters[{field}][$gt]={EncodeValue(value)}");
        return this;
    }

    public FilterBuilder GreaterThanOrEqual(string field, object value)
    {
        _filters.Add($"filters[{field}][$gte]={EncodeValue(value)}");
        return this;
    }

    public FilterBuilder LessThan(string field, object value)
    {
        _filters.Add($"filters[{field}][$lt]={EncodeValue(value)}");
        return this;
    }

    public FilterBuilder LessThanOrEqual(string field, object value)
    {
        _filters.Add($"filters[{field}][$lte]={EncodeValue(value)}");
        return this;
    }

    // 字串操作
    public FilterBuilder Contains(string field, string value)
    {
        _filters.Add($"filters[{field}][$contains]={EncodeValue(value)}");
        return this;
    }

    public FilterBuilder NotContains(string field, string value)
    {
        _filters.Add($"filters[{field}][$notContains]={EncodeValue(value)}");
        return this;
    }

    public FilterBuilder StartsWith(string field, string value)
    {
        _filters.Add($"filters[{field}][$startsWith]={EncodeValue(value)}");
        return this;
    }

    public FilterBuilder EndsWith(string field, string value)
    {
        _filters.Add($"filters[{field}][$endsWith]={EncodeValue(value)}");
        return this;
    }

    // 空值檢查
    public FilterBuilder IsNull(string field)
    {
        _filters.Add($"filters[{field}][$null]=true");
        return this;
    }

    public FilterBuilder IsNotNull(string field)
    {
        _filters.Add($"filters[{field}][$notNull]=true");
        return this;
    }

    // 陣列操作
    public FilterBuilder In(string field, params object[] values)
    {
        for (int i = 0; i < values.Length; i++)
        {
            _filters.Add($"filters[{field}][$in][{i}]={EncodeValue(values[i])}");
        }
        return this;
    }

    public FilterBuilder NotIn(string field, params object[] values)
    {
        for (int i = 0; i < values.Length; i++)
        {
            _filters.Add($"filters[{field}][$notIn][{i}]={EncodeValue(values[i])}");
        }
        return this;
    }

    // 邏輯操作
    public FilterBuilder AndNested(Action<FilterBuilder> nestedFilter)
    {
        var nested = new FilterBuilder();
        nestedFilter(nested);
        var nestedFilters = nested._filters;
        
        for (int i = 0; i < nestedFilters.Count; i++)
        {
            _filters.Add($"filters[$and][{i}][{nestedFilters[i].Replace("filters[", "")}");
        }
        return this;
    }

    public FilterBuilder Or(Action<FilterBuilder> nestedFilter)
    {
        var nested = new FilterBuilder();
        nestedFilter(nested);
        var nestedFilters = nested._filters;
        
        for (int i = 0; i < nestedFilters.Count; i++)
        {
            _filters.Add($"filters[$or][{i}][{nestedFilters[i].Replace("filters[", "")}");
        }
        return this;
    }

    public string Build()
    {
        return string.Join("&", _filters);
    }

    private string EncodeValue(object? value)
    {
        if (value == null) return "";
        return Uri.EscapeDataString(value.ToString()!);
    }
}