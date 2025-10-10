using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Reflection;
using System.Collections;

namespace Further.Strapi.Serialization;



/// <summary>
/// 輔助類，包含共用的讀寫邏輯
/// </summary>
public static class ConverterHelper
{
    public static bool CanConvertType(Type typeToConvert)
    {
        // 檢查是否為 StrapiMediaField 或其 List
        if (typeToConvert == typeof(StrapiMediaField) || typeToConvert == typeof(List<StrapiMediaField>))
        {
            return true;
        }

        // 檢查是否為 Collection Type 或其 List
        if (typeToConvert.GetCustomAttribute<StrapiCollectionNameAttribute>() != null)
        {
            return true;
        }

        if (typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(List<>))
        {
            var elementType = typeToConvert.GetGenericArguments()[0];
            return elementType.GetCustomAttribute<StrapiCollectionNameAttribute>() != null;
        }

        return false;
    }

    public static T? ReadValue<T>(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return default;
        }

        // 處理 StrapiMediaField
        if (typeToConvert == typeof(StrapiMediaField))
        {
            return (T?)(object?)ReadMediaField(ref reader, options);
        }

        // 處理 List<StrapiMediaField>
        if (typeToConvert == typeof(List<StrapiMediaField>))
        {
            return (T?)(object?)ReadMediaFieldList(ref reader, options);
        }

        // 處理其他類型，使用標準反序列化
        using var document = JsonDocument.ParseValue(ref reader);
        var json = document.RootElement.GetRawText();
        
        var tempOptions = new JsonSerializerOptions(options);
        tempOptions.Converters.RemoveAll(c => c is ConverToDocumentId or ConverToId);
        
