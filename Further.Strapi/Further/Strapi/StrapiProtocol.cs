using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Volo.Abp.Json;

namespace Further.Strapi;

/// <summary>
/// Strapi API 協定工具類別，處理路徑命名、查詢語法等
/// </summary>
public static class StrapiProtocol
{
    /// <summary>
    /// 路徑建構器
    /// </summary>
    public static class Paths
    {
        /// <summary>
        /// 建構 Collection Type 路徑
        /// </summary>
        public static string CollectionType<T>(string documentId = null)
        {
            var collectionName = GetCollectionName<T>();
            return documentId == null 
                ? $"api/{collectionName}" 
                : $"api/{collectionName}/{documentId}";
        }

        /// <summary>
        /// 建構 Single Type 路徑
        /// </summary>
        public static string SingleType<T>()
        {
            var typeName = GetSingleTypeName<T>();
            return $"api/{typeName}";
        }

        /// <summary>
        /// 建構 Media 路徑
        /// </summary>
        public static string Media(string fileId = null)
        {
            if (string.IsNullOrWhiteSpace(fileId))
            {
                if (fileId == null)
                {
                    return "api/upload";
                }
                
                // 空字串或純空白字元應該拋出異常
                throw new ArgumentException("FileId cannot be empty or whitespace. Use null for upload endpoint.", nameof(fileId));
            }
            
            return $"api/upload/files/{fileId}";
        }

        private static string GetCollectionName<T>()
        {
            var type = typeof(T);
            var attr = type.GetCustomAttribute<StrapiCollectionNameAttribute>();
            
            if (attr != null)
                return attr.CollectionName;
                
            // 預設: 類型名稱轉 kebab-case + 複數形式
            var kebabName = ConvertToKebabCase(type.Name);
            return $"{kebabName}s";
        }

        private static string GetSingleTypeName<T>()
        {
            var type = typeof(T);
            var attr = type.GetCustomAttribute<StrapiSingleTypeNameAttribute>();
            
            if (attr != null)
                return attr.TypeName;
                
            // 預設: 類型名稱轉 kebab-case (單數)
            return ConvertToKebabCase(type.Name);
        }

        private static string ConvertToKebabCase(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            
            var kebab = System.Text.RegularExpressions.Regex.Replace(input, "(?<!^)([A-Z])", "-$1");
            return kebab.ToLower();
        }
    }

    /// <summary>
    /// Populate 查詢建構器
    /// </summary>
    public static class Populate
    {
        /// <summary>
        /// 自動產生 Populate 查詢 (根據型別屬性)
        /// </summary>
        public static string Auto<T>(int maxDepth = 5)
        {
            var builder = new PopulateBuilder();
            return builder.GenerateForType<T>(maxDepth);
        }

        /// <summary>
        /// 手動建構 Populate 查詢
        /// </summary>
        public static PopulateBuilder Manual()
        {
            return new PopulateBuilder();
        }

        /// <summary>
        /// 填充所有欄位 (*)
        /// </summary>
        public static string All()
        {
            return "populate=*";
        }

        /// <summary>
        /// 深度填充 (**)
        /// </summary>
        public static string Deep()
        {
            return "populate=**";
        }
    }

    /// <summary>
    /// HTTP 回應處理工具
    /// </summary>
    public static class Response
    {
        /// <summary>
        /// 將 HTTP 回應反序列化為指定類型
        /// </summary>
        public static async Task<TResponse> DeserializeResponse<TResponse>(HttpResponseMessage response, IJsonSerializer jsonSerializer)
        {
            try
            {
                var jsonString = await response.Content.ReadAsStringAsync();

                if (response != null && response.IsSuccessStatusCode)
                {
                    return jsonSerializer.Deserialize<TResponse>(jsonString, camelCase: true);
                }

                throw new InvalidOperationException($"{jsonString}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"無法反序列化回應，錯誤代碼:{response?.StatusCode}，錯誤訊息:{ex.Message}");
            }
        }

        /// <summary>
        /// 從 Strapi API 回應中提取 documentId
        /// </summary>
        public static async Task<string> ExtractDocumentId(HttpResponseMessage response)
        {
            try
            {
                var jsonString = await response.Content.ReadAsStringAsync();

                if (response != null && response.IsSuccessStatusCode)
                {
                    // 使用 JsonDocument 直接從 JSON 中提取 documentId
                    using var document = JsonDocument.Parse(jsonString);
                    var root = document.RootElement;
                    
                    // 檢查是否有 data 屬性
                    if (root.TryGetProperty("data", out var dataElement))
                    {
                        // 檢查 data 中是否有 documentId 屬性
                        if (dataElement.TryGetProperty("documentId", out var documentIdElement))
                        {
                            var documentId = documentIdElement.GetString();
                            if (!string.IsNullOrWhiteSpace(documentId))
                            {
                                return documentId;
                            }
                        }
                    }
                    
                    throw new InvalidOperationException("Strapi API response does not contain documentId in data object.");
                }

                throw new InvalidOperationException($"HTTP request failed: {jsonString}");
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Failed to parse JSON response: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"無法取得 DocumentId，錯誤代碼:{response?.StatusCode}，錯誤訊息:{ex.Message}");
            }
        }
    }

