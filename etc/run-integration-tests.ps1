# Strapi 整合測試自動化腳本
# 用於 CI/CD 管道中執行完整的 Strapi 整合測試
Write-Host "🧪 Strapi 整合測試流程" -ForegroundColor Green

# 移動到專案根目錄
$projectRoot = Split-Path $PSScriptRoot -Parent
Push-Location $projectRoot

try {
    # 1. 檢查 .NET
    Write-Host "⚙️ 檢查 .NET..." -ForegroundColor Yellow
    dotnet --version

    # 2. Restore dependencies
    Write-Host "🔧 Restore dependencies..." -ForegroundColor Yellow
    dotnet restore

    # 3. 先準備 Strapi 環境（在 build 之前）
    Write-Host "🟢 準備 Strapi 環境..." -ForegroundColor Yellow
    Push-Location "etc\strapi-integration-test"

    # 清理舊資料庫
    Write-Host "🗑️ 清理舊資料庫..." -ForegroundColor Yellow
    Remove-Item ".tmp" -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item ".strapi" -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item ".env" -Force -ErrorAction SilentlyContinue
    
    # 清理 C# 測試配置檔案
    Write-Host "🗑️ 清理 C# 測試配置..." -ForegroundColor Yellow
    Remove-Item "../../Further.Strapi.Tests/appsettings.json" -Force -ErrorAction SilentlyContinue
    
    # 清理可能的暫存檔案鎖定
    Write-Host "🧹 清理暫存檔案..." -ForegroundColor Yellow
    Get-Process -Name "node" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
    Start-Sleep 2
    
    # 檢查 npm 依賴
    Write-Host "📦 檢查 npm 依賴..." -ForegroundColor Yellow
    if (!(Test-Path "node_modules")) {
        Write-Host "⚠️ 沒有 node_modules，執行 npm install..." -ForegroundColor Red
        npm install
    } else {
        Write-Host "✅ node_modules 存在，跳過安裝" -ForegroundColor Green
    }

    # 建立測試用的 .env
    Write-Host "🌐 建立測試環境設定..." -ForegroundColor Yellow
    $appKey1 = [System.Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes((New-Guid).ToString()))
    $appKey2 = [System.Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes((New-Guid).ToString()))
    $apiTokenSalt = [System.Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes((New-Guid).ToString()))
    $adminJwtSecret = [System.Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes((New-Guid).ToString()))
    $transferTokenSalt = [System.Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes((New-Guid).ToString()))
    $jwtSecret = [System.Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes((New-Guid).ToString()))
    $encryptionKey = [System.Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes((New-Guid).ToString()))

    $envContent = @"
HOST=0.0.0.0
PORT=1337
APP_KEYS=$appKey1,$appKey2
API_TOKEN_SALT=$apiTokenSalt
ADMIN_JWT_SECRET=$adminJwtSecret
TRANSFER_TOKEN_SALT=$transferTokenSalt
JWT_SECRET=$jwtSecret
ENCRYPTION_KEY=$encryptionKey
DATABASE_CLIENT=sqlite
DATABASE_FILENAME=.tmp/data.db
"@
    $envContent | Out-File -FilePath ".env" -Encoding utf8
    Write-Host "✅ .env 檔案已生成" -ForegroundColor Green

    # 啟動 Strapi
    Write-Host "🚀 啟動 Strapi..." -ForegroundColor Yellow
    $strapiProcess = Start-Process -FilePath "npm" -ArgumentList "run", "develop" -PassThru -NoNewWindow
    
    # 等待 Strapi 啟動
    Write-Host "⏰ 等待 Strapi 啟動..." -ForegroundColor Yellow
    $timeout = 60
    $elapsed = 0
    do {
        Start-Sleep 3
        $elapsed += 3
        try {
            $response = Invoke-WebRequest -Uri "http://localhost:1337/" -Method GET -TimeoutSec 5
            if ($response.StatusCode -eq 200 -or $response.StatusCode -eq 302) {
                Write-Host "✅ Strapi 已啟動!" -ForegroundColor Green
                break
            }
        } catch {
            Write-Host "⏰ 還在等待... ($elapsed/$timeout 秒)" -ForegroundColor Gray
        }
    } while ($elapsed -lt $timeout)

    if ($elapsed -ge $timeout) {
        Write-Host "❌ Strapi 啟動超時!" -ForegroundColor Red
        exit 1
    }

    # 執行 CI 設定腳本 (建立管理員和 API Token)
    Write-Host "🔑 執行 CI 設定腳本..." -ForegroundColor Yellow
    try {
        node scripts/setup-ci.js
        if ($LASTEXITCODE -ne 0) {
            Write-Host "❌ CI 設定失敗!" -ForegroundColor Red
            throw "CI setup failed"
        } else {
            Write-Host "✅ CI 設定成功!" -ForegroundColor Green
        }
    } catch {
        Write-Host "❌ CI 設定失敗: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "💡 無法設定測試環境，請檢查 Strapi 啟動狀況和 setup-ci.js 腳本" -ForegroundColor Yellow
        
        # 不要使用無效的預設 token，直接報錯退出
        Write-Host "🛑 沒有有效的 API token，測試無法進行" -ForegroundColor Red
        exit 1
    }

    Pop-Location

    # 4. 現在才 Build（在 appsettings.json 生成之後）
    Write-Host "🏗️ Build（在 API Token 生成後）..." -ForegroundColor Yellow
    dotnet build --no-restore --configuration Release

    # 5. 驗證必要檔案是否存在
    Write-Host "🔍 驗證必要檔案..." -ForegroundColor Yellow
    
    # 檢查 .env 檔案
    $envPath = "etc\strapi-integration-test\.env"
    if (!(Test-Path $envPath)) {
        Write-Host "❌ 找不到 Strapi .env 檔案: $envPath" -ForegroundColor Red
        Write-Host "💡 請確認 CI 設定腳本正確執行並生成了 .env 檔案" -ForegroundColor Yellow
        exit 1
    } else {
        Write-Host "✅ Strapi .env 檔案存在" -ForegroundColor Green
    }
    
    # 檢查 appsettings.json 檔案
    $appSettingsPath = "Further.Strapi.Tests\appsettings.json"
    if (!(Test-Path $appSettingsPath)) {
        Write-Host "❌ 找不到 C# 測試配置檔案: $appSettingsPath" -ForegroundColor Red
        Write-Host "💡 請確認 CI 設定腳本正確執行並生成了 appsettings.json 檔案" -ForegroundColor Yellow
        exit 1
    } else {
        Write-Host "✅ C# 測試配置檔案存在" -ForegroundColor Green
        
        # 驗證 appsettings.json 內容
        try {
            $appSettings = Get-Content $appSettingsPath | ConvertFrom-Json
            if ($appSettings.Strapi.StrapiToken -and $appSettings.Strapi.StrapiToken.Length -gt 10) {
                Write-Host "✅ API Token 已正確設定" -ForegroundColor Green
                Write-Host "🔑 Token 預覽: $($appSettings.Strapi.StrapiToken.Substring(0, 20))..." -ForegroundColor Cyan
            } else {
                Write-Host "❌ API Token 無效或為空" -ForegroundColor Red
                Write-Host "💡 Token 值: $($appSettings.Strapi.StrapiToken)" -ForegroundColor Yellow
                exit 1
            }
        } catch {
            Write-Host "❌ 無法解析 appsettings.json 檔案" -ForegroundColor Red
            Write-Host "💡 錯誤: $($_.Exception.Message)" -ForegroundColor Yellow
            exit 1
        }
    }

    Write-Host "🎯 設定完成！開始執行測試" -ForegroundColor Green
    Write-Host "💡 Strapi 正在 http://localhost:1337 運行，API Token 已設定完成" -ForegroundColor Yellow
    
    # 執行測試
    Write-Host "🧪 執行整合測試..." -ForegroundColor Yellow
    
    if ($env:ENABLE_COVERAGE -eq "true") {
        Write-Host "📊 啟用覆蓋率收集..." -ForegroundColor Cyan
        dotnet test --no-build --configuration Release --verbosity normal `
            --collect:"XPlat Code Coverage" `
            --results-directory:"TestResults" `
            --logger:"trx;LogFileName=test-results.trx" `
            --logger:"html;LogFileName=test-results.html" `
            -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura
        
        # 移動覆蓋率檔案到根目錄
        $coverageFiles = Get-ChildItem -Path "TestResults" -Filter "coverage.cobertura.xml" -Recurse
        if ($coverageFiles.Count -gt 0) {
            Copy-Item $coverageFiles[0].FullName -Destination "coverage.cobertura.xml"
            Write-Host "✅ 覆蓋率報告已生成: coverage.cobertura.xml" -ForegroundColor Green
            
            # 顯示覆蓋率檔案路徑
            Write-Host "📁 測試結果檔案位置:" -ForegroundColor Cyan
            Write-Host "   - TestResults/" -ForegroundColor Gray
            Write-Host "   - coverage.cobertura.xml" -ForegroundColor Gray
        } else {
            Write-Host "⚠️ 未找到覆蓋率檔案" -ForegroundColor Yellow
        }
    } else {
        dotnet test --no-build --configuration Release --verbosity normal `
            --logger:"trx;LogFileName=test-results.trx" `
            --logger:"html;LogFileName=test-results.html" `
            --results-directory:"TestResults"
        
        Write-Host "📁 測試結果檔案位置:" -ForegroundColor Cyan
        Write-Host "   - TestResults/" -ForegroundColor Gray
    }
    
    $testExitCode = $LASTEXITCODE
    if ($testExitCode -eq 0) {
        Write-Host "✅ 所有測試都通過！" -ForegroundColor Green
    } else {
        Write-Host "❌ 測試失敗，退出代碼: $testExitCode" -ForegroundColor Red
    }
    
    # 自動停止 Strapi
    Write-Host "🛑 測試完成，正在停止 Strapi..." -ForegroundColor Yellow
    if ($strapiProcess -and !$strapiProcess.HasExited) {
        # 嘗試優雅停止
        try {
            $strapiProcess.CloseMainWindow()
            $strapiProcess.WaitForExit(5000)  # 等待 5 秒
        } catch {
            Write-Host "⚠️  優雅停止失敗，使用強制停止" -ForegroundColor Yellow
        }
        
        # 如果還沒停止，強制終止
        if (!$strapiProcess.HasExited) {
            Stop-Process -Id $strapiProcess.Id -Force -ErrorAction SilentlyContinue
            Start-Sleep 2
        }
        
        Write-Host "✅ Strapi 已停止" -ForegroundColor Green
    }
    
    # 清理可能殘留的 node 程序
    Write-Host "🧹 清理可能殘留的 node 程序..." -ForegroundColor Yellow
    Get-Process -Name "node" -ErrorAction SilentlyContinue | Where-Object { $_.ProcessName -eq "node" } | Stop-Process -Force -ErrorAction SilentlyContinue
    
    Write-Host "🎉 CI 測試流程完成！" -ForegroundColor Green
    exit $testExitCode

} finally {
    # 清理：停止 Strapi 程序
    Write-Host "`n🛑 正在停止 Strapi..." -ForegroundColor Yellow
    
    if ($strapiProcess -and !$strapiProcess.HasExited) {
        # 嘗試優雅停止
        try {
            $strapiProcess.CloseMainWindow()
            $strapiProcess.WaitForExit(5000)  # 等待 5 秒
        } catch {
            Write-Host "⚠️  優雅停止失敗，使用強制停止" -ForegroundColor Yellow
        }
        
        # 如果還沒停止，強制終止
        if (!$strapiProcess.HasExited) {
            Stop-Process -Id $strapiProcess.Id -Force -ErrorAction SilentlyContinue
            Start-Sleep 2
        }
        
        Write-Host "✅ Strapi 已停止" -ForegroundColor Green
    } else {
        Write-Host "ℹ️  Strapi 程序已經停止" -ForegroundColor Gray
    }
    
    # 確保所有 node 程序都停止（以防萬一）
    Write-Host "🧹 清理可能殘留的 node 程序..." -ForegroundColor Yellow
    Get-Process -Name "node" -ErrorAction SilentlyContinue | Where-Object { $_.ProcessName -eq "node" } | Stop-Process -Force -ErrorAction SilentlyContinue
    
    Pop-Location
}