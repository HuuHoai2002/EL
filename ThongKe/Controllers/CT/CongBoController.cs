using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using ThongKe.Data;
using ThongKe.DTOs;
using ThongKe.Entities;
using ThongKe.Shared;
using ThongKe.Shared.Extensions;

namespace ThongKe.Controllers.CT;

[Route("api/congbo")]
[ApiController]
public class CongBoController(AppDbContext context) : ControllerBase
{
    //Diện tich gieo trồng và thu hoạch lúa
    [HttpPost("dientichlua/get-data-lists")]
    public async Task<IActionResult> GetPagedDataCongBoDienTichLua(CongBoDienTichLuaQueryDto data)
    {
        var query = context.CongBoDienTichLuas.AsQueryable();

        if (data.CreatedFrom.HasValue)
        {
            var utcCreatedFrom = data.CreatedFrom.Value.ToUtc();
            query = query.Where(t => t.CreatedAt >= utcCreatedFrom);
        }

        if (data.CreatedTo.HasValue)
        {
            var utcCreatedTo = data.CreatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.CreatedAt < utcCreatedTo);
        }

        if (data.UpdatedFrom.HasValue)
        {
            var utcUpdatedFrom = data.UpdatedFrom.Value.ToUtc();
            query = query.Where(t => t.UpdatedAt >= utcUpdatedFrom);
        }

