# Further.Strapi 發布指南

## 🚀 如何發布新版本

### 1. 更新版本號
編輯 `common.props` 文件中的版本號：
```xml
<Version>1.0.1</Version>
```

### 2. 自動發布
將更改推送到 `main` 分支：
```bash
git add common.props
git commit -m "chore: bump version to 1.0.1"
git push origin main
```

### 3. 發布流程
當 `common.props` 檔案變更並推送到 `main` 分支時，GitHub Actions 會自動：

1. ✅ **檢查版本標籤**：確認版本是否已存在
2. 🧪 **執行測試**：確保所有測試通過
3. 📦 **打包**：創建 NuGet 包
4. 🧹 **清理**：移除測試和主機包
5. 📤 **發布到 GitHub Packages**：內部使用
6. 🌍 **發布到 NuGet.org**：公開發布
7. 🏷️ **創建 Git 標籤**：版本管理
8. 📋 **創建 GitHub Release**：發布說明

## 🔧 必需的 Secrets

在 GitHub 倉庫 `yinchang0626/Further.Strapi` 設定中添加以下 secrets：

- `NUGET_API_KEY`: NuGet.org 的 API 金鑰
- `GITHUB_TOKEN`: 自動提供，用於 GitHub Packages

## 📦 發布的包

- `Further.Strapi` - 核心功能
- `Further.Strapi.Contracts` - 契約和介面  
- `Further.Strapi.Shared` - 共用工具

## 🔍 手動觸發

也可以在 GitHub Actions 頁面手動觸發發布：
1. 前往 https://github.com/yinchang0626/Further.Strapi/actions
2. 選擇 "Publish to NuGet" workflow
3. 點擊 "Run workflow"

## 📝 版本號建議

遵循 [語義化版本](https://semver.org/lang/zh-TW/)：
- `1.0.0` - 主要版本（不向下相容的變更）
- `1.1.0` - 次要版本（新功能，向下相容）
- `1.0.1` - 修訂版本（Bug 修復，向下相容）