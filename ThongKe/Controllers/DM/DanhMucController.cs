//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using System.Collections.Generic;
//using ThongKe.Data;
//using ThongKe.DTOs;
//using ThongKe.Entities;
//using ThongKe.Shared;
//using ThongKe.Shared.Extensions;
//using ThongKe.Shared.Utils;
//using static ThongKe.Shared.Utils.Utils;

//namespace ThongKe.Controllers.DM;

//[Authorize]
//[ApiController]
//public class DanhMucController : ControllerBase
//{
//	private readonly AppDbContext _context;
//	private readonly IHttpContextAccessor _httpContextAccessor;

//	public DanhMucController(AppDbContext context, IHttpContextAccessor httpContextAccessor)
//	{
//		_context = context;
//		_httpContextAccessor = httpContextAccessor;
//	}

//	/// <summary>
//	///     Thêm hoặc cập nhật
//	///     <route>POST: api/danhmuc/danhmuc-upsert</route>
//	/// </summary>
//	[HttpPost("api/danhmuc/danhmuc-upsert")]
//	public IActionResult Upsert_DanhMuc(DanhMuc request)
//	{
//		if (request == null || string.IsNullOrEmpty(request.MaKieu) || string.IsNullOrEmpty(request.MaMuc))
//			return StatusCode(400, "request_or_MaKieu_or_MaMuc_required");

//		//check MaKieu and MaMuc unique
//		var checkExist = _context.DanhMuc.FirstOrDefault(x =>
//				x.MaKieu == request.MaKieu && x.MaMuc == request.MaMuc && x.Id != request.Id);

//		if (checkExist != null) return BadRequest(ApiResponse<string>.Fail(["MaKieu and MaMuc already existed"]));

//		if (request.Id > 0)
//		{
//			var objdb = _context.DanhMuc.Find(request.Id);
//			bool isupdate = PermissionUtil.CanUpdateNghiepVu(_httpContextAccessor, _context, objdb);
//			if (!isupdate)
//			{
//				return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<string>.Fail(["Bạn không có quyền truy cập."]));
//			}
//			if (objdb == null) return BadRequest(ApiResponse<string>.Fail(["Id_invalid"]));
//			objdb.Deserialize();
//			objdb.Assign(request);
//			objdb.Updated = Utils.DateTimeToUnixTimeStamp(DateTime.Now);
//			objdb.Serialize();
//			//TODO updated by
//			_context.DanhMuc.Update(objdb);
//		}
//		else
//		{
//			var resultCreate = PermissionUtil.CanCreateDanhMuc(_httpContextAccessor, _context);
//			if (!resultCreate.CanCreate)
//			{
//				return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<string>.Fail(["Bạn không có quyền truy cập."]));
//			}

//			request.Scope = resultCreate.Scope;
//			request.TinhThanhId = resultCreate.TinhThanhId;
//			request.DonViThongKeId = resultCreate.DonViThongKeId;
//			request.CreatedBy = resultCreate.CreatedBy;

//			//request.Created = Utils.DateTimeToUnixTimeStamp(DateTime.Now);
//			//TODO created by

//			request.Serialize();
//			_context.DanhMuc.Add(request);
//		}

//		if (!string.IsNullOrEmpty(request.ParentMaMuc))
//		{
//			var parent = _context.DanhMuc.FirstOrDefault(x => x.MaMuc == request.ParentMaMuc);
//			if (parent == null) return BadRequest(ApiResponse<string>.Fail(["ParentMaMuc_invalid"]));
//			if (parent.ChildrenMaMucs == null)
//				parent.ChildrenMaMucs = new List<string>();
//			parent.ChildrenMaMucs.Add(request.MaMuc);
//			_context.DanhMuc.Update(parent);
//		}
//		_context.SaveChanges();
//		return Ok(request);
//	}

//	[HttpPost("api/danhmuc/danhmuc-upsertmultil")]
//	public IActionResult UpsertMultil_DanhMuc(List<DanhMuc> requests)
//	{
//		foreach (var request in requests)
//		{
//			if (request == null || string.IsNullOrEmpty(request.MaKieu) || string.IsNullOrEmpty(request.MaMuc))
//				return StatusCode(400, "request_or_MaKieu_or_MaMuc_required");

