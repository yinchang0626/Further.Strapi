# Strapi API REST Client 測試

這個資料夾包含用於測試 Strapi API 的 `.http` 檔案，可以在 VS Code 中使用 REST Client 擴充功能直接執行。

## 檔案說明

### 1. `strapi-api-tests.http`
- 全面的 API 測試套件
- 包含所有主要端點測試
- 測試權限和錯誤情況

### 2. `single-type-tests.http`
- 專門針對 Single Type 的測試
- 重點測試 Global 和 About 類型
- 包含 CRUD 操作測試

### 3. `strapi-diagnostics.http`
- 診斷和除錯用的快速測試
- 檢查 Strapi 健康狀態
- 測試不同的路徑格式

## 使用方式

1. 安裝 VS Code 的 "REST Client" 擴充功能
2. 開啟任一 `.http` 檔案
3. 點擊 "Send Request" 或使用快捷鍵 `Ctrl+Alt+R`
4. 查看回應結果

## 設定

### API Token
```
@apiToken = bd8cdd66daecf5db8dbdfbeccbbf4e4adba0a38834f72a66700e31b1ad5864051a3ede3c0f028aae7621ef3077cc2226ed6e61e8c68c80f58d53d70d2ac64f8c401ca0c004378a8d62480ea78190eb9c505571c1b538f659a4a06e8c39e5a1c39ede18dfb600b11511b4f61c84b9fbe92e77a90340122a79e98d8ef9915fb5f1
```

### Base URL
```
@baseUrl = http://localhost:1337
```

## 常見問題排除

### 404 錯誤
1. 檢查 Content Type 是否正確建立
2. 確認 API 端點權限設定
3. 驗證 Content Type 名稱和路徑

### 403 權限錯誤
1. 檢查 API Token 權限
2. 確認 Public 角色權限設定
3. 驗證 Content Type 的存取權限

### 500 伺服器錯誤
1. 檢查 Strapi 伺服器 Console 輸出
2. 確認資料庫連線狀態
3. 檢查 Content Type 配置

## 測試順序建議

1. 先執行 `strapi-diagnostics.http` 中的健康檢查
2. 測試基本的 API 連線
3. 再執行 `single-type-tests.http` 中的具體測試
4. 最後執行完整的 `strapi-api-tests.http` 測試套件

## 預期結果

### 成功的 GET 請求應該返回：
```json
{
  "data": {
    "documentId": "...",
    "siteName": "...",
    "siteDescription": "...",
    "createdAt": "...",
    "updatedAt": "...",
    "publishedAt": "..."
  },
  "meta": {}
}
```

### 成功的 PUT 請求應該返回更新後的 documentId