# Strapi 整合測試自動化腳本

# =============================================================================
# 函式定義
# =============================================================================

function Clear-TestEnvironment {
    Write-Host "🗑️ 清理測試環境..." -ForegroundColor Yellow
    
    # 清理 Strapi 檔案
    Push-Location "etc\strapi-integration-test"
    Remove-Item ".tmp" -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item ".strapi" -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item ".env" -Force -ErrorAction SilentlyContinue
    Pop-Location
    
    # 清理 C# 測試配置
    Remove-Item "Further.Strapi.Tests\appsettings.json" -Force -ErrorAction SilentlyContinue
    
    # 停止 node 程序
    Get-Process -Name "node" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
    Start-Sleep 2
    
    # 清理測試結果
    Remove-Item "TestResults" -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item "coverage.cobertura.xml" -Force -ErrorAction SilentlyContinue
    
    Write-Host "✅ 清理完成" -ForegroundColor Green
}

function Test-FilesReady {
    Write-Host "🔍 檢查測試檔案是否準備就緒..." -ForegroundColor Yellow
    
    # 檢查 Strapi .env
    $envPath = "etc\strapi-integration-test\.env"
    if (!(Test-Path $envPath)) {
        Write-Host "❌ Strapi .env 不存在: $envPath" -ForegroundColor Red
        return $false
    }
    Write-Host "✅ Strapi .env 存在" -ForegroundColor Green
    
    # 檢查 C# appsettings.json
    $appSettingsPath = "Further.Strapi.Tests\appsettings.json"
    if (!(Test-Path $appSettingsPath)) {
        Write-Host "❌ C# 測試配置不存在: $appSettingsPath" -ForegroundColor Red
        return $false
    }
    
    # 檢查 API Token
    $appSettings = Get-Content $appSettingsPath | ConvertFrom-Json
    if (!$appSettings.Strapi.StrapiToken -or $appSettings.Strapi.StrapiToken.Length -le 10) {
        Write-Host "❌ API Token 無效" -ForegroundColor Red
        return $false
    }
    
    Write-Host "✅ C# 測試配置存在且 API Token 有效" -ForegroundColor Green
    Write-Host "✅ 所有測試檔案準備就緒" -ForegroundColor Green
    return $true
}

function Stop-StrapiProcess {
    param([System.Diagnostics.Process]$StrapiProcess)
    
    Write-Host "🛑 停止 Strapi..." -ForegroundColor Yellow
    
    if ($StrapiProcess -and !$StrapiProcess.HasExited) {
        $StrapiProcess.CloseMainWindow()
        $StrapiProcess.WaitForExit(5000)
        
        if (!$StrapiProcess.HasExited) {
            Stop-Process -Id $StrapiProcess.Id -Force -ErrorAction SilentlyContinue
        }
    }
    
    Get-Process -Name "node" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
    Write-Host "✅ Strapi 已停止" -ForegroundColor Green
}

# =============================================================================
# 主執行邏輯
# =============================================================================

Write-Host "🧪 Strapi 整合測試流程" -ForegroundColor Green

$projectRoot = Split-Path $PSScriptRoot -Parent
Push-Location $projectRoot

try {
    # 1. 清理環境
    Clear-TestEnvironment
    
    # 2. Restore dependencies
    Write-Host "🔧 Restore dependencies..." -ForegroundColor Yellow
    dotnet restore

    # 3. 準備 Strapi 環境
    Write-Host "🟢 準備 Strapi 環境..." -ForegroundColor Yellow
    Push-Location "etc\strapi-integration-test"
    
    # 檢查 node_modules
    if (!(Test-Path "node_modules")) {
        Write-Host "📦 執行 npm install..." -ForegroundColor Yellow
        npm install
    }

    # 建立測試 .env
    Write-Host "🌐 建立測試環境設定..." -ForegroundColor Yellow
    $appKey1 = [System.Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes((New-Guid).ToString()))
    $appKey2 = [System.Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes((New-Guid).ToString()))
    $apiTokenSalt = [System.Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes((New-Guid).ToString()))
    $adminJwtSecret = [System.Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes((New-Guid).ToString()))
    $transferTokenSalt = [System.Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes((New-Guid).ToString()))
    $jwtSecret = [System.Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes((New-Guid).ToString()))

    $envContent = @"
HOST=0.0.0.0
PORT=1337
APP_KEYS=$appKey1,$appKey2
API_TOKEN_SALT=$apiTokenSalt
ADMIN_JWT_SECRET=$adminJwtSecret
TRANSFER_TOKEN_SALT=$transferTokenSalt
JWT_SECRET=$jwtSecret
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
            Write-Host "⏰ 等待中... ($elapsed/$timeout 秒)" -ForegroundColor Gray
        }
    } while ($elapsed -lt $timeout)

    if ($elapsed -ge $timeout) {
        Write-Host "❌ Strapi 啟動超時!" -ForegroundColor Red
        exit 1
    }

    # 執行 CI 設定腳本
    Write-Host "🔑 執行 CI 設定..." -ForegroundColor Yellow
    node scripts/setup-ci.js
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ CI 設定失敗!" -ForegroundColor Red
        exit 1
    }
    Write-Host "✅ CI 設定成功!" -ForegroundColor Green

    Pop-Location

    # 4. Build
    Write-Host "🏗️ Build..." -ForegroundColor Yellow
    dotnet build --no-restore --configuration Release

    # 5. 驗證測試檔案
    $isReady = Test-FilesReady
    if (!$isReady) {
        Write-Host "❌ 測試檔案未準備好，無法執行測試" -ForegroundColor Red
        exit 1
    }

    # 6. 執行測試
    Write-Host "🧪 執行整合測試..." -ForegroundColor Yellow
    
    dotnet test --no-build --configuration Release --verbosity normal `
        --collect:"XPlat Code Coverage" `
        --results-directory:"TestResults" `
        --logger:"trx;LogFileName=test-results.trx" `
        --logger:"html;LogFileName=test-results.html" `
        -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura
    
    # 移動覆蓋率檔案
    $coverageFiles = Get-ChildItem -Path "TestResults" -Filter "coverage.cobertura.xml" -Recurse
    if ($coverageFiles.Count -gt 0) {
        Copy-Item $coverageFiles[0].FullName -Destination "coverage.cobertura.xml"
        Write-Host "✅ 覆蓋率報告已生成: coverage.cobertura.xml" -ForegroundColor Green
    }
    
    $testExitCode = $LASTEXITCODE
    if ($testExitCode -eq 0) {
        Write-Host "✅ 所有測試通過！" -ForegroundColor Green
    } else {
        Write-Host "❌ 測試失敗" -ForegroundColor Red
    }
    
    Stop-StrapiProcess -StrapiProcess $strapiProcess
    
    Write-Host "🎉 測試流程完成！" -ForegroundColor Green
    exit $testExitCode

} finally {
    if ($strapiProcess) {
        Stop-StrapiProcess -StrapiProcess $strapiProcess
    }
    Pop-Location
}