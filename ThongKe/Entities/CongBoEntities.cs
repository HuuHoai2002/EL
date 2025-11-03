using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace ThongKe.Entities;

[Index(nameof(CreatedBy))]
[Index(nameof(UpdatedBy))]
public class CongBoDienTichLua
{
    [Key] public int Id { get; set; }
    public string MaTinhThanh { get; set; } = string.Empty;
    public string TenTinhThanh { get; set; } = string.Empty;
    public string MuaVu { get; set; } = string.Empty;
    public int Nam { get; set; }
    public int Thang { get; set; }
    public double DienTichThuHoach { get; set; }
    public double DienTichGieoTrong { get; set; }
    public double DienTichUocTinhGieoTrong { get; set; }
    public double DienTichUocTinhThuHoach { get; set; }
    public string DonViTinh { get; set; } = string.Empty;
    [NotMapped] public double TongDienTichThuHoach { get; set; } = 0;
    [NotMapped] public double TongDienTichGieoTrong { get; set; } = 0;
    [NotMapped] public double TongDienTichUocTinhGieoTrong { get; set; } = 0;
    [NotMapped] public double TongDienTichUocTinhThuHoach { get; set; } = 0;
    public int IsCongBo { get; set; } // 0: Chưa công bố, 1: Đã công bố
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    [MaxLength(100)] public string? CreatedBy { get; set; }
    [MaxLength(100)] public string? UpdatedBy { get; set; }

    public void Assign(CongBoDienTichLua candidate)
    {
        MaTinhThanh = candidate.MaTinhThanh;
        TenTinhThanh = candidate.TenTinhThanh;
        MuaVu = candidate.MuaVu;
        Nam = candidate.Nam;
        Thang = candidate.Thang;
        DienTichThuHoach = candidate.DienTichThuHoach;
        DienTichGieoTrong = candidate.DienTichGieoTrong;
        DienTichUocTinhGieoTrong = candidate.DienTichUocTinhGieoTrong;
        DienTichUocTinhThuHoach = candidate.DienTichUocTinhThuHoach;
        DonViTinh = candidate.DonViTinh;
        IsCongBo = candidate.IsCongBo;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = candidate.UpdatedBy;
    }
}

public class CongBoDienTichLua2
{
    [Key] public int Id { get; set; }
    public int Nam { get; set; }
    public int Thang { get; set; }
    public int IsCongBo { get; set; } // 0: Chưa công bố, 1: Đã công bố

    [JsonIgnore]
    [Column(TypeName = "CLOB")]
    public string DienTichGieoTrongVaThuThoachLuasJSON { get; set; } = string.Empty;

    [NotMapped] public List<DienTichLua> DienTichGieoTrongVaThuHoachLuas { get; set; } = [];

    [JsonIgnore]
    [Column(TypeName = "CLOB")]
    public string DienTichCaNuocJSON { get; set; } = string.Empty;

    [NotMapped] public DienTichLua? DienTichCaNuoc { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    [MaxLength(100)] public string? CreatedBy { get; set; }
    [MaxLength(100)] public string? UpdatedBy { get; set; }

    public void Assign(CongBoDienTichLua2 candidate)
    {
        Nam = candidate.Nam;
        Thang = candidate.Thang;
        IsCongBo = candidate.IsCongBo;
        DienTichGieoTrongVaThuHoachLuas = candidate.DienTichGieoTrongVaThuHoachLuas;
        DienTichCaNuoc = candidate.DienTichCaNuoc;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = candidate.UpdatedBy;
    }

    public void Deserialize()
    {
        if (!string.IsNullOrEmpty(DienTichGieoTrongVaThuThoachLuasJSON))
            DienTichGieoTrongVaThuHoachLuas =
                JsonSerializer.Deserialize<List<DienTichLua>>(DienTichGieoTrongVaThuThoachLuasJSON) ?? [];
        if (!string.IsNullOrEmpty(DienTichCaNuocJSON))
            DienTichCaNuoc = JsonSerializer.Deserialize<DienTichLua>(DienTichCaNuocJSON);
    }

    public void Serialize()
    {
        DienTichGieoTrongVaThuThoachLuasJSON = JsonSerializer.Serialize(DienTichGieoTrongVaThuHoachLuas);
        DienTichCaNuocJSON = JsonSerializer.Serialize(DienTichCaNuoc);
    }
}

public class DienTichLua
{
    public string MaTinhThanh { get; set; } = string.Empty;
    public string TenTinhThanh { get; set; } = string.Empty;
    public string MuaVu { get; set; } = string.Empty;
    public double DienTichThuHoach { get; set; }
    public double DienTichGieoTrong { get; set; }
    public double DienTichUocTinhThuHoach { get; set; }
    public double DienTichUocTinhGieoTrong { get; set; }
    public string DonViTinh { get; set; } = string.Empty;
}

public class CongBoDienTichCayHangNamKhac
{
    [Key] public int Id { get; set; }
    public string MaTinhThanh { get; set; } = string.Empty;
    public string TenTinhThanh { get; set; } = string.Empty;
    public string LoaiCay { get; set; } = string.Empty;
    public int Nam { get; set; }
    public int Thang { get; set; }
    public double DienTichThuHoach { get; set; }
    public double DienTichGieoTrong { get; set; }
    public double DienTichUocTinhGieoTrong { get; set; }
    public double DienTichUocTinhThuHoach { get; set; }
    public string DonViTinh { get; set; } = string.Empty;
    [NotMapped] public double TongDienTichThuHoach { get; set; } = 0;
    [NotMapped] public double TongDienTichGieoTrong { get; set; } = 0;
    [NotMapped] public double TongDienTichUocTinhGieoTrong { get; set; } = 0;
    [NotMapped] public double TongDienTichUocTinhThuHoach { get; set; } = 0;
    public int IsCongBo { get; set; } // 0: Chưa công bố, 1: Đã công bố
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(100)] public string? CreatedBy { get; set; }

