using System.Drawing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using ThongKe.Data;
using ThongKe.Entities;
using ThongKe.Shared;

namespace ThongKe.Controllers.CT;

[Route("api/import")]
[ApiController]
public class ImportController(AppDbContext context) : ControllerBase
{
    [HttpPost("dientichlua-excel")]
    public async Task<IActionResult> ImportExcelCongBoDienTichLua(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<object>.Fail(["Vui lòng chọn file Excel để import."]));

        var allowedExtensions = new[] { ".xlsx", ".xls" };
        var fileExtension = Path.GetExtension(file.FileName).ToLower();
        if (!allowedExtensions.Contains(fileExtension))
            return BadRequest(ApiResponse<object>.Fail(["Chỉ chấp nhận file Excel (.xlsx, .xls)."]));

        try
        {
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0;

            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets.FirstOrDefault();

            if (worksheet == null)
                return BadRequest(ApiResponse<object>.Fail(["File Excel không chứa worksheet nào."]));

            var rowCount = worksheet.Dimension?.Rows ?? 0;
            if (rowCount <= 1) // Chỉ có header hoặc không có dữ liệu
                return BadRequest(ApiResponse<object>.Fail(["File Excel không chứa dữ liệu để import."]));

            var errorMessages = new List<string>();
            var successCount = 0;
            var updateCount = 0;
            var insertCount = 0;

            // Bắt đầu từ row 2 (bỏ qua header)
            for (var row = 2; row <= rowCount; row++)
                try
                {
                    var maTinhThanh = worksheet.Cells[row, 1].Value?.ToString()?.Trim() ?? "";
                    var tenTinhThanh = worksheet.Cells[row, 2].Value?.ToString()?.Trim() ?? "";
                    var muaVu = worksheet.Cells[row, 3].Value?.ToString()?.Trim() ?? "";

                    if (!int.TryParse(worksheet.Cells[row, 4].Value?.ToString(), out var nam))
                    {
                        errorMessages.Add($"Dòng {row}: Năm không hợp lệ");
                        continue;
                    }

                    if (!int.TryParse(worksheet.Cells[row, 5].Value?.ToString(), out var thang))
                    {
                        errorMessages.Add($"Dòng {row}: Tháng không hợp lệ");
                        continue;
                    }

                    if (!double.TryParse(worksheet.Cells[row, 6].Value?.ToString(), out var dienTichThuHoach))
                        dienTichThuHoach = 0;

                    if (!double.TryParse(worksheet.Cells[row, 7].Value?.ToString(), out var dienTichGieoTrong))
                        dienTichGieoTrong = 0;

                    if (!double.TryParse(worksheet.Cells[row, 8].Value?.ToString(), out var dienTichUocTinhGieoTrong))
                        dienTichUocTinhGieoTrong = 0;

                    if (!double.TryParse(worksheet.Cells[row, 9].Value?.ToString(), out var dienTichUocTinhThuHoach))
                        dienTichUocTinhThuHoach = 0;

                    var donViTinh = worksheet.Cells[row, 10].Value?.ToString()?.Trim() ?? "";

                    if (string.IsNullOrEmpty(maTinhThanh) || string.IsNullOrEmpty(tenTinhThanh) ||
                        string.IsNullOrEmpty(muaVu))
                    {
                        errorMessages.Add(
                            $"Dòng {row}: Thiếu thông tin bắt buộc (Mã tỉnh thành, Tên tỉnh thành, Mùa vụ)");
                        continue;
                    }

                    var existingRecord = await context.CongBoDienTichLuas
                        .FirstOrDefaultAsync(x =>
                            x.MaTinhThanh == maTinhThanh &&
                            x.TenTinhThanh == tenTinhThanh &&
                            x.MuaVu == muaVu &&
                            x.Nam == nam &&
                            x.Thang == thang);

                    if (existingRecord != null)
                    {
                        existingRecord.DienTichThuHoach = dienTichThuHoach;
                        existingRecord.DienTichGieoTrong = dienTichGieoTrong;
                        existingRecord.DienTichUocTinhGieoTrong = dienTichUocTinhGieoTrong;
                        existingRecord.DienTichUocTinhThuHoach = dienTichUocTinhThuHoach;
                        existingRecord.DonViTinh = donViTinh;
                        existingRecord.UpdatedAt = DateTime.UtcNow;

                        context.CongBoDienTichLuas.Update(existingRecord);
                        updateCount++;
                    }
                    else
                    {
                        var newRecord = new CongBoDienTichLua
                        {
                            MaTinhThanh = maTinhThanh,
                            TenTinhThanh = tenTinhThanh,
                            MuaVu = muaVu,
                            Nam = nam,
                            Thang = thang,
                            DienTichThuHoach = dienTichThuHoach,
                            DienTichGieoTrong = dienTichGieoTrong,
                            DienTichUocTinhGieoTrong = dienTichUocTinhGieoTrong,
                            DienTichUocTinhThuHoach = dienTichUocTinhThuHoach,
                            DonViTinh = donViTinh,
                            IsCongBo = 0,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        context.CongBoDienTichLuas.Add(newRecord);
                        insertCount++;
                    }

                    successCount++;
                }
                catch (Exception ex)
                {
                    errorMessages.Add($"Dòng {row}: Lỗi xử lý - {ex.Message}");
                }

            if (successCount > 0) await context.SaveChangesAsync();

            var result = new
            {
                TotalRows = rowCount - 1,
                SuccessCount = successCount,
                InsertCount = insertCount,
                UpdateCount = updateCount,
                ErrorCount = errorMessages.Count,
                Errors = errorMessages
            };

            var message = $"Import thành công {successCount} bản ghi ({insertCount} thêm mới, {updateCount} cập nhật)";
            if (errorMessages.Count > 0)
                message += $", {errorMessages.Count} lỗi";

            return Ok(ApiResponse<object>.Ok(result, message));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.Fail([$"Lỗi khi xử lý file Excel: {ex.Message}"]));
        }
    }

    [HttpGet("dientichlua-excel/sample")]
    public IActionResult DownloadSampleExcel()
    {
        try
        {
            var sampleData = new[]
            {
                new
                {
                    MaTinhThanh = "01", TenTinhThanh = "Hà Nội", MuaVu = "Đông xuân", Nam = 2025, Thang = 1,
                    DienTichThuHoach = 1500.5, DienTichGieoTrong = 1600.0, DienTichUocTinhGieoTrong = 1650.0,
                    DienTichUocTinhThuHoach = 1580.0, DonViTinh = "1000 ha"
                },
                new
                {
                    MaTinhThanh = "01", TenTinhThanh = "Hà Nội", MuaVu = "Đông xuân", Nam = 2025, Thang = 2,
                    DienTichThuHoach = 1500.5, DienTichGieoTrong = 1600.0, DienTichUocTinhGieoTrong = 1650.0,
                    DienTichUocTinhThuHoach = 1580.0, DonViTinh = "1000 ha"
                },
                new
                {
                    MaTinhThanh = "01", TenTinhThanh = "Hà Nội", MuaVu = "Đông xuân", Nam = 2025, Thang = 3,
                    DienTichThuHoach = 1500.5, DienTichGieoTrong = 1600.0, DienTichUocTinhGieoTrong = 1650.0,
                    DienTichUocTinhThuHoach = 1580.0, DonViTinh = "1000 ha"
                },
                new
                {
                    MaTinhThanh = "01", TenTinhThanh = "Hà Nội", MuaVu = "Đông xuân", Nam = 2025, Thang = 4,
                    DienTichThuHoach = 1500.5, DienTichGieoTrong = 1600.0, DienTichUocTinhGieoTrong = 1650.0,
                    DienTichUocTinhThuHoach = 1580.0, DonViTinh = "1000 ha"
                },
                new
                {
                    MaTinhThanh = "01", TenTinhThanh = "Hà Nội", MuaVu = "Đông xuân", Nam = 2025, Thang = 5,
                    DienTichThuHoach = 1500.5, DienTichGieoTrong = 1600.0, DienTichUocTinhGieoTrong = 1650.0,
                    DienTichUocTinhThuHoach = 1580.0, DonViTinh = "1000 ha"
                },
                new
                {
                    MaTinhThanh = "01", TenTinhThanh = "Hà Nội", MuaVu = "Đông xuân", Nam = 2025, Thang = 6,
                    DienTichThuHoach = 1500.5, DienTichGieoTrong = 1600.0, DienTichUocTinhGieoTrong = 1650.0,
                    DienTichUocTinhThuHoach = 1580.0, DonViTinh = "1000 ha"
                },
                new
                {
                    MaTinhThanh = "01", TenTinhThanh = "Hà Nội", MuaVu = "Đông xuân", Nam = 2025, Thang = 7,
                    DienTichThuHoach = 1500.5, DienTichGieoTrong = 1600.0, DienTichUocTinhGieoTrong = 1650.0,
                    DienTichUocTinhThuHoach = 1580.0, DonViTinh = "1000 ha"
                },
                new
                {
                    MaTinhThanh = "01", TenTinhThanh = "Hà Nội", MuaVu = "Đông xuân", Nam = 2025, Thang = 8,
                    DienTichThuHoach = 1500.5, DienTichGieoTrong = 1600.0, DienTichUocTinhGieoTrong = 1650.0,
                    DienTichUocTinhThuHoach = 1580.0, DonViTinh = "1000 ha"
                },
                new
                {
                    MaTinhThanh = "01", TenTinhThanh = "Hà Nội", MuaVu = "Đông xuân", Nam = 2025, Thang = 9,
                    DienTichThuHoach = 1500.5, DienTichGieoTrong = 1600.0, DienTichUocTinhGieoTrong = 1650.0,
                    DienTichUocTinhThuHoach = 1580.0, DonViTinh = "1000 ha"
                },
                new
                {
                    MaTinhThanh = "01", TenTinhThanh = "Hà Nội", MuaVu = "Đông xuân", Nam = 2025, Thang = 10,
                    DienTichThuHoach = 1500.5, DienTichGieoTrong = 1600.0, DienTichUocTinhGieoTrong = 1650.0,
                    DienTichUocTinhThuHoach = 1580.0, DonViTinh = "1000 ha"
                },
                new
                {
                    MaTinhThanh = "01", TenTinhThanh = "Hà Nội", MuaVu = "Đông xuân", Nam = 2025, Thang = 11,
                    DienTichThuHoach = 1500.5, DienTichGieoTrong = 1600.0, DienTichUocTinhGieoTrong = 1650.0,
                    DienTichUocTinhThuHoach = 1580.0, DonViTinh = "1000 ha"
                },
                new
                {
                    MaTinhThanh = "01", TenTinhThanh = "Hà Nội", MuaVu = "Đông xuân", Nam = 2025, Thang = 12,
                    DienTichThuHoach = 1500.5, DienTichGieoTrong = 1600.0, DienTichUocTinhGieoTrong = 1650.0,
                    DienTichUocTinhThuHoach = 1580.0, DonViTinh = "1000 ha"
                }
            };

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("DienTichLua");

            // Header
            var headers = new[]
            {
                "MaTinhThanh", "TenTinhThanh", "MuaVu", "Nam", "Thang", "DienTichThuHoach", "DienTichGieoTrong",
                "DienTichUocTinhGieoTrong", "DienTichUocTinhThuHoach", "DonViTinh"
            };
            for (var i = 0; i < headers.Length; i++) worksheet.Cells[1, i + 1].Value = headers[i];

            // Style header
            using (var headerRange = worksheet.Cells[1, 1, 1, headers.Length])
            {
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                headerRange.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
                headerRange.Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }

            // Data
            for (var i = 0; i < sampleData.Length; i++)
            {
                var row = i + 2;
                var data = sampleData[i];

                worksheet.Cells[row, 1].Value = data.MaTinhThanh;
                worksheet.Cells[row, 2].Value = data.TenTinhThanh;
                worksheet.Cells[row, 3].Value = data.MuaVu;
                worksheet.Cells[row, 4].Value = data.Nam;
                worksheet.Cells[row, 5].Value = data.Thang;
                worksheet.Cells[row, 6].Value = data.DienTichThuHoach;
                worksheet.Cells[row, 7].Value = data.DienTichGieoTrong;
                worksheet.Cells[row, 8].Value = data.DienTichUocTinhGieoTrong;
                worksheet.Cells[row, 9].Value = data.DienTichUocTinhThuHoach;
                worksheet.Cells[row, 10].Value = data.DonViTinh;
            }

            // Auto fit columns
            worksheet.Cells.AutoFitColumns();

            // Convert to byte array
            var fileBytes = package.GetAsByteArray();
            var fileName = $"DienTichLua_Sample_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.Fail([$"Lỗi khi tạo file Excel mẫu: {ex.Message}"]));
        }
    }

    [HttpPost("cayhangnamkhac-excel")]
    public async Task<IActionResult> ImportExcelCongBoDienTichCayHangNamKhac(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<object>.Fail(["Vui lòng chọn file Excel để import."]));

        var allowedExtensions = new[] { ".xlsx", ".xls" };
        var fileExtension = Path.GetExtension(file.FileName).ToLower();
        if (!allowedExtensions.Contains(fileExtension))
            return BadRequest(ApiResponse<object>.Fail(["Chỉ chấp nhận file Excel (.xlsx, .xls)."]));

        try
        {
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0;

            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets.FirstOrDefault();

            if (worksheet == null)
                return BadRequest(ApiResponse<object>.Fail(["File Excel không chứa worksheet nào."]));

            var rowCount = worksheet.Dimension?.Rows ?? 0;
            if (rowCount <= 1) // Chỉ có header hoặc không có dữ liệu
                return BadRequest(ApiResponse<object>.Fail(["File Excel không chứa dữ liệu để import."]));

            var errorMessages = new List<string>();
            var successCount = 0;
            var updateCount = 0;
            var insertCount = 0;

            // Bắt đầu từ row 2 (bỏ qua header)
            for (var row = 2; row <= rowCount; row++)
                try
                {
                    var maTinhThanh = worksheet.Cells[row, 1].Value?.ToString()?.Trim() ?? "";
                    var tenTinhThanh = worksheet.Cells[row, 2].Value?.ToString()?.Trim() ?? "";
                    var loaiCay = worksheet.Cells[row, 3].Value?.ToString()?.Trim() ?? "";

                    if (!int.TryParse(worksheet.Cells[row, 4].Value?.ToString(), out var nam))
                    {
                        errorMessages.Add($"Dòng {row}: Năm không hợp lệ");
                        continue;
                    }

                    if (!int.TryParse(worksheet.Cells[row, 5].Value?.ToString(), out var thang))
                    {
                        errorMessages.Add($"Dòng {row}: Tháng không hợp lệ");
                        continue;
                    }

                    if (!double.TryParse(worksheet.Cells[row, 6].Value?.ToString(), out var dienTichThuHoach))
                        dienTichThuHoach = 0;

                    if (!double.TryParse(worksheet.Cells[row, 7].Value?.ToString(), out var dienTichGieoTrong))
                        dienTichGieoTrong = 0;

                    if (!double.TryParse(worksheet.Cells[row, 8].Value?.ToString(), out var dienTichUocTinhGieoTrong))
                        dienTichUocTinhGieoTrong = 0;

                    if (!double.TryParse(worksheet.Cells[row, 9].Value?.ToString(), out var dienTichUocTinhThuHoach))
                        dienTichUocTinhThuHoach = 0;

                    var donViTinh = worksheet.Cells[row, 10].Value?.ToString()?.Trim() ?? "";

                    if (string.IsNullOrEmpty(maTinhThanh) || string.IsNullOrEmpty(tenTinhThanh) ||
                        string.IsNullOrEmpty(loaiCay))
                    {
                        errorMessages.Add(
                            $"Dòng {row}: Thiếu thông tin bắt buộc (Mã tỉnh thành, Tên tỉnh thành, Loại cây)");
                        continue;
                    }

                    var existingRecord = await context.CongBoDienTichCayHangNamKhacs
                        .FirstOrDefaultAsync(x =>
                            x.MaTinhThanh == maTinhThanh &&
                            x.TenTinhThanh == tenTinhThanh &&
                            x.LoaiCay == loaiCay &&
                            x.Nam == nam &&
                            x.Thang == thang);

                    if (existingRecord != null)
                    {
                        existingRecord.DienTichThuHoach = dienTichThuHoach;
                        existingRecord.DienTichGieoTrong = dienTichGieoTrong;
                        existingRecord.DienTichUocTinhGieoTrong = dienTichUocTinhGieoTrong;
                        existingRecord.DienTichUocTinhThuHoach = dienTichUocTinhThuHoach;
                        existingRecord.DonViTinh = donViTinh;
                        existingRecord.UpdatedAt = DateTime.UtcNow;

                        context.CongBoDienTichCayHangNamKhacs.Update(existingRecord);
                        updateCount++;
                    }
                    else
                    {
                        var newRecord = new CongBoDienTichCayHangNamKhac
                        {
                            MaTinhThanh = maTinhThanh,
                            TenTinhThanh = tenTinhThanh,
                            LoaiCay = loaiCay,
                            Nam = nam,
                            Thang = thang,
                            DienTichThuHoach = dienTichThuHoach,
                            DienTichGieoTrong = dienTichGieoTrong,
                            DienTichUocTinhGieoTrong = dienTichUocTinhGieoTrong,
                            DienTichUocTinhThuHoach = dienTichUocTinhThuHoach,
                            DonViTinh = donViTinh,
                            IsCongBo = 0,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        context.CongBoDienTichCayHangNamKhacs.Add(newRecord);
                        insertCount++;
                    }

                    successCount++;
                }
                catch (Exception ex)
                {
                    errorMessages.Add($"Dòng {row}: Lỗi xử lý - {ex.Message}");
                }

            if (successCount > 0) await context.SaveChangesAsync();

            var result = new
            {
                TotalRows = rowCount - 1,
                SuccessCount = successCount,
                InsertCount = insertCount,
                UpdateCount = updateCount,
                ErrorCount = errorMessages.Count,
                Errors = errorMessages
            };

            var message = $"Import thành công {successCount} bản ghi ({insertCount} thêm mới, {updateCount} cập nhật)";
            if (errorMessages.Count > 0)
                message += $", {errorMessages.Count} lỗi";

            return Ok(ApiResponse<object>.Ok(result, message));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.Fail([$"Lỗi khi xử lý file Excel: {ex.Message}"]));
        }
    }

    [HttpGet("cayhangnamkhac-excel/sample")]
    public IActionResult DownloadSampleExcelCayHangNamKhac()
    {
        try
        {
            var sampleData = new[]
            {
                new
                {
                    MaTinhThanh = "01", TenTinhThanh = "Hà Nội", LoaiCay = "Cà phê", Nam = 2025, Thang = 1,
                    DienTichThuHoach = 1200.5, DienTichGieoTrong = 1300.0, DienTichUocTinhGieoTrong = 1350.0,
                    DienTichUocTinhThuHoach = 1280.0, DonViTinh = "1000 ha"
                },
                new
                {
                    MaTinhThanh = "01", TenTinhThanh = "Hà Nội", LoaiCay = "Cao su", Nam = 2025, Thang = 1,
                    DienTichThuHoach = 800.5, DienTichGieoTrong = 900.0, DienTichUocTinhGieoTrong = 950.0,
                    DienTichUocTinhThuHoach = 880.0, DonViTinh = "1000 ha"
                },
                new
                {
                    MaTinhThanh = "01", TenTinhThanh = "Hà Nội", LoaiCay = "Điều", Nam = 2025, Thang = 1,
                    DienTichThuHoach = 500.5, DienTichGieoTrong = 600.0, DienTichUocTinhGieoTrong = 650.0,
                    DienTichUocTinhThuHoach = 580.0, DonViTinh = "1000 ha"
                },
                new
                {
                    MaTinhThanh = "02", TenTinhThanh = "Hà Giang", LoaiCay = "Cà phê", Nam = 2025, Thang = 1,
                    DienTichThuHoach = 300.5, DienTichGieoTrong = 400.0, DienTichUocTinhGieoTrong = 450.0,
                    DienTichUocTinhThuHoach = 380.0, DonViTinh = "1000 ha"
                },
                new
                {
                    MaTinhThanh = "02", TenTinhThanh = "Hà Giang", LoaiCay = "Chè", Nam = 2025, Thang = 1,
                    DienTichThuHoach = 250.5, DienTichGieoTrong = 300.0, DienTichUocTinhGieoTrong = 320.0,
                    DienTichUocTinhThuHoach = 280.0, DonViTinh = "1000 ha"
                }
            };

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("DienTichCayHangNamKhac");

            // Header
            var headers = new[]
            {
                "MaTinhThanh", "TenTinhThanh", "LoaiCay", "Nam", "Thang", "DienTichThuHoach", "DienTichGieoTrong",
                "DienTichUocTinhGieoTrong", "DienTichUocTinhThuHoach", "DonViTinh"
            };
            for (var i = 0; i < headers.Length; i++) worksheet.Cells[1, i + 1].Value = headers[i];

            // Style header
            using (var headerRange = worksheet.Cells[1, 1, 1, headers.Length])
            {
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                headerRange.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
                headerRange.Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }

            // Data
            for (var i = 0; i < sampleData.Length; i++)
            {
                var row = i + 2;
                var data = sampleData[i];

                worksheet.Cells[row, 1].Value = data.MaTinhThanh;
                worksheet.Cells[row, 2].Value = data.TenTinhThanh;
                worksheet.Cells[row, 3].Value = data.LoaiCay;
                worksheet.Cells[row, 4].Value = data.Nam;
                worksheet.Cells[row, 5].Value = data.Thang;
                worksheet.Cells[row, 6].Value = data.DienTichThuHoach;
                worksheet.Cells[row, 7].Value = data.DienTichGieoTrong;
                worksheet.Cells[row, 8].Value = data.DienTichUocTinhGieoTrong;
                worksheet.Cells[row, 9].Value = data.DienTichUocTinhThuHoach;
                worksheet.Cells[row, 10].Value = data.DonViTinh;
            }

            // Auto fit columns
            worksheet.Cells.AutoFitColumns();

            // Convert to byte array
            var fileBytes = package.GetAsByteArray();
            var fileName = $"DienTichCayHangNamKhac_Sample_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.Fail([$"Lỗi khi tạo file Excel mẫu: {ex.Message}"]));
        }
    }

    [HttpPost("sanluongsanphamchannuoi-excel")]
    public async Task<IActionResult> ImportExcelCongBoSanLuongSanPhamChanNuoi(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<object>.Fail(["Vui lòng chọn file Excel để import."]));

        var allowedExtensions = new[] { ".xlsx", ".xls" };
        var fileExtension = Path.GetExtension(file.FileName).ToLower();
        if (!allowedExtensions.Contains(fileExtension))
            return BadRequest(ApiResponse<object>.Fail(["Chỉ chấp nhận file Excel (.xlsx, .xls)."]));

        try
        {
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0;

            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets.FirstOrDefault();

            if (worksheet == null)
                return BadRequest(ApiResponse<object>.Fail(["File Excel không chứa worksheet nào."]));

            var rowCount = worksheet.Dimension?.Rows ?? 0;
            if (rowCount <= 1) // Chỉ có header hoặc không có dữ liệu
                return BadRequest(ApiResponse<object>.Fail(["File Excel không chứa dữ liệu để import."]));

            var errorMessages = new List<string>();
            var successCount = 0;
            var updateCount = 0;
            var insertCount = 0;

            // Bắt đầu từ row 2 (bỏ qua header)
            for (var row = 2; row <= rowCount; row++)
                try
                {
                    var maTinhThanh = worksheet.Cells[row, 1].Value?.ToString()?.Trim() ?? "";
                    var tenTinhThanh = worksheet.Cells[row, 2].Value?.ToString()?.Trim() ?? "";

                    if (!int.TryParse(worksheet.Cells[row, 3].Value?.ToString(), out var nam))
                    {
                        errorMessages.Add($"Dòng {row}: Năm không hợp lệ");
                        continue;
                    }

                    if (!int.TryParse(worksheet.Cells[row, 4].Value?.ToString(), out var quy))
                    {
                        errorMessages.Add($"Dòng {row}: Quý không hợp lệ");
                        continue;
                    }

                    var tenSanPham = worksheet.Cells[row, 5].Value?.ToString()?.Trim() ?? "";
                    var donViTinh = worksheet.Cells[row, 6].Value?.ToString()?.Trim() ?? "";

                    if (!double.TryParse(worksheet.Cells[row, 7].Value?.ToString(), out var sanLuongThitHoi))
                        sanLuongThitHoi = 0;

                    if (!double.TryParse(worksheet.Cells[row, 8].Value?.ToString(), out var sanLuongChanNuoiKhac))
                        sanLuongChanNuoiKhac = 0;

                    if (string.IsNullOrEmpty(maTinhThanh) || string.IsNullOrEmpty(tenTinhThanh) ||
                        string.IsNullOrEmpty(tenSanPham))
                    {
                        errorMessages.Add(
                            $"Dòng {row}: Thiếu thông tin bắt buộc (Mã tỉnh thành, Tên tỉnh thành, Tên sản phẩm)");
                        continue;
                    }

                    // Kiểm tra quý hợp lệ (1-4)
                    if (quy < 1 || quy > 4)
                    {
                        errorMessages.Add($"Dòng {row}: Quý phải từ 1 đến 4");
                        continue;
                    }

                    var existingRecord = await context.CongBoSanLuongSanPhamChanNuois
                        .FirstOrDefaultAsync(x =>
                            x.MaTinhThanh == maTinhThanh &&
                            x.TenTinhThanh == tenTinhThanh &&
                            x.Nam == nam &&
                            x.Quy == quy &&
                            x.TenSanPham == tenSanPham);

                    if (existingRecord != null)
                    {
                        existingRecord.DonViTinh = donViTinh;
                        existingRecord.SanLuongThitHoi = sanLuongThitHoi;
                        existingRecord.SanLuongChanNuoiKhac = sanLuongChanNuoiKhac;
                        existingRecord.UpdatedAt = DateTime.UtcNow;

                        context.CongBoSanLuongSanPhamChanNuois.Update(existingRecord);
                        updateCount++;
                    }
                    else
                    {
                        var newRecord = new CongBoSanLuongSanPhamChanNuoi
                        {
                            MaTinhThanh = maTinhThanh,
                            TenTinhThanh = tenTinhThanh,
                            Nam = nam,
                            Quy = quy,
                            TenSanPham = tenSanPham,
                            DonViTinh = donViTinh,
                            SanLuongThitHoi = sanLuongThitHoi,
                            SanLuongChanNuoiKhac = sanLuongChanNuoiKhac,
                            IsCongBo = 0,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        context.CongBoSanLuongSanPhamChanNuois.Add(newRecord);
                        insertCount++;
                    }

                    successCount++;
                }
                catch (Exception ex)
                {
                    errorMessages.Add($"Dòng {row}: Lỗi xử lý - {ex.Message}");
                }

            if (successCount > 0) await context.SaveChangesAsync();

            var result = new
            {
                TotalRows = rowCount - 1,
                SuccessCount = successCount,
                InsertCount = insertCount,
                UpdateCount = updateCount,
                ErrorCount = errorMessages.Count,
                Errors = errorMessages
            };

            var message = $"Import thành công {successCount} bản ghi ({insertCount} thêm mới, {updateCount} cập nhật)";
            if (errorMessages.Count > 0)
                message += $", {errorMessages.Count} lỗi";

            return Ok(ApiResponse<object>.Ok(result, message));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.Fail([$"Lỗi khi xử lý file Excel: {ex.Message}"]));
        }
    }

    [HttpGet("sanluongsanphamchannuoi-excel/sample")]
    public IActionResult DownloadSampleExcelSanLuongSanPhamChanNuoi()
    {
        try
        {
            var sampleData = new[]
            {
                new
                {
                    MaTinhThanh = "01", TenTinhThanh = "Hà Nội", Nam = 2025, Quy = 1,
                    TenSanPham = "Thịt lợn", DonViTinh = "Tấn", SanLuongThitHoi = 1500.5, SanLuongChanNuoiKhac = 200.0
                },
                new
                {
                    MaTinhThanh = "01", TenTinhThanh = "Hà Nội", Nam = 2025, Quy = 1,
                    TenSanPham = "Thịt bò", DonViTinh = "Tấn", SanLuongThitHoi = 800.3, SanLuongChanNuoiKhac = 150.0
                },
                new
                {
                    MaTinhThanh = "01", TenTinhThanh = "Hà Nội", Nam = 2025, Quy = 1,
                    TenSanPham = "Thịt gà", DonViTinh = "Tấn", SanLuongThitHoi = 1200.8, SanLuongChanNuoiKhac = 300.0
                },
                new
                {
                    MaTinhThanh = "01", TenTinhThanh = "Hà Nội", Nam = 2025, Quy = 1,
                    TenSanPham = "Trứng gà", DonViTinh = "Triệu quả", SanLuongThitHoi = 0.0, SanLuongChanNuoiKhac = 450.5
                },
                new
                {
                    MaTinhThanh = "02", TenTinhThanh = "Hà Giang", Nam = 2025, Quy = 1,
                    TenSanPham = "Thịt lợn", DonViTinh = "Tấn", SanLuongThitHoi = 500.2, SanLuongChanNuoiKhac = 80.0
                },
                new
                {
                    MaTinhThanh = "02", TenTinhThanh = "Hà Giang", Nam = 2025, Quy = 1,
                    TenSanPham = "Sữa bò", DonViTinh = "1000 lít", SanLuongThitHoi = 0.0, SanLuongChanNuoiKhac = 250.3
                },
                new
                {
                    MaTinhThanh = "03", TenTinhThanh = "Cao Bằng", Nam = 2025, Quy = 2,
                    TenSanPham = "Thịt trâu", DonViTinh = "Tấn", SanLuongThitHoi = 300.1, SanLuongChanNuoiKhac = 50.0
                },
                new
                {
                    MaTinhThanh = "03", TenTinhThanh = "Cao Bằng", Nam = 2025, Quy = 2,
                    TenSanPham = "Mật ong", DonViTinh = "Tấn", SanLuongThitHoi = 0.0, SanLuongChanNuoiKhac = 15.8
                }
            };

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("SanLuongSanPhamChanNuoi");

            // Header
            var headers = new[]
            {
                "MaTinhThanh", "TenTinhThanh", "Nam", "Quy", "TenSanPham", "DonViTinh", "SanLuongThitHoi", "SanLuongChanNuoiKhac"
            };
            for (var i = 0; i < headers.Length; i++) worksheet.Cells[1, i + 1].Value = headers[i];

            // Style header
            using (var headerRange = worksheet.Cells[1, 1, 1, headers.Length])
            {
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                headerRange.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
                headerRange.Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }

            // Data
            for (var i = 0; i < sampleData.Length; i++)
            {
                var row = i + 2;
                var data = sampleData[i];

                worksheet.Cells[row, 1].Value = data.MaTinhThanh;
                worksheet.Cells[row, 2].Value = data.TenTinhThanh;
                worksheet.Cells[row, 3].Value = data.Nam;
                worksheet.Cells[row, 4].Value = data.Quy;
                worksheet.Cells[row, 5].Value = data.TenSanPham;
                worksheet.Cells[row, 6].Value = data.DonViTinh;
                worksheet.Cells[row, 7].Value = data.SanLuongThitHoi;
                worksheet.Cells[row, 8].Value = data.SanLuongChanNuoiKhac;
            }

            // Auto fit columns
            worksheet.Cells.AutoFitColumns();

            // Convert to byte array
            var fileBytes = package.GetAsByteArray();
            var fileName = $"SanLuongSanPhamChanNuoi_Sample_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.Fail([$"Lỗi khi tạo file Excel mẫu: {ex.Message}"]));
        }
    }

    [HttpPost("xuatkhaunongsan-excel")]
    public async Task<IActionResult> ImportExcelCongBoXuatKhauNongSan(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<object>.Fail(["Vui lòng chọn file Excel để import."]));

        var allowedExtensions = new[] { ".xlsx", ".xls" };
        var fileExtension = Path.GetExtension(file.FileName).ToLower();
        if (!allowedExtensions.Contains(fileExtension))
            return BadRequest(ApiResponse<object>.Fail(["Chỉ chấp nhận file Excel (.xlsx, .xls)."]));

        try
        {
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0;

            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets.FirstOrDefault();

            if (worksheet == null)
                return BadRequest(ApiResponse<object>.Fail(["File Excel không chứa worksheet nào."]));

            var rowCount = worksheet.Dimension?.Rows ?? 0;
            if (rowCount <= 1) // Chỉ có header hoặc không có dữ liệu
                return BadRequest(ApiResponse<object>.Fail(["File Excel không chứa dữ liệu để import."]));

            var errorMessages = new List<string>();
            var successCount = 0;
            var updateCount = 0;
            var insertCount = 0;

            // Bắt đầu từ row 2 (bỏ qua header)
            for (var row = 2; row <= rowCount; row++)
                try
                {
                    var maQuocGiaVungLT = worksheet.Cells[row, 1].Value?.ToString()?.Trim() ?? "";
                    var tenQuocGiaVungLT = worksheet.Cells[row, 2].Value?.ToString()?.Trim() ?? "";

                    if (!int.TryParse(worksheet.Cells[row, 3].Value?.ToString(), out var nam))
                    {
                        errorMessages.Add($"Dòng {row}: Năm không hợp lệ");
                        continue;
                    }

                    if (!int.TryParse(worksheet.Cells[row, 4].Value?.ToString(), out var thang))
                    {
                        errorMessages.Add($"Dòng {row}: Tháng không hợp lệ");
                        continue;
                    }

                    var loaiNongSan = worksheet.Cells[row, 5].Value?.ToString()?.Trim() ?? "";
                    var donViKhoiLuong = worksheet.Cells[row, 6].Value?.ToString()?.Trim() ?? "";

                    if (!double.TryParse(worksheet.Cells[row, 7].Value?.ToString(), out var khoiLuong))
                        khoiLuong = 0;

                    var donViGiaTri = worksheet.Cells[row, 8].Value?.ToString()?.Trim() ?? "";

                    if (!double.TryParse(worksheet.Cells[row, 9].Value?.ToString(), out var giaTri))
                        giaTri = 0;

                    if (string.IsNullOrEmpty(maQuocGiaVungLT) || string.IsNullOrEmpty(tenQuocGiaVungLT) ||
                        string.IsNullOrEmpty(loaiNongSan))
                    {
                        errorMessages.Add(
                            $"Dòng {row}: Thiếu thông tin bắt buộc (Mã quốc gia/vùng lãnh thổ, Tên quốc gia/vùng lãnh thổ, Loại nông sản)");
                        continue;
                    }

                    // Kiểm tra tháng hợp lệ (1-12)
                    if (thang < 1 || thang > 12)
                    {
                        errorMessages.Add($"Dòng {row}: Tháng phải từ 1 đến 12");
                        continue;
                    }

                    // Kiểm tra loại nông sản hợp lệ
                    var validLoaiNongSan = new[] { "Gạo", "Cà phê", "Cao su", "Chè", "Hạt điều", "Hạt tiêu", "Hàng rau quả" };
                    if (!validLoaiNongSan.Contains(loaiNongSan))
                    {
                        errorMessages.Add($"Dòng {row}: Loại nông sản không hợp lệ. Chỉ chấp nhận: {string.Join(", ", validLoaiNongSan)}");
                        continue;
                    }

                    var existingRecord = await context.CongBoXuatKhauNongSans
                        .FirstOrDefaultAsync(x =>
                            x.MaQuocGiaVungLT == maQuocGiaVungLT &&
                            x.TenQuocGiaVungLT == tenQuocGiaVungLT &&
                            x.Nam == nam &&
                            x.Thang == thang &&
                            x.LoaiNongSan == loaiNongSan);

                    if (existingRecord != null)
                    {
                        existingRecord.DonViKhoiLuong = donViKhoiLuong;
                        existingRecord.KhoiLuong = khoiLuong;
                        existingRecord.DonViGiaTri = donViGiaTri;
                        existingRecord.GiaTri = giaTri;
                        existingRecord.UpdatedAt = DateTime.UtcNow;

                        context.CongBoXuatKhauNongSans.Update(existingRecord);
                        updateCount++;
                    }
                    else
                    {
                        var newRecord = new CongBoXuatKhauNongSan
                        {
                            MaQuocGiaVungLT = maQuocGiaVungLT,
                            TenQuocGiaVungLT = tenQuocGiaVungLT,
                            Nam = nam,
                            Thang = thang,
                            LoaiNongSan = loaiNongSan,
                            DonViKhoiLuong = donViKhoiLuong,
                            KhoiLuong = khoiLuong,
                            DonViGiaTri = donViGiaTri,
                            GiaTri = giaTri,
                            IsCongBo = 0,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        context.CongBoXuatKhauNongSans.Add(newRecord);
                        insertCount++;
                    }

                    successCount++;
                }
                catch (Exception ex)
                {
                    errorMessages.Add($"Dòng {row}: Lỗi xử lý - {ex.Message}");
                }

            if (successCount > 0) await context.SaveChangesAsync();

            var result = new
            {
                TotalRows = rowCount - 1,
                SuccessCount = successCount,
                InsertCount = insertCount,
                UpdateCount = updateCount,
                ErrorCount = errorMessages.Count,
                Errors = errorMessages
            };

            var message = $"Import thành công {successCount} bản ghi ({insertCount} thêm mới, {updateCount} cập nhật)";
            if (errorMessages.Count > 0)
                message += $", {errorMessages.Count} lỗi";

            return Ok(ApiResponse<object>.Ok(result, message));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.Fail([$"Lỗi khi xử lý file Excel: {ex.Message}"]));
        }
    }

    [HttpGet("xuatkhaunongsan-excel/sample")]
    public IActionResult DownloadSampleExcelXuatKhauNongSan()
    {
        try
        {
            var sampleData = new[]
            {
                new
                {
                    MaQuocGiaVungLT = "CN", TenQuocGiaVungLT = "Trung Quốc", Nam = 2025, Thang = 1,
                    LoaiNongSan = "Gạo", DonViKhoiLuong = "Tấn", KhoiLuong = 15000.5, DonViGiaTri = "1000 USD", GiaTri = 7500.3
                },
                new
                {
                    MaQuocGiaVungLT = "CN", TenQuocGiaVungLT = "Trung Quốc", Nam = 2025, Thang = 1,
                    LoaiNongSan = "Cà phê", DonViKhoiLuong = "Tấn", KhoiLuong = 8500.2, DonViGiaTri = "1000 USD", GiaTri = 12750.8
                },
                new
                {
                    MaQuocGiaVungLT = "US", TenQuocGiaVungLT = "Hoa Kỳ", Nam = 2025, Thang = 1,
                    LoaiNongSan = "Hạt điều", DonViKhoiLuong = "Tấn", KhoiLuong = 2500.0, DonViGiaTri = "1000 USD", GiaTri = 15000.0
                },
                new
                {
                    MaQuocGiaVungLT = "US", TenQuocGiaVungLT = "Hoa Kỳ", Nam = 2025, Thang = 1,
                    LoaiNongSan = "Hạt tiêu", DonViKhoiLuong = "Tấn", KhoiLuong = 1200.5, DonViGiaTri = "1000 USD", GiaTri = 8500.0
                },
                new
                {
                    MaQuocGiaVungLT = "JP", TenQuocGiaVungLT = "Nhật Bản", Nam = 2025, Thang = 2,
                    LoaiNongSan = "Hàng rau quả", DonViKhoiLuong = "Tấn", KhoiLuong = 5500.3, DonViGiaTri = "1000 USD", GiaTri = 9200.5
                },
                new
                {
                    MaQuocGiaVungLT = "DE", TenQuocGiaVungLT = "Đức", Nam = 2025, Thang = 2,
                    LoaiNongSan = "Chè", DonViKhoiLuong = "Tấn", KhoiLuong = 800.2, DonViGiaTri = "1000 USD", GiaTri = 2400.8
                },
                new
                {
                    MaQuocGiaVungLT = "MY", TenQuocGiaVungLT = "Malaysia", Nam = 2025, Thang = 3,
                    LoaiNongSan = "Cao su", DonViKhoiLuong = "Tấn", KhoiLuong = 12000.0, DonViGiaTri = "1000 USD", GiaTri = 18000.0
                },
                new
                {
                    MaQuocGiaVungLT = "IN", TenQuocGiaVungLT = "Ấn Độ", Nam = 2025, Thang = 3,
                    LoaiNongSan = "Gạo", DonViKhoiLuong = "Tấn", KhoiLuong = 25000.8, DonViGiaTri = "1000 USD", GiaTri = 11250.5
                }
            };

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("XuatKhauNongSan");

            // Header
            var headers = new[]
            {
                "MaQuocGiaVungLT", "TenQuocGiaVungLT", "Nam", "Thang", "LoaiNongSan", "DonViKhoiLuong", "KhoiLuong", "DonViGiaTri", "GiaTri"
            };
            for (var i = 0; i < headers.Length; i++) worksheet.Cells[1, i + 1].Value = headers[i];

            // Style header
            using (var headerRange = worksheet.Cells[1, 1, 1, headers.Length])
            {
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                headerRange.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
                headerRange.Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }

            // Data
            for (var i = 0; i < sampleData.Length; i++)
            {
                var row = i + 2;
                var data = sampleData[i];

                worksheet.Cells[row, 1].Value = data.MaQuocGiaVungLT;
                worksheet.Cells[row, 2].Value = data.TenQuocGiaVungLT;
                worksheet.Cells[row, 3].Value = data.Nam;
                worksheet.Cells[row, 4].Value = data.Thang;
                worksheet.Cells[row, 5].Value = data.LoaiNongSan;
                worksheet.Cells[row, 6].Value = data.DonViKhoiLuong;
                worksheet.Cells[row, 7].Value = data.KhoiLuong;
                worksheet.Cells[row, 8].Value = data.DonViGiaTri;
                worksheet.Cells[row, 9].Value = data.GiaTri;
            }

            // Auto fit columns
            worksheet.Cells.AutoFitColumns();

            // Convert to byte array
            var fileBytes = package.GetAsByteArray();
            var fileName = $"XuatKhauNongSan_Sample_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.Fail([$"Lỗi khi tạo file Excel mẫu: {ex.Message}"]));
        }
    }
}