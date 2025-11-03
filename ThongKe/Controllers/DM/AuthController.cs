using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using ThongKe.Data;
using ThongKe.DTOs;
using ThongKe.Entities;
using ThongKe.Shared;
using ThongKe.Shared.Enums;
using ThongKe.Shared.Extensions;
using ThongKe.Shared.Utils;
using UAParser;
using static ThongKe.Shared.Utils.Utils;
using static ThongKe.Shared.Utils.Utils.PermissionUtil;

namespace ThongKe.Controllers.DM;

[ApiController]
public class AuthController(
    AppDbContext context,
    IConfiguration configuration,
    IMemoryCache memoryCache,
    IHttpContextAccessor httpContextAccessor)
    : ControllerBase
{
    /// <summary>
    ///     Lay phien dang nhap hien tai cua nguoi dung
    /// </summary>
    /// <returns></returns>
    [HttpGet("api/auth/me")]
    [Authorize]
    public Task<IActionResult> GetMe()
    {
        var value = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (value != null)
        {
            var userId = int.Parse(value);
            var user = context.User.FirstOrDefault(u => u.Id == userId);
            if (user == null)
                return Task.FromResult<IActionResult>(Unauthorized(
                    ApiResponse<string>.Fail(["Phiên đăng nhập không hợp lệ."])
                ));
            user.Deserialize();
            if (user.RoleId != null)
                user.RoleName = StaticData.Roles.FirstOrDefault(r => r.Value == user.RoleId)?.Key;
            // Select from Headers["Authorization"]
            var accessToken = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
            var meta = new Dictionary<string, object?>
            {
                { "accessToken", accessToken }
            };
            return Task.FromResult<IActionResult>(Ok(
                ApiResponse<User>.Ok(user, "Thành công!", meta)
            ));
        }

        return Task.FromResult<IActionResult>(Unauthorized(
            ApiResponse<string>.Fail(["Phiên đăng nhập không hợp lệ."])
        ));
    }

    // xác thực mã 
    [HttpPost("api/auth/verify-code")]
    public async Task<IActionResult> VerifyCode(RequestVerifyCode request)
    {
        var cacheKey = $"verification:{request.Email}";

        if (memoryCache.TryGetValue(cacheKey, out int? storedCode))
        {
            // Nếu tìm thấy mã trong cache
            if (storedCode == request.Code)
            {
                //Xóa mã khỏi cache ngay sau khi sử dụng thành công
                memoryCache.Remove(cacheKey);

                var user = context.User.FirstOrDefault(u => u.Email == request.Email);
                if (user != null)
                {
                    if (user.IsVerifyRegister == 1)
                        return BadRequest("Tài khoản đã được xác thực");
                    user.IsVerifyRegister = 1; // xác thực tài khoản
                    context.User.Update(user);
                    context.SaveChanges();
                }

                return Ok("Xác thực email thành công!");
            }

            return BadRequest("Mã xác thực không chính xác.");
        }

        return BadRequest("Mã xác thực đã hết hạn hoặc không tồn tại.");
    }

    // Tạo tài khoản (admin)
    [HttpPost("api/auth/create")]
    [Authorize]
    public IActionResult Create([FromBody] User request)
    {
        var resultCreate = CanCreateAuth(httpContextAccessor, context, request);
        if (!resultCreate.CanCreate)
            return StatusCode(StatusCodes.Status403Forbidden,
                ApiResponse<string>.Fail(["Bạn không có quyền truy cập."]));
        string maDonVi = resultCreate.DonViThongKeId;

        if (!string.IsNullOrEmpty(maDonVi) && !StaticData.IsValidDonVi(maDonVi))
        {
            var checkDv = context.DonViThongKe.FirstOrDefault(x => x.MaDonVi == maDonVi);
            if (checkDv == null) return BadRequest(ApiResponse<string>.Fail(["Đơn vị thống kê không hợp lệ"]));
        }

        if (request.Members != null)
        {
            // thêm thành viên giám sát thì phải có vai trò giám sát
            if (request.Members.Count() > 0 && request.RoleId != UserRoles.GiamSat)
                return BadRequest(ApiResponse<string>.Fail(["Vai Trò không hợp lệ"]));
            var checkMembers = context.User.Where(u => request.Members.Contains(u.Id)).ToList();
            if (checkMembers.Count() != request.Members.Count())
                return BadRequest(ApiResponse<string>.Fail(["Thành viên giám sát không hợp lệ"]));
        }

        // thêm đơn vị thống kê bị giám sát thì phải có vai trò giám sát
        if (request.GiamSatDonViThongKeIds != null)
        {
            if (request.GiamSatDonViThongKeIds.Count() > 0 && request.RoleId != UserRoles.GiamSat)
                return BadRequest(ApiResponse<string>.Fail(["Vai Trò không hợp lệ"]));
            var data = context.DonViThongKe.AsEnumerable();
            var dataStatic = StaticData.DonViThongKes;
            var query = data.Concat(dataStatic);

            var checkDonViThongKe = query.Where(dv => request.GiamSatDonViThongKeIds.Contains(dv.MaDonVi)).ToList();

            if (checkDonViThongKe.Count() != request.GiamSatDonViThongKeIds.Count())
                return BadRequest(ApiResponse<string>.Fail(["Đơn vị thống kê không tồn tại"]));
            //var getUserDonViTK = context.User.Where(u => u.DonViThongKeId != null && request.GiamSatDonViThongKeId.Contains(u.DonViThongKeId)).ToList();
            //if (getUserDonViTK.Count() == 0)
            //{
            //	return BadRequest(ApiResponse<string>.Fail(["Đơn vị thống kê không có thành viên"]));
            //}
        }

        var username = request.UserName ?? "";
        var password = request.Password ?? "";
        var email = request.Email ?? "";

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(email))
            return BadRequest(ApiResponse<string>.Fail(["UserName, Password, Email không được để trống"]));

        if (username.Contains(" ") || password.Contains(" ") || email.Contains(" "))
            return BadRequest(ApiResponse<string>.Fail(["UserName, Password, Email không được có khoảng trắng"]));

        var exists = context.User.FirstOrDefault(u =>
            u.UserName == username ||
            u.Email == email);

        if (exists != null)
            return Conflict(ApiResponse<string>.Fail(["UserName, Email đã tồn tại"]));

        if (request.Scope == Scope.Tinh && request.TinhThanhId == null)
            return BadRequest(ApiResponse<string>.Fail(["TinhThanhId không được để trống"]));

        request.UserName = username;
        request.Password = GetMd5Hash(password);
        request.Email = email;
        request.RoleId = request.RoleId;
        request.Scope = resultCreate.Scope;
        request.TinhThanhId = resultCreate.TinhThanhId;
        request.DonViThongKeId = resultCreate.DonViThongKeId;
        request.CreatedBy = resultCreate.CreatedBy;
        request.IsActive = 1;
        request.Serialize();
        context.User.Add(request);
        context.SaveChanges();
        return Ok("Create success");
    }

    // Đăng nhập trả về token và refresh token
    [HttpPost("api/auth/login")]
    public IActionResult Login(LoginRequest request)
    {
        var hash = GetMd5Hash(request.Password);
        var user = context.User.FirstOrDefault(u => u.UserName == request.UserName && u.Password == hash);
        if (user == null)
            return Unauthorized("Invalid username or password");
        if (user.IsActive != 1 || user.IsLocked == 1)
            return Unauthorized("Account is inactive or locked");

        if (user.IsLocked == 1) return Unauthorized("Người dùng đang bị tạm khóa!");
        var token = GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken();
        // Lưu refresh token vào DB
        user.RefreshToken = GetMd5Hash(refreshToken);
        var refreshExpiresTime = new DateTimeOffset(DateTime.UtcNow.AddDays(120));
        user.RefreshTokenExpiryTime = refreshExpiresTime.ToUnixTimeSeconds(); // thời hạn 120 ngày
        context.User.Update(user);
        context.SaveChanges();

        // Lấy IP address từ HttpContext
        var ipAddress = GetClientIp(HttpContext);
        var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
        var uaParser = Parser.GetDefault();
        var clientInfo = uaParser.Parse(userAgent);
        // Ghi log đăng nhập
        var logAuth = new LogAuth
        {
            UserName = user.UserName,
            IsLogin = 1,
            User = user,
            Created = DateTimeToUnixTimeStamp(DateTime.Now),
            Content = $"Đăng nhập tài khoản {user.UserName}",
            IpAddress = GetClientIp(HttpContext),
            BrowserVersion = clientInfo.UA.Family + clientInfo.UA.Major,
            OSVersion = clientInfo.OS.Family + clientInfo.OS.Major,
            DeviceType = clientInfo.Device.Family
        };
        context.LogAuth.Add(logAuth);
        context.SaveChanges();
        return Ok(new
        {
            accessToken = token,
            refreshToken,
            refreshExpiresIn = refreshExpiresTime,
            user
        });
    }

    // Sinh JWT token 
    private string GenerateJwtToken(User user)
    {
        var jwtKey = configuration["JWT:SecretKey"];
        var jwtIssuer = configuration["JWT:Issuer"];
        var jwtAudience = configuration["JWT:Audience"];
        var jwtExpireMinutes = int.Parse(configuration["JWT:ExpireMinutes"]);

        if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience) ||
            jwtExpireMinutes <= 0) throw new Exception("JWT configuration is missing");

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        // Tạo danh sách claims
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new(JwtRegisteredClaimNames.PhoneNumber, user.Phone ?? ""),
            new(JwtRegisteredClaimNames.Name, user.FullName ?? ""),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("UserName", user.UserName),
            new("IsActive", user.IsActive.ToString()),
            new("IsLocked", user.IsLocked.ToString())
        };
        if (user.Scope != null)
            claims.Add(new Claim("Scope", user.Scope));
        //if (user.SupervisorId != null)
        //	claims.Add(new Claim("SupervisorId", user.SupervisorId.ToString()));
        if (user.TinhThanhId != null)
            claims.Add(new Claim("TinhThanhId", user.TinhThanhId));
        if (user.DonViThongKeId != null)
            claims.Add(new Claim("DonViThongKeId", user.DonViThongKeId));
        if (user.RoleId != null)
            claims.Add(new Claim("RoleId", user.RoleId));
        // Tạo token
        var token = new JwtSecurityToken(
            jwtIssuer,
            jwtAudience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(jwtExpireMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // Sinh refresh token
    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }

    private static string? GetClientIp(HttpContext httpContext)
    {
        if (httpContext == null) return null;

        // 1) Kiểm tra X-Forwarded-For (nếu app đứng sau proxy)
        var headers = httpContext.Request.Headers;
        if (headers.TryGetValue("X-Forwarded-For", out var xffValues))
        {
            // X-Forwarded-For có thể chứa nhiều IP: "client, proxy1, proxy2"
            var xff = xffValues.FirstOrDefault();
            if (!string.IsNullOrEmpty(xff))
            {
                var first = xff.Split(',').Select(s => s.Trim()).FirstOrDefault();
                if (IsValidIp(first)) return NormalizeIp(first);
            }
        }

        // 2) Kiểm tra Forwarded header (RFC 7239) — "for=" value
        if (headers.TryGetValue("Forwarded", out var fwdValues))
        {
            var fwd = fwdValues.FirstOrDefault();
            if (!string.IsNullOrEmpty(fwd))
            {
                // ví dụ: Forwarded: for=192.0.2.43; proto=https; by=203.0.113.43
                var forPart = fwd.Split(';')
                    .Select(p => p.Trim())
                    .FirstOrDefault(p => p.StartsWith("for=", StringComparison.OrdinalIgnoreCase));
                if (forPart != null)
                {
                    var ip = forPart.Substring(4).Trim('"');
                    if (IsValidIp(ip)) return NormalizeIp(ip);
                }
            }
        }

        // 3) Fallback: IP từ kết nối (khi không có proxy)
        var remoteIp = httpContext.Connection.RemoteIpAddress;
        if (remoteIp != null) return NormalizeIp(remoteIp.ToString());

        return null;
    }

    private static bool IsValidIp(string? ip)
    {
        if (string.IsNullOrWhiteSpace(ip)) return false;
        var trimmed = ip.Split(':')[0];
        return IPAddress.TryParse(trimmed, out _);
    }

    private static string NormalizeIp(string ip)
    {
        if (IPAddress.TryParse(ip, out var addr))
        {
            if (addr.IsIPv4MappedToIPv6) return addr.MapToIPv4().ToString();
            return addr.ToString();
        }

        return ip;
    }

    // đổi token mới khi hết hạn
    [HttpPost("api/auth/refresh-token")]
    public IActionResult RefreshToken([FromBody] RefreshTokenRequest request)
    {
        // var clientIp = GetClientIp(HttpContext);
        //
        // if (string.IsNullOrEmpty(request.RefreshToken)) return BadRequest("Invalid request.");
        // var rateLimitKey = $"refresh_rate_limit:{clientIp}";
        // if (memoryCache.TryGetValue(rateLimitKey, out int attempts))
        // {
        //     if (attempts >= 5) // Max 5 attempts per 5 minutes
        //         return StatusCode(429, "Too many requests. Please try again later.");
        //     memoryCache.Set(rateLimitKey, attempts + 1, TimeSpan.FromMinutes(5));
        // }
        // else
        // {
        //     memoryCache.Set(rateLimitKey, 1, TimeSpan.FromMinutes(5));
        // }

        var hashRefreshToken = GetMd5Hash(request.RefreshToken);
        var user = context.User.FirstOrDefault(u =>
            u.RefreshToken == hashRefreshToken && !(u.IsActive != 1 || u.IsLocked == 1));
        if (user == null)
            // Tra ve 400 de phan biet voi 401 Unauthorized
            return BadRequest(
                ApiResponse<object>.Fail(["Mã làm mới không hợp lệ hoặc tài khoản không hoạt động."])
            );

        var now = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
        if (user.RefreshTokenExpiryTime < now)
            // Tra ve 400 de phan biet voi 401 Unauthorized
            return BadRequest(
                ApiResponse<object>.Fail(["Mã làm mới không hợp lệ hoặc tài khoản không hoạt động."])
            );

        var token = GenerateJwtToken(user);
        // memoryCache.Remove(rateLimitKey);

        return Ok(
            ApiResponse<object>.Ok(new
            {
                AccessToken = token
            })
        );
    }

    // Đăng xuất
    [HttpPost("api/auth/logout")]
    [Authorize]
    public IActionResult Logout()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        // Lấy lại thông tin user từ token
        var user = context.User.FirstOrDefault(u => u.Id == userId);
        if (user == null)
            return Unauthorized("User not found");

        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;
        context.User.Update(user);

        // Ghi log đăng xuất
        var logAuth = new LogAuth
        {
            UserName = user.UserName,
            IsLogout = 1,
            User = user,
            Created = DateTimeToUnixTimeStamp(DateTime.Now),
            Content = $"Đăng xuất tài khoản {user.UserName}",
            IpAddress = GetClientIp(HttpContext)
            // không ghi log User-Agent
        };
        context.LogAuth.Add(logAuth);
        context.SaveChanges();
        return Ok("Logout success");
    }

    // xem lịch sử hoạt động của user (admin)
    [HttpPost("api/auth/user/history")]
    [Authorize]
    public IActionResult GetHistoryUser([FromBody] RequestLoginDevice request)
    {
        if (request.UserId <= 0)
            return StatusCode(400, "UserId_required");
        var page = request?.Page > 0 ? request.Page : 1;
        var limit = (request?.Limit ?? 0) > 0 ? request.Limit : int.MaxValue;

        var _DbItem = context.User.Find(request.UserId);
        if (_DbItem == null)
            return StatusCode(StatusCodes.Status404NotFound, "UserId_invalid");
        // kiểm tra quyền xem lịch sử user
        var isRead = CanReadAuth(httpContextAccessor, context, _DbItem);
        if (!isRead)
            return StatusCode(StatusCodes.Status403Forbidden,
                ApiResponse<string>.Fail(["Bạn không có quyền xem người dùng này."]));
        var query = context.LogAuth.Where(x => x.UserName == _DbItem.UserName);
        if (request.FromDateCreated.HasValue)
            query = query.Where(t => t.Created >= request.FromDateCreated.Value);
        if (request.ToDateCreated.HasValue)
            query = query.Where(t => t.Created <= request.ToDateCreated.Value);
        query = query.OrderBy(x => x.Id);
        var response = query.ToPagedListResponse(page, limit);
        return Ok(response);
    }

    // người dùng xem danh sách thiết bị đã đăng nhập
    [HttpPost("api/auth/user/logindevice")]
    [Authorize]
    public IActionResult GetLoginDevice([FromBody] RequestLoginDevice request)
    {
        if (request.UserId <= 0)
            return StatusCode(400, "UserId_required");
        var page = request?.Page > 0 ? request.Page : 1;
        var limit = (request?.Limit ?? 0) > 0 ? request.Limit : int.MaxValue;
        var _DbItem = context.User.Find(request.UserId);
        if (_DbItem == null)
            return StatusCode(StatusCodes.Status404NotFound, "UserId_invalid");
        var isRead = CanReadAuth(httpContextAccessor, context, _DbItem);
        if (!isRead)
            return StatusCode(StatusCodes.Status403Forbidden,
                ApiResponse<string>.Fail(["Bạn không có quyền xem người dùng này."]));
        var query = context.LogAuth.Where(x => x.UserName == _DbItem.UserName);
        if (request.FromDateCreated.HasValue)
            query = query.Where(t => t.Created >= request.FromDateCreated.Value);
        if (request.ToDateCreated.HasValue)
            query = query.Where(t => t.Created <= request.ToDateCreated.Value);
        query = query.Where(i => i.IsLogin == 1); // chỉ lấy lịch sử đăng nhập
        query = query.OrderBy(x => x.Id);
        var response = query.ToPagedListResponse(page, limit);
        return Ok(response);
    }

    // Lấy thông tin user theo Id
    [HttpGet("api/auth/user/{id}")]
    [Authorize]
    public IActionResult Get_UserById(long id)
    {
        if (id <= 0)
            return StatusCode(400, "Id_required");
        var _DbItem = context.User.Find(id);
        var isRead = CanReadAuth(httpContextAccessor, context, _DbItem);
        if (!isRead)
            return StatusCode(StatusCodes.Status403Forbidden,
                ApiResponse<string>.Fail(["Bạn không có quyền xem người dùng này."]));
        _DbItem.Deserialize();


        if (_DbItem == null)
            return StatusCode(404, "Id_invalid");
        if (_DbItem.RoleId != null)
            _DbItem.RoleName = StaticData.Roles.FirstOrDefault(r => r.Value == _DbItem.RoleId)?.Value;
        //if (_DbItem.SupervisorId != null)
        //{
        //	_DbItem.Supervisor = context.User.Find(_DbItem.SupervisorId);
        //	if (_DbItem.Supervisor != null)
        //	{
        //		_DbItem.Supervisor.RoleName = StaticData.Roles.FirstOrDefault(r => r.Key == _DbItem.Supervisor.RoleId)?.Value;
        //	}
        //}

        if (_DbItem.Members != null && _DbItem.Members.Count() > 0)
        {
            var queryMember = context.User.Where(x => _DbItem.Members != null && _DbItem.Members.Contains(x.Id));

            _DbItem.Subordinates = queryMember.ToList();
            foreach (var sub in _DbItem.Subordinates)
                if (sub.RoleId != null)
                    sub.RoleName = StaticData.Roles.FirstOrDefault(r => r.Key == sub.RoleId)?.Value;
        }

        if (_DbItem.GiamSatDonViThongKeIds != null && _DbItem.GiamSatDonViThongKeIds.Count() > 0)
        {
            var data = context.DonViThongKe.AsEnumerable();
            var dataStatic = StaticData.DonViThongKes;
            var queryDonVi = data.Concat(dataStatic);
            var donVi = queryDonVi.Where(x => _DbItem.GiamSatDonViThongKeIds.Contains(x.MaDonVi));
            _DbItem.GiamSatDonViThongKes = donVi.ToList();
        }

        _DbItem.Password = ""; // ẩn password trong response
        _DbItem.RefreshToken = null; // ẩn refresh token trong response
        _DbItem.RefreshTokenExpiryTime = null;
        _DbItem.ResetPasswordGuid = null; // ẩn GUID reset password trong response
        _DbItem.ResetPasswordExpiryTime = null;
        _DbItem.CountResetPassword = 0;
        _DbItem.CountResetPasswordTime = null;
        return Ok(_DbItem);
    }

    // Cập nhật thông tin user (admin)
    [HttpPost("api/auth/user/admin-update/{id}")]
    public IActionResult Admin_Update_User(long id, [FromBody] RequestChangeInfoUser request)
    {
        if (id <= 0)
            return StatusCode(400, "Id_required");
        var _DbItem = context.User.Find(id);
        if (_DbItem == null)
            return StatusCode(404, "Id_invalid");
        var canUpdate = CanUpdateAuth(httpContextAccessor, context, _DbItem);
        if (!canUpdate)
            return StatusCode(StatusCodes.Status403Forbidden,
                ApiResponse<string>.Fail(["Bạn không có quyền cập nhật người dùng này."]));

        if (!StaticData.IsValidRole(request.RoleId))
            return BadRequest(ApiResponse<string>.Fail(["Vai Trò không hợp lệ"]));
        if (request.Members != null)
        {
            if (request.Members.Count() > 0 && request.RoleId != UserRoles.GiamSat)
                return BadRequest(ApiResponse<string>.Fail(["Vai Trò không hợp lệ"]));
            var checkMembers = context.User.Where(u => request.Members.Contains(u.Id)).ToList();
            if (checkMembers.Count() != request.Members.Count())
                return BadRequest(ApiResponse<string>.Fail(["Thành viên giám sát không hợp lệ"]));
        }

        if (request.GiamSatDonViThongKeIds != null)
        {
            if (request.GiamSatDonViThongKeIds.Count() > 0 && request.RoleId != UserRoles.GiamSat)
                return BadRequest(ApiResponse<string>.Fail(["Vai Trò không hợp lệ"]));
            var data = context.DonViThongKe.AsEnumerable();
            var dataStatic = StaticData.DonViThongKes;
            var query = data.Concat(dataStatic);

            var checkDonViThongKe = query.Where(dv => request.GiamSatDonViThongKeIds.Contains(dv.MaDonVi)).ToList();

            if (checkDonViThongKe.Count() != request.GiamSatDonViThongKeIds.Count())
                return BadRequest(ApiResponse<string>.Fail(["Đơn vị thống kê không tồn tại"]));
            //var getUserDonViTK = context.User.Where(u => u.DonViThongKeId != null && request.GiamSatDonViThongKeIds.Contains(u.DonViThongKeId)).ToList();
            //if (getUserDonViTK.Count() == 0)
            //{
            //	return BadRequest(ApiResponse<string>.Fail(["Đơn vị thống kê không có người dùng"]));
            //}
            //if (request.Members == null)
            //{
            //	request.Members = new List<long>();
            //}
            //request.Members.AddRange(getUserDonViTK.Select(u => u.Id));
        }

        _DbItem.Members = request.Members; // chuyển Members thành MemberJson trước khi lưu
        _DbItem.Email = request.Email;
        _DbItem.FullName = request.FullName;
        _DbItem.Phone = request.Phone;
        _DbItem.RoleId = request.RoleId;
        _DbItem.Scope = request.Scope;
        _DbItem.TinhThanhId = request.TinhThanhId;
        _DbItem.DonViThongKeId = request.DonViThongKeId;
        _DbItem.Serialize();
        context.Update(_DbItem);
        context.SaveChanges();
        return Ok(_DbItem);
    }

    // Cập nhật thông tin user của tài khoản người dùng
    [HttpPost("api/auth/user/update/{id}")]
    [Authorize]
    public IActionResult Update_User(long id, [FromBody] RequestChangeInfoUser request)
    {
        if (id <= 0)
            return StatusCode(400, "Id_required");
        var _DbItem = context.User.Find(id);
        if (_DbItem == null)
            return StatusCode(StatusCodes.Status404NotFound, "UserId_invalid");
        // chỉ cho mình user sửa
        var isUpdate = CanUpdateAuth(httpContextAccessor, context, _DbItem, UpdatePermissionType.Private);
        if (!isUpdate)
            return StatusCode(StatusCodes.Status403Forbidden,
                ApiResponse<string>.Fail(["Bạn không có quyền cập nhật người dùng này."]));
        _DbItem.Email = request.Email;
        _DbItem.FullName = request.FullName;
        _DbItem.Phone = request.Phone;
        context.Update(_DbItem);
        context.SaveChanges();
        return Ok(_DbItem);
    }

    // Cập nhật thông tin user (admin)
    //[HttpPost("api/auth/user/admin-update/{id}")]
    //public IActionResult Admin_Update_User(long id, [FromBody] RequestChangeInfoUser request)
    //{
    //	if (id <= 0)
    //		return StatusCode(400, "Id_required");
    //	var _DbItem = _context.User.Find(id);
    //	if (_DbItem == null)
    //		return StatusCode(404, "Id_invalid");
    //	_DbItem.Email = request.Email;
    //	_DbItem.FullName = request.FullName;
    //	_DbItem.Phone = request.Phone;
    //	_DbItem.RoleIds = request.RoleIds;
    //	_DbItem.OrganizationId = request.OrganizationId;
    //	_DbItem.SupervisorId = request.SupervisorId;
    //	_context.Update(_DbItem);
    //	_context.SaveChanges();
    //	return Ok(_DbItem);
    //}

    // Khóa/ mở khóa nhiều user
    [HttpPost("api/auth/user/lock-multiple")]
    [Authorize]
    public IActionResult Lock_MultipleUser([FromBody] RequestLockMultipleUser request)
    {
        if (request.UserIds == null || request.UserIds.Count == 0)
            return StatusCode(StatusCodes.Status404NotFound, "UserIds_invalid");
        var users = context.User.Where(u => request.UserIds.Contains(u.Id)).ToList();
        if (users.Count != request.UserIds.Count)
            return StatusCode(StatusCodes.Status404NotFound, "UserId_invalid");

        foreach (var user in users)
        {
            var isUpdate = CanUpdateAuth(httpContextAccessor, context, user);
            if (!isUpdate)
                return StatusCode(StatusCodes.Status403Forbidden,
                    ApiResponse<string>.Fail(["Bạn không có quyền cập nhật người dùng này."]));
            if (user.IsActive != 1)
                return StatusCode(400, $"User {user.UserName} is inactive");
            if (request.IsLocked && user.IsLocked == 1)
                return StatusCode(400, $"User {user.UserName} already locked");
            if (!request.IsLocked && user.IsLocked == 0)
                return StatusCode(400, $"User {user.UserName} already unlocked");
            user.IsLocked = request.IsLocked ? 1 : 0;
            user.IsActive = request.IsLocked ? 0 : 1;
        }

        context.UpdateRange(users);
        context.SaveChanges();
        return Ok(users);
    }

    // duyệt nhiều user **
    [HttpPost("api/auth/user/approve-multiple")]
    [Authorize]
    public IActionResult Approve_MultipleUser([FromBody] RequestApproveMultipleUser request)
    {
        if (request.UserIds == null || request.UserIds.Count == 0)
            return StatusCode(400, "UserIds_required");
        var users = context.User.Where(u => request.UserIds.Contains(u.Id)).ToList();
        if (users.Count != request.UserIds.Count)
            return StatusCode(404, "One or more UserIds are invalid");
        foreach (var user in users)
        {
            var isUpdate = CanUpdateAuth(httpContextAccessor, context, user);
            if (!isUpdate)
                return StatusCode(StatusCodes.Status403Forbidden,
                    ApiResponse<string>.Fail(["Bạn không có quyền cập nhật người dùng này."]));

            if (user.IsActive == 1)
                return StatusCode(400, $"User {user.UserName} already active");
            user.IsInactive = 0;
            user.IsActive = 1;
        }

        context.UpdateRange(users);
        context.SaveChanges();
        return Ok(users);
    }

    // Đổi mật khẩu
    [HttpPost("api/auth/user/change-password")]
    [Authorize]
    public IActionResult Change_Password(ChangePasswordRequest request)
    {
        if (request.UserId <= 0 || string.IsNullOrWhiteSpace(request.NewPassword) ||
            string.IsNullOrWhiteSpace(request.OldPassword))
            return StatusCode(StatusCodes.Status400BadRequest,
                ApiResponse<string>.Fail(["UserId, OldPassword, NewPassword không được để trống"]));
        var _DbItem = context.User.Find(request.UserId);
        if (_DbItem == null)
            return StatusCode(StatusCodes.Status404NotFound, ApiResponse<string>.Fail(["UserId_invalid"]));

        var isUpdate = CanUpdateAuth(httpContextAccessor, context, _DbItem, UpdatePermissionType.Private);
        if (!isUpdate)
            return StatusCode(StatusCodes.Status403Forbidden,
                ApiResponse<string>.Fail(["Bạn không có quyền cập nhật người dùng này."]));
        if (_DbItem.Password != GetMd5Hash(request.OldPassword))
            return StatusCode(400, "Old_password_incorrect");
        _DbItem.Password = GetMd5Hash(request.NewPassword);
        _DbItem.RefreshToken = null; // xóa refresh token cũ
        _DbItem.RefreshTokenExpiryTime = null;
        context.Update(_DbItem);
        context.SaveChanges();
        return Ok(_DbItem);
    }

    // quên mật khẩu
    [HttpPost("api/auth/forgot-password")]
    public IActionResult Forgot_Password(ForgotPassword request)
    {
        if (string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.NewPassword) ||
            string.IsNullOrWhiteSpace(request.ConfirmPassword)) // 
            return BadRequest("NewPassword, Token, ConfirmPassword must not be empty");

        if (request.NewPassword != request.ConfirmPassword)
            return BadRequest("NewPassword and ConfirmPassword do not match");

        //// Kiểm tra GUID trong DB có user nào khớp với GUID trong token không
        var user = context.User.FirstOrDefault(u => u.ResetPasswordGuid == GetMd5Hash(request.Token));
        if (user == null)
            return NotFound("User not found");

        // kiểm tra thời hạn reset password trong ngày
        var dateNow = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
        if (user.ResetPasswordExpiryTime < dateNow)
            return BadRequest("Link đặt lại mật khẩu đã hết hạn");
        // Update password
        user.Password = GetMd5Hash(request.NewPassword);
        user.RefreshToken = null; // xóa refresh token cũ
        user.RefreshTokenExpiryTime = null;
        user.ResetPasswordGuid = null; // xóa GUID đã dùng
        user.ResetPasswordExpiryTime = null; // xóa thời hạn reset password
        context.Update(user);
        context.SaveChanges();
        return Ok("Password reset successfully");
    }

    /// Lấy danh sách (phân trang, tìm kiếm)
    [HttpPost("api/auth/users-list")]
    [Authorize]
    public IActionResult Get_UsersList(PagedListRequest request)
    {
        var getdata = context.User.AsQueryable();
        var query = ApplyDataFilterAuth<User>(httpContextAccessor, context, getdata);

        var page = request?.Page > 0 ? request.Page : 1;
        var limit = (request?.Limit ?? 0) > 0 ? request.Limit : int.MaxValue;
        var search = request?.Search;

        if (request.FromDateCreated.HasValue)
            query = query.Where(t => t.Created >= request.FromDateCreated.Value);
        if (request.ToDateCreated.HasValue)
            query = query.Where(t => t.Created <= request.ToDateCreated.Value);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x =>
                x.UserName.Contains(search) || x.FullName.Contains(search) || x.Email.Contains(search) ||
                x.Phone.Contains(search));

        query = query.OrderByDescending(x => x.Created);
        var response = query.ToPagedListResponse(page, limit);

        response.Items.ForEach(item =>
        {
            item.Password = ""; // ẩn password trong response
            item.RefreshToken = null; // ẩn refresh token trong response
            item.RefreshTokenExpiryTime = null;
            item.ResetPasswordGuid = null; // ẩn GUID reset password trong response
            item.ResetPasswordExpiryTime = null;
            item.CountResetPassword = 0;
            item.CountResetPasswordTime = null;
        });
        return Ok(response); //response
    }

    private string GetMd5Hash(string input)
    {
        using (var md5 = MD5.Create())
        {
            var inputBytes = Encoding.UTF8.GetBytes(input);
            var hashBytes = md5.ComputeHash(inputBytes);
            var sb = new StringBuilder();
            foreach (var b in hashBytes)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }


    #region khóa/mở khóa 1 user

    // Khóa/mở khóa user
    //[HttpPost("api/auth/user/lock/{id}")]
    //public IActionResult Lock_User(Int64 id, [FromBody] bool IsLocked)
    //{
    //	var issuer = Utils.ReadUser(httpContextAccessor, context);
    //	if (!StaticData.IsValidRole(issuer.RoleId))
    //		return StatusCode(403, "Người dùng không có vai trò");
    //	if (id <= 0)
    //		return StatusCode(400, "Id_required");
    //	if (issuer.Scope != Scope.Tw)
    //	{
    //		var userToLock = context.User.Find(id);
    //		if (userToLock == null)
    //			return StatusCode(404, "Id_invalid");

    //		if (userToLock.Scope != issuer.Scope)
    //			return StatusCode(403, "Không có quyền khóa/mở khóa người dùng ngoài phạm vi của mình");

    //		if (issuer.TinhThanhId != "" && userToLock.TinhThanhId != issuer.TinhThanhId)
    //			return StatusCode(403, "Không có quyền khóa/mở khóa người dùng ngoài phạm vi của mình");

    //		if (issuer.DonViThongKeId != "" && userToLock.DonViThongKeId != issuer.DonViThongKeId)
    //			return StatusCode(403, "Không có quyền khóa/mở khóa người dùng ngoài phạm vi của mình");
    //	}
    //	var _DbItem = context.User.Find(id);
    //	if (_DbItem == null)
    //		return StatusCode(404, "Id_invalid");
    //	if (_DbItem.IsActive != 1)
    //		return StatusCode(400, "User_is_inactive");
    //	if (IsLocked && _DbItem.IsLocked == 1)
    //		return StatusCode(400, "User_already_locked");
    //	if (!IsLocked && _DbItem.IsLocked == 0)
    //		return StatusCode(400, "User_already_unlocked");
    //	_DbItem.IsLocked = IsLocked ? 1 : 0;
    //	_DbItem.IsActive = IsLocked ? 0 : 1;

    //	context.Update(_DbItem);
    //	context.SaveChanges();
    //	return Ok(_DbItem);
    //}

    #endregion

    // quên mật khẩu
    //[HttpPost("api/auth/forgot-password")]
    //public IActionResult Forgot_Password(ForgotPassword request)
    //{
    //	if (string.IsNullOrWhiteSpace(request.Token) ||
    //		string.IsNullOrWhiteSpace(request.NewPassword) ||
    //		string.IsNullOrWhiteSpace(request.ConfirmPassword))
    //		return BadRequest("Token, NewPassword, ConfirmPassword must not be empty");

    //	if (request.NewPassword != request.ConfirmPassword)
    //		return BadRequest("NewPassword and ConfirmPassword do not match");

    //	try
    //	{
    //		// verify token
    //		var handler = new JwtSecurityTokenHandler();
    //		var securityToken = _configuration["JWT:SecretKey"];
    //		var jwtIssuer = _configuration["JWT:Issuer"];
    //		var jwtAudience = _configuration["JWT:Audience"];
    //		if (string.IsNullOrEmpty(securityToken) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
    //			return StatusCode(500, "Server_configuration_error");
    //		var tokenValidationParameters = new TokenValidationParameters
    //		{
    //			ValidateIssuerSigningKey = true, // bật kiểm tra chữ ký
    //			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securityToken)),// verify chữ ký đối xứng
    //			ValidateIssuer = true, // kiểm tra is issuer
    //			ValidateAudience = true, // // kiểm tra audience
    //			ValidateLifetime = true, // kiểm tra thời gian hết hạn
    //			ValidAudience = jwtAudience, // audience
    //			ValidIssuer = jwtIssuer, // issuer
    //			ClockSkew = TimeSpan.Zero // độ trễ cho token hết hạn
    //		};

    //		SecurityToken validatedToken;
    //		ClaimsPrincipal claimsPrincipal;

    //		try
    //		{
    //			claimsPrincipal = handler.ValidateToken(request.Token, tokenValidationParameters, out validatedToken);
    //		}
    //		catch (SecurityTokenExpiredException)
    //		{
    //			return Unauthorized("Link expired");
    //		}
    //		catch (Exception)
    //		{
    //			return BadRequest("Invalid token");
    //		}

    //		var userIdClaim = claimsPrincipal.FindFirst("UserId")?.Value;
    //		if (string.IsNullOrEmpty(userIdClaim))
    //		{
    //			return BadRequest("Invalid token format");
    //		}

    //		long userId = Convert.ToInt64(userIdClaim);

    //		// Find user in database
    //		var user = _context.User.FirstOrDefault(u => u.Id == userId);
    //		if (user == null)
    //			return NotFound("User not found");

    //		// Update password
    //		user.Password = GetMd5Hash(request.NewPassword);
    //		_context.Update(user);
    //		_context.SaveChanges();
    //		return Ok("Password reset successfully");
    //	}
    //	catch
    //	{
    //		return BadRequest("Invalid token");
    //	}
    //}

    #region nếu frontend gửi token trong header Authorization

    // quên mật khẩu
    //[HttpPost("api/auth/forgot-password")]
    //[Authorize] // cần token trong header Authorization
    //public IActionResult Forgot_Password(ForgotPassword request)
    //{
    //	if (string.IsNullOrWhiteSpace(request.NewPassword) || string.IsNullOrWhiteSpace(request.ConfirmPassword)) // string.IsNullOrWhiteSpace(request.Token) ||
    //		return BadRequest("NewPassword, ConfirmPassword must not be empty");

    //	if (request.NewPassword != request.ConfirmPassword)
    //		return BadRequest("NewPassword and ConfirmPassword do not match");

    //	var userIdClaim = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
    //	if (userIdClaim <= 0)
    //		return BadRequest("Invalid token format");

    //	long userId = Convert.ToInt64(userIdClaim);

    //	// Find user in database
    //	var user = _context.User.FirstOrDefault(u => u.Id == userId);
    //	if (user == null)
    //		return NotFound("User not found");
    //	var resetGuidClaim = User.FindFirst("resetPassWordGuid")?.Value;
    //	//// Kiểm tra GUID trong DB có khớp với GUID trong token không
    //	//if (string.IsNullOrEmpty(user.ResetPasswordGuid) || user.ResetPasswordGuid != resetGuidClaim)
    //	//{
    //	//	return BadRequest("Link đặt lại mật khẩu này đã được sử dụng hoặc không hợp lệ");
    //	//}
    //	// Update password
    //	user.Password = GetMd5Hash(request.NewPassword);
    //	user.RefreshToken = null; // xóa refresh token cũ
    //	user.RefreshTokenExpiryTime = null;
    //	_context.Update(user);
    //	_context.SaveChanges();
    //	return Ok("Password reset successfully");
    //}

    #endregion
}