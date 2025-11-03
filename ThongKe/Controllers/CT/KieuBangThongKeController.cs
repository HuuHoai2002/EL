using Microsoft.AspNetCore.Mvc;
using ThongKe.Data;
using ThongKe.DTOs;
using ThongKe.Entities;
using ThongKe.Shared;
using ThongKe.Shared.Extensions;
using static ThongKe.Shared.Utils.Utils;

namespace ThongKe.Controllers.CT;

[Route("api/kieubangthongkes")]
[ApiController]
public class KieuBangThongKeController(
    AppDbContext context,
    IQueryService queryService,
    ITableService tableService,
    IHttpContextAccessor httpContextAccessor)
    : ControllerBase
{
    [HttpGet("get-all-in-one")]
    public async Task<IActionResult> GetAll()
    {
        var getAll = context.KieuBangThongKes.AsQueryable();
        var query = PermissionUtil.ApplyDataFilterNghiepVu(httpContextAccessor, context, getAll);

        foreach (var item in query) item.Deserialize();

        return Ok(ApiResponse<List<KieuBangThongKe>>.Ok(query.ToList()));
    }

    // [HttpPost("upsert")]
    // public async Task<IActionResult> Upsert([FromBody] KieuBangThongKe data)
    // {
    // 	if (data.ChiTieuIds == null || data.ChiTieuIds.Count < 2)
    // 		return BadRequest(ApiResponse<KieuBangThongKe>.Fail(
    // 				["Vui lòng chọn ít nhất 2 chỉ tiêu để tạo kiểu bảng thống kê."]
    // 		));
    //
    // 	if (data.ChiTieuIds.Count > 10)
    // 		return BadRequest(ApiResponse<KieuBangThongKe>.Fail(
    // 				["Số lượng chỉ tiêu trong một kiểu bảng thống kê không được vượt quá 10."]
    // 		));
    //
    // 	var (hasError, chiTieuResults) =
    // 			await queryService.CheckPhienBanChiTieuHasPublished(data.ChiTieuIds, data.PhienBanChiTieuId);
    // 	if (hasError != null) return BadRequest(ApiResponse<KieuBangThongKe>.Fail([hasError]));
    //
    // 	var tableName = tableService.GetTableNameForBangThongKe(data);
    //
    // 	data.ChiTieus = chiTieuResults;
    // 	data.TableName = tableName;
    // 	data.LastTableCreatedAt = DateTime.UtcNow;
    // 	data.HashingSchema =
    // 			HashingUtils.CreateHashFromListString(QueryUtils.ExtractChiTieuUniqueColumns(chiTieuResults));
    //
    // 	KieuBangThongKe? kieuBangThongKe;
    // 	string? oldTableName = null;
    //
    // 	if (data.Id > 0)
    // 	{
    // 		kieuBangThongKe = await context.KieuBangThongKes.FindAsync(data.Id);
    // 		if (kieuBangThongKe == null)
    // 			return BadRequest(ApiResponse<KieuBangThongKe>.Fail(
    // 					["Không tìm thấy kiểu bảng thống kê cần cập nhật."]));
    //
    // 		bool isupdate = PermissionUtil.CanUpdateNghiepVu(httpContextAccessor, context, kieuBangThongKe);
    // 		if (!isupdate)
    // 		{
    // 			return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<string>.Fail(["Bạn không có quyền truy cập."]));
    // 		}
    // 		oldTableName = kieuBangThongKe.TableName;
    // 		kieuBangThongKe.Assign(data);
    // 	}
    // 	else
    // 	{
    // 		var resultCreate = PermissionUtil.CanCreateNghiepVu(httpContextAccessor, context);
    // 		if (!resultCreate.CanCreate)
    // 		{
    // 			return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<string>.Fail(["Bạn không có quyền truy cập."]));
    // 		}
    // 		kieuBangThongKe = new KieuBangThongKe();
    // 		kieuBangThongKe.Scope = resultCreate.Scope;
    // 		kieuBangThongKe.TinhThanhId = resultCreate.TinhThanhId;
    // 		kieuBangThongKe.DonViThongKeId = resultCreate.DonViThongKeId;
    // 		kieuBangThongKe.CreatedBy = resultCreate.CreatedBy;
    // 		kieuBangThongKe.Assign(data);
    // 		context.KieuBangThongKes.Add(kieuBangThongKe);
    // 	}
    //
    // 	await context.SaveChangesAsync();
    //
    // 	string? tableError;
    //
    // 	if (data.Id > 0)
    // 		// Update table
    // 		(tableError, _) = await tableService.UpdateBangThongKe(
    // 				oldTableName!, data);
    // 	else
    // 		// Create table
    // 		(tableError, _) = await tableService.CreateBangThongKe(
    // 				data);
    //
    // 	if (tableError != null)
    // 		return BadRequest(ApiResponse<KieuBangThongKe>.Fail([tableError]));
    //
    // 	return Ok(ApiResponse<KieuBangThongKe>.Ok(kieuBangThongKe));
    // }

    [HttpPost("get-all")]
    public async Task<IActionResult> GetPaged([FromBody] KieuBangThongKeQueryDto data)
    {
        var query = context.KieuBangThongKes.AsQueryable();
        query = PermissionUtil.ApplyDataFilterNghiepVu(httpContextAccessor, context, query);

        if (!string.IsNullOrEmpty(data.Keyword))
        {
            var keyword = data.Keyword.RemoveSpecialCharactersUnicode().ToLower();
            query = query.Where(t =>
                t.Ten.ToLower().Contains(keyword) ||
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

        if (data.PhienBanChiTieuId.HasValue) query = query.Where(x => x.PhienBanChiTieuId == data.PhienBanChiTieuId);

        var result = await query.ToPagedResultAsync(
            data.PageNumber,
            data.PageSize,
            data.OrderBy,
            data.Ascending
        );

        foreach (var item in result.Records)
        {
            item.Deserialize();
            item.ChiTieus = [];
        }

        return Ok(ApiResponse<KieuBangThongKe>.PagedResult(result));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await context.KieuBangThongKes
            .FindAsync(id);
        if (result == null)
            return NotFound(ApiResponse<KieuBangThongKe>.Fail(["Không tìm thấy kiểu bảng thống kê."]));
        var isRead = PermissionUtil.CanReadNghiepVu(httpContextAccessor, context, result);
        if (!isRead)
            return StatusCode(StatusCodes.Status403Forbidden,
                ApiResponse<string>.Fail(["Bạn không có quyền truy cập."]));
        result.Deserialize();

        foreach (var chiTieu in result.ChiTieus) chiTieu.Deserialize();

        return Ok(ApiResponse<KieuBangThongKe>.Ok(result));
    }

    [HttpPost("delete/{id:int}")]
    public async Task<IActionResult> DeleteById(int id)
    {
        var kieuBangThongKe = await context.KieuBangThongKes.FindAsync(id);
        if (kieuBangThongKe == null)
        {
            var errorMessage = $"Kiểu bảng thống kê với ID {id} không tồn tại.";
            return BadRequest(ApiResponse<bool>.Fail([errorMessage]));
        }

        var isDelete = PermissionUtil.CanDeleteNghiepVu(httpContextAccessor, context, kieuBangThongKe);
        if (!isDelete)
            return StatusCode(StatusCodes.Status403Forbidden,
                ApiResponse<string>.Fail(["Bạn không có quyền truy cập."]));
        var tableName = tableService.GetTableNameForBangThongKe(kieuBangThongKe);
        context.KieuBangThongKes.Remove(kieuBangThongKe);

        if (kieuBangThongKe.HasTableInDatabase)
        {
            var (error, _) = await tableService.DropTable(tableName);

            if (error != null)
                return BadRequest(ApiResponse<bool>.Fail(
                    [$"Xóa kiểu bảng thống kê thất bại. Lỗi khi xóa bảng dữ liệu động: {error}"]
                ));
        }

        await context.SaveChangesAsync();

        return Ok(ApiResponse<bool>.Ok(true, $"Xóa thành công kiểu bảng thống kê với ID {id}"));
    }

    [HttpGet("get-schemas/{id:int}")]
    public async Task<IActionResult> GetDataPreview(int id)
    {
        var result = await context.KieuBangThongKes
            .FindAsync(id);
        if (result == null)
            return NotFound(ApiResponse<KieuBangThongKe>.Fail(["Không tìm thấy kiểu bảng thống kê."]));

        var schemas = await queryService.GetTableSchema(result.TableName!);

        return Ok(ApiResponse<object>.Ok(schemas));
    }
}