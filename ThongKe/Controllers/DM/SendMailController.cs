using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using ThongKe.Data;

namespace ThongKe.Controllers.DM;

[ApiController]
[AllowAnonymous]
public class SendMailController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _context;
    private readonly IMemoryCache _memoryCache;


    public SendMailController(AppDbContext context, IConfiguration configuration, IMemoryCache memoryCache)
    {
        _context = context;
        _configuration = configuration;
        _memoryCache = memoryCache;
    }

    /// <summary>
    ///     Gửi email đặt lại mật khẩu cho người dùng
    /// </summary>
    /// <param name="request">Email người dùng cần đặt lại mật khẩu</param>
    /// <returns>Thông báo kết quả</returns>
    [HttpPost("api/auth/send-reset-password")]
    //[Authorize(Roles = "Admin")]
    public IActionResult SendResetPasswordEmail([FromBody] ResetPasswordRequest request)
    {
        if (string.IsNullOrEmpty(request.Email)) return BadRequest("Email không được để trống");

        var user = _context.User.FirstOrDefault(u => u.Email == request.Email);
        if (user == null)
            return Ok("Nếu email của bạn tồn tại trong hệ thống, chúng tôi đã gửi một link reset mật khẩu.");
        if (user.IsVerifyRegister == 0)
            return BadRequest("Vui lòng xác thực tài khoản trước khi quên mật khẩu");
        // KIỂM TRA VÀ RESET BỘ ĐẾM
        var lastRequestDate = user.CountResetPasswordTime.HasValue
            ? DateTimeOffset.FromUnixTimeSeconds(user.CountResetPasswordTime.Value).UtcDateTime.Date
            : DateTime.MinValue.Date;
        if (lastRequestDate < DateTime.UtcNow.Date) user.CountResetPassword = 0;
        // KIỂM TRA GIỚI HẠN
        const int max_resetpass = 5;
        if (user.CountResetPassword >= max_resetpass)
            // KHÔNG KHÓA TÀI KHOẢN. Chỉ chặn yêu cầu.
            return BadRequest("Bạn đã vượt quá số lần yêu cầu đổi mật khẩu trong ngày. Vui lòng thử lại sau.");
        // TĂNG BỘ ĐẾM VÀ TẠO TOKEN
        user.CountResetPassword++;
        user.CountResetPasswordTime = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();


        // Tạo GUID mới và lưu vào user
        var guid = Guid.NewGuid() + DateTime.Now.ToString("yyyyMMddHHmmss");
        user.ResetPasswordGuid = GetMd5Hash(guid);
        user.ResetPasswordExpiryTime = new DateTimeOffset(DateTime.UtcNow.AddMinutes(10)).ToUnixTimeSeconds();
        ; // Token resetpass hết hạn sau 10 phút

        // Tạo link đặt lại mật khẩu
        var resetLink = $"{_configuration["ApplicationUrl"]}/reset-password?token={guid}";

        // Nếu không gửi email, trả về link để admin copy gửi thủ công
        if (!request.IsSendMail)
        {
            _context.User.Update(user);
            _context.SaveChanges();
            return Ok(resetLink);
        }

        // Cấu hình email
        var fromAddress = _configuration["SmtpSettings:UserName"]; // Gmail đăng nhập SMTP
        var fromPassword = _configuration["SmtpSettings:Password"]; // App password Gmail
        var smtpHost = _configuration["SmtpSettings:Server"]; // smtp.gmail.com
        var smtpPort = int.Parse(_configuration["SmtpSettings:SmtpPort"]); // Port
        var enableSsl = bool.Parse(_configuration["SmtpSettings:EnableSsl"]); // true
        if (string.IsNullOrEmpty(fromAddress) || string.IsNullOrEmpty(fromPassword) || string.IsNullOrEmpty(smtpHost) ||
            smtpPort <= 0) return StatusCode(500, "SMTP configuration is incomplete. Please check your settings.");

        try
        {
            using (var smtp = new SmtpClient())
            {
                smtp.Host = smtpHost;
                smtp.Port = smtpPort;
                smtp.EnableSsl = enableSsl;
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(fromAddress, fromPassword);

                //lấy template email
                var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "EmailTemplates",
                    "PasswordResetEmail.html");
                if (!System.IO.File.Exists(templatePath))
                    return StatusCode(500,
                        "Email template file not found. Please ensure the template exists at: " + templatePath);
                var content = System.IO.File.ReadAllText(templatePath, Encoding.UTF8);

                // thay placeholder bằng dữ liệu thật
                content = content.Replace("{{fullName}}", user.FullName)
                    .Replace("{{resetLink}}", resetLink);
                using (var message = new MailMessage())
                {
                    message.From = new MailAddress(fromAddress, "Hệ thống thống kê");
                    message.Subject = "Đặt lại mật khẩu của bạn";
                    message.IsBodyHtml = true;
                    message.Body = content;
                    message.To.Add(new MailAddress(user.Email));

                    smtp.Send(message);
                    message.Dispose(); // Giải phóng tài nguyên của MailMessage
                    smtp.Dispose();
                }
            }

            _context.User.Update(user);
            _context.SaveChanges();

            return Ok("Nếu email của bạn tồn tại trong hệ thống, chúng tôi đã gửi một link reset mật khẩu.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Lỗi khi gửi email: {ex.Message}");
        }
    }

    [HttpPost("api/auth/sendVerifiy-code")]
    public async Task<IActionResult> SendVerificationCode(VerifyRegisterRequest request)
    {
        if (string.IsNullOrEmpty(request.Email)) return BadRequest("Email không được để trống");

        var user = _context.User.FirstOrDefault(u => u.Email == request.Email);
        if (user == null) return StatusCode(500, "Lỗi đăng kí");

        var code = new Random().Next(100000, 999999);

        var cacheKey = $"verification:{request.Email}";

        var expiration = TimeSpan.FromMinutes(5); // Hết hạn sau 5 phút

        var cacheOptions = new MemoryCacheEntryOptions()
            // Mã sẽ tự động bị xóa khỏi cache sau 5 phút
            .SetAbsoluteExpiration(expiration);

        _memoryCache.Set(cacheKey, code, cacheOptions); // lưu

        var fromAddress = _configuration["SmtpSettings:UserName"]; // Gmail đăng nhập SMTP
        var fromPassword = _configuration["SmtpSettings:Password"]; // App password Gmail
        var smtpHost = _configuration["SmtpSettings:Server"]; // smtp.gmail.com
        var smtpPort = int.Parse(_configuration["SmtpSettings:SmtpPort"]); // Port
        var enableSsl = bool.Parse(_configuration["SmtpSettings:EnableSsl"]); // true
        if (string.IsNullOrEmpty(fromAddress) || string.IsNullOrEmpty(fromPassword) || string.IsNullOrEmpty(smtpHost) ||
            smtpPort <= 0) return StatusCode(500, "SMTP configuration is incomplete. Please check your settings.");

        try
        {
            using (var smtp = new SmtpClient())
            {
                smtp.Host = smtpHost;
                smtp.Port = smtpPort;
                smtp.EnableSsl = enableSsl;
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(fromAddress, fromPassword);

                //lấy template email
                var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "EmailTemplates",
                    "SendVerifyRegister.html");
                if (!System.IO.File.Exists(templatePath))
                    return StatusCode(500,
                        "Email template file not found. Please ensure the template exists at: " + templatePath);
                var content = System.IO.File.ReadAllText(templatePath, Encoding.UTF8);

                // thay placeholder bằng dữ liệu thật
                content = content.Replace("{{userName}}", user.FullName)
                    .Replace("{{code}}", code.ToString())
                    .Replace("{{expiry_minutes}}", expiration.ToString());

                using (var message = new MailMessage())
                {
                    message.From = new MailAddress(fromAddress, "Hệ thống thống kê");
                    message.Subject = "Xác thực tài khoản của bạn";
                    message.IsBodyHtml = true;
                    message.Body = content;
                    message.To.Add(new MailAddress(user.Email));

                    smtp.Send(message);
                    message.Dispose(); // Giải phóng tài nguyên của MailMessage
                    smtp.Dispose();
                }
            }

            return Ok($"Đã gửi mã xác thực đến {request.Email}. Mã sẽ hết hạn sau 5 phút.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Lỗi khi gửi email: {ex.Message}");
        }
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

    #region cách dùng jwt nhưng link dùng đc nhiều lần

    /// <summary>
    ///     Tạo token đặt lại mật khẩu
    /// </summary>
    //private string GenerateJwtToken(User user, string guid)
    //{
    //	var jwtKey = _configuration["JWT:SecretKey"];
    //	var jwtIssuer = _configuration["JWT:Issuer"];
    //	var jwtAudience = _configuration["JWT:Audience"];
    //	var jwtExpireMinutes = (int)10;
    //	//var jwtExpireMinutes = int.Parse(_configuration["JWT:ExpireMinutes"] ?? "60");

    //	if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience) || jwtExpireMinutes <= 0)
    //	{
    //		throw new Exception("JWT configuration is missing");
    //	}

    //	var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
    //	var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    //	// Tạo danh sách claims
    //	var claims = new List<Claim>
    //						{
    //								new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
    //								new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
    //								new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
    //								new Claim(JwtRegisteredClaimNames.PhoneNumber, user.Phone ?? ""),
    //								new Claim(JwtRegisteredClaimNames.Name, user.FullName ?? ""),
    //								new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
    //								//new Claim("UserId", user.Id.ToString()),
    //								new Claim("UserName", user.UserName),
    //								new Claim("ResetPassWordGuid", guid)
    //						};

    //	// Thêm roles vào claims
    //	if (user.RoleIds != null && user.RoleIds.Count > 0)
    //	{
    //		var roles = _context.Role.Where(r => user.RoleIds.Contains(r.Id)).ToList();
    //		foreach (var role in roles)
    //		{
    //			claims.Add(new Claim(ClaimTypes.Role, role.Name));
    //		}
    //	}

    //	// Tạo token
    //	var token = new JwtSecurityToken(
    //			issuer: jwtIssuer,
    //			audience: jwtAudience,
    //			claims: claims,
    //			expires: DateTime.UtcNow.AddMinutes(jwtExpireMinutes),
    //			signingCredentials: credentials
    //	);

    //	return new JwtSecurityTokenHandler().WriteToken(token);
    //}

    #endregion

    public class ResetPasswordRequest
    {
        public required string Email { get; set; }
        public required bool IsSendMail { get; set; }
    }

    public class VerifyRegisterRequest
    {
        public string Email { get; set; }
    }
}