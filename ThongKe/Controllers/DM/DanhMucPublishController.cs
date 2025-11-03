using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ThongKe.Data;
using ThongKe.DTOs;
using ThongKe.Entities;
using ThongKe.Shared.Extensions;
using ThongKe.Shared.Utils;

namespace ThongKe.Controllers.DM;

[Authorize]
[ApiController]
public class DanhMucPublishController : ControllerBase
{
	private readonly AppDbContext _context;

	public DanhMucPublishController(AppDbContext context)
	{
		_context = context;
	}

	/// </summary>
	[HttpPost("api/danhmuc/danhmucpublish/detail")]
	public IActionResult GetDetailKieuDanhMucInPublish(FilterPublishDanhMuc request)
	{
		DanhMucPublish? dataPublish;

		if (request.PublishId == null)
			dataPublish = _context.DanhMucPublish.OrderByDescending(x => x.Published).FirstOrDefault();
		else
			dataPublish = _context.DanhMucPublish.Find(request.PublishId);

		if (dataPublish == null) return NotFound("Chưa có dữ liệu publish");

		dataPublish.Deserialize();
		//var listDM = _context.KieuDanhMuc.Where(kdm => dataPublish.MaKieuDanhMucGocs.Contains(kdm.MaKieu));
		//foreach (var item in listDM)
		//{
			
		//}
		return StatusCode(StatusCodes.Status200OK, dataPublish);
	}


	/// <summary>
	///    upsert danhmuc
	///     <route>POST: api/danhmuc/danhmucpublish-upsert</route>
	/// </summary>
	[HttpPost("api/danhmuc/danhmucpublish-upsert")]
	public IActionResult Upsert_DanhMucPublish(DanhMucPublish request)
	{
		if (request == null)
			return StatusCode(StatusCodes.Status400BadRequest, "request_required");
		if (string.IsNullOrEmpty(request.TenPhienBan))
			return StatusCode(StatusCodes.Status400BadRequest, "tenPhienBan_required");
		// tên không trùng
		var checkTenPB = _context.DanhMucPublish.FirstOrDefault(x => x.TenPhienBan == request.TenPhienBan);
		if (checkTenPB != null)
		{
			if (request.Id <= 0 || (request.Id > 0 && checkTenPB.Id != request.Id))
				return StatusCode(StatusCodes.Status409Conflict, "tenPhienBan_exists");
		}
		
		// update
		if (request.Id > 0)
		{
			var objdb = _context.DanhMucPublish.Find(request.Id);
			if (objdb == null)
				return StatusCode(StatusCodes.Status404NotFound, "Id_invalid");
			if (objdb.IsPublish == 1)
				return StatusCode(StatusCodes.Status400BadRequest, "Phiên bản đã được phê duyệt, không thể cập nhật");
			objdb.Updated = Utils.DateTimeToUnixTimeStamp(DateTime.Now);
			objdb.Assign(request);
			_context.Update(objdb);
		}
		else
		{
			if (request.MaKieuDanhMucGocs.Count == 0 && request.MaKieuDanhMucGocs == null)
				return StatusCode(StatusCodes.Status400BadRequest, "maKieuDanhMucGoc_required");

			var listKieuDanhMucGoc = _context.KieuDanhMuc.Where(x => request.MaKieuDanhMucGocs.Contains(x.MaKieu));
			if (listKieuDanhMucGoc.Count() != request.MaKieuDanhMucGocs.Count)
				return StatusCode(StatusCodes.Status400BadRequest, "maKieuDanhMucGocs_invalid");
			foreach (var item in listKieuDanhMucGoc)
			{
				item.Deserialize();
			}
			var danhMucPublish = new DanhMucPublish();
			danhMucPublish.DanhMucs = listKieuDanhMucGoc.ToList();
			danhMucPublish.MaKieuDanhMucGocs = request.MaKieuDanhMucGocs;
			danhMucPublish.Assign(request);
			danhMucPublish.Serialize();
			_context.DanhMucPublish.Add(danhMucPublish);
		}
		_context.SaveChanges();
		return StatusCode(StatusCodes.Status200OK);
	}

	///// <summary>
	/////     Import list danh mục
	/////     <route>POST: api/danhmuc/danhmucpublish-update</route>
	///// </summary>
	//[HttpPost("api/danhmuc/danhmucpublish-update")]
	//[AllowAnonymous]
	//public IActionResult Update_DanhMucPublish(DanhMucPublish request)
	//{
	//	if (request == null) return StatusCode(400, "request_required");

	//	if (request.Id <= 0) return StatusCode(400, "id_required");

	//	var objdb = _context.DanhMucPublish.Find(request.Id);

	//	if (objdb == null) return StatusCode(404, "Id_invalid");

	//	objdb.Updated = Utils.DateTimeToUnixTimeStamp(DateTime.Now);
	//	objdb.Assign(request);

	//	_context.Update(objdb);
	//	_context.SaveChanges();

	//	return Ok(request);
	//}

