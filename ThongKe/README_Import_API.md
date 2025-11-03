# API Import Excel - Diện Tích Lúa

## Mô tả

API này cho phép import dữ liệu diện tích gieo trồng và thu hoạch lúa từ file Excel vào cơ sở dữ liệu.

## Endpoints

### 1. Download File Excel Mẫu

**GET** `/api/import/dientichlua-excel/sample`

Tải xuống file Excel mẫu với dữ liệu demo để test API import.

```bash
curl -O -J "http://localhost:5050/api/import/dientichlua-excel/sample"
```

### 2. Import Dữ Liệu từ Excel

**POST** `/api/import/dientichlua-excel`

## Cách sử dụng

### Bước 1: Tải file Excel mẫu

```bash
curl -O -J "http://localhost:5050/api/import/dientichlua-excel/sample"
```

### Bước 2: Chỉnh sửa dữ liệu trong file Excel

File Excel đã tải về sẽ có sẵn dữ liệu mẫu. Bạn có thể chỉnh sửa hoặc thêm dữ liệu mới theo định dạng có sẵn.

### Bước 3: Upload file Excel để import

```bash
curl -X POST "http://localhost:5050/api/import/dientichlua-excel" \
  -H "Content-Type: multipart/form-data" \
  -F "file=@/path/to/your/excel/file.xlsx"
```

## Định dạng file Excel

File Excel cần có cấu trúc như sau (hàng đầu tiên là header):

| Cột | Tên cột                  | Kiểu dữ liệu | Bắt buộc | Mô tả                         |
|-----|--------------------------|--------------|----------|-------------------------------|
| A   | MaTinhThanh              | String       | Có       | Mã tỉnh thành                 |
| B   | TenTinhThanh             | String       | Có       | Tên tỉnh thành                |
| C   | MuaVu                    | String       | Có       | Mùa vụ (VD: Mùa khô, Mùa mưa) |
| D   | Nam                      | Number       | Có       | Năm                           |
| E   | Thang                    | Number       | Có       | Tháng                         |
| F   | DienTichThuHoach         | Number       | Không    | Diện tích thu hoạch           |
| G   | DienTichGieoTrong        | Number       | Không    | Diện tích gieo trồng          |
| H   | DienTichUocTinhGieoTrong | Number       | Không    | Diện tích ước tính gieo trồng |
| I   | DienTichUocTinhThuHoach  | Number       | Không    | Diện tích ước tính thu hoạch  |
| J   | DonViTinh                | String       | Không    | Đơn vị tính (VD: ha, m²)      |

### 3. Response

API sẽ trả về thông tin chi tiết về quá trình import:

**Thành công:**

```json
{
  "isSuccess": true,
  "data": {
    "message": "Import thành công",
    "totalRows": 100,
    "successCount": 95,
    "insertCount": 70,
    "updateCount": 25,
    "errorCount": 5,
    "errors": [
      "Dòng 10: Thiếu mã tỉnh thành",
      "Dòng 25: Năm không hợp lệ"
    ]
  },
  "message": "Import Excel thành công",
  "errors": []
}
```

**Lỗi:**

```json
{
  "isSuccess": false,
  "data": null,
  "message": "Import thất bại",
  "errors": [
    "Chỉ chấp nhận file Excel (.xlsx, .xls)."
  ]
}
```

## Logic xử lý

### Kiểm tra trung lặp

API sẽ kiểm tra trung lặp dựa trên 5 trường:

- `MaTinhThanh`
- `TenTinhThanh`
- `MuaVu`
- `Nam`
- `Thang`

### Xử lý dữ liệu

- **Nếu bản ghi đã tồn tại**: Cập nhật thông tin mới
- **Nếu bản ghi chưa tồn tại**: Tạo mới bản ghi
- **Nếu có lỗi**: Ghi lại lỗi và tiếp tục xử lý các dòng khác

### Validation

- File phải có định dạng .xlsx hoặc .xls
- File phải chứa ít nhất 1 worksheet
- Phải có ít nhất 1 dòng dữ liệu (ngoài header)
- Các trường bắt buộc phải có giá trị
- Năm và tháng phải là số nguyên hợp lệ

## Lưu ý

- File Excel cần tuân thủ đúng định dạng như mô tả
- API hỗ trợ xử lý hàng loạt, tự động commit transaction khi thành công
- Nếu có lỗi trong quá trình xử lý, transaction sẽ rollback để đảm bảo tính toàn vẹn dữ liệu
- Kích thước file tối đa có thể bị giới hạn bởi cấu hình server