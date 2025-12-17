# Strapi 共用組件管理策略

## 概述

這個文件說明如何在多個 Strapi 專案間管理共用組件，確保組件定義的一致性和同步。

## 核心理念

1. **靜態 JSON 檔案定義**：組件以 JSON 檔案形式定義在 `src/components/{category}/{name}.json`
2. **自動型別產生**：Strapi 啟動時自動掃描 JSON 檔案並產生 TypeScript 定義
3. **啟動前同步**：確保在 Strapi 啟動前所有必要的組件檔案都存在

## 檔案結構

```
Further.Strapi/
├── etc/strapi-integration-test/
│   ├── src/components/shared/
│   │   ├── string-item.json
│   │   ├── media.json
│   │   └── slider.json
│   └── scripts/setup-ci.js
└── sync-shared-components.js

Tourmap.Booking.Strapi/
├── etc/strapi-integration-test/
│   ├── src/components/shared/
│   │   ├── string-item.json
│   │   ├── media.json
│   │   └── slider.json
│   └── scripts/setup-ci.js
```

## 工具和腳本

### 1. setup-ci.js

**位置：**
- `Further.Strapi/etc/strapi-integration-test/scripts/setup-ci.js`
- `Tourmap.Booking.Strapi/etc/strapi-integration-test/scripts/setup-ci.js`

**功能：**
- 在 Strapi 啟動前確保所有共用組件 JSON 檔案存在
- 建立管理員帳號和 API Token
- 設定 API 權限

**使用方式：**
```bash
# Further.Strapi
cd Further.Strapi/etc/strapi-integration-test
node scripts/setup-ci.js Further.Strapi.Tests

# Tourmap.Booking.Strapi
cd Tourmap.Booking.Strapi/etc/strapi-integration-test  
node scripts/setup-ci.js Tourmap.Booking.Strapi.Tests
```

**參數說明：**
- 測試專案目錄名稱為必要參數，不可省略
- 如果不提供參數，腳本會報錯並顯示使用方式

### 2. sync-shared-components.js

**位置：** `Further.Strapi/sync-shared-components.js`

**功能：**
- 同步所有 Strapi 專案的共用組件
- 驗證專案結構
- 列出可用的共用組件

**使用方式：**
```bash
cd Further.Strapi

# 同步組件到所有專案
node sync-shared-components.js sync

# 列出可用的共用組件
node sync-shared-components.js list

# 驗證專案結構
node sync-shared-components.js validate
```

## 共用組件定義

目前支援的共用組件：

### shared.string-item
```json
{
  "collectionName": "components_shared_string_items",
  "info": {
    "displayName": "StringItem"
  },
  "attributes": {
    "value": {
      "type": "string"
    }
  }
}
```

### shared.media
```json
{
  "collectionName": "components_shared_media",
  "info": {
    "displayName": "Media",
    "icon": "file-video"
  },
  "attributes": {
    "file": {
      "type": "media",
      "allowedTypes": ["images", "files", "videos"]
    }
  }
}
```

### shared.slider
```json
{
  "collectionName": "components_shared_sliders",
  "info": {
    "displayName": "Slider",
    "icon": "address-book"
  },
  "attributes": {
    "files": {
      "type": "media",
      "multiple": true,
      "allowedTypes": ["images"]
    }
  }
}
```

## 工作流程

### 開發環境設置

1. **確保組件同步**：
   ```bash
   cd Further.Strapi
   node sync-shared-components.js sync
   ```

2. **啟動 Strapi**：
   ```bash
   cd Further.Strapi/etc/strapi-integration-test
   npm run develop
   ```

3. **Strapi 會自動**：
   - 掃描 `src/components/` 目錄下的所有 JSON 檔案
   - 產生對應的 TypeScript 定義到 `types/generated/components.d.ts`
   - 註冊組件到 ComponentSchemas

### CI/CD 環境

1. **執行 setup-ci.js**：
   ```bash
   # 啟動 Strapi（背景執行）
   npm run develop &
   
   # 執行設置腳本（必須指定測試專案目錄）
   node scripts/setup-ci.js Further.Strapi.Tests
   
   # 或者對於 Tourmap.Booking.Strapi
   node scripts/setup-ci.js Tourmap.Booking.Strapi.Tests
   ```

### 新增共用組件

1. **更新組件定義**：
   編輯 `sync-shared-components.js` 中的 `SHARED_COMPONENTS` 物件

2. **同步到所有專案**：
   ```bash
   node sync-shared-components.js sync
   ```

3. **重新啟動 Strapi** 來產生新的 TypeScript 定義

## 優點

1. **一致性**：所有專案使用相同的組件定義
2. **自動化**：啟動時自動確保組件存在
3. **型別安全**：Strapi 自動產生 TypeScript 定義
4. **版本控制**：組件定義版本化管理
5. **易於維護**：單一真實來源（Single Source of Truth）

## C# 整合

在 C# 專案中，你可以使用對應的組件類別：

```csharp
// Further.Strapi.Shared/Components/SharedComponents.cs
[StrapiComponentName("shared.string-item")]
public class StringItem
{
    public string Value { get; set; } = string.Empty;
}

[StrapiComponentName("shared.media")]
public class Media
{
    public StrapiMediaField? File { get; set; }
}

[StrapiComponentName("shared.slider")]
public class Slider
{
    public List<StrapiMediaField>? Files { get; set; }
}
```

> 注意：`StrapiMediaField` 和 Relation 類型會由 `TypeAwareConverter` 自動識別並轉換，不需要手動標註 `[JsonConverter]` 屬性。

## 故障排除

### 組件定義不同步
```bash
node sync-shared-components.js validate
node sync-shared-components.js sync
```

### TypeScript 定義未更新
```bash
cd {strapi-project}
npx strapi ts:generate-types
# 或重新啟動開發伺服器
npm run develop
```

### 組件無法使用
1. 檢查 JSON 檔案是否存在
2. 檢查 `components.d.ts` 是否包含組件定義
3. 重新啟動 Strapi 開發伺服器