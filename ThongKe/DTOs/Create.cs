using System.ComponentModel.DataAnnotations;
using ThongKe.Entities;

namespace ThongKe.DTOs;

public class DuLieuChiTieuDto
{
    public int? Id { get; set; } = null; // for update data
    public int? PhanToId1 { get; set; } = null;
    public int? PhanToId2 { get; set; } = null;
    public int? PhanToId3 { get; set; } = null;
    public int? PhanToId4 { get; set; } = null;
    public int? PhanToId5 { get; set; } = null;
    public int? PhanToId6 { get; set; } = null;
    public int? PhanToId7 { get; set; } = null;
    public int? PhanToId8 { get; set; } = null;
    public int? PhanToId9 { get; set; } = null;
    public int? PhanToId10 { get; set; } = null;
    public double? Data { get; set; }
}

public class ImportDataDto
{
    public int ChiTieuId { get; set; }
    public string? Nguon { get; set; }
    public string? Ten { get; set; }
    public List<DuLieuChiTieuDto> Records { get; set; } = [];
}

public class AddVariantDto
{
    [Required] public int ChiTieuId { get; set; }
    [Required] public ChiTieu ChiTieuVariant { get; set; }
}

public class UpsertRecordDto
{
    [Required] public int KieuBangThongKeId { get; set; }
    public int? RecordId { get; set; } = null; // for update
    public Dictionary<string, object> Record { get; set; } = new();
    public bool ExactMatch { get; set; } = true;
}

public class UpsertManyRecordDto
{
    [Required] public int KieuBangThongKeId { get; set; }

    public List<Dictionary<string, object?>> Records { get; set; } = [];
}

public class UpsertManyFormRecordDto
{
    public int? Id { get; set; } = null;
    [Required] public int BieuMauId { get; set; }

    public string? NguonDuLieu { get; set; }

    public List<Dictionary<string, object?>> Records { get; set; } = [];
}