    [MaxLength(100)] public string? UpdatedBy { get; set; }

    public void Assign(CongBoDienTichCayHangNamKhac candidate)
    {
        MaTinhThanh = candidate.MaTinhThanh;
        TenTinhThanh = candidate.TenTinhThanh;
        LoaiCay = candidate.LoaiCay;
        Nam = candidate.Nam;
        Thang = candidate.Thang;
        DienTichThuHoach = candidate.DienTichThuHoach;
        DienTichGieoTrong = candidate.DienTichGieoTrong;
        DienTichUocTinhGieoTrong = candidate.DienTichUocTinhGieoTrong;
        DienTichUocTinhThuHoach = candidate.DienTichUocTinhThuHoach;
        DonViTinh = candidate.DonViTinh;
        IsCongBo = candidate.IsCongBo;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = candidate.UpdatedBy;
    }
}

public class CongBoDienTichCayHangNamKhac2
{
    [Key] public int Id { get; set; }
    public int Nam { get; set; }
    public int Thang { get; set; }
    public int IsCongBo { get; set; } // 0: Chưa công bố, 1: Đã công bố

    [JsonIgnore]
    [Column(TypeName = "CLOB")]
    public string DienTichGieoTrongVaThuHoachCayHangNamKhacsJSON { get; set; } = string.Empty;

    [NotMapped] public List<DienTichCayHangNamKhac> DienTichGieoTrongVaThuHoachCayHangNamKhacs { get; set; } = [];

    [JsonIgnore]
    [Column(TypeName = "CLOB")]
    public string DienTichCaNuocJSON { get; set; } = string.Empty;

    [NotMapped] public DienTichCayHangNamKhac? DienTichCaNuoc { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(100)] public string? CreatedBy { get; set; }

    [MaxLength(100)] public string? UpdatedBy { get; set; }

    public void Assign(CongBoDienTichCayHangNamKhac2 candidate)
    {
        Nam = candidate.Nam;
        Thang = candidate.Thang;
        IsCongBo = candidate.IsCongBo;
        DienTichGieoTrongVaThuHoachCayHangNamKhacs = candidate.DienTichGieoTrongVaThuHoachCayHangNamKhacs;
        DienTichCaNuoc = candidate.DienTichCaNuoc;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = candidate.UpdatedBy;
    }

    public void Deserialize()
    {
        if (!string.IsNullOrEmpty(DienTichGieoTrongVaThuHoachCayHangNamKhacsJSON))
            DienTichGieoTrongVaThuHoachCayHangNamKhacs =
                JsonSerializer.Deserialize<List<DienTichCayHangNamKhac>>(
                    DienTichGieoTrongVaThuHoachCayHangNamKhacsJSON) ?? [];
        if (!string.IsNullOrEmpty(DienTichCaNuocJSON))
            DienTichCaNuoc = JsonSerializer.Deserialize<DienTichCayHangNamKhac>(DienTichCaNuocJSON);
    }

    public void Serialize()
    {
        DienTichGieoTrongVaThuHoachCayHangNamKhacsJSON =
            JsonSerializer.Serialize(DienTichGieoTrongVaThuHoachCayHangNamKhacs);
        DienTichCaNuocJSON = JsonSerializer.Serialize(DienTichCaNuoc);
    }
}

public class DienTichCayHangNamKhac
{
    public string MaTinhThanh { get; set; } = string.Empty;
    public string TenTinhThanh { get; set; } = string.Empty;
    public string LoaiCay { get; set; } = string.Empty;
    public double DienTichGieoTrong { get; set; }
    public double DienTichUocTinhGieoTrong { get; set; }
    public double DienTichThuHoach { get; set; }
    public double DienTichUocTinhThuHoach { get; set; }
    public string DonViTinh { get; set; } = string.Empty;
}

public class CongBoSanLuongSanPhamChanNuoi
{
    [Key] public int Id { get; set; }
    public string MaTinhThanh { get; set; } = string.Empty;
    public string TenTinhThanh { get; set; } = string.Empty;
    public int Nam { get; set; }
    public int Quy { get; set; }
    public string TenSanPham { get; set; } = string.Empty;
    public string DonViTinh { get; set; } = string.Empty;
    public double SanLuongThitHoi { get; set; }
    public double SanLuongChanNuoiKhac { get; set; }
    [NotMapped] public double TongSanLuongThitHoi { get; set; } = 0;
    [NotMapped] public double TongSanLuongChanNuoiKhac { get; set; } = 0;
    public int IsCongBo { get; set; } // 0: Chưa công bố, 1: Đã công bố
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    [MaxLength(100)] public string? CreatedBy { get; set; }

    [MaxLength(100)] public string? UpdatedBy { get; set; }

    public void Assign(CongBoSanLuongSanPhamChanNuoi candidate)
    {
        MaTinhThanh = candidate.MaTinhThanh;
        TenTinhThanh = candidate.TenTinhThanh;
        Nam = candidate.Nam;
        Quy = candidate.Quy;
        TenSanPham = candidate.TenSanPham;
        DonViTinh = candidate.DonViTinh;
        SanLuongThitHoi = candidate.SanLuongThitHoi;
        SanLuongChanNuoiKhac = candidate.SanLuongChanNuoiKhac;
        IsCongBo = candidate.IsCongBo;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = candidate.UpdatedBy;
    }
}

public class CongBoSanLuongSanPhamChanNuoi2
{
    [Key] public int Id { get; set; }
    public int Nam { get; set; }
    public int Quy { get; set; }
    public int IsCongBo { get; set; } // 0: Chưa công bố, 1: Đã công bố

    [JsonIgnore]
    [Column(TypeName = "CLOB")]
    public string SanLuongSanPhamChanNuoisJSON { get; set; } = string.Empty;

    [NotMapped] public List<SanLuongSanPhamChanNuoi> SanLuongSanPhamChanNuois { get; set; } = [];

