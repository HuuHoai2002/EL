using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ThongKe.Shared.Enums;
using ThongKe.Shared.Extensions;
using ThongKe.Shared.Utils;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace ThongKe.Entities;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable InconsistentNaming
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable  CollectionNeverUpdated.Global
// ReSharper disable  UnusedAutoPropertyAccessor.Global

// TODO move index to db context
// TODO move constraint of attr to API, & db context if need
[Index(nameof(CreatedBy))]
[Index(nameof(UpdatedBy))]
[Index(nameof(DonViId))]
public class BieuMau
{
    [Key] public int Id { get; set; }

    // TODO MaBieuMau
    [Required] [MaxLength(500)] public string TenBieuMau { get; set; } = string.Empty;
    [MaxLength(1000)] public string? MoTa { get; set; }

    // TODO remove, use DonVi in ChiTieu
    public int? DonViId { get; set; }

    // TODO use constants
    public string? KyBaoCao { get; set; }

    // TODO remove ChiTieuIds, value alrealdy has in ChiTieus
    [Required]
    [Length(1, 100)]
    [NotMapped]
    public List<int> ChiTieuIds { get; set; } = [];

    [NotMapped] public List<ChiTieu> ChiTieus { get; set; } = [];

    [JsonIgnore]
    [Column(TypeName = "CLOB")]
    public string ChiTieusJSON { get; set; } = string.Empty;

    public string? SoBieuMau { get; set; }
    public int? SoThongTu { get; set; }
    public string? ThongTu { get; set; }

    [Range(1, 12)] public int? Thang { get; set; }
    [Range(1900, 3000)] public int? Nam { get; set; }

    public string? NgayLapBieu { get; set; }
    public string? NguoiLapBieu { get; set; }

    public string? DonViBaoCao { get; set; }
    public string? DonViNhanBaoCao { get; set; }

    // TODO why has attr?
    // TODO should use int64 or DateTime and string if need
    [NotMapped] public List<string> NgayNhanBaoCao { get; set; } = [];

    [JsonIgnore]
    [Column(TypeName = "CLOB")]
    public string? NgayNhanBaoCaoJSON { get; set; } = string.Empty;

    // TODO replace by 1 dymanic JSON, backend should not care about this, should for frontend decise
    [JsonIgnore]
    [Column(TypeName = "CLOB")]
    public string CotsJSON { get; set; } = string.Empty;

    [NotMapped] public List<Dictionary<string, object?>> Cots { get; set; } = [];

    [JsonIgnore]
    [Column(TypeName = "CLOB")]
    public string HangsJSON { get; set; } = string.Empty;

    [NotMapped] public List<Dictionary<string, object?>> Hangs { get; set; } = [];

    // meta
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    [MaxLength(100)] public string? CreatedBy { get; set; }
    [MaxLength(100)] public string? UpdatedBy { get; set; }

    public void Assign(BieuMau bieuMau)
    {
        TenBieuMau = bieuMau.TenBieuMau;
        MoTa = bieuMau.MoTa;
        DonViId = bieuMau.DonViId;
        KyBaoCao = bieuMau.KyBaoCao;
        ChiTieuIds = bieuMau.ChiTieuIds;
        SoBieuMau = bieuMau.SoBieuMau;
        SoThongTu = bieuMau.SoThongTu;
        ThongTu = bieuMau.ThongTu;
        Thang = bieuMau.Thang;
        Nam = bieuMau.Nam;
        NgayLapBieu = bieuMau.NgayLapBieu;
        NguoiLapBieu = bieuMau.NguoiLapBieu;
        DonViBaoCao = bieuMau.DonViBaoCao;
        DonViNhanBaoCao = bieuMau.DonViNhanBaoCao;
        Cots = bieuMau.Cots;
        CotsJSON = JsonConvert.SerializeObject(Cots);
        Hangs = bieuMau.Hangs;
        HangsJSON = JsonConvert.SerializeObject(Hangs);
        ChiTieus = bieuMau.ChiTieus;
        ChiTieusJSON = JsonConvert.SerializeObject(ChiTieus);
        NgayNhanBaoCao = bieuMau.NgayNhanBaoCao;
        NgayNhanBaoCaoJSON = JsonConvert.SerializeObject(NgayNhanBaoCao);
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = bieuMau.UpdatedBy;
    }

    public void Deserialize()
    {
        if (CotsJSON.Length > 0)
            Cots = JsonConvert.DeserializeObject<List<Dictionary<string, object?>>>(CotsJSON) ?? [];
        if (HangsJSON.Length > 0)
            Hangs = JsonConvert.DeserializeObject<List<Dictionary<string, object?>>>(HangsJSON) ?? [];
        if (NgayNhanBaoCaoJSON is { Length: > 0 })
            NgayNhanBaoCao = JsonConvert.DeserializeObject<List<string>>(NgayNhanBaoCaoJSON) ?? [];
        if (ChiTieusJSON.Length <= 0) return;
        var chiTieus = JsonConvert.DeserializeObject<List<ChiTieu>>(ChiTieusJSON) ?? [];
        ChiTieus = chiTieus;
        ChiTieuIds = chiTieus.Select(x => x.Id).ToList();
    }
}

