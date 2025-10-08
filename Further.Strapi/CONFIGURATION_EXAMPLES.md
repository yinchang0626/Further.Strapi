# Strapi 配置使用範例

## 模組依賴配置優先順序

### 配置執行順序

1. **StrapiContractsModule** (最低優先度)
   - 從 appsettings.json 讀取 "Strapi" 配置區段
   - 設定預設值

2. **StrapiModule** (中等優先度)
   - DependsOn StrapiContractsModule
   - 會調用 `services.AddStrapi()`
   - 不覆蓋任何設定，使用 appsettings.json

3. **應用程式模組** (最高優先度)
   - 可以透過以下方式覆蓋配置

## 配置方式範例

### 方式 1: 覆蓋配置

```csharp
services.AddStrapi(builder =>
{
    builder.ConfigureOptions(options =>
    {
        options.StrapiUrl = "http://production-strapi.example.com";
        options.StrapiToken = "production-token";
    });
});
```

### 方式 2: 不覆蓋 appsettings.json 設定

```csharp
services.AddStrapi(); // 直接使用 appsettings.json 的設定
```

### 方式 3: 進階 Builder 配置

```csharp
services.AddStrapi(builder =>
{
    // 覆蓋基本配置
    builder.ConfigureOptions(options =>
    {
        options.StrapiUrl = "http://advanced-strapi.example.com";
        options.StrapiToken = "advanced-token";
    });

    // 自訂 HttpClient 行為
    builder.ConfigureHttpClient((serviceProvider, client) =>
    {
        client.Timeout = TimeSpan.FromSeconds(60);
        client.DefaultRequestHeaders.Add("X-Custom-Header", "MyApp");
        
        // 可以從 DI 容器取得其他服務
        var logger = serviceProvider.GetService<ILogger<string>>();
        logger?.LogInformation("Configuring Strapi HttpClient");
    });

    // 配置 Polly 重試策略
    builder.ConfigureHttpClientBuilder(clientBuilder =>
    {
        // 這裡可以加入 Polly 套件的重試策略
        // clientBuilder.AddPolicyHandler(...);
    });
});
```

## appsettings.json 範例

```json
{
  "Strapi": {
    "StrapiUrl": "http://localhost:1337",
    "StrapiToken": "your-default-token-from-config"
  }
}
```

## ABP 模組配置優先順序

**後者覆蓋前者的規則：**

1. `StrapiContractsModule.ConfigureServices()`
   ```csharp
   services.Configure<StrapiOptions>(configuration.GetSection("Strapi"));
   ```

2. `StrapiModule.ConfigureServices()`
   ```csharp
   services.AddStrapi(); // 不覆蓋，使用 appsettings.json
   ```

3. `應用程式模組.ConfigureServices()`
   ```csharp
   services.AddStrapi(builder => { ... }); // 最高優先度
   ```

**拓樸依賴規則：** 越高階的模組越後執行，因此有越高的優先度。

## 覆蓋策略行為

### Options 配置 (累加式)
- 新的屬性覆蓋舊的
- 未設定的屬性保留前面的值
- 使用 `services.Configure<StrapiOptions>()`

### HttpClient 配置 (完全覆蓋)
- 後面的配置完全取代前面的配置
- 包括 Timeout、Headers、BaseAddress 等所有設定
- 使用 `services.AddHttpClient()`

### HttpClientBuilder 配置 (完全覆蓋)
- 用於配置 Polly 策略、Message Handlers 等
- 也是完全覆蓋的行為

## 優勢

✅ **符合 ABP 模組拓樸依賴的優先順序**  
✅ **高階模組可以完全控制 Strapi 行為**  
✅ **簡單直觀，不需要複雜的檢查邏輯**  
✅ **支援靈活的配置策略**

## 注意事項

⚠️ **Options 是累加的，HttpClient 配置是覆蓋的**  
⚠️ **如果多個模組都配置 HttpClient，只有最後一個會生效**  
⚠️ **建議在最高階的應用程式模組進行 HttpClient 的最終配置**