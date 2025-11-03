using Microsoft.EntityFrameworkCore;
using ThongKe.Entities;

namespace ThongKe.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<DonVi> DonVis { get; set; }
    public DbSet<ChiTieu> ChiTieus { get; set; }
    public DbSet<ChiTieuDaNhap> ChiTieuDaNhaps { get; set; }
    public DbSet<DuLieuChiTieu> DuLieuChiTieus { get; set; }
    public DbSet<KieuBangThongKe> KieuBangThongKes { get; set; }
    public DbSet<CongBoDienTichLua> CongBoDienTichLuas { get; set; }
    public DbSet<CongBoDienTichCayHangNamKhac> CongBoDienTichCayHangNamKhacs { get; set; }
    public DbSet<CongBoSanLuongSanPhamChanNuoi> CongBoSanLuongSanPhamChanNuois { get; set; }
    public DbSet<CongBoXuatKhauNongSan> CongBoXuatKhauNongSans { get; set; }
    public DbSet<BieuMau> BieuMaus { get; set; }
    public DbSet<DuLieuBieuMau> DuLieuBieuMaus { get; set; }
    public DbSet<KieuDanhMuc> KieuDanhMuc { get; set; }
    //public DbSet<DanhMuc> DanhMuc { get; set; }
    public DbSet<DanhMucImport> DanhMucImport { get; set; }
    public DbSet<DanhMucPublish> DanhMucPublish { get; set; }
    public DbSet<DonViThongKe> DonViThongKe { get; set; }
    public DbSet<User> User { get; set; }
    public DbSet<LogAuth> LogAuth { get; set; }
    public DbSet<CongBoNangSuatLua> CongBoNangSuatLuas { get; set; }
    public DbSet<CongBoSanLuongLua> CongBoSanLuongLuas { get; set; }
    public DbSet<CongBoSanLuongCayHangNamKhac> CongBoSanLuongCayHangNamKhacs { get; set; }
    public DbSet<CongBoDienTichCayLauNamChinh> CongBoDienTichCayLauNamChinhs { get; set; }
    public DbSet<CongBoSanLuongCayLauNamChinh> CongBoSanLuongCayLauNamChinhs { get; set; }
    public DbSet<CongBoSoLuongGiaSucGiaCam> CongBoSoLuongGiaSucGiaCams { get; set; }
    public DbSet<CongBoDienTichNuoiTrongThuySan> CongBoDienTichNuoiTrongThuySans { get; set; }
    public DbSet<CongBoSanLuongNuoiTrongThuySan> CongBoSanLuongNuoiTrongThuySans { get; set; }
    public DbSet<CongBoSanLuongThuySanKhaiThac> CongBoSanLuongThuySanKhaiThacs { get; set; }
    public DbSet<CongBoGiaSanPhamNLTSChinh> CongBoGiaSanPhamNLTSChinhs { get; set; }
    public DbSet<CongBoXuatKhauThuySan> CongBoXuatKhauThuySans { get; set; }
    public DbSet<CongBoXuatKhauSanPhamChanNuoi> CongBoXuatKhauSanPhamChanNuois { get; set; }
    public DbSet<CongBoXuatKhauLamSan> CongBoXuatKhauLamSans { get; set; }
    public DbSet<CongBoXuatKhauDauVaoSanXuat> CongBoXuatKhauDauVaoSanXuats { get; set; }
    public DbSet<CongBoXuatKhauMuoi> CongBoXuatKhauMuois { get; set; }
    public DbSet<CongBoNhapKhau> CongBoNhapKhaus { get; set; }
    public DbSet<CongBoCanCanThuongMai> CongBoCanCanThuongMais { get; set; }
}