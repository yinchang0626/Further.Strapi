using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Further.Strapi;

/// <summary>
/// Strapi Media Library 檔案模型
/// </summary>
public class StrapiMediaFile
{
    public int Id { get; set; }
    public string DocumentId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public double Size { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string AlternativeText { get; set; } = string.Empty;
    public string Caption { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string Ext { get; set; } = string.Empty;
    public string Hash { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
}

/// <summary>
/// 檔案上傳請求模型
/// </summary>
public class FileUploadRequest
{
    public Stream FileStream { get; set; } = null!;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string? AlternativeText { get; set; }
    public string? Caption { get; set; }
    public string? Path { get; set; } // 僅在 AWS S3 等 provider 支援
}

/// <summary>
/// 關聯檔案到內容項目的上傳請求
/// </summary>
public class EntryFileUploadRequest : FileUploadRequest
{
    public string RefId { get; set; } = string.Empty; // 關聯的 entry ID
    public string Ref { get; set; } = string.Empty; // 模型的 uid，例如 "api::restaurant.restaurant"
    public string Field { get; set; } = string.Empty; // 欄位名稱
    public string? Source { get; set; } // 外掛名稱（可選）
}

/// <summary>
/// 檔案資訊更新請求
/// </summary>
public class FileInfoUpdateRequest
{
    public string? AlternativeText { get; set; }
    public string? Caption { get; set; }
    public string? Name { get; set; }
}

/// <summary>
/// Strapi Media Library Provider 接口
/// 用於處理 Strapi 的媒體庫操作（檔案上傳、取得、刪除）
/// </summary>
public interface IMediaLibraryProvider
{
    /// <summary>
    /// 上傳檔案到 Strapi Media Library
    /// </summary>
    /// <param name="fileUpload">檔案上傳請求</param>
    /// <returns>上傳後的檔案資訊</returns>
    Task<StrapiMediaFile> UploadAsync(FileUploadRequest fileUpload);

    /// <summary>
    /// 上傳檔案並關聯到特定的內容項目
    /// </summary>
    /// <param name="entryFileUpload">關聯檔案上傳請求</param>
    /// <returns>上傳後的檔案資訊</returns>
    Task<StrapiMediaFile> UploadEntryFileAsync(EntryFileUploadRequest entryFileUpload);

    /// <summary>
    /// 取得單一檔案資訊
    /// </summary>
    /// <param name="fileId">檔案 ID</param>
    /// <returns>檔案資訊</returns>
    Task<StrapiMediaFile> GetAsync(int fileId);

    /// <summary>
    /// 取得檔案列表
    /// </summary>
    /// <returns>檔案列表</returns>
    Task<List<StrapiMediaFile>> GetListAsync();

    /// <summary>
    /// 更新檔案資訊 (metadata)
    /// </summary>
    /// <param name="fileId">檔案 ID</param>
    /// <param name="updateRequest">更新請求</param>
    /// <returns>更新後的檔案資訊</returns>
    Task<StrapiMediaFile> UpdateFileInfoAsync(int fileId, FileInfoUpdateRequest updateRequest);

    /// <summary>
    /// 刪除檔案
    /// </summary>
    /// <param name="fileId">檔案 ID</param>
    Task DeleteAsync(int fileId);
}