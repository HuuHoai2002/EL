using System.ComponentModel.DataAnnotations;
using ThongKe.Shared;
using ThongKe.Shared.Enums;

namespace ThongKe.DTOs;

public class ChiTieuQueryDto : BaseRequest
{
    public int? DonViId { get; set; } = null;
    public int? ParentId { get; set; } = null;
    public TrangThaiChiTieu? TrangThai { get; set; }
}

public class BieuDoQueryDto
{
    [Required] public int PhienBanChiTieuId { get; set; }

    [Required] [Length(1, 10)] public List<int> ChiTieuIds { get; set; } = [];

    public string AggregateFunction { get; set; } = Constants.AggreatedFunctions.First();
}

public class BieuDoFilterDto
{
    [Required] public int PhienBanChiTieuId { get; set; }

    [Required] [Length(1, 10)] public List<int> ChiTieuIds { get; set; } = [];
}

public class DonViQueryDto : BaseRequest
{
}

public class NhapChiTieuQueryDto : BaseRequest
{
    public string? Nguon { get; set; }
    public string? Ten { get; set; }
    public List<int> ChiTieuIds { get; set; } = [];
}

public class PhanTichChiTieuQueryDto : DataFilter
{
    [Required] [Length(1, 100)] public List<int> ChiTieuIds { get; set; } = [];
    [Required] public string GroupByColumn { get; set; } = string.Empty;
    public int Cursor { get; set; } = 0;
    public int PageSize { get; set; } = 50;
    
    // Tham số điều khiển việc tính thống kê mô tả
    public bool IncludeStatistics { get; set; } = false;
    
    // New filter system
    public List<ChiTieuFilter>? ChiTieuFilters { get; set; } // Filter cho từng chỉ tiêu
    public GroupByFilter? GroupByFilter { get; set; } // Filter cho kết quả sau group by
    
    // Legacy filter properties (để backward compatibility)
    public Dictionary<string, object?>? Filters { get; set; }
    public string? SearchTerm { get; set; }
    public List<string>? SearchColumns { get; set; }
}

// Filter operation types
public enum FilterOperation
{
    Equal = 1,      // Bằng
    Greater = 2,    // Lớn hơn
    Less = 3,       // Nhỏ hơn
    GreaterOrEqual = 4, // Lớn hơn hoặc bằng
    LessOrEqual = 5,    // Nhỏ hơn hoặc bằng
    Between = 6,    // Trong khoảng từ - đến
    Contains = 7,   // Chứa (cho string)
    StartsWith = 8, // Bắt đầu bằng (cho string)
    EndsWith = 9    // Kết thúc bằng (cho string)
}

// Filter condition for individual column
public class FilterCondition
{
    public string ColumnName { get; set; } = string.Empty;
    public FilterOperation Operation { get; set; } = FilterOperation.Equal;
    public object? Value { get; set; }
    public object? ValueTo { get; set; } // Chỉ dùng cho Between operation
}

// Filter for individual ChiTieu
public class ChiTieuFilter
{
    public int ChiTieuId { get; set; }
    public List<FilterCondition> Conditions { get; set; } = [];
}

// Filter for grouped results (after group by)
public class GroupByFilter
{
    public List<FilterCondition> Conditions { get; set; } = [];
}

public class PhanTichChiTieuRequestDto
{
    [Required] [Length(1, 100)] public List<int> ChiTieuIds { get; set; } = [];
    [Required] public string GroupByColumn { get; set; } = string.Empty;
    public int Cursor { get; set; } = 0;
    public int PageSize { get; set; } = 50;
    
    // Tham số điều khiển việc tính thống kê mô tả
    public bool IncludeStatistics { get; set; } = false;
    
    // New filter system
    public List<ChiTieuFilter>? ChiTieuFilters { get; set; } // Filter cho từng chỉ tiêu
    public GroupByFilter? GroupByFilter { get; set; } // Filter cho kết quả sau group by
    
    // Legacy filter properties (để backward compatibility)
    public Dictionary<string, object?>? Filters { get; set; }
    public string? SearchTerm { get; set; }
    public List<string>? SearchColumns { get; set; }
}

public class KieuBangThongKeQueryDto : BaseRequest
{
    public int? PhienBanChiTieuId { get; set; } = null;
}

public class BieuMauQueryDto : BaseRequest
{
    public int? PhienBanChiTieuId { get; set; } = null;

