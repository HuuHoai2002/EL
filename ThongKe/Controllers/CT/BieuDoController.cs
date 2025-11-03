// using System.Data;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using Oracle.ManagedDataAccess.Client;
// using ThongKe.Data;
// using ThongKe.DTOs;
// using ThongKe.Shared;
// using ThongKe.Shared.Enums;
// using ThongKe.Shared.Utils;
//
// namespace ThongKe.Controllers.CT;
//
// [Route("api/bieudos")]
// [ApiController]
// public class BieuDoController(
//     AppDbContext context,
//     IQueryService queryService
// ) : ControllerBase
// {
//     [HttpPost("query")]
//     public async Task<IActionResult> Query(BieuDoQueryDto query)
//     {
//         if (!Constants.AggreatedFunctions.Contains(query.AggregateFunction!.ToUpper()))
//             return BadRequest(ApiResponse<string>.Fail(
//                 ["Hàm tổng hợp không hợp lệ."]
//             ));
//
//         var phienBanChiTieu = await context.PhienBanChiTieus.FindAsync(query.PhienBanChiTieuId);
//         if (phienBanChiTieu == null)
//             return BadRequest(ApiResponse<string>.Fail(
//                 ["Không tìm thấy phiên bản chỉ tiêu."]
//             ));
//         phienBanChiTieu.Deserialize();
//
//         var chiTieus = phienBanChiTieu.ChiTieus.Where(x => query.ChiTieuIds.Contains(x.Id)).ToList();
//         if (chiTieus.Count != query.ChiTieuIds.Count)
//             return BadRequest(ApiResponse<string>.Fail(
//                 ["Không tìm thấy một hoặc nhiều chỉ tiêu tương ứng với biểu đồ."]
//             ));
//
//         foreach (var chiTieu in chiTieus) chiTieu.Deserialize();
//         var (chiTieuColumns, commonPhanToColumns, error) = QueryUtils.ExtractListChiTieuPhanTos(chiTieus);
//
//         var response = new Dictionary<string, object?>
//         {
//             { "flattenSeries", new Dictionary<string, object?>() },
//             { "series", new List<Dictionary<string, object?>>() },
//             { "tables", new Dictionary<string, object?>() },
//             { "strings", new Dictionary<string, object?>() },
//             { "type", string.Empty }
//         };
//
//         // ref to response
//         var flattenSeries = (Dictionary<string, object?>)response["flattenSeries"]!;
//         var tables = (Dictionary<string, object?>)response["tables"]!;
//         var strings = (Dictionary<string, object?>)response["strings"]!;
//         var series = (List<Dictionary<string, object?>>)response["series"]!;
//
//         flattenSeries["xLabels"] = new List<string>();
//         flattenSeries["uData"] = new List<int?>();
//
//         foreach (var column in chiTieuColumns)
//         {
//             flattenSeries[column] = null;
//             var chiTieu = chiTieus.FirstOrDefault(x => x.DefineColumnName == column);
//             tables[column] = chiTieu?.TableName;
//             strings[column] = chiTieu?.TenChiTieu;
//         }
//
//         await using var conn = (OracleConnection)context.Database.GetDbConnection();
//         if (conn.State != ConnectionState.Open) await conn.OpenAsync();
//
//         // not throw error yet (no common phan to)
//         if (!string.IsNullOrEmpty(error))
//         {
//             response["type"] = "ChiTieu";
//             foreach (var column in commonPhanToColumns) flattenSeries[column] = null;
//         }
//         else
//         {
//             response["type"] = "PhanTo";
//         }
//
//         foreach (var key in flattenSeries.Keys.ToList())
//         {
//             if (key is "xLabels" or "uData") continue;
//             if (tables[key] == null || strings[key] == null) continue;
//
//             var result = await queryService.GetAggregatedColumnsAsync(
//                 conn, tables[key]!.ToString()!, [key], query.AggregateFunction ?? "SUM"
//             );
//
//             flattenSeries[key] = result.TryGetValue(key, out var val) ? val ?? 0 : 0;
//
//             series.Add(new Dictionary<string, object?>
//             {
//                 { "id", key },
//                 { "label", strings[key]!.ToString() },
//                 { "value", flattenSeries[key] ?? 0 }
//             });
//
//             ((List<string>)flattenSeries["xLabels"]!).Add(strings[key]!.ToString()!);
//             ((List<int?>)flattenSeries["uData"]!).Add(Convert.ToInt32(flattenSeries[key]));
//         }
//
//         await conn.CloseAsync();
//
//         return Ok(ApiResponse<Dictionary<string, object?>>.Ok(response));
//     }
//
//     [HttpPost("filter")]
//     public async Task<IActionResult> Filter(BieuDoFilterDto filter)
//     {
//         var phienBanChiTieu = await context.PhienBanChiTieus.FindAsync(filter.PhienBanChiTieuId);
//         if (phienBanChiTieu == null)
//             return BadRequest(ApiResponse<string>.Fail(
//                 ["Không tìm thấy phiên bản chỉ tiêu."]
//             ));
//         phienBanChiTieu.Deserialize();
//
//         var chiTieus = phienBanChiTieu.ChiTieus.Where(x => filter.ChiTieuIds.Contains(x.Id)).ToList();
//         if (chiTieus.Count != filter.ChiTieuIds.Count)
//             return BadRequest(ApiResponse<string>.Fail(
//                 ["Không tìm thấy một hoặc nhiều chỉ tiêu tương ứng với biểu đồ."]
//             ));
//
//         await using var conn = (OracleConnection)context.Database.GetDbConnection();
//         if (conn.State != ConnectionState.Open) await conn.OpenAsync();
//
//         var dicts = new Dictionary<int, object?>();
//         foreach (var chiTieu in chiTieus)
//         {
//             var results = await queryService.GetAllDataInTable(conn, chiTieu.TableName!);
//             dicts.TryAdd(chiTieu.Id, results);
//         }
//
//         await conn.CloseAsync();
//
//         return Ok(ApiResponse<object>.Ok(dicts));
//     }
// }

