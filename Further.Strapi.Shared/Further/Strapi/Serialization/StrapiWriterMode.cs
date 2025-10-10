using System;

namespace Further.Strapi.Serialization;

/// <summary>
/// 指定 Collection Type 屬性的序列化方式
/// </summary>
public enum StrapiWriterMode
{
    /// <summary>
    /// 序列化為 Id (數字)
    /// </summary>
    Id,
    
    /// <summary>
    /// 序列化為 DocumentId (字串)
    /// </summary>
    DocumentId,
    
    /// <summary>
    /// 序列化完整物件
    /// </summary>
    FullObject
}