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

[Route("api/dulieuchitieus")]
[ApiController]
public class DuLieuChiTieuController(AppDbContext context, IHttpContextAccessor httpContextAccessor) : ControllerBase
{
	[HttpPost("upsert")]
	public async Task<IActionResult> UpsertDuLieuChiTieu([FromBody] ImportDataDto data)
	{
		var chiTieu = await context.ChiTieus.FindAsync(data.ChiTieuId);
		if (chiTieu == null)
			return BadRequest(ApiResponse<ChiTieu>.Fail(new List<string> { "Chỉ tiêu không tồn tại trong hệ thống." }));

		using var transaction = await context.Database.BeginTransactionAsync();

		try
		{
			var duLieuChiTieuList = new List<DuLieuChiTieu>();
			var soLuongThemMoi = 0;
			var soLuongCapNhat = 0;
			// record có id (CHỈ 1 RECORD)
			if (data.Records.Count == 1 && data.Records[0].Id.HasValue && data.Records[0].Id.Value > 0)
			{
				var record = data.Records[0];
				var existingData = await context.DuLieuChiTieus.FindAsync(record.Id.Value);

				if (existingData == null)
				{
					await transaction.RollbackAsync();
					return BadRequest(ApiResponse<object>.Fail(new List<string>
												{ $"Không tìm thấy dữ liệu chỉ tiêu với ID: {record.Id.Value}" }));
				}

				// Kiểm tra xem phân tổ mới có trùng với bản ghi khác không
				var duplicateRecord = await context.DuLieuChiTieus
						.Where(x => x.Id != record.Id.Value &&
												x.ChiTieuId == data.ChiTieuId &&
												x.PhanToId1 == record.PhanToId1 &&
												x.PhanToId2 == record.PhanToId2 &&
												x.PhanToId3 == record.PhanToId3 &&
												x.PhanToId4 == record.PhanToId4 &&
												x.PhanToId5 == record.PhanToId5 &&
												x.PhanToId6 == record.PhanToId6 &&
												x.PhanToId7 == record.PhanToId7 &&
												x.PhanToId8 == record.PhanToId8 &&
												x.PhanToId9 == record.PhanToId9 &&
												x.PhanToId10 == record.PhanToId10)
						.FirstOrDefaultAsync();

				// Nếu có bản ghi trùng phân tổ, xóa nó
				if (duplicateRecord != null) context.DuLieuChiTieus.Remove(duplicateRecord);

				// Cập nhật toàn bộ dữ liệu
				var tempData = new DuLieuChiTieu
				{
					ChiTieuId = existingData.ChiTieuId,
					PhanToId1 = record.PhanToId1,
					PhanToId2 = record.PhanToId2,
					PhanToId3 = record.PhanToId3,
					PhanToId4 = record.PhanToId4,
					PhanToId5 = record.PhanToId5,
					PhanToId6 = record.PhanToId6,
					PhanToId7 = record.PhanToId7,
					PhanToId8 = record.PhanToId8,
					PhanToId9 = record.PhanToId9,
					PhanToId10 = record.PhanToId10,
					Data = record.Data ?? 0
				};

				bool isupdate = PermissionUtil.CanUpdateNghiepVu(httpContextAccessor, context, existingData, GiamSatUserReadUpSert.No);
				if (!isupdate)
				{
					return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<string>.Fail(["Bạn không có quyền truy cập."]));
				}

				existingData.Assign(tempData);
				await context.SaveChangesAsync();
				await transaction.CommitAsync();

				var message = duplicateRecord != null
						? "Cập nhật thành công dữ liệu (đã xóa 1 bản ghi trùng phân tổ)."
						: "Cập nhật thành công dữ liệu.";

				return Ok(ApiResponse<DuLieuChiTieu>.Ok(existingData, message));
			}

			// record không có id (insert)
			if (data.Records.Any(r => r.Id.HasValue && r.Id.Value > 0))
			{
				await transaction.RollbackAsync();
				return BadRequest(ApiResponse<object>.Fail(new List<string>
										{ "Không thể cập nhật nhiều bản ghi có ID cùng lúc. Vui lòng cập nhật từng bản ghi một." }));
			}

			// Load tất cả existing records theo ChiTieuId một lần
			var existingRecordsByPhanTo = await context.DuLieuChiTieus
					.Where(x => x.ChiTieuId == data.ChiTieuId)
					.ToListAsync();

			foreach (var record in data.Records)
			{
				// Kiểm tra trùng phân tổ
				var existingData = existingRecordsByPhanTo.FirstOrDefault(x =>
						x.PhanToId1 == record.PhanToId1 &&
						x.PhanToId2 == record.PhanToId2 &&
						x.PhanToId3 == record.PhanToId3 &&
						x.PhanToId4 == record.PhanToId4 &&
						x.PhanToId5 == record.PhanToId5 &&
						x.PhanToId6 == record.PhanToId6 &&
						x.PhanToId7 == record.PhanToId7 &&
						x.PhanToId8 == record.PhanToId8 &&
						x.PhanToId9 == record.PhanToId9 &&
						x.PhanToId10 == record.PhanToId10
				);

				if (existingData != null)
				{
					// Cập nhật Data khi trùng phân tổ
					existingData.Data = record.Data ?? 0;
					existingData.UpdatedAt = DateTime.UtcNow;
					duLieuChiTieuList.Add(existingData);
					soLuongCapNhat++;
				}
				else
				{

					// Thêm mới
					var newData = new DuLieuChiTieu
					{
						ChiTieuId = data.ChiTieuId,
						PhanToId1 = record.PhanToId1,
						PhanToId2 = record.PhanToId2,
						PhanToId3 = record.PhanToId3,
						PhanToId4 = record.PhanToId4,
						PhanToId5 = record.PhanToId5,
						PhanToId6 = record.PhanToId6,
						PhanToId7 = record.PhanToId7,
						PhanToId8 = record.PhanToId8,
						PhanToId9 = record.PhanToId9,
						PhanToId10 = record.PhanToId10,
						Data = record.Data ?? 0,
						CreatedAt = DateTime.UtcNow
					};
					var resultCreate = PermissionUtil.CanCreateNghiepVu(httpContextAccessor, context, GiamSatUserReadUpSert.No);
					if (!resultCreate.CanCreate)
					{
						return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<string>.Fail(["Bạn không có quyền truy cập."]));
					}
					newData.Scope = resultCreate.Scope;
					newData.CreatedBy = resultCreate.CreatedBy;
					newData.DonViThongKeId = resultCreate.DonViThongKeId;
					newData.TinhThanhId = resultCreate.TinhThanhId;
					context.DuLieuChiTieus.Add(newData);
					duLieuChiTieuList.Add(newData);
					existingRecordsByPhanTo.Add(newData);
					soLuongThemMoi++;
				}
			}