//			//check MaKieu and MaMuc unique
//			var checkExist = _context.DanhMuc.FirstOrDefault(x =>
//					x.MaKieu == request.MaKieu && x.MaMuc == request.MaMuc && x.Id != request.Id);

//			if (checkExist != null) return BadRequest(ApiResponse<string>.Fail(["MaKieu and MaMuc already existed"]));

//			if (request.Id > 0)
//			{
//				var objdb = _context.DanhMuc.Find(request.Id);
//				bool isupdate = PermissionUtil.CanUpdateNghiepVu(_httpContextAccessor, _context, objdb);
//				if (!isupdate)
//				{
//					return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<string>.Fail(["Bạn không có quyền truy cập."]));
//				}
//				if (objdb == null) return BadRequest(ApiResponse<string>.Fail(["Id_invalid"]));
//				objdb.Deserialize();
//				objdb.Assign(request);
//				objdb.Updated = Utils.DateTimeToUnixTimeStamp(DateTime.Now);
//				objdb.Serialize();
//				//TODO updated by
//				_context.DanhMuc.Update(objdb);
//			}
//			else
//			{
//				var resultCreate = PermissionUtil.CanCreateDanhMuc(_httpContextAccessor, _context);
//				if (!resultCreate.CanCreate)
//				{
//					return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<string>.Fail(["Bạn không có quyền truy cập."]));
//				}

//				request.Scope = resultCreate.Scope;
//				request.TinhThanhId = resultCreate.TinhThanhId;
//				request.DonViThongKeId = resultCreate.DonViThongKeId;
//				request.CreatedBy = resultCreate.CreatedBy;

//				//request.Created = Utils.DateTimeToUnixTimeStamp(DateTime.Now);
//				//TODO created by

//				request.Serialize();
//				_context.DanhMuc.Add(request);
//			}

//			if (!string.IsNullOrEmpty(request.ParentMaMuc))
//			{
//				var parent = _context.DanhMuc.FirstOrDefault(x => x.MaMuc == request.ParentMaMuc);
//				if (parent == null) return BadRequest(ApiResponse<string>.Fail(["ParentMaMuc_invalid"]));
//				if (parent.ChildrenMaMucs == null)
//					parent.ChildrenMaMucs = new List<string>();
//				parent.ChildrenMaMucs.Add(request.MaMuc);
//				_context.DanhMuc.Update(parent);
//			}
//			_context.SaveChanges();
//		}
//		return Ok(requests);
//	}


//	/// <summary>
//	///     Lấy chi tiết theo Id.
//	///     <route>GET: api/danhmuc/danhmuc/{id}</route>
//	///     <param name="id">Id</param>
//	/// </summary>
//	[HttpGet("api/danhmuc/danhmuc/{id}")]
//	public IActionResult Get_DanhMucById(long id)
//	{
//		if (id <= 0)
//			return BadRequest(ApiResponse<string>.Fail(["Id_required"]));
//		var _DbItem = _context.DanhMuc.Find(id);
//		if (_DbItem == null)
//			return BadRequest(ApiResponse<string>.Fail(["Id_invalid"]));
//		bool isRead = PermissionUtil.CanReadNghiepVu(_httpContextAccessor, _context, _DbItem);
//		if (!isRead) return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<string>.Fail(["Bạn không có quyền xem dữ liệu này"]));
//		_DbItem.Deserialize();
//		if (!string.IsNullOrEmpty(_DbItem.MaKieu))
//			_DbItem.KieuDanhMuc = _context.KieuDanhMuc.FirstOrDefault(x => x.Ma == _DbItem.MaKieu);
//		return Ok(_DbItem);
//	}

//	#region đang dùng xóa multiple

//	/// <summary>
//	///     Xóa danh mục theo Id.
//	///     <route>POST: api/danhmuc/danhmuc/delete/{id}</route>
//	///     <param name="id">Id</param>
//	/// </summary>
//	//[HttpPost("api/danhmuc/danhmuc/delete/{id}")]
//	//[AllowAnonymous]
//	//public IActionResult Delete_DanhMucById(Int64 id)
//	//{
//	//	if (id <= 0)
//	//		return StatusCode(400, "Id_required");
//	//	var _DbItem = _context.DanhMuc.Find(id);
//	//	if (_DbItem == null)
//	//		return StatusCode(404, "Id_invalid");
//	//	_context.DanhMuc.Remove(_DbItem);
//	//	_context.SaveChanges();
//	//	return Ok("Xóa thành công");
//	//}

