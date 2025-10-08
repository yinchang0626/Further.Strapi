using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Further.Strapi;

public class CollectionTypeProvider<T> : ICollectionTypeProvider<T>
    where T : class
{
    private readonly IHttpClientFactory _httpClientFactory;

    public CollectionTypeProvider(
        IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
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

        var responseData = await StrapiProtocol.Response.DeserializeResponse<StrapiSingleResponse<T>>(response);
        
        if (responseData?.Data == null)
        {
            throw new InvalidOperationException("Strapi API response does not contain Data object.");
        }

        return responseData.Data;
    }

    public async Task<string> CreateAsync(T collectionType)
    {
        var client = CreateHttpClient();
        var jsonContent = StrapiProtocol.Request.SerializeData(collectionType);
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
        var jsonContent = StrapiProtocol.Request.SerializeData(collectionType);
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