	///// <summary>
	/////     Lấy danh sách (phân trang, tìm kiếm).
	/////     <route>POST: api/danhmuc/danhmucpublishs-list</route>
	/////     <param name="request">page, limit, search</param>
	/////     <returns>PagedListResponse<DanhMuc></returns>
	///// </summary>
	[HttpPost("api/danhmuc/danhmucpublishs-list")]
	public IActionResult Get_DanhMucPublishList(PagedListRequestDanhMucPublish request)
	{
		if (request == null)
			return StatusCode(400, "request_required");
		var timeNow = Utils.DateTimeToUnixTimeStamp(DateTime.Now);
		var page = request?.Page > 0 ? request.Page : 1;
		var limit = (request?.Limit ?? 0) > 0 ? request.Limit : int.MaxValue;
		var search = request?.Search;
		var query = _context.DanhMucPublish.AsQueryable();

		if (request.IsPublished.HasValue)
		{
			if (request.IsPublished.Value == 1)
				query = query.Where(t => t.IsPublish == 1 && t.IsDraft == 0);
			else if (request.IsPublished.Value == 0)
				query = query.Where(t => t.IsPublish == 0 && t.IsDraft == 1);
		}
		if (!string.IsNullOrWhiteSpace(request.CreatedBy))
			query = query.Where(t => t.CreatedBy == request.CreatedBy);
		if (!string.IsNullOrWhiteSpace(request.UpdatedBy))
			query = query.Where(t => t.UpdatedBy == request.UpdatedBy);
		if (request.FromDateCreated.HasValue)
			query = query.Where(t => t.Created >= request.FromDateCreated.Value);
		if (request.ToDateCreated.HasValue)
			query = query.Where(t => t.Created <= request.ToDateCreated.Value);
		if (request.FromDateUpdated.HasValue)
			query = query.Where(t => t.Updated >= request.FromDateUpdated.Value);
		if (request.ToDateUpdated.HasValue)
			query = query.Where(t => t.Updated <= request.ToDateUpdated.Value);
		if (request.FromDateThoiGianHetHan.HasValue)
			query = query.Where(t => t.ThoiGianHetHan >= request.FromDateThoiGianHetHan.Value);
		if (request.ToDateThoiGianHetHan.HasValue)

			query = query.Where(t => t.ThoiGianHetHan <= request.ToDateThoiGianHetHan.Value);
		if (request.IsHetHan == true)
			query = query.Where(t => t.ThoiGianHetHan < timeNow);
		else if (request.IsHetHan == false) query = query.Where(t => t.ThoiGianHetHan >= timeNow);

		if (!string.IsNullOrWhiteSpace(search)) query = query.Where(x => x.TenPhienBan.Contains(search));

		query = query.OrderByDescending(x => x.Created);
		var response = query.ToPagedListResponse(page, limit);
		response.Items = response.Items.Select(item =>
		{
			if (item.MaKieuDanhMucGocs != null && item.MaKieuDanhMucGocs.Any())
			{
				var tenKieuDMGocs = _context.KieuDanhMuc
								.Where(kdm => item.MaKieuDanhMucGocs.Contains(kdm.MaKieu))
								.Select(kdm => kdm.Ten)
								.ToList();

				item.TenKieuDanhMucGoc = string.Join(", ", tenKieuDMGocs);
			}
			item.Deserialize();
			return item;
		}).ToList();

		return Ok(response);
	}


	//[HttpGet("api/danhmuc/danhmucpublish/getlist-danhmucpublish")]
	//[AllowAnonymous]
	//public IActionResult GetListDanhMucPublish()
	//{
	//	var getphienban = _context.DanhMucPublish.Where(dm => dm.IsPublish == 1 && dm.IsDraft == 0);
	//	foreach (var item in getphienban) item.Deserialize();
	//	return Ok(getphienban);
	//}

	//[HttpGet("api/danhmuc/danhmucpublish/getkieudanhmucs/{id}")]
	//[AllowAnonymous]
	//public IActionResult GetKieuDanhMucPublishs(long id)
	//{
	//	if (id <= 0) return BadRequest("Id_required");

	//	var danhMucPublish = _context.DanhMucPublish.Find(id);
	//	if (danhMucPublish == null) return NotFound("Id_invalid");

	//	if (danhMucPublish.MaKieuDanhMucs == null) return NotFound("Dữ liệu rỗng");

	//	var kieuDanhMucQuery = _context.KieuDanhMuc.Where(kdm => danhMucPublish.MaKieuDanhMucs.Contains(kdm.Ma));
	//	foreach (var item in kieuDanhMucQuery) item.Deserialize();
	//	return Ok(new { Data = kieuDanhMucQuery });
	//}

	[HttpGet("api/danhmuc/danhmucpublish/publishdanhmucs/{id}")]
	public IActionResult PublishDanhMucs(long id)
	{
		if (id <= 0) return BadRequest("Id_required");
		var danhMucPublish = _context.DanhMucPublish.Find(id);
		if (danhMucPublish == null) return NotFound("Id_invalid");

		if (danhMucPublish.IsPublish == 1) return BadRequest("Phiên bản đã được phê duyệt");
		danhMucPublish.IsPublish = 1;
		danhMucPublish.IsDraft = 0;
		danhMucPublish.Published = Utils.DateTimeToUnixTimeStamp(DateTime.Now);
		_context.Update(danhMucPublish);
		_context.SaveChanges();
		return Ok("Phê duyệt phiên bản thành công");
	}
}