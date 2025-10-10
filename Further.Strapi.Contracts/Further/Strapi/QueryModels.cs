using System.Collections.Generic;

namespace Further.Strapi;

/// <summary>
/// 篩選操作符
/// </summary>
public enum FilterOperator
{
    Equals,
    NotEquals,
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual,
    Contains,
    ContainsInsensitive,
    NotContains,
    StartsWith,
    EndsWith,
    IsNull,
    NotNull,
    In,
    NotIn,
    Between
}

/// <summary>
/// 排序方向
/// </summary>
public enum SortDirection
{
    Asc,
    Desc
}

/// <summary>
/// 分頁輸入參數
/// </summary>
public class PaginationInput
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

/// <summary>
/// 分頁結果
/// </summary>
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int PageCount { get; set; }
    public int Total { get; set; }
}

/// <summary>
/// 分頁建構器
/// </summary>
public class PaginationBuilder
{
    private int _page = 1;
    private int _pageSize = 25;
    private int? _start;
    private int? _limit;
    private bool _withCount = true;

    /// <summary>
    /// 設定頁碼和每頁大小（標準分頁模式）
    /// </summary>
    public PaginationBuilder Page(int page, int pageSize = 25)
    {
        _page = page;
        _pageSize = pageSize;
        _start = null; // 清除 start/limit 模式
        _limit = null;
        return this;
    }

    /// <summary>
    /// 設定起始位置和限制數量（偏移分頁模式）
    /// </summary>
    public PaginationBuilder StartLimit(int start, int limit)
    {
        _start = start;
        _limit = limit;
        _page = 1; // 清除 page/pageSize 模式
        _pageSize = 25;
        return this;
    }

    /// <summary>
    /// 設定是否包含總數統計（影響效能）
    /// </summary>
    public PaginationBuilder WithCount(bool withCount = true)
    {
        _withCount = withCount;
        return this;
    }

    /// <summary>
    /// 建構分頁查詢字串
    /// </summary>
    public string Build()
    {
        var parts = new List<string>();

        if (_start.HasValue && _limit.HasValue)
        {
            // 使用 start/limit 模式
            parts.Add($"pagination[start]={_start}");
            parts.Add($"pagination[limit]={_limit}");
        }
        else
        {
            // 使用 page/pageSize 模式
            parts.Add($"pagination[page]={_page}");
            parts.Add($"pagination[pageSize]={_pageSize}");
        }

        if (!_withCount)
        {
            parts.Add("pagination[withCount]=false");
        }

        return string.Join("&", parts);
    }

    /// <summary>
    /// 從 PaginationInput 創建 PaginationBuilder
    /// </summary>
    public static PaginationBuilder FromInput(PaginationInput input)
    {
        return new PaginationBuilder().Page(input.Page, input.PageSize);
    }
}