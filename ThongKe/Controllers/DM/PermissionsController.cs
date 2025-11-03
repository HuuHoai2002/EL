//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using ThongKe.Data;
//using ThongKe.DTOs;
//using ThongKe.Entities;
//using ThongKe.Shared.Authorization.Attributes;
//using ThongKe.Shared.Authorization.Permissions;
//using ThongKe.Shared.Authorization.Services;
//using ThongKe.Shared.Extensions;

//namespace ThongKe.Controllers.DM;

//public class PermissionsController : ControllerBase
//{
//	private readonly AppDbContext _context;
//	//private readonly IPermissionSyncService _permissionSyncService;

//	public PermissionsController(AppDbContext context
//		//, IPermissionSyncService permissionSyncService
//		)
//	{
//		_context = context;
//		//_permissionSyncService = permissionSyncService;
//	}

//	// Đồng bộ permissions từ code vào database
//	//[HttpPost("api/permissions/sync")]
//	//public async Task<IActionResult> SyncPermissions()
//	//{
//	//	try
//	//	{
//	//		await _permissionSyncService.SyncPermissionsAsync();
//	//		await _permissionSyncService.CleanupOrphanedPermissionsAsync();
//	//		return Ok("Đồng bộ permissions thành công");
//	//	}
//	//	catch (Exception ex)
//	//	{
//	//		return StatusCode(500, $"Lỗi khi đồng bộ permissions: {ex.Message}");
//	//	}
//	//}

//	// Lấy định nghĩa permissions từ code (không cần DB)
//	//[HttpGet("api/permissions/definitions")]
//	//public IActionResult GetPermissionDefinitions()
//	//{
//	//	try
//	//	{
//	//		var permissions = PermissionManager.GetAllPermissions();
//	//		return Ok(permissions);
//	//	}
//	//	catch (Exception ex)
//	//	{
//	//		return StatusCode(500, $"Lỗi khi lấy permission definitions: {ex.Message}");
//	//	}
//	//}

//	//Lấy permissions gốc từ code(không cần Db)
//	[HttpGet("api/permissions/definitions/roots")]
//	public IActionResult GetRootPermissionDefinitions()
//	{
//		try
//		{
//			var rootPermissions = PermissionManager.GetRootPermissions();
//			return Ok(rootPermissions);
//		}
//		catch (Exception ex)
//		{
//			return StatusCode(500, $"Lỗi khi lấy root permission definitions: {ex.Message}");
//		}
//	}

//	// Lấy permissions theo module từ code
//	//[HttpGet("api/permissions/definitions/module/{module}")]
//	//public IActionResult GetPermissionDefinitionsByModule(string module)
//	//{
//	//	try
//	//	{
//	//		var permissions = PermissionManager.GetPermissionsByModule(module);
//	//		return Ok(permissions);
//	//	}
//	//	catch (Exception ex)
//	//	{
//	//		return StatusCode(500, $"Lỗi khi lấy permission definitions theo module: {ex.Message}");
//	//	}
//	//}

//	// Lấy danh sách tất cả permissions với hierarchy từ DB dạng flat
//	[HttpGet("api/permissions")]
//	public async Task<IActionResult> GetAllPermissions()
//	{
//		var permissions = await _context.Permission
//			.OrderBy(p => p.Discriminator)
//			.ThenBy(p => p.DisplayName)
//			.ToListAsync();

//		return Ok(permissions);
//	}

//	// Lấy permissions dạng tree (hierarchy) từ DB dạng cây
//	[HttpGet("api/permissions/tree")]
//	public async Task<IActionResult> GetPermissionsTree()
//	{
//		var allPermissions = await _context.Permission.ToListAsync();

//		// Lấy chỉ các root permissions (không có parent)
//		var rootPermissions = allPermissions.Where(p => p.ParentId == null).ToList();

//		var tree = rootPermissions.Select(root => BuildPermissionTreeNode(root, allPermissions)).ToList();

//		return Ok(tree);
//	}

//	// Lấy permissions theo module/discriminator từ DB
//	//[HttpGet("api/permissions/by-module/{discriminator}")]
//	//public async Task<IActionResult> GetPermissionsByModule(string discriminator)
//	//{
//	//	var permissions = await _context.Permission
//	//		.Where(p => p.Discriminator == discriminator)
//	//		.OrderBy(p => p.DisplayName)
//	//		.ToListAsync();

//	//	return Ok(permissions);
//	//}

//	// Lấy permissions của một role với hierarchy từ DB
//	//[HttpGet("api/permissions/role/{roleId}")]
//	//public async Task<IActionResult> GetRolePermissions(int roleId)
//	//{
//	//	if (roleId <= 0)
//	//		return BadRequest("RoleId is required");

//	//	var role = await _context.Role.FindAsync(roleId);
//	//	if (role == null)
//	//		return NotFound("Role not found");

//	//	var rolePermissions = await _context.RolePermission
//	//		.Where(rp => rp.RoleId == roleId)
//	//		.Include(rp => rp.Permission)
//	//		.ToListAsync();

