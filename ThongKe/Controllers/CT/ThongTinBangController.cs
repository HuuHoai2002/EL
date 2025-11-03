using Microsoft.AspNetCore.Mvc;
using ThongKe.Data;
using ThongKe.Shared;

namespace ThongKe.Controllers.CT;

[Route("api/thongtinbangs")]
[ApiController]
public class ThongTinBangController(AppDbContext context, IQueryService queryService) : ControllerBase
{
    [HttpGet("get-all")]
    public IActionResult GetAll(string? keyword)
    {
        List<object> results = [];
        var kieuBangs = context.KieuBangThongKes.Where(x => x.TableName != null);
        var chiTieus = context.ChiTieus.ToList();

        if (!string.IsNullOrEmpty(keyword))
        {
            keyword = keyword.ToLower();
            kieuBangs = kieuBangs.Where(x => x.Ten!.ToLower().Contains(keyword) || x.MoTa!.Contains(keyword));
        }

        foreach (var result in kieuBangs.ToList())
        {
            result.Deserialize();
            if (result.ChiTieus.Count <= 0) continue;
            results.Add(new
            {
                result.Ten,
                TenBang = result.TableName,
                KieuBangThongKeId = result.Id,
                result.PhienBanChiTieuId,
                LoaiBang = "Bảng thống kê",
                result.UpdatedAt
            });
        }

        foreach (var result in chiTieus)
        {
            result.Deserialize();
            results.Add(new
            {
                Ten = result.TenChiTieu,
                TenBang = result.TableName,
                ChiTieuId = result.Id,
                LoaiBang = "Bảng chỉ tiêu",
                result.UpdatedAt
            });
        }

        return Ok(ApiResponse<object>.Ok(results));
    }
}