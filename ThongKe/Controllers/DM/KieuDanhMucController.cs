using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using System.Xml.Linq;
using ThongKe.Data;
using ThongKe.DTOs;
using ThongKe.Entities;
using ThongKe.Shared;
using ThongKe.Shared.Extensions;
using ThongKe.Shared.Utils;
using static ThongKe.Shared.Utils.Utils;

namespace ThongKe.Controllers.DM;

[Authorize]
[ApiController]
public class KieuDanhMucController(
		AppDbContext context,
		IConfiguration configuration,
		IHttpContextAccessor httpContextAccessor)
		: ControllerBase
{

	/// <summary>
	/// Tạo mới 1 cây danh mục hoặc cập nhật cây danh mục
	/// </summary>
	/// <param name="request"></param>
	/// <returns></returns>
	[HttpPost("api/danhmuc/kieudanhmuc-upsert")]
	public IActionResult Upsert_KieuDanhMuc(KieuDanhMuc request)
	{
		if (request == null || string.IsNullOrEmpty(request.Ten)) return StatusCode(StatusCodes.Status400BadRequest, "request_or_Ten_required");
		
		GenerateMaKieuRecursive(request);

		if (!string.IsNullOrEmpty(request.MaKieu))
		{
			var existed = context.KieuDanhMuc.FirstOrDefault(x => x.MaKieu == request.MaKieu && x.Id != request.Id);
			if (existed != null)
				return Conflict(ApiResponse<string>.Fail(["Ma_already_exists"]));
		}

		// Validate maKieu trong cây không được trùng lặp
		var duplicateMaKieuError = ValidateDuplicateMaKieuInTree(request);
		if (!string.IsNullOrEmpty(duplicateMaKieuError))
		{
			return Conflict(ApiResponse<string>.Fail([duplicateMaKieuError]));
		}

		// Validate maMuc trong node không được trùng lặp
		var duplicateError = ValidateDuplicateMaMucInNodes(request);
		if (!string.IsNullOrEmpty(duplicateError))
		{
			return Conflict(ApiResponse<string>.Fail([duplicateError]));
		}

		// update
		if (request.Id > 0)
		{
			var existed = context.KieuDanhMuc.Find(request.Id);
			if (existed == null)
				return StatusCode(StatusCodes.Status400BadRequest, "Id_invalid");

			existed.Assign(request);
			existed.Serialize();
			existed.Updated = Utils.DateTimeToUnixTimeStamp(DateTime.Now);
			context.KieuDanhMuc.Update(existed);
		}
		//create
		else
		{
			var kieuDanhMuc = new KieuDanhMuc();
			kieuDanhMuc.Assign(request);
			kieuDanhMuc.Updated = Utils.DateTimeToUnixTimeStamp(DateTime.Now);
			kieuDanhMuc.Serialize(); // Chuyển các list sang JSON
			context.Add(kieuDanhMuc);
		}
		context.SaveChanges();
		return StatusCode(StatusCodes.Status200OK);
	}

	private string ValidateDuplicateMaKieuInTree(KieuDanhMuc rootNode)
	{
		if (rootNode == null) return string.Empty;

		var allMaKieus = new List<string>();
		CollectAllMaKieusFromTree(rootNode, allMaKieus);

		// Remove null or empty values
		var validMaKieus = allMaKieus.Where(mk => !string.IsNullOrEmpty(mk)).ToList();

		// Check for duplicates
		var duplicates = validMaKieus
			.GroupBy(mk => mk)
			.Where(g => g.Count() > 1)
			.Select(g => g.Key)
			.ToList();

		if (duplicates.Any())
		{
			return $"Trùng lặp MaKieu trong cây: {string.Join(", ", duplicates)}";
		}

		return string.Empty;
	}

	// hàm lấy tất cả maKieu trong cây
	private void CollectAllMaKieusFromTree(KieuDanhMuc node, List<string> maKieusList)
	{
		if (node == null) return;

		if (!string.IsNullOrEmpty(node.MaKieu))
		{
			maKieusList.Add(node.MaKieu);
		}

		// Recursively collect from children
		if (node.ChildrenKieuDanhMucs != null && node.ChildrenKieuDanhMucs.Count > 0)
		{
			foreach (var child in node.ChildrenKieuDanhMucs)
			{
				CollectAllMaKieusFromTree(child, maKieusList);
			}
		}
	}

	// check validate MaMuc trong Node không trùng
	private string ValidateDuplicateMaMucInNodes(KieuDanhMuc node)
	{
		if (node == null) return string.Empty;

		// Kiểm tra trùng lặp MaMuc trong node hiện tại
		if (node.DanhMucs != null && node.DanhMucs.Count > 0)
		{
			var duplicates = node.DanhMucs
				.Where(dm => !string.IsNullOrEmpty(dm.MaMuc))
				.GroupBy(dm => dm.MaMuc)
				.Where(g => g.Count() > 1)
				.Select(g => g.Key)
				.ToList();

			if (duplicates.Any())
			{
				var nodeName = !string.IsNullOrEmpty(node.Ten) ? node.Ten : "Unknown";
				return $"Trùng lặp MaMuc trong node '{nodeName}': {string.Join(", ", duplicates)}";
			}
		}

		// Duyệt qua các node con
		if (node.ChildrenKieuDanhMucs != null && node.ChildrenKieuDanhMucs.Count > 0)
		{
			foreach (var child in node.ChildrenKieuDanhMucs)
			{
				var childError = ValidateDuplicateMaMucInNodes(child);
				if (!string.IsNullOrEmpty(childError))
				{
					return childError;
				}
			}
		}

		return string.Empty;
	}

	private void GenerateMaKieuRecursive(KieuDanhMuc node)
	{
		if (node == null) return;

		// Tạo mã cho node hiện tại
		var key = Utils.StringVNToKey(node.Ten ?? "");
		node.MaKieu = key;

		// Sinh mã cho các danh mục con (nếu có)
		if (node.ChildrenKieuDanhMucs != null && node.ChildrenKieuDanhMucs.Count > 0)
		{
			foreach (var child in node.ChildrenKieuDanhMucs)
			{
				GenerateMaKieuRecursive(child);
			}
		}
	}

	///// <summary>
	/////     Mở hoặc đóng kiểu danh mục
	/////     <route>POST: api/danhmuc/kieudanhmuc/openorclose</route>
	///// </summary>
	[HttpPost("api/danhmuc/kieudanhmuc/openorclose")]
	public IActionResult OpenOrClose_KieuDanhMuc(RequestOpenCloseKieuDanhMuc request)
	{
		if (request.Id <= 0)
			return StatusCode(400, "Id_required");

		var kieuDanhMuc = context.KieuDanhMuc.Find(request.Id);
		if (kieuDanhMuc == null)
			return StatusCode(404, "Id_invalid");

		//bool isuCreate = PermissionUtil.CanUpdateNghiepVu(httpContextAccessor, context, kieuDanhMuc);
		//if (!isuCreate)
		//{
		//	return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<string>.Fail(["Bạn không có quyền truy cập."]));
		//}

		if (request.IsOpen)
		{
			if (kieuDanhMuc.IsOpen == 1 && kieuDanhMuc.IsClosed == 0) return Ok("Kiểu danh mục đã ở trạng thái mở");
			kieuDanhMuc.IsOpen = 1;
			kieuDanhMuc.IsClosed = 0;
		}
		else
		{
			if (kieuDanhMuc.IsOpen == 0 && kieuDanhMuc.IsClosed == 1) return Ok("Kiểu danh mục đã ở trạng thái đóng");
			kieuDanhMuc.IsOpen = 0;
			kieuDanhMuc.IsClosed = 1;
		}

		kieuDanhMuc.Updated = Utils.DateTimeToUnixTimeStamp(DateTime.Now);
		context.KieuDanhMuc.Update(kieuDanhMuc);
		context.SaveChanges();

		return Ok("Cập nhật trạng thái thành công");
	}

	///// <summary>
	/////     Lấy chi tiết theo Id.
	/////     <route>GET: api/danhmuc/kieudanhmuc/{id}</route>
	/////     <param name="id">Id</param>
	///// </summary>
	[HttpGet("api/danhmuc/kieudanhmuc/{id}")]
	public IActionResult Get_KieuDanhMucById(long id)
	{
		if (id <= 0)
			return StatusCode(StatusCodes.Status400BadRequest, ApiResponse<string>.Fail(["Id_required"]));
		var _DbItem = context.KieuDanhMuc.Find(id);
		if (_DbItem == null)
			return StatusCode(StatusCodes.Status404NotFound, ApiResponse<string>.Fail(["Id_invalid"]));

		//bool isuCreate = PermissionUtil.CanReadNghiepVu(httpContextAccessor, context, _DbItem);
		//if (!isuCreate)
		//{
		//	return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<string>.Fail(["Bạn không có quyền truy cập."]));
		//}

		_DbItem.Deserialize();
		_DbItem.TotalDanhMuc = TongDanhMucMoiNode(_DbItem);

		return Ok(_DbItem);
	}

	/// <summary>
	/// Xóa danh mục theo Id và các tham số khác.
	/// </summary>
	/// <param name="request"></param>
	/// <returns></returns>
	[HttpPost("api/danhmuc/kieudanhmuc/delete")]
	public IActionResult Delete_KieuDanhMuc(DeleteDanhMuc request)
	{
		// trường hợp xóa tất cả
		if (request.Id > 0)
		{
			var getId = context.KieuDanhMuc.Find(request.Id);
			if (getId == null)
				return StatusCode(StatusCodes.Status404NotFound, ApiResponse<string>.Fail(["Id_invalid"]));
			getId.Deserialize();

			if (request.DeleteAll)
			{
				context.KieuDanhMuc.Remove(getId);
			}

			// trường hợp chỉ xóa node kiểu danh mục
			if (!string.IsNullOrEmpty(request.MaKieu) && (request.MaMuc == null || request.MaMuc.Count == 0))
			{
				// tìm và xóa node makieu
				var result = RemoveNodesFromTree(getId, request.MaKieu);
				if (result == false)
				{
					return StatusCode(StatusCodes.Status404NotFound, ApiResponse<string>.Fail(["MaKieu_not_found"]));
				}
				getId.Serialize();
				context.KieuDanhMuc.Update(getId);
			}

			if ((request.MaMuc != null && request.MaMuc.Count > 0) && !string.IsNullOrEmpty(request.MaKieu))
			{
				// Tìm node có MaKieu tương ứng
				var targetNode = DanhMucUtils.FindNodeByMaKieu(getId, request.MaKieu);
				if (targetNode == null)
				{
					return StatusCode(StatusCodes.Status404NotFound, ApiResponse<string>.Fail(["MaKieu_not_found"]));
				}

				// Tìm và xóa DanhMuc trong node đó
				var danhMucsToRemove = targetNode.DanhMucs?.Where(dm => request.MaMuc.Contains(dm.MaMuc)).ToList();

				if (danhMucsToRemove == null || danhMucsToRemove.Count == 0)
				{
					return StatusCode(StatusCodes.Status404NotFound, ApiResponse<string>.Fail(["MaMuc_not_found"]));
				}

				targetNode.DanhMucs?.RemoveAll(dm => danhMucsToRemove.Any(r => r.MaMuc == dm.MaMuc));

				getId.Serialize();
				context.KieuDanhMuc.Update(getId);
			}
			context.SaveChanges();
			return StatusCode(StatusCodes.Status200OK);
		}
		else
		{
			return StatusCode(StatusCodes.Status400BadRequest, ApiResponse<string>.Fail(["Id_required"]));
		}
	}

	private List<KieuDanhMuc> FindNodesByMaKieu(KieuDanhMuc rootNode, string maKieu)
	{
		var foundNodes = new List<KieuDanhMuc>();

		if (rootNode == null || string.IsNullOrEmpty(maKieu))
			return foundNodes;

		// Check if current node matches
		if (rootNode.MaKieu == maKieu)
		{
			foundNodes.Add(rootNode);
		}

		// Recursively search in children
		if (rootNode.ChildrenKieuDanhMucs != null && rootNode.ChildrenKieuDanhMucs.Count > 0)
		{
			foreach (var child in rootNode.ChildrenKieuDanhMucs)
			{
				var childResults = FindNodesByMaKieu(child, maKieu);
				foundNodes.AddRange(childResults);
			}
		}

		return foundNodes;
	}

	private bool RemoveNodesFromTree(KieuDanhMuc rootNode, string maKieu)
	{
		if (rootNode == null || string.IsNullOrEmpty(maKieu))
			return false;

		bool nodeRemoved = false;

		// Check children and remove matching nodes
		if (rootNode.ChildrenKieuDanhMucs != null && rootNode.ChildrenKieuDanhMucs.Count > 0)
		{
			// Remove children that match the MaKieu
			var childrenToRemove = rootNode.ChildrenKieuDanhMucs
				.Where(child => child.MaKieu == maKieu)
				.ToList();

			foreach (var childToRemove in childrenToRemove)
			{
				rootNode.ChildrenKieuDanhMucs.Remove(childToRemove);
				nodeRemoved = true;
			}

			// Recursively remove from remaining children
			var remainingChildren = rootNode.ChildrenKieuDanhMucs.ToList();
			foreach (var child in remainingChildren)
			{
				if (RemoveNodesFromTree(child, maKieu))
				{
					nodeRemoved = true;
				}
			}
		}

		return nodeRemoved;
	}

	/// <summary>
	/// Lấy danh sách cây danh mục có phân trang và lọc
	/// </summary>
	/// <param name="request"></param>
	/// <returns></returns>
	[HttpPost("api/danhmuc/kieudanhmucs-list")]
	public IActionResult Get_KieuDanhMucList(PagedListRequestKieuDanhMuc request)
	{
		if (request == null)
			return StatusCode(StatusCodes.Status400BadRequest, "request_required");
		var page = request?.Page > 0 ? request.Page : 1;
		var limit = (request?.Limit ?? 0) > 0 ? request.Limit : int.MaxValue;
		var search = request?.Search;
		var query = context.KieuDanhMuc.AsQueryable();


		if(request.MaKieus != null && request.MaKieus.Count > 0)
		{
			query = query.Where(t => request.MaKieus.Contains(t.MaKieu));
		}	
		if (request.IsOpenFilter.HasValue)
		{
			if (request.IsOpenFilter.Value)
				query = query.Where(t => t.IsOpen == 1 && t.IsClosed == 0);
			else
				query = query.Where(t => t.IsOpen == 0 && t.IsClosed == 1);
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
		if (!string.IsNullOrWhiteSpace(search))
			query = query.Where(x => x.Ten.Contains(search) || x.NhomDanhMuc.Contains(search));
		query = query.OrderByDescending(x => x.Created);
		var response = query.ToPagedListResponse(page, limit);
		foreach (var item in response.Items)
		{
			item.Deserialize();
			item.TotalDanhMuc = TongDanhMucMoiNode(item);
		}
		return StatusCode(StatusCodes.Status200OK, response);
	}

	// tính count danh mục mỗi node
	private int TongDanhMucMoiNode(KieuDanhMuc node)
	{
		if (node == null)
			return 0;

		int total = node.DanhMucs != null ? node.DanhMucs.Count : 0;

		if (node.ChildrenKieuDanhMucs != null && node.ChildrenKieuDanhMucs.Count > 0)
		{
			foreach (var child in node.ChildrenKieuDanhMucs)
			{
				// Đệ quy xuống từng con và gán kết quả vào từng node con luôn
				child.TotalDanhMuc = TongDanhMucMoiNode(child);
			}
		}
		return total;
	}


	//// xóa kiểu danh mục hàng loạt
	//[HttpPost("api/danhmuc/kieudanhmuc-delete-multiple")]
	//public IActionResult Delete_KieuDanhMuc_Multiple([FromBody] List<long> ids)
	//{
	//	if (ids == null || ids.Count == 0)
	//		return StatusCode(StatusCodes.Status400BadRequest, ApiResponse<string>.Fail(["Ids_required"]));
	//	// kiểm tra có danh mục nào đang sử dụng kiểu danh mục này không
	//	var kieuMas = context.KieuDanhMuc.Where(x => ids.Contains(x.Id)).Select(dm => dm.Ma).ToList();
	//	var usedKieuMas = context.DanhMuc.Where(dm => kieuMas.Contains(dm.MaKieu)).Select(dm => dm.MaKieu).Distinct()
	//			.ToList();
	//	if (usedKieuMas.Count > 0)
	//		return StatusCode(StatusCodes.Status400BadRequest, $"Không thể xóa. Có {usedKieuMas.Count} danh mục được sử dụng ở kiểu danh mục này.");
	//	var listDb = context.KieuDanhMuc.Where(x => ids.Contains(x.Id)).ToList();
	//	if (listDb.Count == 0)
	//		return StatusCode(404, "Ids_invalid");
	//	foreach (var item in listDb)
	//	{
	//		bool isuDelete = PermissionUtil.CanDeleteNghiepVu(httpContextAccessor, context, item);
	//		if (!isuDelete)
	//		{
	//			return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<string>.Fail(["Bạn không có quyền truy cập."]));
	//		}
	//	}
	//	context.KieuDanhMuc.RemoveRange(listDb);
	//	context.SaveChanges();
	//	return Ok("Xóa thành công");
	//}
	//public IActionResult Delete_KieuDanhMuc(DeleteDanhMuc request)
	//{
	//	// trường hợp xóa tất cả
	//	if (request.Id > 0)
	//	{
	//		var getId = context.KieuDanhMuc.Find(request.Id);
	//		if (getId == null)
	//			return StatusCode(StatusCodes.Status404NotFound, ApiResponse<string>.Fail(["Id_invalid"]));
	//		getId.Deserialize();

	//		if (request.DeleteAll)
	//		{
	//			context.KieuDanhMuc.Remove(getId);
	//		}

	//		// trường hợp chỉ xóa node kiểu danh mục
	//		if (!string.IsNullOrEmpty(request.MaKieu) && string.IsNullOrEmpty(request.MaMuc))
	//		{
	//			var nodesToRemove = FindNodeByMaKieu(getId, request.MaKieu);
	//			if (nodesToRemove.Count == 0)
	//			{
	//				return StatusCode(StatusCodes.Status404NotFound, ApiResponse<string>.Fail(["MaKieu_not_found"]));
	//			}


	//			RemoveNodesFromTree(getId, request.MaKieu);
	//			getId.Serialize();
	//			context.KieuDanhMuc.Update(getId);
	//		}
	//		if (!string.IsNullOrEmpty(request.MaMuc) && !string.IsNullOrEmpty(request.MaKieu))
	//		{
	//			// Tìm node có MaKieu tương ứng
	//			var targetNode = FindNodeByMaKieu(getId, request.MaKieu);
	//			if (targetNode == null)
	//			{
	//				return StatusCode(StatusCodes.Status404NotFound, ApiResponse<string>.Fail(["MaKieu_not_found"]));
	//			}

	//			// Tìm và xóa DanhMuc trong node đó
	//			var danhMucsToRemove = targetNode.DanhMucs?.Where(dm => dm.MaMuc == request.MaMuc).ToList();
	//			if (danhMucsToRemove == null || danhMucsToRemove.Count == 0)
	//			{
	//				return StatusCode(StatusCodes.Status404NotFound, ApiResponse<string>.Fail(["MaMuc_not_found"]));
	//			}

	//			foreach (var dm in danhMucsToRemove)
	//			{
	//				targetNode.DanhMucs!.Remove(dm);
	//			}

	//			getId.Serialize();
	//			context.KieuDanhMuc.Update(getId);
	//		}
	//		context.SaveChanges();
	//		return StatusCode(StatusCodes.Status200OK);
	//	}
	//	else
	//	{
	//		return StatusCode(StatusCodes.Status400BadRequest, ApiResponse<string>.Fail(["Id_required"]));
	//	}
	//}

	// tìm 1 node theo mã kiểu


	/// <summary>
	///     Lấy danh sách (phân trang, tìm kiếm).
	///     <route>POST: api/danhmuc/kieudanhmucs-list</route>
	///     <param name="request">page, limit, search</param>
	///     <returns>PagedListResponse<KieuDanhMuc></returns>
	/// </summary>
	/// 

	/// <summary>
	///     Thống kê tóm tắt danh sách kiểu/mẫu danh mục
	///     <route>POST: api/danhmuc/kieudanhmuc-tomtatlist</route>
	///     ///
	///     <param name="request">page, limit, isDanhMucData, isNhomDanhMuc, IsTrangThai</param>
	///     <returns>PagedListResponse<KieuDanhMuc></returns>
	/// </summary>
	//[HttpPost("api/danhmuc/kieudanhmuc-tomtatlist")]
	//public IActionResult Get_TomTatList(PagedListRequestKieuDanhMucTomTatList request)
	//{
	//	if (request == null)
	//		return BadRequest("request_required");
	//	var page = request.Page > 0 ? request.Page : 1;
	//	var limit = request.Limit > 0 ? request.Limit : int.MaxValue;
	//	var getAll = context.KieuDanhMuc.AsQueryable();

	//	var query = PermissionUtil.ApplyDataFilterNghiepVu(httpContextAccessor, context, getAll);
	//	query = query.OrderBy(x => x.Id);
	//	var response = query.ToPagedListResponse(page, limit);

	//	object? soLuongDanhMucData = null;
	//	if (request.IsDanhMucData)
	//		soLuongDanhMucData = response.Items.Select(item => new
	//		{
	//			item.Id,
	//			TotalDanhMuc = context.DanhMuc.Count(x => x.MaKieu == item.Ma)
	//		});
	//	// Thống kê theo NhomDanhMuc nếu được yêu cầu
	//	object? soLuongNhomDanhMuc = null;
	//	if (request.IsNhomDanhMuc)
	//		soLuongNhomDanhMuc = response.Items
	//				.GroupBy(x => x.NhomDanhMuc)
	//				.Select(g => new
	//				{
	//					NhomDanhMuc = g.Key,
	//					SoLuong = g.Count()
	//				});

	//	// Thống kê theo TrangThai nếu được yêu cầu
	//	var soLuongTrangThai = response.Items.GroupBy(x => new { x.IsOpen, x.IsClosed }).Select(g => new
	//	{
	//		TrangThai = g.Key.IsOpen == 1 && g.Key.IsClosed == 0 ? "Mở" : "Đóng",
	//		SoLuong = g.Count()
	//	}).ToList();
	//	return Ok(new
	//	{
	//		response.Total,
	//		response.TotalPage,
	//		response.Page,
	//		soLuongDanhMucData,
	//		SoLuongTheoNhomDanhMuc = soLuongNhomDanhMuc,
	//		SoLuongTheoTrangThai = soLuongTrangThai
	//	});
	//}


	///// Thống kê tóm tắt cho màn hình danh sách bản ghi của 1 kiểu/mẫu danh mục
	///// <param name="makieudanhmuc">makieudanhmuc</param>
	///// <route>POST: api/danhmuc/danhmuc-tomttat</route>
	///// </summary>
	//[HttpPost("api/danhmuc/kieudanhmuc-tomttat")]
	//public IActionResult Get_TomTatDanhMuc(string maKieuDanhMuc)
	//{
	//	if (string.IsNullOrWhiteSpace(maKieuDanhMuc))
	//		return StatusCode(StatusCodes.Status400BadRequest, "MaKieu_required");

	//	var getAll = context.DanhMuc.AsQueryable();

	//	var query = PermissionUtil.ApplyDataFilterNghiepVu(httpContextAccessor, context, getAll);

	//	var filteredQuery = query.Where(x => x.MaKieu == maKieuDanhMuc);

	//	if (!filteredQuery.Any()) return NotFound("Không tìm thấy dữ liệu cho mã kiểu danh mục này");

	//	//var resultTrangthai = filteredQuery.GroupBy(x => x.TrangThai).Select(g => new
	//	//{
	//	//	TrangThai = g.Key,
	//	//	SoLuong = g.Count()
	//	//}).ToList();

	//	var resultNguon = filteredQuery.GroupBy(x => x.Nguon).Select(g => new
	//	{
	//		Nguon = g.Key,
	//		SoLuong = g.Count()
	//	}).ToList();
	//	var response = new
	//	{
	//		//SoLuongTheoTrangThai = resultTrangthai,
	//		SoLuongTheoNguon = resultNguon
	//	};

	//	return Ok(response);
	//}


	///// ///
	///// <summary>
	/////     API clone 1 kiểu/mẫu danh mục và danh sách bản ghi của nó
	/////     <route>POST: api/danhmuc/kieudanhmuc-saochep</route>
	///// </summary>
	//[HttpPost("api/danhmuc/kieudanhmuc-saochep")]
	//public IActionResult Clone_KieuDanhMucAndData(RequestCloneDanhMuc dataSend)
	//{
	//	if (string.IsNullOrEmpty(dataSend.MaKieu)) return BadRequest("MaKieu_required");
	//	var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
	//	var textClone = $" Sao chép {timestamp}";
	//	var maKieuClone = "";
	//	KieuDanhMuc? clonedKieuDanhMuc = null;
	//	List<DanhMuc>? clonedDanhMucList = null;

	//	// KieuDanhMuc
	//	if (!dataSend.IsCloneDanhMucData && !dataSend.IsCloneKieuDanhMuc)
	//		return BadRequest("Ít nhất một trong hai giá trị IsCloneDanhMucData hoặc IsCloneKieuDanhMuc phải true");
	//	if (dataSend.IsCloneKieuDanhMuc)
	//	{
	//		var kieuDanhMuc = context.KieuDanhMuc.FirstOrDefault(x => x.Ma == dataSend.MaKieu);
	//		if (kieuDanhMuc == null) return NotFound("Không tìm thấy kiểu danh mục");

	//		kieuDanhMuc.Deserialize();

	//		// 2. Clone KieuDanhMuc
	//		clonedKieuDanhMuc = new KieuDanhMuc
	//		{
	//			Ten = kieuDanhMuc.Ten + $"{textClone}",
	//			MoTa = kieuDanhMuc.MoTa,
	//			Format = kieuDanhMuc.Format,
	//			NhomDanhMuc = kieuDanhMuc.NhomDanhMuc,
	//			ParentMa = kieuDanhMuc.ParentMa != null ? kieuDanhMuc.ParentMa + Utils.StringVNToKey(textClone) : null,
	//			TruongDuLieus = kieuDanhMuc.TruongDuLieus,
	//			IsOpen = 1,
	//			IsClosed = 0,
	//			Created = Utils.DateTimeToUnixTimeStamp(DateTime.Now),
	//			Scope = kieuDanhMuc.Scope,
	//			TinhThanhId = kieuDanhMuc.TinhThanhId,
	//			DonViThongKeId = kieuDanhMuc.DonViThongKeId,
	//			CreatedBy = kieuDanhMuc.CreatedBy
	//			//Updated = Utils.DateTimeToUnixTimeStamp(DateTime.Now),
	//			//UpdatedBy = originalKieuDanhMuc.UpdatedBy
	//		};

	//		clonedKieuDanhMuc.Ma = Utils.StringVNToKey(clonedKieuDanhMuc.Ten);
	//		maKieuClone = clonedKieuDanhMuc.Ma;
	//		clonedKieuDanhMuc.Serialize();

	//		var resultCreate = PermissionUtil.CanCreateNghiepVu(httpContextAccessor, context);
	//		if (!resultCreate.CanCreate)
	//		{
	//			return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<string>.Fail(["Bạn không có quyền truy cập."]));
	//		}
	//		clonedKieuDanhMuc.DonViThongKeId = resultCreate.DonViThongKeId;
	//		clonedKieuDanhMuc.TinhThanhId = resultCreate.TinhThanhId;
	//		clonedKieuDanhMuc.Scope = resultCreate.Scope;
	//		clonedKieuDanhMuc.CreatedBy = resultCreate.CreatedBy;

	//		context.KieuDanhMuc.Add(clonedKieuDanhMuc);
	//		context.SaveChanges();
	//	}

	//	if (dataSend.IsCloneDanhMucData)
	//	{
	//		// 3. Clone DanhMuc list
	//		var danhMucList = context.DanhMuc.Where(x => x.MaKieu == dataSend.MaKieu).ToList();
	//		if (danhMucList.Any())
	//		{
	//			clonedDanhMucList = danhMucList.Select(dm =>
	//			{
	//				dm.Deserialize();
	//				var newTenMuc = dm.TenMuc + textClone;
	//				var newMaKieu = dm.MaKieu + textClone;
	//				var newItem = new DanhMuc
	//				{
	//					TenMuc = newTenMuc,
	//					MaMuc = Utils.StringVNToKey(newTenMuc),
	//					MaKieu = Utils.StringVNToKey(newMaKieu),
	//					MoTa = dm.MoTa,
	//					//TrangThai = dm.TrangThai,
	//					Nguon = dm.Nguon,
	//					ParentMaKieu =
	//												dm.ParentMaKieu != null ? dm.ParentMaKieu + Utils.StringVNToKey(textClone) : null,
	//					ParentMaMuc = dm.ParentMaMuc != null ? dm.ParentMaMuc + Utils.StringVNToKey(textClone) : null,
	//					Stt = dm.Stt,
	//					DanhSachs = dm.DanhSachs,
	//					ChildrenMaMucs = dm.ChildrenMaMucs?.Select(ma => ma + Utils.StringVNToKey(textClone)).ToList(),
	//					ChildrenMaKieu = dm.ChildrenMaKieu != null
	//												? dm.ChildrenMaKieu + Utils.StringVNToKey(textClone)
	//												: null,
	//					Created = Utils.DateTimeToUnixTimeStamp(DateTime.Now),
	//				};
	//				newItem.Serialize();
	//				return newItem;
	//			}).ToList();

	//			var resultCreate = PermissionUtil.CanCreateNghiepVu(httpContextAccessor, context);
	//			if (!resultCreate.CanCreate)
	//			{
	//				return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<string>.Fail(["Bạn không có quyền truy cập."]));
	//			}
	//			foreach (var dm in clonedDanhMucList)
	//			{
	//				dm.DonViThongKeId = resultCreate.DonViThongKeId;
	//				dm.TinhThanhId = resultCreate.TinhThanhId;
	//				dm.Scope = resultCreate.Scope;
	//				dm.CreatedBy = resultCreate.CreatedBy;
	//			}
	//			context.DanhMuc.AddRange(clonedDanhMucList);
	//			context.SaveChanges();
	//		}
	//		else
	//		{
	//			if (!dataSend.IsCloneKieuDanhMuc)
	//				return NotFound("Không tìm thấy dữ liệu danh mục để sao chép");
	//		}
	//	}

	//	var response = new
	//	{
	//		success = true,
	//		message = clonedKieuDanhMuc != null && clonedDanhMucList != null
	//					? $"Đã sao chép kiểu danh mục và {clonedDanhMucList.Count} dữ liệu danh mục"
	//					: clonedKieuDanhMuc != null && !dataSend.IsCloneDanhMucData
	//							? "Đã sao chép kiểu danh mục"
	//							: clonedKieuDanhMuc != null && dataSend.IsCloneDanhMucData
	//									? "Đã sao chép kiểu danh mục, không tìm thấy dữ liệu danh mục để sao chép"
	//									: clonedDanhMucList != null
	//											? $"Đã sao chép {clonedDanhMucList.Count} dữ liệu danh mục"
	//											: "Không có gì được sao chép"
	//	};
	//	return Ok(response);
	//}

	/// <summary>
	///     Thêm hoặc cập nhật
	///     <route>POST: api/danhmuc/kieudanhmuc-upsert</route>
	/// </summary>
	/// 

	//[HttpPost("api/danhmuc/kieudanhmuc-upsert")]
	//public IActionResult Upsert_KieuDanhMuc(KieuDanhMuc request)
	//{
	//	if (request == null || string.IsNullOrEmpty(request.Ten)) return StatusCode(400, "request_or_Ten_required");

	//	request.Ma = Utils.StringVNToKey(request.Ten);

	//	var checkExist = context.KieuDanhMuc.FirstOrDefault(x => x.Ma == request.Ma && x.Id != request.Id);

	//	if (checkExist != null) return StatusCode(400, "Ten_already_exists");

	//	if (request.Id > 0)
	//	{
	//		bool isupdate = PermissionUtil.CanUpdateNghiepVu(httpContextAccessor, context, request);
	//		if (!isupdate)
	//		{
	//			return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<string>.Fail(["Bạn không có quyền truy cập."]));
	//		}

	//		var objdb = context.KieuDanhMuc.Find(request.Id);
	//		if (objdb == null)
	//			return StatusCode(StatusCodes.Status404NotFound, ApiResponse<string>.Fail(["Id_invalid"]));
	//		objdb.Deserialize();
	//		objdb.Assign(request);
	//		objdb.Updated = Utils.DateTimeToUnixTimeStamp(DateTime.Now);
	//		objdb.Serialize();
	//		//TODO updated by
	//		context.KieuDanhMuc.Update(objdb);
	//	}
	//	else
	//	{
	//		var resultCreate = PermissionUtil.CanCreateDanhMuc(httpContextAccessor, context);
	//		if (!resultCreate.CanCreate)
	//		{
	//			return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<string>.Fail(["Bạn không có quyền truy cập."]));
	//		}

	//		var kieuDanhMuc = new KieuDanhMuc();

	//		request.Scope = resultCreate.Scope;
	//		request.TinhThanhId = resultCreate.TinhThanhId;
	//		request.DonViThongKeId = resultCreate.DonViThongKeId;
	//		request.CreatedBy = resultCreate.CreatedBy;

	//		kieuDanhMuc.Assign(request);
	//		request.Serialize();
	//		context.KieuDanhMuc.Add(request);
	//	}

	//	if (!string.IsNullOrEmpty(request.ParentMa))
	//	{
	//		var parent = context.KieuDanhMuc.FirstOrDefault(x => x.Ma == request.ParentMa);
	//		if (parent == null) return StatusCode(400, "ParentMa_invalid");
	//		if (parent.ChildrenMas == null)
	//			parent.ChildrenMas = new List<string>();
	//		parent.ChildrenMas.Add(request.Ma);
	//		context.KieuDanhMuc.Update(parent);
	//	}

	//	context.SaveChanges();
	//	return Ok(request);
	//}

	/// <summary>
	///     Thêm hoặc cập nhật
	///     <route>POST: api/danhmuc/kieudanhmuc-upsert</route>
	/// </summary>
	/// 
}