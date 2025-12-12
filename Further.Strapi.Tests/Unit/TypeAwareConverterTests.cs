using System;
using System.Collections.Generic;
using System.Text.Json;
using Xunit;
using Shouldly;
using Further.Strapi.Serialization;
using Further.Strapi.Tests.Models;

namespace Further.Strapi.Tests.Unit;

/// <summary>
/// TypeAwareConverter 單元測試
/// 驗證類型識別序列化器的自動轉換功能
/// </summary>
public class TypeAwareConverterTests : StrapiIntegrationTestBase
{
    private readonly ITypeAwareConverter _converter;

    public TypeAwareConverterTests()
    {
        _converter = GetRequiredService<ITypeAwareConverter>();
    }

    #region Media 類型測試（透過複雜物件驗證）

    [Fact]
    public void PreprocessForWrite_WithObjectContainingMedia_ShouldConvertMediaToId()
    {
        // Arrange - 透過包含 Media 的物件來測試
        var article = new ArticleWithoutConverterAttribute
        {
            Title = "Test",
            Cover = new StrapiMediaField
            {
                Id = 123,
                DocumentId = "media-doc-123",
                Name = "test.jpg",
                Url = "/uploads/test.jpg"
            }
        };

        // Act
        var result = _converter.ConvertForWrite(article);

        // Assert
        result.ShouldBeOfType<Dictionary<string, object?>>();
        var dict = (Dictionary<string, object?>)result!;
        dict["cover"].ShouldBe(123); // Media 轉換為 Id
    }

    [Fact]
    public void PreprocessForWrite_WithObjectContainingMediaZeroId_ShouldReturnNull()
    {
        // Arrange
        var article = new ArticleWithoutConverterAttribute
        {
            Title = "Test",
            Cover = new StrapiMediaField
            {
                Id = 0,
                DocumentId = "media-doc-123",
                Name = "test.jpg"
            }
        };

        // Act
        var result = _converter.ConvertForWrite(article);

        // Assert
        var dict = (Dictionary<string, object?>)result!;
        dict["cover"].ShouldBeNull(); // Id 為 0 時返回 null
    }

    [Fact]
    public void PreprocessForWrite_WithObjectContainingMediaList_ShouldConvertToIdList()
    {
        // Arrange
        var gallery = new GalleryWithoutConverterAttribute
        {
            Title = "Test Gallery",
            Images = new List<StrapiMediaField>
            {
                new StrapiMediaField { Id = 1, Name = "image1.jpg" },
                new StrapiMediaField { Id = 2, Name = "image2.jpg" },
                new StrapiMediaField { Id = 3, Name = "image3.jpg" }
            }
        };

        // Act
        var result = _converter.ConvertForWrite(gallery);

        // Assert
        var dict = (Dictionary<string, object?>)result!;
        dict["images"].ShouldBeOfType<List<int>>();
        var idList = (List<int>)dict["images"]!;
        idList.Count.ShouldBe(3);
        idList.ShouldContain(1);
        idList.ShouldContain(2);
        idList.ShouldContain(3);
    }

    #endregion

    #region Relation 類型測試 (有 StrapiCollectionName)

    [Fact]
    public void PreprocessForWrite_WithObjectContainingRelation_ShouldConvertToDocumentId()
    {
        // Arrange
        var article = new ArticleWithoutConverterAttribute
        {
            Title = "Test",
            Author = new Author
            {
                Id = 123,
                DocumentId = "author-doc-456",
                Name = "Test Author",
                Email = "author@test.com"
            }
        };

        // Act
        var result = _converter.ConvertForWrite(article);

        // Assert
        var dict = (Dictionary<string, object?>)result!;
        dict["author"].ShouldBe("author-doc-456"); // Relation 轉換為 DocumentId
    }

