using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using ThongKe.Data;
using ThongKe.DTOs;
using ThongKe.Entities;
using ThongKe.Shared;
using ThongKe.Shared.Enums;
using ThongKe.Shared.Utils;

namespace ThongKe.Controllers.DM;

public class AuthorController(
		AppDbContext context,
		IHttpContextAccessor httpContextAccessor)
		: ControllerBase
{

	[HttpGet("api/scopes")]
	public IActionResult GetScopes()
	{
		return Ok(ApiResponse<List<KV>>.Ok(StaticData.Scopes));
	}

	[HttpGet("api/list-role")]
	public IActionResult GetListRole()
	{
		return Ok(ApiResponse<List<KV>>.Ok(StaticData.Roles));
	}

	[HttpGet("api/list-tinhthanh")]
	public IActionResult GetListTinhThanh()
	{
		return Ok(ApiResponse<List<KV>>.Ok(StaticData.TinhThanhs));
	}
}

