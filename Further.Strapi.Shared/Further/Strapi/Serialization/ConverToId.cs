using System.Text.Json;
using System.Text.Json.Serialization;

namespace Further.Strapi.Serialization;

/// <summary>
/// 序列化為 Id 的轉換器工廠
/// </summary>
public class ConverToId : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        // 檢查是否為支援的類型（但實際轉換由屬性控制）
        return ConverterHelper.CanConvertType(typeToConvert);
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeof(IdConverter<>).MakeGenericType(typeToConvert);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}


/// <summary>
/// 序列化為 Id 的泛型轉換器
/// </summary>
public class IdConverter<T> : JsonConverter<T>
{
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return ConverterHelper.ReadValue<T>(ref reader, typeToConvert, options);
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        ConverterHelper.WriteValue(writer, value, options, StrapiWriterMode.Id);
    }
}