using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThongKe.Data;
using ThongKe.DTOs;
using ThongKe.Shared;
using ThongKe.Shared.Utils;

namespace ThongKe.Controllers.CT;

[Route("api/bangthongkes")]
[ApiController]
[Authorize]
public class BangThongKeController(AppDbContext context, IQueryService queryService, ITableService tableService)
    : ControllerBase
{
    [HttpPost("get-all-records")]
    public async Task<IActionResult> GetAllRecordsFor([FromBody] RecordQueryDto data)
    {
        var kieuBangThongKe = await context.KieuBangThongKes.FindAsync(data.Id);

        if (kieuBangThongKe == null)
            return BadRequest(ApiResponse<string>.Fail(
                ["Không tìm thấy kiểu bảng thống kê."]
            ));

        if (kieuBangThongKe.TableName == null)
            return BadRequest(
                ApiResponse<string>.Fail(["Kiểu bảng thống kê không có bảng dữ liệu tương ứng."]
                ));

        var results = await queryService.GetAllDataInTable(kieuBangThongKe.TableName!, data.OrderBy, data);
        return Ok(ApiResponse<List<Dictionary<string, object>>>.Ok(results));
    }

    [HttpPost("cursor-filter")]
    public async Task<IActionResult> GetCursorBasedData([FromBody] BangThongKeQueryDto data)
    {
        if (data.KieuBangThongKeId == 0)
            return Ok(ApiResponse<Dictionary<string, object>>.CursorResult(null));

        var kieuBangThongKe = await context.KieuBangThongKes.FindAsync(data.KieuBangThongKeId);

        if (kieuBangThongKe == null)
            return BadRequest(ApiResponse<string>.Fail(
                ["Không tìm thấy kiểu bảng thống kê."]
            ));

        if (kieuBangThongKe.TableName == null)
            return BadRequest(
                ApiResponse<string>.Fail(["Kiểu bảng thống kê không có bảng dữ liệu tương ứng."]
                ));

        kieuBangThongKe.Deserialize();

        var results =
            await queryService.GetCursorBasedDataInTable(kieuBangThongKe.TableName!, "id", data.Cursor, data.PageSize,
                data);
        return Ok(ApiResponse<Dictionary<string, object>>.CursorResult(results));
    }

    [HttpGet("get-data-structure/{kieuBangThongKeId:int}")]
    public async Task<IActionResult> GetDataStructure(int kieuBangThongKeId)
    {
        var kieuBangThongKe = await context.KieuBangThongKes.FindAsync(kieuBangThongKeId);

        if (kieuBangThongKe == null)
            return BadRequest(ApiResponse<string>.Fail(
                ["Không tìm thấy kiểu bảng thống kê."]
            ));

        if (kieuBangThongKe.TableName == null)
            return BadRequest(
                ApiResponse<string>.Fail(["Kiểu bảng thống kê không có bảng dữ liệu tương ứng."]
                ));

        kieuBangThongKe.Deserialize();

        Dictionary<string, object?> structure = new();

        foreach (var item in kieuBangThongKe.ChiTieus)
        {
            structure.TryAdd(item.ColumnName, null);
            foreach (var phanTo in item.PhanTos) structure.TryAdd(phanTo.ColumnName, null);
        }

        return Ok(ApiResponse<Dictionary<string, object?>>.Ok(structure));
    }

    [HttpPost("upsert-many-records")]
    public async Task<IActionResult> UpsertManyRecords([FromBody] UpsertManyRecordDto data)
    {
        var kieuBangThongKe = await context.KieuBangThongKes.FindAsync(data.KieuBangThongKeId);

        if (kieuBangThongKe == null)
            return BadRequest(ApiResponse<string>.Fail(
                ["Không tìm thấy kiểu bảng thống kê."]
            ));

        if (kieuBangThongKe.TableName == null)
            return BadRequest(
                ApiResponse<string>.Fail(["Kiểu bảng thống kê không có bảng dữ liệu tương ứng."]
                ));

        kieuBangThongKe.Deserialize();

        var columns = QueryUtils.ExtractChiTieuColumnsWithId(kieuBangThongKe.ChiTieus)
            .ToList();
        var uniqueColumns = QueryUtils.ExtractChiTieuUniqueColumns(kieuBangThongKe.ChiTieus);
        var records = data.Records.ToList();

        foreach (var record in records)
        foreach (var (key, value) in record)
        {
            // Value là id và nó không được null và để trống
            if (uniqueColumns.Contains(key) && (value == null || value.ToString()?.Length == 0))
                return BadRequest(ApiResponse<string>.Fail(
                    [$"Cột '{key}' là cột duy nhất và không được để trống."]
                ));
            if (!columns.Contains(key))
                record.Remove(key);
        }

        var result =
            await tableService.AutoUpsertManyRecordsToTable(kieuBangThongKe.TableName!, columns, uniqueColumns,
                records);

        return Ok(ApiResponse<int?>.Ok(result));
    }

    [HttpGet("get-record/{kieuBangThongKeId:int}/{recordId:int}")]
    public async Task<IActionResult> GetRecorDetail(int kieuBangThongKeId,
        int recordId)
    {
        var kieuBangThongKe = await context.KieuBangThongKes.FindAsync(kieuBangThongKeId);

        if (kieuBangThongKe == null)
            return BadRequest(ApiResponse<string>.Fail(
                ["Không tìm thấy kiểu bảng thống kê."]
            ));

        if (kieuBangThongKe.TableName == null)
            return BadRequest(
                ApiResponse<string>.Fail(["Kiểu bảng thống kê không có bảng dữ liệu tương ứng."]
                ));
        var result = await queryService.GetRecordInTable(kieuBangThongKe.TableName!, "id", recordId);

        return Ok(ApiResponse<Dictionary<string, object?>>.Ok(result));
    }

    [HttpPost("delete-many-records/{kieuBangThongKeId:int}")]
    public async Task<IActionResult> DeleteManyRecords(int kieuBangThongKeId,
        [FromBody] DeleteManyRecordRequest data)
    {
        var kieuBangThongKe = await context.KieuBangThongKes.FindAsync(kieuBangThongKeId);

        if (kieuBangThongKe == null)
            return BadRequest(ApiResponse<string>.Fail(
                ["Không tìm thấy kiểu bảng thống kê."]
            ));

        if (kieuBangThongKe.TableName == null)
            return BadRequest(
                ApiResponse<string>.Fail(["Kiểu bảng thống kê không có bảng dữ liệu tương ứng."]
                ));
        var result = await tableService.DeleteManyRecordsInTable(kieuBangThongKe.TableName!, "id", data.Ids);
        if (result == null)
            return BadRequest(ApiResponse<string>.Fail(["Xóa bản ghi thất bại."]));

        return Ok(ApiResponse<int?>.Ok(result, "Xóa bản ghi thành công."));
    }
}