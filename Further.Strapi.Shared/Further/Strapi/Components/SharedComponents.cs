using Further.Strapi.Serialization;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Further.Strapi.Components;

/// <summary>
/// Strapi 預設 demo 常用組件
/// 這些是 Strapi 啟動時預設包含的基礎組件
/// </summary>

[StrapiComponentName("shared.media")]
public class Media
{
    [JsonConverter(typeof(ConverToId))]
    public StrapiMediaField? File { get; set; }
}

[StrapiComponentName("shared.slider")]
public class Slider
{
    [JsonConverter(typeof(ConverToId))]
    public List<StrapiMediaField>? Files { get; set; }
}

/// <summary>
/// 字串項目組件
/// 常用於選項列表、標籤等場景
/// </summary>
[StrapiComponentName("shared.string-item")]
public class StringItem
{
    public string? Value { get; set; }
}