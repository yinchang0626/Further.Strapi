using Further.Strapi.Tests.Models;
using Shouldly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Further.Strapi.Tests.Integration;

/// <summary>
/// Media Library Provider 多檔上傳整合測試
/// 測試複雜的多檔案上傳、批次處理和關聯功能
/// 移植自 Tourmap.Booking.Document.Tests.Addition.AdditionIntegrationTest
/// </summary>
public class MediaLibraryMultiFileTests : StrapiRealIntegrationTestBase
{
    private readonly IMediaLibraryProvider _mediaLibraryProvider;
    private readonly ICollectionTypeProvider<Article> _articleProvider;
    private readonly ITestOutputHelper _output;

    public MediaLibraryMultiFileTests(ITestOutputHelper output)
    {
        _output = output;
        _mediaLibraryProvider = GetRequiredService<IMediaLibraryProvider>();
        _articleProvider = GetRequiredService<ICollectionTypeProvider<Article>>();
    }

    [Fact]
    public async Task UploadMultipleFiles_ShouldSucceed()
    {
        // Arrange - 準備多個測試檔案
        var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        var uploadedFileIds = new List<int>();
        
        var fileUploads = new List<FileUploadRequest>
        {
            new FileUploadRequest
            {
                FileName = $"test-multi-1-{timestamp}.jpg",
                ContentType = "image/jpeg",
                FileStream = new MemoryStream(CreateTestJpegBytes()),
                AlternativeText = $"多檔案測試 1 {timestamp}",
                Caption = $"測試多檔案上傳 1 - {timestamp}"
            },
            new FileUploadRequest
            {
                FileName = $"test-multi-2-{timestamp}.jpg",
                ContentType = "image/jpeg",
                FileStream = new MemoryStream(CreateTestJpegBytes()),
                AlternativeText = $"多檔案測試 2 {timestamp}",
                Caption = $"測試多檔案上傳 2 - {timestamp}"
            },
            new FileUploadRequest
            {
                FileName = $"test-multi-3-{timestamp}.jpg",
                ContentType = "image/jpeg",
                FileStream = new MemoryStream(CreateTestJpegBytes()),
                AlternativeText = $"多檔案測試 3 {timestamp}",
                Caption = $"測試多檔案上傳 3 - {timestamp}"
            }
        };
        
        try
        {
            // Act - 批次上傳多個檔案到媒體庫
            _output.WriteLine($"開始批次上傳 {fileUploads.Count} 個檔案...");
            var uploadResults = await _mediaLibraryProvider.UploadMultipleAsync(fileUploads);
            
            // Assert
            uploadResults.ShouldNotBeNull();
            uploadResults.Count.ShouldBe(3);
            
            foreach (var uploadResult in uploadResults)
            {
                uploadResult.Id.ShouldBeGreaterThan(0);
                uploadResult.Name.ShouldContain("test-multi");
                uploadResult.Mime.ShouldBe("image/jpeg");
                uploadResult.Url.ShouldNotBeNullOrEmpty();
                uploadResult.AlternativeText.ShouldNotBeNullOrEmpty();
                uploadResult.Caption.ShouldNotBeNullOrEmpty();
                
                uploadedFileIds.Add(uploadResult.Id);
            }
            
            _output.WriteLine($"✅ 多檔案批次上傳成功，共 {uploadResults.Count} 個檔案");
            for (int i = 0; i < uploadResults.Count; i++)
            {
                var result = uploadResults[i];
                _output.WriteLine($"   檔案 {i + 1} - ID: {result.Id}, 名稱: {result.Name}");
                _output.WriteLine($"            AlternativeText: '{result.AlternativeText}'");
                _output.WriteLine($"            Caption: '{result.Caption}'");
            }
        }
        finally
        {
            // Cleanup
            foreach (var fileId in uploadedFileIds)
            {
                try
                {
                    await _mediaLibraryProvider.DeleteAsync(fileId);
                    _output.WriteLine($"🗑️ 已清理檔案 ID: {fileId}");
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"⚠️ 清理檔案 {fileId} 時發生錯誤: {ex.Message}");
                }
            }
        }
    }

    [Fact]
    public async Task UploadFiles_ThenAssociateWithArticle_ShouldSucceed()
    {
        // Arrange - 建立 Article 測試用的實體資料
        var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        var testArticle = new Article
        {
            Title = $"測試檔案關聯的文章 {timestamp}",
            Description = "測試兩步驟檔案關聯流程",
            Slug = $"test-file-association-{timestamp}"
        };

        string documentId = null;
        var uploadedFileIds = new List<int>();
        
        try
        {
            // Step 1: 建立 Article 實體
            documentId = await _articleProvider.CreateAsync(testArticle);
            documentId.ShouldNotBeNullOrEmpty();
            _output.WriteLine($"✅ Step 1 - Article 建立成功，DocumentId: {documentId}");
            
            // Step 2: 上傳檔案到媒體庫（純檔案上傳，不自動關聯）
            var uploadResult = await _mediaLibraryProvider.UploadAsync(new FileUploadRequest
            {
                FileName = $"article-cover-{timestamp}.jpg",
                ContentType = "image/jpeg",
                FileStream = new MemoryStream(CreateTestJpegBytes()),
                AlternativeText = $"Article 封面圖片 {timestamp}",
                Caption = $"兩步驟上傳測試 - {timestamp}"
            });
            
            uploadResult.ShouldNotBeNull();
            uploadedFileIds.Add(uploadResult.Id);
            _output.WriteLine($"✅ Step 2 - 檔案上傳成功，ID: {uploadResult.Id}");
            
            // Step 3: 手動關聯檔案到 Article 實體
            // ⚠️ 重要：必須先從 Strapi 讀取完整資料，再修改特定欄位，避免覆蓋其他欄位
            var existingArticle = await _articleProvider.GetAsync(documentId);
            existingArticle.ShouldNotBeNull();
            
            // 修改 Cover 欄位，序列化時會自動將 StrapiMediaField 轉換成 ID
            existingArticle.Cover = uploadResult;
            
            var updateResult = await _articleProvider.UpdateAsync(documentId, existingArticle);
            updateResult.ShouldBe(documentId);
            
            // 重新取得更新後的 Article 資料（GetAsync 會自動 populate）
            var updatedArticle = await _articleProvider.GetAsync(documentId);
            updatedArticle.ShouldNotBeNull();
            updatedArticle.Cover.ShouldNotBeNull();
            updatedArticle.Cover.Id.ShouldBe(uploadResult.Id);
            
            _output.WriteLine($"✅ Step 3 - Article 檔案關聯成功");
            _output.WriteLine($"   兩步驟流程完成");
            _output.WriteLine($"   檔案 ID: {updatedArticle.Cover.Id}");
            _output.WriteLine($"   檔案名稱: {updatedArticle.Cover.Name}");
        }
        finally
        {
            // Cleanup
            if (!string.IsNullOrEmpty(documentId))
            {
                await _articleProvider.DeleteAsync(documentId);
                _output.WriteLine($"🗑️ 已清理 Article DocumentId: {documentId}");
            }
            
            foreach (var fileId in uploadedFileIds)
            {
                try
                {
                    await _mediaLibraryProvider.DeleteAsync(fileId);
                    _output.WriteLine($"🗑️ 已清理檔案 ID: {fileId}");
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"⚠️ 清理檔案時發生錯誤: {ex.Message}");
                }
            }
        }
    }

    [Fact]
    public async Task UploadFiles_ThenAssociateCoverWithArticle_ShouldSucceed()
    {
        // Arrange - 測試檔案上傳後關聯到 Article Cover（因為 Article 沒有 Gallery 欄位）
        var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        var testArticle = new Article
        {
            Title = $"測試 Cover 檔案關聯 {timestamp}",
            Description = "測試檔案關聯功能（使用 Cover 欄位）",
            Slug = $"test-cover-files-{timestamp}"
        };

        string documentId = null;
        var uploadedFileIds = new List<int>();
        
        try
        {
            // Step 1: 建立 Article 實體
            documentId = await _articleProvider.CreateAsync(testArticle);
            documentId.ShouldNotBeNullOrEmpty();
            _output.WriteLine($"✅ Step 1 - Article 建立成功，DocumentId: {documentId}");
            
            // Step 2: 上傳多個檔案到媒體庫（測試批次上傳功能）
            var bannerUploads = new List<FileUploadRequest>
            {
                new FileUploadRequest
                {
                    FileName = $"banner-1-{timestamp}.jpg",
                    ContentType = "image/jpeg",
                    FileStream = new MemoryStream(CreateTestJpegBytes()),
                    AlternativeText = $"Banner 圖片 1 {timestamp}",
                    Caption = $"Banner 輪播圖 1 - {timestamp}"
                },
                new FileUploadRequest
                {
                    FileName = $"banner-2-{timestamp}.jpg",
                    ContentType = "image/jpeg",
                    FileStream = new MemoryStream(CreateTestJpegBytes()),
                    AlternativeText = $"Banner 圖片 2 {timestamp}",
                    Caption = $"Banner 輪播圖 2 - {timestamp}"
                },
                new FileUploadRequest
                {
                    FileName = $"banner-3-{timestamp}.jpg",
                    ContentType = "image/jpeg",
                    FileStream = new MemoryStream(CreateTestJpegBytes()),
                    AlternativeText = $"Banner 圖片 3 {timestamp}",
                    Caption = $"Banner 輪播圖 3 - {timestamp}"
                }
            };
            
            var uploadResults = await _mediaLibraryProvider.UploadMultipleAsync(bannerUploads);
            uploadResults.ShouldNotBeNull();
            uploadResults.Count.ShouldBe(3);
            uploadedFileIds.AddRange(uploadResults.Select(r => r.Id));
            
            _output.WriteLine($"✅ Step 2 - 檔案批次上傳成功，共 {uploadResults.Count} 個檔案");
            for (int i = 0; i < uploadResults.Count; i++)
            {
                _output.WriteLine($"   檔案 {i + 1} - ID: {uploadResults[i].Id}, 名稱: {uploadResults[i].Name}");
            }
            
            // Step 3: 手動關聯第一個檔案到 Article Cover（因為 Article 沒有 Gallery 欄位）
            var existingArticle = await _articleProvider.GetAsync(documentId);
            existingArticle.ShouldNotBeNull();
            
            // 改用 blocks 進行多檔案測試
            // 由於 Article 沒有 Gallery 欄位，此測試改為驗證單一 Cover 欄位
            existingArticle.Cover = uploadResults.First();
            
            var updateResult = await _articleProvider.UpdateAsync(documentId, existingArticle);
            updateResult.ShouldBe(documentId);
            
            // Step 4: 驗證 Cover 關聯成功（改為單檔案測試）
            var updatedArticle = await _articleProvider.GetAsync(documentId);
            updatedArticle.ShouldNotBeNull();
            updatedArticle.Cover.ShouldNotBeNull();
            updatedArticle.Cover.Id.ShouldBe(uploadResults.First().Id);
            updatedArticle.Cover.Name.ShouldContain("banner-1");
            updatedArticle.Cover.Mime.ShouldBe("image/jpeg");
            
            _output.WriteLine($"✅ Cover 驗證成功 - ID: {updatedArticle.Cover.Id}, 名稱: {updatedArticle.Cover.Name}");
            _output.WriteLine($"✅ Step 3 - 檔案關聯成功（使用 Cover 欄位）");
            _output.WriteLine($"   注意：Article 沒有 Gallery 欄位，改用 Cover 進行測試");
        }
        finally
        {
            // Cleanup
            if (!string.IsNullOrEmpty(documentId))
            {
                await _articleProvider.DeleteAsync(documentId);
                _output.WriteLine($"🗑️ 已清理 Article DocumentId: {documentId}");
            }
            
            foreach (var fileId in uploadedFileIds)
            {
                try
                {
                    await _mediaLibraryProvider.DeleteAsync(fileId);
                    _output.WriteLine($"🗑️ 已清理 Banner 檔案 ID: {fileId}");
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"⚠️ 清理 Banner 檔案時發生錯誤: {ex.Message}");
                }
            }
        }
    }

    [Fact]
    public async Task CreateArticle_WithMixedFilesSpecification_ShouldSucceed()
    {
        // Arrange - 測試同時指定多種檔案類型的情況 [,,,,,]
        var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        var testArticle = new Article
        {
            Title = $"測試混合檔案指定功能 {timestamp}",
            Description = "測試混合檔案指定功能 - 同時處理封面圖片和多個 Gallery 檔案",
            Slug = $"test-mixed-files-{timestamp}"
        };

        string documentId = null;
        var uploadedFileIds = new List<int>();
        
        try
        {
            // Step 1: 建立 Article 實體
            documentId = await _articleProvider.CreateAsync(testArticle);
            documentId.ShouldNotBeNullOrEmpty();
            _output.WriteLine($"✅ Step 1 - Article 建立成功，DocumentId: {documentId}");
            
            // Step 2: 同時上傳封面圖片和多個 Gallery 檔案
            var mixedUploads = new List<FileUploadRequest>
            {
                // 封面圖片
                new FileUploadRequest
                {
                    FileName = $"cover-mixed-{timestamp}.jpg",
                    ContentType = "image/jpeg",
                    FileStream = new MemoryStream(CreateTestJpegBytes()),
                    AlternativeText = $"混合測試封面圖片 {timestamp}",
                    Caption = $"混合測試封面 - {timestamp}"
                },
                // Gallery 圖片 1
                new FileUploadRequest
                {
                    FileName = $"gallery-mixed-1-{timestamp}.jpg",
                    ContentType = "image/jpeg",
                    FileStream = new MemoryStream(CreateTestJpegBytes()),
                    AlternativeText = $"混合測試 Gallery 1 {timestamp}",
                    Caption = $"混合測試 Gallery 1 - {timestamp}"
                },
                // Gallery 圖片 2
                new FileUploadRequest
                {
                    FileName = $"gallery-mixed-2-{timestamp}.jpg",
                    ContentType = "image/jpeg",
                    FileStream = new MemoryStream(CreateTestJpegBytes()),
                    AlternativeText = $"混合測試 Gallery 2 {timestamp}",
                    Caption = $"混合測試 Gallery 2 - {timestamp}"
                },
                // Gallery 圖片 3
                new FileUploadRequest
                {
                    FileName = $"gallery-mixed-3-{timestamp}.jpg",
                    ContentType = "image/jpeg",
                    FileStream = new MemoryStream(CreateTestJpegBytes()),
                    AlternativeText = $"混合測試 Gallery 3 {timestamp}",
                    Caption = $"混合測試 Gallery 3 - {timestamp}"
                },
                // Gallery 圖片 4
                new FileUploadRequest
                {
                    FileName = $"gallery-mixed-4-{timestamp}.jpg",
                    ContentType = "image/jpeg",
                    FileStream = new MemoryStream(CreateTestJpegBytes()),
                    AlternativeText = $"混合測試 Gallery 4 {timestamp}",
                    Caption = $"混合測試 Gallery 4 - {timestamp}"
                }
            };
            
            var uploadResults = await _mediaLibraryProvider.UploadMultipleAsync(mixedUploads);
            uploadResults.ShouldNotBeNull();
            uploadResults.Count.ShouldBe(5);
            uploadedFileIds.AddRange(uploadResults.Select(r => r.Id));
            
            _output.WriteLine($"✅ Step 2 - 混合檔案批次上傳成功，共 {uploadResults.Count} 個檔案");
            
            // Step 3: 分別關聯不同類型的檔案
            var existingArticle = await _articleProvider.GetAsync(documentId);
            existingArticle.ShouldNotBeNull();
            
            // 設定封面圖片（第一個檔案）
            existingArticle.Cover = uploadResults[0];
            
            // 注意：Article 沒有 Gallery 欄位，所以只測試 Cover 欄位
            // 剩餘檔案暫時不關聯到 Article（因為沒有對應欄位）
            
            var updateResult = await _articleProvider.UpdateAsync(documentId, existingArticle);
            updateResult.ShouldBe(documentId);
            
            // Step 4: 驗證混合檔案關聯成功
            var updatedArticle = await _articleProvider.GetAsync(documentId);
            updatedArticle.ShouldNotBeNull();
            
            // 驗證封面圖片
            updatedArticle.Cover.ShouldNotBeNull();
            updatedArticle.Cover.Id.ShouldBe(uploadResults[0].Id);
            updatedArticle.Cover.Name.ShouldContain("cover-mixed");
            _output.WriteLine($"✅ 封面圖片驗證成功 - ID: {updatedArticle.Cover.Id}, 名稱: {updatedArticle.Cover.Name}");
            
            _output.WriteLine($"✅ Step 3 - 混合檔案關聯完成");
            _output.WriteLine($"   測試結果：封面圖片 1 個（Article 沒有 Gallery 欄位）");
            _output.WriteLine($"   其餘檔案已上傳但未關聯到 Article");
        }
        finally
        {
            // Cleanup
            if (!string.IsNullOrEmpty(documentId))
            {
                await _articleProvider.DeleteAsync(documentId);
                _output.WriteLine($"🗑️ 已清理 Article DocumentId: {documentId}");
            }
            
            foreach (var fileId in uploadedFileIds)
            {
                try
                {
                    await _mediaLibraryProvider.DeleteAsync(fileId);
                    _output.WriteLine($"🗑️ 已清理混合檔案 ID: {fileId}");
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"⚠️ 清理混合檔案時發生錯誤: {ex.Message}");
                }
            }
        }
    }

    [Fact]
    public async Task UploadLargeFileSet_ShouldSucceed()
    {
        // Arrange - 測試大量檔案上傳（10個檔案）
        var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        var uploadedFileIds = new List<int>();
        
        var largeFileSet = new List<FileUploadRequest>();
        for (int i = 1; i <= 10; i++)
        {
            largeFileSet.Add(new FileUploadRequest
            {
                FileName = $"large-set-{i:D2}-{timestamp}.jpg",
                ContentType = "image/jpeg",
                FileStream = new MemoryStream(CreateTestJpegBytes()),
                AlternativeText = $"大量上傳測試 {i} {timestamp}",
                Caption = $"大量檔案上傳測試 {i} - {timestamp}"
            });
        }
        
        try
        {
            // Act - 批次上傳大量檔案
            _output.WriteLine($"開始批次上傳 {largeFileSet.Count} 個檔案（大量檔案測試）...");
            var uploadResults = await _mediaLibraryProvider.UploadMultipleAsync(largeFileSet);
            
            // Assert
            uploadResults.ShouldNotBeNull();
            uploadResults.Count.ShouldBe(10);
            uploadedFileIds.AddRange(uploadResults.Select(r => r.Id));
            
            // 驗證每個檔案
            foreach (var uploadResult in uploadResults)
            {
                uploadResult.Id.ShouldBeGreaterThan(0);
                uploadResult.Name.ShouldContain("large-set");
                uploadResult.Mime.ShouldBe("image/jpeg");
                uploadResult.Url.ShouldNotBeNullOrEmpty();
                uploadResult.AlternativeText.ShouldNotBeNullOrEmpty();
                uploadResult.Caption.ShouldNotBeNullOrEmpty();
            }
            
            _output.WriteLine($"✅ 大量檔案批次上傳成功，共 {uploadResults.Count} 個檔案");
            _output.WriteLine($"   檔案 ID 範圍: {uploadResults.Min(r => r.Id)} - {uploadResults.Max(r => r.Id)}");
            
            // 驗證檔案順序正確性
            for (int i = 0; i < uploadResults.Count; i++)
            {
                var expectedNumber = i + 1;
                uploadResults[i].Name.ShouldContain($"large-set-{expectedNumber:D2}");
                _output.WriteLine($"   檔案 {expectedNumber:D2} - ID: {uploadResults[i].Id}, 名稱: {uploadResults[i].Name}");
            }
        }
        finally
        {
            // Cleanup
            _output.WriteLine($"開始清理 {uploadedFileIds.Count} 個測試檔案...");
            var cleanupTasks = uploadedFileIds.Select(async fileId =>
            {
                try
                {
                    await _mediaLibraryProvider.DeleteAsync(fileId);
                    _output.WriteLine($"🗑️ 已清理檔案 ID: {fileId}");
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"⚠️ 清理檔案 {fileId} 時發生錯誤: {ex.Message}");
                }
            });
            
            await Task.WhenAll(cleanupTasks);
            _output.WriteLine($"🗑️ 大量檔案清理完成");
        }
    }
}