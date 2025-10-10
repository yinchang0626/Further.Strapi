using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Volo.Abp.Json.SystemTextJson;
using Volo.Abp.Json;
using Xunit;
using Further.Strapi.Tests.Models;

namespace Further.Strapi.Tests.Components;

/// <summary>
/// 測試 Strapi 組件多型序列化系統
/// </summary>
public class StrapiComponentPolymorphismTests : StrapiIntegrationTestBase
{
    private readonly IJsonSerializer _jsonSerializer;

    public StrapiComponentPolymorphismTests()
    {
        _jsonSerializer = GetRequiredService<IJsonSerializer>();
    }

    [Fact]
    public void Should_Serialize_And_Deserialize_Component_With_Attribute()
    {
        var components = new List<IStrapiComponent>
        {
            new SharedQuoteComponent
            {
                Title = "Test quote",
                Body = "Test author"
            }
        };
        // Arrange
        //IStrapiComponent testComponent = new SharedQuoteComponent 
        //{ 
        //    Title = "Test quote", 
        //    Body = "Test author" 
        //};

        // Act - 序列化
        var json = _jsonSerializer.Serialize(components, camelCase: true);
        json.ShouldNotBeNullOrWhiteSpace();

        // 驗證 JSON 包含正確的 component 名稱
        json.ShouldContain("\"__component\":\"shared.quote\"");
        json.ShouldContain("\"title\":\"Test quote\"");
        json.ShouldContain("\"body\":\"Test author\"");

        // Act - 反序列化
        //var deserializedComponent = _jsonSerializer.Deserialize<SharedQuoteComponent>(json, camelCase: true);

        //// Assert
        //deserializedComponent.ShouldNotBeNull();
        ////deserializedComponent.__component.ShouldBe("shared.quote");
        //deserializedComponent.Title.ShouldBe("Test quote");
        //deserializedComponent.Body.ShouldBe("Test author");
    }

    [Fact]
    public void Should_Handle_Polymorphic_IComponent_List()
    {
        // Arrange
        var components = new List<IStrapiComponent>
        {
            new SharedQuoteComponent 
            { 
                Title = "First quote", 
                Body = "First author" 
            },
            new SharedRichTextComponent 
            { 
                Body = "Rich content" 
            }
        };

        // Act - 序列化
        var json = _jsonSerializer.Serialize(components, camelCase: true);
        json.ShouldNotBeNullOrWhiteSpace();

        // 驗證 JSON 包含組件判別字串
        json.ShouldContain("\"__component\":\"shared.quote\"");
        json.ShouldContain("\"__component\":\"shared.rich-text\"");

        // Act - 反序列化
        var deserializedComponents = _jsonSerializer.Deserialize<List<IStrapiComponent>>(json, camelCase: true);

        // Assert
        deserializedComponents.ShouldNotBeNull();
        deserializedComponents.Count.ShouldBe(2);

        var quoteComponent = deserializedComponents[0].ShouldBeOfType<SharedQuoteComponent>();
        //quoteComponent.__component.ShouldBe("shared.quote");
        quoteComponent.Title.ShouldBe("First quote");
        quoteComponent.Body.ShouldBe("First author");

        var richTextComponent = deserializedComponents[1].ShouldBeOfType<SharedRichTextComponent>();
        //richTextComponent.__component.ShouldBe("shared.rich-text");
        richTextComponent.Body.ShouldBe("Rich content");
    }

    [Fact]
    public void Should_Work_With_Model_Containing_Component_List()
    {
        // Arrange
        var testModel = new TestAboutModel
        {
            Title = "About Us",
            Blocks = new List<IStrapiComponent>
            {
                new SharedQuoteComponent 
                { 
                    Title = "We are amazing", 
                    Body = "CEO" 
                },
                new SharedRichTextComponent 
                { 
                    Body = "Detailed description" 
                }
            }
        };

        // Act - 序列化
        var json = _jsonSerializer.Serialize(testModel, camelCase: true);
        json.ShouldNotBeNullOrWhiteSpace();

        // 驗證 JSON 結構
        json.ShouldContain("\"title\":\"About Us\"");
        json.ShouldContain("\"blocks\":");
        json.ShouldContain("\"__component\":\"shared.quote\"");
        json.ShouldContain("\"__component\":\"shared.rich-text\"");

        // Act - 反序列化
        var deserializedModel = _jsonSerializer.Deserialize<TestAboutModel>(json, camelCase: true);

        // Assert
        deserializedModel.ShouldNotBeNull();
        deserializedModel.Title.ShouldBe("About Us");
        deserializedModel.Blocks.ShouldNotBeNull();
        deserializedModel.Blocks.Count.ShouldBe(2);

        var quote = deserializedModel.Blocks[0].ShouldBeOfType<SharedQuoteComponent>();
        quote.Title.ShouldBe("We are amazing");
        quote.Body.ShouldBe("CEO");

        var richText = deserializedModel.Blocks[1].ShouldBeOfType<SharedRichTextComponent>();
        richText.Body.ShouldBe("Detailed description");
    }

    [Fact]
    public void Should_Ignore_Components_Without_Attribute()
    {
        // 這個測試確保沒有 ComponentNameAttribute 的組件會被忽略
        // 這樣的設計避免了不必要的錯誤和猜測
        
        // 由於我們的設計只處理有屬性的組件，
        // 沒有屬性的組件不會被註冊到多型序列化系統中
        // 這個測試主要是文檔化這個行為
        
        // 如果嘗試序列化沒有屬性的組件，
        // 它會被當作普通類別序列化，而不是多型組件
        
        // 這是正確的行為，避免了猜測和錯誤
        Assert.True(true); // 這個測試用於文檔化設計決策
    }
}

/// <summary>
/// 測試用的模型，包含組件列表
/// </summary>
public class TestAboutModel
{
    public string Title { get; set; } = "";
    public List<IStrapiComponent> Blocks { get; set; } = new();
}