        return JsonSerializer.Deserialize<T>(json, tempOptions);
    }

    public static object? ReadValue(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        // 處理 StrapiMediaField
        if (typeToConvert == typeof(StrapiMediaField))
        {
            return ReadMediaField(ref reader, options);
        }

        // 處理 List<StrapiMediaField>
        if (typeToConvert == typeof(List<StrapiMediaField>))
        {
            return ReadMediaFieldList(ref reader, options);
        }

        // 處理其他類型，使用標準反序列化
        using var document = JsonDocument.ParseValue(ref reader);
        var json = document.RootElement.GetRawText();
        
        var tempOptions = new JsonSerializerOptions(options);
        tempOptions.Converters.RemoveAll(c => c is ConverToDocumentId or ConverToId);
        
        return JsonSerializer.Deserialize(json, typeToConvert, tempOptions);
    }

    public static void WriteValue(Utf8JsonWriter writer, object value, JsonSerializerOptions options, StrapiWriterMode mode)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        var valueType = value.GetType();

        // 處理 StrapiMediaField
        if (valueType == typeof(StrapiMediaField))
        {
            WriteMediaField(writer, (StrapiMediaField)value, mode);
            return;
        }

        // 處理 List<StrapiMediaField>
        if (valueType == typeof(List<StrapiMediaField>))
        {
            WriteMediaFieldList(writer, (List<StrapiMediaField>)value, mode);
            return;
        }

        // 處理單筆 Collection Type
        if (valueType.GetCustomAttribute<StrapiCollectionNameAttribute>() != null)
        {
            WriteSingleCollectionType(writer, value, valueType, options, mode);
            return;
        }

        // 處理 List<Collection Type>
        if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(List<>))
        {
            var elementType = valueType.GetGenericArguments()[0];
            if (elementType.GetCustomAttribute<StrapiCollectionNameAttribute>() != null)
            {
                WriteListCollectionType(writer, value, elementType, options, mode);
                return;
            }
        }

        // 不是支援的類型，使用一般序列化
        JsonSerializer.Serialize(writer, value, valueType, options);
    }

    private static void WriteSingleCollectionType(Utf8JsonWriter writer, object value, Type valueType, JsonSerializerOptions options, StrapiWriterMode mode)
    {
        switch (mode)
        {
            case StrapiWriterMode.Id:
                WriteIdValue(writer, value, valueType);
                break;
                
            case StrapiWriterMode.DocumentId:
                WriteDocumentIdValue(writer, value, valueType);
                break;
                
            case StrapiWriterMode.FullObject:
                WriteFullObject(writer, value, valueType, options);
                break;
        }
    }

    private static void WriteListCollectionType(Utf8JsonWriter writer, object value, Type elementType, JsonSerializerOptions options, StrapiWriterMode mode)
    {
        writer.WriteStartArray();
        
        if (value is IEnumerable enumerable)
        {
            foreach (var item in enumerable)
            {
                if (item == null)
                {
                    writer.WriteNullValue();
                    continue;
                }

                switch (mode)
                {
                    case StrapiWriterMode.Id:
                        WriteIdValue(writer, item, elementType);
                        break;
                        
                    case StrapiWriterMode.DocumentId:
                        WriteDocumentIdValue(writer, item, elementType);
                        break;
                        
                    case StrapiWriterMode.FullObject:
                        WriteFullObject(writer, item, elementType, options);
                        break;
                }
            }
        }
        
        writer.WriteEndArray();
    }

    private static void WriteIdValue(Utf8JsonWriter writer, object value, Type valueType)
    {
        var idProp = valueType.GetProperty("Id");
        if (idProp != null)
        {
            var id = idProp.GetValue(value);
            if (id != null)
            {
                JsonSerializer.Serialize(writer, id, idProp.PropertyType);
                return;
            }
        }
        writer.WriteNullValue();
    }

    private static void WriteDocumentIdValue(Utf8JsonWriter writer, object value, Type valueType)
    {
        var documentIdProp = valueType.GetProperty("DocumentId");
        if (documentIdProp != null)
        {
            var documentId = documentIdProp.GetValue(value) as string;
            if (!string.IsNullOrWhiteSpace(documentId))
            {
                writer.WriteStringValue(documentId);
                return;
            }
        }
        writer.WriteNullValue();
    }

    private static void WriteFullObject(Utf8JsonWriter writer, object value, Type valueType, JsonSerializerOptions options)
    {
        var tempOptions = new JsonSerializerOptions(options);
        tempOptions.Converters.RemoveAll(c => c is ConverToDocumentId or ConverToId);
        
        JsonSerializer.Serialize(writer, value, valueType, tempOptions);
    }

    private static void WriteMediaField(Utf8JsonWriter writer, StrapiMediaField value, StrapiWriterMode mode)
    {
        switch (mode)
        {
            case StrapiWriterMode.Id:
                if (value.Id > 0)
                {
                    writer.WriteNumberValue(value.Id);
                }
                else
                {
                    writer.WriteNullValue();
                }
                break;

            case StrapiWriterMode.DocumentId:
                if (!string.IsNullOrWhiteSpace(value.DocumentId))
                {
                    writer.WriteStringValue(value.DocumentId);
                }
                else
                {
                    writer.WriteNullValue();
                }
                break;

            case StrapiWriterMode.FullObject:
                JsonSerializer.Serialize(writer, value, typeof(StrapiMediaField));
                break;
        }
    }

    private static void WriteMediaFieldList(Utf8JsonWriter writer, List<StrapiMediaField> value, StrapiWriterMode mode)
    {
        writer.WriteStartArray();

        foreach (var item in value)
        {
            if (item == null)
            {
                writer.WriteNullValue();
            }
            else
            {
                WriteMediaField(writer, item, mode);
            }
        }

        writer.WriteEndArray();
    }

    private static StrapiMediaField? ReadMediaField(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType == JsonTokenType.Number)
        {
            var id = reader.GetInt32();
            return new StrapiMediaField { Id = id };
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var documentId = reader.GetString();
            return new StrapiMediaField { DocumentId = documentId ?? "" };
        }

        if (reader.TokenType == JsonTokenType.StartObject)
        {
            using var document = JsonDocument.ParseValue(ref reader);
            var json = document.RootElement.GetRawText();
            
            var tempOptions = new JsonSerializerOptions(options);
            tempOptions.Converters.RemoveAll(c => c is ConverToDocumentId or ConverToId);
            
            return JsonSerializer.Deserialize<StrapiMediaField>(json, tempOptions);
        }

        throw new JsonException($"Unexpected token type: {reader.TokenType}");
    }

    private static List<StrapiMediaField>? ReadMediaFieldList(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("Expected StartArray token");
        }

        var files = new List<StrapiMediaField>();
        
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
            {
                break;
            }

            var file = ReadMediaField(ref reader, options);
            if (file != null)
            {
                files.Add(file);
            }
        }

        return files;
    }
}