    [Fact]
    public void PreprocessForWrite_WithObjectContainingRelationList_ShouldConvertToDocumentIdList()
    {
        // Arrange
        var post = new PostWithCategories
        {
            Title = "Test Post",
            Categories = new List<Category>
            {
                new Category { DocumentId = "cat-1", Name = "Category 1" },
                new Category { DocumentId = "cat-2", Name = "Category 2" }
            }
        };

        // Act
        var result = _converter.ConvertForWrite(post);

        // Assert
        var dict = (Dictionary<string, object?>)result!;
        dict["categories"].ShouldBeOfType<List<string>>();
        var docIdList = (List<string>)dict["categories"]!;
        docIdList.Count.ShouldBe(2);
        docIdList.ShouldContain("cat-1");
        docIdList.ShouldContain("cat-2");
    }

    #endregion

    #region 複雜物件測試

    [Fact]
    public void PreprocessForWrite_WithComplexObject_ShouldAutoConvertMediaAndRelation()
    {
        // Arrange - 建立一個沒有 [JsonConverter] 標註的測試類別
        var article = new ArticleWithoutConverterAttribute
        {
            Id = 1,
            DocumentId = "article-123",
            Title = "Test Article",
            Description = "Test Description",
            Cover = new StrapiMediaField { Id = 10, Name = "cover.jpg" },
            Author = new Author { DocumentId = "author-456", Name = "Author" },
            Category = new Category { DocumentId = "category-789", Name = "Tech" }
        };

        // Act
        var result = _converter.ConvertForWrite(article);

        // Assert
        result.ShouldBeOfType<Dictionary<string, object?>>();
        var dict = (Dictionary<string, object?>)result!;

        // 基本欄位應該保持原樣
        dict["title"].ShouldBe("Test Article");
        dict["description"].ShouldBe("Test Description");

        // Media 應該轉換為 Id
        dict["cover"].ShouldBe(10);

        // Relation 應該轉換為 DocumentId
        dict["author"].ShouldBe("author-456");
        dict["category"].ShouldBe("category-789");
    }

    [Fact]
    public void PreprocessForWrite_WithNullProperties_ShouldHandleGracefully()
    {
        // Arrange
        var article = new ArticleWithoutConverterAttribute
        {
            Title = "Test Article",
            Description = "Test Description",
            Cover = null,
            Author = null,
            Category = null
        };

        // Act
        var result = _converter.ConvertForWrite(article);

        // Assert
        result.ShouldBeOfType<Dictionary<string, object?>>();
        var dict = (Dictionary<string, object?>)result!;

        dict["title"].ShouldBe("Test Article");
        dict["cover"].ShouldBeNull();
        dict["author"].ShouldBeNull();
        dict["category"].ShouldBeNull();
    }

    #endregion

    #region 基本類型測試

    [Fact]
    public void PreprocessForWrite_WithString_ShouldReturnUnchanged()
    {
        // Arrange
        var value = "Hello World";

        // Act
        var result = _converter.ConvertForWrite(value);

        // Assert
        result.ShouldBe("Hello World");
    }

    [Fact]
    public void PreprocessForWrite_WithNumber_ShouldReturnUnchanged()
    {
        // Arrange
        var value = 42;

        // Act
        var result = _converter.ConvertForWrite(value);

        // Assert
        result.ShouldBe(42);
    }

    [Fact]
    public void PreprocessForWrite_WithNull_ShouldReturnNull()
    {
        // Act
        var result = _converter.ConvertForWrite(null);

        // Assert
        result.ShouldBeNull();
    }

    #endregion

    #region 類型檢查方法測試

    [Fact]
    public void HasStrapiCollectionName_WithCollectionType_ShouldReturnTrue()
    {
        // Assert
        _converter.HasStrapiCollectionName(typeof(Article)).ShouldBeTrue();
        _converter.HasStrapiCollectionName(typeof(Author)).ShouldBeTrue();
        _converter.HasStrapiCollectionName(typeof(Category)).ShouldBeTrue();
    }

    [Fact]
    public void HasStrapiCollectionName_WithNonCollectionType_ShouldReturnFalse()
    {
        // Assert
        _converter.HasStrapiCollectionName(typeof(string)).ShouldBeFalse();
        _converter.HasStrapiCollectionName(typeof(StrapiMediaField)).ShouldBeFalse();
    }