    [JsonIgnore]
    [Column(TypeName = "CLOB")]
    public string SanLuongCaNuocJSON { get; set; } = string.Empty;

    [NotMapped] public SanLuongSanPhamChanNuoi? SanLuongCaNuoc { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(100)] public string? CreatedBy { get; set; }

    [MaxLength(100)] public string? UpdatedBy { get; set; }

    public void Assign(CongBoSanLuongSanPhamChanNuoi2 candidate)
    {
        Nam = candidate.Nam;
        Quy = candidate.Quy;
        IsCongBo = candidate.IsCongBo;
        SanLuongSanPhamChanNuois = candidate.SanLuongSanPhamChanNuois;
        SanLuongCaNuoc = candidate.SanLuongCaNuoc;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = candidate.UpdatedBy;
    }

    public void Deserialize()
    {
        if (!string.IsNullOrEmpty(SanLuongSanPhamChanNuoisJSON))
            SanLuongSanPhamChanNuois =
                JsonSerializer.Deserialize<List<SanLuongSanPhamChanNuoi>>(SanLuongSanPhamChanNuoisJSON) ?? [];
        if (!string.IsNullOrEmpty(SanLuongCaNuocJSON))
            SanLuongCaNuoc = JsonSerializer.Deserialize<SanLuongSanPhamChanNuoi>(SanLuongCaNuocJSON);
    }

    public void Serialize()
    {
        SanLuongSanPhamChanNuoisJSON = JsonSerializer.Serialize(SanLuongSanPhamChanNuois);
        SanLuongCaNuocJSON = JsonSerializer.Serialize(SanLuongCaNuoc);
    }
}

public class SanLuongSanPhamChanNuoi
{
    public string MaTinhThanh { get; set; } = string.Empty;
    public string TenTinhThanh { get; set; } = string.Empty;
    public string TenSanPham { get; set; } = string.Empty;
    public string DonViTinh { get; set; } = string.Empty;
    public double SanLuongThitHoi { get; set; }
    public double SanLuongChanNuoiKhac { get; set; }
}

public class CongBoXuatKhauNongSan2
{
    [Key] public int Id { get; set; }
    public int Nam { get; set; }
    public int Thang { get; set; }
    public int IsCongBo { get; set; } // 0: Chưa công bố, 1: Đã công bố

    [JsonIgnore]
    [Column(TypeName = "CLOB")]
    public string XuatKhauGaosJSON { get; set; } = string.Empty;

    [NotMapped] public List<XuatKhauNongSan> XuatKhauGaos { get; set; } = [];

    [JsonIgnore]
    [Column(TypeName = "CLOB")]
    public string XuatKhauCaPhesJSON { get; set; } = string.Empty;

    [NotMapped] public List<XuatKhauNongSan> XuatKhauCaPhes { get; set; } = [];

    [JsonIgnore]
    [Column(TypeName = "CLOB")]
    public string XuatKhauCaoSusJSON { get; set; } = string.Empty;

    [NotMapped] public List<XuatKhauNongSan> XuatKhauCaoSus { get; set; } = [];

    [JsonIgnore]
    [Column(TypeName = "CLOB")]
    public string XuatKhauChesJSON { get; set; } = string.Empty;

    [NotMapped] public List<XuatKhauNongSan> XuatKhauChes { get; set; } = [];

    [JsonIgnore]
    [Column(TypeName = "CLOB")]
    public string XuatKhauHatDieusJSON { get; set; } = string.Empty;

    [NotMapped] public List<XuatKhauNongSan> XuatKhauHatDieus { get; set; } = [];

    [JsonIgnore]
    [Column(TypeName = "CLOB")]
    public string XuatKhauHatTieusJSON { get; set; } = string.Empty;

    [NotMapped] public List<XuatKhauNongSan> XuatKhauHatTieus { get; set; } = [];

    [JsonIgnore]
    [Column(TypeName = "CLOB")]
    public string XuatKhauHangRauQuasJSON { get; set; } = string.Empty;

    [NotMapped] public List<XuatKhauNongSan> XuatKhauHangRauQuas { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(100)] public string? CreatedBy { get; set; }

    [MaxLength(100)] public string? UpdatedBy { get; set; }

    public void Assign(CongBoXuatKhauNongSan2 candidate)
    {
        Nam = candidate.Nam;
        Thang = candidate.Thang;
        IsCongBo = candidate.IsCongBo;
        XuatKhauGaos = candidate.XuatKhauGaos;
        XuatKhauCaPhes = candidate.XuatKhauCaPhes;
        XuatKhauCaoSus = candidate.XuatKhauCaoSus;
        XuatKhauChes = candidate.XuatKhauChes;
        XuatKhauHatDieus = candidate.XuatKhauHatDieus;
        XuatKhauHatTieus = candidate.XuatKhauHatTieus;
        XuatKhauHangRauQuas = candidate.XuatKhauHangRauQuas;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = candidate.UpdatedBy;
    }

    public void Deserialize()
    {
        if (!string.IsNullOrEmpty(XuatKhauGaosJSON))
            XuatKhauGaos = JsonSerializer.Deserialize<List<XuatKhauNongSan>>(XuatKhauGaosJSON) ?? [];
        if (!string.IsNullOrEmpty(XuatKhauCaPhesJSON))
            XuatKhauCaPhes = JsonSerializer.Deserialize<List<XuatKhauNongSan>>(XuatKhauCaPhesJSON) ?? [];
        if (!string.IsNullOrEmpty(XuatKhauCaoSusJSON))
            XuatKhauCaoSus = JsonSerializer.Deserialize<List<XuatKhauNongSan>>(XuatKhauCaoSusJSON) ?? [];
        if (!string.IsNullOrEmpty(XuatKhauChesJSON))
            XuatKhauChes = JsonSerializer.Deserialize<List<XuatKhauNongSan>>(XuatKhauChesJSON) ?? [];
        if (!string.IsNullOrEmpty(XuatKhauHatDieusJSON))
            XuatKhauHatDieus = JsonSerializer.Deserialize<List<XuatKhauNongSan>>(XuatKhauHatDieusJSON) ?? [];
        if (!string.IsNullOrEmpty(XuatKhauHatTieusJSON))
            XuatKhauHatTieus = JsonSerializer.Deserialize<List<XuatKhauNongSan>>(XuatKhauHatTieusJSON) ?? [];
        if (!string.IsNullOrEmpty(XuatKhauHangRauQuasJSON))
            XuatKhauHangRauQuas = JsonSerializer.Deserialize<List<XuatKhauNongSan>>(XuatKhauHangRauQuasJSON) ?? [];
    }