// TODO move index to db context
[Index(nameof(TenChiTieu), IsUnique = true)]
[Index(nameof(CreatedBy))]
[Index(nameof(UpdatedBy))]
[Index(nameof(DonViId))]
[Index(nameof(ParentId))]
[Index(nameof(TrangThai))]
public class ChiTieu : BasicInfo
{
    [Key] public int Id { get; set; }

    public string? Ma { get; set; }
    public string? Ten { get; set; }
    [MaxLength(2000)] public string? MoTa { get; set; }

    public TrangThaiChiTieu? MaTrangThai { get; set; } = TrangThaiChiTieu.NonPublished;
    [NotMapped] public string TenTrangThai => EnumUtils.GetDisplayName(TrangThai);

    [NotMapped] public List<PhanTo> PhanTos { get; set; } = [];

    [JsonIgnore]
    [Column(TypeName = "CLOB")]
    public string PhanTosJSON { get; set; } = string.Empty;

    [NotMapped] public List<ChiTieu> BienThes { get; set; } = []; // for response only

    [JsonIgnore]
    [Column(TypeName = "CLOB")]
    public string? BienThesJSON { get; set; } = string.Empty;

    // TODO replace by consts
    public int? DonViId { get; set; }

    // meta
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    [MaxLength(100)] public string? UpdatedBy { get; set; }

    // TODO remove
    [MaxLength(1000)] public string TenChiTieu { get; set; }
    public TrangThaiChiTieu TrangThai { get; set; } = TrangThaiChiTieu.NonPublished;
    [JsonIgnore] public string Hash { get; set; } = string.Empty; // MD5 hash only in server
    public int? ParentId { get; set; } // for hierarchical criteria
    public int IsBienThe { get; set; } = Constants.OracleBoolean.FALSE;
    [NotMapped] public int SoLuongPhanTo { get; set; } // for display only

    // TODO replace by other function
    [NotMapped]
    public string ColumnName =>
        $"chi_tieu_{TenChiTieu.RemoveVietnameseAccents().ToSnakeCase()}_$${Id}";

    [NotMapped]
    public string TableName =>
        $"BangChiTieu_{TenChiTieu.RemoveVietnameseAccents().ToSnakeCase()}_$${Id}";

    // TODO 1 ChiTieu has many bang_chi_tieu


    public void Assign(ChiTieu chiTieu)
    {
        Ma = chiTieu.Ma;
        Ten = chiTieu.Ten;
        TenChiTieu = chiTieu.TenChiTieu.RemoveSpecialCharactersUnicode();
        MoTa = chiTieu.MoTa;
        MaTrangThai = chiTieu.MaTrangThai;
        TrangThai = chiTieu.TrangThai;
        DonViId = chiTieu.DonViId;
        PhanTos = chiTieu.PhanTos;
        PhanTosJSON = JsonSerializer.Serialize(PhanTos);
        BienThes = chiTieu.BienThes;
        BienThesJSON = JsonSerializer.Serialize(BienThes);
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = chiTieu.UpdatedBy;
        Hash = HashingUtils.HashMd5(chiTieu.TenChiTieu.ToLower()); // TODO why? hash only base on TenChiTieu for what?
        ParentId = chiTieu.ParentId;
    }

    public void Deserialize()
    {
        if (PhanTosJSON.Length <= 0) return;
        PhanTos = JsonSerializer.Deserialize<List<PhanTo>>(PhanTosJSON) ?? [];
        SoLuongPhanTo = PhanTos?.Count ?? 0;
    }
}

public class PhanToProperty
{
    public bool IsRequired { get; set; } = false;
    public double? MinValue { get; set; }
    public double? MaxValue { get; set; }
    public int? MaxLength { get; set; }
    public int? MinLength { get; set; }
    public List<string> AllowedValues { get; set; } = [];
    public string? RegexPattern { get; set; }
    public string? DateFormat { get; set; }
}

// ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
// ReSharper disable once MemberCanBePrivate.Global
// ReSharper disable once ClassNeverInstantiated.Global
public class PhanTo
{
    // TODO use consts
    public string DataType { get; set; } = string.Empty;

    public string DataTypeText =>
        Constants.LoaiDuLieus.FirstOrDefault(x => x.Value == DataType)?.Label ?? "Không xác định";

    // TODO replace by map function
    [JsonIgnore] public string OracleDbType { get; set; } = string.Empty;

