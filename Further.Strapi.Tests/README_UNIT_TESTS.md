# Further.Strapi 單元測試指南

## 🎯 測試目標

對 `StrapiServiceCollectionExtensions.cs` 進行全面的單元測試，確保所有擴展方法和配置功能正常運作。

## 📋 需要驗證的核心功能

### 1. **服務註冊功能**
- ✅ `AddStrapi()` 無參數調用
- ✅ `AddStrapi(builder)` 有參數調用
- ✅ HttpClient 正確註冊
- ✅ IHttpClientFactory 可正常取得

### 2. **配置覆蓋策略**
- ✅ 重複調用 `AddStrapi` 的覆蓋行為
- ✅ 後面的配置覆蓋前面的配置
- ✅ Options 累加式更新
- ✅ HttpClient 完全覆蓋

### 3. **StrapiServiceBuilder 功能**
- ✅ `ConfigureOptions()` 選項配置
- ✅ `ConfigureHttpClient()` 客戶端配置
- ✅ `ConfigureHttpClientBuilder()` 建構器配置
- ✅ Action 集合正確管理

### 4. **HttpClient 配置驗證**
- ✅ BaseAddress 正確設定
- ✅ Authorization Header 正確設定
- ✅ Accept Header 正確設定
- ✅ 自訂配置正確套用

### 5. **錯誤處理和邊界情況**
- ✅ Null 參數處理
- ✅ 空字串 Token 處理
- ✅ 無效配置的容錯處理

## 🧪 測試文件結構

```
Further.Strapi.Tests/
├── Extensions/
│   └── StrapiServiceCollectionExtensionsTests.cs  # 主要擴展方法測試
├── Options/
│   └── StrapiOptionsTests.cs                      # 配置選項測試
├── Protocol/
│   └── StrapiProtocolTests.cs                     # 協定工具測試
└── Further.Strapi.Tests.csproj                    # 測試項目檔案
```

## 🔧 測試套件和工具

- **測試框架**: xUnit 2.9.3
- **斷言庫**: Shouldly 4.2.1
- **模擬工具**: NSubstitute 5.3.0
- **測試執行器**: Visual Studio Test Runner
- **覆蓋率**: 目標 90%+ 代碼覆蓋率

## 📝 關鍵測試案例

### 1. 基本註冊測試
```csharp
[Fact]
public void AddStrapi_WithoutConfiguration_ShouldRegisterHttpClient()
{
    // 驗證無參數調用是否正確註冊 HttpClient
}
```

### 2. 配置覆蓋測試
```csharp
[Fact]
public void AddStrapi_CalledMultipleTimes_ShouldAllowOverride()
{
    // 驗證多次調用的覆蓋策略
}
```

### 3. HttpClient 配置測試
```csharp
[Fact]
public void AddStrapi_HttpClient_ShouldHaveCorrectBaseAddressAndHeaders()
{
    // 驗證 HttpClient 的配置是否正確套用
}
```

### 4. Builder 功能測試
```csharp
[Fact]
public void AddStrapi_WithHttpClientConfiguration_ShouldApplyCustomConfiguration()
{
    // 驗證 Builder 模式的配置功能
}
```

## ⚡ 執行測試

### Visual Studio
```
測試 → 執行所有測試
```

### 命令列
```bash
cd C:\Works\Further.LiveKit\modules\Further.Strapi\Further.Strapi.Tests
dotnet test
```

### 覆蓋率報告
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## 🎯 測試重點檢查清單

### StrapiServiceCollectionExtensions
- [ ] ✅ 基本 HttpClient 註冊
- [ ] ✅ Options 配置正確套用  
- [ ] ✅ 自訂 HttpClient 配置正確執行
- [ ] ✅ HttpClientBuilder 配置正確套用
- [ ] ✅ 重複調用的覆蓋行為
- [ ] ✅ Null 參數容錯處理
- [ ] ✅ 認證 Header 正確設定
- [ ] ✅ Accept Header 正確設定

### StrapiServiceBuilder
- [ ] ✅ ConfigureOptions 正確註冊
- [ ] ✅ ConfigureHttpClient Action 正確收集
- [ ] ✅ ConfigureHttpClientBuilder Action 正確收集
- [ ] ✅ GetHttpClientActions 返回正確集合
- [ ] ✅ GetHttpClientBuilderActions 返回正確集合

### StrapiOptions
- [ ] ✅ 預設值正確
- [ ] ✅ 屬性設定正確保存
- [ ] ✅ HttpClientName 常數正確
- [ ] ✅ URL 格式驗證
- [ ] ✅ Token 格式驗證

### StrapiProtocol (基礎測試)
- [ ] ✅ 路徑建構正確
- [ ] ✅ 查詢參數建構正確  
- [ ] ✅ 請求序列化正確
- [ ] ✅ Attribute 名稱解析正確

## 🚀 下一步：整合測試

完成單元測試後，準備進行：

1. **整合測試**: 測試與實際 Strapi 服務的互動
2. **端到端測試**: 完整的 CRUD 流程測試
3. **效能測試**: HttpClient 池化和效能驗證
4. **負載測試**: 併發請求處理能力

## 📊 預期測試結果

- **測試覆蓋率**: 目標 >90%
- **測試案例**: 約 15-20 個單元測試
- **執行時間**: < 5 秒
- **通過率**: 100%

這些測試將確保 `StrapiServiceCollectionExtensions` 的所有功能都能正確運作，為後續的整合測試奠定堅實基礎。