//	//	var result = rolePermissions.Select(rp => new
//	//	{
//	//		rp.PermissionId,
//	//		rp.Permission.Name,
//	//		rp.Permission.DisplayName,
//	//		rp.Permission.Description,
//	//		rp.Permission.Discriminator,
//	//		rp.Permission.ParentId,
//	//		rp.Permission.ParentName,
//	//		IsGranted = rp.IsGranted
//	//	}).ToList();

//	//	return Ok(result);
//	//}

//	// Lấy permissions tree cho role với trạng thái granted từ DB
//	//[HttpGet("api/permissions/role/{roleId}/tree")]
//	//public async Task<IActionResult> GetRolePermissionsTree(int roleId)
//	//{
//	//	if (roleId <= 0)
//	//		return BadRequest("RoleId is required");

//	//	var role = await _context.Role.FindAsync(roleId);
//	//	if (role == null)
//	//		return NotFound("Role not found");

//	//	var allPermissions = await _context.Permission.ToListAsync();

//	//	var rolePermissions = await _context.RolePermission
//	//		.Where(rp => rp.RoleId == roleId)
//	//		.ToDictionaryAsync(rp => rp.PermissionId, rp => rp.IsGranted);

//	//	var rootPermissions = allPermissions.Where(p => p.ParentId == null).ToList();

//	//	var tree = rootPermissions.Select(root => BuildRolePermissionTreeNode(root, allPermissions, rolePermissions)).ToList();

//	//	return Ok(tree);
//	//}

//	// Lấy danh sách permissions ĐÃ ĐƯỢC GÁN CHO ROLE từ DB
//	[HttpGet("api/permissions/role/{roleId}")]
//	public async Task<IActionResult> GetRolePermissions(int roleId)
//	{
//		if (roleId <= 0)
//			return BadRequest("RoleId is required");

//		var rolePermissions = await _context.Permission
//				.Where(p => p.RoleId == roleId && p.IsGranted == 1)
//				.ToListAsync();

//		return Ok(rolePermissions);
//	}


//	// Cập nhật permissions cho role - LƯU VÀO DB
//	[HttpPost("api/permissions/role/{roleId}")]
//	public async Task<IActionResult> UpdateRolePermissions(int roleId, [FromBody] UpdateRolePermissionsRequest request)
//	{
//		if (roleId <= 0 || request?.Permissions == null)
//			return BadRequest("RoleId and Permissions are required");

//		var role = await _context.Role.FirstOrDefaultAsync(x => x.Id == roleId && x.IsOpen == 1);
//		if (role == null)
//			return NotFound("Role not found");

//		if (role.IsStatic == 1)
//			return BadRequest("Không thể thay đổi quyền của vai trò hệ thống");

//		// Lấy tất cả permissions từ code để validate
//		var allCodePermissions = PermissionManager.GetAllPermissions();
//		var codePermissionDict = allCodePermissions.ToDictionary(p => p.Name, p => p);

//		// Validate hierarchy
//		foreach (var permissionRequest in request.Permissions.Where(p => p.IsGranted == 1))
//		{
//			if (codePermissionDict.TryGetValue(permissionRequest.PermissionName, out var permissionDef))
//			{
//				// Check parent permission
//				if (!string.IsNullOrEmpty(permissionDef.ParentName))
//				{
//					bool isParentGranted = request.Permissions.Any(p =>
//							p.PermissionName == permissionDef.ParentName && p.IsGranted == 1);

//					if (!isParentGranted)
//					{
//						return BadRequest($"Không thể cấp quyền '{permissionDef.DisplayName}' khi quyền cha chưa được cấp");
//					}
//				}
//			}
//		}

//		// Xóa tất cả permissions cũ của role
//		var existingRolePermissions = await _context.Permission
//				.Where(p => p.RoleId == roleId)
//				.ToListAsync();

//		_context.Permission.RemoveRange(existingRolePermissions);

//		// Thêm permissions mới - CHỈ LƯU NHỮNG PERMISSIONS ĐƯỢC GRANTED
//		foreach (var permissionRequest in request.Permissions.Where(p => p.IsGranted == 1))
//		{
//			if (codePermissionDict.TryGetValue(permissionRequest.PermissionName, out var permissionDef))
//			{
//				var permission = new Permission
//				{
//					Name = permissionDef.Name,
//					DisplayName = permissionDef.DisplayName,
//					Description = permissionDef.Description ?? "",
//					Discriminator = permissionDef.Discriminator,
//					ParentName = permissionDef.ParentName,
//					IsGrantedByDefault = permissionDef.IsGrantedByDefault,
//					IsEnabled = permissionDef.IsEnabled,
//					Created = permissionDef.Created,
//					RoleId = roleId,
//					IsGranted = 1
//				};
//				_context.Permission.Add(permission);
//			}
//		}

//		await _context.SaveChangesAsync();
//		return Ok("Cập nhật permissions thành công");
//	}