    [MaxLength(200)] public string TenPhanTo { get; set; } = string.Empty;
    [MaxLength(500)] public string? MoTa { get; set; } = string.Empty;
    [Range(1, 10)] public int Index { get; set; }

    // TODO remove
    public List<PhanTo> Children { get; set; } = [];

    // TODO replace by 1 DanhMuc and used level(default value is 1)
    public List<DanhMuc> DanhMucs { get; set; } = [];

    // TODO replace by function
    [MaxLength(255)] public string ColumnName { get; set; } = string.Empty;

    // TODO belongs to DataType??
    // TODO should use directly not other class
    public PhanToProperty Property { get; set; } = new();
}

[Index(nameof(CreatedBy))]
[Index(nameof(UpdatedBy))]
[Index(nameof(TableName), IsUnique = true)]
[Index(nameof(HashingSchema), IsUnique = true)]
[Index(nameof(PhienBanChiTieuId))]
public class KieuBangThongKe : BasicInfo
{
    [Key] public int Id { get; set; }

    [MaxLength(1000)] public string Ten { get; set; } = string.Empty;

    [MaxLength(1000)] public string? MoTa { get; set; }

    [JsonIgnore]
    [Column(TypeName = "CLOB")]
    public string ChiTieusJSON { get; set; } = string.Empty;

    [NotMapped] public List<ChiTieu> ChiTieus { get; set; } = [];

    [NotMapped] public List<int>? ChiTieuIds { get; set; } // for input only

    public int PhienBanChiTieuId { get; set; }

    public string? TableName { get; set; } // for dynamic table name in database

    public string HashingSchema { get; set; } = string.Empty;

    [NotMapped] public bool HasTableInDatabase => !string.IsNullOrEmpty(TableName); // for display only

    [NotMapped] public int SoLuongChiTieu { get; set; } // for display only

    [MaxLength(100)] public string? UpdatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastTableCreatedAt { get; set; }

    public void Assign(KieuBangThongKe kieuBangThongKe)
    {
        Ten = kieuBangThongKe.Ten;
        MoTa = kieuBangThongKe.MoTa;
        ChiTieusJSON = JsonSerializer.Serialize(kieuBangThongKe.ChiTieus);
        ChiTieus = kieuBangThongKe.ChiTieus;
        SoLuongChiTieu = kieuBangThongKe.ChiTieus.Count;
        PhienBanChiTieuId = kieuBangThongKe.PhienBanChiTieuId;
        TableName = kieuBangThongKe.TableName;
        LastTableCreatedAt = kieuBangThongKe.LastTableCreatedAt;
        HashingSchema = kieuBangThongKe.HashingSchema;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deserialize()
    {
        if (ChiTieusJSON.Length <= 0) return;
        var chiTieus = JsonSerializer.Deserialize<List<ChiTieu>>(ChiTieusJSON);
        ChiTieus = chiTieus ?? [];
        SoLuongChiTieu = chiTieus?.Count ?? 0;
        ChiTieuIds = chiTieus?.Select(t => t.Id).ToList() ?? [];
    }
}

// TODO replace DuLieuChiTieu by bang_chi_tieu & attr data_tables of ChiTieu
// TODO allow user change current(default) bang_chi_tieu or select bang_chi_tieu when import data
[Index(nameof(CreatedBy))]
[Index(nameof(PhanToId1))]
[Index(nameof(PhanToId2))]
[Index(nameof(PhanToId3))]
[Index(nameof(PhanToId4))]
[Index(nameof(PhanToId5))]
[Index(nameof(PhanToId6))]
[Index(nameof(PhanToId7))]
[Index(nameof(PhanToId8))]
[Index(nameof(PhanToId9))]
[Index(nameof(PhanToId10))]
[Index(nameof(ChiTieuId))]
[Index(nameof(ImportId))]
public class DuLieuChiTieu : BasicInfo
{
    [Key] public int Id { get; set; }

    public double Data { get; set; }

    public int? ChiTieuId { get; set; }

    public int? PhanToId1 { get; set; }
    public int? PhanToId2 { get; set; }
    public int? PhanToId3 { get; set; }
    public int? PhanToId4 { get; set; }
    public int? PhanToId5 { get; set; }
    public int? PhanToId6 { get; set; }
    public int? PhanToId7 { get; set; }
    public int? PhanToId8 { get; set; }
    public int? PhanToId9 { get; set; }
    public int? PhanToId10 { get; set; }

    [NotMapped] public string? ErrorMessage { get; set; } = null; // Only in server

    [NotMapped] public bool HasError => !string.IsNullOrEmpty(ErrorMessage); // Only in server

    [NotMapped] public string Hash { get; set; } = Guid.NewGuid().ToString();

    public int? ImportId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(100)] public string? UpdatedBy { get; set; }

