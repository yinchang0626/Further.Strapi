using System;
using Xunit;
using Shouldly;

namespace Further.Strapi.Tests.Unit;

/// <summary>
/// MediaLibrary 相關數據模型的純單元測試 - 無外部依賴
/// 測試媒體庫相關的數據模型和屬性
/// </summary>
public class MediaLibraryProviderUnitTests
{
    [Fact]
    public void StrapiMediaField_Properties_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        var mediaField = new StrapiMediaField
        {
            Id = 123,
            DocumentId = "media-123",
            Name = "test-image.jpg",
            AlternativeText = "Test Image",
            Caption = "Test Caption",
            Width = 1920,
            Height = 1080,
            Hash = "test-hash",
            Ext = ".jpg",
            Mime = "image/jpeg",
            Size = 1024.5m,
            Url = "/uploads/test-image.jpg",
            PreviewUrl = "/uploads/preview-test-image.jpg",
            Provider = "local",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        // Assert
        mediaField.Id.ShouldBe(123);
        mediaField.DocumentId.ShouldBe("media-123");
        mediaField.Name.ShouldBe("test-image.jpg");
        mediaField.AlternativeText.ShouldBe("Test Image");
        mediaField.Caption.ShouldBe("Test Caption");
        mediaField.Width.ShouldBe(1920);
        mediaField.Height.ShouldBe(1080);
        mediaField.Ext.ShouldBe(".jpg");
        mediaField.Mime.ShouldBe("image/jpeg");
        mediaField.Size.ShouldBe(1024.5m);
        mediaField.Provider.ShouldBe("local");
    }

    [Fact]
    public void StrapiMediaField_WithNullValues_ShouldHandleGracefully()
    {
        // Arrange & Act
        var mediaField = new StrapiMediaField
        {
            Id = 1,
            Name = "test.jpg"
            // Don't set other properties
        };

        // Assert
        mediaField.Id.ShouldBe(1);
        mediaField.Name.ShouldBe("test.jpg");
        // DocumentId will be empty string by default, not null
        mediaField.DocumentId.ShouldNotBeNull();
        mediaField.AlternativeText.ShouldBeNull();
        mediaField.Caption.ShouldBeNull();
    }

    [Fact]
    public void FileInfoUpdateRequest_Properties_ShouldSetCorrectly()
    {
        // Arrange & Act
        var updateRequest = new FileInfoUpdateRequest
        {
            AlternativeText = "Updated Alt Text",
            Caption = "Updated Caption",
            Name = "updated-file.jpg"
        };

        // Assert
        updateRequest.AlternativeText.ShouldBe("Updated Alt Text");
        updateRequest.Caption.ShouldBe("Updated Caption");
        updateRequest.Name.ShouldBe("updated-file.jpg");
    }

    [Fact]
    public void StrapiMediaField_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var mediaField = new StrapiMediaField();

        // Assert
        mediaField.Id.ShouldBe(0);
        mediaField.DocumentId.ShouldNotBeNull();
        mediaField.Size.ShouldBe(0);
        mediaField.Width.ShouldBeNull(); // int? 預設為 null
        mediaField.Height.ShouldBeNull(); // int? 預設為 null
    }

    [Fact]
    public void FileInfoUpdateRequest_AllowsNullValues()
    {
        // Arrange & Act
        var updateRequest = new FileInfoUpdateRequest
        {
            Name = null,
            AlternativeText = null,
            Caption = null
        };

        // Assert
        updateRequest.Name.ShouldBeNull();
        updateRequest.AlternativeText.ShouldBeNull();
        updateRequest.Caption.ShouldBeNull();
    }

    [Fact]
    public void StrapiMediaField_Properties_ShouldAcceptNullableValues()
    {
        // Arrange & Act
        var mediaField = new StrapiMediaField();

        // Assert - Testing properties that should have default values
        mediaField.DocumentId.ShouldNotBeNull();
        mediaField.Name.ShouldNotBeNull(); // string 預設為 string.Empty
        mediaField.AlternativeText.ShouldBeNull();
        mediaField.Caption.ShouldBeNull();
        mediaField.Hash.ShouldNotBeNull(); // string 預設為 string.Empty
        mediaField.Ext.ShouldBeNull();
        mediaField.Mime.ShouldNotBeNull(); // string 預設為 string.Empty
        mediaField.Url.ShouldNotBeNull(); // string 預設為 string.Empty
        mediaField.PreviewUrl.ShouldBeNull();
        mediaField.Provider.ShouldNotBeNull(); // string 預設為 string.Empty
    }
}