//	//// Cập nhật permissions cho role
//	//[HttpPost("api/permissions/role/{roleId}")]
//	//public IActionResult UpdateRolePermissions(int roleId, [FromBody] UpdateRolePermissionsRequest request)
//	//{
//	//	if (roleId <= 0 || request?.Permissions == null)
//	//		return BadRequest("RoleId and Permissions are required");
//	//	// chỉ thêm quyền cho vai trò đã mở khóa
//	//	var role = _context.Role.FirstOrDefault(x => x.Id == roleId && x.IsOpen == 1);
//	//	if (role == null)
//	//		return NotFound("Role not found");

//	//	if (role.IsStatic == 1)
//	//		return BadRequest("Không thể thay đổi quyền của vai trò hệ thống");

//	//	// Get all permissions for validation
//	//	var allPermissions = _context.Permission.ToList();
//	//	var permissionDict = allPermissions.ToDictionary(p => p.Id, p => p);

//	//	// Validate cấp quyền cha mới cho quyền con
//	//	// lấy quyền đang bật
//	//	foreach (var permissionRequest in request.Permissions.Where(p => p.IsGranted == 1))
//	//	{
//	//		if (permissionDict.TryGetValue(permissionRequest.PermissionId, out var permission) && permission.ParentId.HasValue)
//	//		{
//	//			// Check if parent permission is also being granted
//	//			bool isParentGranted = request.Permissions.Any(p =>
//	//					p.PermissionId == permission.ParentId && p.IsGranted == 1);

//	//			if (!isParentGranted)
//	//			{
//	//				return BadRequest($"Không thể cấp quyền '{permission.DisplayName}' khi quyền cha chưa được cấp");
//	//			}
//	//		}
//	//	}

//	//	// Xóa tất cả permissions cũ của role
//	//	var existingRolePermissions = _context.RolePermission
//	//		.Where(rp => rp.RoleId == roleId)
//	//		.ToList();

//	//	_context.RolePermission.RemoveRange(existingRolePermissions);

//	//	// Thêm permissions mới
//	//	foreach (var permissionRequest in request.Permissions)
//	//	{
//	//		var permission =  _context.Permission.Find(permissionRequest.PermissionId);
//	//		if (permission != null)
//	//		{
//	//			var rolePermission = new RolePermission
//	//			{
//	//				RoleId = roleId,
//	//				PermissionId = permissionRequest.PermissionId,
//	//				IsGranted = permissionRequest.IsGranted
//	//			};
//	//			_context.RolePermission.Add(rolePermission);
//	//		}
//	//	}
//	//	 _context.SaveChanges();
//	//	return Ok("Cập nhật permissions thành công");
//	//}

//	// build cây cha-con
//	private PermissionTreeNode BuildPermissionTreeNode(Permission permission, List<Permission> allPermissions)
//	{
//		var children = allPermissions.Where(p => p.ParentId == permission.Id).ToList();

//		return new PermissionTreeNode
//		{
//			Id = permission.Id,
//			Name = permission.Name,
//			DisplayName = permission.DisplayName,
//			Description = permission.Description,
//			Discriminator = permission.Discriminator,
//			ParentId = permission.ParentId,
//			IsEnabled = permission.IsEnabled == 1,
//			Children = children.Select(child => BuildPermissionTreeNode(child, allPermissions)).ToList()
//		};
//	}

//	// Helper method để build role permission tree node
//	//private RolePermissionTreeNode BuildRolePermissionTreeNode(Permission permission, 
//	//	List<Permission> allPermissions, Dictionary<int, bool> rolePermissions)
//	//{
//	//	var children = allPermissions.Where(p => p.ParentId == permission.Id).ToList();
//	//	var isGranted = rolePermissions.ContainsKey(permission.Id) && rolePermissions[permission.Id];

//	//	return new RolePermissionTreeNode
//	//	{
//	//		Id = permission.Id,
//	//		Name = permission.Name,
//	//		DisplayName = permission.DisplayName,
//	//		Description = permission.Description,
//	//		Discriminator = permission.Discriminator,
//	//		ParentId = permission.ParentId,
//	//		IsEnabled = permission.IsEnabled == 1,
//	//		IsGranted = isGranted,
//	//		Children = children.Select(child => BuildRolePermissionTreeNode(child, allPermissions, rolePermissions)).ToList()
//	//	};
//	//}
//}

//public class UpdateRolePermissionsRequest
//{
//	public List<PermissionRequest> Permissions { get; set; } = new();
//}

//public class PermissionRequest
//{
//	public string PermissionName { get; set; }
//	public int IsGranted { get; set; }
//}

//public class PermissionTreeNode
//{
//	public int Id { get; set; }
//	public string Name { get; set; } = "";
//	public string DisplayName { get; set; } = "";
//	public string Description { get; set; } = "";
//	public string Discriminator { get; set; } = "";
//	public int? ParentId { get; set; }
//	public bool IsEnabled { get; set; }
//	public List<PermissionTreeNode> Children { get; set; } = new();
//}

//public class RolePermissionTreeNode : PermissionTreeNode
//{
//	public bool IsGranted { get; set; }
//	public new List<RolePermissionTreeNode> Children { get; set; } = new();
//}