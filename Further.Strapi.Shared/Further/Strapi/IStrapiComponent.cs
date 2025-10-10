using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Further.Strapi
{
    /// <summary>
    /// Base interface for all Strapi Dynamic Zone components
    /// 多型系統會自動添加 __component 屬性，所以介面本身只標記 JsonPropertyName
    /// </summary>
    public interface IStrapiComponent
    {
        // 多型系統會自動處理 __component 屬性
        // 介面本身不需要定義屬性，只需要標記用途
    }
}
