using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Further.Strapi;

/// <summary>
/// Strapi Media Library Provider 實現
/// 用於處理 Strapi 的媒體庫操作（檔案上傳、取得、刪除）
/// </summary>
public class MediaLibraryProvider : IMediaLibraryProvider
{
    private readonly IHttpClientFactory _httpClientFactory;

    public MediaLibraryProvider(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    private HttpClient CreateHttpClient()
    {
        return _httpClientFactory.CreateClient(StrapiOptions.HttpClientName);
    }

    /// <summary>
    /// 上傳檔案到 Strapi Media Library
    /// </summary>
    public async Task<StrapiMediaFile> UploadAsync(FileUploadRequest fileUpload)
    {
        var client = CreateHttpClient();
        
        // 建構 API 路徑
        var path = StrapiProtocol.Paths.Media();

        // 使用協定工具建立 form data
        var form = StrapiProtocol.MediaLibrary.CreateUploadForm(fileUpload);

        // 建立請求
        var request = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = form
        };

        // 發送請求
        var response = await client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"無法上傳檔案。狀態碼: {response.StatusCode}");
        }

        // 反序列化回應 (Strapi Media Library 回傳的是陣列)
        var uploadedFiles = await StrapiProtocol.Response.DeserializeResponse<List<StrapiMediaFile>>(response);
        
        if (uploadedFiles == null || uploadedFiles.Count == 0)
        {
            throw new InvalidOperationException("檔案上傳失敗，未收到檔案資訊。");
        }

        return uploadedFiles[0];
    }

    /// <summary>
    /// 上傳檔案並關聯到特定的內容項目
    /// </summary>
    public async Task<StrapiMediaFile> UploadEntryFileAsync(EntryFileUploadRequest entryFileUpload)
    {
        var client = CreateHttpClient();
        
        // 建構 API 路徑
        var path = StrapiProtocol.Paths.Media();

        // 使用協定工具建立 form data
        var form = StrapiProtocol.MediaLibrary.CreateEntryFileUploadForm(entryFileUpload);

        // 建立請求
        var request = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = form
        };

        // 發送請求
        var response = await client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"無法上傳關聯檔案。狀態碼: {response.StatusCode}");
        }

        // 反序列化回應 (Strapi Media Library 回傳的是陣列)
        var uploadedFiles = await StrapiProtocol.Response.DeserializeResponse<List<StrapiMediaFile>>(response);
        
        if (uploadedFiles == null || uploadedFiles.Count == 0)
        {
            throw new InvalidOperationException("關聯檔案上傳失敗，未收到檔案資訊。");
        }

        return uploadedFiles[0];
    }
    public async Task<StrapiMediaFile> GetAsync(int fileId)
    {
        var client = CreateHttpClient();
        
        // 建構 API 路徑
        var path = StrapiProtocol.Paths.Media(fileId.ToString());
        var request = new HttpRequestMessage(HttpMethod.Get, path);

        // 發送請求
        var response = await client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"無法取得檔案資訊。狀態碼: {response.StatusCode}");
        }

        // 反序列化回應
        return await StrapiProtocol.Response.DeserializeResponse<StrapiMediaFile>(response);
    }

    /// <summary>
    /// 取得檔案列表
    /// </summary>
    public async Task<List<StrapiMediaFile>> GetListAsync()
    {
        var client = CreateHttpClient();
        
        // 建構 API 路徑
        var path = StrapiProtocol.Paths.Media() + "/files";
        var request = new HttpRequestMessage(HttpMethod.Get, path);

        // 發送請求
        var response = await client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"無法取得檔案列表。狀態碼: {response.StatusCode}");
        }

        // 反序列化回應
        return await StrapiProtocol.Response.DeserializeResponse<List<StrapiMediaFile>>(response);
    }

    /// <summary>
    /// 更新檔案資訊 (metadata)
    /// </summary>
    public async Task<StrapiMediaFile> UpdateFileInfoAsync(int fileId, FileInfoUpdateRequest updateRequest)
    {
        var client = CreateHttpClient();
        
        // 建構 API 路徑 (根據官方文檔：POST /api/upload?id=x)
        var path = $"{StrapiProtocol.Paths.Media()}?id={fileId}";

        // 使用協定工具建立 form data
        var form = StrapiProtocol.MediaLibrary.CreateFileInfoUpdateForm(updateRequest);

        // 建立請求
        var request = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = form
        };

        // 發送請求
        var response = await client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"無法更新檔案資訊。狀態碼: {response.StatusCode}");
        }

        // 反序列化回應
        return await StrapiProtocol.Response.DeserializeResponse<StrapiMediaFile>(response);
    }

    /// <summary>
    /// 刪除檔案
    /// </summary>
    public async Task DeleteAsync(int fileId)
    {
        var client = CreateHttpClient();
        
        // 建構 API 路徑
        var path = StrapiProtocol.Paths.Media(fileId.ToString());
        var request = new HttpRequestMessage(HttpMethod.Delete, path);

        // 發送請求
        var response = await client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"無法刪除檔案。狀態碼: {response.StatusCode}");
        }
    }
}