    public void Serialize()
    {
        XuatKhauGaosJSON = JsonSerializer.Serialize(XuatKhauGaos);
        XuatKhauCaPhesJSON = JsonSerializer.Serialize(XuatKhauCaPhes);
        XuatKhauCaoSusJSON = JsonSerializer.Serialize(XuatKhauCaoSus);
        XuatKhauChesJSON = JsonSerializer.Serialize(XuatKhauChes);
        XuatKhauHatDieusJSON = JsonSerializer.Serialize(XuatKhauHatDieus);
        XuatKhauHatTieusJSON = JsonSerializer.Serialize(XuatKhauHatTieus);
        XuatKhauHangRauQuasJSON = JsonSerializer.Serialize(XuatKhauHangRauQuas);
    }
}

public class XuatKhauNongSan
{
    public string DonViKhoiLuong { get; set; } = string.Empty;
    public double KhoiLuong { get; set; } = 0;
    public string DonViGiaTri { get; set; } = string.Empty;
    public double GiaTri { get; set; } = 0;
    public string MaQuocGiaVungLT { get; set; } = string.Empty;
    public string TenQuocGiaVungLT { get; set; } = string.Empty;

    public int IsTatCaQuocGiaVungLT { get; set; } =
        0; // 1: Tat ca quoc gia vung lanh tho, 0: Khong phai tat ca quoc gia vung lanh tho
}

public class CongBoXuatKhauNongSan
{
    [Key] public int Id { get; set; }
    public string MaQuocGiaVungLT { get; set; } = string.Empty;
    public string TenQuocGiaVungLT { get; set; } = string.Empty;
    public int Nam { get; set; }
    public int Thang { get; set; }

    public string LoaiNongSan { get; set; } =
        string.Empty; // Gạo, Cà phê, Cao su, Chè, Hạt điều, Hạt tiêu, Hàng rau quả (required value, select option, use value here)

    public string DonViKhoiLuong { get; set; } = string.Empty;
    public double KhoiLuong { get; set; }
    public string DonViGiaTri { get; set; } = string.Empty;
    public double GiaTri { get; set; }
    public int IsCongBo { get; set; } // 0: Chưa công bố, 1: Đã công bố
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    [MaxLength(100)] public string? CreatedBy { get; set; }
    [MaxLength(100)] public string? UpdatedBy { get; set; }

    public void Assign(CongBoXuatKhauNongSan candidate)
    {
        MaQuocGiaVungLT = candidate.MaQuocGiaVungLT;
        TenQuocGiaVungLT = candidate.TenQuocGiaVungLT;
        Nam = candidate.Nam;
        Thang = candidate.Thang;
        LoaiNongSan = candidate.LoaiNongSan;
        DonViKhoiLuong = candidate.DonViKhoiLuong;
        KhoiLuong = candidate.KhoiLuong;
        DonViGiaTri = candidate.DonViGiaTri;
        GiaTri = candidate.GiaTri;
        IsCongBo = candidate.IsCongBo;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = candidate.UpdatedBy;
    }
}

public class CongBoNangSuatLua
{
    [Key] public int Id { get; set; }
    public string MaTinhThanh { get; set; } = string.Empty;
    public string TenTinhThanh { get; set; } = string.Empty;
    public string MuaVu { get; set; } = string.Empty;
    public int Nam { get; set; }
    public int Thang { get; set; }
    public double NangSuatLua { get; set; }
    public double NangSuatLuaUocTinh { get; set; }
    public string DonViTinh { get; set; } = string.Empty;
    [NotMapped] public double TongNangSuatLua { get; set; } = 0;
    [NotMapped] public double TongNangSuatLuaUocTinh { get; set; } = 0;
    public int IsCongBo { get; set; } // 0: Chưa công bố, 1: Đã công bố
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    [MaxLength(100)] public string? CreatedBy { get; set; }
    [MaxLength(100)] public string? UpdatedBy { get; set; }

    public void Assign(CongBoNangSuatLua candidate)
    {
        MaTinhThanh = candidate.MaTinhThanh;
        TenTinhThanh = candidate.TenTinhThanh;
        MuaVu = candidate.MuaVu;
        Nam = candidate.Nam;
        Thang = candidate.Thang;
        NangSuatLua = candidate.NangSuatLua;
        NangSuatLuaUocTinh = candidate.NangSuatLuaUocTinh;
        DonViTinh = candidate.DonViTinh;
        IsCongBo = candidate.IsCongBo;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = candidate.UpdatedBy;
    }
}

public class CongBoSanLuongLua
{
    [Key] public int Id { get; set; }
    public string MaTinhThanh { get; set; } = string.Empty;
    public string TenTinhThanh { get; set; } = string.Empty;
    public string MuaVu { get; set; } = string.Empty;
    public int Nam { get; set; }
    public int Thang { get; set; }
    public double SanLuongLua { get; set; }
    public double SanLuongLuaUocTinh { get; set; }
    public string DonViTinh { get; set; } = string.Empty;
    [NotMapped] public double TongSanLuongLua { get; set; } = 0;
    [NotMapped] public double TongSanLuongLuaUocTinh { get; set; } = 0;
    public int IsCongBo { get; set; } // 0: Chưa công bố, 1: Đã công bố
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    [MaxLength(100)] public string? CreatedBy { get; set; }
    [MaxLength(100)] public string? UpdatedBy { get; set; }

