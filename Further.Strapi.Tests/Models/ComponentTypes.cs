using Further.Strapi.Serialization;
using Further.Strapi.Tests.Models;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Further.Strapi;

/// <summary>
/// Shared Quote Component - 對應 shared.quote
/// </summary>
[StrapiComponentName("shared.quote")]
public class SharedQuoteComponent : IStrapiComponent
{
    public int? Id { get; set; }
    
    public string Title { get; set; } = string.Empty;
    
    public string Body { get; set; } = string.Empty;
}

/// <summary>
/// Shared Rich Text Component - 對應 shared.rich-text
/// </summary>
[StrapiComponentName("shared.rich-text")]
public class SharedRichTextComponent : IStrapiComponent
{
    public int? Id { get; set; }
    
    public string Body { get; set; } = string.Empty;
}

/// <summary>
/// Shared Media Component - 對應 shared.media
/// </summary>
[StrapiComponentName("shared.media")]
public class SharedMediaComponent : IStrapiComponent
{
    public int? Id { get; set; }

    /// <summary>
    /// Media file (images, files, or videos)
    /// </summary>
    [JsonConverter(typeof(ConverToId))]
    public StrapiMediaField? File { get; set; }
}
/// <summary>
/// Shared SEO Component - 對應 shared.seo
/// </summary>
[StrapiComponentName("shared.seo")]
public class SharedSeoComponent : IStrapiComponent
{
    public int? Id { get; set; }

    /// <summary>
    /// Meta description for SEO (required)
    /// </summary>
    public string MetaDescription { get; set; } = string.Empty;

    /// <summary>
    /// Meta title for SEO (required)
    /// </summary>
    public string MetaTitle { get; set; } = string.Empty;

    /// <summary>
    /// Share image for social media
    /// </summary>
    [JsonConverter(typeof(ConverToId))]
    public StrapiMediaField? ShareImage { get; set; }
}

/// <summary>
/// Shared Slider Component - 對應 shared.slider
/// </summary>
[StrapiComponentName("shared.slider")]
public class SharedSliderComponent : IStrapiComponent
{
    public int? Id { get; set; }

    /// <summary>
    /// Multiple image files
    /// </summary>
    [JsonConverter(typeof(ConverToId))]
    public List<StrapiMediaField> Files { get; set; }
}