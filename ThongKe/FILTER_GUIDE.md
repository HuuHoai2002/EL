# Hướng dẫn sử dụng hệ thống Filter cho API Phân tích chỉ tiêu

## 🎯 Tổng quan

API Phân tích chỉ tiêu hỗ trợ 2 level filter:
1. **ChiTieuFilter**: Lọc dữ liệu ở từng chỉ tiêu (áp dụng trên SQL query)
2. **GroupByFilter**: Lọc kết quả sau khi group by theo phân tổ chung

## 📋 Các loại Filter Operation

```csharp
public enum FilterOperation
{
    Equal = 1,          // Bằng
    Greater = 2,        // Lớn hơn  
    Less = 3,           // Nhỏ hơn
    GreaterOrEqual = 4, // Lớn hơn hoặc bằng
    LessOrEqual = 5,    // Nhỏ hơn hoặc bằng
    Between = 6,        // Trong khoảng từ - đến
    Contains = 7,       // Chứa (cho string)
    StartsWith = 8,     // Bắt đầu bằng (cho string)
    EndsWith = 9        // Kết thúc bằng (cho string)
}
```

## 🔧 Cấu trúc Request

```json
{
  "ChiTieuIds": [1, 2, 3],
  "GroupByColumn": "ma_tinh_thanh",
  "ChiTieuFilters": [
    {
      "ChiTieuId": 1,
      "Conditions": [
        {
          "ColumnName": "nam",
          "Operation": 1,
          "Value": 2025
        },
        {
          "ColumnName": "gia_tri",
          "Operation": 6,
          "Value": 1000,
          "ValueTo": 5000
        }
      ]
    }
  ],
  "GroupByFilter": {
    "Conditions": [
      {
        "ColumnName": "ChiTieuSum",
        "Operation": 2,
        "Value": 10000
      }
    ]
  }
}
```

## 📊 Chi tiết Filter

### 1. ChiTieuFilter - Lọc dữ liệu từng chỉ tiêu

**Mục đích**: Lọc dữ liệu trong bảng của từng chỉ tiêu trước khi group by

**Cấu trúc**:
```json
{
  "ChiTieuId": 1,
  "Conditions": [
    {
      "ColumnName": "ten_cot_trong_bang",
      "Operation": 1-9,
      "Value": "gia_tri",
      "ValueTo": "gia_tri_den" // chỉ dùng cho Between
    }
  ]
}
```

**Ví dụ**:
```json
// Lọc chỉ tiêu ID=1: năm 2025, tháng từ 1-6
{
  "ChiTieuId": 1,
  "Conditions": [
    {
      "ColumnName": "nam",
      "Operation": 1,
      "Value": 2025
    },
    {
      "ColumnName": "thang", 
      "Operation": 6,
      "Value": 1,
      "ValueTo": 6
    }
  ]
}
```

### 2. GroupByFilter - Lọc kết quả sau group by

**Mục đích**: Lọc kết quả đã được group by theo phân tổ chung

**Columns có thể filter**:
- `PhanToValue`: Giá trị của phân tổ
- `ChiTieuSum`: Tổng giá trị chỉ tiêu trong group
- `RecordCount`: Số lượng record trong group  
- `PhanToKey`: Key của phân tổ

**Ví dụ**:
```json
// Chỉ lấy các group có tổng chỉ tiêu > 50000
{
  "Conditions": [
    {
      "ColumnName": "ChiTieuSum",
      "Operation": 2,
      "Value": 50000
    }
  ]
}
```

## 🚀 Ví dụ thực tế

### Ví dụ 1: Lọc theo năm và khoảng giá trị

```json
{
  "ChiTieuIds": [1, 2],
  "GroupByColumn": "ma_tinh_thanh",
  "ChiTieuFilters": [
    {
      "ChiTieuId": 1,
      "Conditions": [
        {
          "ColumnName": "nam",
          "Operation": 1,
          "Value": 2025
        },
        {
          "ColumnName": "gia_tri",
          "Operation": 6,
          "Value": 1000,
          "ValueTo": 10000
        }
      ]
    },
    {
      "ChiTieuId": 2, 
      "Conditions": [
        {
          "ColumnName": "nam",
          "Operation": 1,
          "Value": 2025
        }
      ]
    }
  ],
  "GroupByFilter": {
    "Conditions": [
      {
        "ColumnName": "ChiTieuSum",
        "Operation": 2,
        "Value": 5000
      }
    ]
  }
}
```

### Ví dụ 2: Lọc theo tên và số lượng record

```json
{
  "ChiTieuIds": [3],
  "GroupByColumn": "ten_tinh_thanh",
  "ChiTieuFilters": [
    {
      "ChiTieuId": 3,
      "Conditions": [
        {
          "ColumnName": "ten_tinh_thanh",
          "Operation": 7,
          "Value": "Hà"
        }
      ]
    }
  ],
  "GroupByFilter": {
    "Conditions": [
      {
        "ColumnName": "RecordCount",
        "Operation": 4,
        "Value": 10
      }
    ]
  }
}
```

## ⚠️ Lưu ý

1. **FilterOperation**: Sử dụng số nguyên (1-9) thay vì string
2. **ValueTo**: Chỉ sử dụng khi Operation = 6 (Between)
3. **ColumnName**: Phải khớp chính xác với tên cột trong database
4. **GroupByFilter**: Chỉ filter được 4 columns cố định
5. **Performance**: ChiTieuFilter sẽ nhanh hơn vì filter ở SQL level

## 🔍 Debugging

API sẽ log ra console:
- SQL query được thực thi
- Parameters được truyền vào
- Số lượng records được xử lý

Kiểm tra console để debug filter conditions.