    public void Assign(CongBoSanLuongLua candidate)
    {
        MaTinhThanh = candidate.MaTinhThanh;
        TenTinhThanh = candidate.TenTinhThanh;
        MuaVu = candidate.MuaVu;
        Nam = candidate.Nam;
        Thang = candidate.Thang;
        SanLuongLua = candidate.SanLuongLua;
        SanLuongLuaUocTinh = candidate.SanLuongLuaUocTinh;
        DonViTinh = candidate.DonViTinh;
        IsCongBo = candidate.IsCongBo;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = candidate.UpdatedBy;
    }
}

public class CongBoSanLuongCayHangNamKhac
{
    [Key] public int Id { get; set; }
    public string MaTinhThanh { get; set; } = string.Empty;
    public string TenTinhThanh { get; set; } = string.Empty;
    public string LoaiCay { get; set; } = string.Empty;
    public string MuaVu { get; set; } = string.Empty;
    public int Nam { get; set; }
    public int Thang { get; set; }
    public double SanLuong { get; set; }
    public double SanLuongUocTinh { get; set; }
    public string DonViTinh { get; set; } = string.Empty;
    [NotMapped] public double TongSanLuong { get; set; } = 0;
    [NotMapped] public double TongSanLuongUocTinh { get; set; } = 0;
    public int IsCongBo { get; set; } // 0: Chưa công bố, 1: Đã công bố
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(100)] public string? CreatedBy { get; set; }

    [MaxLength(100)] public string? UpdatedBy { get; set; }

    public void Assign(CongBoSanLuongCayHangNamKhac candidate)
    {
        MaTinhThanh = candidate.MaTinhThanh;
        TenTinhThanh = candidate.TenTinhThanh;
        LoaiCay = candidate.LoaiCay;
        MuaVu = candidate.MuaVu;
        Nam = candidate.Nam;
        Thang = candidate.Thang;
        SanLuong = candidate.SanLuong;
        SanLuongUocTinh = candidate.SanLuongUocTinh;
        DonViTinh = candidate.DonViTinh;
        IsCongBo = candidate.IsCongBo;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = candidate.UpdatedBy;
    }
}

public class CongBoDienTichCayLauNamChinh
{
    [Key] public int Id { get; set; }
    public string MaTinhThanh { get; set; } = string.Empty;
    public string TenTinhThanh { get; set; } = string.Empty;
    public string LoaiCay { get; set; } = string.Empty;
    public string MuaVu { get; set; } = string.Empty;
    public int Nam { get; set; }
    public int Thang { get; set; }
    public double DienTichThuHoach { get; set; }
    public double DienTichGieoTrong { get; set; }
    public double DienTichUocTinhGieoTrong { get; set; }
    public double DienTichUocTinhThuHoach { get; set; }
    public string DonViTinh { get; set; } = string.Empty;
    [NotMapped] public double TongDienTichThuHoach { get; set; } = 0;
    [NotMapped] public double TongDienTichGieoTrong { get; set; } = 0;
    [NotMapped] public double TongDienTichUocTinhGieoTrong { get; set; } = 0;
    [NotMapped] public double TongDienTichUocTinhThuHoach { get; set; } = 0;
    public int IsCongBo { get; set; } // 0: Chưa công bố, 1: Đã công bố
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(100)] public string? CreatedBy { get; set; }

    [MaxLength(100)] public string? UpdatedBy { get; set; }

    public void Assign(CongBoDienTichCayLauNamChinh candidate)
    {
        MaTinhThanh = candidate.MaTinhThanh;
        TenTinhThanh = candidate.TenTinhThanh;
        LoaiCay = candidate.LoaiCay;
        MuaVu = candidate.MuaVu;
        Nam = candidate.Nam;
        Thang = candidate.Thang;
        DienTichThuHoach = candidate.DienTichThuHoach;
        DienTichGieoTrong = candidate.DienTichGieoTrong;
        DienTichUocTinhGieoTrong = candidate.DienTichUocTinhGieoTrong;
        DienTichUocTinhThuHoach = candidate.DienTichUocTinhThuHoach;
        DonViTinh = candidate.DonViTinh;
        IsCongBo = candidate.IsCongBo;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = candidate.UpdatedBy;
    }
}

public class CongBoSanLuongCayLauNamChinh
{
    [Key] public int Id { get; set; }
    public string MaTinhThanh { get; set; } = string.Empty;
    public string TenTinhThanh { get; set; } = string.Empty;
    public string LoaiCay { get; set; } = string.Empty;
    public string MuaVu { get; set; } = string.Empty;
    public int Nam { get; set; }
    public int Thang { get; set; }
    public double SanLuong { get; set; }
    public double SanLuongUocTinh { get; set; }
    public string DonViTinh { get; set; } = string.Empty;
    [NotMapped] public double TongSanLuong { get; set; } = 0;
    [NotMapped] public double TongSanLuongUocTinh { get; set; } = 0;
    public int IsCongBo { get; set; } // 0: Chưa công bố, 1: Đã công bố
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(100)] public string? CreatedBy { get; set; }

    [MaxLength(100)] public string? UpdatedBy { get; set; }

    public void Assign(CongBoSanLuongCayLauNamChinh candidate)
    {
        MaTinhThanh = candidate.MaTinhThanh;
        TenTinhThanh = candidate.TenTinhThanh;
        LoaiCay = candidate.LoaiCay;
        MuaVu = candidate.MuaVu;
        Nam = candidate.Nam;
        Thang = candidate.Thang;
        SanLuong = candidate.SanLuong;
        SanLuongUocTinh = candidate.SanLuongUocTinh;
        DonViTinh = candidate.DonViTinh;
        IsCongBo = candidate.IsCongBo;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = candidate.UpdatedBy;
    }
}

