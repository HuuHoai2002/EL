using ThongKe.Entities;
using ThongKe.Shared;
using ThongKe.Shared.Enums;

namespace ThongKe.DTOs;

public class FixedTypeResponse
{
    public string Label { get; set; } = string.Empty;
    public LoaiDuLieuCoDinh Value { get; set; } = default!;
}

public class ThongKeDuLieuChiTieu
{
    public PhanTo? PhanTo { get; set; } = null!;
    public List<ThongKeChiTieuItem> Items { get; set; } = [];
}

public class ThongKeChiTieuItem
{
    public int PhanToId { get; set; }
    public double TotalData { get; set; }
}

public class ThongKeDuLieuChiTieuResponse
{
    public ChiTieu ChiTieu { get; set; } = null!;
    public int TongSoLuongBanGhi { get; set; }
    public int TongSoLuongBanGhiPhanTo { get; set; }
    public List<ThongKeDuLieuChiTieu> Statistics { get; set; } = [];
}

public class ThongKeChiTieu
{
    public string TenChiTieu { get; set; } = string.Empty;
    public int Id { get; set; }
    public int RecordCount { get; set; }
}

public class ThongKeDanhSachChiTieuResponse
{
    public int SoLuongChiTieu { get; set; }
    public int SoLuongPhanTo { get; set; }
    public List<ThongKeChiTieu> TopChiTieus { get; set; } = [];
}

public class CongBoDienTichLuaPagedResponse
{
    public PagedResult<CongBoDienTichLua> Data { get; set; }
    public double TongDienTichThuHoach { get; set; }
    public double TongDienTichGieoTrong { get; set; }
    public double TongDienTichUocTinhGieoTrong { get; set; }
    public double TongDienTichUocTinhThuHoach { get; set; }
}

public class CongBoDienTichCayHangNamKhacPagedResponse
{
    public PagedResult<CongBoDienTichCayHangNamKhac> Data { get; set; }
    public double TongDienTichThuHoach { get; set; }
    public double TongDienTichGieoTrong { get; set; }
    public double TongDienTichUocTinhGieoTrong { get; set; }
    public double TongDienTichUocTinhThuHoach { get; set; }
}

public class CongBoSanLuongSanPhamChanNuoiPagedResponse
{
    public PagedResult<CongBoSanLuongSanPhamChanNuoi> Data { get; set; }
    public double TongSanLuongThitHoi { get; set; }
    public double TongSanLuongChanNuoiKhac { get; set; }
}

public class CongBoXuatKhauNongSanPagedResponse
{
    public PagedResult<CongBoXuatKhauNongSan> Data { get; set; }
    public double TongKhoiLuong { get; set; }
    public double TongGiaTri { get; set; }
}

public class CongBoNangSuatLuaPagedResponse
{
    public PagedResult<CongBoNangSuatLua> Data { get; set; }
    public double TongNangSuatLua { get; set; }
    public double TongNangSuatLuaUocTinh { get; set; }
}

public class CongBoSanLuongLuaPagedResponse
{
    public PagedResult<CongBoSanLuongLua> Data { get; set; }
    public double TongSanLuongLua { get; set; }
    public double TongSanLuongLuaUocTinh { get; set; }
}

public class CongBoSanLuongCayHangNamKhacPagedResponse
{
    public PagedResult<CongBoSanLuongCayHangNamKhac> Data { get; set; }
    public double TongSanLuong { get; set; }
    public double TongSanLuongUocTinh { get; set; }
}

public class CongBoSanLuongCayLauNamChinhPagedResponse
{
    public PagedResult<CongBoSanLuongCayLauNamChinh> Data { get; set; }
    public double TongSanLuong { get; set; }
    public double TongSanLuongUocTinh { get; set; }
}

public class CongBoSoLuongGiaSucGiaCamPagedResponse
{
    public PagedResult<CongBoSoLuongGiaSucGiaCam> Data { get; set; }
    public double TongSoLuong { get; set; }
    public double TongSoLuongUocTinh { get; set; }
}

public class CongBoDienTichNuoiTrongThuySanPagedResponse
{
    public PagedResult<CongBoDienTichNuoiTrongThuySan> Data { get; set; }
    public double TongDienTich { get; set; }
    public double TongDienTichUocTinh { get; set; }
}

public class CongBoSanLuongNuoiTrongThuySanPagedResponse
{
    public PagedResult<CongBoSanLuongNuoiTrongThuySan> Data { get; set; }
    public double TongSanLuong { get; set; }
    public double TongSanLuongUocTinh { get; set; }
}

public class CongBoSanLuongThuySanKhaiThacPagedResponse
{
    public PagedResult<CongBoSanLuongThuySanKhaiThac> Data { get; set; }
    public double TongSanLuong { get; set; }
    public double TongSanLuongUocTinh { get; set; }
}

public class CongBoGiaSanPhamNLTSChinhPagedResponse
{
    public PagedResult<CongBoGiaSanPhamNLTSChinh> Data { get; set; }
    public double TongGiaTri { get; set; }
    public double TongGiaTriUocTinh { get; set; }
}

public class CongBoDienTichCayLauNamChinhPagedResponse
{
    public PagedResult<CongBoDienTichCayLauNamChinh> Data { get; set; }
    public double TongDienTichThuHoach { get; set; }
    public double TongDienTichGieoTrong { get; set; }
    public double TongDienTichUocTinhGieoTrong { get; set; }
    public double TongDienTichUocTinhThuHoach { get; set; }
}

