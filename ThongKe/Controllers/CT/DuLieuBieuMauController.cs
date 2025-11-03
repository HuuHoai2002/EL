using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;
using ThongKe.Data;
using ThongKe.DTOs;
using ThongKe.Entities;
using ThongKe.Shared;
using ThongKe.Shared.Extensions;
using ThongKe.Shared.Utils;

namespace ThongKe.Controllers.CT;

[Route("api/dulieubieumaus")]
[ApiController]
public class DuLieuBieuMauController(AppDbContext context, IJobQueue job, IQueryService queryService) : ControllerBase
{
    // [HttpPost("submit")]
    // public async Task<IActionResult> Submit([FromBody] UpsertManyFormRecordDto data)
    // {
    //     var bieuMau = await context.BieuMaus.FindAsync(data.BieuMauId);
    //     if (bieuMau == null)
    //         return BadRequest(ApiResponse<string>.Fail(["Không tìm thấy biểu mẫu."]));
    //     bieuMau.Deserialize();
    //
    //     DuLieuBieuMau? duLieuBieuMau = null;
    //
    //     if (data.Id > 0)
    //     {
    //         duLieuBieuMau = await context.DuLieuBieuMaus.FindAsync(data.Id);
    //         if (duLieuBieuMau == null)
    //             return BadRequest(ApiResponse<string>.Fail(
    //                 ["Không tìm thấy dữ liệu biểu mẫu."]));
    //     }
    //
    //     var chiTieuDict = bieuMau.ChiTieus.ToDictionary(x => x.Id, x => x);
    //
    //     var processedRecords = bieuMau.ChiTieus.Select(x => new BieuMauProcessedPayload
    //     {
    //         ChiTieuId = x.Id,
    //         TableName = x.TableName,
    //         Records = [],
    //         Columns = [],
    //         UniqueColumns = []
    //     }).ToList();
    //
    //     foreach (var originalRecord in data.Records)
    //     {
    //         var chiTieuId = originalRecord.TryGetValue("chi_tieu_id", out var chiTieuIdObj)
    //             ? chiTieuIdObj as int?
    //             : null;
    //         if (chiTieuId == null) continue;
    //         var chiTieu = chiTieuDict.GetValueOrDefault(chiTieuId.Value);
    //         if (chiTieu == null) continue;
    //         var processed = processedRecords
    //             .FirstOrDefault(x => x.ChiTieuId == chiTieuId);
    //         if (processed == null) continue;
    //
    //         var columns =
    //             QueryUtils.ExtractChiTieuColumns([chiTieu]);
    //         var uniqueColumns =
    //             QueryUtils.ExtractChiTieuUniqueColumns([chiTieu]);
    //
    //         foreach (var (key, value) in originalRecord)
    //         {
    //             if (!columns.Contains(key))
    //                 continue;
    //             if (uniqueColumns.Contains(key) && (value == null || value.ToString()?.Length == 0))
    //                 return BadRequest(ApiResponse<string>.Fail(
    //                     [$"Cột '{key}' là cột duy nhất và không được để trống."]));
    //             var dataType = QueryUtils.GetTypeByPhanToColumnName(chiTieu.PhanTos, key);
    //             // ... validate data type
    //         }
    //
    //         originalRecord.TryAdd("bieu_mau_id", data.BieuMauId);
    //         originalRecord.TryAdd("hashing", HashingUtils.HashingDataWithUniqueColumns(uniqueColumns, originalRecord));
    //         var cloneRecord = new Dictionary<string, object?>(originalRecord);
    //
    //         processed.Records.Add(cloneRecord);
    //         processed.Columns = columns;
    //         processed.UniqueColumns = uniqueColumns;
    //     }
    //
    //     if (duLieuBieuMau != null)
    //     {
    //         duLieuBieuMau.AssignDongDuLieu(data.Records);
    //         duLieuBieuMau.AssignBieuMau(bieuMau);
    //         context.DuLieuBieuMaus.Update(duLieuBieuMau);
    //     }
    //     else
    //     {
    //         duLieuBieuMau = new DuLieuBieuMau
    //         {
    //             BieuMauId = data.BieuMauId,
    //             NguonDuLieu = data.NguonDuLieu ??
    //                           $"Phiên nhập liệu từ biểu mẫu ngày {DateTime.Now:yyyy-MM-dd HH:mm:ss}"
    //         };
    //         duLieuBieuMau.AssignDongDuLieu(data.Records);
    //         duLieuBieuMau.AssignBieuMau(bieuMau);
    //         context.DuLieuBieuMaus.Add(duLieuBieuMau);
    //     }
    //
    //     await context.SaveChangesAsync();
    //
    //     job.Enqueue(new Job
    //     {
    //         Type = JobType.InsertFormRecord,
    //         Payload = processedRecords
    //     });
    //
    //     return Ok(ApiResponse<bool?>.Ok(true));
    // }

