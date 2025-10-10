# GitHub Actions 內建變數說明

## 🔍 GitHub Actions Context 變數

在 GitHub Actions 中，我們可以使用多種內建變數來控制 workflow 的執行：

### 📝 Commit Message 相關

```yaml
# 取得 commit message
${{ github.event.head_commit.message }}

# 檢查 commit message 是否包含特定文字
contains(github.event.head_commit.message, '[integration]')

# 其他 commit 資訊
${{ github.event.head_commit.author.name }}
${{ github.event.head_commit.author.email }}
${{ github.event.head_commit.id }}
${{ github.event.head_commit.timestamp }}
```

### 🚀 觸發事件相關

```yaml
# 觸發事件類型
${{ github.event_name }}
# 可能值: push, pull_request, workflow_dispatch, schedule 等

# 分支資訊
${{ github.ref }}        # refs/heads/main
${{ github.ref_name }}   # main
${{ github.base_ref }}   # PR 的目標分支
${{ github.head_ref }}   # PR 的來源分支
```

### 🎛️ 手動觸發輸入

```yaml
# workflow_dispatch 的輸入參數
${{ github.event.inputs.parameter_name }}

# 範例
inputs:
  run_integration_tests:
    description: 'Run integration tests'
    required: false
    default: false
    type: boolean

# 在 job 中使用
if: github.event.inputs.run_integration_tests == 'true'
```

### 📋 Repository 資訊

```yaml
${{ github.repository }}        # owner/repo-name
${{ github.repository_owner }}  # owner
${{ github.workspace }}         # workspace 路徑
${{ github.sha }}              # commit SHA
${{ github.actor }}            # 觸發者
```

## 💡 實際應用範例

### 1. 條件執行整合測試

```yaml
integration-test:
  if: |
    github.event.inputs.run_integration_tests == 'true' || 
    contains(github.event.head_commit.message, '[integration]') ||
    github.ref == 'refs/heads/main'
```

### 2. 根據變更的檔案決定執行

```yaml
test-frontend:
  if: contains(github.event.head_commit.message, 'frontend') || 
      contains(github.event.head_commit.modified, '*.js')
```

### 3. 根據分支執行不同的 job

```yaml
deploy-staging:
  if: github.ref == 'refs/heads/develop'
  
deploy-production:  
  if: github.ref == 'refs/heads/main'
```

### 4. 根據 PR 標籤執行

```yaml
security-scan:
  if: contains(github.event.pull_request.labels.*.name, 'security')
```

## 🔧 我們的實現

在我們的 CI workflow 中：

```yaml
integration-test:
  runs-on: ubuntu-latest
  needs: build
  # 兩個條件的 OR 邏輯
  if: |
    github.event.inputs.run_integration_tests == 'true' || 
    contains(github.event.head_commit.message, '[integration]')
```

### 條件說明：
1. `github.event.inputs.run_integration_tests == 'true'`
   - 手動觸發時選擇執行整合測試
   
2. `contains(github.event.head_commit.message, '[integration]')`
   - commit message 包含 `[integration]` 字串

### 觸發邏輯：
- ✅ 手動觸發 + 勾選執行整合測試
- ✅ commit message 包含 `[integration]`
- ❌ 一般的 push (只執行單元測試)
- ❌ 手動觸發但未勾選整合測試

## 🎯 最佳實踐

1. **使用明確的標籤**: `[integration]`, `[deploy]`, `[skip-ci]`
2. **組合多個條件**: 使用 `||` 和 `&&` 邏輯運算子
3. **考慮效能**: 避免不必要的長時間執行
4. **文件化**: 清楚說明觸發條件和用法