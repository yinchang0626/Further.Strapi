using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Further.Strapi;

/// <summary>
/// Strapi Single Type Provider 實現
/// 用於處理 Strapi 的 Single Type 內容類型（如全域設定、首頁資訊等唯一性內容）
/// </summary>
/// <typeparam name="T">Single Type 的資料模型</typeparam>
public class SingleTypeProvider<T> : ISingleTypeProvider<T> where T : class
{
    private readonly IHttpClientFactory _httpClientFactory;

    public SingleTypeProvider(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    private HttpClient CreateHttpClient()
    {
        return _httpClientFactory.CreateClient(StrapiOptions.HttpClientName);
    }

    /// <summary>
    /// 取得 Single Type 資料
    /// </summary>
    public async Task<T> GetAsync()
    {
        var client = CreateHttpClient();
        
        // 建構 API 路徑
        var path = StrapiProtocol.Paths.SingleType<T>();
        
        // 自動產生 populate 查詢
        var populateQuery = StrapiProtocol.Populate.Auto<T>();
        var query = string.IsNullOrEmpty(populateQuery) ? "" : $"?{populateQuery}";
        var request = new HttpRequestMessage(HttpMethod.Get, $"{path}{query}");

        // 發送 GET 請求
        var response = await client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"無法取得 Single Type 資料。狀態碼: {response.StatusCode}");
        }

        // 反序列化回應
        var responseData = await StrapiProtocol.Response.DeserializeResponse<StrapiSingleResponse<T>>(response);
        
        if (responseData?.Data == null)
        {
            throw new InvalidOperationException("Strapi API response does not contain Data object.");
        }

        return responseData.Data;
    }

    /// <summary>
    /// 更新 Single Type 資料
    /// </summary>
    public async Task<string> UpdateAsync<TUpdateValue>(TUpdateValue updateValue)
    {
        var client = CreateHttpClient();

        // 建構 API 路徑
        var path = StrapiProtocol.Paths.SingleType<T>();

        // 序列化請求資料
        var jsonContent = StrapiProtocol.Request.SerializeData(updateValue);
        var request = new HttpRequestMessage(HttpMethod.Put, path)
        {
            Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
        };

        // 發送 PUT 請求
        var response = await client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"無法更新 Single Type 資料。狀態碼: {response.StatusCode}");
        }

        // 反序列化回應並提取 Document ID
        return await StrapiProtocol.Response.ExtractDocumentId(response);
    }
}