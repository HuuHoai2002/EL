using KHCN.Services;
using KHCN.Shared;
using Microsoft.AspNetCore.Mvc;

namespace KHCN.Modules.HealthCheck;

[ApiController]
[Route("/api/v1/[controller]")]
public class HealthCheckController() : BaseSevice
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(ApiResponse<int>.Ok(DateTime.UtcNow.Microsecond));
    }
}