public class CongBoXuatKhauThuySanPagedResponse
{
    public PagedResult<CongBoXuatKhauThuySan> Data { get; set; }
    public double TongGiaTri { get; set; }
    public double TongGiaTriUocTinh { get; set; }
}

public class CongBoXuatKhauSanPhamChanNuoiPagedResponse
{
    public PagedResult<CongBoXuatKhauSanPhamChanNuoi> Data { get; set; }
    public double TongGiaTri { get; set; }
    public double TongGiaTriUocTinh { get; set; }
}

public class CongBoXuatKhauLamSanPagedResponse
{
    public PagedResult<CongBoXuatKhauLamSan> Data { get; set; }
    public double TongGiaTri { get; set; }
    public double TongGiaTriUocTinh { get; set; }
}

public class CongBoNhapKhauPagedResponse
{
    public PagedResult<CongBoNhapKhau> Data { get; set; }
    public double TongGiaTri { get; set; }
    public double TongGiaTriUocTinh { get; set; }
}

public class CongBoXuatKhauDauVaoSanXuatPagedResponse
{
    public PagedResult<CongBoXuatKhauDauVaoSanXuat> Data { get; set; }
    public double TongGiaTri { get; set; }
    public double TongGiaTriUocTinh { get; set; }
}

public class CongBoCanCanThuongMaiPagedResponse
{
    public PagedResult<CongBoCanCanThuongMai> Data { get; set; }
    public double TongGiaTri { get; set; }
    public double TongGiaTriUocTinh { get; set; }
}

public class CongBoXuatKhauMuoiPagedResponse
{
    public PagedResult<CongBoXuatKhauMuoi> Data { get; set; }
    public double TongGiaTri { get; set; }
    public double TongGiaTriUocTinh { get; set; }
}

public class RequestLockMultipleUser
{
    public required List<long> UserIds { get; set; }
    public required bool IsLocked { get; set; }
}

public class RequestApproveMultipleUser
{
    public required List<long> UserIds { get; set; }
}

public class RequestLoginDevice : PagedListRequest
{
    public long UserId { get; set; }
}

public class PagedListRequestDanhMuc : PagedListRequest
{
    public List<string>? MaKieus { get; set; }
    public bool? IsDataKieuDanhMuc { get; set; }
}

public class RequestChangeInfoUser
{
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public string Email { get; set; }
    public string RoleId { get; set; }
    public string? Scope { get; set; }
    public string? TinhThanhId { get; set; }
    public string? DonViThongKeId { get; set; }
    public List<string>? GiamSatDonViThongKeIds { get; set; }
    public List<long>? Members { get; set; }
}

public class PagedListRequestRole : PagedListRequest
{
    public bool? IsOpenFilter { get; set; }
}

public class RequestChangePermission
{
    public required long UserId { get; set; }
    public required string RoleId { get; set; }
    public required string Scope { get; set; }
    public string? DonViThongKeId { get; set; }
    public string? TinhId { get; set; }
    public long? SupervisorId { get; set; }
}

public class PagedListRequestDanhMucPublish : PagedListRequest
{
    public long? FromDateThoiGianHetHan { get; set; }
    public long? ToDateThoiGianHetHan { get; set; }
    public bool? IsHetHan { get; set; }
    public int? IsPublished { get; set; }
}

public class PagedListRequestKieuDanhMuc : PagedListRequest
{
    public bool? IsOpenFilter { get; set; }
    public List<string>? MaKieus { get; set; }
}

public class FilterPublishDanhMuc
{
    public long? PublishId { get; set; }
    //public List<string>? MaKieus { get; set; }
    //public bool IsIncludeData { get; set; }
}

public class RequestCloneDanhMuc
{
    public string MaKieu { get; set; }
    public bool IsCloneKieuDanhMuc { get; set; }
    public bool IsCloneDanhMucData { get; set; }
}

public class RequestOpenCloseKieuDanhMuc
{
    public long Id { get; set; }
    public bool IsOpen { get; set; }
}
public class DeleteDanhMuc
{
  public long Id { get; set; }
	public string? MaKieu { get; set; } 
  public List<string>? MaMuc { get; set; }
  public bool DeleteAll { get; set; }
}

public class PagedListRequestKieuDanhMucTomTatList
{
    public int Page { get; set; }
    public int Limit { get; set; }
    public bool IsNhomDanhMuc { get; set; }
    public bool IsDanhMucData { get; set; }

    //public bool IsTrangThai { get; set; }
}

public class DonViThongKepagedRequest : BaseRequest
{
}

public class LoginRequest
{
    public required string UserName { get; set; }
    public required string Password { get; set; }
}

public class ChangePasswordRequest
{
    public required string OldPassword { get; set; }
    public required string NewPassword { get; set; }
    public required long UserId { get; set; }
}

public class ForgotPassword
{
    public required string Token { get; set; }
    public required string NewPassword { get; set; }
    public required string ConfirmPassword { get; set; }
}

public class RefreshTokenRequest
{
    public required string RefreshToken { get; set; }
}

public class RequestVerifyCode
{
    public required string Email { get; set; }
    public required int Code { get; set; }
}