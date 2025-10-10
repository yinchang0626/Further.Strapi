using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace Further.Strapi.Tests.Services;

/// <summary>
/// 取得文章列表的輸入參數
/// </summary>
public class GetArticlesInput : PagedAndSortedResultRequestDto
{
    /// <summary>
    /// 搜尋關鍵字（在標題中搜尋）
    /// </summary>
    public string Search { get; set; }

    /// <summary>
    /// 作者 DocumentId 篩選
    /// </summary>
    public string AuthorDocumentId { get; set; }

    /// <summary>
    /// 分類 DocumentId 篩選
    /// </summary>
    public string CategoryDocumentId { get; set; }

    /// <summary>
    /// 是否只顯示已發布的文章
    /// </summary>
    public bool PublishedOnly { get; set; } = true;

    /// <summary>
    /// 排序欄位
    /// </summary>
    public string SortBy { get; set; } = "publishedAt";

    /// <summary>
    /// 排序方向 (asc/desc)
    /// </summary>
    public string SortDirection { get; set; } = "desc";
}

/// <summary>
/// 創建文章的輸入參數
/// </summary>
public class CreateArticleInput
{
    /// <summary>
    /// 文章標題
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 文章描述
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// URL slug
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// 封面圖片檔案 ID
    /// </summary>
    public int? CoverFileId { get; set; }

    /// <summary>
    /// 作者 DocumentId
    /// </summary>
    public string AuthorDocumentId { get; set; }

    /// <summary>
    /// 分類 DocumentId
    /// </summary>
    public string CategoryDocumentId { get; set; }

    /// <summary>
    /// 動態區塊
    /// </summary>
    public List<IStrapiComponent> Blocks { get; set; }
}

/// <summary>
/// 更新文章的輸入參數
/// </summary>
public class UpdateArticleInput
{
    /// <summary>
    /// 文章標題
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 文章描述
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// URL slug
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// 封面圖片檔案 ID
    /// </summary>
    public int? CoverFileId { get; set; }

    /// <summary>
    /// 作者 DocumentId
    /// </summary>
    public string AuthorDocumentId { get; set; }

    /// <summary>
    /// 分類 DocumentId
    /// </summary>
    public string CategoryDocumentId { get; set; }

    /// <summary>
    /// 動態區塊
    /// </summary>
    public List<IStrapiComponent> Blocks { get; set; }
}