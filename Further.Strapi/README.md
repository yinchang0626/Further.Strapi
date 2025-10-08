# Strapi .NET Client 使用指南

## 服務註冊

### 1. 基本配置

```csharp
// 方式 1: 直接配置選項
services.AddStrapi(builder =>
{
    builder.ConfigureOptions(options =>
    {
        options.StrapiUrl = "http://localhost:1337";
        options.StrapiToken = "your-api-token-here";
    });
});

// 方式 2: 從配置文件讀取 (最簡單)
services.AddStrapi(); // 使用 appsettings.json 的 "Strapi" 區段
```

### 2. 進階配置

```csharp
services.AddStrapi(builder =>
{
    // 配置基本選項
    builder.ConfigureOptions(options =>
    {
        options.StrapiUrl = "http://localhost:1337";
        options.StrapiToken = "your-api-token-here";
    });

    // 自訂 HttpClient 配置
    builder.ConfigureHttpClient((serviceProvider, client) =>
    {
        client.Timeout = TimeSpan.FromSeconds(30);
        client.DefaultRequestHeaders.Add("Custom-Header", "Value");
    });

    // 配置 Polly 重試策略
    builder.ConfigureHttpClientBuilder(clientBuilder =>
    {
        clientBuilder.AddTransientHttpErrorPolicy(policyBuilder =>
            policyBuilder.WaitAndRetryAsync(
                3,
                i => TimeSpan.FromSeconds(Math.Pow(2, i))
            )
        );
    });
});
```

## 配置文件 (appsettings.json)

```json
{
  "Strapi": {
    "StrapiUrl": "http://localhost:1337",
    "StrapiToken": "your-api-token-here"
  }
}
```

## 使用範例

### 1. 定義模型

```csharp
// Collection Type 範例
[StrapiCollectionName("articles")]
public class Article
{
    public string DocumentId { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public DateTime PublishedAt { get; set; }
    public Author Author { get; set; }
    public List<Category> Categories { get; set; }
}

// Single Type 範例
[StrapiSingleTypeName("global-setting")]
public class GlobalSetting
{
    public string DocumentId { get; set; }
    public string SiteName { get; set; }
    public string SiteDescription { get; set; }
    public string ContactEmail { get; set; }
    public bool MaintenanceMode { get; set; }
}

public class Author
{
    public string DocumentId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}

public class Category
{
    public string DocumentId { get; set; }
    public string Name { get; set; }
}
```

### 2. 在服務中使用

```csharp
// Collection Type 服務
public class ArticleService
{
    private readonly ICollectionTypeProvider<Article> _articleProvider;

    public ArticleService(ICollectionTypeProvider<Article> articleProvider)
    {
        _articleProvider = articleProvider;
    }

    // 取得單一文章 (自動 populate 關聯資料)
    public async Task<Article> GetArticleAsync(string documentId)
    {
        return await _articleProvider.GetAsync(documentId);
    }

    // 建立新文章
    public async Task<string> CreateArticleAsync(Article article)
    {
        return await _articleProvider.CreateAsync(article);
    }

    // 更新文章
    public async Task<string> UpdateArticleAsync(string documentId, Article article)
    {
        return await _articleProvider.UpdateAsync(documentId, article);
    }

    // 刪除文章
    public async Task DeleteArticleAsync(string documentId)
    {
        await _articleProvider.DeleteAsync(documentId);
    }
}

// Single Type 服務
public class GlobalSettingService
{
    private readonly ISingleTypeProvider<GlobalSetting> _globalSettingProvider;

    public GlobalSettingService(ISingleTypeProvider<GlobalSetting> globalSettingProvider)
    {
        _globalSettingProvider = globalSettingProvider;
    }

    // 取得全域設定
    public async Task<GlobalSetting> GetGlobalSettingAsync()
    {
        return await _globalSettingProvider.GetAsync();
    }

    // 更新全域設定
    public async Task<string> UpdateGlobalSettingAsync(GlobalSetting setting)
    {
        return await _globalSettingProvider.UpdateAsync(setting);
    }
}

// Media Library 服務
public class MediaService
{
    private readonly IMediaLibraryProvider _mediaProvider;

    public MediaService(IMediaLibraryProvider mediaProvider)
    {
        _mediaProvider = mediaProvider;
    }

    // 上傳檔案
    public async Task<StrapiMediaFile> UploadFileAsync(Stream fileStream, string fileName, string contentType)
    {
        var uploadRequest = new FileUploadRequest
        {
            FileStream = fileStream,
            FileName = fileName,
            ContentType = contentType,
            AlternativeText = "Uploaded file",
            Caption = "File uploaded via API"
        };

        return await _mediaProvider.UploadAsync(uploadRequest);
    }

    // 上傳檔案並關聯到內容項目
    public async Task<StrapiMediaFile> UploadRestaurantCoverAsync(Stream fileStream, string fileName, string restaurantId)
    {
        var uploadRequest = new EntryFileUploadRequest
        {
            FileStream = fileStream,
            FileName = fileName,
            ContentType = "image/jpeg",
            AlternativeText = "Restaurant cover image",
            RefId = restaurantId,
            Ref = "api::restaurant.restaurant",
            Field = "cover"
        };

        return await _mediaProvider.UploadEntryFileAsync(uploadRequest);
    }

    // 取得檔案資訊
    public async Task<StrapiMediaFile> GetFileAsync(int fileId)
    {
        return await _mediaProvider.GetAsync(fileId);
    }

    // 取得所有檔案
    public async Task<List<StrapiMediaFile>> GetAllFilesAsync()
    {
        return await _mediaProvider.GetListAsync();
    }

    // 更新檔案資訊
    public async Task<StrapiMediaFile> UpdateFileInfoAsync(int fileId, string newAltText, string newCaption)
    {
        var updateRequest = new FileInfoUpdateRequest
        {
            AlternativeText = newAltText,
            Caption = newCaption
        };

        return await _mediaProvider.UpdateFileInfoAsync(fileId, updateRequest);
    }

    // 刪除檔案
    public async Task DeleteFileAsync(int fileId)
    {
        await _mediaProvider.DeleteAsync(fileId);
    }
}
```