public class CongBoSoLuongGiaSucGiaCam
{
    [Key] public int Id { get; set; }
    public string MaTinhThanh { get; set; } = string.Empty;
    public string TenTinhThanh { get; set; } = string.Empty;
    public string LoaiVatNuoi { get; set; } = string.Empty;
    public int Nam { get; set; }
    public int Thang { get; set; }
    public double SoLuong { get; set; }
    public double SoLuongUocTinh { get; set; }
    public string DonViTinh { get; set; } = string.Empty;
    [NotMapped] public double TongSoLuong { get; set; } = 0;
    [NotMapped] public double TongSoLuongUocTinh { get; set; } = 0;
    public int IsCongBo { get; set; } // 0: Chưa công bố, 1: Đã công bố
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(100)] public string? CreatedBy { get; set; }

    [MaxLength(100)] public string? UpdatedBy { get; set; }

    public void Assign(CongBoSoLuongGiaSucGiaCam candidate)
    {
        MaTinhThanh = candidate.MaTinhThanh;
        TenTinhThanh = candidate.TenTinhThanh;
        LoaiVatNuoi = candidate.LoaiVatNuoi;
        Nam = candidate.Nam;
        Thang = candidate.Thang;
        SoLuong = candidate.SoLuong;
        SoLuongUocTinh = candidate.SoLuongUocTinh;
        DonViTinh = candidate.DonViTinh;
        IsCongBo = candidate.IsCongBo;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = candidate.UpdatedBy;
    }
}

public class CongBoDienTichNuoiTrongThuySan
{
    [Key] public int Id { get; set; }
    public string MaTinhThanh { get; set; } = string.Empty;
    public string TenTinhThanh { get; set; } = string.Empty;
    public string LoaiThuySanChinh { get; set; } = string.Empty;
    public string MoiTruongNuoi { get; set; } = string.Empty;
    public int Nam { get; set; }
    public int Thang { get; set; }
    public double DienTich { get; set; }
    public double DienTichUocTinh { get; set; }
    public string DonViTinh { get; set; } = string.Empty;
    [NotMapped] public double TongDienTich { get; set; } = 0;
    [NotMapped] public double TongDienTichUocTinh { get; set; } = 0;
    public int IsCongBo { get; set; } // 0: Chưa công bố, 1: Đã công bố
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(100)] public string? CreatedBy { get; set; }

    [MaxLength(100)] public string? UpdatedBy { get; set; }

    public void Assign(CongBoDienTichNuoiTrongThuySan candidate)
    {
        MaTinhThanh = candidate.MaTinhThanh;
        TenTinhThanh = candidate.TenTinhThanh;
        LoaiThuySanChinh = candidate.LoaiThuySanChinh;
        MoiTruongNuoi = candidate.MoiTruongNuoi;
        Nam = candidate.Nam;
        Thang = candidate.Thang;
        DienTich = candidate.DienTich;
        DienTichUocTinh = candidate.DienTichUocTinh;
        DonViTinh = candidate.DonViTinh;
        IsCongBo = candidate.IsCongBo;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = candidate.UpdatedBy;
    }
}

public class CongBoSanLuongNuoiTrongThuySan
{
    [Key] public int Id { get; set; }
    public string MaTinhThanh { get; set; } = string.Empty;
    public string TenTinhThanh { get; set; } = string.Empty;
    public string LoaiThuySanChinh { get; set; } = string.Empty;
    public string MoiTruongNuoi { get; set; } = string.Empty;
    public int Nam { get; set; }
    public int Thang { get; set; }
    public double SanLuong { get; set; }
    public double SanLuongUocTinh { get; set; }
    public string DonViTinh { get; set; } = string.Empty;
    [NotMapped] public double TongSanLuong { get; set; } = 0;
    [NotMapped] public double TongSanLuongUocTinh { get; set; } = 0;
    public int IsCongBo { get; set; } // 0: Chưa công bố, 1: Đã công bố
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(100)] public string? CreatedBy { get; set; }

    [MaxLength(100)] public string? UpdatedBy { get; set; }

    public void Assign(CongBoSanLuongNuoiTrongThuySan candidate)
    {
        MaTinhThanh = candidate.MaTinhThanh;
        TenTinhThanh = candidate.TenTinhThanh;
        LoaiThuySanChinh = candidate.LoaiThuySanChinh;
        MoiTruongNuoi = candidate.MoiTruongNuoi;
        Nam = candidate.Nam;
        Thang = candidate.Thang;
        SanLuong = candidate.SanLuong;
        SanLuongUocTinh = candidate.SanLuongUocTinh;
        DonViTinh = candidate.DonViTinh;
        IsCongBo = candidate.IsCongBo;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = candidate.UpdatedBy;
    }
}

public class CongBoSanLuongThuySanKhaiThac
{
    [Key] public int Id { get; set; }
    public string MaTinhThanh { get; set; } = string.Empty;
    public string TenTinhThanh { get; set; } = string.Empty;
    public string LoaiThuySanChinh { get; set; } = string.Empty;
    public int Nam { get; set; }
    public int Thang { get; set; }
    public double SanLuong { get; set; }
    public double SanLuongUocTinh { get; set; }
    public string DonViTinh { get; set; } = string.Empty;
    [NotMapped] public double TongSanLuong { get; set; } = 0;
    [NotMapped] public double TongSanLuongUocTinh { get; set; } = 0;
    public int IsCongBo { get; set; } // 0: Chưa công bố, 1: Đã công bố
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(100)] public string? CreatedBy { get; set; }

    [MaxLength(100)] public string? UpdatedBy { get; set; }

    public void Assign(CongBoSanLuongThuySanKhaiThac candidate)
    {
        MaTinhThanh = candidate.MaTinhThanh;
        TenTinhThanh = candidate.TenTinhThanh;
        LoaiThuySanChinh = candidate.LoaiThuySanChinh;
        Nam = candidate.Nam;
        Thang = candidate.Thang;
        SanLuong = candidate.SanLuong;
        SanLuongUocTinh = candidate.SanLuongUocTinh;
        DonViTinh = candidate.DonViTinh;
        IsCongBo = candidate.IsCongBo;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = candidate.UpdatedBy;
    }
}

