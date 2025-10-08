using System;
using System.Collections.Generic;

namespace Further.Strapi;

public class SortBuilder
{
    private readonly List<string> _sorts = new();
    private int _index = 0;

    public SortBuilder Asc(string field)
    {
        _sorts.Add($"sort[{_index}]={field}:asc");
        _index++;
        return this;
    }

    public SortBuilder Desc(string field)
    {
        _sorts.Add($"sort[{_index}]={field}:desc");
        _index++;
        return this;
    }

    public SortBuilder By(string field, SortDirection direction = SortDirection.Asc)
    {
        var dir = direction == SortDirection.Asc ? "asc" : "desc";
        _sorts.Add($"sort[{_index}]={field}:{dir}");
        _index++;
        return this;
    }

    public string Build()
    {
        return string.Join("&", _sorts);
    }
}

public enum SortDirection
{
    Asc,
    Desc
}