### 3. 控制器使用

```csharp
// Collection Type 控制器
[ApiController]
[Route("api/[controller]")]
public class ArticlesController : ControllerBase
{
    private readonly ICollectionTypeProvider<Article> _articleProvider;

    public ArticlesController(ICollectionTypeProvider<Article> articleProvider)
    {
        _articleProvider = articleProvider;
    }

    [HttpGet("{documentId}")]
    public async Task<ActionResult<Article>> Get(string documentId)
    {
        try
        {
            var article = await _articleProvider.GetAsync(documentId);
            return Ok(article);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost]
    public async Task<ActionResult<string>> Create(Article article)
    {
        try
        {
            var documentId = await _articleProvider.CreateAsync(article);
            return CreatedAtAction(nameof(Get), new { documentId }, documentId);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{documentId}")]
    public async Task<ActionResult<string>> Update(string documentId, Article article)
    {
        try
        {
            var result = await _articleProvider.UpdateAsync(documentId, article);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{documentId}")]
    public async Task<IActionResult> Delete(string documentId)
    {
        try
        {
            await _articleProvider.DeleteAsync(documentId);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }
}

// Single Type 控制器
[ApiController]
[Route("api/[controller]")]
public class GlobalSettingController : ControllerBase
{
    private readonly ISingleTypeProvider<GlobalSetting> _globalSettingProvider;

    public GlobalSettingController(ISingleTypeProvider<GlobalSetting> globalSettingProvider)
    {
        _globalSettingProvider = globalSettingProvider;
    }

    [HttpGet]
    public async Task<ActionResult<GlobalSetting>> Get()
    {
        try
        {
            var setting = await _globalSettingProvider.GetAsync();
            return Ok(setting);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPut]
    public async Task<ActionResult<string>> Update(GlobalSetting setting)
    {
        try
        {
            var documentId = await _globalSettingProvider.UpdateAsync(setting);
            return Ok(documentId);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}

// Media Library 控制器
[ApiController]
[Route("api/[controller]")]
public class MediaController : ControllerBase
{
    private readonly IMediaLibraryProvider _mediaProvider;

    public MediaController(IMediaLibraryProvider mediaProvider)
    {
        _mediaProvider = mediaProvider;
    }

    [HttpPost("upload")]
    public async Task<ActionResult<StrapiMediaFile>> Upload(IFormFile file)
    {
        try
        {
            var uploadRequest = new FileUploadRequest
            {
                FileStream = file.OpenReadStream(),
                FileName = file.FileName,
                ContentType = file.ContentType,
                AlternativeText = file.FileName
            };

            var result = await _mediaProvider.UploadAsync(uploadRequest);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{fileId:int}")]
    public async Task<ActionResult<StrapiMediaFile>> Get(int fileId)
    {
        try
        {
            var file = await _mediaProvider.GetAsync(fileId);
            return Ok(file);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<StrapiMediaFile>>> GetList()
    {
        try
        {
            var files = await _mediaProvider.GetListAsync();
            return Ok(files);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{fileId:int}")]
    public async Task<IActionResult> Delete(int fileId)
    {
        try
        {
            await _mediaProvider.DeleteAsync(fileId);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }
}
```

