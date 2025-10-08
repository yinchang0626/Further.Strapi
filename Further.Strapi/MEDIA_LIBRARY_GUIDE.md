# Media Library 使用指南

## 概念說明

**Media Library Provider** 專門處理 Strapi 的媒體庫操作，包括檔案上傳、取得、列表和刪除。

## 避免的壞味道 ❌

原始 `StrapiImageHttpClient` 有以下問題：
1. **不必要的 configuratorName 參數** - 增加複雜度
2. **URL 正規化混在 Provider 中** - 違反單一責任原則
3. **繼承基底類別** - 造成不必要的耦合
4. **處理 StrapiUrlAttribute** - 不是 Provider 的責任

## 新設計的優點 ✅

1. **單一責任** - 只負責 HTTP 請求/回應
2. **簡潔 API** - 沒有多餘的參數
3. **型別安全** - 使用強型別模型
4. **統一模式** - 與其他 Provider 一致
5. **協定集中** - Form 建構邏輯在 StrapiProtocol 中統一管理

## Protocol 使用範例

### Form 建構

```csharp
// 直接使用 StrapiProtocol 建構上傳表單
var fileUpload = new FileUploadRequest
{
    FileStream = file.OpenReadStream(),
    FileName = file.FileName,
    ContentType = file.ContentType,
    AlternativeText = "Product image",
    Caption = "Main product photo"
};

// Protocol 會自動處理 MultipartFormDataContent 的建立
var form = StrapiProtocol.MediaLibrary.CreateUploadForm(fileUpload);

// form 包含:
// - files: 檔案內容
// - fileInfo[0][alternativeText]: 替代文字 (可選)
// - fileInfo[0][caption]: 說明文字 (可選)
```

## 基本使用

### 模型定義

```csharp
public class StrapiMediaFile
{
    public int Id { get; set; }
    public string DocumentId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public double Size { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string AlternativeText { get; set; } = string.Empty;
    public string Caption { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class FileUploadRequest
{
    public Stream FileStream { get; set; } = null!;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string? AlternativeText { get; set; }
    public string? Caption { get; set; }
}
```

### 服務注入

```csharp
public class MediaService
{
    private readonly IMediaLibraryProvider _mediaProvider;

    public MediaService(IMediaLibraryProvider mediaProvider)
    {
        _mediaProvider = mediaProvider;
    }
}
```

## 操作範例

### 1. 基本檔案上傳

```csharp
public async Task<StrapiMediaFile> UploadImageAsync(IFormFile file)
{
    var uploadRequest = new FileUploadRequest
    {
        FileStream = file.OpenReadStream(),
        FileName = file.FileName,
        ContentType = file.ContentType,
        AlternativeText = "Product image",
        Caption = "Main product photo",
        Path = "products/images" // 僅在 AWS S3 等 provider 支援
    };

    return await _mediaProvider.UploadAsync(uploadRequest);
}

// 上傳本地檔案
public async Task<StrapiMediaFile> UploadLocalFileAsync(string filePath)
{
    using var fileStream = File.OpenRead(filePath);
    var fileName = Path.GetFileName(filePath);
    var contentType = GetContentType(fileName);

    var uploadRequest = new FileUploadRequest
    {
        FileStream = fileStream,
        FileName = fileName,
        ContentType = contentType
    };

    return await _mediaProvider.UploadAsync(uploadRequest);
}
```

### 2. 關聯檔案上傳 (Upload Entry Files)

```csharp
// 上傳餐廳封面圖片並自動關聯
public async Task<StrapiMediaFile> UploadRestaurantCoverAsync(IFormFile file, string restaurantId)
{
    var uploadRequest = new EntryFileUploadRequest
    {
        FileStream = file.OpenReadStream(),
        FileName = file.FileName,
        ContentType = file.ContentType,
        AlternativeText = "Restaurant cover image",
        Caption = "Main restaurant photo",
        RefId = restaurantId,                    // 餐廳的 document ID
        Ref = "api::restaurant.restaurant",     // 內容類型的 uid
        Field = "cover"                         // 欄位名稱
    };

    return await _mediaProvider.UploadEntryFileAsync(uploadRequest);
}

// 上傳用戶頭像
public async Task<StrapiMediaFile> UploadUserAvatarAsync(IFormFile file, string userId)
{
    var uploadRequest = new EntryFileUploadRequest
    {
        FileStream = file.OpenReadStream(),
        FileName = file.FileName,
        ContentType = file.ContentType,
        AlternativeText = "User avatar",
        RefId = userId,
        Ref = "plugin::users-permissions.user",  // 用戶權限外掛
        Field = "avatar",
        Source = "users-permissions"             // 外掛名稱
    };

    return await _mediaProvider.UploadEntryFileAsync(uploadRequest);
}
```