        if (data.UpdatedTo.HasValue)
        {
            var utcUpdatedTo = data.UpdatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.UpdatedAt < utcUpdatedTo);
        }


        if (!string.IsNullOrEmpty(data.CreatedBy))
        {
            var createdBy = data.CreatedBy.ToLower();
            query = query.Where(t => (t.CreatedBy ?? "").ToLower().Contains(createdBy));
        }

        if (data.Nams != null && data.Nams.Count > 0) query = query.Where(t => data.Nams.Contains(t.Nam));
        if (data.Thangs != null && data.Thangs.Count > 0) query = query.Where(t => data.Thangs.Contains(t.Thang));
        if (data.MaTinhThanhs != null && data.MaTinhThanhs.Count > 0)
            query = query.Where(t => data.MaTinhThanhs.Contains(t.MaTinhThanh));
        if (data.MuaVus != null && data.MuaVus.Count > 0) query = query.Where(t => data.MuaVus.Contains(t.MuaVu));
        if (data.IsCongBo.HasValue) query = query.Where(t => t.IsCongBo == data.IsCongBo.Value);

        if (!string.IsNullOrEmpty(data.Keyword))
        {
            var keyword = data.Keyword.RemoveSpecialCharactersUnicode().ToLower();
            query = query.Where(t =>
                t.Nam.ToString().ToLower().Contains(keyword) ||
                t.Thang.ToString().ToLower().Contains(keyword));
        }

        var TongDienTichThuHoach = query.Sum(t => t.DienTichThuHoach);
        var TongDienTichGieoTrong = query.Sum(t => t.DienTichGieoTrong);
        var TongDienTichUocTinhGieoTrong = query.Sum(t => t.DienTichUocTinhGieoTrong);
        var TongDienTichUocTinhThuHoach = query.Sum(t => t.DienTichUocTinhThuHoach);

        // Áp dụng sắp xếp
        if (!string.IsNullOrEmpty(data.OrderBy))
        {
            var property = typeof(CongBoDienTichLua).GetProperty(data.OrderBy,
                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (property != null)
                query = data.Ascending
                    ? query.OrderBy(x => EF.Property<object>(x, property.Name))
                    : query.OrderByDescending(x => EF.Property<object>(x, property.Name));
        }

        PagedResult<CongBoDienTichLua> result;
        if (data.IsAllData == 1)
        {
            var allRecords = await query.ToListAsync();
            result = new PagedResult<CongBoDienTichLua>(allRecords, allRecords.Count, 1, allRecords.Count);
        }
        else
        {
            result = await query.ToPagedResultAsync(data.PageNumber, data.PageSize);
        }

        var response = new CongBoDienTichLuaPagedResponse
        {
            Data = result,
            TongDienTichThuHoach = TongDienTichThuHoach,
            TongDienTichGieoTrong = TongDienTichGieoTrong,
            TongDienTichUocTinhGieoTrong = TongDienTichUocTinhGieoTrong,
            TongDienTichUocTinhThuHoach = TongDienTichUocTinhThuHoach
        };

        return Ok(ApiResponse<CongBoDienTichLuaPagedResponse>.Ok(response));
    }

    [HttpPost("dientichlua/delete/{id:int}")]
    public async Task<IActionResult> DeleteCongBoDienTichLua(int id)
    {
        var existingData = await context.CongBoDienTichLuas.FindAsync(id);
        if (existingData == null)
            return BadRequest(ApiResponse<bool>.Fail(new List<string> { "Không tìm thấy dữ liệu cần xóa." }));
        if (existingData.IsCongBo == 1)
            return BadRequest(ApiResponse<bool>.Fail(new List<string> { "Không thể xóa dữ liệu đã được công bố." }));
        context.CongBoDienTichLuas.Remove(existingData);
        await context.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Ok(true, "Xóa dữ liệu thành công."));
    }

    [HttpPost("dientichlua/upsert")]
    public async Task<IActionResult> UpsertCongBoDienTichLua([FromBody] CongBoDienTichLua data)
    {
        if (data.Id > 0)
        {
            if (data.IsCongBo == 1)
                return BadRequest(ApiResponse<CongBoDienTichLua>.Fail(["Không thể cập dữ liệu đã được công bố."]));
            var existingData = await context.CongBoDienTichLuas.FindAsync(data.Id);
            if (existingData == null)
                return BadRequest(ApiResponse<CongBoDienTichLua>.Fail(["Không tìm thấy dữ liệu cần cập nhật."]));
            existingData.Assign(data);
            await context.SaveChangesAsync();
            return Ok(ApiResponse<CongBoDienTichLua>.Ok(existingData));
        }

        context.CongBoDienTichLuas.Add(data);
        await context.SaveChangesAsync();
        return Ok(ApiResponse<CongBoDienTichLua>.Ok(data));
    }

    [HttpGet("dientichlua/{id:int}")]
    public async Task<IActionResult> GetByIdCongBoDienTichLua(int id)
    {
        var result = await context.CongBoDienTichLuas
            .FindAsync(id);
        if (result == null)
            return NotFound(ApiResponse<CongBoDienTichLua>.Fail(["Không tìm thấy dữ liệu."]));

        return Ok(ApiResponse<CongBoDienTichLua>.Ok(result));
    }

    [HttpGet("dientichlua/changestatus/{id:int}/{iscongbo:int}")]
    public async Task<IActionResult> ChangeStatusCongBoDienTichLua(int id, int iscongbo)
    {
        var result = await context.CongBoDienTichLuas
            .FindAsync(id);
        if (result == null)
            return NotFound(ApiResponse<bool>.Fail(["Không tìm thấy dữ liệu."]));
        if (iscongbo != 0 && iscongbo != 1)
            return BadRequest(ApiResponse<bool>.Fail(["Trạng thái công bố không hợp lệ."]));
        if (result.IsCongBo == iscongbo)
            return BadRequest(ApiResponse<bool>.Fail(["Không có thay đổi về trạng thái công bố"]));
        result.IsCongBo = iscongbo;
        context.CongBoDienTichLuas.Update(result);
        await context.SaveChangesAsync();

        return Ok(ApiResponse<bool>.Ok(true, "Cập nhật trạng thái thành công."));
    }

    //[HttpPost("dientichlua/import-excel")]
    //public async Task<IActionResult> ImportExcelCongBoDienTichLua(IFormFile file)
    //{
    //    if (file == null || file.Length == 0)
    //        return BadRequest(ApiResponse<object>.Fail(["Vui lòng chọn file Excel để import."]));

    //    var allowedExtensions = new[] { ".xlsx", ".xls" };
    //    var fileExtension = Path.GetExtension(file.FileName).ToLower();
    //    if (!allowedExtensions.Contains(fileExtension))
    //        return BadRequest(ApiResponse<object>.Fail(["Chỉ chấp nhận file Excel (.xlsx, .xls)."]));

    //    try
    //    {
    //        using var stream = new MemoryStream();
    //        await file.CopyToAsync(stream);
    //        stream.Position = 0;

    //        using var package = new ExcelPackage(stream);
    //        var worksheet = package.Workbook.Worksheets.FirstOrDefault();

    //        if (worksheet == null)
    //            return BadRequest(ApiResponse<object>.Fail(["File Excel không chứa worksheet nào."]));

    //        var rowCount = worksheet.Dimension?.Rows ?? 0;
    //        if (rowCount <= 1) // Chỉ có header hoặc không có dữ liệu
    //            return BadRequest(ApiResponse<object>.Fail(["File Excel không chứa dữ liệu để import."]));

    //        var importResults = new List<object>();
    //        var errorMessages = new List<string>();
    //        var successCount = 0;
    //        var updateCount = 0;
    //        var insertCount = 0;

    //        // Bắt đầu từ row 2 (bỏ qua header)
    //        for (var row = 2; row <= rowCount; row++)
    //            try
    //            {
    //                // Đọc dữ liệu theo thứ tự cột trong model (bỏ qua Id, CreatedAt, UpdatedAt, IsCongBo, CreatedBy, UpdatedBy, NotMapped fields)
    //                var maTinhThanh = worksheet.Cells[row, 1].Value?.ToString()?.Trim() ?? "";
    //                var tenTinhThanh = worksheet.Cells[row, 2].Value?.ToString()?.Trim() ?? "";
    //                var muaVu = worksheet.Cells[row, 3].Value?.ToString()?.Trim() ?? "";

    //                if (!int.TryParse(worksheet.Cells[row, 4].Value?.ToString(), out var nam))
    //                {
    //                    errorMessages.Add($"Dòng {row}: Năm không hợp lệ");
    //                    continue;
    //                }

    //                if (!int.TryParse(worksheet.Cells[row, 5].Value?.ToString(), out var thang))
    //                {
    //                    errorMessages.Add($"Dòng {row}: Tháng không hợp lệ");
    //                    continue;
    //                }

    //                if (!double.TryParse(worksheet.Cells[row, 6].Value?.ToString(), out var dienTichThuHoach))
    //                    dienTichThuHoach = 0;

    //                if (!double.TryParse(worksheet.Cells[row, 7].Value?.ToString(), out var dienTichGieoTrong))
    //                    dienTichGieoTrong = 0;

    //                if (!double.TryParse(worksheet.Cells[row, 8].Value?.ToString(), out var dienTichUocTinhGieoTrong))
    //                    dienTichUocTinhGieoTrong = 0;

    //                if (!double.TryParse(worksheet.Cells[row, 9].Value?.ToString(), out var dienTichUocTinhThuHoach))
    //                    dienTichUocTinhThuHoach = 0;

    //                var donViTinh = worksheet.Cells[row, 10].Value?.ToString()?.Trim() ?? "";

    //                // Validate required fields
    //                if (string.IsNullOrEmpty(maTinhThanh) || string.IsNullOrEmpty(tenTinhThanh) ||
    //                    string.IsNullOrEmpty(muaVu))
    //                {
    //                    errorMessages.Add(
    //                        $"Dòng {row}: Thiếu thông tin bắt buộc (Mã tỉnh thành, Tên tỉnh thành, Mùa vụ)");
    //                    continue;
    //                }

    //                // Check if record exists based on unique combination
    //                var existingRecord = await context.CongBoDienTichLuas
    //                    .FirstOrDefaultAsync(x =>
    //                        x.MaTinhThanh == maTinhThanh &&
    //                        x.TenTinhThanh == tenTinhThanh &&
    //                        x.MuaVu == muaVu &&
    //                        x.Nam == nam &&
    //                        x.Thang == thang);

    //                if (existingRecord != null)
    //                {
    //                    // Update existing record
    //                    existingRecord.DienTichThuHoach = dienTichThuHoach;
    //                    existingRecord.DienTichGieoTrong = dienTichGieoTrong;
    //                    existingRecord.DienTichUocTinhGieoTrong = dienTichUocTinhGieoTrong;
    //                    existingRecord.DienTichUocTinhThuHoach = dienTichUocTinhThuHoach;
    //                    existingRecord.DonViTinh = donViTinh;
    //                    existingRecord.UpdatedAt = DateTime.UtcNow;

    //                    context.CongBoDienTichLuas.Update(existingRecord);
    //                    updateCount++;
    //                }
    //                else
    //                {
    //                    // Create new record
    //                    var newRecord = new CongBoDienTichLua
    //                    {
    //                        MaTinhThanh = maTinhThanh,
    //                        TenTinhThanh = tenTinhThanh,
    //                        MuaVu = muaVu,
    //                        Nam = nam,
    //                        Thang = thang,
    //                        DienTichThuHoach = dienTichThuHoach,
    //                        DienTichGieoTrong = dienTichGieoTrong,
    //                        DienTichUocTinhGieoTrong = dienTichUocTinhGieoTrong,
    //                        DienTichUocTinhThuHoach = dienTichUocTinhThuHoach,
    //                        DonViTinh = donViTinh,
    //                        IsCongBo = 0,
    //                        CreatedAt = DateTime.UtcNow,
    //                        UpdatedAt = DateTime.UtcNow
    //                    };

    //                    context.CongBoDienTichLuas.Add(newRecord);
    //                    insertCount++;
    //                }

    //                successCount++;
    //            }
    //            catch (Exception ex)
    //            {
    //                errorMessages.Add($"Dòng {row}: Lỗi xử lý - {ex.Message}");
    //            }

    //        if (successCount > 0) await context.SaveChangesAsync();

    //        var result = new
    //        {
    //            TotalRows = rowCount - 1,
    //            SuccessCount = successCount,
    //            InsertCount = insertCount,
    //            UpdateCount = updateCount,
    //            ErrorCount = errorMessages.Count,
    //            Errors = errorMessages
    //        };

    //        var message = $"Import thành công {successCount} bản ghi ({insertCount} thêm mới, {updateCount} cập nhật)";
    //        if (errorMessages.Count > 0)
    //            message += $", {errorMessages.Count} lỗi";

    //        return Ok(ApiResponse<object>.Ok(result, message));
    //    }
    //    catch (Exception ex)
    //    {
    //        return BadRequest(ApiResponse<object>.Fail([$"Lỗi khi xử lý file Excel: {ex.Message}"]));
    //    }
    //}

    //Diện tich gieo trồng và thu hoạch cây hàng năm khác
    [HttpPost("dientichcayhangnamkhac/get-data-lists")]
    public async Task<IActionResult> GetPagedDataCongBoDienTichCayHangNamKhac(CongBoDienTichCayHangNamKhacQueryDto data)
    {
        var query = context.CongBoDienTichCayHangNamKhacs.AsQueryable();

        if (data.CreatedFrom.HasValue)
        {
            var utcCreatedFrom = data.CreatedFrom.Value.ToUtc();
            query = query.Where(t => t.CreatedAt >= utcCreatedFrom);
        }

        if (data.CreatedTo.HasValue)
        {
            var utcCreatedTo = data.CreatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.CreatedAt < utcCreatedTo);
        }

        if (data.UpdatedFrom.HasValue)
        {
            var utcUpdatedFrom = data.UpdatedFrom.Value.ToUtc();
            query = query.Where(t => t.UpdatedAt >= utcUpdatedFrom);
        }

        if (data.UpdatedTo.HasValue)
        {
            var utcUpdatedTo = data.UpdatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.UpdatedAt < utcUpdatedTo);
        }


        if (!string.IsNullOrEmpty(data.CreatedBy))
        {
            var createdBy = data.CreatedBy.ToLower();
            query = query.Where(t => (t.CreatedBy ?? "").ToLower().Contains(createdBy));
        }

        if (data.Nams != null && data.Nams.Count > 0) query = query.Where(t => data.Nams.Contains(t.Nam));
        if (data.Thangs != null && data.Thangs.Count > 0) query = query.Where(t => data.Thangs.Contains(t.Thang));
        if (data.MaTinhThanhs != null && data.MaTinhThanhs.Count > 0)
            query = query.Where(t => data.MaTinhThanhs.Contains(t.MaTinhThanh));
        if (data.LoaiCays != null && data.LoaiCays.Count > 0)
            query = query.Where(t => data.LoaiCays.Contains(t.LoaiCay));
        if (data.IsCongBo.HasValue) query = query.Where(t => t.IsCongBo == data.IsCongBo.Value);

        if (!string.IsNullOrEmpty(data.Keyword))
        {
            var keyword = data.Keyword.RemoveSpecialCharactersUnicode().ToLower();
            query = query.Where(t =>
                t.Nam.ToString().ToLower().Contains(keyword) ||
                t.Thang.ToString().ToLower().Contains(keyword));
        }

        var TongDienTichThuHoach = query.Sum(t => t.DienTichThuHoach);
        var TongDienTichGieoTrong = query.Sum(t => t.DienTichGieoTrong);
        var TongDienTichUocTinhGieoTrong = query.Sum(t => t.DienTichUocTinhGieoTrong);
        var TongDienTichUocTinhThuHoach = query.Sum(t => t.DienTichUocTinhThuHoach);

        var result = await query.ToPagedResultAsync(
            data.PageNumber,
            data.PageSize,
            data.OrderBy,
            data.Ascending
        );

        var response = new CongBoDienTichCayHangNamKhacPagedResponse
        {
            Data = result,
            TongDienTichThuHoach = TongDienTichThuHoach,
            TongDienTichGieoTrong = TongDienTichGieoTrong,
            TongDienTichUocTinhGieoTrong = TongDienTichUocTinhGieoTrong,
            TongDienTichUocTinhThuHoach = TongDienTichUocTinhThuHoach
        };

        return Ok(ApiResponse<CongBoDienTichCayHangNamKhacPagedResponse>.Ok(response));
    }

    [HttpPost("dientichcayhangnamkhac/delete/{id:int}")]
    public async Task<IActionResult> DeleteCongBoDienTichCayHangNamKhac(int id)
    {
        var existingData = await context.CongBoDienTichCayHangNamKhacs.FindAsync(id);
        if (existingData == null)
            return BadRequest(ApiResponse<bool>.Fail(new List<string> { "Không tìm thấy dữ liệu cần xóa." }));
        if (existingData.IsCongBo == 1)
            return BadRequest(ApiResponse<bool>.Fail(new List<string> { "Không thể xóa dữ liệu đã được công bố." }));
        context.CongBoDienTichCayHangNamKhacs.Remove(existingData);
        await context.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Ok(true, "Xóa dữ liệu thành công."));
    }

    [HttpPost("dientichcayhangnamkhac/upsert")]
    public async Task<IActionResult> UpsertCongBoDienTichCayHangNamKhac([FromBody] CongBoDienTichCayHangNamKhac data)
    {
        if (data.Id > 0)
        {
            if (data.IsCongBo == 1)
                return BadRequest(
                    ApiResponse<CongBoDienTichCayHangNamKhac>.Fail(["Không thể cập dữ liệu đã được công bố."]));
            var existingData = await context.CongBoDienTichCayHangNamKhacs.FindAsync(data.Id);
            if (existingData == null)
                return BadRequest(
                    ApiResponse<CongBoDienTichCayHangNamKhac>.Fail(["Không tìm thấy dữ liệu cần cập nhật."]));
            existingData.Assign(data);
            await context.SaveChangesAsync();
            return Ok(ApiResponse<CongBoDienTichCayHangNamKhac>.Ok(existingData));
        }

        context.CongBoDienTichCayHangNamKhacs.Add(data);
        await context.SaveChangesAsync();
        return Ok(ApiResponse<CongBoDienTichCayHangNamKhac>.Ok(data));
    }

    [HttpGet("dientichcayhangnamkhac/{id:int}")]
    public async Task<IActionResult> GetByIdCongBoDienTichCayHangNamKhac(int id)
    {
        var result = await context.CongBoDienTichCayHangNamKhacs
            .FindAsync(id);
        if (result == null)
            return NotFound(ApiResponse<CongBoDienTichCayHangNamKhac>.Fail(["Không tìm thấy dữ liệu."]));

        return Ok(ApiResponse<CongBoDienTichCayHangNamKhac>.Ok(result));
    }

    [HttpGet("dientichcayhangnamkhac/changestatus/{id:int}/{iscongbo:int}")]
    public async Task<IActionResult> ChangeStatusCongBoDienTichCayHangNamKhac(int id, int iscongbo)
    {
        var result = await context.CongBoDienTichCayHangNamKhacs
            .FindAsync(id);
        if (result == null)
            return NotFound(ApiResponse<bool>.Fail(["Không tìm thấy dữ liệu."]));
        if (iscongbo != 0 && iscongbo != 1)
            return BadRequest(ApiResponse<bool>.Fail(["Trạng thái công bố không hợp lệ."]));
        if (result.IsCongBo == iscongbo)
            return BadRequest(ApiResponse<bool>.Fail(["Không có thay đổi về trạng thái công bố"]));
        result.IsCongBo = iscongbo;
        context.CongBoDienTichCayHangNamKhacs.Update(result);
        await context.SaveChangesAsync();

        return Ok(ApiResponse<bool>.Ok(true, "Cập nhật trạng thái thành công."));
    }

    //Sản lượng sản phẩm chăn nuôi
    [HttpPost("sanluongsanphamchannuoi/get-data-lists")]
    public async Task<IActionResult> GetPagedDataCongBoSanLuongSanPhamChanNuoi(
        CongBoSanLuongSanPhamChanNuoiQueryDto data)
    {
        var query = context.CongBoSanLuongSanPhamChanNuois.AsQueryable();

        if (data.CreatedFrom.HasValue)
        {
            var utcCreatedFrom = data.CreatedFrom.Value.ToUtc();
            query = query.Where(t => t.CreatedAt >= utcCreatedFrom);
        }

        if (data.CreatedTo.HasValue)
        {
            var utcCreatedTo = data.CreatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.CreatedAt < utcCreatedTo);
        }

        if (data.UpdatedFrom.HasValue)
        {
            var utcUpdatedFrom = data.UpdatedFrom.Value.ToUtc();
            query = query.Where(t => t.UpdatedAt >= utcUpdatedFrom);
        }

        if (data.UpdatedTo.HasValue)
        {
            var utcUpdatedTo = data.UpdatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.UpdatedAt < utcUpdatedTo);
        }


        if (!string.IsNullOrEmpty(data.CreatedBy))
        {
            var createdBy = data.CreatedBy.ToLower();
            query = query.Where(t => (t.CreatedBy ?? "").ToLower().Contains(createdBy));
        }

        if (data.Nams != null && data.Nams.Count > 0) query = query.Where(t => data.Nams.Contains(t.Nam));
        if (data.Quys != null && data.Quys.Count > 0) query = query.Where(t => data.Quys.Contains(t.Quy));
        if (data.MaTinhThanhs != null && data.MaTinhThanhs.Count > 0)
            query = query.Where(t => data.MaTinhThanhs.Contains(t.MaTinhThanh));
        if (data.TenSanPhams != null && data.TenSanPhams.Count > 0)
            query = query.Where(t => data.TenSanPhams.Contains(t.TenSanPham));
        if (data.IsCongBo.HasValue) query = query.Where(t => t.IsCongBo == data.IsCongBo.Value);

        if (!string.IsNullOrEmpty(data.Keyword))
        {
            var keyword = data.Keyword.RemoveSpecialCharactersUnicode().ToLower();
            query = query.Where(t =>
                t.Nam.ToString().ToLower().Contains(keyword) ||
                t.Quy.ToString().ToLower().Contains(keyword));
        }

        var TongSanLuongThitHoi = query.Sum(t => t.SanLuongThitHoi);
        var TongSanLuongChanNuoiKhac = query.Sum(t => t.SanLuongChanNuoiKhac);

        var result = await query.ToPagedResultAsync(
            data.PageNumber,
            data.PageSize,
            data.OrderBy,
            data.Ascending
        );

        var response = new CongBoSanLuongSanPhamChanNuoiPagedResponse
        {
            Data = result,
            TongSanLuongThitHoi = TongSanLuongThitHoi,
            TongSanLuongChanNuoiKhac = TongSanLuongChanNuoiKhac
        };

        return Ok(ApiResponse<CongBoSanLuongSanPhamChanNuoiPagedResponse>.Ok(response));
    }

    [HttpPost("sanluongsanphamchannuoi/delete/{id:int}")]
    public async Task<IActionResult> DeleteCongBoSanLuongSanPhamChanNuoi(int id)
    {
        var existingData = await context.CongBoSanLuongSanPhamChanNuois.FindAsync(id);
        if (existingData == null)
            return BadRequest(ApiResponse<bool>.Fail(new List<string> { "Không tìm thấy dữ liệu cần xóa." }));
        if (existingData.IsCongBo == 1)
            return BadRequest(ApiResponse<bool>.Fail(new List<string> { "Không thể xóa dữ liệu đã được công bố." }));
        context.CongBoSanLuongSanPhamChanNuois.Remove(existingData);
        await context.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Ok(true, "Xóa dữ liệu thành công."));
    }

    [HttpPost("sanluongsanphamchannuoi/upsert")]
    public async Task<IActionResult> UpsertCongBoSanLuongSanPhamChanNuoi([FromBody] CongBoSanLuongSanPhamChanNuoi data)
    {
        if (data.Id > 0)
        {
            if (data.IsCongBo == 1)
                return BadRequest(
                    ApiResponse<CongBoSanLuongSanPhamChanNuoi>.Fail(["Không thể cập dữ liệu đã được công bố."]));
            var existingData = await context.CongBoSanLuongSanPhamChanNuois.FindAsync(data.Id);
            if (existingData == null)
                return BadRequest(
                    ApiResponse<CongBoSanLuongSanPhamChanNuoi>.Fail(["Không tìm thấy dữ liệu cần cập nhật."]));
            existingData.Assign(data);
            await context.SaveChangesAsync();
            return Ok(ApiResponse<CongBoSanLuongSanPhamChanNuoi>.Ok(existingData));
        }

        context.CongBoSanLuongSanPhamChanNuois.Add(data);
        await context.SaveChangesAsync();
        return Ok(ApiResponse<CongBoSanLuongSanPhamChanNuoi>.Ok(data));
    }

    [HttpGet("sanluongsanphamchannuoi/{id:int}")]
    public async Task<IActionResult> GetByIdCongBoSanLuongSanPhamChanNuoi(int id)
    {
        var result = await context.CongBoSanLuongSanPhamChanNuois
            .FindAsync(id);
        if (result == null)
            return NotFound(ApiResponse<CongBoSanLuongSanPhamChanNuoi>.Fail(["Không tìm thấy dữ liệu."]));

        return Ok(ApiResponse<CongBoSanLuongSanPhamChanNuoi>.Ok(result));
    }

    [HttpGet("sanluongsanphamchannuoi/changestatus/{id:int}/{iscongbo:int}")]
    public async Task<IActionResult> ChangeStatusCongBoSanLuongSanPhamChanNuoi(int id, int iscongbo)
    {
        var result = await context.CongBoSanLuongSanPhamChanNuois
            .FindAsync(id);
        if (result == null)
            return NotFound(ApiResponse<bool>.Fail(["Không tìm thấy dữ liệu."]));
        if (iscongbo != 0 && iscongbo != 1)
            return BadRequest(ApiResponse<bool>.Fail(["Trạng thái công bố không hợp lệ."]));
        if (result.IsCongBo == iscongbo)
            return BadRequest(ApiResponse<bool>.Fail(["Không có thay đổi về trạng thái công bố"]));
        result.IsCongBo = iscongbo;
        context.CongBoSanLuongSanPhamChanNuois.Update(result);
        await context.SaveChangesAsync();

        return Ok(ApiResponse<bool>.Ok(true, "Cập nhật trạng thái thành công."));
    }


    //Xuất khẩu nông sản - Gạo
    [HttpPost("xuatkhaunongsan/gao/get-data-lists")]
    public async Task<IActionResult> GetPagedDataCongBoXuatKhauGao(CongBoXuatKhauNongSanQueryDto data)
    {
        var query = context.CongBoXuatKhauNongSans.AsQueryable();

        query = query.Where(t => t.LoaiNongSan == "Gạo");

        if (data.CreatedFrom.HasValue)
        {
            var utcCreatedFrom = data.CreatedFrom.Value.ToUtc();
            query = query.Where(t => t.CreatedAt >= utcCreatedFrom);
        }

        if (data.CreatedTo.HasValue)
        {
            var utcCreatedTo = data.CreatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.CreatedAt < utcCreatedTo);
        }

        if (data.UpdatedFrom.HasValue)
        {
            var utcUpdatedFrom = data.UpdatedFrom.Value.ToUtc();
            query = query.Where(t => t.UpdatedAt >= utcUpdatedFrom);
        }

        if (data.UpdatedTo.HasValue)
        {
            var utcUpdatedTo = data.UpdatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.UpdatedAt < utcUpdatedTo);
        }

        if (!string.IsNullOrEmpty(data.CreatedBy))
        {
            var createdBy = data.CreatedBy.ToLower();
            query = query.Where(t => (t.CreatedBy ?? "").ToLower().Contains(createdBy));
        }

        if (data.Nams != null && data.Nams.Count > 0) query = query.Where(t => data.Nams.Contains(t.Nam));
        if (data.Thangs != null && data.Thangs.Count > 0) query = query.Where(t => data.Thangs.Contains(t.Thang));
        if (data.MaQuocGiaVungLTs != null && data.MaQuocGiaVungLTs.Count > 0)
            query = query.Where(t => data.MaQuocGiaVungLTs.Contains(t.MaQuocGiaVungLT));
        if (data.IsCongBo.HasValue) query = query.Where(t => t.IsCongBo == data.IsCongBo.Value);

        if (!string.IsNullOrEmpty(data.Keyword))
        {
            var keyword = data.Keyword.RemoveSpecialCharactersUnicode().ToLower();
            query = query.Where(t =>
                t.Nam.ToString().ToLower().Contains(keyword) ||
                t.Thang.ToString().ToLower().Contains(keyword));
        }

        var TongKhoiLuong = query.Sum(t => t.KhoiLuong);
        var TongGiaTri = query.Sum(t => t.GiaTri);

        var result = await query.ToPagedResultAsync(
            data.PageNumber,
            data.PageSize,
            data.OrderBy,
            data.Ascending
        );

        var response = new CongBoXuatKhauNongSanPagedResponse
        {
            Data = result,
            TongKhoiLuong = TongKhoiLuong,
            TongGiaTri = TongGiaTri
        };

        return Ok(ApiResponse<CongBoXuatKhauNongSanPagedResponse>.Ok(response));
    }

    //Xuất khẩu nông sản - Cà phê
    [HttpPost("xuatkhaunongsan/caphe/get-data-lists")]
    public async Task<IActionResult> GetPagedDataCongBoXuatKhauCaPhe(CongBoXuatKhauNongSanQueryDto data)
    {
        var query = context.CongBoXuatKhauNongSans.AsQueryable();

        query = query.Where(t => t.LoaiNongSan == "Cà phê");

        if (data.CreatedFrom.HasValue)
        {
            var utcCreatedFrom = data.CreatedFrom.Value.ToUtc();
            query = query.Where(t => t.CreatedAt >= utcCreatedFrom);
        }

        if (data.CreatedTo.HasValue)
        {
            var utcCreatedTo = data.CreatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.CreatedAt < utcCreatedTo);
        }

        if (data.UpdatedFrom.HasValue)
        {
            var utcUpdatedFrom = data.UpdatedFrom.Value.ToUtc();
            query = query.Where(t => t.UpdatedAt >= utcUpdatedFrom);
        }

        if (data.UpdatedTo.HasValue)
        {
            var utcUpdatedTo = data.UpdatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.UpdatedAt < utcUpdatedTo);
        }

        if (!string.IsNullOrEmpty(data.CreatedBy))
        {
            var createdBy = data.CreatedBy.ToLower();
            query = query.Where(t => (t.CreatedBy ?? "").ToLower().Contains(createdBy));
        }

        if (data.Nams != null && data.Nams.Count > 0) query = query.Where(t => data.Nams.Contains(t.Nam));
        if (data.Thangs != null && data.Thangs.Count > 0) query = query.Where(t => data.Thangs.Contains(t.Thang));
        if (data.MaQuocGiaVungLTs != null && data.MaQuocGiaVungLTs.Count > 0)
            query = query.Where(t => data.MaQuocGiaVungLTs.Contains(t.MaQuocGiaVungLT));
        if (data.IsCongBo.HasValue) query = query.Where(t => t.IsCongBo == data.IsCongBo.Value);

        if (!string.IsNullOrEmpty(data.Keyword))
        {
            var keyword = data.Keyword.RemoveSpecialCharactersUnicode().ToLower();
            query = query.Where(t =>
                t.Nam.ToString().ToLower().Contains(keyword) ||
                t.Thang.ToString().ToLower().Contains(keyword));
        }

        var TongKhoiLuong = query.Sum(t => t.KhoiLuong);
        var TongGiaTri = query.Sum(t => t.GiaTri);

        var result = await query.ToPagedResultAsync(
            data.PageNumber,
            data.PageSize,
            data.OrderBy,
            data.Ascending
        );

        var response = new CongBoXuatKhauNongSanPagedResponse
        {
            Data = result,
            TongKhoiLuong = TongKhoiLuong,
            TongGiaTri = TongGiaTri
        };

        return Ok(ApiResponse<CongBoXuatKhauNongSanPagedResponse>.Ok(response));
    }

    //Xuất khẩu nông sản - Cao su
    [HttpPost("xuatkhaunongsan/caosu/get-data-lists")]
    public async Task<IActionResult> GetPagedDataCongBoXuatKhauCaoSu(CongBoXuatKhauNongSanQueryDto data)
    {
        var query = context.CongBoXuatKhauNongSans.AsQueryable();

        query = query.Where(t => t.LoaiNongSan == "Cao su");

        if (data.CreatedFrom.HasValue)
        {
            var utcCreatedFrom = data.CreatedFrom.Value.ToUtc();
            query = query.Where(t => t.CreatedAt >= utcCreatedFrom);
        }

        if (data.CreatedTo.HasValue)
        {
            var utcCreatedTo = data.CreatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.CreatedAt < utcCreatedTo);
        }

        if (data.UpdatedFrom.HasValue)
        {
            var utcUpdatedFrom = data.UpdatedFrom.Value.ToUtc();
            query = query.Where(t => t.UpdatedAt >= utcUpdatedFrom);
        }

        if (data.UpdatedTo.HasValue)
        {
            var utcUpdatedTo = data.UpdatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.UpdatedAt < utcUpdatedTo);
        }

        if (!string.IsNullOrEmpty(data.CreatedBy))
        {
            var createdBy = data.CreatedBy.ToLower();
            query = query.Where(t => (t.CreatedBy ?? "").ToLower().Contains(createdBy));
        }

        if (data.Nams != null && data.Nams.Count > 0) query = query.Where(t => data.Nams.Contains(t.Nam));
        if (data.Thangs != null && data.Thangs.Count > 0) query = query.Where(t => data.Thangs.Contains(t.Thang));
        if (data.MaQuocGiaVungLTs != null && data.MaQuocGiaVungLTs.Count > 0)
            query = query.Where(t => data.MaQuocGiaVungLTs.Contains(t.MaQuocGiaVungLT));
        if (data.IsCongBo.HasValue) query = query.Where(t => t.IsCongBo == data.IsCongBo.Value);

        if (!string.IsNullOrEmpty(data.Keyword))
        {
            var keyword = data.Keyword.RemoveSpecialCharactersUnicode().ToLower();
            query = query.Where(t =>
                t.Nam.ToString().ToLower().Contains(keyword) ||
                t.Thang.ToString().ToLower().Contains(keyword));
        }

        var TongKhoiLuong = query.Sum(t => t.KhoiLuong);
        var TongGiaTri = query.Sum(t => t.GiaTri);

        var result = await query.ToPagedResultAsync(
            data.PageNumber,
            data.PageSize,
            data.OrderBy,
            data.Ascending
        );

        var response = new CongBoXuatKhauNongSanPagedResponse
        {
            Data = result,
            TongKhoiLuong = TongKhoiLuong,
            TongGiaTri = TongGiaTri
        };

        return Ok(ApiResponse<CongBoXuatKhauNongSanPagedResponse>.Ok(response));
    }

    //Xuất khẩu nông sản - Chè
    [HttpPost("xuatkhaunongsan/che/get-data-lists")]
    public async Task<IActionResult> GetPagedDataCongBoXuatKhauChe(CongBoXuatKhauNongSanQueryDto data)
    {
        var query = context.CongBoXuatKhauNongSans.AsQueryable();

        query = query.Where(t => t.LoaiNongSan == "Chè");

        if (data.CreatedFrom.HasValue)
        {
            var utcCreatedFrom = data.CreatedFrom.Value.ToUtc();
            query = query.Where(t => t.CreatedAt >= utcCreatedFrom);
        }

        if (data.CreatedTo.HasValue)
        {
            var utcCreatedTo = data.CreatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.CreatedAt < utcCreatedTo);
        }

        if (data.UpdatedFrom.HasValue)
        {
            var utcUpdatedFrom = data.UpdatedFrom.Value.ToUtc();
            query = query.Where(t => t.UpdatedAt >= utcUpdatedFrom);
        }

        if (data.UpdatedTo.HasValue)
        {
            var utcUpdatedTo = data.UpdatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.UpdatedAt < utcUpdatedTo);
        }

        if (!string.IsNullOrEmpty(data.CreatedBy))
        {
            var createdBy = data.CreatedBy.ToLower();
            query = query.Where(t => (t.CreatedBy ?? "").ToLower().Contains(createdBy));
        }

        if (data.Nams != null && data.Nams.Count > 0) query = query.Where(t => data.Nams.Contains(t.Nam));
        if (data.Thangs != null && data.Thangs.Count > 0) query = query.Where(t => data.Thangs.Contains(t.Thang));
        if (data.MaQuocGiaVungLTs != null && data.MaQuocGiaVungLTs.Count > 0)
            query = query.Where(t => data.MaQuocGiaVungLTs.Contains(t.MaQuocGiaVungLT));
        if (data.IsCongBo.HasValue) query = query.Where(t => t.IsCongBo == data.IsCongBo.Value);

        if (!string.IsNullOrEmpty(data.Keyword))
        {
            var keyword = data.Keyword.RemoveSpecialCharactersUnicode().ToLower();
            query = query.Where(t =>
                t.Nam.ToString().ToLower().Contains(keyword) ||
                t.Thang.ToString().ToLower().Contains(keyword));
        }

        var TongKhoiLuong = query.Sum(t => t.KhoiLuong);
        var TongGiaTri = query.Sum(t => t.GiaTri);

        var result = await query.ToPagedResultAsync(
            data.PageNumber,
            data.PageSize,
            data.OrderBy,
            data.Ascending
        );

        var response = new CongBoXuatKhauNongSanPagedResponse
        {
            Data = result,
            TongKhoiLuong = TongKhoiLuong,
            TongGiaTri = TongGiaTri
        };

        return Ok(ApiResponse<CongBoXuatKhauNongSanPagedResponse>.Ok(response));
    }

    //Xuất khẩu nông sản - Hạt điều
    [HttpPost("xuatkhaunongsan/hatdieu/get-data-lists")]
    public async Task<IActionResult> GetPagedDataCongBoXuatKhauHatDieu(CongBoXuatKhauNongSanQueryDto data)
    {
        var query = context.CongBoXuatKhauNongSans.AsQueryable();

        query = query.Where(t => t.LoaiNongSan == "Hạt điều");

        if (data.CreatedFrom.HasValue)
        {
            var utcCreatedFrom = data.CreatedFrom.Value.ToUtc();
            query = query.Where(t => t.CreatedAt >= utcCreatedFrom);
        }

        if (data.CreatedTo.HasValue)
        {
            var utcCreatedTo = data.CreatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.CreatedAt < utcCreatedTo);
        }

        if (data.UpdatedFrom.HasValue)
        {
            var utcUpdatedFrom = data.UpdatedFrom.Value.ToUtc();
            query = query.Where(t => t.UpdatedAt >= utcUpdatedFrom);
        }

        if (data.UpdatedTo.HasValue)
        {
            var utcUpdatedTo = data.UpdatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.UpdatedAt < utcUpdatedTo);
        }

        if (!string.IsNullOrEmpty(data.CreatedBy))
        {
            var createdBy = data.CreatedBy.ToLower();
            query = query.Where(t => (t.CreatedBy ?? "").ToLower().Contains(createdBy));
        }

        if (data.Nams != null && data.Nams.Count > 0) query = query.Where(t => data.Nams.Contains(t.Nam));
        if (data.Thangs != null && data.Thangs.Count > 0) query = query.Where(t => data.Thangs.Contains(t.Thang));
        if (data.MaQuocGiaVungLTs != null && data.MaQuocGiaVungLTs.Count > 0)
            query = query.Where(t => data.MaQuocGiaVungLTs.Contains(t.MaQuocGiaVungLT));
        if (data.IsCongBo.HasValue) query = query.Where(t => t.IsCongBo == data.IsCongBo.Value);

        if (!string.IsNullOrEmpty(data.Keyword))
        {
            var keyword = data.Keyword.RemoveSpecialCharactersUnicode().ToLower();
            query = query.Where(t =>
                t.Nam.ToString().ToLower().Contains(keyword) ||
                t.Thang.ToString().ToLower().Contains(keyword));
        }

        var TongKhoiLuong = query.Sum(t => t.KhoiLuong);
        var TongGiaTri = query.Sum(t => t.GiaTri);

        var result = await query.ToPagedResultAsync(
            data.PageNumber,
            data.PageSize,
            data.OrderBy,
            data.Ascending
        );

        var response = new CongBoXuatKhauNongSanPagedResponse
        {
            Data = result,
            TongKhoiLuong = TongKhoiLuong,
            TongGiaTri = TongGiaTri
        };

        return Ok(ApiResponse<CongBoXuatKhauNongSanPagedResponse>.Ok(response));
    }

    //Xuất khẩu nông sản - Hạt tiêu
    [HttpPost("xuatkhaunongsan/hattieu/get-data-lists")]
    public async Task<IActionResult> GetPagedDataCongBoXuatKhauHatTieu(CongBoXuatKhauNongSanQueryDto data)
    {
        var query = context.CongBoXuatKhauNongSans.AsQueryable();

        query = query.Where(t => t.LoaiNongSan == "Hạt tiêu");

        if (data.CreatedFrom.HasValue)
        {
            var utcCreatedFrom = data.CreatedFrom.Value.ToUtc();
            query = query.Where(t => t.CreatedAt >= utcCreatedFrom);
        }

        if (data.CreatedTo.HasValue)
        {
            var utcCreatedTo = data.CreatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.CreatedAt < utcCreatedTo);
        }

        if (data.UpdatedFrom.HasValue)
        {
            var utcUpdatedFrom = data.UpdatedFrom.Value.ToUtc();
            query = query.Where(t => t.UpdatedAt >= utcUpdatedFrom);
        }

        if (data.UpdatedTo.HasValue)
        {
            var utcUpdatedTo = data.UpdatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.UpdatedAt < utcUpdatedTo);
        }

        if (!string.IsNullOrEmpty(data.CreatedBy))
        {
            var createdBy = data.CreatedBy.ToLower();
            query = query.Where(t => (t.CreatedBy ?? "").ToLower().Contains(createdBy));
        }

        if (data.Nams != null && data.Nams.Count > 0) query = query.Where(t => data.Nams.Contains(t.Nam));
        if (data.Thangs != null && data.Thangs.Count > 0) query = query.Where(t => data.Thangs.Contains(t.Thang));
        if (data.MaQuocGiaVungLTs != null && data.MaQuocGiaVungLTs.Count > 0)
            query = query.Where(t => data.MaQuocGiaVungLTs.Contains(t.MaQuocGiaVungLT));
        if (data.IsCongBo.HasValue) query = query.Where(t => t.IsCongBo == data.IsCongBo.Value);

        if (!string.IsNullOrEmpty(data.Keyword))
        {
            var keyword = data.Keyword.RemoveSpecialCharactersUnicode().ToLower();
            query = query.Where(t =>
                t.Nam.ToString().ToLower().Contains(keyword) ||
                t.Thang.ToString().ToLower().Contains(keyword));
        }

        var TongKhoiLuong = query.Sum(t => t.KhoiLuong);
        var TongGiaTri = query.Sum(t => t.GiaTri);

        var result = await query.ToPagedResultAsync(
            data.PageNumber,
            data.PageSize,
            data.OrderBy,
            data.Ascending
        );

        var response = new CongBoXuatKhauNongSanPagedResponse
        {
            Data = result,
            TongKhoiLuong = TongKhoiLuong,
            TongGiaTri = TongGiaTri
        };

        return Ok(ApiResponse<CongBoXuatKhauNongSanPagedResponse>.Ok(response));
    }

    //Xuất khẩu nông sản - Hàng rau quả
    [HttpPost("xuatkhaunongsan/hangrauqua/get-data-lists")]
    public async Task<IActionResult> GetPagedDataCongBoXuatKhauHangRauQua(CongBoXuatKhauNongSanQueryDto data)
    {
        var query = context.CongBoXuatKhauNongSans.AsQueryable();

        query = query.Where(t => t.LoaiNongSan == "Hàng rau quả");

        if (data.CreatedFrom.HasValue)
        {
            var utcCreatedFrom = data.CreatedFrom.Value.ToUtc();
            query = query.Where(t => t.CreatedAt >= utcCreatedFrom);
        }

        if (data.CreatedTo.HasValue)
        {
            var utcCreatedTo = data.CreatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.CreatedAt < utcCreatedTo);
        }

        if (data.UpdatedFrom.HasValue)
        {
            var utcUpdatedFrom = data.UpdatedFrom.Value.ToUtc();
            query = query.Where(t => t.UpdatedAt >= utcUpdatedFrom);
        }

        if (data.UpdatedTo.HasValue)
        {
            var utcUpdatedTo = data.UpdatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.UpdatedAt < utcUpdatedTo);
        }

        if (!string.IsNullOrEmpty(data.CreatedBy))
        {
            var createdBy = data.CreatedBy.ToLower();
            query = query.Where(t => (t.CreatedBy ?? "").ToLower().Contains(createdBy));
        }

        if (data.Nams != null && data.Nams.Count > 0) query = query.Where(t => data.Nams.Contains(t.Nam));
        if (data.Thangs != null && data.Thangs.Count > 0) query = query.Where(t => data.Thangs.Contains(t.Thang));
        if (data.MaQuocGiaVungLTs != null && data.MaQuocGiaVungLTs.Count > 0)
            query = query.Where(t => data.MaQuocGiaVungLTs.Contains(t.MaQuocGiaVungLT));
        if (data.IsCongBo.HasValue) query = query.Where(t => t.IsCongBo == data.IsCongBo.Value);

        if (!string.IsNullOrEmpty(data.Keyword))
        {
            var keyword = data.Keyword.RemoveSpecialCharactersUnicode().ToLower();
            query = query.Where(t =>
                t.Nam.ToString().ToLower().Contains(keyword) ||
                t.Thang.ToString().ToLower().Contains(keyword));
        }

        var TongKhoiLuong = query.Sum(t => t.KhoiLuong);
        var TongGiaTri = query.Sum(t => t.GiaTri);

        var result = await query.ToPagedResultAsync(
            data.PageNumber,
            data.PageSize,
            data.OrderBy,
            data.Ascending
        );

        var response = new CongBoXuatKhauNongSanPagedResponse
        {
            Data = result,
            TongKhoiLuong = TongKhoiLuong,
            TongGiaTri = TongGiaTri
        };

        return Ok(ApiResponse<CongBoXuatKhauNongSanPagedResponse>.Ok(response));
    }

    [HttpPost("xuatkhaunongsan/delete/{id:int}")]
    public async Task<IActionResult> DeleteCongBoXuatKhauNongSan(int id)
    {
        var existingData = await context.CongBoXuatKhauNongSans.FindAsync(id);
        if (existingData == null)
            return BadRequest(ApiResponse<bool>.Fail(new List<string> { "Không tìm thấy dữ liệu cần xóa." }));
        if (existingData.IsCongBo == 1)
            return BadRequest(ApiResponse<bool>.Fail(new List<string> { "Không thể xóa dữ liệu đã được công bố." }));
        context.CongBoXuatKhauNongSans.Remove(existingData);
        await context.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Ok(true, "Xóa dữ liệu thành công."));
    }

    [HttpPost("xuatkhaunongsan/upsert")]
    public async Task<IActionResult> UpsertCongBoXuatKhauNongSan([FromBody] CongBoXuatKhauNongSan data)
    {
        if (data.Id > 0)
        {
            if (data.IsCongBo == 1)
                return BadRequest(ApiResponse<CongBoXuatKhauNongSan>.Fail(["Không thể cập dữ liệu đã được công bố."]));
            var existingData = await context.CongBoXuatKhauNongSans.FindAsync(data.Id);
            if (existingData == null)
                return BadRequest(ApiResponse<CongBoXuatKhauNongSan>.Fail(["Không tìm thấy dữ liệu cần cập nhật."]));
            existingData.Assign(data);
            await context.SaveChangesAsync();
            return Ok(ApiResponse<CongBoXuatKhauNongSan>.Ok(existingData));
        }

        context.CongBoXuatKhauNongSans.Add(data);
        await context.SaveChangesAsync();
        return Ok(ApiResponse<CongBoXuatKhauNongSan>.Ok(data));
    }

    [HttpGet("xuatkhaunongsan/{id:int}")]
    public async Task<IActionResult> GetByIdCongBoXuatKhauNongSan(int id)
    {
        var result = await context.CongBoXuatKhauNongSans
            .FindAsync(id);
        if (result == null)
            return NotFound(ApiResponse<CongBoXuatKhauNongSan>.Fail(["Không tìm thấy dữ liệu."]));

        return Ok(ApiResponse<CongBoXuatKhauNongSan>.Ok(result));
    }

    [HttpGet("xuatkhaunongsan/changestatus/{id:int}/{iscongbo:int}")]
    public async Task<IActionResult> ChangeStatusCongBoXuatKhauNongSan(int id, int iscongbo)
    {
        var result = await context.CongBoXuatKhauNongSans
            .FindAsync(id);
        if (result == null)
            return NotFound(ApiResponse<bool>.Fail(["Không tìm thấy dữ liệu."]));
        if (iscongbo != 0 && iscongbo != 1)
            return BadRequest(ApiResponse<bool>.Fail(["Trạng thái công bố không hợp lệ."]));
        if (result.IsCongBo == iscongbo)
            return BadRequest(ApiResponse<bool>.Fail(["Không có thay đổi về trạng thái công bố"]));
        result.IsCongBo = iscongbo;
        context.CongBoXuatKhauNongSans.Update(result);
        await context.SaveChangesAsync();

        return Ok(ApiResponse<bool>.Ok(true, "Cập nhật trạng thái thành công."));
    }

    //Năng suất lúa
    [HttpPost("nangsuatlua/get-data-lists")]
    public async Task<IActionResult> GetPagedDataCongBoNangSuatLua(CongBoDienTichLuaQueryDto data)
    {
        var query = context.CongBoNangSuatLuas.AsQueryable();

        if (data.CreatedFrom.HasValue)
        {
            var utcCreatedFrom = data.CreatedFrom.Value.ToUtc();
            query = query.Where(t => t.CreatedAt >= utcCreatedFrom);
        }

        if (data.CreatedTo.HasValue)
        {
            var utcCreatedTo = data.CreatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.CreatedAt < utcCreatedTo);
        }

        if (data.UpdatedFrom.HasValue)
        {
            var utcUpdatedFrom = data.UpdatedFrom.Value.ToUtc();
            query = query.Where(t => t.UpdatedAt >= utcUpdatedFrom);
        }

        if (data.UpdatedTo.HasValue)
        {
            var utcUpdatedTo = data.UpdatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.UpdatedAt < utcUpdatedTo);
        }


        if (!string.IsNullOrEmpty(data.CreatedBy))
        {
            var createdBy = data.CreatedBy.ToLower();
            query = query.Where(t => (t.CreatedBy ?? "").ToLower().Contains(createdBy));
        }

        if (data.Nams != null && data.Nams.Count > 0) query = query.Where(t => data.Nams.Contains(t.Nam));
        if (data.Thangs != null && data.Thangs.Count > 0) query = query.Where(t => data.Thangs.Contains(t.Thang));
        if (data.MaTinhThanhs != null && data.MaTinhThanhs.Count > 0)
            query = query.Where(t => data.MaTinhThanhs.Contains(t.MaTinhThanh));
        if (data.MuaVus != null && data.MuaVus.Count > 0) query = query.Where(t => data.MuaVus.Contains(t.MuaVu));
        if (data.IsCongBo.HasValue) query = query.Where(t => t.IsCongBo == data.IsCongBo.Value);

        if (!string.IsNullOrEmpty(data.Keyword))
        {
            var keyword = data.Keyword.RemoveSpecialCharactersUnicode().ToLower();
            query = query.Where(t =>
                t.Nam.ToString().ToLower().Contains(keyword) ||
                t.Thang.ToString().ToLower().Contains(keyword));
        }

        var TongNangSuatLua = query.Sum(t => t.NangSuatLua);
        var TongNangSuatLuaUocTinh = query.Sum(t => t.NangSuatLuaUocTinh);

        // Áp dụng sắp xếp
        if (!string.IsNullOrEmpty(data.OrderBy))
        {
            var property = typeof(CongBoNangSuatLua).GetProperty(data.OrderBy,
                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (property != null)
                query = data.Ascending
                    ? query.OrderBy(x => EF.Property<object>(x, property.Name))
                    : query.OrderByDescending(x => EF.Property<object>(x, property.Name));
        }

        PagedResult<CongBoNangSuatLua> result;
        if (data.IsAllData == 1)
        {
            var allRecords = await query.ToListAsync();
            result = new PagedResult<CongBoNangSuatLua>(allRecords, allRecords.Count, 1, allRecords.Count);
        }
        else
        {
            result = await query.ToPagedResultAsync(data.PageNumber, data.PageSize);
        }

        var response = new CongBoNangSuatLuaPagedResponse
        {
            Data = result,
            TongNangSuatLua = TongNangSuatLua,
            TongNangSuatLuaUocTinh = TongNangSuatLuaUocTinh
        };

        return Ok(ApiResponse<CongBoNangSuatLuaPagedResponse>.Ok(response));
    }

    [HttpPost("nangsuatlua/delete/{id:int}")]
    public async Task<IActionResult> DeleteCongBoNangSuatLua(int id)
    {
        var existingData = await context.CongBoNangSuatLuas.FindAsync(id);
        if (existingData == null)
            return BadRequest(ApiResponse<bool>.Fail(new List<string> { "Không tìm thấy dữ liệu cần xóa." }));
        if (existingData.IsCongBo == 1)
            return BadRequest(ApiResponse<bool>.Fail(new List<string> { "Không thể xóa dữ liệu đã được công bố." }));
        context.CongBoNangSuatLuas.Remove(existingData);
        await context.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Ok(true, "Xóa dữ liệu thành công."));
    }

    [HttpPost("nangsuatlua/upsert")]
    public async Task<IActionResult> UpsertCongBoNangSuatLua([FromBody] CongBoNangSuatLua data)
    {
        if (data.Id > 0)
        {
            if (data.IsCongBo == 1)
                return BadRequest(ApiResponse<CongBoNangSuatLua>.Fail(["Không thể cập dữ liệu đã được công bố."]));
            var existingData = await context.CongBoNangSuatLuas.FindAsync(data.Id);
            if (existingData == null)
                return BadRequest(ApiResponse<CongBoNangSuatLua>.Fail(["Không tìm thấy dữ liệu cần cập nhật."]));
            existingData.Assign(data);
            await context.SaveChangesAsync();
            return Ok(ApiResponse<CongBoNangSuatLua>.Ok(existingData));
        }

        context.CongBoNangSuatLuas.Add(data);
        await context.SaveChangesAsync();
        return Ok(ApiResponse<CongBoNangSuatLua>.Ok(data));
    }

    [HttpGet("nangsuatlua/{id:int}")]
    public async Task<IActionResult> GetByIdCongBoNangSuatLua(int id)
    {
        var result = await context.CongBoNangSuatLuas
            .FindAsync(id);
        if (result == null)
            return NotFound(ApiResponse<CongBoNangSuatLua>.Fail(["Không tìm thấy dữ liệu."]));

        return Ok(ApiResponse<CongBoNangSuatLua>.Ok(result));
    }

    [HttpGet("nangsuatlua/changestatus/{id:int}/{iscongbo:int}")]
    public async Task<IActionResult> ChangeStatusCongBoNangSuatLua(int id, int iscongbo)
    {
        var result = await context.CongBoNangSuatLuas
            .FindAsync(id);
        if (result == null)
            return NotFound(ApiResponse<bool>.Fail(["Không tìm thấy dữ liệu."]));
        if (iscongbo != 0 && iscongbo != 1)
            return BadRequest(ApiResponse<bool>.Fail(["Trạng thái công bố không hợp lệ."]));
        if (result.IsCongBo == iscongbo)
            return BadRequest(ApiResponse<bool>.Fail(["Không có thay đổi về trạng thái công bố"]));
        result.IsCongBo = iscongbo;
        context.CongBoNangSuatLuas.Update(result);
        await context.SaveChangesAsync();

        return Ok(ApiResponse<bool>.Ok(true, "Cập nhật trạng thái thành công."));
    }

    //Sản lượng lúa
    [HttpPost("sanluonglua/get-data-lists")]
    public async Task<IActionResult> GetPagedDataCongBoSanLuongLua(CongBoDienTichLuaQueryDto data)
    {
        var query = context.CongBoSanLuongLuas.AsQueryable();

        if (data.CreatedFrom.HasValue)
        {
            var utcCreatedFrom = data.CreatedFrom.Value.ToUtc();
            query = query.Where(t => t.CreatedAt >= utcCreatedFrom);
        }

        if (data.CreatedTo.HasValue)
        {
            var utcCreatedTo = data.CreatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.CreatedAt < utcCreatedTo);
        }

        if (data.UpdatedFrom.HasValue)
        {
            var utcUpdatedFrom = data.UpdatedFrom.Value.ToUtc();
            query = query.Where(t => t.UpdatedAt >= utcUpdatedFrom);
        }

        if (data.UpdatedTo.HasValue)
        {
            var utcUpdatedTo = data.UpdatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.UpdatedAt < utcUpdatedTo);
        }


        if (!string.IsNullOrEmpty(data.CreatedBy))
        {
            var createdBy = data.CreatedBy.ToLower();
            query = query.Where(t => (t.CreatedBy ?? "").ToLower().Contains(createdBy));
        }

        if (data.Nams != null && data.Nams.Count > 0) query = query.Where(t => data.Nams.Contains(t.Nam));
        if (data.Thangs != null && data.Thangs.Count > 0) query = query.Where(t => data.Thangs.Contains(t.Thang));
        if (data.MaTinhThanhs != null && data.MaTinhThanhs.Count > 0)
            query = query.Where(t => data.MaTinhThanhs.Contains(t.MaTinhThanh));
        if (data.MuaVus != null && data.MuaVus.Count > 0) query = query.Where(t => data.MuaVus.Contains(t.MuaVu));
        if (data.IsCongBo.HasValue) query = query.Where(t => t.IsCongBo == data.IsCongBo.Value);

        if (!string.IsNullOrEmpty(data.Keyword))
        {
            var keyword = data.Keyword.RemoveSpecialCharactersUnicode().ToLower();
            query = query.Where(t =>
                t.Nam.ToString().ToLower().Contains(keyword) ||
                t.Thang.ToString().ToLower().Contains(keyword));
        }

        var TongSanLuongLua = query.Sum(t => t.SanLuongLua);
        var TongSanLuongLuaUocTinh = query.Sum(t => t.SanLuongLuaUocTinh);

        // Áp dụng sắp xếp
        if (!string.IsNullOrEmpty(data.OrderBy))
        {
            var property = typeof(CongBoSanLuongLua).GetProperty(data.OrderBy,
                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (property != null)
                query = data.Ascending
                    ? query.OrderBy(x => EF.Property<object>(x, property.Name))
                    : query.OrderByDescending(x => EF.Property<object>(x, property.Name));
        }

        PagedResult<CongBoSanLuongLua> result;
        if (data.IsAllData == 1)
        {
            var allRecords = await query.ToListAsync();
            result = new PagedResult<CongBoSanLuongLua>(allRecords, allRecords.Count, 1, allRecords.Count);
        }
        else
        {
            result = await query.ToPagedResultAsync(data.PageNumber, data.PageSize);
        }

        var response = new CongBoSanLuongLuaPagedResponse
        {
            Data = result,
            TongSanLuongLua = TongSanLuongLua,
            TongSanLuongLuaUocTinh = TongSanLuongLuaUocTinh
        };

        return Ok(ApiResponse<CongBoSanLuongLuaPagedResponse>.Ok(response));
    }

    [HttpPost("sanluonglua/delete/{id:int}")]
    public async Task<IActionResult> DeleteCongBoSanLuongLua(int id)
    {
        var existingData = await context.CongBoSanLuongLuas.FindAsync(id);
        if (existingData == null)
            return BadRequest(ApiResponse<bool>.Fail(new List<string> { "Không tìm thấy dữ liệu cần xóa." }));
        if (existingData.IsCongBo == 1)
            return BadRequest(ApiResponse<bool>.Fail(new List<string> { "Không thể xóa dữ liệu đã được công bố." }));
        context.CongBoSanLuongLuas.Remove(existingData);
        await context.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Ok(true, "Xóa dữ liệu thành công."));
    }

    [HttpPost("sanluonglua/upsert")]
    public async Task<IActionResult> UpsertCongBoSanLuongLua([FromBody] CongBoSanLuongLua data)
    {
        if (data.Id > 0)
        {
            if (data.IsCongBo == 1)
                return BadRequest(ApiResponse<CongBoSanLuongLua>.Fail(["Không thể cập dữ liệu đã được công bố."]));
            var existingData = await context.CongBoSanLuongLuas.FindAsync(data.Id);
            if (existingData == null)
                return BadRequest(ApiResponse<CongBoSanLuongLua>.Fail(["Không tìm thấy dữ liệu cần cập nhật."]));
            existingData.Assign(data);
            await context.SaveChangesAsync();
            return Ok(ApiResponse<CongBoSanLuongLua>.Ok(existingData));
        }

        context.CongBoSanLuongLuas.Add(data);
        await context.SaveChangesAsync();
        return Ok(ApiResponse<CongBoSanLuongLua>.Ok(data));
    }

    [HttpGet("sanluonglua/{id:int}")]
    public async Task<IActionResult> GetByIdCongBoSanLuongLua(int id)
    {
        var result = await context.CongBoSanLuongLuas
            .FindAsync(id);
        if (result == null)
            return NotFound(ApiResponse<CongBoSanLuongLua>.Fail(["Không tìm thấy dữ liệu."]));

        return Ok(ApiResponse<CongBoSanLuongLua>.Ok(result));
    }

    [HttpGet("sanluonglua/changestatus/{id:int}/{iscongbo:int}")]
    public async Task<IActionResult> ChangeStatusCongBoSanLuongLua(int id, int iscongbo)
    {
        var result = await context.CongBoSanLuongLuas
            .FindAsync(id);
        if (result == null)
            return NotFound(ApiResponse<bool>.Fail(["Không tìm thấy dữ liệu."]));
        if (iscongbo != 0 && iscongbo != 1)
            return BadRequest(ApiResponse<bool>.Fail(["Trạng thái công bố không hợp lệ."]));
        if (result.IsCongBo == iscongbo)
            return BadRequest(ApiResponse<bool>.Fail(["Không có thay đổi về trạng thái công bố"]));
        result.IsCongBo = iscongbo;
        context.CongBoSanLuongLuas.Update(result);
        await context.SaveChangesAsync();

        return Ok(ApiResponse<bool>.Ok(true, "Cập nhật trạng thái thành công."));
    }

    //Sản lượng cây hàng năm khác
    [HttpPost("sanluongcayhangnamkhac/get-data-lists")]
    public async Task<IActionResult> GetPagedDataCongBoSanLuongCayHangNamKhac(CongBoSanLuongCayHangNamKhacQueryDto data)
    {
        var query = context.CongBoSanLuongCayHangNamKhacs.AsQueryable();

        if (data.CreatedFrom.HasValue)
        {
            var utcCreatedFrom = data.CreatedFrom.Value.ToUtc();
            query = query.Where(t => t.CreatedAt >= utcCreatedFrom);
        }

        if (data.CreatedTo.HasValue)
        {
            var utcCreatedTo = data.CreatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.CreatedAt < utcCreatedTo);
        }

        if (data.UpdatedFrom.HasValue)
        {
            var utcUpdatedFrom = data.UpdatedFrom.Value.ToUtc();
            query = query.Where(t => t.UpdatedAt >= utcUpdatedFrom);
        }

        if (data.UpdatedTo.HasValue)
        {
            var utcUpdatedTo = data.UpdatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.UpdatedAt < utcUpdatedTo);
        }


        if (!string.IsNullOrEmpty(data.CreatedBy))
        {
            var createdBy = data.CreatedBy.ToLower();
            query = query.Where(t => (t.CreatedBy ?? "").ToLower().Contains(createdBy));
        }

        if (data.Nams != null && data.Nams.Count > 0) query = query.Where(t => data.Nams.Contains(t.Nam));
        if (data.Thangs != null && data.Thangs.Count > 0) query = query.Where(t => data.Thangs.Contains(t.Thang));
        if (data.MaTinhThanhs != null && data.MaTinhThanhs.Count > 0)
            query = query.Where(t => data.MaTinhThanhs.Contains(t.MaTinhThanh));
        if (data.LoaiCays != null && data.LoaiCays.Count > 0)
            query = query.Where(t => data.LoaiCays.Contains(t.LoaiCay));
        if (data.MuaVus != null && data.MuaVus.Count > 0)
            query = query.Where(t => data.MuaVus.Contains(t.MuaVu));
        if (data.IsCongBo.HasValue) query = query.Where(t => t.IsCongBo == data.IsCongBo.Value);

        if (!string.IsNullOrEmpty(data.Keyword))
        {
            var keyword = data.Keyword.RemoveSpecialCharactersUnicode().ToLower();
            query = query.Where(t =>
                t.Nam.ToString().ToLower().Contains(keyword) ||
                t.Thang.ToString().ToLower().Contains(keyword));
        }

        var TongSanLuong = query.Sum(t => t.SanLuong);
        var TongSanLuongUocTinh = query.Sum(t => t.SanLuongUocTinh);

        var result = await query.ToPagedResultAsync(
            data.PageNumber,
            data.PageSize,
            data.OrderBy,
            data.Ascending
        );

        var response = new CongBoSanLuongCayHangNamKhacPagedResponse
        {
            Data = result,
            TongSanLuong = TongSanLuong,
            TongSanLuongUocTinh = TongSanLuongUocTinh
        };

        return Ok(ApiResponse<CongBoSanLuongCayHangNamKhacPagedResponse>.Ok(response));
    }

    [HttpPost("sanluongcayhangnamkhac/delete/{id:int}")]
    public async Task<IActionResult> DeleteCongBoSanLuongCayHangNamKhac(int id)
    {
        var existingData = await context.CongBoSanLuongCayHangNamKhacs.FindAsync(id);
        if (existingData == null)
            return BadRequest(ApiResponse<bool>.Fail(new List<string> { "Không tìm thấy dữ liệu cần xóa." }));
        if (existingData.IsCongBo == 1)
            return BadRequest(ApiResponse<bool>.Fail(new List<string> { "Không thể xóa dữ liệu đã được công bố." }));
        context.CongBoSanLuongCayHangNamKhacs.Remove(existingData);
        await context.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Ok(true, "Xóa dữ liệu thành công."));
    }

    [HttpPost("sanluongcayhangnamkhac/upsert")]
    public async Task<IActionResult> UpsertCongBoSanLuongCayHangNamKhac([FromBody] CongBoSanLuongCayHangNamKhac data)
    {
        if (data.Id > 0)
        {
            if (data.IsCongBo == 1)
                return BadRequest(
                    ApiResponse<CongBoSanLuongCayHangNamKhac>.Fail(["Không thể cập dữ liệu đã được công bố."]));
            var existingData = await context.CongBoSanLuongCayHangNamKhacs.FindAsync(data.Id);
            if (existingData == null)
                return BadRequest(
                    ApiResponse<CongBoSanLuongCayHangNamKhac>.Fail(["Không tìm thấy dữ liệu cần cập nhật."]));
            existingData.Assign(data);
            await context.SaveChangesAsync();
            return Ok(ApiResponse<CongBoSanLuongCayHangNamKhac>.Ok(existingData));
        }

        context.CongBoSanLuongCayHangNamKhacs.Add(data);
        await context.SaveChangesAsync();
        return Ok(ApiResponse<CongBoSanLuongCayHangNamKhac>.Ok(data));
    }

    [HttpGet("sanluongcayhangnamkhac/{id:int}")]
    public async Task<IActionResult> GetByIdCongBoSanLuongCayHangNamKhac(int id)
    {
        var result = await context.CongBoSanLuongCayHangNamKhacs
            .FindAsync(id);
        if (result == null)
            return NotFound(ApiResponse<CongBoSanLuongCayHangNamKhac>.Fail(["Không tìm thấy dữ liệu."]));

        return Ok(ApiResponse<CongBoSanLuongCayHangNamKhac>.Ok(result));
    }

    [HttpGet("sanluongcayhangnamkhac/changestatus/{id:int}/{iscongbo:int}")]
    public async Task<IActionResult> ChangeStatusCongBoSanLuongCayHangNamKhac(int id, int iscongbo)
    {
        var result = await context.CongBoSanLuongCayHangNamKhacs
            .FindAsync(id);
        if (result == null)
            return NotFound(ApiResponse<bool>.Fail(["Không tìm thấy dữ liệu."]));
        if (iscongbo != 0 && iscongbo != 1)
            return BadRequest(ApiResponse<bool>.Fail(["Trạng thái công bố không hợp lệ."]));
        if (result.IsCongBo == iscongbo)
            return BadRequest(ApiResponse<bool>.Fail(["Không có thay đổi về trạng thái công bố"]));
        result.IsCongBo = iscongbo;
        context.CongBoSanLuongCayHangNamKhacs.Update(result);
        await context.SaveChangesAsync();

        return Ok(ApiResponse<bool>.Ok(true, "Cập nhật trạng thái thành công."));
    }

    //Diện tich gieo trồng và thu hoạch cây lâu năm chính
    [HttpPost("dientichcaylaunamchinh/get-data-lists")]
    public async Task<IActionResult> GetPagedDataCongBoDienTichCayLauNamChinh(CongBoDienTichCayHangNamKhacQueryDto data)
    {
        var query = context.CongBoDienTichCayLauNamChinhs.AsQueryable();

        if (data.CreatedFrom.HasValue)
        {
            var utcCreatedFrom = data.CreatedFrom.Value.ToUtc();
            query = query.Where(t => t.CreatedAt >= utcCreatedFrom);
        }

        if (data.CreatedTo.HasValue)
        {
            var utcCreatedTo = data.CreatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.CreatedAt < utcCreatedTo);
        }

        if (data.UpdatedFrom.HasValue)
        {
            var utcUpdatedFrom = data.UpdatedFrom.Value.ToUtc();
            query = query.Where(t => t.UpdatedAt >= utcUpdatedFrom);
        }

        if (data.UpdatedTo.HasValue)
        {
            var utcUpdatedTo = data.UpdatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.UpdatedAt < utcUpdatedTo);
        }


        if (!string.IsNullOrEmpty(data.CreatedBy))
        {
            var createdBy = data.CreatedBy.ToLower();
            query = query.Where(t => (t.CreatedBy ?? "").ToLower().Contains(createdBy));
        }

        if (data.Nams != null && data.Nams.Count > 0) query = query.Where(t => data.Nams.Contains(t.Nam));
        if (data.Thangs != null && data.Thangs.Count > 0) query = query.Where(t => data.Thangs.Contains(t.Thang));
        if (data.MaTinhThanhs != null && data.MaTinhThanhs.Count > 0)
            query = query.Where(t => data.MaTinhThanhs.Contains(t.MaTinhThanh));
        if (data.LoaiCays != null && data.LoaiCays.Count > 0)
            query = query.Where(t => data.LoaiCays.Contains(t.LoaiCay));
        if (data.MuaVus != null && data.MuaVus.Count > 0)
            query = query.Where(t => data.MuaVus.Contains(t.MuaVu));
        if (data.IsCongBo.HasValue) query = query.Where(t => t.IsCongBo == data.IsCongBo.Value);

        if (!string.IsNullOrEmpty(data.Keyword))
        {
            var keyword = data.Keyword.RemoveSpecialCharactersUnicode().ToLower();
            query = query.Where(t =>
                t.Nam.ToString().ToLower().Contains(keyword) ||
                t.Thang.ToString().ToLower().Contains(keyword));
        }

        var TongDienTichThuHoach = query.Sum(t => t.DienTichThuHoach);
        var TongDienTichGieoTrong = query.Sum(t => t.DienTichGieoTrong);
        var TongDienTichUocTinhGieoTrong = query.Sum(t => t.DienTichUocTinhGieoTrong);
        var TongDienTichUocTinhThuHoach = query.Sum(t => t.DienTichUocTinhThuHoach);

        var result = await query.ToPagedResultAsync(
            data.PageNumber,
            data.PageSize,
            data.OrderBy,
            data.Ascending
        );

        var response = new CongBoDienTichCayLauNamChinhPagedResponse
        {
            Data = result,
            TongDienTichThuHoach = TongDienTichThuHoach,
            TongDienTichGieoTrong = TongDienTichGieoTrong,
            TongDienTichUocTinhGieoTrong = TongDienTichUocTinhGieoTrong,
            TongDienTichUocTinhThuHoach = TongDienTichUocTinhThuHoach
        };

        return Ok(ApiResponse<CongBoDienTichCayLauNamChinhPagedResponse>.Ok(response));
    }

    [HttpPost("dientichcaylaunamchinh/delete/{id:int}")]
    public async Task<IActionResult> DeleteCongBoDienTichCayLauNamChinh(int id)
    {
        var existingData = await context.CongBoDienTichCayLauNamChinhs.FindAsync(id);
        if (existingData == null)
            return BadRequest(ApiResponse<bool>.Fail(new List<string> { "Không tìm thấy dữ liệu cần xóa." }));
        if (existingData.IsCongBo == 1)
            return BadRequest(ApiResponse<bool>.Fail(new List<string> { "Không thể xóa dữ liệu đã được công bố." }));
        context.CongBoDienTichCayLauNamChinhs.Remove(existingData);
        await context.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Ok(true, "Xóa dữ liệu thành công."));
    }

    [HttpPost("dientichcaylaunamchinh/upsert")]
    public async Task<IActionResult> UpsertCongBoDienTichCayLauNamChinh([FromBody] CongBoDienTichCayLauNamChinh data)
    {
        if (data.Id > 0)
        {
            if (data.IsCongBo == 1)
                return BadRequest(
                    ApiResponse<CongBoDienTichCayLauNamChinh>.Fail(["Không thể cập dữ liệu đã được công bố."]));
            var existingData = await context.CongBoDienTichCayLauNamChinhs.FindAsync(data.Id);
            if (existingData == null)
                return BadRequest(
                    ApiResponse<CongBoDienTichCayLauNamChinh>.Fail(["Không tìm thấy dữ liệu cần cập nhật."]));
            existingData.Assign(data);
            await context.SaveChangesAsync();
            return Ok(ApiResponse<CongBoDienTichCayLauNamChinh>.Ok(existingData));
        }

        context.CongBoDienTichCayLauNamChinhs.Add(data);
        await context.SaveChangesAsync();
        return Ok(ApiResponse<CongBoDienTichCayLauNamChinh>.Ok(data));
    }

    [HttpGet("dientichcaylaunamchinh/{id:int}")]
    public async Task<IActionResult> GetByIdCongBoDienTichCayLauNamChinh(int id)
    {
        var result = await context.CongBoDienTichCayLauNamChinhs
            .FindAsync(id);
        if (result == null)
            return NotFound(ApiResponse<CongBoDienTichCayLauNamChinh>.Fail(["Không tìm thấy dữ liệu."]));

        return Ok(ApiResponse<CongBoDienTichCayLauNamChinh>.Ok(result));
    }

    [HttpGet("dientichcaylaunamchinh/changestatus/{id:int}/{iscongbo:int}")]
    public async Task<IActionResult> ChangeStatusCongBoDienTichCayLauNamChinh(int id, int iscongbo)
    {
        var result = await context.CongBoDienTichCayLauNamChinhs
            .FindAsync(id);
        if (result == null)
            return NotFound(ApiResponse<bool>.Fail(["Không tìm thấy dữ liệu."]));
        if (iscongbo != 0 && iscongbo != 1)
            return BadRequest(ApiResponse<bool>.Fail(["Trạng thái công bố không hợp lệ."]));
        if (result.IsCongBo == iscongbo)
            return BadRequest(ApiResponse<bool>.Fail(["Không có thay đổi về trạng thái công bố"]));
        result.IsCongBo = iscongbo;
        context.CongBoDienTichCayLauNamChinhs.Update(result);
        await context.SaveChangesAsync();

        return Ok(ApiResponse<bool>.Ok(true, "Cập nhật trạng thái thành công."));
    }

    //Sản lượng một số cây lâu năm chính
    [HttpPost("sanluongcaylaunamchinh/get-data-lists")]
    public async Task<IActionResult> GetPagedDataCongBoSanLuongCayLauNamChinh(CongBoSanLuongCayHangNamKhacQueryDto data)
    {
        var query = context.CongBoSanLuongCayLauNamChinhs.AsQueryable();

        if (data.CreatedFrom.HasValue)
        {
            var utcCreatedFrom = data.CreatedFrom.Value.ToUtc();
            query = query.Where(t => t.CreatedAt >= utcCreatedFrom);
        }

        if (data.CreatedTo.HasValue)
        {
            var utcCreatedTo = data.CreatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.CreatedAt < utcCreatedTo);
        }

        if (data.UpdatedFrom.HasValue)
        {
            var utcUpdatedFrom = data.UpdatedFrom.Value.ToUtc();
            query = query.Where(t => t.UpdatedAt >= utcUpdatedFrom);
        }

        if (data.UpdatedTo.HasValue)
        {
            var utcUpdatedTo = data.UpdatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.UpdatedAt < utcUpdatedTo);
        }


        if (!string.IsNullOrEmpty(data.CreatedBy))
        {
            var createdBy = data.CreatedBy.ToLower();
            query = query.Where(t => (t.CreatedBy ?? "").ToLower().Contains(createdBy));
        }

        if (data.Nams != null && data.Nams.Count > 0) query = query.Where(t => data.Nams.Contains(t.Nam));
        if (data.Thangs != null && data.Thangs.Count > 0) query = query.Where(t => data.Thangs.Contains(t.Thang));
        if (data.MaTinhThanhs != null && data.MaTinhThanhs.Count > 0)
            query = query.Where(t => data.MaTinhThanhs.Contains(t.MaTinhThanh));
        if (data.LoaiCays != null && data.LoaiCays.Count > 0)
            query = query.Where(t => data.LoaiCays.Contains(t.LoaiCay));
        if (data.MuaVus != null && data.MuaVus.Count > 0)
            query = query.Where(t => data.MuaVus.Contains(t.MuaVu));
        if (data.IsCongBo.HasValue) query = query.Where(t => t.IsCongBo == data.IsCongBo.Value);

        if (!string.IsNullOrEmpty(data.Keyword))
        {
            var keyword = data.Keyword.RemoveSpecialCharactersUnicode().ToLower();
            query = query.Where(t =>
                t.Nam.ToString().ToLower().Contains(keyword) ||
                t.Thang.ToString().ToLower().Contains(keyword));
        }

        var TongSanLuong = query.Sum(t => t.SanLuong);
        var TongSanLuongUocTinh = query.Sum(t => t.SanLuongUocTinh);

        var result = await query.ToPagedResultAsync(
            data.PageNumber,
            data.PageSize,
            data.OrderBy,
            data.Ascending
        );

        var response = new CongBoSanLuongCayLauNamChinhPagedResponse
        {
            Data = result,
            TongSanLuong = TongSanLuong,
            TongSanLuongUocTinh = TongSanLuongUocTinh
        };

        return Ok(ApiResponse<CongBoSanLuongCayLauNamChinhPagedResponse>.Ok(response));
    }

    [HttpPost("sanluongcaylaunamchinh/delete/{id:int}")]
    public async Task<IActionResult> DeleteCongBoSanLuongCayLauNamChinh(int id)
    {
        var existingData = await context.CongBoSanLuongCayLauNamChinhs.FindAsync(id);
        if (existingData == null)
            return BadRequest(ApiResponse<bool>.Fail(new List<string> { "Không tìm thấy dữ liệu cần xóa." }));
        if (existingData.IsCongBo == 1)
            return BadRequest(ApiResponse<bool>.Fail(new List<string> { "Không thể xóa dữ liệu đã được công bố." }));
        context.CongBoSanLuongCayLauNamChinhs.Remove(existingData);
        await context.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Ok(true, "Xóa dữ liệu thành công."));
    }

    [HttpPost("sanluongcaylaunamchinh/upsert")]
    public async Task<IActionResult> UpsertCongBoSanLuongCayLauNamChinh([FromBody] CongBoSanLuongCayLauNamChinh data)
    {
        if (data.Id > 0)
        {
            if (data.IsCongBo == 1)
                return BadRequest(
                    ApiResponse<CongBoSanLuongCayLauNamChinh>.Fail(["Không thể cập dữ liệu đã được công bố."]));
            var existingData = await context.CongBoSanLuongCayLauNamChinhs.FindAsync(data.Id);
            if (existingData == null)
                return BadRequest(
                    ApiResponse<CongBoSanLuongCayLauNamChinh>.Fail(["Không tìm thấy dữ liệu cần cập nhật."]));
            existingData.Assign(data);
            await context.SaveChangesAsync();
            return Ok(ApiResponse<CongBoSanLuongCayLauNamChinh>.Ok(existingData));
        }

        context.CongBoSanLuongCayLauNamChinhs.Add(data);
        await context.SaveChangesAsync();
        return Ok(ApiResponse<CongBoSanLuongCayLauNamChinh>.Ok(data));
    }

    [HttpGet("sanluongcaylaunamchinh/{id:int}")]
    public async Task<IActionResult> GetByIdCongBoSanLuongCayLauNamChinh(int id)
    {
        var result = await context.CongBoSanLuongCayLauNamChinhs
            .FindAsync(id);
        if (result == null)
            return NotFound(ApiResponse<CongBoSanLuongCayLauNamChinh>.Fail(["Không tìm thấy dữ liệu."]));

        return Ok(ApiResponse<CongBoSanLuongCayLauNamChinh>.Ok(result));
    }

    [HttpGet("sanluongcaylaunamchinh/changestatus/{id:int}/{iscongbo:int}")]
    public async Task<IActionResult> ChangeStatusCongBoSanLuongCayLauNamChinh(int id, int iscongbo)
    {
        var result = await context.CongBoSanLuongCayLauNamChinhs
            .FindAsync(id);
        if (result == null)
            return NotFound(ApiResponse<bool>.Fail(["Không tìm thấy dữ liệu."]));
        if (iscongbo != 0 && iscongbo != 1)
            return BadRequest(ApiResponse<bool>.Fail(["Trạng thái công bố không hợp lệ."]));
        if (result.IsCongBo == iscongbo)
            return BadRequest(ApiResponse<bool>.Fail(["Không có thay đổi về trạng thái công bố"]));
        result.IsCongBo = iscongbo;
        context.CongBoSanLuongCayLauNamChinhs.Update(result);
        await context.SaveChangesAsync();

        return Ok(ApiResponse<bool>.Ok(true, "Cập nhật trạng thái thành công."));
    }

    //Số lượng gia súc, gia cầm
    [HttpPost("soluonggiasucgiacam/get-data-lists")]
    public async Task<IActionResult> GetPagedDataCongBoSoLuongGiaSucGiaCam(CongBoSoLuongGiaSucGiaCamQueryDto data)
    {
        var query = context.CongBoSoLuongGiaSucGiaCams.AsQueryable();

        if (data.CreatedFrom.HasValue)
        {
            var utcCreatedFrom = data.CreatedFrom.Value.ToUtc();
            query = query.Where(t => t.CreatedAt >= utcCreatedFrom);
        }

        if (data.CreatedTo.HasValue)
        {
            var utcCreatedTo = data.CreatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.CreatedAt < utcCreatedTo);
        }

        if (data.UpdatedFrom.HasValue)
        {
            var utcUpdatedFrom = data.UpdatedFrom.Value.ToUtc();
            query = query.Where(t => t.UpdatedAt >= utcUpdatedFrom);
        }

        if (data.UpdatedTo.HasValue)
        {
            var utcUpdatedTo = data.UpdatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.UpdatedAt < utcUpdatedTo);
        }


        if (!string.IsNullOrEmpty(data.CreatedBy))
        {
            var createdBy = data.CreatedBy.ToLower();
            query = query.Where(t => (t.CreatedBy ?? "").ToLower().Contains(createdBy));
        }

        if (data.Nams != null && data.Nams.Count > 0) query = query.Where(t => data.Nams.Contains(t.Nam));
        if (data.Thangs != null && data.Thangs.Count > 0) query = query.Where(t => data.Thangs.Contains(t.Thang));
        if (data.MaTinhThanhs != null && data.MaTinhThanhs.Count > 0)
            query = query.Where(t => data.MaTinhThanhs.Contains(t.MaTinhThanh));
        if (data.LoaiVatNuois != null && data.LoaiVatNuois.Count > 0)
            query = query.Where(t => data.LoaiVatNuois.Contains(t.LoaiVatNuoi));
        if (data.IsCongBo.HasValue) query = query.Where(t => t.IsCongBo == data.IsCongBo.Value);

        if (!string.IsNullOrEmpty(data.Keyword))
        {
            var keyword = data.Keyword.RemoveSpecialCharactersUnicode().ToLower();
            query = query.Where(t =>
                t.Nam.ToString().ToLower().Contains(keyword) ||
                t.Thang.ToString().ToLower().Contains(keyword));
        }

        var TongSoLuong = query.Sum(t => t.SoLuong);
        var TongSoLuongUocTinh = query.Sum(t => t.SoLuongUocTinh);

        var result = await query.ToPagedResultAsync(
            data.PageNumber,
            data.PageSize,
            data.OrderBy,
            data.Ascending
        );

        var response = new CongBoSoLuongGiaSucGiaCamPagedResponse
        {
            Data = result,
            TongSoLuong = TongSoLuong,
            TongSoLuongUocTinh = TongSoLuongUocTinh
        };

        return Ok(ApiResponse<CongBoSoLuongGiaSucGiaCamPagedResponse>.Ok(response));
    }

    [HttpPost("soluonggiasucgiacam/delete/{id:int}")]
    public async Task<IActionResult> DeleteCongBoSoLuongGiaSucGiaCam(int id)
    {
        var existingData = await context.CongBoSoLuongGiaSucGiaCams.FindAsync(id);
        if (existingData == null)
            return BadRequest(ApiResponse<bool>.Fail(new List<string> { "Không tìm thấy dữ liệu cần xóa." }));
        if (existingData.IsCongBo == 1)
            return BadRequest(ApiResponse<bool>.Fail(new List<string> { "Không thể xóa dữ liệu đã được công bố." }));
        context.CongBoSoLuongGiaSucGiaCams.Remove(existingData);
        await context.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Ok(true, "Xóa dữ liệu thành công."));
    }

    [HttpPost("soluonggiasucgiacam/upsert")]
    public async Task<IActionResult> UpsertCongBoSoLuongGiaSucGiaCam([FromBody] CongBoSoLuongGiaSucGiaCam data)
    {
        if (data.Id > 0)
        {
            if (data.IsCongBo == 1)
                return BadRequest(
                    ApiResponse<CongBoSoLuongGiaSucGiaCam>.Fail(["Không thể cập dữ liệu đã được công bố."]));
            var existingData = await context.CongBoSoLuongGiaSucGiaCams.FindAsync(data.Id);
            if (existingData == null)
                return BadRequest(
                    ApiResponse<CongBoSoLuongGiaSucGiaCam>.Fail(["Không tìm thấy dữ liệu cần cập nhật."]));
            existingData.Assign(data);
            await context.SaveChangesAsync();
            return Ok(ApiResponse<CongBoSoLuongGiaSucGiaCam>.Ok(existingData));
        }

        context.CongBoSoLuongGiaSucGiaCams.Add(data);
        await context.SaveChangesAsync();
        return Ok(ApiResponse<CongBoSoLuongGiaSucGiaCam>.Ok(data));
    }

    [HttpGet("soluonggiasucgiacam/{id:int}")]
    public async Task<IActionResult> GetByIdCongBoSoLuongGiaSucGiaCam(int id)
    {
        var result = await context.CongBoSoLuongGiaSucGiaCams
            .FindAsync(id);
        if (result == null)
            return NotFound(ApiResponse<CongBoSoLuongGiaSucGiaCam>.Fail(["Không tìm thấy dữ liệu."]));

        return Ok(ApiResponse<CongBoSoLuongGiaSucGiaCam>.Ok(result));
    }

    [HttpGet("soluonggiasucgiacam/changestatus/{id:int}/{iscongbo:int}")]
    public async Task<IActionResult> ChangeStatusCongBoSoLuongGiaSucGiaCam(int id, int iscongbo)
    {
        var result = await context.CongBoSoLuongGiaSucGiaCams
            .FindAsync(id);
        if (result == null)
            return NotFound(ApiResponse<bool>.Fail(["Không tìm thấy dữ liệu."]));
        if (iscongbo != 0 && iscongbo != 1)
            return BadRequest(ApiResponse<bool>.Fail(["Trạng thái công bố không hợp lệ."]));
        if (result.IsCongBo == iscongbo)
            return BadRequest(ApiResponse<bool>.Fail(["Không có thay đổi về trạng thái công bố"]));
        result.IsCongBo = iscongbo;
        context.CongBoSoLuongGiaSucGiaCams.Update(result);
        await context.SaveChangesAsync();

        return Ok(ApiResponse<bool>.Ok(true, "Cập nhật trạng thái thành công."));
    }

    //Diện tích nuôi trồng thủy sản
    [HttpPost("dientichnuoitrongthuysan/get-data-lists")]
    public async Task<IActionResult> GetPagedDataCongBoDienTichNuoiTrongThuySan(CongBoDienTichNuoiTrongThuySanQueryDto data)
    {
        var query = context.CongBoDienTichNuoiTrongThuySans.AsQueryable();

        if (data.CreatedFrom.HasValue)
        {
            var utcCreatedFrom = data.CreatedFrom.Value.ToUtc();
            query = query.Where(t => t.CreatedAt >= utcCreatedFrom);
        }

        if (data.CreatedTo.HasValue)
        {
            var utcCreatedTo = data.CreatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.CreatedAt < utcCreatedTo);
        }

        if (data.UpdatedFrom.HasValue)
        {
            var utcUpdatedFrom = data.UpdatedFrom.Value.ToUtc();
            query = query.Where(t => t.UpdatedAt >= utcUpdatedFrom);
        }

        if (data.UpdatedTo.HasValue)
        {
            var utcUpdatedTo = data.UpdatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.UpdatedAt < utcUpdatedTo);
        }


        if (!string.IsNullOrEmpty(data.CreatedBy))
        {
            var createdBy = data.CreatedBy.ToLower();
            query = query.Where(t => (t.CreatedBy ?? "").ToLower().Contains(createdBy));
        }

        if (data.Nams != null && data.Nams.Count > 0) query = query.Where(t => data.Nams.Contains(t.Nam));
        if (data.Thangs != null && data.Thangs.Count > 0) query = query.Where(t => data.Thangs.Contains(t.Thang));
        if (data.MaTinhThanhs != null && data.MaTinhThanhs.Count > 0)
            query = query.Where(t => data.MaTinhThanhs.Contains(t.MaTinhThanh));
        if (data.LoaiThuySanChinhs != null && data.LoaiThuySanChinhs.Count > 0)
            query = query.Where(t => data.LoaiThuySanChinhs.Contains(t.LoaiThuySanChinh));
        if (data.MoiTruongNuois != null && data.MoiTruongNuois.Count > 0)
            query = query.Where(t => data.MoiTruongNuois.Contains(t.MoiTruongNuoi));
        if (data.IsCongBo.HasValue) query = query.Where(t => t.IsCongBo == data.IsCongBo.Value);

        if (!string.IsNullOrEmpty(data.Keyword))
        {
            var keyword = data.Keyword.RemoveSpecialCharactersUnicode().ToLower();
            query = query.Where(t =>
                t.Nam.ToString().ToLower().Contains(keyword) ||
                t.Thang.ToString().ToLower().Contains(keyword));
        }

        var TongDienTich = query.Sum(t => t.DienTich);
        var TongDienTichUocTinh = query.Sum(t => t.DienTichUocTinh);

        var result = await query.ToPagedResultAsync(
            data.PageNumber,
            data.PageSize,
            data.OrderBy,
            data.Ascending
        );

        var response = new CongBoDienTichNuoiTrongThuySanPagedResponse
        {
            Data = result,
            TongDienTich = TongDienTich,
            TongDienTichUocTinh = TongDienTichUocTinh
        };

        return Ok(ApiResponse<CongBoDienTichNuoiTrongThuySanPagedResponse>.Ok(response));
    }

    [HttpPost("dientichnuoitrongthuysan/delete/{id:int}")]
    public async Task<IActionResult> DeleteCongBoDienTichNuoiTrongThuySan(int id)
    {
        var existingData = await context.CongBoDienTichNuoiTrongThuySans.FindAsync(id);
        if (existingData == null)
            return BadRequest(ApiResponse<bool>.Fail(new List<string> { "Không tìm thấy dữ liệu cần xóa." }));
        if (existingData.IsCongBo == 1)
            return BadRequest(ApiResponse<bool>.Fail(new List<string> { "Không thể xóa dữ liệu đã được công bố." }));
        context.CongBoDienTichNuoiTrongThuySans.Remove(existingData);
        await context.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Ok(true, "Xóa dữ liệu thành công."));
    }

    [HttpPost("dientichnuoitrongthuysan/upsert")]
    public async Task<IActionResult> UpsertCongBoDienTichNuoiTrongThuySan([FromBody] CongBoDienTichNuoiTrongThuySan data)
    {
        if (data.Id > 0)
        {
            if (data.IsCongBo == 1)
                return BadRequest(
                    ApiResponse<CongBoDienTichNuoiTrongThuySan>.Fail(["Không thể cập dữ liệu đã được công bố."]));
            var existingData = await context.CongBoDienTichNuoiTrongThuySans.FindAsync(data.Id);
            if (existingData == null)
                return BadRequest(
                    ApiResponse<CongBoDienTichNuoiTrongThuySan>.Fail(["Không tìm thấy dữ liệu cần cập nhật."]));
            existingData.Assign(data);
            await context.SaveChangesAsync();
            return Ok(ApiResponse<CongBoDienTichNuoiTrongThuySan>.Ok(existingData));
        }

        context.CongBoDienTichNuoiTrongThuySans.Add(data);
        await context.SaveChangesAsync();
        return Ok(ApiResponse<CongBoDienTichNuoiTrongThuySan>.Ok(data));
    }

    [HttpGet("dientichnuoitrongthuysan/{id:int}")]
    public async Task<IActionResult> GetByIdCongBoDienTichNuoiTrongThuySan(int id)
    {
        var result = await context.CongBoDienTichNuoiTrongThuySans
            .FindAsync(id);
        if (result == null)
            return NotFound(ApiResponse<CongBoDienTichNuoiTrongThuySan>.Fail(["Không tìm thấy dữ liệu."]));

        return Ok(ApiResponse<CongBoDienTichNuoiTrongThuySan>.Ok(result));
    }

    [HttpGet("dientichnuoitrongthuysan/changestatus/{id:int}/{iscongbo:int}")]
    public async Task<IActionResult> ChangeStatusCongBoDienTichNuoiTrongThuySan(int id, int iscongbo)
    {
        var result = await context.CongBoDienTichNuoiTrongThuySans
            .FindAsync(id);
        if (result == null)
            return NotFound(ApiResponse<bool>.Fail(["Không tìm thấy dữ liệu."]));
        if (iscongbo != 0 && iscongbo != 1)
            return BadRequest(ApiResponse<bool>.Fail(["Trạng thái công bố không hợp lệ."]));
        if (result.IsCongBo == iscongbo)
            return BadRequest(ApiResponse<bool>.Fail(["Không có thay đổi về trạng thái công bố"]));
        result.IsCongBo = iscongbo;
        context.CongBoDienTichNuoiTrongThuySans.Update(result);
        await context.SaveChangesAsync();

        return Ok(ApiResponse<bool>.Ok(true, "Cập nhật trạng thái thành công."));
    }

    //Sản lượng nuôi trồng thủy sản
    [HttpPost("sanluongnuoitrongthuysan/get-data-lists")]
    public async Task<IActionResult> GetPagedDataCongBoSanLuongNuoiTrongThuySan(CongBoDienTichNuoiTrongThuySanQueryDto data)
    {
        var query = context.CongBoSanLuongNuoiTrongThuySans.AsQueryable();

        if (data.CreatedFrom.HasValue)
        {
            var utcCreatedFrom = data.CreatedFrom.Value.ToUtc();
            query = query.Where(t => t.CreatedAt >= utcCreatedFrom);
        }

        if (data.CreatedTo.HasValue)
        {
            var utcCreatedTo = data.CreatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.CreatedAt < utcCreatedTo);
        }

        if (data.UpdatedFrom.HasValue)
        {
            var utcUpdatedFrom = data.UpdatedFrom.Value.ToUtc();
            query = query.Where(t => t.UpdatedAt >= utcUpdatedFrom);
        }

        if (data.UpdatedTo.HasValue)
        {
            var utcUpdatedTo = data.UpdatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.UpdatedAt < utcUpdatedTo);
        }


        if (!string.IsNullOrEmpty(data.CreatedBy))
        {
            var createdBy = data.CreatedBy.ToLower();
            query = query.Where(t => (t.CreatedBy ?? "").ToLower().Contains(createdBy));
        }

        if (data.Nams != null && data.Nams.Count > 0) query = query.Where(t => data.Nams.Contains(t.Nam));
        if (data.Thangs != null && data.Thangs.Count > 0) query = query.Where(t => data.Thangs.Contains(t.Thang));
        if (data.MaTinhThanhs != null && data.MaTinhThanhs.Count > 0)
            query = query.Where(t => data.MaTinhThanhs.Contains(t.MaTinhThanh));
        if (data.LoaiThuySanChinhs != null && data.LoaiThuySanChinhs.Count > 0)
            query = query.Where(t => data.LoaiThuySanChinhs.Contains(t.LoaiThuySanChinh));
        if (data.MoiTruongNuois != null && data.MoiTruongNuois.Count > 0)
            query = query.Where(t => data.MoiTruongNuois.Contains(t.MoiTruongNuoi));
        if (data.IsCongBo.HasValue) query = query.Where(t => t.IsCongBo == data.IsCongBo.Value);

        if (!string.IsNullOrEmpty(data.Keyword))
        {
            var keyword = data.Keyword.RemoveSpecialCharactersUnicode().ToLower();
            query = query.Where(t =>
                t.Nam.ToString().ToLower().Contains(keyword) ||
                t.Thang.ToString().ToLower().Contains(keyword));
        }

        var TongSanLuong = query.Sum(t => t.SanLuong);
        var TongSanLuongUocTinh = query.Sum(t => t.SanLuongUocTinh);

        var result = await query.ToPagedResultAsync(
            data.PageNumber,
            data.PageSize,
            data.OrderBy,
            data.Ascending
        );

        var response = new CongBoSanLuongNuoiTrongThuySanPagedResponse
        {
            Data = result,
            TongSanLuong = TongSanLuong,
            TongSanLuongUocTinh = TongSanLuongUocTinh
        };

        return Ok(ApiResponse<CongBoSanLuongNuoiTrongThuySanPagedResponse>.Ok(response));
    }

    [HttpPost("sanluongnuoitrongthuysan/delete/{id:int}")]
    public async Task<IActionResult> DeleteCongBoSanLuongNuoiTrongThuySan(int id)
    {
        var existingData = await context.CongBoSanLuongNuoiTrongThuySans.FindAsync(id);
        if (existingData == null)
            return BadRequest(ApiResponse<bool>.Fail(new List<string> { "Không tìm thấy dữ liệu cần xóa." }));
        if (existingData.IsCongBo == 1)
            return BadRequest(ApiResponse<bool>.Fail(new List<string> { "Không thể xóa dữ liệu đã được công bố." }));
        context.CongBoSanLuongNuoiTrongThuySans.Remove(existingData);
        await context.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Ok(true, "Xóa dữ liệu thành công."));
    }

    [HttpPost("sanluongnuoitrongthuysan/upsert")]
    public async Task<IActionResult> UpsertCongBoSanLuongNuoiTrongThuySan([FromBody] CongBoSanLuongNuoiTrongThuySan data)
    {
        if (data.Id > 0)
        {
            if (data.IsCongBo == 1)
                return BadRequest(
                    ApiResponse<CongBoSanLuongNuoiTrongThuySan>.Fail(["Không thể cập dữ liệu đã được công bố."]));
            var existingData = await context.CongBoSanLuongNuoiTrongThuySans.FindAsync(data.Id);
            if (existingData == null)
                return BadRequest(
                    ApiResponse<CongBoSanLuongNuoiTrongThuySan>.Fail(["Không tìm thấy dữ liệu cần cập nhật."]));
            existingData.Assign(data);
            await context.SaveChangesAsync();
            return Ok(ApiResponse<CongBoSanLuongNuoiTrongThuySan>.Ok(existingData));
        }

        context.CongBoSanLuongNuoiTrongThuySans.Add(data);
        await context.SaveChangesAsync();
        return Ok(ApiResponse<CongBoSanLuongNuoiTrongThuySan>.Ok(data));
    }

    [HttpGet("sanluongnuoitrongthuysan/{id:int}")]
    public async Task<IActionResult> GetByIdCongBoSanLuongNuoiTrongThuySan(int id)
    {
        var result = await context.CongBoSanLuongNuoiTrongThuySans
            .FindAsync(id);
        if (result == null)
            return NotFound(ApiResponse<CongBoSanLuongNuoiTrongThuySan>.Fail(["Không tìm thấy dữ liệu."]));

        return Ok(ApiResponse<CongBoSanLuongNuoiTrongThuySan>.Ok(result));
    }

    [HttpGet("sanluongnuoitrongthuysan/changestatus/{id:int}/{iscongbo:int}")]
    public async Task<IActionResult> ChangeStatusCongBoSanLuongNuoiTrongThuySan(int id, int iscongbo)
    {
        var result = await context.CongBoSanLuongNuoiTrongThuySans
            .FindAsync(id);
        if (result == null)
            return NotFound(ApiResponse<bool>.Fail(["Không tìm thấy dữ liệu."]));
        if (iscongbo != 0 && iscongbo != 1)
            return BadRequest(ApiResponse<bool>.Fail(["Trạng thái công bố không hợp lệ."]));
        if (result.IsCongBo == iscongbo)
            return BadRequest(ApiResponse<bool>.Fail(["Không có thay đổi về trạng thái công bố"]));
        result.IsCongBo = iscongbo;
        context.CongBoSanLuongNuoiTrongThuySans.Update(result);
        await context.SaveChangesAsync();

        return Ok(ApiResponse<bool>.Ok(true, "Cập nhật trạng thái thành công."));
    }

    //Sản lượng thủy sản khai thác
    [HttpPost("sanluongthuysankhaithac/get-data-lists")]
    public async Task<IActionResult> GetPagedDataCongBoSanLuongThuySanKhaiThac(CongBoDienTichNuoiTrongThuySanQueryDto data)
    {
        var query = context.CongBoSanLuongThuySanKhaiThacs.AsQueryable();

        if (data.CreatedFrom.HasValue)
        {
            var utcCreatedFrom = data.CreatedFrom.Value.ToUtc();
            query = query.Where(t => t.CreatedAt >= utcCreatedFrom);
        }

        if (data.CreatedTo.HasValue)
        {
            var utcCreatedTo = data.CreatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.CreatedAt < utcCreatedTo);
        }

        if (data.UpdatedFrom.HasValue)
        {
            var utcUpdatedFrom = data.UpdatedFrom.Value.ToUtc();
            query = query.Where(t => t.UpdatedAt >= utcUpdatedFrom);
        }

        if (data.UpdatedTo.HasValue)
        {
            var utcUpdatedTo = data.UpdatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.UpdatedAt < utcUpdatedTo);
        }


        if (!string.IsNullOrEmpty(data.CreatedBy))
        {
            var createdBy = data.CreatedBy.ToLower();
            query = query.Where(t => (t.CreatedBy ?? "").ToLower().Contains(createdBy));
        }

        if (data.Nams != null && data.Nams.Count > 0) query = query.Where(t => data.Nams.Contains(t.Nam));
        if (data.Thangs != null && data.Thangs.Count > 0) query = query.Where(t => data.Thangs.Contains(t.Thang));
        if (data.MaTinhThanhs != null && data.MaTinhThanhs.Count > 0)
            query = query.Where(t => data.MaTinhThanhs.Contains(t.MaTinhThanh));
        if (data.LoaiThuySanChinhs != null && data.LoaiThuySanChinhs.Count > 0)
            query = query.Where(t => data.LoaiThuySanChinhs.Contains(t.LoaiThuySanChinh));
        if (data.IsCongBo.HasValue) query = query.Where(t => t.IsCongBo == data.IsCongBo.Value);

        if (!string.IsNullOrEmpty(data.Keyword))
        {
            var keyword = data.Keyword.RemoveSpecialCharactersUnicode().ToLower();
            query = query.Where(t =>
                t.Nam.ToString().ToLower().Contains(keyword) ||
                t.Thang.ToString().ToLower().Contains(keyword));
        }

        var TongSanLuong = query.Sum(t => t.SanLuong);
        var TongSanLuongUocTinh = query.Sum(t => t.SanLuongUocTinh);

        var result = await query.ToPagedResultAsync(
            data.PageNumber,
            data.PageSize,
            data.OrderBy,
            data.Ascending
        );

        var response = new CongBoSanLuongThuySanKhaiThacPagedResponse
        {
            Data = result,
            TongSanLuong = TongSanLuong,
            TongSanLuongUocTinh = TongSanLuongUocTinh
        };

        return Ok(ApiResponse<CongBoSanLuongThuySanKhaiThacPagedResponse>.Ok(response));
    }

    [HttpPost("sanluongthuysankhaithac/delete/{id:int}")]
    public async Task<IActionResult> DeleteCongBoSanLuongThuySanKhaiThac(int id)
    {
        var existingData = await context.CongBoSanLuongThuySanKhaiThacs.FindAsync(id);
        if (existingData == null)
            return BadRequest(ApiResponse<bool>.Fail(new List<string> { "Không tìm thấy dữ liệu cần xóa." }));
        if (existingData.IsCongBo == 1)
            return BadRequest(ApiResponse<bool>.Fail(new List<string> { "Không thể xóa dữ liệu đã được công bố." }));
        context.CongBoSanLuongThuySanKhaiThacs.Remove(existingData);
        await context.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Ok(true, "Xóa dữ liệu thành công."));
    }

    [HttpPost("sanluongthuysankhaithac/upsert")]
    public async Task<IActionResult> UpsertCongBoSanLuongThuySanKhaiThac([FromBody] CongBoSanLuongThuySanKhaiThac data)
    {
        if (data.Id > 0)
        {
            if (data.IsCongBo == 1)
                return BadRequest(
                    ApiResponse<CongBoSanLuongThuySanKhaiThac>.Fail(["Không thể cập dữ liệu đã được công bố."]));
            var existingData = await context.CongBoSanLuongThuySanKhaiThacs.FindAsync(data.Id);
            if (existingData == null)
                return BadRequest(
                    ApiResponse<CongBoSanLuongThuySanKhaiThac>.Fail(["Không tìm thấy dữ liệu cần cập nhật."]));
            existingData.Assign(data);
            await context.SaveChangesAsync();
            return Ok(ApiResponse<CongBoSanLuongThuySanKhaiThac>.Ok(existingData));
        }

        context.CongBoSanLuongThuySanKhaiThacs.Add(data);
        await context.SaveChangesAsync();
        return Ok(ApiResponse<CongBoSanLuongThuySanKhaiThac>.Ok(data));
    }

    [HttpGet("sanluongthuysankhaithac/{id:int}")]
    public async Task<IActionResult> GetByIdCongBoSanLuongThuySanKhaiThac(int id)
    {
        var result = await context.CongBoSanLuongThuySanKhaiThacs
            .FindAsync(id);
        if (result == null)
            return NotFound(ApiResponse<CongBoSanLuongThuySanKhaiThac>.Fail(["Không tìm thấy dữ liệu."]));

        return Ok(ApiResponse<CongBoSanLuongThuySanKhaiThac>.Ok(result));
    }

    [HttpGet("sanluongthuysankhaithac/changestatus/{id:int}/{iscongbo:int}")]
    public async Task<IActionResult> ChangeStatusCongBoSanLuongThuySanKhaiThac(int id, int iscongbo)
    {
        var result = await context.CongBoSanLuongThuySanKhaiThacs
            .FindAsync(id);
        if (result == null)
            return NotFound(ApiResponse<bool>.Fail(["Không tìm thấy dữ liệu."]));
        if (iscongbo != 0 && iscongbo != 1)
            return BadRequest(ApiResponse<bool>.Fail(["Trạng thái công bố không hợp lệ."]));
        if (result.IsCongBo == iscongbo)
            return BadRequest(ApiResponse<bool>.Fail(["Không có thay đổi về trạng thái công bố"]));
        result.IsCongBo = iscongbo;
        context.CongBoSanLuongThuySanKhaiThacs.Update(result);
        await context.SaveChangesAsync();

        return Ok(ApiResponse<bool>.Ok(true, "Cập nhật trạng thái thành công."));
    }

    //Giá cả thị trường một số sản phẩm NLTS chính
    [HttpPost("giasanphamnltschinh/get-data-lists")]
    public async Task<IActionResult> GetPagedDataCongBoGiaSanPhamNLTSChinh(CongBoGiaSanPhamNLTSChinhQueryDto data)
    {
        var query = context.CongBoGiaSanPhamNLTSChinhs.AsQueryable();

        if (data.CreatedFrom.HasValue)
        {
            var utcCreatedFrom = data.CreatedFrom.Value.ToUtc();
            query = query.Where(t => t.CreatedAt >= utcCreatedFrom);
        }

        if (data.CreatedTo.HasValue)
        {
            var utcCreatedTo = data.CreatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.CreatedAt < utcCreatedTo);
        }

        if (data.UpdatedFrom.HasValue)
        {
            var utcUpdatedFrom = data.UpdatedFrom.Value.ToUtc();
            query = query.Where(t => t.UpdatedAt >= utcUpdatedFrom);
        }

        if (data.UpdatedTo.HasValue)
        {
            var utcUpdatedTo = data.UpdatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.UpdatedAt < utcUpdatedTo);
        }


        if (!string.IsNullOrEmpty(data.CreatedBy))
        {
            var createdBy = data.CreatedBy.ToLower();
            query = query.Where(t => (t.CreatedBy ?? "").ToLower().Contains(createdBy));
        }

        if (data.Nams != null && data.Nams.Count > 0) query = query.Where(t => data.Nams.Contains(t.Nam));
        if (data.Thangs != null && data.Thangs.Count > 0) query = query.Where(t => data.Thangs.Contains(t.Thang));
        if (data.MaTinhThanhs != null && data.MaTinhThanhs.Count > 0)
            query = query.Where(t => data.MaTinhThanhs.Contains(t.MaTinhThanh));
        if (data.SanPhamNLTSs != null && data.SanPhamNLTSs.Count > 0)
            query = query.Where(t => data.SanPhamNLTSs.Contains(t.SanPhamNLTS));
        if (data.LoaiGias != null && data.LoaiGias.Count > 0)
            query = query.Where(t => data.LoaiGias.Contains(t.LoaiGia));
        if (data.IsCongBo.HasValue) query = query.Where(t => t.IsCongBo == data.IsCongBo.Value);

        if (!string.IsNullOrEmpty(data.Keyword))
        {
            var keyword = data.Keyword.RemoveSpecialCharactersUnicode().ToLower();
            query = query.Where(t =>
                t.Nam.ToString().ToLower().Contains(keyword) ||
                t.Thang.ToString().ToLower().Contains(keyword));
        }

        var TongGiaTri = query.Sum(t => t.GiaTri);
        var TongGiaTriUocTinh = query.Sum(t => t.GiaTriUocTinh);

        var result = await query.ToPagedResultAsync(
            data.PageNumber,
            data.PageSize,
            data.OrderBy,
            data.Ascending
        );

        var response = new CongBoGiaSanPhamNLTSChinhPagedResponse
        {
            Data = result,
            TongGiaTri = TongGiaTri,
            TongGiaTriUocTinh = TongGiaTriUocTinh
        };

        return Ok(ApiResponse<CongBoGiaSanPhamNLTSChinhPagedResponse>.Ok(response));
    }

    [HttpPost("giasanphamnltschinh/delete/{id:int}")]
    public async Task<IActionResult> DeleteCongBoGiaSanPhamNLTSChinh(int id)
    {
        var existingData = await context.CongBoGiaSanPhamNLTSChinhs.FindAsync(id);
        if (existingData == null)
            return BadRequest(ApiResponse<bool>.Fail(new List<string> { "Không tìm thấy dữ liệu cần xóa." }));
        if (existingData.IsCongBo == 1)
            return BadRequest(ApiResponse<bool>.Fail(new List<string> { "Không thể xóa dữ liệu đã được công bố." }));
        context.CongBoGiaSanPhamNLTSChinhs.Remove(existingData);
        await context.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Ok(true, "Xóa dữ liệu thành công."));
    }

    [HttpPost("giasanphamnltschinh/upsert")]
    public async Task<IActionResult> UpsertCongBoGiaSanPhamNLTSChinh([FromBody] CongBoGiaSanPhamNLTSChinh data)
    {
        if (data.Id > 0)
        {
            if (data.IsCongBo == 1)
                return BadRequest(
                    ApiResponse<CongBoGiaSanPhamNLTSChinh>.Fail(["Không thể cập dữ liệu đã được công bố."]));
            var existingData = await context.CongBoGiaSanPhamNLTSChinhs.FindAsync(data.Id);
            if (existingData == null)
                return BadRequest(
                    ApiResponse<CongBoGiaSanPhamNLTSChinh>.Fail(["Không tìm thấy dữ liệu cần cập nhật."]));
            existingData.Assign(data);
            await context.SaveChangesAsync();
            return Ok(ApiResponse<CongBoGiaSanPhamNLTSChinh>.Ok(existingData));
        }

        context.CongBoGiaSanPhamNLTSChinhs.Add(data);
        await context.SaveChangesAsync();
        return Ok(ApiResponse<CongBoGiaSanPhamNLTSChinh>.Ok(data));
    }

    [HttpGet("giasanphamnltschinh/{id:int}")]
    public async Task<IActionResult> GetByIdCongBoGiaSanPhamNLTSChinh(int id)
    {
        var result = await context.CongBoGiaSanPhamNLTSChinhs
            .FindAsync(id);
        if (result == null)
            return NotFound(ApiResponse<CongBoGiaSanPhamNLTSChinh>.Fail(["Không tìm thấy dữ liệu."]));

        return Ok(ApiResponse<CongBoGiaSanPhamNLTSChinh>.Ok(result));
    }

    [HttpGet("giasanphamnltschinh/changestatus/{id:int}/{iscongbo:int}")]
    public async Task<IActionResult> ChangeStatusCongBoGiaSanPhamNLTSChinh(int id, int iscongbo)
    {
        var result = await context.CongBoGiaSanPhamNLTSChinhs
            .FindAsync(id);
        if (result == null)
            return NotFound(ApiResponse<bool>.Fail(["Không tìm thấy dữ liệu."]));
        if (iscongbo != 0 && iscongbo != 1)
            return BadRequest(ApiResponse<bool>.Fail(["Trạng thái công bố không hợp lệ."]));
        if (result.IsCongBo == iscongbo)
            return BadRequest(ApiResponse<bool>.Fail(["Không có thay đổi về trạng thái công bố"]));
        result.IsCongBo = iscongbo;
        context.CongBoGiaSanPhamNLTSChinhs.Update(result);
        await context.SaveChangesAsync();

        return Ok(ApiResponse<bool>.Ok(true, "Cập nhật trạng thái thành công."));
    }

    //Xuất khẩu Thủy sản
    [HttpPost("xuatkhauthuysan/get-data-lists")]
    public async Task<IActionResult> GetPagedDataCongBoXuatKhauThuySan(CongBoXuatKhauQueryDto data)
    {
        var query = context.CongBoXuatKhauThuySans.AsQueryable();

        if (data.CreatedFrom.HasValue)
        {
            var utcCreatedFrom = data.CreatedFrom.Value.ToUtc();
            query = query.Where(t => t.CreatedAt >= utcCreatedFrom);
        }

        if (data.CreatedTo.HasValue)
        {
            var utcCreatedTo = data.CreatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.CreatedAt < utcCreatedTo);
        }

        if (data.UpdatedFrom.HasValue)
        {
            var utcUpdatedFrom = data.UpdatedFrom.Value.ToUtc();
            query = query.Where(t => t.UpdatedAt >= utcUpdatedFrom);
        }

        if (data.UpdatedTo.HasValue)
        {
            var utcUpdatedTo = data.UpdatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.UpdatedAt < utcUpdatedTo);
        }


        if (!string.IsNullOrEmpty(data.CreatedBy))
        {
            var createdBy = data.CreatedBy.ToLower();
            query = query.Where(t => (t.CreatedBy ?? "").ToLower().Contains(createdBy));
        }

        if (data.Nams != null && data.Nams.Count > 0) query = query.Where(t => data.Nams.Contains(t.Nam));
        if (data.Thangs != null && data.Thangs.Count > 0) query = query.Where(t => data.Thangs.Contains(t.Thang));
        if (data.MaQuocGiaVungLTs != null && data.MaQuocGiaVungLTs.Count > 0)
            query = query.Where(t => data.MaQuocGiaVungLTs.Contains(t.MaQuocGiaVungLT));
        if (data.IsCongBo.HasValue) query = query.Where(t => t.IsCongBo == data.IsCongBo.Value);

        if (!string.IsNullOrEmpty(data.Keyword))
        {
            var keyword = data.Keyword.RemoveSpecialCharactersUnicode().ToLower();
            query = query.Where(t =>
                t.Nam.ToString().ToLower().Contains(keyword) ||
                t.Thang.ToString().ToLower().Contains(keyword));
        }

        var TongGiaTri = query.Sum(t => t.GiaTri);
        var TongGiaTriUocTinh = query.Sum(t => t.GiaTriUocTinh);

        var result = await query.ToPagedResultAsync(
            data.PageNumber,
            data.PageSize,
            data.OrderBy,
            data.Ascending
        );

        var response = new CongBoXuatKhauThuySanPagedResponse
        {
            Data = result,
            TongGiaTri = TongGiaTri,
            TongGiaTriUocTinh = TongGiaTriUocTinh
        };

        return Ok(ApiResponse<CongBoXuatKhauThuySanPagedResponse>.Ok(response));
    }

    [HttpPost("xuatkhauthuysan/delete/{id:int}")]
    public async Task<IActionResult> DeleteCongBoXuatKhauThuySan(int id)
    {
        var existingData = await context.CongBoXuatKhauThuySans.FindAsync(id);
        if (existingData == null)
            return BadRequest(ApiResponse<bool>.Fail(new List<string> { "Không tìm thấy dữ liệu cần xóa." }));
        if (existingData.IsCongBo == 1)
            return BadRequest(ApiResponse<bool>.Fail(new List<string> { "Không thể xóa dữ liệu đã được công bố." }));
        context.CongBoXuatKhauThuySans.Remove(existingData);
        await context.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Ok(true, "Xóa dữ liệu thành công."));
    }

    [HttpPost("xuatkhauthuysan/upsert")]
    public async Task<IActionResult> UpsertCongBoXuatKhauThuySan([FromBody] CongBoXuatKhauThuySan data)
    {
        if (data.Id > 0)
        {
            if (data.IsCongBo == 1)
                return BadRequest(
                    ApiResponse<CongBoXuatKhauThuySan>.Fail(["Không thể cập dữ liệu đã được công bố."]));
            var existingData = await context.CongBoXuatKhauThuySans.FindAsync(data.Id);
            if (existingData == null)
                return BadRequest(
                    ApiResponse<CongBoXuatKhauThuySan>.Fail(["Không tìm thấy dữ liệu cần cập nhật."]));
            existingData.Assign(data);
            await context.SaveChangesAsync();
            return Ok(ApiResponse<CongBoXuatKhauThuySan>.Ok(existingData));
        }

        context.CongBoXuatKhauThuySans.Add(data);
        await context.SaveChangesAsync();
        return Ok(ApiResponse<CongBoXuatKhauThuySan>.Ok(data));
    }

    [HttpGet("xuatkhauthuysan/{id:int}")]
    public async Task<IActionResult> GetByIdCongBoXuatKhauThuySan(int id)
    {
        var result = await context.CongBoXuatKhauThuySans
            .FindAsync(id);
        if (result == null)
            return NotFound(ApiResponse<CongBoXuatKhauThuySan>.Fail(["Không tìm thấy dữ liệu."]));

        return Ok(ApiResponse<CongBoXuatKhauThuySan>.Ok(result));
    }

    [HttpGet("xuatkhauthuysan/changestatus/{id:int}/{iscongbo:int}")]
    public async Task<IActionResult> ChangeStatusCongBoXuatKhauThuySan(int id, int iscongbo)
    {
        var result = await context.CongBoXuatKhauThuySans
            .FindAsync(id);
        if (result == null)
            return NotFound(ApiResponse<bool>.Fail(["Không tìm thấy dữ liệu."]));
        if (iscongbo != 0 && iscongbo != 1)
            return BadRequest(ApiResponse<bool>.Fail(["Trạng thái công bố không hợp lệ."]));
        if (result.IsCongBo == iscongbo)
            return BadRequest(ApiResponse<bool>.Fail(["Không có thay đổi về trạng thái công bố"]));
        result.IsCongBo = iscongbo;
        context.CongBoXuatKhauThuySans.Update(result);
        await context.SaveChangesAsync();

        return Ok(ApiResponse<bool>.Ok(true, "Cập nhật trạng thái thành công."));
    }

    //Xuất khẩu Sản phẩm chăn nuôi
    [HttpPost("xuatkhausanphamchannuoi/catra/get-data-lists")]
    public async Task<IActionResult> GetPagedDataCongBoXuatKhauSanPhamChanNuoi_CaTra(CongBoXuatKhauQueryDto data)
    {
        var query = context.CongBoXuatKhauSanPhamChanNuois.Where(x => x.LoaiSanPhamChanNuoi == "Cá tra").AsQueryable();

        if (data.CreatedFrom.HasValue)
        {
            var utcCreatedFrom = data.CreatedFrom.Value.ToUtc();
            query = query.Where(t => t.CreatedAt >= utcCreatedFrom);
        }

        if (data.CreatedTo.HasValue)
        {
            var utcCreatedTo = data.CreatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.CreatedAt < utcCreatedTo);
        }

        if (data.UpdatedFrom.HasValue)
        {
            var utcUpdatedFrom = data.UpdatedFrom.Value.ToUtc();
            query = query.Where(t => t.UpdatedAt >= utcUpdatedFrom);
        }

        if (data.UpdatedTo.HasValue)
        {
            var utcUpdatedTo = data.UpdatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.UpdatedAt < utcUpdatedTo);
        }


        if (!string.IsNullOrEmpty(data.CreatedBy))
        {
            var createdBy = data.CreatedBy.ToLower();
            query = query.Where(t => (t.CreatedBy ?? "").ToLower().Contains(createdBy));
        }

        if (data.Nams != null && data.Nams.Count > 0) query = query.Where(t => data.Nams.Contains(t.Nam));
        if (data.Thangs != null && data.Thangs.Count > 0) query = query.Where(t => data.Thangs.Contains(t.Thang));
        if (data.MaQuocGiaVungLTs != null && data.MaQuocGiaVungLTs.Count > 0)
            query = query.Where(t => data.MaQuocGiaVungLTs.Contains(t.MaQuocGiaVungLT));
        if (data.IsCongBo.HasValue) query = query.Where(t => t.IsCongBo == data.IsCongBo.Value);

        if (!string.IsNullOrEmpty(data.Keyword))
        {
            var keyword = data.Keyword.RemoveSpecialCharactersUnicode().ToLower();
            query = query.Where(t =>
                t.Nam.ToString().ToLower().Contains(keyword) ||
                t.Thang.ToString().ToLower().Contains(keyword));
        }

        var TongGiaTri = query.Sum(t => t.GiaTri);
        var TongGiaTriUocTinh = query.Sum(t => t.GiaTriUocTinh);

        var result = await query.ToPagedResultAsync(
            data.PageNumber,
            data.PageSize,
            data.OrderBy,
            data.Ascending
        );

        var response = new CongBoXuatKhauSanPhamChanNuoiPagedResponse
        {
            Data = result,
            TongGiaTri = TongGiaTri,
            TongGiaTriUocTinh = TongGiaTriUocTinh
        };

        return Ok(ApiResponse<CongBoXuatKhauSanPhamChanNuoiPagedResponse>.Ok(response));
    }

    [HttpPost("xuatkhausanphamchannuoi/tom/get-data-lists")]
    public async Task<IActionResult> GetPagedDataCongBoXuatKhauSanPhamChanNuoi_Tom(CongBoXuatKhauQueryDto data)
    {
        var query = context.CongBoXuatKhauSanPhamChanNuois.Where(x => x.LoaiSanPhamChanNuoi == "Tôm").AsQueryable();

        if (data.CreatedFrom.HasValue)
        {
            var utcCreatedFrom = data.CreatedFrom.Value.ToUtc();
            query = query.Where(t => t.CreatedAt >= utcCreatedFrom);
        }

        if (data.CreatedTo.HasValue)
        {
            var utcCreatedTo = data.CreatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.CreatedAt < utcCreatedTo);
        }

        if (data.UpdatedFrom.HasValue)
        {
            var utcUpdatedFrom = data.UpdatedFrom.Value.ToUtc();
            query = query.Where(t => t.UpdatedAt >= utcUpdatedFrom);
        }

        if (data.UpdatedTo.HasValue)
        {
            var utcUpdatedTo = data.UpdatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.UpdatedAt < utcUpdatedTo);
        }


        if (!string.IsNullOrEmpty(data.CreatedBy))
        {
            var createdBy = data.CreatedBy.ToLower();
            query = query.Where(t => (t.CreatedBy ?? "").ToLower().Contains(createdBy));
        }

        if (data.Nams != null && data.Nams.Count > 0) query = query.Where(t => data.Nams.Contains(t.Nam));
        if (data.Thangs != null && data.Thangs.Count > 0) query = query.Where(t => data.Thangs.Contains(t.Thang));
        if (data.MaQuocGiaVungLTs != null && data.MaQuocGiaVungLTs.Count > 0)
            query = query.Where(t => data.MaQuocGiaVungLTs.Contains(t.MaQuocGiaVungLT));
        if (data.IsCongBo.HasValue) query = query.Where(t => t.IsCongBo == data.IsCongBo.Value);

        if (!string.IsNullOrEmpty(data.Keyword))
        {
            var keyword = data.Keyword.RemoveSpecialCharactersUnicode().ToLower();
            query = query.Where(t =>
                t.Nam.ToString().ToLower().Contains(keyword) ||
                t.Thang.ToString().ToLower().Contains(keyword));
        }

        var TongGiaTri = query.Sum(t => t.GiaTri);
        var TongGiaTriUocTinh = query.Sum(t => t.GiaTriUocTinh);

        var result = await query.ToPagedResultAsync(
            data.PageNumber,
            data.PageSize,
            data.OrderBy,
            data.Ascending
        );

        var response = new CongBoXuatKhauSanPhamChanNuoiPagedResponse
        {
            Data = result,
            TongGiaTri = TongGiaTri,
            TongGiaTriUocTinh = TongGiaTriUocTinh
        };

        return Ok(ApiResponse<CongBoXuatKhauSanPhamChanNuoiPagedResponse>.Ok(response));
    }

    [HttpPost("xuatkhausanphamchannuoi/delete/{id:int}")]
    public async Task<IActionResult> DeleteCongBoXuatKhauSanPhamChanNuoi(int id)
    {
        var existingData = await context.CongBoXuatKhauSanPhamChanNuois.FindAsync(id);
        if (existingData == null)
            return BadRequest(ApiResponse<bool>.Fail(new List<string> { "Không tìm thấy dữ liệu cần xóa." }));
        if (existingData.IsCongBo == 1)
            return BadRequest(ApiResponse<bool>.Fail(new List<string> { "Không thể xóa dữ liệu đã được công bố." }));
        context.CongBoXuatKhauSanPhamChanNuois.Remove(existingData);
        await context.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Ok(true, "Xóa dữ liệu thành công."));
    }

    [HttpPost("xuatkhausanphamchannuoi/upsert")]
    public async Task<IActionResult> UpsertCongBoXuatKhauSanPhamChanNuoi([FromBody] CongBoXuatKhauSanPhamChanNuoi data)
    {
        if (data.Id > 0)
        {
            if (data.IsCongBo == 1)
                return BadRequest(
                    ApiResponse<CongBoXuatKhauSanPhamChanNuoi>.Fail(["Không thể cập dữ liệu đã được công bố."]));
            var existingData = await context.CongBoXuatKhauSanPhamChanNuois.FindAsync(data.Id);
            if (existingData == null)
                return BadRequest(
                    ApiResponse<CongBoXuatKhauSanPhamChanNuoi>.Fail(["Không tìm thấy dữ liệu cần cập nhật."]));
            existingData.Assign(data);
            await context.SaveChangesAsync();
            return Ok(ApiResponse<CongBoXuatKhauSanPhamChanNuoi>.Ok(existingData));
        }

        context.CongBoXuatKhauSanPhamChanNuois.Add(data);
        await context.SaveChangesAsync();
        return Ok(ApiResponse<CongBoXuatKhauSanPhamChanNuoi>.Ok(data));
    }

    [HttpGet("xuatkhausanphamchannuoi/{id:int}")]
    public async Task<IActionResult> GetByIdCongBoXuatKhauSanPhamChanNuoi(int id)
    {
        var result = await context.CongBoXuatKhauSanPhamChanNuois
            .FindAsync(id);
        if (result == null)
            return NotFound(ApiResponse<CongBoXuatKhauSanPhamChanNuoi>.Fail(["Không tìm thấy dữ liệu."]));

        return Ok(ApiResponse<CongBoXuatKhauSanPhamChanNuoi>.Ok(result));
    }

    [HttpGet("xuatkhausanphamchannuoi/changestatus/{id:int}/{iscongbo:int}")]
    public async Task<IActionResult> ChangeStatusCongBoXuatKhauSanPhamChanNuoi(int id, int iscongbo)
    {
        var result = await context.CongBoXuatKhauSanPhamChanNuois
            .FindAsync(id);
        if (result == null)
            return NotFound(ApiResponse<bool>.Fail(["Không tìm thấy dữ liệu."]));
        if (iscongbo != 0 && iscongbo != 1)
            return BadRequest(ApiResponse<bool>.Fail(["Trạng thái công bố không hợp lệ."]));
        if (result.IsCongBo == iscongbo)
            return BadRequest(ApiResponse<bool>.Fail(["Không có thay đổi về trạng thái công bố"]));
        result.IsCongBo = iscongbo;
        context.CongBoXuatKhauSanPhamChanNuois.Update(result);
        await context.SaveChangesAsync();

        return Ok(ApiResponse<bool>.Ok(true, "Cập nhật trạng thái thành công."));
    }

    //Xuất khẩu Lâm sản
    [HttpPost("xuatkhaulamsan/sanphamgo/get-data-lists")]
    public async Task<IActionResult> GetPagedDataCongBoXuatKhauLamSan_SanPhamGo(CongBoXuatKhauQueryDto data)
    {
        var query = context.CongBoXuatKhauLamSans.Where(x => x.LoaiLamSan == "Gỗ và sản phẩm gỗ").AsQueryable();

        if (data.CreatedFrom.HasValue)
        {
            var utcCreatedFrom = data.CreatedFrom.Value.ToUtc();
            query = query.Where(t => t.CreatedAt >= utcCreatedFrom);
        }

        if (data.CreatedTo.HasValue)
        {
            var utcCreatedTo = data.CreatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.CreatedAt < utcCreatedTo);
        }

        if (data.UpdatedFrom.HasValue)
        {
            var utcUpdatedFrom = data.UpdatedFrom.Value.ToUtc();
            query = query.Where(t => t.UpdatedAt >= utcUpdatedFrom);
        }

        if (data.UpdatedTo.HasValue)
        {
            var utcUpdatedTo = data.UpdatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.UpdatedAt < utcUpdatedTo);
        }


        if (!string.IsNullOrEmpty(data.CreatedBy))
        {
            var createdBy = data.CreatedBy.ToLower();
            query = query.Where(t => (t.CreatedBy ?? "").ToLower().Contains(createdBy));
        }

        if (data.Nams != null && data.Nams.Count > 0) query = query.Where(t => data.Nams.Contains(t.Nam));
        if (data.Thangs != null && data.Thangs.Count > 0) query = query.Where(t => data.Thangs.Contains(t.Thang));
        if (data.MaQuocGiaVungLTs != null && data.MaQuocGiaVungLTs.Count > 0)
            query = query.Where(t => data.MaQuocGiaVungLTs.Contains(t.MaQuocGiaVungLT));
        if (data.IsCongBo.HasValue) query = query.Where(t => t.IsCongBo == data.IsCongBo.Value);

        if (!string.IsNullOrEmpty(data.Keyword))
        {
            var keyword = data.Keyword.RemoveSpecialCharactersUnicode().ToLower();
            query = query.Where(t =>
                t.Nam.ToString().ToLower().Contains(keyword) ||
                t.Thang.ToString().ToLower().Contains(keyword));
        }

        var TongGiaTri = query.Sum(t => t.GiaTri);
        var TongGiaTriUocTinh = query.Sum(t => t.GiaTriUocTinh);

        var result = await query.ToPagedResultAsync(
            data.PageNumber,
            data.PageSize,
            data.OrderBy,
            data.Ascending
        );

        var response = new CongBoXuatKhauLamSanPagedResponse
        {
            Data = result,
            TongGiaTri = TongGiaTri,
            TongGiaTriUocTinh = TongGiaTriUocTinh
        };

        return Ok(ApiResponse<CongBoXuatKhauLamSanPagedResponse>.Ok(response));
    }

    [HttpPost("xuatkhaulamsan/sanphammaytrecoi/get-data-lists")]
    public async Task<IActionResult> GetPagedDataCongBoXuatKhauLamSan_SanPhamMayTreCoi(CongBoXuatKhauQueryDto data)
    {
        var query = context.CongBoXuatKhauLamSans.Where(x => x.LoaiLamSan == "Sản phẩm mây, tre, cói và thảm").AsQueryable();

        if (data.CreatedFrom.HasValue)
        {
            var utcCreatedFrom = data.CreatedFrom.Value.ToUtc();
            query = query.Where(t => t.CreatedAt >= utcCreatedFrom);
        }

        if (data.CreatedTo.HasValue)
        {
            var utcCreatedTo = data.CreatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.CreatedAt < utcCreatedTo);
        }

        if (data.UpdatedFrom.HasValue)
        {
            var utcUpdatedFrom = data.UpdatedFrom.Value.ToUtc();
            query = query.Where(t => t.UpdatedAt >= utcUpdatedFrom);
        }

        if (data.UpdatedTo.HasValue)
        {
            var utcUpdatedTo = data.UpdatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.UpdatedAt < utcUpdatedTo);
        }


        if (!string.IsNullOrEmpty(data.CreatedBy))
        {
            var createdBy = data.CreatedBy.ToLower();
            query = query.Where(t => (t.CreatedBy ?? "").ToLower().Contains(createdBy));
        }

        if (data.Nams != null && data.Nams.Count > 0) query = query.Where(t => data.Nams.Contains(t.Nam));
        if (data.Thangs != null && data.Thangs.Count > 0) query = query.Where(t => data.Thangs.Contains(t.Thang));
        if (data.MaQuocGiaVungLTs != null && data.MaQuocGiaVungLTs.Count > 0)
            query = query.Where(t => data.MaQuocGiaVungLTs.Contains(t.MaQuocGiaVungLT));
        if (data.IsCongBo.HasValue) query = query.Where(t => t.IsCongBo == data.IsCongBo.Value);

        if (!string.IsNullOrEmpty(data.Keyword))
        {
            var keyword = data.Keyword.RemoveSpecialCharactersUnicode().ToLower();
            query = query.Where(t =>
                t.Nam.ToString().ToLower().Contains(keyword) ||
                t.Thang.ToString().ToLower().Contains(keyword));
        }

        var TongGiaTri = query.Sum(t => t.GiaTri);
        var TongGiaTriUocTinh = query.Sum(t => t.GiaTriUocTinh);

        var result = await query.ToPagedResultAsync(
            data.PageNumber,
            data.PageSize,
            data.OrderBy,
            data.Ascending
        );

        var response = new CongBoXuatKhauLamSanPagedResponse
        {
            Data = result,
            TongGiaTri = TongGiaTri,
            TongGiaTriUocTinh = TongGiaTriUocTinh
        };

        return Ok(ApiResponse<CongBoXuatKhauLamSanPagedResponse>.Ok(response));
    }

    [HttpPost("xuatkhaulamsan/delete/{id:int}")]
    public async Task<IActionResult> DeleteCongBoXuatKhauLamSan(int id)
    {
        var existingData = await context.CongBoXuatKhauLamSans.FindAsync(id);
        if (existingData == null)
            return BadRequest(ApiResponse<bool>.Fail(new List<string> { "Không tìm thấy dữ liệu cần xóa." }));
        if (existingData.IsCongBo == 1)
            return BadRequest(ApiResponse<bool>.Fail(new List<string> { "Không thể xóa dữ liệu đã được công bố." }));
        context.CongBoXuatKhauLamSans.Remove(existingData);
        await context.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Ok(true, "Xóa dữ liệu thành công."));
    }

    [HttpPost("xuatkhaulamsan/upsert")]
    public async Task<IActionResult> UpsertCongBoXuatKhauLamSan([FromBody] CongBoXuatKhauLamSan data)
    {
        if (data.Id > 0)
        {
            if (data.IsCongBo == 1)
                return BadRequest(
                    ApiResponse<CongBoXuatKhauLamSan>.Fail(["Không thể cập dữ liệu đã được công bố."]));
            var existingData = await context.CongBoXuatKhauLamSans.FindAsync(data.Id);
            if (existingData == null)
                return BadRequest(
                    ApiResponse<CongBoXuatKhauLamSan>.Fail(["Không tìm thấy dữ liệu cần cập nhật."]));
            existingData.Assign(data);
            await context.SaveChangesAsync();
            return Ok(ApiResponse<CongBoXuatKhauLamSan>.Ok(existingData));
        }

        context.CongBoXuatKhauLamSans.Add(data);
        await context.SaveChangesAsync();
        return Ok(ApiResponse<CongBoXuatKhauLamSan>.Ok(data));
    }

    [HttpGet("xuatkhaulamsan/{id:int}")]
    public async Task<IActionResult> GetByIdCongBoXuatKhauLamSan(int id)
    {
        var result = await context.CongBoXuatKhauLamSans
            .FindAsync(id);
        if (result == null)
            return NotFound(ApiResponse<CongBoXuatKhauLamSan>.Fail(["Không tìm thấy dữ liệu."]));

        return Ok(ApiResponse<CongBoXuatKhauLamSan>.Ok(result));
    }

    [HttpGet("xuatkhaulamsan/changestatus/{id:int}/{iscongbo:int}")]
    public async Task<IActionResult> ChangeStatusCongBoXuatKhauLamSan(int id, int iscongbo)
    {
        var result = await context.CongBoXuatKhauLamSans
            .FindAsync(id);
        if (result == null)
            return NotFound(ApiResponse<bool>.Fail(["Không tìm thấy dữ liệu."]));
        if (iscongbo != 0 && iscongbo != 1)
            return BadRequest(ApiResponse<bool>.Fail(["Trạng thái công bố không hợp lệ."]));
        if (result.IsCongBo == iscongbo)
            return BadRequest(ApiResponse<bool>.Fail(["Không có thay đổi về trạng thái công bố"]));
        result.IsCongBo = iscongbo;
        context.CongBoXuatKhauLamSans.Update(result);
        await context.SaveChangesAsync();

        return Ok(ApiResponse<bool>.Ok(true, "Cập nhật trạng thái thành công."));
    }

    //Xuất khẩu Đầu vào sản xuất
    [HttpPost("dauvaosanxuat/get-data-lists")]
    public async Task<IActionResult> GetPagedDataCongBoXuatKhauDauVaoSanXuat(CongBoXuatKhauQueryDto data)
    {
        var query = context.CongBoXuatKhauDauVaoSanXuats.AsQueryable();

        if (data.CreatedFrom.HasValue)
        {
            var utcCreatedFrom = data.CreatedFrom.Value.ToUtc();
            query = query.Where(t => t.CreatedAt >= utcCreatedFrom);
        }

        if (data.CreatedTo.HasValue)
        {
            var utcCreatedTo = data.CreatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.CreatedAt < utcCreatedTo);
        }

        if (data.UpdatedFrom.HasValue)
        {
            var utcUpdatedFrom = data.UpdatedFrom.Value.ToUtc();
            query = query.Where(t => t.UpdatedAt >= utcUpdatedFrom);
        }

        if (data.UpdatedTo.HasValue)
        {
            var utcUpdatedTo = data.UpdatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.UpdatedAt < utcUpdatedTo);
        }


        if (!string.IsNullOrEmpty(data.CreatedBy))
        {
            var createdBy = data.CreatedBy.ToLower();
            query = query.Where(t => (t.CreatedBy ?? "").ToLower().Contains(createdBy));
        }

        if (data.Nams != null && data.Nams.Count > 0) query = query.Where(t => data.Nams.Contains(t.Nam));
        if (data.Thangs != null && data.Thangs.Count > 0) query = query.Where(t => data.Thangs.Contains(t.Thang));
        if (data.IsCongBo.HasValue) query = query.Where(t => t.IsCongBo == data.IsCongBo.Value);

        if (!string.IsNullOrEmpty(data.Keyword))
        {
            var keyword = data.Keyword.RemoveSpecialCharactersUnicode().ToLower();
            query = query.Where(t =>
                t.Nam.ToString().ToLower().Contains(keyword) ||
                t.Thang.ToString().ToLower().Contains(keyword));
        }

        var TongGiaTri = query.Sum(t => t.GiaTri);
        var TongGiaTriUocTinh = query.Sum(t => t.GiaTriUocTinh);

        var result = await query.ToPagedResultAsync(
            data.PageNumber,
            data.PageSize,
            data.OrderBy,
            data.Ascending
        );

        var response = new CongBoXuatKhauDauVaoSanXuatPagedResponse
        {
            Data = result,
            TongGiaTri = TongGiaTri,
            TongGiaTriUocTinh = TongGiaTriUocTinh
        };

        return Ok(ApiResponse<CongBoXuatKhauDauVaoSanXuatPagedResponse>.Ok(response));
    }

    [HttpPost("dauvaosanxuat/delete/{id:int}")]
    public async Task<IActionResult> DeleteCongBoXuatKhauDauVaoSanXuat(int id)
    {
        var existingData = await context.CongBoXuatKhauDauVaoSanXuats.FindAsync(id);
        if (existingData == null)
            return BadRequest(ApiResponse<bool>.Fail(new List<string> { "Không tìm thấy dữ liệu cần xóa." }));
        if (existingData.IsCongBo == 1)
            return BadRequest(ApiResponse<bool>.Fail(new List<string> { "Không thể xóa dữ liệu đã được công bố." }));
        context.CongBoXuatKhauDauVaoSanXuats.Remove(existingData);
        await context.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Ok(true, "Xóa dữ liệu thành công."));
    }

    [HttpPost("dauvaosanxuat/upsert")]
    public async Task<IActionResult> UpsertCongBoXuatKhauDauVaoSanXuat([FromBody] CongBoXuatKhauDauVaoSanXuat data)
    {
        if (data.Id > 0)
        {
            if (data.IsCongBo == 1)
                return BadRequest(
                    ApiResponse<CongBoXuatKhauDauVaoSanXuat>.Fail(["Không thể cập dữ liệu đã được công bố."]));
            var existingData = await context.CongBoXuatKhauDauVaoSanXuats.FindAsync(data.Id);
            if (existingData == null)
                return BadRequest(
                    ApiResponse<CongBoXuatKhauDauVaoSanXuat>.Fail(["Không tìm thấy dữ liệu cần cập nhật."]));
            existingData.Assign(data);
            await context.SaveChangesAsync();
            return Ok(ApiResponse<CongBoXuatKhauDauVaoSanXuat>.Ok(existingData));
        }

        context.CongBoXuatKhauDauVaoSanXuats.Add(data);
        await context.SaveChangesAsync();
        return Ok(ApiResponse<CongBoXuatKhauDauVaoSanXuat>.Ok(data));
    }

    [HttpGet("dauvaosanxuat/{id:int}")]
    public async Task<IActionResult> GetByIdCongBoXuatKhauDauVaoSanXuat(int id)
    {
        var result = await context.CongBoXuatKhauDauVaoSanXuats
            .FindAsync(id);
        if (result == null)
            return NotFound(ApiResponse<CongBoXuatKhauDauVaoSanXuat>.Fail(["Không tìm thấy dữ liệu."]));

        return Ok(ApiResponse<CongBoXuatKhauDauVaoSanXuat>.Ok(result));
    }

    [HttpGet("dauvaosanxuat/changestatus/{id:int}/{iscongbo:int}")]
    public async Task<IActionResult> ChangeStatusCongBoXuatKhauDauVaoSanXuat(int id, int iscongbo)
    {
        var result = await context.CongBoXuatKhauDauVaoSanXuats
            .FindAsync(id);
        if (result == null)
            return NotFound(ApiResponse<bool>.Fail(["Không tìm thấy dữ liệu."]));
        if (iscongbo != 0 && iscongbo != 1)
            return BadRequest(ApiResponse<bool>.Fail(["Trạng thái công bố không hợp lệ."]));
        if (result.IsCongBo == iscongbo)
            return BadRequest(ApiResponse<bool>.Fail(["Không có thay đổi về trạng thái công bố"]));
        result.IsCongBo = iscongbo;
        context.CongBoXuatKhauDauVaoSanXuats.Update(result);
        await context.SaveChangesAsync();

        return Ok(ApiResponse<bool>.Ok(true, "Cập nhật trạng thái thành công."));
    }

    //Xuất khẩu Muối
    [HttpPost("xuatkhaumuoi/get-data-lists")]
    public async Task<IActionResult> GetPagedDataCongBoXuatKhauMuoi(CongBoXuatKhauQueryDto data)
    {
        var query = context.CongBoXuatKhauMuois.AsQueryable();

        if (data.CreatedFrom.HasValue)
        {
            var utcCreatedFrom = data.CreatedFrom.Value.ToUtc();
            query = query.Where(t => t.CreatedAt >= utcCreatedFrom);
        }

        if (data.CreatedTo.HasValue)
        {
            var utcCreatedTo = data.CreatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.CreatedAt < utcCreatedTo);
        }

        if (data.UpdatedFrom.HasValue)
        {
            var utcUpdatedFrom = data.UpdatedFrom.Value.ToUtc();
            query = query.Where(t => t.UpdatedAt >= utcUpdatedFrom);
        }

        if (data.UpdatedTo.HasValue)
        {
            var utcUpdatedTo = data.UpdatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.UpdatedAt < utcUpdatedTo);
        }


        if (!string.IsNullOrEmpty(data.CreatedBy))
        {
            var createdBy = data.CreatedBy.ToLower();
            query = query.Where(t => (t.CreatedBy ?? "").ToLower().Contains(createdBy));
        }

        if (data.Nams != null && data.Nams.Count > 0) query = query.Where(t => data.Nams.Contains(t.Nam));
        if (data.Thangs != null && data.Thangs.Count > 0) query = query.Where(t => data.Thangs.Contains(t.Thang));
        if (data.MaQuocGiaVungLTs != null && data.MaQuocGiaVungLTs.Count > 0)
            query = query.Where(t => data.MaQuocGiaVungLTs.Contains(t.MaQuocGiaVungLT));
        if (data.IsCongBo.HasValue) query = query.Where(t => t.IsCongBo == data.IsCongBo.Value);

        if (!string.IsNullOrEmpty(data.Keyword))
        {
            var keyword = data.Keyword.RemoveSpecialCharactersUnicode().ToLower();
            query = query.Where(t =>
                t.Nam.ToString().ToLower().Contains(keyword) ||
                t.Thang.ToString().ToLower().Contains(keyword));
        }

        var TongGiaTri = query.Sum(t => t.GiaTri);
        var TongGiaTriUocTinh = query.Sum(t => t.GiaTriUocTinh);

        var result = await query.ToPagedResultAsync(
            data.PageNumber,
            data.PageSize,
            data.OrderBy,
            data.Ascending
        );

        var response = new CongBoXuatKhauMuoiPagedResponse
        {
            Data = result,
            TongGiaTri = TongGiaTri,
            TongGiaTriUocTinh = TongGiaTriUocTinh
        };

        return Ok(ApiResponse<CongBoXuatKhauMuoiPagedResponse>.Ok(response));
    }

    [HttpPost("xuatkhaumuoi/delete/{id:int}")]
    public async Task<IActionResult> DeleteCongBoXuatKhauMuoi(int id)
    {
        var existingData = await context.CongBoXuatKhauMuois.FindAsync(id);
        if (existingData == null)
            return BadRequest(ApiResponse<bool>.Fail(new List<string> { "Không tìm thấy dữ liệu cần xóa." }));
        if (existingData.IsCongBo == 1)
            return BadRequest(ApiResponse<bool>.Fail(new List<string> { "Không thể xóa dữ liệu đã được công bố." }));
        context.CongBoXuatKhauMuois.Remove(existingData);
        await context.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Ok(true, "Xóa dữ liệu thành công."));
    }

    [HttpPost("xuatkhaumuoi/upsert")]
    public async Task<IActionResult> UpsertCongBoXuatKhauMuoi([FromBody] CongBoXuatKhauMuoi data)
    {
        if (data.Id > 0)
        {
            if (data.IsCongBo == 1)
                return BadRequest(
                    ApiResponse<CongBoXuatKhauMuoi>.Fail(["Không thể cập dữ liệu đã được công bố."]));
            var existingData = await context.CongBoXuatKhauMuois.FindAsync(data.Id);
            if (existingData == null)
                return BadRequest(
                    ApiResponse<CongBoXuatKhauMuoi>.Fail(["Không tìm thấy dữ liệu cần cập nhật."]));
            existingData.Assign(data);
            await context.SaveChangesAsync();
            return Ok(ApiResponse<CongBoXuatKhauMuoi>.Ok(existingData));
        }

        context.CongBoXuatKhauMuois.Add(data);
        await context.SaveChangesAsync();
        return Ok(ApiResponse<CongBoXuatKhauMuoi>.Ok(data));
    }

    [HttpGet("xuatkhaumuoi/{id:int}")]
    public async Task<IActionResult> GetByIdCongBoXuatKhauMuoi(int id)
    {
        var result = await context.CongBoXuatKhauMuois
            .FindAsync(id);
        if (result == null)
            return NotFound(ApiResponse<CongBoXuatKhauMuoi>.Fail(["Không tìm thấy dữ liệu."]));

        return Ok(ApiResponse<CongBoXuatKhauMuoi>.Ok(result));
    }

    [HttpGet("xuatkhaumuoi/changestatus/{id:int}/{iscongbo:int}")]
    public async Task<IActionResult> ChangeStatusCongBoXuatKhauMuoi(int id, int iscongbo)
    {
        var result = await context.CongBoXuatKhauMuois
            .FindAsync(id);
        if (result == null)
            return NotFound(ApiResponse<bool>.Fail(["Không tìm thấy dữ liệu."]));
        if (iscongbo != 0 && iscongbo != 1)
            return BadRequest(ApiResponse<bool>.Fail(["Trạng thái công bố không hợp lệ."]));
        if (result.IsCongBo == iscongbo)
            return BadRequest(ApiResponse<bool>.Fail(["Không có thay đổi về trạng thái công bố"]));
        result.IsCongBo = iscongbo;
        context.CongBoXuatKhauMuois.Update(result);
        await context.SaveChangesAsync();

        return Ok(ApiResponse<bool>.Ok(true, "Cập nhật trạng thái thành công."));
    }

    //Nhập khẩu
    [HttpPost("nhapkhau/nongsan/get-data-lists")]
    public async Task<IActionResult> GetPagedDataCongBoNhapKhau_NongSan(CongBoXuatKhauQueryDto data)
    {
        var query = context.CongBoNhapKhaus.Where(x => x.LoaiSanPham == "Nông sản").AsQueryable();

        if (data.CreatedFrom.HasValue)
        {
            var utcCreatedFrom = data.CreatedFrom.Value.ToUtc();
            query = query.Where(t => t.CreatedAt >= utcCreatedFrom);
        }

        if (data.CreatedTo.HasValue)
        {
            var utcCreatedTo = data.CreatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.CreatedAt < utcCreatedTo);
        }

        if (data.UpdatedFrom.HasValue)
        {
            var utcUpdatedFrom = data.UpdatedFrom.Value.ToUtc();
            query = query.Where(t => t.UpdatedAt >= utcUpdatedFrom);
        }

        if (data.UpdatedTo.HasValue)
        {
            var utcUpdatedTo = data.UpdatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.UpdatedAt < utcUpdatedTo);
        }


        if (!string.IsNullOrEmpty(data.CreatedBy))
        {
            var createdBy = data.CreatedBy.ToLower();
            query = query.Where(t => (t.CreatedBy ?? "").ToLower().Contains(createdBy));
        }

        if (data.Nams != null && data.Nams.Count > 0) query = query.Where(t => data.Nams.Contains(t.Nam));
        if (data.Thangs != null && data.Thangs.Count > 0) query = query.Where(t => data.Thangs.Contains(t.Thang));
        if (data.MaQuocGiaVungLTs != null && data.MaQuocGiaVungLTs.Count > 0)
            query = query.Where(t => data.MaQuocGiaVungLTs.Contains(t.MaQuocGiaVungLT));
        if (data.IsCongBo.HasValue) query = query.Where(t => t.IsCongBo == data.IsCongBo.Value);

        if (!string.IsNullOrEmpty(data.Keyword))
        {
            var keyword = data.Keyword.RemoveSpecialCharactersUnicode().ToLower();
            query = query.Where(t =>
                t.Nam.ToString().ToLower().Contains(keyword) ||
                t.Thang.ToString().ToLower().Contains(keyword));
        }

        var TongGiaTri = query.Sum(t => t.GiaTri);
        var TongGiaTriUocTinh = query.Sum(t => t.GiaTriUocTinh);

        var result = await query.ToPagedResultAsync(
            data.PageNumber,
            data.PageSize,
            data.OrderBy,
            data.Ascending
        );

        var response = new CongBoNhapKhauPagedResponse
        {
            Data = result,
            TongGiaTri = TongGiaTri,
            TongGiaTriUocTinh = TongGiaTriUocTinh
        };

        return Ok(ApiResponse<CongBoNhapKhauPagedResponse>.Ok(response));
    }

    [HttpPost("nhapkhau/sanphamchannuoi/get-data-lists")]
    public async Task<IActionResult> GetPagedDataCongBoNhapKhau_SanPhamChanNuoi(CongBoXuatKhauQueryDto data)
    {
        var query = context.CongBoNhapKhaus.Where(x => x.LoaiSanPham == "Sản phẩm chăn nuôi").AsQueryable();

        if (data.CreatedFrom.HasValue)
        {
            var utcCreatedFrom = data.CreatedFrom.Value.ToUtc();
            query = query.Where(t => t.CreatedAt >= utcCreatedFrom);
        }

        if (data.CreatedTo.HasValue)
        {
            var utcCreatedTo = data.CreatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.CreatedAt < utcCreatedTo);
        }

        if (data.UpdatedFrom.HasValue)
        {
            var utcUpdatedFrom = data.UpdatedFrom.Value.ToUtc();
            query = query.Where(t => t.UpdatedAt >= utcUpdatedFrom);
        }

        if (data.UpdatedTo.HasValue)
        {
            var utcUpdatedTo = data.UpdatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.UpdatedAt < utcUpdatedTo);
        }


        if (!string.IsNullOrEmpty(data.CreatedBy))
        {
            var createdBy = data.CreatedBy.ToLower();
            query = query.Where(t => (t.CreatedBy ?? "").ToLower().Contains(createdBy));
        }

        if (data.Nams != null && data.Nams.Count > 0) query = query.Where(t => data.Nams.Contains(t.Nam));
        if (data.Thangs != null && data.Thangs.Count > 0) query = query.Where(t => data.Thangs.Contains(t.Thang));
        if (data.MaQuocGiaVungLTs != null && data.MaQuocGiaVungLTs.Count > 0)
            query = query.Where(t => data.MaQuocGiaVungLTs.Contains(t.MaQuocGiaVungLT));
        if (data.IsCongBo.HasValue) query = query.Where(t => t.IsCongBo == data.IsCongBo.Value);

        if (!string.IsNullOrEmpty(data.Keyword))
        {
            var keyword = data.Keyword.RemoveSpecialCharactersUnicode().ToLower();
            query = query.Where(t =>
                t.Nam.ToString().ToLower().Contains(keyword) ||
                t.Thang.ToString().ToLower().Contains(keyword));
        }

        var TongGiaTri = query.Sum(t => t.GiaTri);
        var TongGiaTriUocTinh = query.Sum(t => t.GiaTriUocTinh);

        var result = await query.ToPagedResultAsync(
            data.PageNumber,
            data.PageSize,
            data.OrderBy,
            data.Ascending
        );

        var response = new CongBoNhapKhauPagedResponse
        {
            Data = result,
            TongGiaTri = TongGiaTri,
            TongGiaTriUocTinh = TongGiaTriUocTinh
        };

        return Ok(ApiResponse<CongBoNhapKhauPagedResponse>.Ok(response));
    }

    [HttpPost("nhapkhau/thuysan/get-data-lists")]
    public async Task<IActionResult> GetPagedDataCongBoNhapKhau_ThuySan(CongBoXuatKhauQueryDto data)
    {
        var query = context.CongBoNhapKhaus.Where(x => x.LoaiSanPham == "Thủy sản").AsQueryable();

        if (data.CreatedFrom.HasValue)
        {
            var utcCreatedFrom = data.CreatedFrom.Value.ToUtc();
            query = query.Where(t => t.CreatedAt >= utcCreatedFrom);
        }

        if (data.CreatedTo.HasValue)
        {
            var utcCreatedTo = data.CreatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.CreatedAt < utcCreatedTo);
        }

        if (data.UpdatedFrom.HasValue)
        {
            var utcUpdatedFrom = data.UpdatedFrom.Value.ToUtc();
            query = query.Where(t => t.UpdatedAt >= utcUpdatedFrom);
        }

        if (data.UpdatedTo.HasValue)
        {
            var utcUpdatedTo = data.UpdatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.UpdatedAt < utcUpdatedTo);
        }


        if (!string.IsNullOrEmpty(data.CreatedBy))
        {
            var createdBy = data.CreatedBy.ToLower();
            query = query.Where(t => (t.CreatedBy ?? "").ToLower().Contains(createdBy));
        }

        if (data.Nams != null && data.Nams.Count > 0) query = query.Where(t => data.Nams.Contains(t.Nam));
        if (data.Thangs != null && data.Thangs.Count > 0) query = query.Where(t => data.Thangs.Contains(t.Thang));
        if (data.MaQuocGiaVungLTs != null && data.MaQuocGiaVungLTs.Count > 0)
            query = query.Where(t => data.MaQuocGiaVungLTs.Contains(t.MaQuocGiaVungLT));
        if (data.IsCongBo.HasValue) query = query.Where(t => t.IsCongBo == data.IsCongBo.Value);

        if (!string.IsNullOrEmpty(data.Keyword))
        {
            var keyword = data.Keyword.RemoveSpecialCharactersUnicode().ToLower();
            query = query.Where(t =>
                t.Nam.ToString().ToLower().Contains(keyword) ||
                t.Thang.ToString().ToLower().Contains(keyword));
        }

        var TongGiaTri = query.Sum(t => t.GiaTri);
        var TongGiaTriUocTinh = query.Sum(t => t.GiaTriUocTinh);

        var result = await query.ToPagedResultAsync(
            data.PageNumber,
            data.PageSize,
            data.OrderBy,
            data.Ascending
        );

        var response = new CongBoNhapKhauPagedResponse
        {
            Data = result,
            TongGiaTri = TongGiaTri,
            TongGiaTriUocTinh = TongGiaTriUocTinh
        };

        return Ok(ApiResponse<CongBoNhapKhauPagedResponse>.Ok(response));
    }

    [HttpPost("nhapkhau/lamsan/get-data-lists")]
    public async Task<IActionResult> GetPagedDataCongBoNhapKhau_LamSan(CongBoXuatKhauQueryDto data)
    {
        var query = context.CongBoNhapKhaus.Where(x => x.LoaiSanPham == "Lâm sản").AsQueryable();

        if (data.CreatedFrom.HasValue)
        {
            var utcCreatedFrom = data.CreatedFrom.Value.ToUtc();
            query = query.Where(t => t.CreatedAt >= utcCreatedFrom);
        }

        if (data.CreatedTo.HasValue)
        {
            var utcCreatedTo = data.CreatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.CreatedAt < utcCreatedTo);
        }

        if (data.UpdatedFrom.HasValue)
        {
            var utcUpdatedFrom = data.UpdatedFrom.Value.ToUtc();
            query = query.Where(t => t.UpdatedAt >= utcUpdatedFrom);
        }

        if (data.UpdatedTo.HasValue)
        {
            var utcUpdatedTo = data.UpdatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.UpdatedAt < utcUpdatedTo);
        }


        if (!string.IsNullOrEmpty(data.CreatedBy))
        {
            var createdBy = data.CreatedBy.ToLower();
            query = query.Where(t => (t.CreatedBy ?? "").ToLower().Contains(createdBy));
        }

        if (data.Nams != null && data.Nams.Count > 0) query = query.Where(t => data.Nams.Contains(t.Nam));
        if (data.Thangs != null && data.Thangs.Count > 0) query = query.Where(t => data.Thangs.Contains(t.Thang));
        if (data.MaQuocGiaVungLTs != null && data.MaQuocGiaVungLTs.Count > 0)
            query = query.Where(t => data.MaQuocGiaVungLTs.Contains(t.MaQuocGiaVungLT));
        if (data.IsCongBo.HasValue) query = query.Where(t => t.IsCongBo == data.IsCongBo.Value);

        if (!string.IsNullOrEmpty(data.Keyword))
        {
            var keyword = data.Keyword.RemoveSpecialCharactersUnicode().ToLower();
            query = query.Where(t =>
                t.Nam.ToString().ToLower().Contains(keyword) ||
                t.Thang.ToString().ToLower().Contains(keyword));
        }

        var TongGiaTri = query.Sum(t => t.GiaTri);
        var TongGiaTriUocTinh = query.Sum(t => t.GiaTriUocTinh);

        var result = await query.ToPagedResultAsync(
            data.PageNumber,
            data.PageSize,
            data.OrderBy,
            data.Ascending
        );

        var response = new CongBoNhapKhauPagedResponse
        {
            Data = result,
            TongGiaTri = TongGiaTri,
            TongGiaTriUocTinh = TongGiaTriUocTinh
        };

        return Ok(ApiResponse<CongBoNhapKhauPagedResponse>.Ok(response));
    }

    [HttpPost("nhapkhau/dauvaosanxuat/get-data-lists")]
    public async Task<IActionResult> GetPagedDataCongBoNhapKhau_DauVaoSanXuat(CongBoXuatKhauQueryDto data)
    {
        var query = context.CongBoNhapKhaus.Where(x => x.LoaiSanPham == "Đầu vào sản xuất").AsQueryable();

        if (data.CreatedFrom.HasValue)
        {
            var utcCreatedFrom = data.CreatedFrom.Value.ToUtc();
            query = query.Where(t => t.CreatedAt >= utcCreatedFrom);
        }

        if (data.CreatedTo.HasValue)
        {
            var utcCreatedTo = data.CreatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.CreatedAt < utcCreatedTo);
        }

        if (data.UpdatedFrom.HasValue)
        {
            var utcUpdatedFrom = data.UpdatedFrom.Value.ToUtc();
            query = query.Where(t => t.UpdatedAt >= utcUpdatedFrom);
        }

        if (data.UpdatedTo.HasValue)
        {
            var utcUpdatedTo = data.UpdatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.UpdatedAt < utcUpdatedTo);
        }


        if (!string.IsNullOrEmpty(data.CreatedBy))
        {
            var createdBy = data.CreatedBy.ToLower();
            query = query.Where(t => (t.CreatedBy ?? "").ToLower().Contains(createdBy));
        }

        if (data.Nams != null && data.Nams.Count > 0) query = query.Where(t => data.Nams.Contains(t.Nam));
        if (data.Thangs != null && data.Thangs.Count > 0) query = query.Where(t => data.Thangs.Contains(t.Thang));
        if (data.MaQuocGiaVungLTs != null && data.MaQuocGiaVungLTs.Count > 0)
            query = query.Where(t => data.MaQuocGiaVungLTs.Contains(t.MaQuocGiaVungLT));
        if (data.IsCongBo.HasValue) query = query.Where(t => t.IsCongBo == data.IsCongBo.Value);

        if (!string.IsNullOrEmpty(data.Keyword))
        {
            var keyword = data.Keyword.RemoveSpecialCharactersUnicode().ToLower();
            query = query.Where(t =>
                t.Nam.ToString().ToLower().Contains(keyword) ||
                t.Thang.ToString().ToLower().Contains(keyword));
        }

        var TongGiaTri = query.Sum(t => t.GiaTri);
        var TongGiaTriUocTinh = query.Sum(t => t.GiaTriUocTinh);

        var result = await query.ToPagedResultAsync(
            data.PageNumber,
            data.PageSize,
            data.OrderBy,
            data.Ascending
        );

        var response = new CongBoNhapKhauPagedResponse
        {
            Data = result,
            TongGiaTri = TongGiaTri,
            TongGiaTriUocTinh = TongGiaTriUocTinh
        };

        return Ok(ApiResponse<CongBoNhapKhauPagedResponse>.Ok(response));
    }

    [HttpPost("nhapkhau/muoi/get-data-lists")]
    public async Task<IActionResult> GetPagedDataCongBoNhapKhau_Muoi(CongBoXuatKhauQueryDto data)
    {
        var query = context.CongBoNhapKhaus.Where(x => x.LoaiSanPham == "Muối").AsQueryable();

        if (data.CreatedFrom.HasValue)
        {
            var utcCreatedFrom = data.CreatedFrom.Value.ToUtc();
            query = query.Where(t => t.CreatedAt >= utcCreatedFrom);
        }

        if (data.CreatedTo.HasValue)
        {
            var utcCreatedTo = data.CreatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.CreatedAt < utcCreatedTo);
        }

        if (data.UpdatedFrom.HasValue)
        {
            var utcUpdatedFrom = data.UpdatedFrom.Value.ToUtc();
            query = query.Where(t => t.UpdatedAt >= utcUpdatedFrom);
        }

        if (data.UpdatedTo.HasValue)
        {
            var utcUpdatedTo = data.UpdatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.UpdatedAt < utcUpdatedTo);
        }


        if (!string.IsNullOrEmpty(data.CreatedBy))
        {
            var createdBy = data.CreatedBy.ToLower();
            query = query.Where(t => (t.CreatedBy ?? "").ToLower().Contains(createdBy));
        }

        if (data.Nams != null && data.Nams.Count > 0) query = query.Where(t => data.Nams.Contains(t.Nam));
        if (data.Thangs != null && data.Thangs.Count > 0) query = query.Where(t => data.Thangs.Contains(t.Thang));
        if (data.MaQuocGiaVungLTs != null && data.MaQuocGiaVungLTs.Count > 0)
            query = query.Where(t => data.MaQuocGiaVungLTs.Contains(t.MaQuocGiaVungLT));
        if (data.IsCongBo.HasValue) query = query.Where(t => t.IsCongBo == data.IsCongBo.Value);

        if (!string.IsNullOrEmpty(data.Keyword))
        {
            var keyword = data.Keyword.RemoveSpecialCharactersUnicode().ToLower();
            query = query.Where(t =>
                t.Nam.ToString().ToLower().Contains(keyword) ||
                t.Thang.ToString().ToLower().Contains(keyword));
        }

        var TongGiaTri = query.Sum(t => t.GiaTri);
        var TongGiaTriUocTinh = query.Sum(t => t.GiaTriUocTinh);

        var result = await query.ToPagedResultAsync(
            data.PageNumber,
            data.PageSize,
            data.OrderBy,
            data.Ascending
        );

        var response = new CongBoNhapKhauPagedResponse
        {
            Data = result,
            TongGiaTri = TongGiaTri,
            TongGiaTriUocTinh = TongGiaTriUocTinh
        };

        return Ok(ApiResponse<CongBoNhapKhauPagedResponse>.Ok(response));
    }

    [HttpPost("nhapkhau/delete/{id:int}")]
    public async Task<IActionResult> DeleteCongBoNhapKhau(int id)
    {
        var existingData = await context.CongBoNhapKhaus.FindAsync(id);
        if (existingData == null)
            return BadRequest(ApiResponse<bool>.Fail(new List<string> { "Không tìm thấy dữ liệu cần xóa." }));
        if (existingData.IsCongBo == 1)
            return BadRequest(ApiResponse<bool>.Fail(new List<string> { "Không thể xóa dữ liệu đã được công bố." }));
        context.CongBoNhapKhaus.Remove(existingData);
        await context.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Ok(true, "Xóa dữ liệu thành công."));
    }

    [HttpPost("nhapkhau/upsert")]
    public async Task<IActionResult> UpsertCongBoNhapKhau([FromBody] CongBoNhapKhau data)
    {
        if (data.Id > 0)
        {
            if (data.IsCongBo == 1)
                return BadRequest(
                    ApiResponse<CongBoNhapKhau>.Fail(["Không thể cập dữ liệu đã được công bố."]));
            var existingData = await context.CongBoNhapKhaus.FindAsync(data.Id);
            if (existingData == null)
                return BadRequest(
                    ApiResponse<CongBoNhapKhau>.Fail(["Không tìm thấy dữ liệu cần cập nhật."]));
            existingData.Assign(data);
            await context.SaveChangesAsync();
            return Ok(ApiResponse<CongBoNhapKhau>.Ok(existingData));
        }

        context.CongBoNhapKhaus.Add(data);
        await context.SaveChangesAsync();
        return Ok(ApiResponse<CongBoNhapKhau>.Ok(data));
    }

    [HttpGet("nhapkhau/{id:int}")]
    public async Task<IActionResult> GetByIdCongBoNhapKhau(int id)
    {
        var result = await context.CongBoNhapKhaus
            .FindAsync(id);
        if (result == null)
            return NotFound(ApiResponse<CongBoNhapKhau>.Fail(["Không tìm thấy dữ liệu."]));

        return Ok(ApiResponse<CongBoNhapKhau>.Ok(result));
    }

    [HttpGet("nhapkhau/changestatus/{id:int}/{iscongbo:int}")]
    public async Task<IActionResult> ChangeStatusCongBoNhapKhau(int id, int iscongbo)
    {
        var result = await context.CongBoNhapKhaus
            .FindAsync(id);
        if (result == null)
            return NotFound(ApiResponse<bool>.Fail(["Không tìm thấy dữ liệu."]));
        if (iscongbo != 0 && iscongbo != 1)
            return BadRequest(ApiResponse<bool>.Fail(["Trạng thái công bố không hợp lệ."]));
        if (result.IsCongBo == iscongbo)
            return BadRequest(ApiResponse<bool>.Fail(["Không có thay đổi về trạng thái công bố"]));
        result.IsCongBo = iscongbo;
        context.CongBoNhapKhaus.Update(result);
        await context.SaveChangesAsync();

        return Ok(ApiResponse<bool>.Ok(true, "Cập nhật trạng thái thành công."));
    }

    //Xuất khẩu Đầu vào sản xuất
    [HttpPost("cancanthuongmai/get-data-lists")]
    public async Task<IActionResult> GetPagedDataCongBoCanCanThuongMai(CongBoXuatKhauQueryDto data)
    {
        var query = context.CongBoCanCanThuongMais.AsQueryable();

        if (data.CreatedFrom.HasValue)
        {
            var utcCreatedFrom = data.CreatedFrom.Value.ToUtc();
            query = query.Where(t => t.CreatedAt >= utcCreatedFrom);
        }

        if (data.CreatedTo.HasValue)
        {
            var utcCreatedTo = data.CreatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.CreatedAt < utcCreatedTo);
        }

        if (data.UpdatedFrom.HasValue)
        {
            var utcUpdatedFrom = data.UpdatedFrom.Value.ToUtc();
            query = query.Where(t => t.UpdatedAt >= utcUpdatedFrom);
        }

        if (data.UpdatedTo.HasValue)
        {
            var utcUpdatedTo = data.UpdatedTo.Value.ToUtc().AddDays(1);
            query = query.Where(t => t.UpdatedAt < utcUpdatedTo);
        }


        if (!string.IsNullOrEmpty(data.CreatedBy))
        {
            var createdBy = data.CreatedBy.ToLower();
            query = query.Where(t => (t.CreatedBy ?? "").ToLower().Contains(createdBy));
        }

        if (data.Nams != null && data.Nams.Count > 0) query = query.Where(t => data.Nams.Contains(t.Nam));
        if (data.Thangs != null && data.Thangs.Count > 0) query = query.Where(t => data.Thangs.Contains(t.Thang));
        if (data.IsCongBo.HasValue) query = query.Where(t => t.IsCongBo == data.IsCongBo.Value);

        if (!string.IsNullOrEmpty(data.Keyword))
        {
            var keyword = data.Keyword.RemoveSpecialCharactersUnicode().ToLower();
            query = query.Where(t =>
                t.Nam.ToString().ToLower().Contains(keyword) ||
                t.Thang.ToString().ToLower().Contains(keyword));
        }

        var TongGiaTri = query.Sum(t => t.GiaTri);
        var TongGiaTriUocTinh = query.Sum(t => t.GiaTriUocTinh);

        var result = await query.ToPagedResultAsync(
            data.PageNumber,
            data.PageSize,
            data.OrderBy,
            data.Ascending
        );

        var response = new CongBoCanCanThuongMaiPagedResponse
        {
            Data = result,
            TongGiaTri = TongGiaTri,
            TongGiaTriUocTinh = TongGiaTriUocTinh
        };

        return Ok(ApiResponse<CongBoCanCanThuongMaiPagedResponse>.Ok(response));
    }

    [HttpPost("cancanthuongmai/delete/{id:int}")]
    public async Task<IActionResult> DeleteCongBoCanCanThuongMai(int id)
    {
        var existingData = await context.CongBoCanCanThuongMais.FindAsync(id);
        if (existingData == null)
            return BadRequest(ApiResponse<bool>.Fail(new List<string> { "Không tìm thấy dữ liệu cần xóa." }));
        if (existingData.IsCongBo == 1)
            return BadRequest(ApiResponse<bool>.Fail(new List<string> { "Không thể xóa dữ liệu đã được công bố." }));
        context.CongBoCanCanThuongMais.Remove(existingData);
        await context.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Ok(true, "Xóa dữ liệu thành công."));
    }

    [HttpPost("cancanthuongmai/upsert")]
    public async Task<IActionResult> UpsertCongBoCanCanThuongMai([FromBody] CongBoCanCanThuongMai data)
    {
        if (data.Id > 0)
        {
            if (data.IsCongBo == 1)
                return BadRequest(
                    ApiResponse<CongBoCanCanThuongMai>.Fail(["Không thể cập dữ liệu đã được công bố."]));
            var existingData = await context.CongBoCanCanThuongMais.FindAsync(data.Id);
            if (existingData == null)
                return BadRequest(
                    ApiResponse<CongBoCanCanThuongMai>.Fail(["Không tìm thấy dữ liệu cần cập nhật."]));
            existingData.Assign(data);
            await context.SaveChangesAsync();
            return Ok(ApiResponse<CongBoCanCanThuongMai>.Ok(existingData));
        }

        context.CongBoCanCanThuongMais.Add(data);
        await context.SaveChangesAsync();
        return Ok(ApiResponse<CongBoCanCanThuongMai>.Ok(data));
    }

    [HttpGet("cancanthuongmai/{id:int}")]
    public async Task<IActionResult> GetByIdCongBoCanCanThuongMai(int id)
    {
        var result = await context.CongBoCanCanThuongMais
            .FindAsync(id);
        if (result == null)
            return NotFound(ApiResponse<CongBoCanCanThuongMai>.Fail(["Không tìm thấy dữ liệu."]));

        return Ok(ApiResponse<CongBoCanCanThuongMai>.Ok(result));
    }

    [HttpGet("cancanthuongmai/changestatus/{id:int}/{iscongbo:int}")]
    public async Task<IActionResult> ChangeStatusCongBoCanCanThuongMai(int id, int iscongbo)
    {
        var result = await context.CongBoCanCanThuongMais
            .FindAsync(id);
        if (result == null)
            return NotFound(ApiResponse<bool>.Fail(["Không tìm thấy dữ liệu."]));
        if (iscongbo != 0 && iscongbo != 1)
            return BadRequest(ApiResponse<bool>.Fail(["Trạng thái công bố không hợp lệ."]));
        if (result.IsCongBo == iscongbo)
            return BadRequest(ApiResponse<bool>.Fail(["Không có thay đổi về trạng thái công bố"]));
        result.IsCongBo = iscongbo;
        context.CongBoCanCanThuongMais.Update(result);
        await context.SaveChangesAsync();

        return Ok(ApiResponse<bool>.Ok(true, "Cập nhật trạng thái thành công."));
    }
}