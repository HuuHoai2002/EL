using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using ThongKe.Data;
using ThongKe.DTOs;
using ThongKe.Entities;
using ThongKe.Shared;
using ThongKe.Shared.Extensions;
using ThongKe.Shared.Utils;
using static ThongKe.Shared.Utils.Utils;

namespace ThongKe.Controllers.DM;

public class DonViThongKeController : ControllerBase
{
	private readonly AppDbContext _context;
	private readonly IHttpContextAccessor _httpContextAccessor;

	public DonViThongKeController(AppDbContext context, IHttpContextAccessor httpContextAccessor)
	{
		_context = context;
		_httpContextAccessor = httpContextAccessor;
	}

	// Thêm, sửa đơn vị
	[HttpPost("api/donvithongke/donvithongke-upsert")]
	public IActionResult UpSert_DonViThongKe([FromBody] DonViThongKe request)
	{
		if (request == null || string.IsNullOrEmpty(request.TenDonVi) || string.IsNullOrEmpty(request.MaDonVi))
			return BadRequest(ApiResponse<string>.Fail(["TenDonVi, MaDonVi không được để trống"]));
		//check DonVi unique
		var staticDonVis = StaticData.DonViThongKes;
		var isStaticExist = staticDonVis.Any(x =>
		x.MaDonVi.Equals(request.MaDonVi, StringComparison.OrdinalIgnoreCase)
		&& x.MaDonVi != null);

		if (isStaticExist)
		{
			return Conflict(ApiResponse<string>.Fail(["MaDonVi đã tồn tại"]));
		}
		var checkExist = _context.DonViThongKe.FirstOrDefault(x => x.MaDonVi == request.MaDonVi && x.Id != request.Id);
		if (checkExist != null)
			return Conflict(ApiResponse<string>.Fail(["MaDonVi đã tồn tại"]));

		if (request.Id > 0)
		{
			bool isupdate = PermissionUtil.CanUpdateNghiepVu(_httpContextAccessor, _context, request);
			if (!isupdate)
			{
				return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<string>.Fail(["Bạn không có quyền truy cập."]));
			}
			var objdb = _context.DonViThongKe.Find(request.Id);
			if (objdb == null) return BadRequest(ApiResponse<string>.Fail(["Không tìm thấy dữ liệu"]));
			objdb.Assign(request);
			_context.DonViThongKe.Update(objdb);
		}
		else
		{
			var resultCreate = PermissionUtil.CanCreateAuth(_httpContextAccessor, _context, request);
			if (!resultCreate.CanCreate)
			{
				return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<string>.Fail(["Bạn không có quyền truy cập."]));
			}
			var donvi = new DonViThongKe();
			request.Scope = resultCreate.Scope;
			request.TinhThanhId = resultCreate.TinhThanhId;
			request.DonViThongKeId = resultCreate.DonViThongKeId;
			request.CreatedBy = resultCreate.CreatedBy; 
			donvi.Assign(request);
			_context.DonViThongKe.Add(donvi);
		}
		_context.SaveChanges();
		return Ok(request);
	}

	// xóa đơn vị
	[HttpGet("api/donvithongke/donvithongke-delete/{id}")]
	public IActionResult Delete_DonViThongKe_Multiple(int id)
	{
		if (id <= 0)
			return BadRequest(ApiResponse<string>.Fail(["Id không được để trống"]));
		var objdb = _context.DonViThongKe.Find(id);
		if (objdb == null)
			return BadRequest(ApiResponse<string>.Fail(["Không tìm thấy dữ liệu"]));
		bool isuDelete = PermissionUtil.CanDeleteNghiepVu(_httpContextAccessor, _context, objdb);
		if (!isuDelete)
		{
			return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<string>.Fail(["Bạn không có quyền xóa dữ liệu này."]));
		}
		_context.DonViThongKe.Remove(objdb);
		_context.SaveChanges();
		return Ok("Xóa thành công");
	}

	// lấy danh sách có phân trang
	[HttpPost("api/donvithongke/donvithongke-pagedlist")]
	public IActionResult GetAllDonViThongKe([FromBody] DonViThongKepagedRequest request)
	{
		if (request == null)
			return BadRequest(ApiResponse<string>.Fail(["Dữ liệu không được để trống"]));

		// Lấy dữ liệu động từ DB, map sang DTO
		var data = _context.DonViThongKe.ToList();
		var dataStatic = StaticData.DonViThongKes;
		var allData = data.Concat(dataStatic).AsQueryable();

		var query = PermissionUtil.ApplyDataFilterNghiepVu<DonViThongKe>(_httpContextAccessor, _context, allData);

		if (!string.IsNullOrEmpty(request.Keyword))
		{
			var keyword = request.Keyword.RemoveSpecialCharactersUnicode().ToLower();
			query = query.Where(t =>
					t.TenDonVi.ToLower().Contains(keyword) ||
					(t.MaDonVi ?? "").ToLower().Contains(keyword));
		}

		if (request.CreatedFrom.HasValue)
		{
			var utcCreatedFrom = request.CreatedFrom.Value.ToUtc();
			query = query.Where(t => t.CreatedAt >= utcCreatedFrom);
		}

		if (request.CreatedTo.HasValue)
		{
			var utcCreatedTo = request.CreatedTo.Value.ToUtc().AddDays(1);
			query = query.Where(t => t.CreatedAt < utcCreatedTo);
		}

		if (request.UpdatedFrom.HasValue)
		{
			var utcUpdatedFrom = request.UpdatedFrom.Value.ToUtc();
			query = query.Where(t => t.UpdatedAt >= utcUpdatedFrom);
		}

		if (request.UpdatedTo.HasValue)
		{
			var utcUpdatedTo = request.UpdatedTo.Value.ToUtc().AddDays(1);
			query = query.Where(t => t.UpdatedAt < utcUpdatedTo);
		}


		if (!string.IsNullOrEmpty(request.CreatedBy))
		{
			var createdBy = request.CreatedBy.ToLower();
			query = query.Where(t => (t.CreatedBy ?? "").ToLower().Contains(createdBy));
		}

		var totalCount = query.ToList().Count();
		var items = query
				.Skip((request.PageNumber - 1) * request.PageSize)
				.Take(request.PageSize)
				.ToList();

		var result = new PagedResult<DonViThongKe>(
		items,
		totalCount,
		request.PageNumber,
		request.PageSize
		);

		return Ok(ApiResponse<DonViThongKe>.PagedResult(result));
	}

	// lấy 1 bản ghi theo id
	[HttpGet("api/donvithongke/{madonvi}")]
	public IActionResult GetByIdDonViThongKe(string madonvi)
	{
		if (string.IsNullOrEmpty(madonvi))
			return BadRequest(ApiResponse<string>.Fail(["Madonvi không được để trống"]));
		var data = _context.DonViThongKe.AsEnumerable();
		var dataStatic = StaticData.DonViThongKes;
		var query = data.Concat(dataStatic);
		var getByMaDonVi = query.FirstOrDefault(x => x.MaDonVi == madonvi);
		if (getByMaDonVi == null)
			return NotFound(ApiResponse<string>.Fail(["Không tìm thấy dữ liệu"]));
		bool canRead = PermissionUtil.CanReadNghiepVu(_httpContextAccessor, _context, getByMaDonVi);
		if (!canRead)
		{
			return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<string>.Fail(["Bạn không có quyền xem dữ liệu này"]));
		}
		return Ok(getByMaDonVi);
	}
}