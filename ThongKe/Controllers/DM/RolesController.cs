//using Microsoft.AspNetCore.Mvc;
//using ThongKe.Data;
//using ThongKe.DTOs;
//using ThongKe.Entities;
//using ThongKe.Shared;

//using ThongKe.Shared.Extensions;
//using ThongKe.Shared.Utils;

//namespace ThongKe.Controllers.DM;

//public class RolesController : ControllerBase
//{
//	private readonly AppDbContext _context;

//	public RolesController(AppDbContext context)
//	{
//		_context = context;
//	}

//	// Thêm, sửa vai trò
//	[HttpPost("api/roles/role-upsert")]
//	public IActionResult UpSert_Role([FromBody] Role request)
//	{
//		if (request == null || string.IsNullOrEmpty(request.Name))
//		{
//			return StatusCode(400, "request_or_Name_required");
//		}
//		//check Name unique
//		//var checkExist = _context.Role.FirstOrDefault(x => x.Name == request.Name && x.Id != request.Id);
//		//if (checkExist != null)
//		//{
//		//	return StatusCode(400, "Name_already_exists");
//		//}
//		var normalizeName = Utils.NormalizeName(request.Name);
//		//check Name unique
//		var checkExist = _context.Role.FirstOrDefault(x => x.NormalizedName == normalizeName && x.Id != request.Id);
//		if (checkExist != null)
//		{
//			return StatusCode(400, "Name_already_exists");
//		}
//		request.NormalizedName = normalizeName;
//		request.IsOpen = 1;
//		request.IsLocked = 0;
//		request.IsStatic = 0;
//		if (request.Id > 0)
//		{
//			var objdb = _context.Role.Find(request.Id);
//			if (objdb == null)
//				return StatusCode(404, "Id_invalid");

//			if (objdb.IsStatic == 1)
//				return StatusCode(400, "Không được sửa vai trò mặc định");
//			objdb.Assign(request);
//			objdb.Updated = Utils.DateTimeToUnixTimeStamp(DateTime.Now);
//			//TODO updated by
//			_context.Role.Update(objdb);
//		}
//		else
//		{
//			request.Created = Utils.DateTimeToUnixTimeStamp(DateTime.Now);
//			//TODO created by
//			_context.Role.Add(request);
//		}
//		_context.SaveChanges();
//		return Ok(request);
//	}

//	// xóa vai trò
//	[HttpGet("api/roles/role-delete/{id}")]
//	public IActionResult Delete_Role(long id)
//	{
//		if (id <= 0)
//			return StatusCode(400, "Id_required");
//		var objdb = _context.Role.Find(id);
//		if (objdb == null)
//			return StatusCode(404, "Id_invalid");
//		if (objdb.IsStatic == 1)
//			return StatusCode(400, "Không được xóa vai trò mặc định");
//		_context.Role.Remove(objdb);
//		_context.SaveChanges();
//		return Ok("Xóa thành công");
//	}

//	// lấy danh sách có phân trang
//	[HttpPost("api/roles/role-pagedlist")]
//	public IActionResult GetAllOrganization([FromBody] PagedListRequestRole request)
//	{
//		if (request == null)
//			return StatusCode(400, "request_required");
//		var page = request?.Page > 0 ? request.Page : 1;
//		var limit = (request?.Limit ?? 0) > 0 ? request.Limit : int.MaxValue;
//		//string? search = request?.Search;

//		var query = _context.Role.AsQueryable();

//		//if (!string.IsNullOrWhiteSpace(request.TrangThai))
//		//	query = query.Where(t => t.Status == request.TrangThai);
//		//if (!string.IsNullOrWhiteSpace(request.CreatedBy))
//		//	query = query.Where(t => t.CreatedBy == request.CreatedBy);
//		//if (!string.IsNullOrWhiteSpace(request.UpdatedBy))
//		//	query = query.Where(t => t.UpdatedBy == request.UpdatedBy);

//		if (request.IsOpenFilter.HasValue)
//		{
//			if (request.IsOpenFilter.Value)
//			{
//				query = query.Where(t => t.IsOpen == 1 && t.IsLocked == 0);
//			}
//			else
//			{
//				query = query.Where(t => t.IsOpen == 0 && t.IsLocked == 1);
//			}
//		}
//		if (request.FromDateCreated.HasValue)
//			query = query.Where(t => t.Created >= request.FromDateCreated.Value);
//		if (request.ToDateCreated.HasValue)
//			query = query.Where(t => t.Created <= request.ToDateCreated.Value);
//		//if (request.FromDateUpdated.HasValue)
//		//	query = query.Where(t => t.Updated >= request.FromDateUpdated.Value);
//		//if (request.ToDateUpdated.HasValue)
//		//	query = query.Where(t => t.Updated <= request.ToDateUpdated.Value);
//		if (!string.IsNullOrWhiteSpace(request.Search))
//		{
//			var keyword = request.Search.Trim().ToLower();

//			query = query.Where(x =>
//					(x.Name ?? "").ToLower().Contains(keyword)
//			);
//		}

//		query = query.OrderByDescending(x => x.Created);
//		var response = query.ToPagedListResponse(page, limit);
//		return Ok(response);
//	}

//	// lấy 1 bản ghi theo id
//	[HttpGet("api/roles/{id}")]
//	public IActionResult GetByIdRole(long id)
//	{
//		if (id <= 0)
//			return StatusCode(400, "Id_required");
//		var getbyId = _context.Role.Find(id);
//		return Ok(getbyId);
//	}

//	[HttpPost("api/roles/lockoropen")]
//	public IActionResult LockRoles([FromBody] RequestLockMultipleRole request)
//	{
//		if (request == null || request.RoleIds == null || request.RoleIds.Count == 0)
//		{
//			return StatusCode(400, "request_or_RoleIds_required");
//		}
//		var roles = _context.Role.Where(x => request.RoleIds.Contains(x.Id)).ToList();
//		if (roles == null || roles.Count == 0)
//		{
//			return StatusCode(404, "No_roles_found");
//		}
//		if (request.IsLocked == true)
//		{
//			foreach (var role in roles)
//			{
//				if (role.IsStatic == 1)
//				{
//					return StatusCode(400, $"Không thể khóa vai trò hệ thống: {role.Name}");
//				}
//				role.IsLocked = 1;
//				role.IsOpen = 0; // khóa
//				_context.Role.Update(role);

//			}
//			_context.SaveChanges();
//			return Ok("Khóa thành công");
//		}
//		else
//		{
//			foreach (var role in roles)
//			{
//				role.IsLocked = 0;
//				role.IsOpen = 1;
//				_context.Role.Update(role);
//			}
//			_context.SaveChanges();
//			return Ok("Mở khóa thành công");
//		}
//	}

//	public class RequestLockMultipleRole
//	{
//		public required List<long> RoleIds { get; set; }
//		public required bool IsLocked { get; set; }
//	}
//}