### 3. 檔案資訊管理

```csharp
// 取得單一檔案
public async Task<StrapiMediaFile> GetFileInfoAsync(int fileId)
{
    return await _mediaProvider.GetAsync(fileId);
}

// 取得所有檔案
public async Task<List<StrapiMediaFile>> GetAllFilesAsync()
{
    return await _mediaProvider.GetListAsync();
}

// 更新檔案 metadata
public async Task<StrapiMediaFile> UpdateFileMetadataAsync(int fileId, string newAltText, string newCaption)
{
    var updateRequest = new FileInfoUpdateRequest
    {
        AlternativeText = newAltText,
        Caption = newCaption,
        Name = "new-filename.jpg" // 也可以更新檔案名稱
    };

    return await _mediaProvider.UpdateFileInfoAsync(fileId, updateRequest);
}

// 刪除檔案
public async Task DeleteFileAsync(int fileId)
{
    await _mediaProvider.DeleteAsync(fileId);
}
```

### 3. 檔案 URL 處理

```csharp
public string GetFullImageUrl(StrapiMediaFile file, string baseUrl)
{
    // URL 正規化在應用層處理，不在 Provider 中
    if (file.Url.StartsWith("http"))
    {
        return file.Url; // 已經是完整 URL
    }
    
    return $"{baseUrl.TrimEnd('/')}{file.Url}";
}

// 範例：產生不同尺寸的圖片 URL
public class ImageUrlHelper
{
    private readonly string _baseUrl;

    public ImageUrlHelper(string baseUrl)
    {
        _baseUrl = baseUrl.TrimEnd('/');
    }

    public string GetThumbnail(StrapiMediaFile file)
    {
        var basePath = Path.GetDirectoryName(file.Url)?.Replace('\\', '/');
        var fileName = Path.GetFileNameWithoutExtension(file.Url);
        var extension = Path.GetExtension(file.Url);
        
        return $"{_baseUrl}{basePath}/thumbnail_{fileName}{extension}";
    }

    public string GetFullSize(StrapiMediaFile file)
    {
        return GetFullImageUrl(file, _baseUrl);
    }
}
```

## 控制器範例

```csharp
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
        if (file == null || file.Length == 0)
        {
            return BadRequest("請選擇檔案");
        }

        // 檔案大小限制 (10MB)
        if (file.Length > 10 * 1024 * 1024)
        {
            return BadRequest("檔案大小不能超過 10MB");
        }

        // 檔案類型檢查
        var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "application/pdf" };
        if (!allowedTypes.Contains(file.ContentType))
        {
            return BadRequest("不支援的檔案類型");
        }

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

    [HttpGet]
    public async Task<ActionResult<List<StrapiMediaFile>>> GetFiles()
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
    public async Task<IActionResult> DeleteFile(int fileId)
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

## API 路徑

Media Library Provider 使用以下 Strapi API 端點：

```
POST   /api/upload              # 基本檔案上傳
POST   /api/upload              # 關聯檔案上傳 (with refId, ref, field)
GET    /api/upload/files        # 取得檔案列表
GET    /api/upload/files/{id}   # 取得特定檔案
POST   /api/upload?id={id}      # 更新檔案資訊 (fileInfo)
DELETE /api/upload/files/{id}   # 刪除檔案
```

### API 參數說明

#### 基本上傳參數
- `files`: 檔案內容 (必須)
- `fileInfo[0][alternativeText]`: 替代文字 (可選)
- `fileInfo[0][caption]`: 說明文字 (可選)
- `path`: 檔案路徑 (僅 AWS S3 等 provider，可選)

#### 關聯上傳參數
- `refId`: 關聯的 entry ID (必須)
- `ref`: 模型的 uid，如 "api::restaurant.restaurant" (必須)
- `field`: 欄位名稱 (必須)
- `source`: 外掛名稱，如 "users-permissions" (可選)

#### 更新 fileInfo 參數
- `fileInfo`: JSON 格式的檔案資訊更新

## 注意事項

### 檔案上傳限制
- 檔案大小限制依 Strapi 設定
- 支援的檔案類型依 Strapi 設定
- multipart/form-data 格式

### 錯誤處理
- 所有方法都會拋出 `InvalidOperationException`
- 需要適當的 try-catch 處理
- HTTP 狀態碼會包含在錯誤訊息中

### 最佳實務
- **檔案驗證** - 在上傳前檢查檔案類型和大小
- **URL 處理** - 在應用層處理 URL 正規化
- **資源管理** - 使用 using 語句管理 Stream
- **錯誤回傳** - 提供有意義的錯誤訊息給前端