using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Shouldly;
using Volo.Abp.Json;
using Further.Strapi.Tests;
using System.Collections.Generic;
using System.Linq;

namespace Further.Strapi.Tests.Unit;

/// <summary>
/// MediaLibraryProvider 簡化版單元測試
/// 測試策略A的簡化媒體庫功能
/// </summary>
public class MediaLibraryProviderExtendedTests : StrapiIntegrationTestBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly MediaLibraryProvider _mediaLibraryProvider;

    public MediaLibraryProviderExtendedTests()
    {
        _httpClientFactory = GetRequiredService<IHttpClientFactory>();
        _jsonSerializer = GetRequiredService<IJsonSerializer>();
        _mediaLibraryProvider = new MediaLibraryProvider(_httpClientFactory, _jsonSerializer);
    }

    [Fact]
    public void Constructor_WithValidDependencies_ShouldNotThrow()
    {
        // Act & Assert
        Should.NotThrow(() => new MediaLibraryProvider(_httpClientFactory, _jsonSerializer));
    }

    [Fact]
    public void FileUploadRequest_DefaultValues_ShouldBeValid()
    {
        // Arrange & Act
        var request = new FileUploadRequest
        {
            FileStream = new MemoryStream(new byte[] { 1, 2, 3 }),
            FileName = "test.jpg",
            ContentType = "image/jpeg"
        };

        // Assert
        request.FileStream.ShouldNotBeNull();
        request.FileName.ShouldBe("test.jpg");
        request.ContentType.ShouldBe("image/jpeg");
        request.AlternativeText.ShouldBeNull();
        request.Caption.ShouldBeNull();
        request.Path.ShouldBeNull();
    }

    [Fact]
    public void FileUploadRequest_WithAllOptionalProperties_ShouldWork()
    {
        // Arrange & Act
        var request = new FileUploadRequest
        {
            FileStream = new MemoryStream(new byte[] { 137, 80, 78, 71 }), // PNG header
            FileName = "test.png",
            ContentType = "image/png",
            AlternativeText = "A beautiful image",
            Caption = "This image shows something amazing",
            Path = "/custom/path"
        };

        // Assert
        request.FileStream.ShouldNotBeNull();
        request.FileName.ShouldBe("test.png");
        request.ContentType.ShouldBe("image/png");
        request.AlternativeText.ShouldBe("A beautiful image");
        request.Caption.ShouldBe("This image shows something amazing");
        request.Path.ShouldBe("/custom/path");
    }

    [Fact]
    public void FileUploadRequest_WithEmptyStream_ShouldWork()
    {
        // Arrange & Act
        var request = new FileUploadRequest
        {
            FileStream = new MemoryStream(new byte[0]),
            FileName = "empty.txt",
            ContentType = "text/plain"
        };

        // Assert
        request.FileStream.Length.ShouldBe(0);
        request.FileName.ShouldBe("empty.txt");
        request.ContentType.ShouldBe("text/plain");
    }

    [Fact]
    public void FileUploadRequestList_ForMultipleUpload_ShouldWork()
    {
        // Arrange & Act
        var requests = new List<FileUploadRequest>
        {
            new FileUploadRequest
            {
                FileStream = new MemoryStream(new byte[] { 255, 216, 255 }), // JPEG header
                FileName = "image1.jpg",
                ContentType = "image/jpeg",
                AlternativeText = "First image",
                Caption = "Caption for first image"
            },
            new FileUploadRequest
            {
                FileStream = new MemoryStream(new byte[] { 137, 80, 78, 71 }), // PNG header
                FileName = "image2.png",
                ContentType = "image/png",
                AlternativeText = "Second image",
                Caption = "Caption for second image"
            }
        };

        // Assert
        requests.Count.ShouldBe(2);
        requests[0].FileName.ShouldBe("image1.jpg");
        requests[0].ContentType.ShouldBe("image/jpeg");
        requests[1].FileName.ShouldBe("image2.png");
        requests[1].ContentType.ShouldBe("image/png");
        
        // 驗證所有檔案都有內容
        requests.All(r => r.FileStream.Length > 0).ShouldBeTrue();
    }

    [Fact]
    public void FileInfoUpdateRequest_AllProperties_ShouldBeValid()
    {
        // Arrange & Act
        var request = new FileInfoUpdateRequest
        {
            Name = "updated-name.jpg",
            AlternativeText = "Updated alt text",
            Caption = "Updated caption"
        };

        // Assert
        request.Name.ShouldBe("updated-name.jpg");
        request.AlternativeText.ShouldBe("Updated alt text");
        request.Caption.ShouldBe("Updated caption");
    }

    [Fact]
    public void FileInfoUpdateRequest_WithNullValues_ShouldWork()
    {
        // Arrange & Act
        var request = new FileInfoUpdateRequest
        {
            Name = null,
            AlternativeText = null,
            Caption = null
        };

        // Assert
        request.Name.ShouldBeNull();
        request.AlternativeText.ShouldBeNull();
        request.Caption.ShouldBeNull();
    }

    [Fact]
    public void StrapiMediaField_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var mediaField = new StrapiMediaField();

        // Assert
        mediaField.Id.ShouldBe(0);
        mediaField.DocumentId.ShouldNotBeNull(); // string, default empty
        mediaField.Name.ShouldNotBeNull(); // string, default empty
        mediaField.AlternativeText.ShouldBeNull();
        mediaField.Caption.ShouldBeNull();
        mediaField.Width.ShouldBeNull();
        mediaField.Height.ShouldBeNull();
        mediaField.Formats.ShouldBeNull();
        mediaField.Hash.ShouldNotBeNull(); // string, default empty
        mediaField.Ext.ShouldBeNull();
        mediaField.Mime.ShouldNotBeNull(); // string, default empty
        mediaField.Size.ShouldBe(0); // decimal, default 0
        mediaField.Url.ShouldNotBeNull(); // string, default empty
        mediaField.PreviewUrl.ShouldBeNull();
        mediaField.Provider.ShouldNotBeNull(); // string, default empty
        mediaField.ProviderMetadata.ShouldBeNull();
        // CreatedAt and UpdatedAt are DateTime (not nullable), default is DateTime.MinValue
        mediaField.CreatedAt.ShouldBe(DateTime.MinValue);
        mediaField.UpdatedAt.ShouldBe(DateTime.MinValue);
    }

    [Fact]
    public void StrapiMediaField_WithData_ShouldSetPropertiesCorrectly()
    {
        // Arrange & Act
        var mediaField = new StrapiMediaField
        {
            Id = 123,
            DocumentId = "doc456",
            Name = "test-image.jpg",
            AlternativeText = "Test image",
            Caption = "This is a test",
            Width = 800,
            Height = 600,
            Hash = "abcdef123456",
            Ext = ".jpg",
            Mime = "image/jpeg",
            Size = 102400,
            Url = "/uploads/test_image_123.jpg",
            Provider = "local"
        };

        // Assert
        mediaField.Id.ShouldBe(123);
        mediaField.DocumentId.ShouldBe("doc456");
        mediaField.Name.ShouldBe("test-image.jpg");
        mediaField.AlternativeText.ShouldBe("Test image");
        mediaField.Caption.ShouldBe("This is a test");
        mediaField.Width.ShouldBe(800);
        mediaField.Height.ShouldBe(600);
        mediaField.Hash.ShouldBe("abcdef123456");
        mediaField.Ext.ShouldBe(".jpg");
        mediaField.Mime.ShouldBe("image/jpeg");
        mediaField.Size.ShouldBe(102400);
        mediaField.Url.ShouldBe("/uploads/test_image_123.jpg");
        mediaField.Provider.ShouldBe("local");
    }

    [Fact]
    public void StrapiMediaFormats_ShouldAllowNullValues()
    {
        // Arrange & Act
        var formats = new StrapiMediaFormats
        {
            Large = null,
            Medium = null,
            Small = null,
            Thumbnail = null
        };

        // Assert
        formats.Large.ShouldBeNull();
        formats.Medium.ShouldBeNull();
        formats.Small.ShouldBeNull();
        formats.Thumbnail.ShouldBeNull();
    }

    [Fact]
    public void StrapiMediaFormats_WithValidFormats_ShouldBeCorrect()
    {
        // Arrange & Act
        var formats = new StrapiMediaFormats
        {
            Large = new StrapiMediaFormat
            {
                Ext = ".jpg",
                Url = "/uploads/large_image.jpg",
                Hash = "large_hash",
                Mime = "image/jpeg",
                Name = "large_image.jpg",
                Path = null,
                Size = 500000,
                Width = 1920,
                Height = 1080
            },
            Thumbnail = new StrapiMediaFormat
            {
                Ext = ".jpg",
                Url = "/uploads/thumbnail_image.jpg",
                Hash = "thumb_hash",
                Mime = "image/jpeg", 
                Name = "thumbnail_image.jpg",
                Path = null,
                Size = 5000,
                Width = 150,
                Height = 150
            }
        };

        // Assert
        formats.Large.ShouldNotBeNull();
        formats.Large.Width.ShouldBe(1920);
        formats.Large.Height.ShouldBe(1080);
        formats.Large.Size.ShouldBe(500000);
        
        formats.Thumbnail.ShouldNotBeNull();
        formats.Thumbnail.Width.ShouldBe(150);
        formats.Thumbnail.Height.ShouldBe(150);
        formats.Thumbnail.Size.ShouldBe(5000);
    }

    [Fact]
    public void StrapiMediaFormat_AllProperties_ShouldBeSettable()
    {
        // Arrange & Act
        var format = new StrapiMediaFormat
        {
            Ext = ".png",
            Url = "/uploads/format_test.png",
            Hash = "format_hash_123",
            Mime = "image/png",
            Name = "format_test.png",
            Path = "/some/path",
            Size = 75000,
            Width = 640,
            Height = 480
        };

        // Assert
        format.Ext.ShouldBe(".png");
        format.Url.ShouldBe("/uploads/format_test.png");
        format.Hash.ShouldBe("format_hash_123");
        format.Mime.ShouldBe("image/png");
        format.Name.ShouldBe("format_test.png");
        format.Path.ShouldBe("/some/path");
        format.Size.ShouldBe(75000);
        format.Width.ShouldBe(640);
        format.Height.ShouldBe(480);
    }

    [Fact]
    public void MultipleFileUploadRequests_ShouldSupportBatchOperations()
    {
        // Arrange
        var jpegBytes = new byte[] { 255, 216, 255 }; // JPEG header
        var pngBytes = new byte[] { 137, 80, 78, 71 }; // PNG header
        
        // Act
        var batchRequests = new[]
        {
            new FileUploadRequest
            {
                FileStream = new MemoryStream(jpegBytes),
                FileName = "batch-1.jpg",
                ContentType = "image/jpeg",
                AlternativeText = "Batch upload 1"
            },
            new FileUploadRequest
            {
                FileStream = new MemoryStream(pngBytes),
                FileName = "batch-2.png", 
                ContentType = "image/png",
                AlternativeText = "Batch upload 2"
            },
            new FileUploadRequest
            {
                FileStream = new MemoryStream(jpegBytes),
                FileName = "batch-3.jpg",
                ContentType = "image/jpeg",
                Caption = "Batch upload 3 caption"
            }
        };

        // Assert
        batchRequests.Length.ShouldBe(3);
        batchRequests.All(r => r.FileStream.Length > 0).ShouldBeTrue();
        batchRequests.All(r => !string.IsNullOrEmpty(r.FileName)).ShouldBeTrue();
        batchRequests.All(r => !string.IsNullOrEmpty(r.ContentType)).ShouldBeTrue();
        
        // 驗證不同檔案類型
        batchRequests.Count(r => r.ContentType == "image/jpeg").ShouldBe(2);
        batchRequests.Count(r => r.ContentType == "image/png").ShouldBe(1);
    }
}