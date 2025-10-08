# Single Type vs Collection Type 使用指南

## 概念說明

### Collection Type (集合類型)
- **用途**：處理多個相同類型的內容項目
- **特徵**：支援 CRUD 操作，每個項目都有唯一的 Document ID
- **範例**：文章 (Articles)、產品 (Products)、用戶 (Users)

### Single Type (單一類型)
- **用途**：處理全域或唯一性的內容
- **特徵**：只有一個實例，主要支援 GET 和 UPDATE 操作
- **範例**：全域設定 (Global Settings)、首頁資訊 (Homepage)、關於我們 (About Us)

## 使用範例

### Collection Type 範例

```csharp
// 模型定義
[StrapiCollectionName("articles")]
public class Article
{
    public string DocumentId { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public DateTime PublishedAt { get; set; }
    public Author Author { get; set; }
}

// 服務使用
public class ArticleService
{
    private readonly ICollectionTypeProvider<Article> _articleProvider;

    public ArticleService(ICollectionTypeProvider<Article> articleProvider)
    {
        _articleProvider = articleProvider;
    }

    // 支援完整的 CRUD 操作
    public async Task<Article> GetAsync(string documentId) 
        => await _articleProvider.GetAsync(documentId);

    public async Task<string> CreateAsync(Article article) 
        => await _articleProvider.CreateAsync(article);

    public async Task<string> UpdateAsync(string documentId, Article article) 
        => await _articleProvider.UpdateAsync(documentId, article);

    public async Task DeleteAsync(string documentId) 
        => await _articleProvider.DeleteAsync(documentId);
}
```

### Single Type 範例

```csharp
// 模型定義
[StrapiSingleTypeName("global-setting")]
public class GlobalSetting
{
    public string DocumentId { get; set; }
    public string SiteName { get; set; }
    public string SiteDescription { get; set; }
    public string ContactEmail { get; set; }
    public bool MaintenanceMode { get; set; }
    public SeoSettings Seo { get; set; }
}

public class SeoSettings
{
    public string MetaTitle { get; set; }
    public string MetaDescription { get; set; }
    public string[] Keywords { get; set; }
}

// 服務使用
public class GlobalSettingService
{
    private readonly ISingleTypeProvider<GlobalSetting> _globalSettingProvider;

    public GlobalSettingService(ISingleTypeProvider<GlobalSetting> globalSettingProvider)
    {
        _globalSettingProvider = globalSettingProvider;
    }

    // 只支援 GET 和 UPDATE (沒有 CREATE 和 DELETE)
    public async Task<GlobalSetting> GetAsync() 
        => await _globalSettingProvider.GetAsync();

    public async Task<string> UpdateAsync(GlobalSetting setting) 
        => await _globalSettingProvider.UpdateAsync(setting);
}
```

## API 路徑對比

### Collection Type API 路徑
```
GET    /api/articles           # 取得所有文章
GET    /api/articles/{id}      # 取得特定文章
POST   /api/articles           # 建立新文章
PUT    /api/articles/{id}      # 更新特定文章
DELETE /api/articles/{id}      # 刪除特定文章
```

### Single Type API 路徑
```
GET    /api/global-setting     # 取得全域設定
PUT    /api/global-setting     # 更新全域設定
```

## 實際應用場景

### Collection Type 適用場景
- ✅ **部落格文章**：多篇文章，需要分別管理
- ✅ **產品目錄**：多個產品，支援新增/刪除
- ✅ **用戶管理**：多個用戶帳號
- ✅ **新聞列表**：多條新聞
- ✅ **相片集**：多張相片

### Single Type 適用場景
- ✅ **網站全域設定**：網站名稱、聯絡資訊
- ✅ **首頁內容**：首頁橫幅、介紹文字
- ✅ **關於我們頁面**：公司簡介
- ✅ **隱私政策**：法律條文
- ✅ **網站維護狀態**：維護模式開關

## 注意事項

### Collection Type
1. **必須提供 Document ID** 進行個別操作
2. **支援完整 CRUD** 操作
3. **自動 populate** 關聯資料
4. **適合大量數據** 的管理

### Single Type
1. **不需要 Document ID** 存取
2. **只支援 GET/UPDATE** 操作
3. **通常包含全域性** 的設定資料
4. **適合唯一性內容** 的管理

### 共同特點
- ✅ 都支援自動 populate 查詢
- ✅ 都使用相同的序列化/反序列化機制
- ✅ 都支援強型別模型
- ✅ 都遵循相同的錯誤處理模式