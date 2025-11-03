using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThongKe.Data;
using ThongKe.DTOs;
using ThongKe.Entities;
using ThongKe.Shared;
using ThongKe.Shared.Extensions;

namespace ThongKe.Controllers.CT;

[Route("api/donvis")]
[ApiController]
public class DonViController(AppDbContext context, IQueryService queryService) : ControllerBase
{
    [HttpPost("upsert")]
    public async Task<IActionResult> UpsertDonVi([FromBody] DonVi data)
    {
        if (data.Id > 0)
        {
            var existingDonVi = await context.DonVis.FindAsync(data.Id);
            if (existingDonVi == null)
                return BadRequest(ApiResponse<ChiTieu>.Fail(["Không tìm thấy đơn vị cần cập nhật."]));
            existingDonVi.Assign(data);
            await context.SaveChangesAsync();
            return Ok(ApiResponse<DonVi>.Ok(existingDonVi));
        }

        var donVi = new DonVi();
        donVi.Assign(data);
        context.DonVis.Add(donVi);
        await context.SaveChangesAsync();
        return Ok(ApiResponse<DonVi>.Ok(donVi));
    }

    [HttpGet("get-all-in-one")]
    public async Task<IActionResult> GetAllDonVis()
    {
        var results = await context.DonVis.OrderBy(x => x.TenDonVi).ToListAsync();

        return Ok(ApiResponse<List<DonVi>>.Ok(results));
    }

    [HttpPost("get-all")]
    public async Task<IActionResult> GetPagedDonVis([FromBody] DonViQueryDto data)
    {
        var query = context.DonVis.AsQueryable();

        if (!string.IsNullOrEmpty(data.Keyword))
        {
            var keyword = data.Keyword.RemoveSpecialCharactersUnicode().ToLower();
            query = query.Where(t =>
                t.TenDonVi.ToLower().Contains(keyword) ||
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

        var result = await query.ToPagedResultAsync(
            data.PageNumber,
            data.PageSize,
            data.OrderBy,
            data.Ascending
        );


        return Ok(ApiResponse<DonVi>.PagedResult(result));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await context.DonVis.FindAsync(id);
        if (result == null)
            return NotFound(ApiResponse<DonVi>.Fail(["Không tìm thấy đơn vị."]));
        return Ok(ApiResponse<DonVi>.Ok(result));
    }

    [HttpPost("delete/{id:int}")]
    public async Task<IActionResult> DeleteDonVi(int id)
    {
        var donVi = await context.DonVis.FindAsync(id);
        if (donVi == null)
        {
            var errorMessage = $"Đơn vị với ID {id} không tồn tại.";
            return BadRequest(ApiResponse<bool>.Fail([errorMessage]));
        }

        context.DonVis.Remove(donVi);
        await context.SaveChangesAsync();
        return Ok(ApiResponse<bool>.Ok(true, $"Xóa thành công đơn vị với ID {id}"));
    }
}