using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThongKe.Data;
using ThongKe.Entities;
using ThongKe.Shared;
using ThongKe.Shared.Extensions;
using ThongKe.Shared.Utils;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static ThongKe.Shared.Utils.Utils;

namespace ThongKe.Controllers.DM;

[Authorize]
[ApiController]
public class DanhMucImportController : ControllerBase
{
	private readonly AppDbContext _context;
	private readonly IHttpContextAccessor _httpContextAccessor;
	public DanhMucImportController(AppDbContext context, IHttpContextAccessor httpContextAccessor)
	{
		_context = context;
		_httpContextAccessor = httpContextAccessor;
	}


	///// <summary>
	/////     Import list danh mục
	/////     <route>POST: api/danhmuc/danhmucimport-upsert</route>
	///// </summary>
	[HttpPost("api/danhmuc/danhmucimport-upserts")]
	public IActionResult Upsert_DanhMucImport(DanhMucImport request)
	{
		if (request == null)
			return StatusCode(StatusCodes.Status400BadRequest, "request_required");

		var checkGoc = _context.KieuDanhMuc.FirstOrDefault(x => x.MaKieu == request.MaKieuGoc);
		if (checkGoc == null)
			return StatusCode(StatusCodes.Status400BadRequest, "MaKieuGoc_invalid");

		// Check kiểu danh mục hiện tại để import data vào
		if (string.IsNullOrEmpty(request.MaKieu))
			return StatusCode(StatusCodes.Status400BadRequest, "MaKieu_required");
		checkGoc.Deserialize();
		var nodeHienTai = DanhMucUtils.FindNodeByMaKieu(checkGoc, request.MaKieu);
		if (nodeHienTai == null)
			return StatusCode(StatusCodes.Status400BadRequest, "MaKieu_invalid");

		// Check danh mục để import data vào
		if (request.DanhMucs == null || request.DanhMucs.Count <= 0)
			return StatusCode(StatusCodes.Status400BadRequest, "DanhMucs_required");

		// Kiểm tra trùng MaMuc trong request
		var duplicateMaMuc = request.DanhMucs
				.GroupBy(dm => dm.MaMuc)
				.Where(g => g.Count() > 1)
				.Select(g => g.Key)
				.ToList();

		if (duplicateMaMuc.Any())
		{
			return StatusCode(StatusCodes.Status400BadRequest, $"Duplicated MaMuc in import: {string.Join(", ", duplicateMaMuc)}");
		}
		//đảm bảo tính toàn vẹn dữ liệu cây
		using var transaction = _context.Database.BeginTransaction();
		try
		{
			// Bắt đầu thực hiện
			var results = new List<object>();
			int countCreated = 0, countUpdated = 0, countFailed = 0;
			if (nodeHienTai.DanhMucs == null)
				nodeHienTai.DanhMucs = new List<DanhMuc>();

			foreach (var danhMuc in request.DanhMucs)
			{
				if (string.IsNullOrEmpty(danhMuc.MaMuc) || string.IsNullOrEmpty(danhMuc.TenMuc))
				{
					results.Add(new
					{
						MaMuc = danhMuc.MaMuc,
						Status = "Failed",
						Reason = "MaMuc_or_Ten_required"
					});
					countFailed++;
					danhMuc.IsError = true;
					continue;
				}

				var existingDanhMuc = nodeHienTai.DanhMucs.FirstOrDefault(dm => dm.MaMuc == danhMuc.MaMuc);
				if (existingDanhMuc != null)
				{
					// tồn tại thì update
					existingDanhMuc.Assign(danhMuc);
					existingDanhMuc.Updated = Utils.DateTimeToUnixTimeStamp(DateTime.Now);
					existingDanhMuc.Serialize();
					countUpdated++;
				}
				else
				{
					// tạo mới
					danhMuc.Serialize();
					nodeHienTai.DanhMucs.Add(danhMuc);
					countCreated++;
				}
			}
			// Serialize lại toàn bộ cây để lưu xuống DB
			checkGoc.Serialize();
			_context.KieuDanhMuc.Update(checkGoc);

			transaction.Commit();

			countFailed = request.DanhMucs.Count - countCreated - countUpdated;
			request.SoBanGhiThemMoi = countCreated;
			request.SoBanGhiCapNhat = countUpdated;
			request.SoBanGhiLoi = countFailed;
			request.Serialize();
			_context.DanhMucImport.Add(request);

			_context.SaveChanges();
			return StatusCode(StatusCodes.Status200OK, new
			{
				Message = "Import completed successfully",
				Created = countCreated,
				Updated = countUpdated,
				Failed = countFailed,
				Results = results
			});
		}
		catch (Exception ex)
		{
			transaction.Rollback();
			return StatusCode(StatusCodes.Status500InternalServerError, $"internal_error: {ex.Message}");
		}
	}

