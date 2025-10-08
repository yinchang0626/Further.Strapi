using System;
using System.Collections.Generic;
using System.Linq;

namespace Further.Strapi;

public class FilterBuilder
{
    private readonly List<string> _filters = new();

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
    public FilterBuilder And(Action<FilterBuilder> nestedFilter)
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

    private string EncodeValue(object value)
    {
        if (value == null) return "";
        return Uri.EscapeDataString(value.ToString());
    }
}