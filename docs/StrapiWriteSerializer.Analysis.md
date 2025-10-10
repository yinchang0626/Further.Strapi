# StrapiWriteSerializer 邏輯分析

## 實際處理流程

```
SerializeForUpdate(object)
    ↓
序列化為JSON字串
    ↓
解析為JsonElement
    ↓
CleanJsonElement(element, isInDynamicZone=false)
    ↓
根據ValueKind分岐：
    ├── Object → CleanJsonObject(obj, isInDynamicZone)
    ├── Array → 檢查是否為Component陣列，遞迴處理每個元素
    └── 其他 → 直接返回值
```

## 關鍵決策點分析

### 1. CleanJsonObject 的核心邏輯

```
CleanJsonObject(obj, isInDynamicZone)
    ↓
檢查是否有 "__component" 欄位
    ↓
分岐：
    ├── 有 "__component" (Component物件)
    │   ├── "id" → 總是移除
    │   ├── "__component" → 根據 isInDynamicZone 決定
    │   │   ├── isInDynamicZone=true → 保留
    │   │   └── isInDynamicZone=false → 移除
    │   └── 其他欄位 → 保留
    │
    └── 沒有 "__component" (一般物件)
        ├── SystemFieldNames ("id", "documentId", etc.) → 移除
        └── 其他欄位 → 保留
```

## 真正的問題

### ❌ 現有問題
1. **單一方法承擔多重責任**：`CleanJsonObject` 同時處理 Component 和一般物件
2. **複雜的巢狀條件**：if-else 邏輯難以閱讀和維護
3. **魔術字串**：`"__component"` 散佈各處

### ✅ 簡單的重構方案

**不需要策略模式**，只需要：

1. **提取方法**：將 Component 和一般物件的處理邏輯分離
2. **常數化**：將魔術字串集中管理
3. **清晰命名**：讓方法名稱反映實際業務邏輯

## 建議的簡化重構

```csharp
private Dictionary<string, object> CleanJsonObject(JsonElement obj, bool isInDynamicZone = false)
{
    var result = new Dictionary<string, object>();
    bool isComponent = obj.TryGetProperty(COMPONENT_FIELD, out _);
    
    foreach (var property in obj.EnumerateObject())
    {
        if (ShouldSkipField(property.Name, isComponent, isInDynamicZone))
            continue;
            
        var cleanedValue = CleanJsonElement(property.Value, isInDynamicZone: false);
        if (cleanedValue != null)
        {
            result[property.Name] = cleanedValue;
        }
    }
    
    return result;
}

private bool ShouldSkipField(string fieldName, bool isComponent, bool isInDynamicZone)
{
    if (isComponent)
    {
        return ShouldSkipComponentField(fieldName, isInDynamicZone);
    }
    else
    {
        return ShouldSkipRegularField(fieldName);
    }
}

private bool ShouldSkipComponentField(string fieldName, bool isInDynamicZone)
{
    if (fieldName.Equals("id", StringComparison.OrdinalIgnoreCase))
        return true;
        
    if (fieldName.Equals(COMPONENT_FIELD, StringComparison.OrdinalIgnoreCase))
        return !isInDynamicZone;  // 在DynamicZone中保留，否則移除
        
    return false;
}

private bool ShouldSkipRegularField(string fieldName)
{
    return SystemFieldNames.Contains(fieldName);
}
```

## 總結

**您說得對**：
- 策略模式在這裡是過度設計
- 上下文物件增加了不必要的複雜性
- 兩種處理邏輯的差異不足以支撐複雜的設計模式

**實際需要的只是**：
- 方法提取 (Extract Method)
- 常數集中 (Constant Extraction) 
- 邏輯分離 (Logic Separation)

這樣既保持了程式碼的簡潔性，又提升了可讀性和可維護性。