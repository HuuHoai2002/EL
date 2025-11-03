using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace ThongKe.Entities;

public class User : BasicInfo
{
    [Key] public long Id { get; set; }
    public required string UserName { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public string? FullName { get; set; }

    public string? Phone { get; set; }

    //public long? SupervisorId { get; set; } // Người giám sát của người dùng
    [JsonIgnore] public string? MemberJson { get; set; } // lưu id người bị giám sát dưới dạng json array

    [JsonIgnore]
    public string? GiamSatDonViThongKeJson { get; set; } // lưu id đơn vị thống kê bị giám sát dưới dạng json array

    [NotMapped] public List<long>? Members { get; set; } // BE nhận vào danh sách id người bị giám sát

    [NotMapped]
    public List<string>? GiamSatDonViThongKeIds { get; set; } // BE nhận vào danh sách đơn vị thống kê Id bị giám sát

    [NotMapped]
    public List<User>?
        Subordinates { get; set; } // danh sách người dùng bị giám sát bởi người giám sát chỉ cho BE trả về

    [NotMapped]
    public List<DonViThongKe>?
        GiamSatDonViThongKes { get; set; } // danh sách đơn vị bị giám sát bởi người giám sát chỉ cho BE trả về

    public string? RoleId { get; set; } //vai trò fix cứng
    [NotMapped] public string? RoleName { get; set; } //vai trò fix cứng
    public string? RefreshToken { get; set; } // Lưu Refresh Token
    public long? RefreshTokenExpiryTime { get; set; } // Thời gian hết hạn
    public int IsVerifyRegister { get; set; } // 0: false, 1: true - đã xác thực khi đăng ký
    public int? IsActive { get; set; } = 0;
    public int? IsInactive { get; set; } = 0;
    public int? IsLocked { get; set; } = 0;
    public long Created { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    public void Deserialize()
    {
        if (MemberJson != null && MemberJson.Length > 0)
            Members = JsonSerializer.Deserialize<List<long>>(MemberJson);
        if (GiamSatDonViThongKeJson != null && GiamSatDonViThongKeJson.Length > 0)
            GiamSatDonViThongKeIds = JsonSerializer.Deserialize<List<string>>(GiamSatDonViThongKeJson);
    }

    public void Serialize()
    {
        MemberJson = JsonSerializer.Serialize(Members);
        GiamSatDonViThongKeJson = JsonSerializer.Serialize(GiamSatDonViThongKeIds);
    }

    public void Assign(User candidate)
    {
        UserName = candidate.UserName;
        Email = candidate.Email;
        Password = candidate.Password;
        FullName = candidate.FullName;
        Phone = candidate.Phone;
        RoleId = candidate.RoleId;
        //SupervisorId = candidate.SupervisorId;
        //IsActive = candidate.IsActive;
        //IsInactive = candidate.IsInactive;
        //IsLocked = candidate.IsLocked;
    }
    //public string? UpdatedBy { get; set; }

    #region dành cho quên mật khẩu

    public string? ResetPasswordGuid { get; set; } // Lưu mã GUID dùng để reset mật khẩu tránh token dùng được nhiều lần
    public long? ResetPasswordExpiryTime { get; set; } // Thời gian hết hạn của mã reset mật khẩu

    public int CountResetPassword { get; set; } =
        0; // đếm số lần reset mật khẩu trong ngày, nếu quá 5 lần thì khóa tài khoản

    public long? CountResetPasswordTime { get; set; } // thời gian bắt đầu đếm số lần reset mật khẩu trong ngày

    #endregion
}

public class LogAuth
{
    [Key] public long Id { get; set; }

    public int IsLogin { get; set; } = 0;
    public int IsLogout { get; set; } = 0;
    public required string UserName { get; set; }
    [NotMapped] public User? User { get; set; } // For response only
    [NotMapped] public string Token { get; set; } // For response only
    public string? BrowserVersion { get; set; } // phiên bản trình duyệt
    public string? OSVersion { get; set; } // hệ điều hành
    public string? DeviceType { get; set; } // thiết bị truy cập
    public string? IpAddress { get; set; } // địa chỉ ip
    public string? Content { get; set; }
    public long Created { get; set; }
}

public class DonViThongKe : BasicInfo
{
    [Key] public int Id { get; set; }
    public string TenDonVi { get; set; }
    public string MaDonVi { get; set; }
    [NotMapped] public string? DonViThongKeId { get; set; } // ẩn field kế thừa
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    [MaxLength(100)] public string? UpdatedBy { get; set; }

    public void Assign(DonViThongKe candidate)
    {
        TenDonVi = candidate.TenDonVi;
        MaDonVi = candidate.MaDonVi;
        Scope = candidate.Scope;
        TinhThanhId = candidate.TinhThanhId;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = candidate.UpdatedBy;
    }
}

[Index(nameof(Scope))]
[Index(nameof(TinhThanhId))]
public class BasicInfo
{
    //[NotMapped] public long? Id { get; set; }
    public string? Scope { get; set; }
    public string? TinhThanhId { get; set; }
    public string? DonViThongKeId { get; set; }
    [MaxLength(100)] public string? CreatedBy { get; set; }
}