using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace KHCN.Shared.Enums;

public enum TrangThaiChiTieu
{
    [Display(Name = "Lưu trữ")] Archived = 3,

    [Display(Name = "Đang xuất bản")] Published = 2,

    [Display(Name = "Chưa xuất bản")] NonPublished = 1
}

public enum LoaiDuLieuCoDinh
{
    [Display(Name = "Ngày")] Day = 1,
    [Display(Name = "Tháng")] Month = 2,
    [Display(Name = "Năm")] Year = 4,

    [Display(Name = "Quý")] Quarter = 3
    // [Display(Name = "Vùng")] Region = 5,
    // [Display(Name = "Thời gian")] Datetime = 8
}

public class OptionValue
{
    public string Label { get; set; }
    public string Value { get; set; }
    public string Hint { get; set; }
    [JsonIgnore] public string AccentLabel { get; set; } = string.Empty;
    [JsonIgnore] public string? OracleDbType { get; set; }
}

public abstract class Constants
{
    public static readonly string DateFormat = "dd/MM/yyyy HH:mm:ss";

    public static HashSet<string> AggreatedFunctions = ["SUM", "AVG", "MIN", "MAX", "COUNT", "STDDEV", "VARIANCE"];

    // ReSharper disable InconsistentNaming
    public class OracleBoolean
    {
        public const int TRUE = 1;
        public const int FALSE = 0;
    }

    public static List<OptionValue> LoaiDuLieus =
    [
        new()
        {
            Label = "Đoạn văn bản ngắn", Value = "van_ban_ngan", OracleDbType = "NVARCHAR2(1000)",
            Hint = "Ví dụ: Tên, địa chỉ, mô tả ngắn..."
        },
        new()
        {
            Label = "Đoạn văn bản", Value = "van_ban", OracleDbType = "CLOB",
            Hint = "Ví dụ: Mô tả chi tiết, ghi chú..."
        },
        new()
        {
            Label = "Số liệu", Value = "so_lieu", OracleDbType = "NUMBER", Hint = "Ví dụ: Số nguyên, số thập phân..."
        },
        new() { Label = "Ngày", Value = "ngay", OracleDbType = "NUMBER(2)" },
        new() { Label = "Tháng", Value = "thang", OracleDbType = "NUMBER(2)" },
        new() { Label = "Năm", Value = "nam", OracleDbType = "NUMBER(4)" },
        new()
        {
            Label = "Quý", Value = "quy", OracleDbType = "NVARCHAR2(255)",
            Hint = "Ví dụ: Quý 1, Quý 2, Quý 3, Quý 4, Quý I,..."
        },
        new() { Label = "Giờ", Value = "gio", OracleDbType = "NVARCHAR2(255)" },
        new()
        {
            Label = "Ngày tháng năm", Value = "ngay_thang_nam", OracleDbType = "NVARCHAR2(255)",
            Hint = "Ví dụ: 24/10/2025"
        },
        new()
        {
            Label = "Ngày & giờ", Value = "ngay_gio", OracleDbType = "NVARCHAR2(255)",
            Hint = "Ví dụ: 24/10/2025 14:30:00"
        },
        new() { Label = "Đúng/Sai", Value = "dung_sai", OracleDbType = "NUMBER(1)" },
        new()
        {
            Label = "Chọn một mục (từ danh mục)", Value = "danh_muc", OracleDbType = "NUMBER(10)",
            Hint = "Ví dụ: Danh mục tỉnh, danh mục đơn vị hành chính..."
        },
        new()
        {
            Label = "Chọn một mục (từ danh sách tự định nghĩa)", Value = "danh_sach",
            OracleDbType = "NVARCHAR2(255)",
            Hint = "Ví dụ: Danh sách trạng thái, danh sách loại hình..."
        }
    ];
}

public static class Scope
{
    public const string Tw = "tw";
    public const string Cuc = "cuc";
    public const string Tinh = "tinh";
    public const string Cn = "cn";
}

public static class UserRoles
{
    public const string Admin = "admin";
    public const string User = "user";
    public const string GiamSat = "giamsat";
}