    [Fact]
    public void HasStrapiSingleTypeName_WithSingleType_ShouldReturnTrue()
    {
        // Assert
        _converter.HasStrapiSingleTypeName(typeof(Global)).ShouldBeTrue();
    }

    #endregion

    #region Component 類型測試

    [Fact]
    public void PreprocessForWrite_WithComponent_ShouldAddComponentName()
    {
        // Arrange - 建立一個 Component 物件
        var component = new SharedRichTextComponent
        {
            Id = 123,
            Body = "Test content"
        };

        // Act - 作為屬性值處理時會加入 __component
        var wrapper = new ObjectWithComponent { RichText = component };
        var result = _converter.ConvertForWrite(wrapper);

        // Assert
        var dict = (Dictionary<string, object?>)result!;
        var richTextDict = (Dictionary<string, object?>)dict["richText"]!;
        richTextDict["__component"].ShouldBe("shared.rich-text");
        richTextDict["body"].ShouldBe("Test content");
    }

    [Fact]
    public void PreprocessForWrite_WithComponentList_ShouldAddComponentNameToEach()
    {
        // Arrange - Dynamic Zone 場景
        var wrapper = new ObjectWithComponentList
        {
            Blocks = new List<IStrapiComponent>
            {
                new SharedRichTextComponent { Body = "Text 1" },
                new SharedQuoteComponent { Title = "Quote", Body = "Quote body" }
            }
        };

        // Act
        var result = _converter.ConvertForWrite(wrapper);

        // Assert
        var dict = (Dictionary<string, object?>)result!;
        var blocks = (List<object?>)dict["blocks"]!;
        blocks.Count.ShouldBe(2);

        var firstBlock = (Dictionary<string, object?>)blocks[0]!;
        firstBlock["__component"].ShouldBe("shared.rich-text");

        var secondBlock = (Dictionary<string, object?>)blocks[1]!;
        secondBlock["__component"].ShouldBe("shared.quote");
    }

    [Fact]
    public void PreprocessForWrite_WithNestedComponentContainingMedia_ShouldConvertMediaToId()
    {
        // Arrange - Component 包含 Media
        var wrapper = new ObjectWithComponent
        {
            MediaComponent = new SharedMediaComponent
            {
                Id = 1,
                File = new StrapiMediaField { Id = 42, Name = "test.jpg" }
            }
        };

        // Act
        var result = _converter.ConvertForWrite(wrapper);

        // Assert
        var dict = (Dictionary<string, object?>)result!;
        var mediaComponentDict = (Dictionary<string, object?>)dict["mediaComponent"]!;
        mediaComponentDict["__component"].ShouldBe("shared.media");
        mediaComponentDict["file"].ShouldBe(42); // Media 轉換為 Id
    }

    #endregion

    #region SingleType 測試

    [Fact]
    public void PreprocessForWrite_WithSingleType_ShouldConvertToDocumentId()
    {
        // Arrange
        var wrapper = new ObjectWithSingleType
        {
            GlobalSettings = new Global
            {
                DocumentId = "global-doc-123",
                SiteName = "Test Site"
            }
        };

        // Act
        var result = _converter.ConvertForWrite(wrapper);

        // Assert
        var dict = (Dictionary<string, object?>)result!;
        dict["globalSettings"].ShouldBe("global-doc-123");
    }

    #endregion

    #region 邊界情況測試

    [Fact]
    public void PreprocessForWrite_WithEmptyList_ShouldReturnNull()
    {
        // Arrange
        var gallery = new GalleryWithoutConverterAttribute
        {
            Title = "Empty Gallery",
            Images = new List<StrapiMediaField>() // 空列表
        };

        // Act
        var result = _converter.ConvertForWrite(gallery);

        // Assert
        var dict = (Dictionary<string, object?>)result!;
        dict["images"].ShouldBeNull(); // 空列表轉換為 null
    }

    [Fact]
    public void PreprocessForWrite_WithRelationMissingDocumentId_ShouldReturnNull()
    {
        // Arrange
        var article = new ArticleWithoutConverterAttribute
        {
            Title = "Test",
            Author = new Author
            {
                Id = 123,
                DocumentId = null, // 沒有 DocumentId
                Name = "Test Author"
            }
        };

        // Act
        var result = _converter.ConvertForWrite(article);

        // Assert
        var dict = (Dictionary<string, object?>)result!;
        dict["author"].ShouldBeNull(); // 沒有 DocumentId 時返回 null
    }

