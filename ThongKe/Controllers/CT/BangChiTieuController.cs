using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThongKe.Data;
using ThongKe.DTOs;
using ThongKe.Shared;

namespace ThongKe.Controllers.CT;

[Route("api/bangchitieus")]
[ApiController]
[Authorize]
public class BangChiTieuController(
    AppDbContext context,
    IQueryService queryService
) : ControllerBase
{
    [HttpPost("get-all-records")]
    public async Task<IActionResult> GetAllRecords([FromBody] BangChiTieuQueryDto data)
    {
        var chiTieu = context.ChiTieus.FirstOrDefault(x => x.TableName == data.TableName);
        if (chiTieu == null)
            return BadRequest(ApiResponse<string>.Fail(
                ["Không tìm thấy chỉ tiêu."]
            ));
        chiTieu.Deserialize();
        var results =
            await queryService.GetAllDataInTable(chiTieu.TableName!, "id", data);
        return Ok(ApiResponse<object>.Ok(results));
    }

    [HttpPost("cursor-filter")]
    public async Task<IActionResult> GetCursorBasedData([FromBody] BangChiTieuQueryDto data)
    {
        var chiTieu = context.ChiTieus.FirstOrDefault(x => x.TableName == data.TableName);
        if (chiTieu == null)
            return BadRequest(ApiResponse<string>.Fail(
                ["Không tìm thấy chỉ tiêu."]
            ));
        chiTieu.Deserialize();
        var results =
            await queryService.GetCursorBasedDataInTable(chiTieu.TableName!, "id", data.Cursor, data.PageSize, data);
        results.Metadata = new Dictionary<string, object>
        {
            { "chiTieu", chiTieu }
        };
        return Ok(ApiResponse<Dictionary<string, object>>.CursorResult(results));
    }

    [HttpPost("get-data-sources")]
    public async Task<IActionResult> GetDataSources([FromBody] GetDataSourceDto data)
    {
        if (data.Id == null && data.Hashing == null)
            return BadRequest(ApiResponse<string>.Fail(
                ["Cần cung cấp Id hoặc Hashing để lấy dữ liệu nguồn."]
            ));
        List<Dictionary<string, object?>> existingDuLieus = [];

        var whereBuilder = data.Hashing != null ? $"\"hashing\" = '{data.Hashing}'" : $"\"id\" = '{data.Id}'";

        var record = await queryService.GetRecordWhereBuilderInTable(data.TableName, whereBuilder);

        if (record.Count == 0)
            return BadRequest(ApiResponse<string>.Fail(
                ["Không tìm thấy bản ghi tương ứng."]
            ));
        var bieuMauId = record.TryGetValue("bieu_mau_id", out var bieuMauIdObj) ? Convert.ToInt32(bieuMauIdObj) : 0;
        if (bieuMauId == 0) return Ok(ApiResponse<object>.Ok(new { record, duLieuBieuMaus = existingDuLieus }));
        var hashing = record["hashing"]?.ToString() ?? "";
        var duLieuBieuMaus = await context.DuLieuBieuMaus.Where(x => x.BieuMauId == bieuMauId).ToListAsync();

        if (bieuMauId > 0)
            foreach (var duLieu in duLieuBieuMaus)
            {
                duLieu.Deserialize();
                existingDuLieus.AddRange(duLieu.DongDuLieus.Where(dongDuLieu =>
                {
                    // ReSharper disable once CanSimplifyDictionaryLookupWithTryGetValue
                    if (!dongDuLieu.ContainsKey("hashing")) return false;
                    dongDuLieu.TryAdd("nguon_du_lieu", duLieu.NguonDuLieu);
                    dongDuLieu.TryAdd("updated_at", duLieu.UpdatedAt);
                    return dongDuLieu["hashing"]?.ToString() == hashing;
                }));
            }

        existingDuLieus = existingDuLieus.OrderByDescending(x => x["updated_at"]).ToList();

        var response = new { record, duLieuBieuMaus = existingDuLieus };

        return Ok(ApiResponse<object>.Ok(response));
    }
}