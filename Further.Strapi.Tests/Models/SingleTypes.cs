using Further.Strapi.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Further.Strapi.Tests.Models;

/// <summary>
/// Global 單一類型 - 對應 api::global.global
/// </summary>
[StrapiSingleTypeName("global")]
public class Global
{
    public string DocumentId { get; set; } = string.Empty;

    [Required]
    public string SiteName { get; set; } = string.Empty;

    [Required]
    public string SiteDescription { get; set; } = string.Empty;

    /// <summary>
    /// 網站 Favicon - Media 檔案
    /// </summary>
    public StrapiMediaField? Favicon { get; set; }
    
    /// <summary>
    /// 預設 SEO 設定 - Component
    /// </summary>
    public SharedSeoComponent? DefaultSeo { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    public DateTime? PublishedAt { get; set; }
}

/// <summary>
/// About 單一類型 - 對應 api::about.about
/// </summary>
[StrapiSingleTypeName("about")]
public class About
{
    public string DocumentId { get; set; } = string.Empty;
    
    public string? Title { get; set; }
    
    /// <summary>
    /// Dynamic Zone - 包含多種 Component 類型的集合
    /// 使用 System.Text.Json 的多型支援自動處理反序列化
    /// </summary>
    public List<IStrapiComponent>? Blocks { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
}