    [Fact]
    public void PreprocessForWrite_WithRelationEmptyDocumentId_ShouldReturnNull()
    {
        // Arrange
        var article = new ArticleWithoutConverterAttribute
        {
            Title = "Test",
            Author = new Author
            {
                Id = 123,
                DocumentId = "   ", // 空白字串
                Name = "Test Author"
            }
        };

        // Act
        var result = _converter.ConvertForWrite(article);

        // Assert
        var dict = (Dictionary<string, object?>)result!;
        dict["author"].ShouldBeNull(); // 空白 DocumentId 時返回 null
    }

    [Fact]
    public void PreprocessForWrite_WithEmptyRelationList_ShouldReturnNull()
    {
        // Arrange
        var post = new PostWithCategories
        {
            Title = "Test Post",
            Categories = new List<Category>() // 空列表
        };

        // Act
        var result = _converter.ConvertForWrite(post);

        // Assert
        var dict = (Dictionary<string, object?>)result!;
        dict["categories"].ShouldBeNull(); // 空列表轉換為 null
    }

    #endregion

    #region JsonPropertyName 測試

    [Fact]
    public void PreprocessForWrite_WithJsonPropertyName_ShouldUseCustomName()
    {
        // Arrange
        var obj = new ObjectWithJsonPropertyName
        {
            MyTitle = "Test Title",
            MyDescription = "Test Description"
        };

        // Act
        var result = _converter.ConvertForWrite(obj);

        // Assert
        var dict = (Dictionary<string, object?>)result!;
        dict.ContainsKey("custom_title").ShouldBeTrue();
        dict["custom_title"].ShouldBe("Test Title");
        dict.ContainsKey("myDescription").ShouldBeTrue(); // 沒有標註的用 camelCase
    }

    #endregion
}

#region 測試用輔助類別

/// <summary>
/// 測試用 - 包含 Component 屬性的物件
/// </summary>
public class ObjectWithComponent
{
    public SharedRichTextComponent? RichText { get; set; }
    public SharedMediaComponent? MediaComponent { get; set; }
}

/// <summary>
/// 測試用 - 包含 Component 列表的物件（模擬 Dynamic Zone）
/// </summary>
public class ObjectWithComponentList
{
    public List<IStrapiComponent>? Blocks { get; set; }
}

/// <summary>
/// 測試用 - 包含 SingleType 的物件
/// </summary>
public class ObjectWithSingleType
{
    public Global? GlobalSettings { get; set; }
}

/// <summary>
/// 測試用 - 包含 JsonPropertyName 標註的物件
/// </summary>
public class ObjectWithJsonPropertyName
{
    [System.Text.Json.Serialization.JsonPropertyName("custom_title")]
    public string MyTitle { get; set; } = string.Empty;

    public string MyDescription { get; set; } = string.Empty;
}

#endregion

/// <summary>
/// 測試用的 Article 類別 - 沒有 [JsonConverter] 標註
/// 用於驗證 TypeAwareConverter 的自動識別功能
/// </summary>
public class ArticleWithoutConverterAttribute
{
    public int? Id { get; set; }
    public string? DocumentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // 沒有 [JsonConverter] 標註 - TypeAwareConverter 應該自動識別並轉換
    public StrapiMediaField? Cover { get; set; }
    public Author? Author { get; set; }
    public Category? Category { get; set; }
}

/// <summary>
/// 測試用的 Gallery 類別 - 包含 Media 列表
/// </summary>
public class GalleryWithoutConverterAttribute
{
    public string Title { get; set; } = string.Empty;
    public List<StrapiMediaField>? Images { get; set; }
}

/// <summary>
/// 測試用的 Post 類別 - 包含 Category 列表 (Relation List)
/// </summary>
public class PostWithCategories
{
    public string Title { get; set; } = string.Empty;
    public List<Category>? Categories { get; set; }
}
