using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Further.Strapi;

/// <summary>
/// Strapi 檔案類型 - 對應 plugin::upload.file (PluginUploadFile)
/// 這是 Strapi 底層的檔案儲存類型
/// 智能序列化：讀取時完整物件，更新時只有 ID
/// </summary>
public class StrapiMediaField
{
    /// <summary>
    /// 檔案 ID
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// 檔案文件 ID (Strapi v5)
    /// </summary>
    public string DocumentId { get; set; } = string.Empty;
    
    /// <summary>
    /// 檔案名稱
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// 替代文字 (用於無障礙)
    /// </summary>
    public string? AlternativeText { get; set; }
    
    /// <summary>
    /// 圖片說明
    /// </summary>
    public string? Caption { get; set; }
    
    /// <summary>
    /// 檔案寬度 (圖片/影片)
    /// </summary>
    public int? Width { get; set; }
    
    /// <summary>
    /// 檔案高度 (圖片/影片)
    /// </summary>
    public int? Height { get; set; }
    
    /// <summary>
    /// 檔案格式資訊 (包含不同尺寸的圖片版本)
    /// 這是內建屬性，不需要 populate
    /// </summary>
    [StrapiIgnorePopulate]
    public StrapiMediaFormats? Formats { get; set; }
    
    /// <summary>
    /// 檔案雜湊值
    /// </summary>
    public string Hash { get; set; } = string.Empty;
    
    /// <summary>
    /// 檔案副檔名
    /// </summary>
    public string? Ext { get; set; }
    
    /// <summary>
    /// MIME 類型
    /// </summary>
    public string Mime { get; set; } = string.Empty;
    
    /// <summary>
    /// 檔案大小 (bytes)
    /// </summary>
    public decimal Size { get; set; }
    
    /// <summary>
    /// 檔案 URL
    /// </summary>
    public string Url { get; set; } = string.Empty;
    
    /// <summary>
    /// 預覽 URL
    /// </summary>
    public string? PreviewUrl { get; set; }
    
    /// <summary>
    /// 儲存提供者
    /// </summary>
    public string Provider { get; set; } = string.Empty;
    
    /// <summary>
    /// 提供者的中繼資料 (Cloudinary, AWS S3 等特定資訊)
    /// 這是內建屬性，不需要 populate
    /// </summary>
    [StrapiIgnorePopulate]
    [JsonPropertyName("provider_metadata")]
    public StrapiProviderMetadata? ProviderMetadata { get; set; }
    
    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// 更新時間
    /// </summary>
    public DateTime UpdatedAt { get; set; }
    
    /// <summary>
    /// 發布時間 (Strapi v5)
    /// </summary>
    public DateTime? PublishedAt { get; set; }
}


/// <summary>
/// Strapi Media 格式資訊
/// 用於表示不同尺寸的圖片版本
/// </summary>
public class StrapiMediaFormat
{
    public string? Name { get; set; }
    public string? Hash { get; set; }
    public string? Ext { get; set; }
    public string? Mime { get; set; }
    public string? Path { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public decimal? Size { get; set; }
    public string? Url { get; set; }
}

/// <summary>
/// Strapi Media 格式集合
/// 包含 thumbnail, small, medium, large 等不同尺寸版本
/// </summary>
public class StrapiMediaFormats
{
    public StrapiMediaFormat? Thumbnail { get; set; }
    public StrapiMediaFormat? Small { get; set; }
    public StrapiMediaFormat? Medium { get; set; }
    public StrapiMediaFormat? Large { get; set; }
}

/// <summary>
/// Strapi Provider Metadata
/// 包含不同儲存提供者的特定中繼資料
/// </summary>
public class StrapiProviderMetadata
{
    /// <summary>
    /// 公開 ID (用於 Cloudinary 等服務)
    /// </summary>
    public string? PublicId { get; set; }

    /// <summary>
    /// 資源類型 (image, video, raw 等)
    /// </summary>
    public string? ResourceType { get; set; }

    /// <summary>
    /// AWS S3 相關資訊
    /// </summary>
    public string? Region { get; set; }
    public string? Bucket { get; set; }
    public string? Key { get; set; }

    public Dictionary<string, object>? AdditionalData { get; set; }
}