public class CongBoGiaSanPhamNLTSChinh
{
    [Key] public int Id { get; set; }
    public string MaTinhThanh { get; set; } = string.Empty;
    public string TenTinhThanh { get; set; } = string.Empty;
    public string SanPhamNLTS { get; set; } = string.Empty;
    public string LoaiGia { get; set; } = string.Empty;
    public int Nam { get; set; }
    public int Thang { get; set; }
    public double GiaTri { get; set; }
    public double GiaTriUocTinh { get; set; }
    public string DonViTinh { get; set; } = string.Empty;
    [NotMapped] public double TongGiaTri { get; set; } = 0;
    [NotMapped] public double TongGiaTriUocTinh { get; set; } = 0;
    public int IsCongBo { get; set; } // 0: Chưa công bố, 1: Đã công bố
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(100)] public string? CreatedBy { get; set; }

    [MaxLength(100)] public string? UpdatedBy { get; set; }

    public void Assign(CongBoGiaSanPhamNLTSChinh candidate)
    {
        MaTinhThanh = candidate.MaTinhThanh;
        TenTinhThanh = candidate.TenTinhThanh;
        SanPhamNLTS = candidate.SanPhamNLTS;
        LoaiGia = candidate.LoaiGia;
        Nam = candidate.Nam;
        Thang = candidate.Thang;
        GiaTri = candidate.GiaTri;
        GiaTriUocTinh = candidate.GiaTriUocTinh;
        DonViTinh = candidate.DonViTinh;
        IsCongBo = candidate.IsCongBo;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = candidate.UpdatedBy;
    }
}

public class CongBoXuatKhauThuySan
{
    [Key] public int Id { get; set; }
    public string MaQuocGiaVungLT { get; set; } = string.Empty;
    public string TenQuocGiaVungLT { get; set; } = string.Empty;
    public int Nam { get; set; }
    public int Thang { get; set; }
    public string DonViGiaTri { get; set; } = string.Empty;
    public double GiaTri { get; set; }
    public string DonViGiaTriUocTinh { get; set; } = string.Empty;
    public double GiaTriUocTinh { get; set; }
    public int IsCongBo { get; set; } // 0: Chưa công bố, 1: Đã công bố
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    [MaxLength(100)] public string? CreatedBy { get; set; }
    [MaxLength(100)] public string? UpdatedBy { get; set; }

    public void Assign(CongBoXuatKhauThuySan candidate)
    {
        MaQuocGiaVungLT = candidate.MaQuocGiaVungLT;
        TenQuocGiaVungLT = candidate.TenQuocGiaVungLT;
        Nam = candidate.Nam;
        Thang = candidate.Thang;
        DonViGiaTri = candidate.DonViGiaTri;
        GiaTri = candidate.GiaTri;
        DonViGiaTriUocTinh = candidate.DonViGiaTriUocTinh;
        GiaTriUocTinh = candidate.GiaTriUocTinh;
        IsCongBo = candidate.IsCongBo;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = candidate.UpdatedBy;
    }
}
public class CongBoXuatKhauSanPhamChanNuoi
{
    [Key] public int Id { get; set; }
    public string MaQuocGiaVungLT { get; set; } = string.Empty;
    public string TenQuocGiaVungLT { get; set; } = string.Empty;
    public int Nam { get; set; }
    public int Thang { get; set; }
    public string LoaiSanPhamChanNuoi { get; set; } = string.Empty; // Cá tra; Tôm (required value, select option, use value here)
    public string DonViGiaTri { get; set; } = string.Empty;
    public double GiaTri { get; set; }
    public string DonViGiaTriUocTinh { get; set; } = string.Empty;
    public double GiaTriUocTinh { get; set; }
    public int IsCongBo { get; set; } // 0: Chưa công bố, 1: Đã công bố
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    [MaxLength(100)] public string? CreatedBy { get; set; }
    [MaxLength(100)] public string? UpdatedBy { get; set; }

    public void Assign(CongBoXuatKhauSanPhamChanNuoi candidate)
    {
        MaQuocGiaVungLT = candidate.MaQuocGiaVungLT;
        TenQuocGiaVungLT = candidate.TenQuocGiaVungLT;
        Nam = candidate.Nam;
        Thang = candidate.Thang;
        LoaiSanPhamChanNuoi = candidate.LoaiSanPhamChanNuoi;
        DonViGiaTri = candidate.DonViGiaTri;
        GiaTri = candidate.GiaTri;
        DonViGiaTriUocTinh = candidate.DonViGiaTriUocTinh;
        GiaTriUocTinh = candidate.GiaTriUocTinh;
        IsCongBo = candidate.IsCongBo;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = candidate.UpdatedBy;
    }
}

public class CongBoXuatKhauLamSan
{
    [Key] public int Id { get; set; }
    public string MaQuocGiaVungLT { get; set; } = string.Empty;
    public string TenQuocGiaVungLT { get; set; } = string.Empty;
    public int Nam { get; set; }
    public int Thang { get; set; }
    public string LoaiLamSan { get; set; } = string.Empty; // Gỗ và sản phẩm gỗ; Sản phẩm mây, tre, cói và thảm(2 options) (required value, select option, use value here)
    public string DonViGiaTri { get; set; } = string.Empty;
    public double GiaTri { get; set; }
    public string DonViGiaTriUocTinh { get; set; } = string.Empty;
    public double GiaTriUocTinh { get; set; }
    public int IsCongBo { get; set; } // 0: Chưa công bố, 1: Đã công bố
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    [MaxLength(100)] public string? CreatedBy { get; set; }
    [MaxLength(100)] public string? UpdatedBy { get; set; }

