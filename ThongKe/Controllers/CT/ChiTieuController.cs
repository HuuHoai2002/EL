using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThongKe.Data;
using ThongKe.DTOs;
using ThongKe.Entities;
using ThongKe.Shared;
using ThongKe.Shared.Enums;
using ThongKe.Shared.Extensions;
using ThongKe.Shared.Utils;

namespace ThongKe.Controllers.CT;

[Route("api/chitieus")]
[ApiController]
[Authorize]
public class ChiTieuController(
    AppDbContext context,
    IQueryService queryService,
    ITableService tableService,
    IHttpContextAccessor httpContextAccessor) : ControllerBase
{
    [HttpPost("upsert")]
    public async Task<IActionResult> Upsert([FromBody] ChiTieu data)
    {
        ChiTieu? chiTieu;
        var isUpdate = false;

        if (data.TrangThai == TrangThaiChiTieu.Published) data.TrangThai = TrangThaiChiTieu.NonPublished;

        foreach (var phanTo in data.PhanTos)
        {
            phanTo.TenPhanTo = phanTo.TenPhanTo.RemoveSpecialCharactersUnicode();
            var dataType = Constants.LoaiDuLieus.FirstOrDefault(dt => dt.Value == phanTo.DataType);
            if (dataType == null)
                return BadRequest(ApiResponse<ChiTieu>.Fail(
                    [$"Loại dữ liệu không hợp lệ cho phân tổ '{phanTo.TenPhanTo}'."]));
            phanTo.DataType = dataType.Value!;
            phanTo.OracleDbType = dataType.OracleDbType!;
            phanTo.ColumnName = $"phan_to_{phanTo.TenPhanTo.RemoveVietnameseAccents().ToSnakeCase()}_${dataType.Value}";
        }

        if (data.Id > 0)
        {
            chiTieu = await context.ChiTieus.FindAsync(data.Id);
            if (chiTieu == null)
                return BadRequest(ApiResponse<ChiTieu>.Fail(["Không tìm thấy chỉ tiêu cần cập nhật."]));
            if (chiTieu.TrangThai == TrangThaiChiTieu.Published)
                return BadRequest(ApiResponse<ChiTieu>.Fail(
                    ["Chỉ tiêu đã được công bố, không thể cập nhật. Vui lòng tạo mới chỉ tiêu khác."]));
            chiTieu.Assign(data);
            isUpdate = true;
        }
        else
        {
            chiTieu = new ChiTieu();
            chiTieu.Assign(data);
            context.ChiTieus.Add(chiTieu);
        }

        await context.SaveChangesAsync();

        // Upsert bảng chỉ tiêu
        await tableService.UpsertBangChiTieu(chiTieu, isUpdate);
        return Ok(ApiResponse<ChiTieu>.Ok(chiTieu));
    }

    [HttpPost("publish/{id:int}")]
    public async Task<IActionResult> Publish(int id)
    {
        var chiTieu = await context.ChiTieus.FindAsync(id);
        if (chiTieu == null)
            return BadRequest(ApiResponse<ChiTieu>.Fail(["Không tìm thấy chỉ tiêu cần công bố."]));
        if (chiTieu.TrangThai == TrangThaiChiTieu.Published)
            return BadRequest(ApiResponse<ChiTieu>.Fail(["Chỉ tiêu đã được công bố."]));
        chiTieu.TrangThai = TrangThaiChiTieu.Published;
        await context.SaveChangesAsync();
        return Ok(ApiResponse<ChiTieu>.Ok(chiTieu, "Công bố chỉ tiêu thành công."));
    }

    [HttpPost("add-variant")]
    public async Task<IActionResult> AddVariant(AddVariantDto data)
    {
        var chiTieu = await context.ChiTieus.FindAsync(data.ChiTieuId);
        if (chiTieu == null)
            return BadRequest(ApiResponse<ChiTieu>.Fail(["Không tìm thấy chỉ tiêu."]));

        var chiTieuVariant = new ChiTieu();
        chiTieuVariant.Assign(data.ChiTieuVariant);
        chiTieuVariant.ParentId = chiTieu.Id;
        chiTieuVariant.IsBienThe = Constants.OracleBoolean.TRUE;

        context.ChiTieus.Add(chiTieuVariant);
        await context.SaveChangesAsync();

        return Ok(ApiResponse<ChiTieu>.Ok(chiTieuVariant));
    }

    [HttpGet("get-list-variants/{id:int}")]
    public async Task<IActionResult> GetListVariants(int id)
    {
        var chiTieu = await context.ChiTieus.FindAsync(id);
        if (chiTieu == null)
            return BadRequest(ApiResponse<ChiTieu>.Fail(["Không tìm thấy chỉ tiêu."]));
        chiTieu.BienThes = context.ChiTieus.Where(c => c.ParentId == id).ToList();
        var bienThes = chiTieu.BienThes;
        return Ok(ApiResponse<List<ChiTieu>>.Ok(bienThes));
    }

    [HttpPost("get-all")]
    public async Task<IActionResult> GetPagedChiTieus([FromBody] ChiTieuQueryDto data)
    {
        var query = context.ChiTieus.AsQueryable();
        // var query = ApplyDataFilterNghiepVu(httpContextAccessor, context, getAll);

        if (!string.IsNullOrEmpty(data.Keyword))
        {
            var keyword = data.Keyword.RemoveSpecialCharactersUnicode().ToLower();
            query = query.Where(t =>
                t.TenChiTieu.ToLower().Contains(keyword) ||
                (t.MoTa ?? "").ToLower().Contains(keyword));
        }

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

        if (data.DonViId.HasValue) query = query.Where(t => t.DonViId == data.DonViId.Value);
        if (data.ParentId.HasValue) query = query.Where(t => t.ParentId == data.ParentId.Value);
        if (data.TrangThai.HasValue) query = query.Where(t => t.TrangThai == data.TrangThai.Value);

        var result = await query.ToPagedResultAsync(
            data.PageNumber,
            data.PageSize,
            data.OrderBy,
            data.Ascending
        );

        foreach (var chiTieu in result.Records) chiTieu.Deserialize();

        return Ok(ApiResponse<ChiTieu>.PagedResult(result));
    }

    [HttpPost("delete/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await context.ChiTieus.FindAsync(id);
        if (result == null)
        {
            var errorMessage = $"Chỉ tiêu với ID {id} không tồn tại.";
            return BadRequest(ApiResponse<bool>.Fail([errorMessage]));
        }

        if (result.TrangThai == TrangThaiChiTieu.Published)
        {
            var errorMessage = $"Chỉ tiêu với ID {id} đã được công bố, không thể xóa.";
            return BadRequest(ApiResponse<bool>.Fail([errorMessage]));
        }

        //var isuDelete = CanDeleteNghiepVu(httpContextAccessor, context, result, GiamSatUserReadUpSert.No);
        //if (!isuDelete)
        //    return StatusCode(StatusCodes.Status403Forbidden,
        //        ApiResponse<string>.Fail(["Bạn không có quyền truy cập."]));
        context.ChiTieus.Remove(result);
        await context.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Ok(true, $"Xóa thành công chỉ tiêu với ID {id}"));
    }

    [HttpGet("get-all-in-one")]
    public async Task<IActionResult> GetAllChiTieus([FromQuery] TrangThaiChiTieu? trangThai)
    {
        var query = context.ChiTieus.AsQueryable();

        if (trangThai.HasValue) query = query.Where(t => t.TrangThai == trangThai.Value);

        var results = await query.Select(t => new
            {
                t.Id,
                t.TenChiTieu,
                t.MoTa
            }
        ).OrderBy(x => x.TenChiTieu).ToListAsync();
        return Ok(ApiResponse<object>.Ok(results));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await context.ChiTieus.FindAsync(id);
        if (result == null) return BadRequest(ApiResponse<ChiTieu>.Fail(["Không tìm thấy chỉ tiêu."]));
        result.Deserialize();
        result.PhanTos.Sort((a, b) => a.Index.CompareTo(b.Index));

        return Ok(ApiResponse<ChiTieu>.Ok(result));
    }

    [HttpGet("{id:int}/get-data-statistics")]
    public async Task<IActionResult> GetDataStatisticsByChiTieuId(int id)
    {
        var result = await context.ChiTieus.FindAsync(id);
        if (result == null)
            return BadRequest(ApiResponse<ThongKeDuLieuChiTieuResponse>.Fail(["Không tìm thấy chỉ tiêu."]));
        result.Deserialize();

        var statistics = new ThongKeDuLieuChiTieuResponse
        {
            ChiTieu = result,
            Statistics = new List<ThongKeDuLieuChiTieu>(),
            TongSoLuongBanGhi = queryService.CountDataByChiTieuId(result.Id),
            TongSoLuongBanGhiPhanTo = 0
        };

        var Data = await context.DuLieuChiTieus
            .Where(td => td.ChiTieuId == result.Id)
            .ToListAsync();

        var propertyCache = new Dictionary<int, PropertyInfo?>();

        foreach (var phanTo in result.PhanTos)
        {
            if (!propertyCache.ContainsKey(phanTo.Index))
                propertyCache[phanTo.Index] = QueryUtils.GetPropertyInfo<DuLieuChiTieu>("PhanToId" + phanTo.Index);

            var property = propertyCache[phanTo.Index];
            if (property == null) continue;

            var thongKe = Data
                .Where(td => property.GetValue(td) != null)
                .GroupBy(td => (int?)property.GetValue(td) ?? 0)
                .Select(g => new ThongKeChiTieuItem
                {
                    PhanToId = g.Key,
                    TotalData = g.Sum(x => x.Data)
                })
                .ToList();

            statistics.TongSoLuongBanGhiPhanTo += Data.Where(td => property.GetValue(td) != null).Count();

            statistics.Statistics.Add(new ThongKeDuLieuChiTieu
            {
                PhanTo = phanTo,
                Items = thongKe
            });
        }

        statistics.ChiTieu.PhanTos.Sort((a, b) => a.Index.CompareTo(b.Index));
        statistics.Statistics.Sort((a, b) => a.PhanTo!.Index.CompareTo(b.PhanTo!.Index!));

        return Ok(ApiResponse<ThongKeDuLieuChiTieuResponse>.Ok(statistics));
    }

    [HttpGet("get-overall-statistics")]
    public async Task<IActionResult> GetOverallStatistics(int? limit = 10)
    {
        var query = context.ChiTieus.AsQueryable();
        //var query = ApplyDataFilterNghiepVu(httpContextAccessor, context, getAll);

        var totalChiTieus = await query.CountAsync();
        var chiTieus = await query.ToListAsync();

        chiTieus.ForEach(c => c.Deserialize());
        var combinations = chiTieus.SelectMany(t => t.PhanTos);
        var combinationCount = combinations.Count();
        var topChiTieus = await context.ChiTieus
            .Select(t => new ThongKeChiTieu
            {
                Id = t.Id,
                TenChiTieu = t.TenChiTieu,
                RecordCount = 0
            })
            .Take(limit ?? 10)
            .ToListAsync();

        foreach (var chiTieu in topChiTieus) chiTieu.RecordCount = queryService.CountDataByChiTieuId(chiTieu.Id);

        topChiTieus = topChiTieus.OrderByDescending(t => t.RecordCount).ToList();

        var response = new ThongKeDanhSachChiTieuResponse
        {
            SoLuongChiTieu = totalChiTieus,
            SoLuongPhanTo = combinationCount,
            TopChiTieus = topChiTieus
        };
        return Ok(ApiResponse<ThongKeDanhSachChiTieuResponse>.Ok(response));
    }

    [HttpPost("get-list-imports")]
    public async Task<IActionResult> GetListChiTieuDaNhapByChiTieuId(IdentityRequest data)
    {
        var query = context.ChiTieuDaNhaps.AsQueryable().Where(t => t.ChiTieuId == data.Id);
        //var query = ApplyDataFilterNghiepVu(httpContextAccessor, context, getAll);

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

        var result = await query.ToPagedResultAsync(
            data.PageNumber,
            data.PageSize,
            data.OrderBy,
            data.Ascending
        );

        return Ok(ApiResponse<ChiTieuDaNhap>.PagedResult(result));
    }

    [HttpGet("get-data-types")]
    public IActionResult GetAllFixedTypes()
    {
        return Ok(ApiResponse<object>.Ok(Constants.LoaiDuLieus));
    }
}