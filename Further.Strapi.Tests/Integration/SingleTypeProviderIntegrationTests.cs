using Further.Strapi.Tests.Models;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System;
using System.Threading.Tasks;
using Volo.Abp.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Further.Strapi.Tests.Integration;

/// <summary>
/// SingleTypeProvider 整合測試 - 測試真實的 Strapi API
/// </summary>
public class SingleTypeProviderIntegrationTests : StrapiRealIntegrationTestBase
{
    private readonly ISingleTypeProvider<Global> _globalProvider;
    private readonly ISingleTypeProvider<About> _aboutProvider;
    private readonly ITestOutputHelper _output;

    public SingleTypeProviderIntegrationTests(ITestOutputHelper output)
    {
        _output = output;
        _globalProvider = GetRequiredService<ISingleTypeProvider<Global>>();
        _aboutProvider = GetRequiredService<ISingleTypeProvider<About>>();
    }

    [Fact]
    public async Task GetAsync_Global_ShouldReturnGlobalSettings()
    {
        // Arrange - 先嘗試初始化 Global，如果不存在的話
        await EnsureGlobalInitializedAsync();

        // Act
        var result = await _globalProvider.GetAsync();

        // Assert
        result.ShouldNotBeNull();
        _output.WriteLine($"Global DocumentId: {result.DocumentId}");
        _output.WriteLine($"SiteName: {result.SiteName}");
        _output.WriteLine($"SiteDescription: {result.SiteDescription}");

        // 檢查必要欄位
        result.DocumentId.ShouldNotBeNullOrWhiteSpace();
        result.SiteName.ShouldNotBeNullOrWhiteSpace();
        result.SiteDescription.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task UpdateAsync_Global_ShouldUpdateSuccessfully()
    {
        // Arrange - 先確保 Global 已初始化
        await EnsureGlobalInitializedAsync();
        
        // 取得目前的資料
        var currentGlobal = await _globalProvider.GetAsync();
        currentGlobal.ShouldNotBeNull();

        var originalSiteName = currentGlobal.SiteName;
        var testSiteName = $"Test Site - {DateTime.Now:yyyy-MM-dd HH:mm:ss}";

        _output.WriteLine($"Original SiteName: {originalSiteName}");
        _output.WriteLine($"New SiteName: {testSiteName}");

        // Act - 使用原始類別進行更新
        currentGlobal.SiteName = testSiteName;
        // 移除可能不存在的 Favicon 設定
        // currentGlobal.Favicon = new StrapiMediaField() { Id = 1 };
        // 保持其他欄位不變

        var updatedDocumentId = await _globalProvider.UpdateAsync(currentGlobal);

        // Assert - 驗證更新結果（需要重新讀取）
        updatedDocumentId.ShouldNotBeNullOrWhiteSpace();
        _output.WriteLine($"Updated DocumentId: {updatedDocumentId}");

        // 重新讀取驗證更新
        var updatedGlobal = await _globalProvider.GetAsync();
        updatedGlobal.ShouldNotBeNull();
        updatedGlobal.SiteName.ShouldBe(testSiteName);
        updatedGlobal.SiteDescription.ShouldBe(currentGlobal.SiteDescription);

        _output.WriteLine($"Updated SiteName: {updatedGlobal.SiteName}");

        // Cleanup - 使用原始類別恢復原本的值
        updatedGlobal.SiteName = originalSiteName;
        // 其他欄位保持現狀

        var restoredDocumentId = await _globalProvider.UpdateAsync(updatedGlobal);
        restoredDocumentId.ShouldNotBeNullOrWhiteSpace();

        // 重新讀取驗證恢復
        var restoredGlobal = await _globalProvider.GetAsync();
        restoredGlobal.SiteName.ShouldBe(originalSiteName);

        _output.WriteLine($"Restored SiteName: {restoredGlobal.SiteName}");
    }

    [Fact]
    public async Task GetAsync_About_ShouldReturnAboutContent()
    {
        // Arrange - 先嘗試初始化 About，如果不存在的話
        await EnsureAboutInitializedAsync();

        // Act
        var result = await _aboutProvider.GetAsync();

        // Assert
        result.ShouldNotBeNull();
        _output.WriteLine($"About DocumentId: {result.DocumentId}");
        _output.WriteLine($"Title: {result.Title ?? "null"}");

        // DocumentId 應該存在
        result.DocumentId.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task UpdateAsync_About_ShouldUpdateSuccessfully()
    {
        // Arrange - 先確保 About 已初始化
        await EnsureAboutInitializedAsync();
        
        // 取得目前的資料
        var currentAbout = await _aboutProvider.GetAsync();
        currentAbout.ShouldNotBeNull();

        var originalTitle = currentAbout.Title;
        var testTitle = $"Test About Title - {DateTime.Now:yyyy-MM-dd HH:mm:ss}";

        _output.WriteLine($"Original Title: {originalTitle ?? "null"}");
        _output.WriteLine($"New Title: {testTitle}");

        // Act - 使用原始類別進行更新
        currentAbout.Title = testTitle;
        // 保持其他欄位不變

        var updatedDocumentId = await _aboutProvider.UpdateAsync(currentAbout);

        // Assert - 驗證更新結果（需要重新讀取）
        updatedDocumentId.ShouldNotBeNullOrWhiteSpace();
        _output.WriteLine($"Updated DocumentId: {updatedDocumentId}");

        // 重新讀取驗證更新
        var updatedAbout = await _aboutProvider.GetAsync();
        updatedAbout.ShouldNotBeNull();
        updatedAbout.Title.ShouldBe(testTitle);

        _output.WriteLine($"Updated Title: {updatedAbout.Title}");

        // Cleanup - 使用原始類別恢復原本的值（如果有的話）
        if (!string.IsNullOrEmpty(originalTitle))
        {
            updatedAbout.Title = originalTitle;
            // 其他欄位保持現狀

            var restoredDocumentId = await _aboutProvider.UpdateAsync(updatedAbout);
            restoredDocumentId.ShouldNotBeNullOrWhiteSpace();

            // 重新讀取驗證恢復
            var restoredAbout = await _aboutProvider.GetAsync();
            restoredAbout.Title.ShouldBe(originalTitle);

            _output.WriteLine($"Restored Title: {restoredAbout.Title}");
        }
    }

    /// <summary>
    /// 確保 Global Single Type 已經初始化
    /// </summary>
    private async Task EnsureGlobalInitializedAsync()
    {
        try
        {
            // 嘗試讀取現有資料
            var existing = await _globalProvider.GetAsync();
            if (existing != null)
            {
                _output.WriteLine("✅ Global 已存在，無需初始化");
                return;
            }
        }
        catch (Exception ex)
        {
            _output.WriteLine($"⚠️ Global 讀取失敗: {ex.Message}，開始初始化...");
        }

        // 如果讀取失敗或不存在，進行初始化
        var initialGlobal = new Global
        {
            SiteName = "Further LiveKit",
            SiteDescription = "一個基於 Strapi 的測試網站"
        };

        try
        {
            var documentId = await _globalProvider.UpdateAsync(initialGlobal);
            _output.WriteLine($"✅ Global 初始化成功，DocumentId: {documentId}");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"❌ Global 初始化失敗: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 確保 About Single Type 已經初始化
    /// </summary>
    private async Task EnsureAboutInitializedAsync()
    {
        try
        {
            // 嘗試讀取現有資料
            var existing = await _aboutProvider.GetAsync();
            if (existing != null)
            {
                _output.WriteLine("✅ About 已存在，無需初始化");
                return;
            }
        }
        catch (Exception ex)
        {
            _output.WriteLine($"⚠️ About 讀取失敗: {ex.Message}，開始初始化...");
        }

        // 如果讀取失敗或不存在，進行初始化
        var initialAbout = new About
        {
            Title = "關於我們",
            // 可以添加其他預設值
        };

        try
        {
            var documentId = await _aboutProvider.UpdateAsync(initialAbout);
            _output.WriteLine($"✅ About 初始化成功，DocumentId: {documentId}");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"❌ About 初始化失敗: {ex.Message}");
            throw;
        }
    }
}