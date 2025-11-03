using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThongKe.Data;
using ThongKe.DTOs;
using ThongKe.Entities;
using ThongKe.Shared;
using ThongKe.Shared.Enums;
using ThongKe.Shared.Extensions;

namespace ThongKe.Controllers.CT;

[Route("api/bieumaus")]
[ApiController]
public class BieuMauController(
    AppDbContext context
) : ControllerBase
{
    [HttpGet("get-all-in-one")]
    public async Task<IActionResult> GetAll()
    {
        var results = await context.BieuMaus.OrderBy(x => x.TenBieuMau).ToListAsync();
        return Ok(ApiResponse<List<BieuMau>>.Ok(results));
    }

    [HttpPost("get-all")]
    public async Task<IActionResult> GetPagedBieuMaus([FromBody] BieuMauQueryDto data)
    {
        var query = context.BieuMaus.AsQueryable();

        if (!string.IsNullOrEmpty(data.Keyword))
        {
            var keyword = data.Keyword.RemoveSpecialCharactersUnicode().ToLower();
            query = query.Where(t =>
                t.TenBieuMau.ToLower().Contains(keyword) ||
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

        if (data.DonViId.HasValue) query = query.Where(x => x.DonViId == data.DonViId);
        if (data.Thang.HasValue) query = query.Where(x => x.Thang == data.Thang);
        if (data.Nam.HasValue) query = query.Where(x => x.Nam == data.Nam);
        if (data.SoThongTu.HasValue) query = query.Where(x => x.SoThongTu == data.SoThongTu);
        if (!string.IsNullOrEmpty(data.DonViBaoCao)) query = query.Where(x => x.DonViBaoCao == data.DonViBaoCao);
        if (!string.IsNullOrEmpty(data.DonViNhanBaoCao))
            query = query.Where(x => x.DonViNhanBaoCao == data.DonViNhanBaoCao);

        var result = await query.ToPagedResultAsync(
            data.PageNumber,
            data.PageSize,
            data.OrderBy,
            data.Ascending
        );

        return Ok(ApiResponse<BieuMau>.PagedResult(result));
    }

    [HttpPost("upsert")]
    public async Task<IActionResult> Upsert([FromBody] BieuMau bieuMau)
    {
        BieuMau? existingBieuMau;

        var chiTieus = context.ChiTieus
            .Where(x => bieuMau.ChiTieuIds.Contains(x.Id) && x.TrangThai == TrangThaiChiTieu.Published).ToList();

        if (chiTieus.Count != bieuMau.ChiTieuIds.Count)
            return BadRequest(
                ApiResponse<BieuMau>.Fail(["Một hoặc nhiều chỉ tiêu không tồn tại hoặc chưa được xuất bản."])
            );

        bieuMau.ChiTieus = chiTieus;
        bieuMau.ChiTieuIds = chiTieus.Select(x => x.Id).ToList();

        if (bieuMau.Id > 0)
        {
            existingBieuMau = await context.BieuMaus.FindAsync(bieuMau.Id);
            if (existingBieuMau == null) return BadRequest(ApiResponse<BieuMau>.Fail(["Biểu mẫu không tồn tại."]));
            existingBieuMau.Assign(bieuMau);
        }
        else
        {
            existingBieuMau = new BieuMau();
            existingBieuMau.Assign(bieuMau);
            context.BieuMaus.Add(existingBieuMau);
        }

        await context.SaveChangesAsync();
        return Ok(ApiResponse<BieuMau>.Ok(existingBieuMau));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var bieuMau = await context.BieuMaus.FindAsync(id);
        if (bieuMau == null) return NotFound(ApiResponse<BieuMau>.Fail(["Biểu mẫu không tồn tại."]));
        bieuMau.Deserialize();
        return Ok(ApiResponse<BieuMau>.Ok(bieuMau));
    }

    [HttpPost("delete/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var data = await context.BieuMaus.FindAsync(id);
        if (data == null)
        {
            var errorMessage = $"Biểu mẫu với ID {id} không tồn tại.";
            return BadRequest(ApiResponse<bool>.Fail([errorMessage]));
        }

        context.BieuMaus.Remove(data);
        await context.SaveChangesAsync();

        return Ok(ApiResponse<bool>.Ok(true, $"Xóa thành công biểu mẫu với ID {id}"));
    }

    // [HttpGet("{bieuMauId:int}/get-data-structure")]
    // public async Task<IActionResult> GetDataStructure(int bieuMauId)
    // {
    //     var bieuMau = await context.BieuMaus.FindAsync(bieuMauId);
    //
    //     if (bieuMau == null)
    //         return BadRequest(ApiResponse<string>.Fail(
    //             ["Không tìm thấy biểu mẫu."]
    //         ));
    //     bieuMau.Deserialize();
    //     var chiTieu = bieuMau.ChiTieus.First();
    //
    //     if (!chiTieu.HasTableInDatabase)
    //         return BadRequest(
    //             ApiResponse<string>.Fail(["Không thể lấy dữ liệu cho biểu mẫu này do không tồn tại bảng chỉ tiêu."]));
    //
    //     Dictionary<string, object?> structure = new();
    //     structure.TryAdd(chiTieu.DefineColumnName, null);
    //
    //     foreach (var phanTo in chiTieu.PhanTos) structure.TryAdd(phanTo.DefineColumnName, null);
    //
    //     return Ok(ApiResponse<Dictionary<string, object?>>.Ok(structure));
    // }
}