			await context.SaveChangesAsync();
			await transaction.CommitAsync();

			// Tạo message động
			var messageParts = new List<string>();
			if (soLuongThemMoi > 0)
				messageParts.Add($"Thêm mới {soLuongThemMoi} bản ghi");
			if (soLuongCapNhat > 0)
				messageParts.Add($"Cập nhật {soLuongCapNhat} bản ghi");

			var finalMessage = messageParts.Count > 0
					? string.Join(", ", messageParts) + " thành công."
					: "Xử lý thành công.";

			return Ok(ApiResponse<object>.Ok(duLieuChiTieuList, finalMessage));
		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync();
			return BadRequest(ApiResponse<object>.Fail(new List<string>
								{ $"Lỗi khi xử lý dữ liệu: {ex.Message}" }));
		}
	}

	[HttpPost("get-imported-data")]
	public async Task<IActionResult> GetPagedChiTieuDaNhapDatas([FromBody] IdentityRequest data)
	{
		var getAll = context.DuLieuChiTieus.AsQueryable().Where(x => x.ImportId == data.Id);
		var query = PermissionUtil.ApplyDataFilterNghiepVu(httpContextAccessor, context, getAll);

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

		return Ok(ApiResponse<PagedResult<DuLieuChiTieu>>.Ok(result));
	}

	[HttpPost("get-data-lists")]
	public async Task<IActionResult> GetPagedDataForChiTieu(IdentityRequest data)
	{
		var getAll = context.DuLieuChiTieus.AsQueryable().Where(t => t.ChiTieuId == data.Id);
		var query = PermissionUtil.ApplyDataFilterNghiepVu(httpContextAccessor, context, getAll);

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

		return Ok(ApiResponse<DuLieuChiTieu>.PagedResult(result));
	}

	[HttpPost("delete/{id:int}")]
	public async Task<IActionResult> DeleteDuLieuChiTieu(int id)
	{
		var existingData = await context.DuLieuChiTieus.FindAsync(id);
		if (existingData == null)
			return BadRequest(ApiResponse<bool>.Fail(new List<string> { "Không tìm thấy dữ liệu cần xóa." }));
		bool isuDelete = PermissionUtil.CanDeleteNghiepVu(httpContextAccessor, context, existingData, GiamSatUserReadUpSert.No);
		if (!isuDelete)
		{
			return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<string>.Fail(["Bạn không có quyền truy cập."]));
		}
		context.DuLieuChiTieus.Remove(existingData);
		await context.SaveChangesAsync();
		return Ok(ApiResponse<bool>.Ok(true, "Xóa dữ liệu thành công."));
	}
}