    /// <summary>
    /// Media Library 工具
    /// </summary>
    public static class MediaLibrary
    {
        /// <summary>
        /// 建立檔案上傳的 MultipartFormDataContent
        /// </summary>
        public static MultipartFormDataContent CreateUploadForm(FileUploadRequest fileUpload)
        {
            var form = new MultipartFormDataContent();

            // 加入檔案內容
            var fileContent = new StreamContent(fileUpload.FileStream);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(fileUpload.ContentType);
            form.Add(fileContent, "files", fileUpload.FileName);

            // 加入檔案 metadata 為 JSON 字符串 (單檔案上傳標準格式)
            var fileInfo = new
            {
                name = fileUpload.FileName,
                alternativeText = fileUpload.AlternativeText,
                caption = fileUpload.Caption
            };

            var fileInfoJson = JsonSerializer.Serialize(fileInfo, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });

            // 不要指定 Content-Type，讓它使用預設的 text/plain
            // 避免使用 StringContent(string, Encoding) 因為它會自動設置 Content-Type
            // 甚至 StringContent(string) 也會設置默認 Content-Type，需要手動清除
            var stringContent = new StringContent(fileInfoJson);
            stringContent.Headers.ContentType = null; // 清除 Content-Type
            form.Add(stringContent, "fileInfo");

            // 加入可選的路徑 (僅在 AWS S3 等 provider 支援)
            if (!string.IsNullOrEmpty(fileUpload.Path))
            {
                form.Add(new StringContent(fileUpload.Path, Encoding.UTF8), "path");
            }

            return form;
        }

        /// <summary>
        /// 建立關聯檔案上傳的 MultipartFormDataContent
        /// </summary>
        public static MultipartFormDataContent CreateEntryFileUploadForm(EntryFileUploadRequest entryFileUpload)
        {
            var form = CreateUploadForm(entryFileUpload);

            // 加入關聯資訊
            form.Add(new StringContent(entryFileUpload.RefId, Encoding.UTF8), "refId");
            form.Add(new StringContent(entryFileUpload.Ref, Encoding.UTF8), "ref");
            form.Add(new StringContent(entryFileUpload.Field, Encoding.UTF8), "field");

            if (!string.IsNullOrEmpty(entryFileUpload.Source))
            {
                form.Add(new StringContent(entryFileUpload.Source, Encoding.UTF8), "source");
            }

            return form;
        }

        /// <summary>
        /// 建立檔案資訊更新的 MultipartFormDataContent
        /// </summary>
        public static MultipartFormDataContent CreateFileInfoUpdateForm(FileInfoUpdateRequest updateRequest)
        {
            var form = new MultipartFormDataContent();

            var fileInfoJson = JsonSerializer.Serialize(updateRequest, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });

            // 不要指定 Content-Type，讓它使用預設的 text/plain
            // 避免使用 StringContent(string, Encoding) 因為它會自動設置 Content-Type
            // 甚至 StringContent(string) 也會設置默認 Content-Type，需要手動清除
            var stringContent = new StringContent(fileInfoJson);
            stringContent.Headers.ContentType = null; // 清除 Content-Type
            form.Add(stringContent, "fileInfo");

            return form;
        }
    }
}

#region Response Models

/// <summary>
/// Strapi API 單一文檔回應模型 - 用於 GET single、POST、PUT 操作
/// </summary>
public class StrapiSingleResponse<T>
    where T : class
{
    public T Data { get; set; }
    public StrapiMeta? Meta { get; set; } = null;
}

/// <summary>
/// Strapi API 多文檔回應模型 - 用於 GET list 操作
/// </summary>
public class StrapiCollectionResponse<T>
    where T : class
{
    public List<T> Data { get; set; }
    public StrapiMeta? Meta { get; set; } = null;
}

/// <summary>
/// Strapi 元資料
/// </summary>
public class StrapiMeta
{
    public StrapiPagination? Pagination { get; set; } = null;
}

/// <summary>
/// 分頁資訊
/// </summary>
public class StrapiPagination
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int PageCount { get; set; }
    public int Total { get; set; }
}

#endregion