    [HttpPost("submit")]
    public async Task<IActionResult> Submit([FromBody] UpsertManyFormRecordDto data)
    {
        var bieuMau = await context.BieuMaus.FindAsync(data.BieuMauId);
        if (bieuMau == null)
            return BadRequest(ApiResponse<string>.Fail(["Không tìm thấy biểu mẫu."]));

        bieuMau.Deserialize();

        DuLieuBieuMau? duLieuBieuMau = null;
        if (data.Id > 0)
        {
            duLieuBieuMau = await context.DuLieuBieuMaus.FindAsync(data.Id);
            if (duLieuBieuMau == null)
                return BadRequest(ApiResponse<string>.Fail(["Không tìm thấy dữ liệu biểu mẫu."]));
        }

        var chiTieuMetadata = bieuMau.ChiTieus.ToDictionary(
            x => x.Id,
            x => new
            {
                ChiTieu = x,
                Columns = QueryUtils.ExtractChiTieuColumns([x]),
                UniqueColumns = QueryUtils.ExtractChiTieuUniqueColumns([x])
            }
        );

        var processedRecords = chiTieuMetadata.Select(kvp => new BieuMauProcessedPayload
        {
            ChiTieuId = kvp.Key,
            TableName = kvp.Value.ChiTieu.TableName,
            Records = [],
            Columns = kvp.Value.Columns,
            UniqueColumns = kvp.Value.UniqueColumns
        }).ToList();

        var processedDict = processedRecords.ToDictionary(x => x.ChiTieuId);

        foreach (var originalRecord in data.Records)
        {
            if (!originalRecord.TryGetValue("chi_tieu_id", out var chiTieuIdObj)
                || chiTieuIdObj is not int chiTieuId)
                continue;

            if (!chiTieuMetadata.TryGetValue(chiTieuId, out var metadata))
                continue;

            var columns = metadata.Columns;
            var uniqueColumns = metadata.UniqueColumns;
            var chiTieu = metadata.ChiTieu;

            foreach (var (key, value) in originalRecord)
            {
                if (!columns.Contains(key))
                    continue;
                if (uniqueColumns.Contains(key) && (value == null || value.ToString()?.Length == 0))
                    return BadRequest(ApiResponse<string>.Fail(
                        [$"Cột '{key}' là cột duy nhất và không được để trống."]));

                var dataType = QueryUtils.GetTypeByPhanToColumnName(chiTieu.PhanTos, key)!;
                var dataTypeErr = QueryUtils.ValidateDataType(dataType, value);

                if (dataTypeErr != null)
                    return BadRequest(ApiResponse<string>.Fail(
                        [$"{dataTypeErr} Tại cột '{key}'."]));
            }

            originalRecord.TryAdd("bieu_mau_id", data.BieuMauId);
            originalRecord.TryAdd("hashing",
                HashingUtils.HashingDataWithUniqueColumns(uniqueColumns, originalRecord));
            var cloneRecord = new Dictionary<string, object?>(originalRecord);
            processedDict[chiTieuId].Records.Add(cloneRecord);
        }

        if (duLieuBieuMau != null)
        {
            duLieuBieuMau.AssignDongDuLieu(data.Records);
            duLieuBieuMau.AssignBieuMau(bieuMau);
            context.DuLieuBieuMaus.Update(duLieuBieuMau);
        }
        else
        {
            duLieuBieuMau = new DuLieuBieuMau
            {
                BieuMauId = data.BieuMauId,
                NguonDuLieu = data.NguonDuLieu
                              ?? $"Phiên nhập liệu từ biểu mẫu ngày {DateTime.Now:yyyy-MM-dd HH:mm:ss}"
            };
            duLieuBieuMau.AssignDongDuLieu(data.Records);
            duLieuBieuMau.AssignBieuMau(bieuMau);
            context.DuLieuBieuMaus.Add(duLieuBieuMau);
        }

        await context.SaveChangesAsync();

        job.Enqueue(new Job
        {
            Type = JobType.InsertFormRecord,
            Payload = processedRecords
        });

        return Ok(ApiResponse<bool?>.Ok(true));
    }

    [HttpPost("get-all")]
    public async Task<IActionResult> GetPagedDuLieuBieuMaus([FromBody] DuLieuBieuMauQueryDto data)
    {
        var query = context.DuLieuBieuMaus.AsQueryable();

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

        if (data.BieuMauId.HasValue) query = query.Where(x => x.BieuMauId == data.BieuMauId);

        var result = await query.ToPagedResultAsync(
            data.PageNumber,
            data.PageSize,
            data.OrderBy,
            data.Ascending
        );

        return Ok(ApiResponse<DuLieuBieuMau>.PagedResult(result));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var duLieuBieuMau = await context.DuLieuBieuMaus.FindAsync(id);
        if (duLieuBieuMau == null)
            return BadRequest(ApiResponse<string>.Fail(
                ["Không tìm thấy biểu mẫu."]
            ));
        duLieuBieuMau.Deserialize();
        var chiTieu = duLieuBieuMau.BieuMau?.ChiTieus.First();
        if (chiTieu == null) return Ok(ApiResponse<DuLieuBieuMau>.Ok(duLieuBieuMau));

        await using var conn = (OracleConnection)context.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open) await conn.OpenAsync();

        var tableName = chiTieu.TableName;
        var selectedColumns = new List<string> { "id", "hashing" };
        var dataInTables = await queryService.SelectDataInTable(conn, tableName!, selectedColumns);
        var dataDict = dataInTables.ToDictionary(
            row => Convert.ToInt32(row["id"]),
            row => row
        );
        // update duLieuBieuMau::DongDuLieus if hashing exists in table => add field is_exist_in_db = true
        foreach (var record in duLieuBieuMau.DongDuLieus!)
            if (record.TryGetValue("hashing", out var hashObj) && hashObj != null)
            {
                var hashing = hashObj.ToString()!;
                var exists = dataDict.Values.Any(row => row["hashing"]?.ToString() == hashing);
                record["is_exist_in_db"] = exists;
            }
            else
            {
                record["is_exist_in_db"] = false;
            }

        await conn.CloseAsync();

        return Ok(ApiResponse<DuLieuBieuMau>.Ok(duLieuBieuMau));
    }
}