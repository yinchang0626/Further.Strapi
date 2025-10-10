using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Json;

namespace Further.Strapi;

public class CollectionTypeProvider<T> : ICollectionTypeProvider<T>
    where T : class
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly StrapiWriteSerializer _strapiWriteSerializer;

    public CollectionTypeProvider(
        IHttpClientFactory httpClientFactory,
        IJsonSerializer jsonSerializer,
        StrapiWriteSerializer strapiWriteSerializer)
    {
        _httpClientFactory = httpClientFactory;
        _jsonSerializer = jsonSerializer;
        _strapiWriteSerializer = strapiWriteSerializer;
    }

    private HttpClient CreateHttpClient()
    {
        return _httpClientFactory.CreateClient(StrapiOptions.HttpClientName);
    }

    public async Task<T> GetAsync(string documentId)
    {
        var client = CreateHttpClient();
        var populateQuery = StrapiProtocol.Populate.Auto<T>();
        var path = StrapiProtocol.Paths.CollectionType<T>(documentId);
        var query = string.IsNullOrEmpty(populateQuery) ? "" : $"?{populateQuery}";
        var request = new HttpRequestMessage(HttpMethod.Get, $"{path}{query}");

        var response = await client.SendAsync(request);

        var responseData = await StrapiProtocol.Response.DeserializeResponse<StrapiSingleResponse<T>>(response, _jsonSerializer);
        
        if (responseData?.Data == null)
        {
            throw new InvalidOperationException("Strapi API response does not contain Data object.");
        }

        return responseData.Data;
    }

    public async Task<List<T>> GetListAsync(
        Action<FilterBuilder>? filter = null,
        Action<PopulateBuilder>? populate = null,
        Action<SortBuilder>? sort = null,
        Action<PaginationBuilder>? pagination = null)
    {
        var client = CreateHttpClient();
        var path = StrapiProtocol.Paths.CollectionType<T>();
        
        // 建構查詢字串
        var queryParams = new List<string>();
        
        // 添加篩選
        if (filter != null)
        {
            var filterBuilder = new FilterBuilder();
            filter(filterBuilder);
            var filterQuery = filterBuilder.Build();
            if (!string.IsNullOrEmpty(filterQuery))
                queryParams.Add(filterQuery);
        }
        
        // 添加填充
        if (populate != null)
        {
            var populateBuilder = new PopulateBuilder();
            populate(populateBuilder);
            var populateQuery = populateBuilder.Build();
            if (!string.IsNullOrEmpty(populateQuery))
                queryParams.Add(populateQuery);
        }
        else
        {
            // 使用自動填充
            var populateBuilder = new PopulateBuilder();
            var populateQuery = populateBuilder.Auto<T>().Build();
            if (!string.IsNullOrEmpty(populateQuery))
                queryParams.Add(populateQuery);
        }
        
        // 添加排序
        if (sort != null)
        {
            var sortBuilder = new SortBuilder();
            sort(sortBuilder);
            var sortQuery = sortBuilder.Build();
            if (!string.IsNullOrEmpty(sortQuery))
                queryParams.Add(sortQuery);
        }
        
        // 添加分頁
        if (pagination != null)
        {
            var paginationBuilder = new PaginationBuilder();
            pagination(paginationBuilder);
            var paginationQuery = paginationBuilder.Build();
            if (!string.IsNullOrEmpty(paginationQuery))
                queryParams.Add(paginationQuery);
        }
        
        // 組合查詢字串
        var query = queryParams.Count > 0 ? $"?{string.Join("&", queryParams)}" : "";
        var request = new HttpRequestMessage(HttpMethod.Get, $"{path}{query}");

        var response = await client.SendAsync(request);

        var responseData = await StrapiProtocol.Response.DeserializeResponse<StrapiCollectionResponse<T>>(response, _jsonSerializer);
        
        if (responseData?.Data == null)
        {
            throw new InvalidOperationException("Strapi API response does not contain Data array.");
        }

        return responseData.Data;
    }

    public async Task<string> CreateAsync(T collectionType)
    {
        var client = CreateHttpClient();
        
        // 使用寫入序列化器並移除系統欄位
        var cleanedJson = _strapiWriteSerializer.SerializeForUpdate(collectionType);
        var jsonContent = $"{{\"data\":{cleanedJson}}}";
        
        var path = StrapiProtocol.Paths.CollectionType<T>();
        var request = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
        };

        var response = await client.SendAsync(request);

        return await StrapiProtocol.Response.ExtractDocumentId(response);
    }

    public async Task<string> UpdateAsync(string documentId, T collectionType)
    {
        var client = CreateHttpClient();
        
        // 使用寫入序列化器並移除系統欄位
        var cleanedJson = _strapiWriteSerializer.SerializeForUpdate(collectionType);
        var jsonContent = $"{{\"data\":{cleanedJson}}}";
        
        var path = StrapiProtocol.Paths.CollectionType<T>(documentId);
        var request = new HttpRequestMessage(HttpMethod.Put, path)
        {
            Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
        };

        var response = await client.SendAsync(request);

        return await StrapiProtocol.Response.ExtractDocumentId(response);
    }

    public async Task DeleteAsync(string documentId)
    {
        var client = CreateHttpClient();
        var path = StrapiProtocol.Paths.CollectionType<T>(documentId);
        var request = new HttpRequestMessage(HttpMethod.Delete, path);

        var response = await client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException("刪除失敗");
        }
    }

}
