---
agent: "ask"
description: "Generate i18n labels from c# enum declaration"
---

## Task

Convert C# enum to i18n labels in three languages: en, zh-Hans, zh-Hant. Read the comments of each enum member to get the label text in different languages.

## Input

Selected code: #selection

## Example

For the following C# enum declaration:

```csharp
public enum OrderStatus
{
    /// <summary>
    /// Processing
    /// 处理中
    /// </summary>
    Processing = 1,

    /// <summary>
    /// Shipped
    /// 已发货
    /// </summary>
    Shipped = 2,

    /// <summary>
    /// Cancelled
    /// 已取消
    /// </summary>
    Cancelled = 4
}
```

The generated i18n labels in JSON like format without '{' and '}' but with an ending comma and three languages (en, zh-Hans, zh-Hant) should be:

```txt
  "orderStatusCancelled": "Cancelled",
  "orderStatusProcessing": "Processing",
  "orderStatusShipped": "Shipped",
```

```txt
  "orderStatusCancelled": "已取消",
  "orderStatusProcessing": "处理中",
  "orderStatusShipped": "已发货",
```

```txt
  "orderStatusCancelled": "已取消",
  "orderStatusProcessing": "處理中",
  "orderStatusShipped": "已發貨",
```