	///// <summary>
	/////     Lấy danh sách (phân trang, tìm kiếm, không trả về DanhMucsJSON).
	/////     <route>POST: api/danhmuc/danhmucimports-list</route>
	/////     <param name="request">page, limit, search</param>
	/////     <returns>PagedListResponse<DanhMucImport></returns>
	///// </summary>
	///
	[HttpPost("api/danhmuc/danhmucimports-list")]
	public IActionResult Get_DanhMucImportList(PagedListRequest request)
	{
		if (request == null)
			return StatusCode(400, "request_required");
		var page = request?.Page > 0 ? request.Page : 1;
		var limit = (request?.Limit ?? 0) > 0 ? request.Limit : int.MaxValue;
		var search = request?.Search;
		var query = _context.DanhMucImport.AsQueryable();
		//var query = PermissionUtil.ApplyDataFilterNghiepVu(_httpContextAccessor, _context, getAll);
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

		if (!string.IsNullOrWhiteSpace(search)) query = query.Where(x => x.TenPhienBan.Contains(search));
		query = query.OrderBy(x => x.Id);
		var response = query.ToPagedListResponse(page, limit);
		// Trả thêm thông tin MaKieuGoc và MaKieu
		var resultWithDetails = response.Items.Select(item =>
		{
			var kieuDanhMuc = _context.KieuDanhMuc.FirstOrDefault(k => k.MaKieu == item.MaKieuGoc);
			kieuDanhMuc?.Deserialize();
			return new
			{
				item.Id,
				item.TenPhienBan,
				item.CreatedBy,
				item.UpdatedBy,
				item.Created,
				item.Updated,
				item.MaKieu,
				TenKieuGoc = kieuDanhMuc?.Ten, // Kiểu danh mục gốc
				TenKieuImport = DanhMucUtils.FindNodeByMaKieu(kieuDanhMuc, item.MaKieu)?.Ten
			};
		});
		return Ok(resultWithDetails);
	}

	///// <summary>
	/////     Lấy chi tiết bản ghi lịch sử import theo maKieuGoc va maKieu.
	/////     <route>GET: api/danhmuc/danhmucimport/{makieudanhmuc}</route>
	/////     <param name="makieudanhmuc">makieudanhmuc</param>
	///// </summary>
	//[HttpPost("api/danhmuc/danhmucimportbymakieu/{makieudanhmuc}")]
	//public IActionResult Get_DanhMucImportByMaKieu(string makieudanhmuc)
	//{
	//	if (string.IsNullOrWhiteSpace(makieudanhmuc))
	//		return StatusCode(400, "makieudanhmuc_required");

	//	var qr = _context.DanhMucImport.AsEnumerable();

	//	qr = qr.Where(x => x.MaKieu != null && x.MaKieu.Contains(makieudanhmuc));

	//	if (!qr.Any())
	//		return StatusCode(404, "makieudanhmuc_invalid");

	//	var data = qr.ToList();

	//	foreach (var item in data) item.Deserialize();

	//	return Ok(data);
	//}


	//// lấy chi tiết của 1 lịch sử import 
	[HttpGet("api/danhmuc/danhmucimport/{id}")]
	public IActionResult Get_DanhMucById(long id)
	{
		if (id <= 0)
			return StatusCode(400, "Id_required");
		var getById = _context.DanhMucImport.Find(id);
		if (getById == null)
			return StatusCode(404, "Id_invalid");
		getById.Deserialize();
		var kieuDanhMuc = _context.KieuDanhMuc.FirstOrDefault(k => k.MaKieu == getById.MaKieuGoc);
		if(kieuDanhMuc == null)
			return StatusCode(StatusCodes.Status400BadRequest, "MaKieuGoc_invalid");
		kieuDanhMuc?.Deserialize();
		getById.TenKieuGoc = kieuDanhMuc.Ten;
		getById.TenKieuImport = DanhMucUtils.FindNodeByMaKieu(kieuDanhMuc, getById.MaKieu)?.Ten;
		return Ok(getById);
	}
}