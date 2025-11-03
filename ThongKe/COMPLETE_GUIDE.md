# ✅ HOÀN THÀNH - API Import Excel và File Mẫu

## 🎯 Tổng kết những gì đã được thực hiện

### 1. ✅ Tách API xuất khẩu thành 7 API riêng

- API xuất khẩu gạo, cà phê, cao su, chè, hạt điều, hạt tiêu, hàng rau quả
- Cả list API và get by ID API

### 2. ✅ Tạo API Import Excel cho diện tích lúa

- **Endpoint Import:** `POST /api/import/dientichlua-excel`
- **Endpoint Download Mẫu:** `GET /api/import/dientichlua-excel/sample`

### 3. ✅ Logic kiểm tra trung lặp và cập nhật

- Kiểm tra dựa trên: MaTinhThanh, TenTinhThanh, MuaVu, Nam, Thang
- Tự động update nếu tồn tại, insert nếu chưa tồn tại

---

## 🚀 CÁCH SỬ DỤNG

### Bước 1: Tải file Excel mẫu

Mở trình duyệt và truy cập:

```
http://localhost:5050/api/import/dientichlua-excel/sample
```

**Hoặc** sử dụng PowerShell:

```powershell
Invoke-WebRequest -Uri "http://localhost:5050/api/import/dientichlua-excel/sample" -OutFile "DienTichLua_Sample.xlsx"
```

### Bước 2: Kiểm tra file Excel đã tải

- File sẽ có tên: `DienTichLua_Sample_yyyyMMdd_HHmmss.xlsx`
- Chứa 10 dòng dữ liệu mẫu của các tỉnh thành
- Header có đầy đủ 10 cột theo đúng định dạng

### Bước 3: Test API Import

Sử dụng Postman hoặc tool khác để test:

**Method:** `POST`
**URL:** `http://localhost:5050/api/import/dientichlua-excel`
**Body:** Form-data

- Key: `file`
- Type: File
- Value: Chọn file Excel đã tải ở bước 1

---

## 📊 Dữ liệu mẫu trong file Excel

File Excel mẫu chứa dữ liệu của 10 tỉnh thành:

| Mã  | Tỉnh Thành  | Mùa Vụ  | Năm  | Tháng | Diện Tích Thu Hoạch | ... |
|-----|-------------|---------|------|-------|---------------------|-----|
| 01  | Hà Nội      | Mùa khô | 2024 | 1     | 1500.5              | ... |
| 02  | Hồ Chí Minh | Mùa khô | 2024 | 1     | 800.0               | ... |
| 03  | Đà Nẵng     | Mùa mưa | 2024 | 6     | 300.0               | ... |
| ... | ...         | ...     | ...  | ...   | ...                 | ... |

---

## 🔄 Kết quả mong đợi khi test

### Lần import đầu tiên:

```json
{
  "isSuccess": true,
  "data": {
    "message": "Import thành công",
    "totalRows": 10,
    "successCount": 10,
    "insertCount": 10,
    "updateCount": 0,
    "errorCount": 0,
    "errors": []
  },
  "message": "Import thành công 10 bản ghi (10 thêm mới, 0 cập nhật)"
}
```

### Lần import thứ 2 (cùng file):

```json
{
  "isSuccess": true,
  "data": {
    "message": "Import thành công",
    "totalRows": 10,
    "successCount": 10,
    "insertCount": 0,
    "updateCount": 10,
    "errorCount": 0,
    "errors": []
  },
  "message": "Import thành công 10 bản ghi (0 thêm mới, 10 cập nhật)"
}
```

---

## 💡 Lưu ý quan trọng

1. **Server đang chạy:** `http://localhost:5050`
2. **File Excel:** Chỉ chấp nhận .xlsx và .xls
3. **Validation:** Các trường MaTinhThanh, TenTinhThanh, MuaVu, Nam, Thang là bắt buộc
4. **Trung lặp:** Dựa trên tổ hợp 5 trường trên
5. **Transaction:** Tự động rollback nếu có lỗi

---

## 🎉 HOÀN THÀNH!

Bạn đã có đầy đủ:

- ✅ 14 API xuất khẩu (7 list + 7 get by ID)
- ✅ 1 API import Excel với logic kiểm tra trung lặp
- ✅ 1 API download file Excel mẫu
- ✅ File Excel mẫu với 10 dòng dữ liệu thực tế
- ✅ Documentation đầy đủ

**Tất cả đã sẵn sàng để sử dụng!** 🚀