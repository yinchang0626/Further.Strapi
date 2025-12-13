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

    #region ProcessList 完整覆蓋測試

    [Fact]
    public void PreprocessForWrite_WithComponentListContainingNull_ShouldHandleNullElements()
    {
        // Arrange - 列表中包含 null 元素
        var wrapper = new ObjectWithComponentList
        {
            Blocks = new List<IStrapiComponent>
            {
                new SharedRichTextComponent { Body = "Text 1" },
                null!,  // null 元素
                new SharedQuoteComponent { Title = "Quote", Body = "Quote body" }
            }
        };

        // Act
        var result = _converter.ConvertForWrite(wrapper);

        // Assert
        var dict = (Dictionary<string, object?>)result!;
        var blocks = (List<object?>)dict["blocks"]!;
        blocks.Count.ShouldBe(3);

        // 第一個是 Component
        var firstBlock = (Dictionary<string, object?>)blocks[0]!;
        firstBlock["__component"].ShouldBe("shared.rich-text");

        // 第二個是 null
        blocks[1].ShouldBeNull();

        // 第三個是 Component
        var thirdBlock = (Dictionary<string, object?>)blocks[2]!;
        thirdBlock["__component"].ShouldBe("shared.quote");
    }

    [Fact]
    public void PreprocessForWrite_WithNonComponentList_ShouldKeepOriginalValues()
    {
        // Arrange - 包含非 Component 類型的列表（如字串列表）
        var wrapper = new ObjectWithStringList
        {
            Tags = new List<string> { "tag1", "tag2", "tag3" }
        };

        // Act
        var result = _converter.ConvertForWrite(wrapper);

        // Assert
        var dict = (Dictionary<string, object?>)result!;
        var tags = (List<object?>)dict["tags"]!;
        tags.Count.ShouldBe(3);
        tags[0].ShouldBe("tag1");
        tags[1].ShouldBe("tag2");
        tags[2].ShouldBe("tag3");
    }

    [Fact]
    public void PreprocessForWrite_WithIListProperty_ShouldProcessAsList()
    {
        // Arrange - 使用 IList<T> 來覆蓋 IsListType 的 IList 分支
        var wrapper = new ObjectWithIListProperty
        {
            Items = new List<string> { "item1", "item2" }
        };

        // Act
        var result = _converter.ConvertForWrite(wrapper);

        // Assert
        var dict = (Dictionary<string, object?>)result!;
        var items = (List<object?>)dict["items"]!;
        items.Count.ShouldBe(2);
        items[0].ShouldBe("item1");
        items[1].ShouldBe("item2");
    }

    [Fact]
    public void PreprocessForWrite_WithIEnumerableProperty_ShouldProcessAsList()
    {
        // Arrange - 使用 IEnumerable<T> 來覆蓋 IsListType 的 IEnumerable 分支
        var wrapper = new ObjectWithIEnumerableProperty
        {
            Items = new List<string> { "a", "b", "c" }
        };

        // Act
        var result = _converter.ConvertForWrite(wrapper);

        // Assert
        var dict = (Dictionary<string, object?>)result!;
        var items = (List<object?>)dict["items"]!;
        items.Count.ShouldBe(3);
    }

    [Fact]
    public void PreprocessForWrite_WithICollectionProperty_ShouldProcessAsList()
    {
        // Arrange - 使用 ICollection<T> 來覆蓋 IsListType 的 ICollection 分支
        var wrapper = new ObjectWithICollectionProperty
        {
            Items = new List<string> { "x", "y" }
        };

        // Act
        var result = _converter.ConvertForWrite(wrapper);

        // Assert
        var dict = (Dictionary<string, object?>)result!;
        var items = (List<object?>)dict["items"]!;
        items.Count.ShouldBe(2);
    }

    [Fact]
    public void PreprocessForWrite_WithIListOfMedia_ShouldConvertToIds()
    {
        // Arrange - 使用 IList<StrapiMediaField> 覆蓋 IsListOfType 的 IList 分支
        var wrapper = new ObjectWithIListMedia
        {
            Images = new List<StrapiMediaField>
            {
                new StrapiMediaField { Id = 100, Name = "img1.jpg" },
                new StrapiMediaField { Id = 200, Name = "img2.jpg" }
            }
        };

        // Act
        var result = _converter.ConvertForWrite(wrapper);

        // Assert
        var dict = (Dictionary<string, object?>)result!;
        var ids = (List<int>)dict["images"]!;
        ids.Count.ShouldBe(2);
        ids.ShouldContain(100);
        ids.ShouldContain(200);
    }

    [Fact]
    public void PreprocessForWrite_WithICollectionOfRelation_ShouldConvertToDocumentIds()
    {
        // Arrange - 使用 ICollection<Category> 覆蓋 IsListOfStrapiCollectionType 的 ICollection 分支
        var wrapper = new ObjectWithICollectionRelation
        {
            Categories = new List<Category>
            {
                new Category { DocumentId = "cat-a", Name = "A" },
                new Category { DocumentId = "cat-b", Name = "B" }
            }
        };

        // Act
        var result = _converter.ConvertForWrite(wrapper);

        // Assert
        var dict = (Dictionary<string, object?>)result!;
        var docIds = (List<string>)dict["categories"]!;
        docIds.Count.ShouldBe(2);
        docIds.ShouldContain("cat-a");
        docIds.ShouldContain("cat-b");
    }

    [Fact]
    public void PreprocessForWrite_WithIEnumerableOfComponent_ShouldAddComponentNames()
    {
        // Arrange - 使用 IEnumerable<IStrapiComponent> 覆蓋 Component 列表的 IEnumerable 分支
        var wrapper = new ObjectWithIEnumerableComponent
        {
            Blocks = new List<IStrapiComponent>
            {
                new SharedRichTextComponent { Body = "Content" }
            }
        };

        // Act
        var result = _converter.ConvertForWrite(wrapper);

        // Assert
        var dict = (Dictionary<string, object?>)result!;
        var blocks = (List<object?>)dict["blocks"]!;
        blocks.Count.ShouldBe(1);
        var firstBlock = (Dictionary<string, object?>)blocks[0]!;
        firstBlock["__component"].ShouldBe("shared.rich-text");
    }

    [Fact]
    public void PreprocessForWrite_WithIEnumerableOfMedia_ShouldConvertToIds()
    {
        // Arrange - 使用 IEnumerable<StrapiMediaField> 覆蓋 IsListOfType 的 IEnumerable 分支
        var wrapper = new ObjectWithIEnumerableMedia
        {
            Images = new List<StrapiMediaField>
            {
                new StrapiMediaField { Id = 300, Name = "img.jpg" }
            }
        };

        // Act
        var result = _converter.ConvertForWrite(wrapper);

        // Assert
        var dict = (Dictionary<string, object?>)result!;
        var ids = (List<int>)dict["images"]!;
        ids.Count.ShouldBe(1);
        ids.ShouldContain(300);
    }

    [Fact]
    public void PreprocessForWrite_WithICollectionOfMedia_ShouldConvertToIds()
    {
        // Arrange - 使用 ICollection<StrapiMediaField> 覆蓋 IsListOfType 的 ICollection 分支
        var wrapper = new ObjectWithICollectionMedia
        {
            Images = new List<StrapiMediaField>
            {
                new StrapiMediaField { Id = 400, Name = "img.jpg" }
            }
        };

        // Act
        var result = _converter.ConvertForWrite(wrapper);

        // Assert
        var dict = (Dictionary<string, object?>)result!;
        var ids = (List<int>)dict["images"]!;
        ids.ShouldContain(400);
    }

    [Fact]
    public void PreprocessForWrite_WithIListOfRelation_ShouldConvertToDocumentIds()
    {
        // Arrange - 使用 IList<Category> 覆蓋 IsListOfStrapiCollectionType 的 IList 分支
        var wrapper = new ObjectWithIListRelation
        {
            Categories = new List<Category>
            {
                new Category { DocumentId = "cat-x", Name = "X" }
            }
        };

        // Act
        var result = _converter.ConvertForWrite(wrapper);

        // Assert
        var dict = (Dictionary<string, object?>)result!;
        var docIds = (List<string>)dict["categories"]!;
        docIds.ShouldContain("cat-x");
    }

    [Fact]
    public void PreprocessForWrite_WithIEnumerableOfRelation_ShouldConvertToDocumentIds()
    {
        // Arrange - 使用 IEnumerable<Category> 覆蓋 IsListOfStrapiCollectionType 的 IEnumerable 分支
        var wrapper = new ObjectWithIEnumerableRelation
        {
            Categories = new List<Category>
            {
                new Category { DocumentId = "cat-y", Name = "Y" }
            }
        };

        // Act
        var result = _converter.ConvertForWrite(wrapper);

        // Assert
        var dict = (Dictionary<string, object?>)result!;
        var docIds = (List<string>)dict["categories"]!;
        docIds.ShouldContain("cat-y");
    }

    [Fact]
    public void PreprocessForWrite_WithMediaListContainingZeroId_ShouldSkipZeroIds()
    {
        // Arrange - 覆蓋 ConvertMediaListToIds 中 media.Id <= 0 的分支
        var gallery = new GalleryWithoutConverterAttribute
        {
            Title = "Mixed Gallery",
            Images = new List<StrapiMediaField>
            {
                new StrapiMediaField { Id = 1, Name = "valid.jpg" },
                new StrapiMediaField { Id = 0, Name = "invalid.jpg" },  // Id = 0 應該被跳過
                new StrapiMediaField { Id = 2, Name = "valid2.jpg" }
            }
        };

        // Act
        var result = _converter.ConvertForWrite(gallery);

        // Assert
        var dict = (Dictionary<string, object?>)result!;
        var ids = (List<int>)dict["images"]!;
        ids.Count.ShouldBe(2);  // 只有 Id > 0 的
        ids.ShouldContain(1);
        ids.ShouldContain(2);
        ids.ShouldNotContain(0);
    }

    [Fact]
    public void PreprocessForWrite_WithMediaListContainingNull_ShouldSkipNullItems()
    {
        // Arrange - 覆蓋 ConvertMediaListToIds 中 item 不是 StrapiMediaField 的分支
        var gallery = new GalleryWithoutConverterAttribute
        {
            Title = "Gallery with null",
            Images = new List<StrapiMediaField>
            {
                new StrapiMediaField { Id = 5, Name = "valid.jpg" },
                null!,  // null 值不是 StrapiMediaField，會被跳過
                new StrapiMediaField { Id = 6, Name = "valid2.jpg" }
            }
        };

        // Act
        var result = _converter.ConvertForWrite(gallery);

        // Assert
        var dict = (Dictionary<string, object?>)result!;
        var ids = (List<int>)dict["images"]!;
        ids.Count.ShouldBe(2);  // 只有有效的 StrapiMediaField
        ids.ShouldContain(5);
        ids.ShouldContain(6);
    }

    [Fact]
    public void PreprocessForWrite_WithNonListGenericType_ShouldNotProcessAsList()
    {
        // Arrange - 使用非列表泛型類型（如 Dictionary）來覆蓋 IsListType 返回 false 的分支
        var wrapper = new ObjectWithDictionaryProperty
        {
            Metadata = new Dictionary<string, string>
            {
                { "key1", "value1" },
                { "key2", "value2" }
            }
        };

        // Act
        var result = _converter.ConvertForWrite(wrapper);

        // Assert
        var dict = (Dictionary<string, object?>)result!;
        // Dictionary 不是列表類型，應該保持原樣
        dict["metadata"].ShouldBeOfType<Dictionary<string, string>>();
    }

    [Fact]
    public void PreprocessForWrite_WithNonGenericArrayProperty_ShouldKeepAsIs()
    {
        // Arrange - 非泛型類型測試
        var wrapper = new ObjectWithNonGenericProperty
        {
            Value = 42,
            Name = "Test"
        };

        // Act
        var result = _converter.ConvertForWrite(wrapper);

        // Assert
        var dict = (Dictionary<string, object?>)result!;
        dict["value"].ShouldBe(42);
        dict["name"].ShouldBe("Test");
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
/// 測試用 - 包含字串列表的物件
/// </summary>
public class ObjectWithStringList
{
    public List<string>? Tags { get; set; }
}

/// <summary>
/// 測試用 - 使用 IList&lt;T&gt; 屬性的物件
/// </summary>
public class ObjectWithIListProperty
{
    public IList<string>? Items { get; set; }
}

/// <summary>
/// 測試用 - 使用 IEnumerable&lt;T&gt; 屬性的物件
/// </summary>
public class ObjectWithIEnumerableProperty
{
    public IEnumerable<string>? Items { get; set; }
}

/// <summary>
/// 測試用 - 使用 ICollection&lt;T&gt; 屬性的物件
/// </summary>
public class ObjectWithICollectionProperty
{
    public ICollection<string>? Items { get; set; }
}

/// <summary>
/// 測試用 - 使用 IList&lt;StrapiMediaField&gt; 屬性的物件
/// </summary>
public class ObjectWithIListMedia
{
    public IList<StrapiMediaField>? Images { get; set; }
}

/// <summary>
/// 測試用 - 使用 ICollection&lt;Category&gt; 屬性的物件
/// </summary>
public class ObjectWithICollectionRelation
{
    public ICollection<Category>? Categories { get; set; }
}

/// <summary>
/// 測試用 - 使用 IEnumerable&lt;IStrapiComponent&gt; 屬性的物件
/// </summary>
public class ObjectWithIEnumerableComponent
{
    public IEnumerable<IStrapiComponent>? Blocks { get; set; }
}

/// <summary>
/// 測試用 - 使用 IEnumerable&lt;StrapiMediaField&gt; 屬性的物件
/// </summary>
public class ObjectWithIEnumerableMedia
{
    public IEnumerable<StrapiMediaField>? Images { get; set; }
}

/// <summary>
/// 測試用 - 使用 ICollection&lt;StrapiMediaField&gt; 屬性的物件
/// </summary>
public class ObjectWithICollectionMedia
{
    public ICollection<StrapiMediaField>? Images { get; set; }
}

/// <summary>
/// 測試用 - 使用 IList&lt;Category&gt; 屬性的物件
/// </summary>
public class ObjectWithIListRelation
{
    public IList<Category>? Categories { get; set; }
}

/// <summary>
/// 測試用 - 使用 IEnumerable&lt;Category&gt; 屬性的物件
/// </summary>
public class ObjectWithIEnumerableRelation
{
    public IEnumerable<Category>? Categories { get; set; }
}

/// <summary>
/// 測試用 - 包含 Dictionary 屬性的物件（非列表泛型類型）
/// </summary>
public class ObjectWithDictionaryProperty
{
    public Dictionary<string, string>? Metadata { get; set; }
}

/// <summary>
/// 測試用 - 包含非泛型屬性的物件
/// </summary>
public class ObjectWithNonGenericProperty
{
    public int Value { get; set; }
    public string Name { get; set; } = string.Empty;
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
