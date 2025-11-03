using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThongKe.Data;
using ThongKe.DTOs;
using ThongKe.Entities;
using ThongKe.Shared;
using ThongKe.Shared.Extensions;
using static ThongKe.Shared.Utils.Utils;
using static ThongKe.Shared.Utils.Utils.PermissionUtil;

namespace ThongKe.Controllers.CT;

[Route("api/nhapchitieus")]
[ApiController]
public class NhapChiTieuController(AppDbContext context, IQueryService queryService, IHttpContextAccessor httpContextAccessor)
		: ControllerBase
{
	[HttpPost("get-all")]
	public async Task<IActionResult> GetPagedChiTieuDaNhaps([FromBody] NhapChiTieuQueryDto data)
	{
		var getAll = context.ChiTieuDaNhaps.AsQueryable();
		var query = PermissionUtil.ApplyDataFilterNghiepVu(httpContextAccessor, context, getAll);


		if (!string.IsNullOrEmpty(data.Keyword))
		{
			var keyword = data.Keyword.RemoveSpecialCharactersUnicode().ToLower();
			query = query.Where(t =>
					(t.Nguon ?? "").ToLower().Contains(keyword) ||
					(t.Ten ?? "").ToLower().Contains(keyword));
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

		if (data.ChiTieuIds.Count > 0) query = query.Where(t => data.ChiTieuIds.Contains(t.ChiTieuId ?? -1));

		var result = await query.ToPagedResultAsync(
				data.PageNumber,
				data.PageSize,
				data.OrderBy,
				data.Ascending
		);

		return Ok(ApiResponse<ChiTieuDaNhap>.PagedResult(result));
	}

	[HttpGet("{id:int}")]
	public async Task<IActionResult> GetChiTieuDaNhapById(int id)
	{
		var result = await context.ChiTieuDaNhaps.FindAsync(id);
		if (result == null) return BadRequest(ApiResponse<ChiTieuDaNhap>.Fail(["Không tìm thấy chỉ tiêu import."]));
		bool isRead = PermissionUtil.CanReadNghiepVu(httpContextAccessor, context, result);
		if (!isRead) return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<string>.Fail(["Bạn không có quyền xem người dùng này."]));
		result.Deserialize();
		return Ok(ApiResponse<ChiTieuDaNhap>.Ok(result));
	}

	[HttpPost("imports-data")]
	public async Task<IActionResult> ImportDataForChiTieuId([FromBody] ImportDataDto data)
	{
		if (data.Records == null || data.Records.Count == 0)
			return BadRequest(ApiResponse<object>.Fail(["Không có dữ liệu để nhập."]));

		var chiTieu = await context.ChiTieus.FindAsync(data.ChiTieuId);
		if (chiTieu == null)
			return BadRequest(ApiResponse<List<DuLieuChiTieu>>.Fail(["Không tìm thấy chỉ tiêu."]));

		chiTieu?.Deserialize();

		using var transaction = await context.Database.BeginTransactionAsync();

		var chiTieuDaNhap = new ChiTieuDaNhap
		{
			ChiTieuId = data.ChiTieuId,
			Nguon = data.Nguon,
			Ten = data.Ten
		};

		var duLieuChiTieuList = new List<DuLieuChiTieu>();
		var duLieuValidList = new List<DuLieuChiTieu>();

		foreach (var dulieuChitieu in data.Records)
		{
			var errors = new List<string>();

			// Validate 
			if (dulieuChitieu.PhanToId1 == null && dulieuChitieu.PhanToId2 == null &&
					dulieuChitieu.PhanToId3 == null && dulieuChitieu.PhanToId4 == null &&
					dulieuChitieu.PhanToId5 == null && dulieuChitieu.PhanToId6 == null &&
					dulieuChitieu.PhanToId7 == null && dulieuChitieu.PhanToId8 == null &&
					dulieuChitieu.PhanToId9 == null && dulieuChitieu.PhanToId10 == null)
			{
				errors.Add("Không có phân tổ nào được chọn.");
			}
			if (!dulieuChitieu.Data.HasValue)
			{
				errors.Add("Dữ liệu không được để trống.");
			}

			var entity = new DuLieuChiTieu
			{
				ChiTieuId = data.ChiTieuId,
				PhanToId1 = dulieuChitieu.PhanToId1,
				PhanToId2 = dulieuChitieu.PhanToId2,
				PhanToId3 = dulieuChitieu.PhanToId3,
				PhanToId4 = dulieuChitieu.PhanToId4,
				PhanToId5 = dulieuChitieu.PhanToId5,
				PhanToId6 = dulieuChitieu.PhanToId6,
				PhanToId7 = dulieuChitieu.PhanToId7,
				PhanToId8 = dulieuChitieu.PhanToId8,
				PhanToId9 = dulieuChitieu.PhanToId9,
				PhanToId10 = dulieuChitieu.PhanToId10,
				Data = dulieuChitieu.Data ?? 0
			};

			if (errors.Any())
			{
				entity.ErrorMessage = string.Join("; ", errors);
				duLieuChiTieuList.Add(entity);
			}
			else
			{
				duLieuChiTieuList.Add(entity);
				duLieuValidList.Add(entity);
			}
		}
		var resultCreateChiTieuDaNhap = PermissionUtil.CanCreateNghiepVu(httpContextAccessor, context, GiamSatUserReadUpSert.No);
		if (!resultCreateChiTieuDaNhap.CanCreate)
		{
			return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<string>.Fail(["Bạn không có quyền truy cập."]));
		}
		chiTieuDaNhap.Scope = resultCreateChiTieuDaNhap.Scope;
		chiTieuDaNhap.TinhThanhId = resultCreateChiTieuDaNhap.TinhThanhId;
		chiTieuDaNhap.DonViThongKeId = resultCreateChiTieuDaNhap.DonViThongKeId;
		chiTieuDaNhap.CreatedBy = resultCreateChiTieuDaNhap.CreatedBy;
		// Chỉ lưu bản ghi hợp lệ vào ChiTieuDaNhap
		chiTieuDaNhap.AssignDongDuLieu(duLieuValidList);

		context.ChiTieuDaNhaps.Add(chiTieuDaNhap);

		var existingDataList = await context.DuLieuChiTieus
				.Where(x => x.ChiTieuId == data.ChiTieuId)
				.ToListAsync();

		int soBanGhiLoi = 0;
		int soBanGhiCapNhat = 0;
		int soBanGhiMoi = 0;

		foreach (var dongDuLieu in chiTieuDaNhap.DongDuLieus)
			try
			{
				var existingData = existingDataList.FirstOrDefault(x =>
						x.PhanToId1 == dongDuLieu.PhanToId1 &&
						x.PhanToId2 == dongDuLieu.PhanToId2 &&
						x.PhanToId3 == dongDuLieu.PhanToId3 &&
						x.PhanToId4 == dongDuLieu.PhanToId4 &&
						x.PhanToId5 == dongDuLieu.PhanToId5 &&
						x.PhanToId6 == dongDuLieu.PhanToId6 &&
						x.PhanToId7 == dongDuLieu.PhanToId7 &&
						x.PhanToId8 == dongDuLieu.PhanToId8 &&
						x.PhanToId9 == dongDuLieu.PhanToId9 &&
						x.PhanToId10 == dongDuLieu.PhanToId10
				);

				if (existingData != null)
				{
					existingData.Data = dongDuLieu.Data;
					existingData.UpdatedAt = DateTime.UtcNow;
					bool isupdate = PermissionUtil.CanUpdateNghiepVu(httpContextAccessor, context, existingData, GiamSatUserReadUpSert.No);
					if (!isupdate)
					{
						return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<string>.Fail(["Bạn không có quyền truy cập."]));
					}
					context.DuLieuChiTieus.Update(existingData);
					soBanGhiCapNhat++;
				}
				else
				{
					var resultCreate = PermissionUtil.CanCreateNghiepVu(httpContextAccessor, context, GiamSatUserReadUpSert.No);
					if (!resultCreate.CanCreate)
					{
						return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<string>.Fail(["Bạn không có quyền truy cập."]));
					}
					dongDuLieu.TinhThanhId = resultCreate.TinhThanhId;
					dongDuLieu.CreatedBy = resultCreate.CreatedBy;
					dongDuLieu.Scope = resultCreate.Scope;
					dongDuLieu.DonViThongKeId = resultCreate.DonViThongKeId;
					context.DuLieuChiTieus.Add(dongDuLieu);
					soBanGhiMoi++;
				}
			}
			catch (Exception ex)
			{
				dongDuLieu.ErrorMessage = ex.Message;
				soBanGhiLoi++;
			}

		// Đếm số bản ghi lỗi từ danh sách tổng
		soBanGhiLoi += duLieuChiTieuList.Count(x => !string.IsNullOrEmpty(x.ErrorMessage) && !duLieuValidList.Contains(x));

		await context.SaveChangesAsync();
		await transaction.CommitAsync();

		var message = $"Nhập thành công: {soBanGhiMoi} bản ghi mới, {soBanGhiCapNhat} bản ghi cập nhật";
		if (soBanGhiLoi > 0)
			message += $", {soBanGhiLoi} bản ghi lỗi";

		return Ok(ApiResponse<object>.Ok(duLieuChiTieuList, message));
	}
}