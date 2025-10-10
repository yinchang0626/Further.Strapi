using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Volo.Abp.Json;
using Xunit;

namespace Further.Strapi.Tests.Components;

/// <summary>
/// 基本多型序列化測試 - 測試手動配置的多型序列化
/// </summary>
public class BasicPolymorphismTests : StrapiIntegrationTestBase
{
    private readonly IJsonSerializer _jsonSerializer;

    public BasicPolymorphismTests()
    {
        _jsonSerializer = GetRequiredService<IJsonSerializer>();
    }

    [Fact]
    public void Should_Serialize_And_Deserialize_Component_With_Attribute()
    {
        // Arrange - 使用集合方式（這是多型序列化的正確方式）
        var components = new List<ITestComponent>
        {
            new TestQuoteComponentManual { Quote = "Test quote", Author = "Test author" }
        };

        // Act - 序列化 (使用 ABP 的 IJsonSerializer)
        var json = _jsonSerializer.Serialize(components, camelCase: true);

        // Assert - 檢查實際輸出
        json.ShouldNotBeNullOrWhiteSpace();
        
        // 輸出實際 JSON 以便檢查
        Console.WriteLine($"ABP Single Component in List JSON: {json}");
        
        // 如果我們的系統正確配置，應該包含 __component
        // (先看看實際輸出什麼)
    }

    [Fact]
    public void Should_Test_Native_JsonPolymorphic_Attribute()
    {
        // Arrange - 使用微軟原生 attribute，不使用自訂解析器
        var components = new List<ITestComponent>
        {
            new TestQuoteComponentManual { Quote = "Test quote", Author = "Test author" },
            new TestRichTextComponentManual { Content = "Rich content" }
        };

        // Act - 使用 ABP 的 IJsonSerializer（應該會使用原生 attribute）
        var json = _jsonSerializer.Serialize(components, camelCase: true);

        // Assert
        json.ShouldNotBeNullOrWhiteSpace();
        Console.WriteLine($"Native attribute JSON: {json}");
        
        // 測試單一物件是否也能正確序列化
        ITestComponent singleComponent = new TestQuoteComponentManual { Quote = "Single quote", Author = "Single author" };
        var singleJson = _jsonSerializer.Serialize(singleComponent, camelCase: true);
        Console.WriteLine($"Single component JSON: {singleJson}");
        
        // 嘗試明確指定類型進行序列化
        //var explicitTypeJson = _jsonSerializer.Serialize((object)singleComponent, camelCase: true);
        //Console.WriteLine($"Explicit type JSON: {explicitTypeJson}");
        
        // 嘗試使用原生 JsonSerializer 看看有無差異
        //var nativeJson = JsonSerializer.Serialize(singleComponent, new JsonSerializerOptions 
        //{ 
        //    PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
        //});
        //Console.WriteLine($"Native JsonSerializer JSON: {nativeJson}");
        
        singleJson.ShouldContain("\"__component\"");

        // 檢查是否包含 __component 分辨器
        json.ShouldContain("\"__component\"");
        json.ShouldContain("\"test.quote\"");
        json.ShouldContain("\"test.rich-text\"");
    }
}

/// <summary>
/// 測試用的基礎類別 - 使用微軟原生 attribute
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "__component")]
[JsonDerivedType(typeof(TestQuoteComponentManual), "test.quote")]
[JsonDerivedType(typeof(TestRichTextComponentManual), "test.rich-text")]
public interface ITestComponent
{
}

/// <summary>
/// 測試用的 Quote 組件
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "__component")]
[JsonDerivedType(typeof(TestQuoteComponentManual), "test.quote")]
public class TestQuoteComponentManual : ITestComponent
{
    public string Quote { get; set; } = "";
    public string Author { get; set; } = "";
}

/// <summary>
/// 測試用的 RichText 組件
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "__component")]
[JsonDerivedType(typeof(TestRichTextComponentManual), "test.rich-text")]
public class TestRichTextComponentManual : ITestComponent
{
    public string Content { get; set; } = "";
}