    // public int? ChiTieuId { get; set; } = null;
    public string? DonViBaoCao { get; set; } = null;
    public string? DonViNhanBaoCao { get; set; } = null;
    public int? DonViId { get; set; } = null;
    public int? Thang { get; set; } = null;
    public int? Nam { get; set; } = null;
    public int? SoThongTu { get; set; } = null;
}

public class DuLieuBieuMauQueryDto : BaseRequest
{
    public int? BieuMauId { get; set; } = null;
}

public class BangThongKeQueryDto : DataFilter
{
    [Required] public int KieuBangThongKeId { get; set; }
    public int Cursor { get; set; } = 0;
    public int PageSize { get; set; } = 50;
}

public class BangChiTieuQueryDto : DataFilter
{
    [Required] public string TableName { get; set; } = string.Empty;
    [Required] public int PhienBanChiTieuId { get; set; }
    public int Cursor { get; set; } = 0;
    public int PageSize { get; set; } = 50;
}

public class RecordQueryDto : DataFilter
{
    [Required] public int Id { get; set; }
    public string? OrderBy { get; set; } = null;
}

public class GetDataSourceDto
{
    [Required] public string TableName { get; set; } = string.Empty;

    // [Required] public int PhienBanChiTieuId { get; set; }
    public int? Id { get; set; }
    public string? Hashing { get; set; }
}

public class CongBoDienTichLuaQueryDto : BaseRequest
{
    public List<int>? Nams { get; set; }
    public List<int>? Thangs { get; set; }
    public List<string>? MaTinhThanhs { get; set; }
    public List<string>? MuaVus { get; set; }
    public int? IsCongBo { get; set; }
    public int? IsAllData { get; set; }
}

public class CongBoDienTichCayHangNamKhacQueryDto : BaseRequest
{
    public List<int>? Nams { get; set; }
    public List<int>? Thangs { get; set; }
    public List<string>? MaTinhThanhs { get; set; }
    public List<string>? LoaiCays { get; set; }
    public List<string>? MuaVus { get; set; }
    public int? IsCongBo { get; set; }
}

public class CongBoSanLuongCayHangNamKhacQueryDto : BaseRequest
{
    public List<int>? Nams { get; set; }
    public List<int>? Thangs { get; set; }
    public List<string>? MaTinhThanhs { get; set; }
    public List<string>? LoaiCays { get; set; }
    public List<string>? MuaVus { get; set; }
    public int? IsCongBo { get; set; }
}

public class CongBoSoLuongGiaSucGiaCamQueryDto : BaseRequest
{
    public List<int>? Nams { get; set; }
    public List<int>? Thangs { get; set; }
    public List<string>? MaTinhThanhs { get; set; }
    public List<string>? LoaiVatNuois { get; set; }
    public int? IsCongBo { get; set; }
}

public class CongBoDienTichNuoiTrongThuySanQueryDto : BaseRequest
{
    public List<int>? Nams { get; set; }
    public List<int>? Thangs { get; set; }
    public List<string>? MaTinhThanhs { get; set; }
    public List<string>? LoaiThuySanChinhs { get; set; }
    public List<string>? MoiTruongNuois { get; set; }
    public int? IsCongBo { get; set; }
}

public class CongBoGiaSanPhamNLTSChinhQueryDto : BaseRequest
{
    public List<int>? Nams { get; set; }
    public List<int>? Thangs { get; set; }
    public List<string>? MaTinhThanhs { get; set; }
    public List<string>? SanPhamNLTSs { get; set; }
    public List<string>? LoaiGias { get; set; }
    public int? IsCongBo { get; set; }
}

public class CongBoSanLuongSanPhamChanNuoiQueryDto : BaseRequest
{
    public List<int>? Nams { get; set; }
    public List<int>? Quys { get; set; }
    public List<string>? MaTinhThanhs { get; set; }
    public List<string>? TenSanPhams { get; set; }
    public int? IsCongBo { get; set; }
}

public class CongBoXuatKhauNongSanQueryDto : BaseRequest
{
    public List<int>? Nams { get; set; }
    public List<int>? Thangs { get; set; }
    public List<string>? MaQuocGiaVungLTs { get; set; }
    public int? IsCongBo { get; set; }
}

public class CongBoXuatKhauQueryDto : BaseRequest
{
    public List<int>? Nams { get; set; }
    public List<int>? Thangs { get; set; }
    public List<string>? MaQuocGiaVungLTs { get; set; }
    public int? IsCongBo { get; set; }
}