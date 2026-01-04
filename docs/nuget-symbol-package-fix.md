# NuGet 符號套件校驗和錯誤修復

## 問題描述

在發布 Further.Strapi 0.0.8 版本時，遇到以下錯誤：

```
The symbol package Further.Strapi 0.0.8 failed validation because of the following reason(s):

The checksum does not match for the dll(s) and corresponding pdb(s).
Your symbol package was not published on NuGet Gallery and is not available for consumption.
```

## 根本原因

這個錯誤表示 DLL 檔案和對應的 PDB (Program Database) 檔案之間的校驗和不匹配。主要原因包括：

1. **缺少確定性建置設定** - 沒有啟用 `Deterministic` 和 `ContinuousIntegrationBuild`
2. **PDB 生成設定不正確** - 沒有使用正確的 `DebugType` 設定
3. **缺少 Source Link 支援** - 沒有包含 Source Link 套件以正確連結原始碼

## 解決方案

在 `common.props` 中新增以下關鍵設定：

### 1. 確定性建置設定

```xml
<!-- Deterministic build settings for reproducible builds -->
<Deterministic>true</Deterministic>
<ContinuousIntegrationBuild Condition="'$(CI)' == 'true' or '$(GITHUB_ACTIONS)' == 'true'">true</ContinuousIntegrationBuild>
```

**說明：**
- `Deterministic=true`: 確保相同的原始碼每次建置都產生完全相同的二進位檔案
- `ContinuousIntegrationBuild=true`: 在 CI 環境中自動啟用，確保建置的可重現性

### 2. PDB 生成設定

```xml
<!-- Debug information settings for proper PDB generation -->
<DebugType>embedded</DebugType>
<DebugSymbols>true</DebugSymbols>
<EmbedUntrackedSources>true</EmbedUntrackedSources>
```

**說明：**
- `DebugType=embedded`: 將 PDB 資訊嵌入到 DLL 中，確保一致性
- `DebugSymbols=true`: 生成偵錯符號
- `EmbedUntrackedSources=true`: 將未追蹤的原始碼檔案嵌入 PDB 中

### 3. Source Link 支援

```xml
<!-- Publishing options -->
<PublishRepositoryUrl>true</PublishRepositoryUrl>
```

並新增套件參考：

```xml
<!-- Source Link support for proper symbol packages -->
<PackageReference Include="Microsoft.SourceLink.GitHub" Version="8.0.0" PrivateAssets="All" />
```

**說明：**
- `PublishRepositoryUrl=true`: 將儲存庫 URL 發布到套件中
- `Microsoft.SourceLink.GitHub`: 啟用 GitHub Source Link 支援，讓偵錯器可以直接連結到 GitHub 上的原始碼

## 驗證步驟

修復後，請執行以下步驟驗證：

1. **清理建置輸出**
   ```powershell
   dotnet clean
   Remove-Item -Path .\shipping -Recurse -Force -ErrorAction SilentlyContinue
   ```

2. **還原套件**
   ```powershell
   dotnet restore
   ```

3. **建置專案**
   ```powershell
   dotnet build --configuration Release
   ```

4. **打包**
   ```powershell
   dotnet pack --no-build --configuration Release --output ./shipping/
   ```

5. **檢查符號套件**
   - 確認 `./shipping/` 目錄中有 `.snupkg` 檔案
   - 使用 NuGet Package Explorer 檢查套件內容

6. **測試發布**
   ```powershell
   dotnet nuget push ./shipping/Further.Strapi.0.0.9.nupkg --source https://api.nuget.org/v3/index.json --api-key YOUR_API_KEY
   dotnet nuget push ./shipping/Further.Strapi.0.0.9.snupkg --source https://api.nuget.org/v3/index.json --api-key YOUR_API_KEY
   ```

## 後續步驟

1. **升級版本**: 將 `common.props` 中的 `<Version>` 從 `0.0.8` 升級到 `0.0.9`
2. **提交變更**: 提交這些設定變更到 Git
3. **推送到 main**: 這將觸發 GitHub Actions 自動發布流程
4. **監控發布**: 檢查 GitHub Actions 日誌確認發布成功

## 預期結果

套用這些修復後：
- ✅ DLL 和 PDB 的校驗和將會匹配
- ✅ 符號套件將成功通過 NuGet.org 驗證
- ✅ 開發者可以在偵錯時直接連結到 GitHub 原始碼
- ✅ 建置結果將是確定性的和可重現的

## 相關資源

- [Microsoft Docs: Deterministic builds](https://learn.microsoft.com/en-us/dotnet/core/project-sdk/msbuild-props#deterministic)
- [Source Link documentation](https://github.com/dotnet/sourcelink)
- [NuGet symbol packages](https://learn.microsoft.com/en-us/nuget/create-packages/symbol-packages-snupkg)

---
*修復日期: 2025年12月22日*
