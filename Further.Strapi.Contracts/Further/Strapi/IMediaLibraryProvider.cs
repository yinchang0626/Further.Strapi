using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Further.Strapi;

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

// EntryFileUploadRequest 已移除
// 應用層請分兩步驟：1. 上傳檔案 2. 更新實體關聯

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
    Task<StrapiMediaField> UploadAsync(FileUploadRequest fileUpload);

    /// <summary>
    /// 批次上傳多個檔案到 Strapi Media Library
    /// </summary>
    /// <param name="fileUploads">檔案上傳請求列表</param>
    /// <returns>上傳後的檔案資訊列表</returns>
    Task<List<StrapiMediaField>> UploadMultipleAsync(IEnumerable<FileUploadRequest> fileUploads);

    /// <summary>
    /// 取得單一檔案資訊
    /// </summary>
    /// <param name="fileId">檔案 ID</param>
    /// <returns>檔案資訊</returns>
    Task<StrapiMediaField> GetAsync(int fileId);

    /// <summary>
    /// 取得檔案列表
    /// </summary>
    /// <returns>檔案列表</returns>
    Task<List<StrapiMediaField>> GetListAsync();

    /// <summary>
    /// 更新檔案資訊 (metadata)
    /// </summary>
    /// <param name="fileId">檔案 ID</param>
    /// <param name="updateRequest">更新請求</param>
    /// <returns>更新後的檔案資訊</returns>
    Task<StrapiMediaField> UpdateFileInfoAsync(int fileId, FileInfoUpdateRequest updateRequest);

    /// <summary>
    /// 刪除檔案
    /// </summary>
    /// <param name="fileId">檔案 ID</param>
    Task DeleteAsync(int fileId);
}