//	#endregion

//	// xóa hàng loạt danh mục
//	[HttpPost("api/danhmuc/danhmuc/delete-multiple")]
//	public IActionResult Delete_MultipleDanhMuc([FromBody] List<long> ids)
//	{
//		if (ids == null || ids.Count == 0)
//			//return StatusCode(400, "Ids_required");
//			return BadRequest(ApiResponse<string>.Fail(["Ids_required"]));
//		var items = _context.DanhMuc.Where(x => ids.Contains(x.Id)).ToList();
//		if (items.Count == 0)
//			return BadRequest(ApiResponse<string>.Fail(["No_valid_Id_found"]));
//		foreach (var item in items)
//		{
//			bool isuDelete = PermissionUtil.CanDeleteNghiepVu(_httpContextAccessor, _context, item);
//			if (!isuDelete)
//			{
//				return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<string>.Fail(["Bạn không có quyền truy cập."]));
//			}
//		}
//		_context.DanhMuc.RemoveRange(items);
//		_context.SaveChanges();
//		return Ok("Xóa thành công");
//	}

//	/// <summary>
//	///     Lấy danh sách (phân trang, tìm kiếm).
//	///     <route>POST: api/danhmuc/danhmucs-list</route>
//	///     <param name="request">page, limit, search</param>
//	///     <returns>PagedListResponse<DanhMuc></returns>
//	/// </summary>
//	[HttpPost("api/danhmuc/danhmucs-list")]
//	public IActionResult Get_DanhMucList(PagedListRequestDanhMuc request)
//	{
//		if (request == null)
//			return BadRequest(ApiResponse<string>.Fail(["request_required"]));
//		var page = request?.Page > 0 ? request.Page : 1;
//		var limit = (request?.Limit ?? 0) > 0 ? request.Limit : int.MaxValue;
//		var search = request?.Search;
//		var getAll = _context.DanhMuc.AsQueryable();

//		var query = PermissionUtil.ApplyDataFilterNghiepVu(_httpContextAccessor, _context, getAll);

//		if (request.MaKieus != null && request.MaKieus.Count > 0)
//			query = query.Where(t => request.MaKieus.Contains(t.MaKieu));
//		//if (!string.IsNullOrWhiteSpace(request.TrangThai))
//		//	query = query.Where(t => t.TrangThai == request.TrangThai);
//		if (!string.IsNullOrWhiteSpace(request.CreatedBy))
//			query = query.Where(t => t.CreatedBy == request.CreatedBy);
//		if (!string.IsNullOrWhiteSpace(request.UpdatedBy))
//			query = query.Where(t => t.UpdatedBy == request.UpdatedBy);
//		if (request.FromDateCreated.HasValue)
//			query = query.Where(t => t.Created >= request.FromDateCreated.Value);
//		if (request.ToDateCreated.HasValue)
//			query = query.Where(t => t.Created <= request.ToDateCreated.Value);
//		if (request.FromDateUpdated.HasValue)
//			query = query.Where(t => t.Updated >= request.FromDateUpdated.Value);
//		if (request.ToDateUpdated.HasValue)
//			query = query.Where(t => t.Updated <= request.ToDateUpdated.Value);

//		if (!string.IsNullOrWhiteSpace(search))
//			query = query.Where(x => x.TenMuc.Contains(search) || x.MaMuc.Contains(search));

//		query = query.OrderByDescending(x => x.Created).ThenByDescending(x => x.Id);
//		var response = query.ToPagedListResponse(page, limit);

//		if (response.Items != null && response.Items.Count > 0 && request.IsDataKieuDanhMuc == true)
//			foreach (var item in response.Items)
//				if (!string.IsNullOrEmpty(item.MaKieu))
//					item.KieuDanhMuc = _context.KieuDanhMuc.FirstOrDefault(x => x.Ma == item.MaKieu);

//		return Ok(response);
//	}

//	[HttpPost("api/danhmuc/getdanhmuccon")]
//	public IActionResult GetDanhMucCon(RequestDanhMucCon request)
//	{
//		if (request == null || string.IsNullOrEmpty(request.MaKieu))
//			return BadRequest(ApiResponse<string>.Fail(["request_MaKieu_required_Or_request_require_"]));

