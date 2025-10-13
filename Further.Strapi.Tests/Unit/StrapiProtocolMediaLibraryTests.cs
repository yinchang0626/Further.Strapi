using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using Xunit;
using Shouldly;

namespace Further.Strapi.Tests.Unit;

/// <summary>
/// StrapiProtocol.MediaLibrary 表單創建功能測試
/// 測試各種 MultipartFormDataContent 創建邏輯
/// </summary>
public class StrapiProtocolMediaLibraryTests
{
    [Fact]
    public void CreateUploadForm_WithValidRequest_ShouldCreateValidForm()
    {
        // Arrange
        var fileUpload = new FileUploadRequest
        {
            FileStream = new MemoryStream(new byte[] { 255, 216, 255, 224 }), // JPEG header
            FileName = "test.jpg",
            ContentType = "image/jpeg",
            AlternativeText = "Test image",
            Caption = "Test caption"
        };

        // Act
        var form = StrapiProtocol.MediaLibrary.CreateUploadForm(fileUpload);

        // Assert
        form.ShouldNotBeNull();
        form.Headers.ContentType.ShouldNotBeNull();
        form.Headers.ContentType!.MediaType.ShouldBe("multipart/form-data");
    }

    [Fact]
    public void CreateMultipleUploadForm_WithValidRequests_ShouldCreateValidForm()
    {
        // Arrange
        var fileUploads = new List<FileUploadRequest>
        {
            new FileUploadRequest
            {
                FileStream = new MemoryStream(new byte[] { 255, 216, 255, 224 }),
                FileName = "test1.jpg",
                ContentType = "image/jpeg",
                AlternativeText = "Test image 1",
                Caption = "Test caption 1"
            },
            new FileUploadRequest
            {
                FileStream = new MemoryStream(new byte[] { 137, 80, 78, 71 }), // PNG header
                FileName = "test2.png",
                ContentType = "image/png",
                AlternativeText = "Test image 2",
                Caption = "Test caption 2"
            }
        };

        // Act
        var form = StrapiProtocol.MediaLibrary.CreateMultipleUploadForm(fileUploads);

        // Assert
        form.ShouldNotBeNull();
        form.Headers.ContentType.ShouldNotBeNull();
        form.Headers.ContentType!.MediaType.ShouldBe("multipart/form-data");
    }

    [Fact]
    public void CreateFileInfoUpdateForm_WithValidRequest_ShouldCreateValidForm()
    {
        // Arrange
        var updateRequest = new FileInfoUpdateRequest
        {
            Name = "updated-image.jpg",
            AlternativeText = "Updated alternative text",
            Caption = "Updated caption"
        };

        // Act
        var form = StrapiProtocol.MediaLibrary.CreateFileInfoUpdateForm(updateRequest);

        // Assert
        form.ShouldNotBeNull();
        form.Headers.ContentType.ShouldNotBeNull();
        form.Headers.ContentType!.MediaType.ShouldBe("multipart/form-data");
    }

    [Fact]
    public void FileUploadRequest_DefaultValues_ShouldBeValid()
    {
        // Arrange & Act
        var request = new FileUploadRequest
        {
            FileStream = new MemoryStream(),
            FileName = "test.txt",
            ContentType = "text/plain"
        };

        // Assert
        request.FileStream.ShouldNotBeNull();
        request.FileName.ShouldBe("test.txt");
        request.ContentType.ShouldBe("text/plain");
        request.AlternativeText.ShouldBeNull();
        request.Caption.ShouldBeNull();
        request.Path.ShouldBeNull();
    }

    [Fact]
    public void FileInfoUpdateRequest_AllProperties_ShouldBeSettable()
    {
        // Arrange & Act
        var request = new FileInfoUpdateRequest
        {
            Name = "new-name.jpg",
            AlternativeText = "New alternative text",
            Caption = "New caption"
        };

        // Assert
        request.Name.ShouldBe("new-name.jpg");
        request.AlternativeText.ShouldBe("New alternative text");
        request.Caption.ShouldBe("New caption");
    }

    [Fact]
    public void FileInfoUpdateRequest_WithPartialInfo_ShouldWork()
    {
        // Arrange & Act
        var request = new FileInfoUpdateRequest
        {
            Name = "partial-update.jpg",
            AlternativeText = null,
            Caption = "Only caption updated"
        };

        // Assert
        request.Name.ShouldBe("partial-update.jpg");
        request.AlternativeText.ShouldBeNull();
        request.Caption.ShouldBe("Only caption updated");
    }

    [Fact]
    public void FileUploadRequest_WithAllOptionalProperties_ShouldWork()
    {
        // Arrange & Act
        var request = new FileUploadRequest
        {
            FileStream = new MemoryStream(new byte[] { 1, 2, 3 }),
            FileName = "complete.jpg",
            ContentType = "image/jpeg",
            AlternativeText = "Complete image",
            Caption = "Complete caption",
            Path = "/custom/path"
        };

        // Assert
        request.FileStream.Length.ShouldBe(3);
        request.FileName.ShouldBe("complete.jpg");
        request.ContentType.ShouldBe("image/jpeg");
        request.AlternativeText.ShouldBe("Complete image");
        request.Caption.ShouldBe("Complete caption");
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
    }

    [Fact]
    public void FileUploadRequest_WithLargeStream_ShouldWork()
    {
        // Arrange
        var largeData = new byte[1024 * 1024]; // 1MB
        for (int i = 0; i < largeData.Length; i++)
        {
            largeData[i] = (byte)(i % 256);
        }

        // Act
        var request = new FileUploadRequest
        {
            FileStream = new MemoryStream(largeData),
            FileName = "large-file.bin",
            ContentType = "application/octet-stream"
        };

        // Assert
        request.FileStream.Length.ShouldBe(1024 * 1024);
        request.FileName.ShouldBe("large-file.bin");
        request.ContentType.ShouldBe("application/octet-stream");
    }

    [Fact]
    public void CreateMultipleUploadForm_WithEmptyList_ShouldCreateValidForm()
    {
        // Arrange
        var fileUploads = new List<FileUploadRequest>();

        // Act
        var form = StrapiProtocol.MediaLibrary.CreateMultipleUploadForm(fileUploads);

        // Assert
        form.ShouldNotBeNull();
        form.Headers.ContentType.ShouldNotBeNull();
        form.Headers.ContentType!.MediaType.ShouldBe("multipart/form-data");
    }

    [Fact]
    public void CreateMultipleUploadForm_WithSingleFile_ShouldCreateValidForm()
    {
        // Arrange
        var fileUploads = new List<FileUploadRequest>
        {
            new FileUploadRequest
            {
                FileStream = new MemoryStream(new byte[] { 255, 216, 255, 224 }),
                FileName = "single.jpg",
                ContentType = "image/jpeg",
                AlternativeText = "Single image",
                Caption = "Single caption"
            }
        };

        // Act
        var form = StrapiProtocol.MediaLibrary.CreateMultipleUploadForm(fileUploads);

        // Assert
        form.ShouldNotBeNull();
        form.Headers.ContentType.ShouldNotBeNull();
        form.Headers.ContentType!.MediaType.ShouldBe("multipart/form-data");
    }
}