## Protocol 使用範例

### 路徑建構

```csharp
// Collection Type 路徑
var listPath = StrapiProtocol.Paths.CollectionType<Article>();          // "api/articles"
var itemPath = StrapiProtocol.Paths.CollectionType<Article>("doc123");  // "api/articles/doc123"

// Single Type 路徑
var globalPath = StrapiProtocol.Paths.SingleType<GlobalSetting>();      // "api/global-setting"

// Media 路徑
var uploadPath = StrapiProtocol.Paths.Media();                          // "api/upload"
var filePath = StrapiProtocol.Paths.Media("file123");                   // "api/upload/files/file123"
```

### Populate 查詢

```csharp
// 自動產生 (根據模型屬性)
var autoPopulate = StrapiProtocol.Populate.Auto<Article>();

// 手動建構
var manualPopulate = StrapiProtocol.Populate.Manual()
    .Add("author")
    .Add("categories")
    .AddWithFields("author", "name", "email")
    .Build();

// 簡單填充
var allPopulate = StrapiProtocol.Populate.All();      // populate=*
var deepPopulate = StrapiProtocol.Populate.Deep();    // populate=**
```

### 篩選查詢

```csharp
var filters = StrapiProtocol.Filters.Create()
    .Equal("status", "published")
    .Contains("title", "Hello")
    .GreaterThanOrEqual("publishedAt", DateTime.Now.AddDays(-30))
    .IsNotNull("author")
    .In("category", "tech", "news")
    .Build();
```

### 排序查詢

```csharp
var sorts = StrapiProtocol.Sort.Create()
    .Desc("publishedAt")
    .Asc("title")
    .Build();
```

### 分頁查詢

```csharp
var pagination = StrapiProtocol.Pagination.Create(page: 1, pageSize: 10);
```

### Media Library 表單

```csharp
// 建立檔案上傳表單
var fileUpload = new FileUploadRequest { ... };
var form = StrapiProtocol.MediaLibrary.CreateUploadForm(fileUpload);
```

## 設計特色

### ✅ **統一的 Builder 模式**
- 只有一個 `AddStrapi` 方法，支援可選的 Builder 配置
- `configure` 參數可為 null，自動使用 appsettings.json 設定

### ✅ **靈活的配置方式**
```csharp
// 使用 appsettings.json
services.AddStrapi();

// 覆蓋特定設定
services.AddStrapi(builder => 
{
    builder.ConfigureOptions(options => { ... });
});

// 進階配置
services.AddStrapi(builder => 
{
    builder.ConfigureOptions(options => { ... });
    builder.ConfigureHttpClient((sp, client) => { ... });
    builder.ConfigureHttpClientBuilder(clientBuilder => { ... });
});
```

### ✅ **統一的協定管理**
所有 Strapi 相關工具都在 `StrapiProtocol` 中，支援：
- 路徑建構
- 查詢語法
- 回應處理
- 請求序列化

## 注意事項

1. **覆蓋策略**：支援重複調用 `AddStrapi`，後面的配置會覆蓋前面的配置
   - **Options 配置**：累加式，相同屬性覆蓋，不同屬性保留
   - **HttpClient 配置**：完全覆蓋，後面的設定完全取代前面的設定
2. **模組優先順序**：符合 ABP 拓樸依賴，高階模組配置優於低階模組
3. **錯誤處理**：所有方法都會拋出 `InvalidOperationException`，需要適當的錯誤處理
4. **型別安全**：使用強型別模型確保編譯時期的型別安全
5. **自動 Populate**：`GetAsync` 方法會自動產生 populate 查詢以載入關聯資料

### 覆蓋策略範例

```csharp
// 模組 1 (低階)
services.AddStrapi(builder =>
{
    builder.ConfigureOptions(options =>
    {
        options.StrapiUrl = "http://base-url.com";
        options.StrapiToken = "base-token";
    });
});

// 模組 2 (高階) - 會覆蓋模組 1 的設定
services.AddStrapi(builder =>
{
    builder.ConfigureOptions(options =>
    {
        options.StrapiUrl = "http://override-url.com"; // 覆蓋
        // StrapiToken 會保留 "base-token"
    });
    
    builder.ConfigureHttpClient((sp, client) =>
    {
        client.Timeout = TimeSpan.FromSeconds(60); // 完全覆蓋 HttpClient 配置
    });
});
```