//using Microsoft.AspNetCore.Mvc;
//using ThongKe.Data;
//using ThongKe.Entities;
//using ThongKe.Shared;
//using ThongKe.Shared.Extensions;
//using ThongKe.Shared.Utils;

//namespace ThongKe.Controllers.DM;

//public class OrganizationsController : ControllerBase
//{
//    private readonly AppDbContext _context;

//    public OrganizationsController(AppDbContext context)
//    {
//        _context = context;
//    }

//    // Thêm, sửa đơn vị
//    [HttpPost("api/organizations/organization-upsert")]
//    public IActionResult UpSert_Organization([FromBody] Organization request)
//    {
//        if (request == null || string.IsNullOrEmpty(request.Name)) return StatusCode(400, "request_or_Name_required");
//        //check MaOrganization unique
//        var checkExist =
//            _context.Organization.FirstOrDefault(x => x.MaOrganization == request.MaOrganization && x.Id != request.Id);
//        if (checkExist != null) return StatusCode(400, "MaOrganization_already_exists");
//        if (request.Id > 0)
//        {
//            var objdb = _context.Organization.Find(request.Id);
//            if (objdb == null) return StatusCode(404, "Id_invalid");
//            objdb.Assign(request);
//            //objdb.Updated = Utils.DateTimeToUnixTimeStamp(DateTime.Now);
//            //TODO updated by
//            _context.Organization.Update(objdb);
//        }
//        else
//        {
//            request.Created = Utils.DateTimeToUnixTimeStamp(DateTime.Now);
//            //TODO created by
//            _context.Organization.Add(request);
//        }

//        _context.SaveChanges();
//        return Ok(request);
//    }

//    // xóa đơn vị
//    [HttpGet("api/organizations/organization-delete/{id}")]
//    public IActionResult Delete_Organization(long id)
//    {
//        if (id <= 0)
//            return StatusCode(400, "Id_required");
//        var objdb = _context.Organization.Find(id);
//        if (objdb == null)
//            return StatusCode(404, "Id_invalid");
//        _context.Organization.Remove(objdb);
//        _context.SaveChanges();
//        return Ok("Xóa thành công");
//    }

//    // lấy danh sách có phân trang
//    [HttpPost("api/organizations/organization-pagedlist")]
//    public IActionResult GetAllOrganization([FromBody] PagedListRequest request)
//    {
//        if (request == null)
//            return StatusCode(400, "request_required");
//        var page = request?.Page > 0 ? request.Page : 1;
//        var limit = (request?.Limit ?? 0) > 0 ? request.Limit : int.MaxValue;
//        //string? search = request?.Search;

//        var query = _context.Organization.AsQueryable();

//        if (!string.IsNullOrWhiteSpace(request.TrangThai))
//            query = query.Where(t => t.Status == request.TrangThai);
//        //if (!string.IsNullOrWhiteSpace(request.CreatedBy))
//        //	query = query.Where(t => t.CreatedBy == request.CreatedBy);
//        //if (!string.IsNullOrWhiteSpace(request.UpdatedBy))
//        //	query = query.Where(t => t.UpdatedBy == request.UpdatedBy);
//        if (request.FromDateCreated.HasValue)
//            query = query.Where(t => t.Created >= request.FromDateCreated.Value);
//        if (request.ToDateCreated.HasValue)
//            query = query.Where(t => t.Created <= request.ToDateCreated.Value);
//        //if (request.FromDateUpdated.HasValue)
//        //	query = query.Where(t => t.Updated >= request.FromDateUpdated.Value);
//        //if (request.ToDateUpdated.HasValue)
//        //	query = query.Where(t => t.Updated <= request.ToDateUpdated.Value);
//        if (!string.IsNullOrWhiteSpace(request.Search))
//        {
//            var keyword = request.Search.Trim().ToLower();

//            query = query.Where(x =>
//                (x.Name ?? "").ToLower().Contains(keyword) ||
//                (x.MaOrganization ?? "").ToLower().Contains(keyword) ||
//                (x.Address ?? "").ToLower().Contains(keyword) ||
//                (x.Phone ?? "").Replace(" ", "").Contains(keyword.Replace(" ", ""))
//            );
//        }

//        query = query.OrderByDescending(x => x.Created);
//        var response = query.ToPagedListResponse(page, limit);
//        return Ok(response);
//    }

//    // lấy 1 bản ghi theo id
//    [HttpGet("api/organizations/{id}")]
//    public IActionResult GetByIdOrganization(long id)
//    {
//        if (id <= 0)
//            return StatusCode(400, "Id_required");
//        var getbyId = _context.Organization.Find(id);
//        return Ok(getbyId);
//    }

//		// lấy người ở trong đơn vị
//		//[HttpPost("api/organizations/userinorganization")]
//  //  public IActionResult GetUserInOrganization([FromBody] RequestGetUserInOrganization request)
//  //  {
//  //      if (request == null || request.OrganizationId <= 0)
//  //          return StatusCode(400, "OrganizationId_required");
//  //      var query = _context.User.Where(x => x.OrganizationId == request.OrganizationId).ToList();
//  //      return Ok(query);
//  //  }

//    public class RequestGetUserInOrganization
//    {
//        public int OrganizationId { get; set; }
//    }
//}