//		var page = request.Page > 0 ? request.Page : 1;
//		var limit = (request?.Limit ?? 0) > 0 ? request.Limit : int.MaxValue;
//		var getAll = _context.DanhMuc.AsQueryable();

//		var query = PermissionUtil.ApplyDataFilterNghiepVu(_httpContextAccessor, _context, getAll);

//		// Lấy danh sách danh mục cha theo MaKieu
//		var danhMucChaQuery = query.Where(x => x.MaKieu == request.MaKieu);

//		List<DanhMuc> danhmuc;
//		if (!string.IsNullOrEmpty(request.MaMuc))
//		{
//			danhmuc = danhMucChaQuery.Where(x => x.MaMuc == request.MaMuc).ToList();
//		}
//		else
//		{
//			danhmuc = danhMucChaQuery.ToList();
//		}

//		if (danhmuc == null || danhmuc.Count == 0)
//			return StatusCode(400, "không tìm thấy dữ liệu");

//		var maxLevel = 2;
//		var maxLevelProp = request.GetType().GetProperty("MaxLevel");
//		if (maxLevelProp != null)
//		{
//			var val = maxLevelProp.GetValue(request);
//			if (val is int v && v > 0) maxLevel = v;
//		}

//		// Phân trang danh mục cha
//		var totalRecords = danhmuc.Count;
//		var pagedDanhMuc = danhmuc.Skip((page - 1) * limit).Take(limit).ToList();

//		// Build tree cho từng danh mục cha đã phân trang
//		var allDanhMucs = query.ToList();
//		var result = pagedDanhMuc.Select(dm => BuildTreeDanhMuc(dm, allDanhMucs, 1, maxLevel)).ToList();

//		// Trả về kết quả phân trang
//		return Ok(result);
//	}

//	private object BuildTreeDanhMuc(DanhMuc parent, List<DanhMuc> allDanhMucs, int level, int maxLevel)
//	{
//		if (level > maxLevel) return null;
//		var children = new List<object>();
//		if (parent.ChildrenMaMucs != null && parent.ChildrenMaMucs.Count > 0)
//			foreach (var maMuc in parent.ChildrenMaMucs)
//			{
//				var child = allDanhMucs.FirstOrDefault(x => x.MaMuc == maMuc);
//				if (child != null)
//				{
//					var childNode = BuildTreeDanhMuc(child, allDanhMucs, level + 1, maxLevel);
//					if (childNode != null)
//						children.Add(childNode);
//				}
//			}

//		return new
//		{
//			parent.Id,
//			parent.TenMuc,
//			parent.MaMuc,
//			parent.MaKieu,
//			parent.KyHieu,
//			parent.MoTa,
//			parent.Stt,
//			parent.Nguon,
//			parent.ParentMaKieu,
//			parent.ParentMaMuc,
//			parent.DanhSachs,
//			//parent.TrangThai,
//			ChildrenDanhMucs = children
//		};
//	}

//	//// chỉ tiêu
//	//[HttpPost("api/danhmuc/danhmucs-kieudanhmuc")]
//	//[AllowAnonymous]
//	//public IActionResult GetDanhMucByKieuDanhMuc(RequestDanhMucByKieuDanhMuc request)
//	//{
//	//	if (request == null || string.IsNullOrEmpty(request.KieuDanhMuc))
//	//	{
//	//		return StatusCode(400, "request_or_KieuDanhMuc_required");
//	//	}
//	//	var query = _context.DanhMuc.Where(x => x.MaKieu == request.KieuDanhMuc);

//	//	var response = query.Select(item => new
//	//	{
//	//		Id = item.Id,
//	//		TenMuc = item.TenMuc
//	//	}).ToList();
//	//	return Ok(response);
//	//}

//	//public class RequestDanhMucByKieuDanhMuc
//	//{
//	//	public required string KieuDanhMuc { get; set; }
//	//}

//	//[HttpPost("api/danhmuc/get")]
//	//[AllowAnonymous]
//	//public IActionResult Get_DanhMucListForKieuDanhMucList(List<string> MaKieu)
//	//{
//	//	var query = _context.DanhMuc.Where(x => MaKieu.Contains(x.MaKieu)).ToList();
//	//	return Ok(query);
//	//}
//}