    public void Assign(CongBoXuatKhauLamSan candidate)
    {
        MaQuocGiaVungLT = candidate.MaQuocGiaVungLT;
        TenQuocGiaVungLT = candidate.TenQuocGiaVungLT;
        Nam = candidate.Nam;
        Thang = candidate.Thang;
        LoaiLamSan = candidate.LoaiLamSan;
        DonViGiaTri = candidate.DonViGiaTri;
        GiaTri = candidate.GiaTri;
        DonViGiaTriUocTinh = candidate.DonViGiaTriUocTinh;
        GiaTriUocTinh = candidate.GiaTriUocTinh;
        IsCongBo = candidate.IsCongBo;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = candidate.UpdatedBy;
    }
}

public class CongBoXuatKhauDauVaoSanXuat
{
    [Key] public int Id { get; set; }
    public int Nam { get; set; }
    public int Thang { get; set; }
    public string DonViGiaTri { get; set; } = string.Empty;
    public double GiaTri { get; set; }
    public string DonViGiaTriUocTinh { get; set; } = string.Empty;
    public double GiaTriUocTinh { get; set; }
    public int IsCongBo { get; set; } // 0: Chưa công bố, 1: Đã công bố
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    [MaxLength(100)] public string? CreatedBy { get; set; }
    [MaxLength(100)] public string? UpdatedBy { get; set; }

    public void Assign(CongBoXuatKhauDauVaoSanXuat candidate)
    {
        Nam = candidate.Nam;
        Thang = candidate.Thang;
        DonViGiaTri = candidate.DonViGiaTri;
        GiaTri = candidate.GiaTri;
        DonViGiaTriUocTinh = candidate.DonViGiaTriUocTinh;
        GiaTriUocTinh = candidate.GiaTriUocTinh;
        IsCongBo = candidate.IsCongBo;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = candidate.UpdatedBy;
    }
}

public class CongBoXuatKhauMuoi
{
    [Key] public int Id { get; set; }
    public string MaQuocGiaVungLT { get; set; } = string.Empty;
    public string TenQuocGiaVungLT { get; set; } = string.Empty;
    public int Nam { get; set; }
    public int Thang { get; set; }
    public string DonViGiaTri { get; set; } = string.Empty;
    public double GiaTri { get; set; }
    public string DonViGiaTriUocTinh { get; set; } = string.Empty;
    public double GiaTriUocTinh { get; set; }
    public int IsCongBo { get; set; } // 0: Chưa công bố, 1: Đã công bố
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    [MaxLength(100)] public string? CreatedBy { get; set; }
    [MaxLength(100)] public string? UpdatedBy { get; set; }

    public void Assign(CongBoXuatKhauMuoi candidate)
    {
        MaQuocGiaVungLT = candidate.MaQuocGiaVungLT;
        TenQuocGiaVungLT = candidate.TenQuocGiaVungLT;
        Nam = candidate.Nam;
        Thang = candidate.Thang;
        DonViGiaTri = candidate.DonViGiaTri;
        GiaTri = candidate.GiaTri;
        DonViGiaTriUocTinh = candidate.DonViGiaTriUocTinh;
        GiaTriUocTinh = candidate.GiaTriUocTinh;
        IsCongBo = candidate.IsCongBo;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = candidate.UpdatedBy;
    }
}

public class CongBoNhapKhau
{
    [Key] public int Id { get; set; }
    public string MaQuocGiaVungLT { get; set; } = string.Empty;
    public string TenQuocGiaVungLT { get; set; } = string.Empty;
    public int Nam { get; set; }
    public int Thang { get; set; }
    public string LoaiSanPham { get; set; } = string.Empty; // Nông sản, Sản phẩm chăn nuôi, Thủy sản, Lâm sản, Đầu vào sản xuất, Muối (required value, select option, use value here)
    public string DonViGiaTri { get; set; } = string.Empty;
    public double GiaTri { get; set; }
    public string DonViGiaTriUocTinh { get; set; } = string.Empty;
    public double GiaTriUocTinh { get; set; }
    public int IsCongBo { get; set; } // 0: Chưa công bố, 1: Đã công bố
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    [MaxLength(100)] public string? CreatedBy { get; set; }
    [MaxLength(100)] public string? UpdatedBy { get; set; }

    public void Assign(CongBoNhapKhau candidate)
    {
        MaQuocGiaVungLT = candidate.MaQuocGiaVungLT;
        TenQuocGiaVungLT = candidate.TenQuocGiaVungLT;
        Nam = candidate.Nam;
        Thang = candidate.Thang;
        LoaiSanPham = candidate.LoaiSanPham;
        DonViGiaTri = candidate.DonViGiaTri;
        GiaTri = candidate.GiaTri;
        DonViGiaTriUocTinh = candidate.DonViGiaTriUocTinh;
        GiaTriUocTinh = candidate.GiaTriUocTinh;
        IsCongBo = candidate.IsCongBo;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = candidate.UpdatedBy;
    }
}

public class CongBoCanCanThuongMai
{
    [Key] public int Id { get; set; }
    public int Nam { get; set; }
    public int Thang { get; set; }
    public string DonViGiaTri { get; set; } = string.Empty;
    public double GiaTri { get; set; }
    public string DonViGiaTriUocTinh { get; set; } = string.Empty;
    public double GiaTriUocTinh { get; set; }
    public int IsCongBo { get; set; } // 0: Chưa công bố, 1: Đã công bố
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    [MaxLength(100)] public string? CreatedBy { get; set; }
    [MaxLength(100)] public string? UpdatedBy { get; set; }

    public void Assign(CongBoCanCanThuongMai candidate)
    {
        Nam = candidate.Nam;
        Thang = candidate.Thang;
        DonViGiaTri = candidate.DonViGiaTri;
        GiaTri = candidate.GiaTri;
        DonViGiaTriUocTinh = candidate.DonViGiaTriUocTinh;
        GiaTriUocTinh = candidate.GiaTriUocTinh;
        IsCongBo = candidate.IsCongBo;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = candidate.UpdatedBy;
    }
}