    public void Assign(DuLieuChiTieu duLieuChiTieu)
    {
        Data = duLieuChiTieu.Data;
        PhanToId1 = duLieuChiTieu.PhanToId1;
        PhanToId2 = duLieuChiTieu.PhanToId2;
        PhanToId3 = duLieuChiTieu.PhanToId3;
        PhanToId4 = duLieuChiTieu.PhanToId4;
        PhanToId5 = duLieuChiTieu.PhanToId5;
        PhanToId6 = duLieuChiTieu.PhanToId6;
        PhanToId7 = duLieuChiTieu.PhanToId7;
        PhanToId8 = duLieuChiTieu.PhanToId8;
        PhanToId9 = duLieuChiTieu.PhanToId9;
        PhanToId10 = duLieuChiTieu.PhanToId10;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = duLieuChiTieu.UpdatedBy;
    }
}

[Index(nameof(CreatedBy))]
[Index(nameof(UpdatedBy))]
[Index(nameof(BieuMauId))]
public class DuLieuBieuMau
{
    [Key] public int Id { get; set; }

    [Required] public int BieuMauId { get; set; }

    public string? NguonDuLieu { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // TODO not found any logic in API use this for update data to bang_chi_tieu?
    [NotMapped] public List<Dictionary<string, object?>> DongDuLieus { get; set; } = [];

    [NotMapped] public BieuMau? BieuMau { get; set; }

    [JsonIgnore]
    [Column(TypeName = "CLOB")]
    // ReSharper disable once InconsistentNaming
    public string DongDuLieusJSON { get; set; } = string.Empty;

    [JsonIgnore]
    [Column(TypeName = "CLOB")]
    // ReSharper disable once InconsistentNaming
    public string? BieuMauJSON { get; set; } = string.Empty;

    [MaxLength(100)] public string? CreatedBy { get; set; }

    [MaxLength(100)] public string? UpdatedBy { get; set; }

    public void AssignDongDuLieu(List<Dictionary<string, object?>> dongDuLieus)
    {
        DongDuLieus = dongDuLieus;
        DongDuLieusJSON = JsonConvert.SerializeObject(dongDuLieus);
        UpdatedAt = DateTime.UtcNow;
    }

    public void AssignBieuMau(BieuMau bieuMau)
    {
        BieuMau = bieuMau;
        BieuMauJSON = JsonConvert.SerializeObject(bieuMau);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deserialize()
    {
        if (DongDuLieusJSON.Length > 0)
            DongDuLieus = JsonConvert.DeserializeObject<List<Dictionary<string, object?>>>(DongDuLieusJSON) ?? [];
        if (BieuMauJSON is { Length: > 0 })
            BieuMau = JsonConvert.DeserializeObject<BieuMau>(BieuMauJSON);
    }
}

[Index(nameof(MaDonVi), IsUnique = true)]
[Index(nameof(CreatedBy))]
[Index(nameof(UpdatedBy))]
public class DonVi
{
    [Key] public int Id { get; set; }

    [MaxLength(255)] public string TenDonVi { get; set; } = string.Empty;

    [MaxLength(100)] public string? MaDonVi { get; set; }

    [MaxLength(1000)] public string? MoTa { get; set; }
    [MaxLength(100)] public string? CreatedBy { get; set; }
    [MaxLength(100)] public string? UpdatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public void Assign(DonVi donVi)
    {
        TenDonVi = donVi.TenDonVi;
        MaDonVi = donVi?.TenDonVi?.Trim().ToLower();
        MoTa = donVi?.MoTa;
        UpdatedAt = DateTime.UtcNow;
    }
}

[Index(nameof(CreatedBy))]
[Index(nameof(UpdatedBy))]
[Index(nameof(ChiTieuId))]
public class ChiTieuDaNhap : BasicInfo
{
    [Key] public int Id { get; set; }

    [MaxLength(1000)] public string? Nguon { get; set; }

    [MaxLength(1000)] public string? Ten { get; set; }

    public int? ChiTieuId { get; set; }

    [JsonIgnore]
    [Column(TypeName = "CLOB")]
    public string DongDuLieuJSON { get; set; } = string.Empty;

    [NotMapped] public List<DuLieuChiTieu> DongDuLieus { get; set; } = [];

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(100)] public string? UpdatedBy { get; set; }

    public void AssignDongDuLieu(List<DuLieuChiTieu> dongDuLieus)
    {
        DongDuLieus = dongDuLieus;
        DongDuLieuJSON = JsonSerializer.Serialize(dongDuLieus);
    }

    public void Deserialize()
    {
        if (DongDuLieuJSON is { Length: > 0 })
            DongDuLieus = JsonSerializer.Deserialize<List<DuLieuChiTieu>>(DongDuLieuJSON) ?? [];
    }
}