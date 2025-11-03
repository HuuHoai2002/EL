using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using ThongKe.Shared.Utils;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace ThongKe.Entities;

[Index(nameof(MaKieu))]
[Index(nameof(NhomDanhMuc))]
public class KieuDanhMuc : BasicInfo
{
	[Key] public long Id { get; set; }
	public string Ten { get; set; }
	public string? MaKieu { get; set; } //unique trong 1 cây
	public string? MoTa { get; set; }
	public string? Format { get; set; }
	public string? NhomDanhMuc { get; set; }
	[NotMapped] public List<DanhMuc>? DanhMucs { get; set; } // for response only

	[JsonIgnore]
	[Column(TypeName = "NCLOB")]
	public string? DanhMucsJson { get; set; } // for response only

	[JsonIgnore]
	[Column(TypeName = "NCLOB")]
	public string? TruongDuLieusJSON { get; set; } // just for backend, not for frontend
	[NotMapped] public List<TruongDuLieu>? TruongDuLieus { get; set; }

	public int IsOpen { get; set; } = 1; // đóng/mở danh mục
	public int IsClosed { get; set; } = 0; // đóng/mở danh mục
	[NotMapped] public int? TotalDanhMuc { get; set; }
	public long Created { get; set; } = Utils.DateTimeToUnixTimeStamp(DateTime.Now); // backend automatically set
	public long Updated { get; set; }
	public string? UpdatedBy { get; set; }
	[JsonIgnore]
	[Column(TypeName = "NCLOB")]
	public string? ChildrenKieuDanhMucsJson { get; set; } // for response only
	[NotMapped] public List<KieuDanhMuc>? ChildrenKieuDanhMucs { get; set; } // for response only

	//public void Assign(KieuDanhMuc candidate)
	//{
	//	Ten = candidate.Ten;
	//	MoTa = candidate.MoTa;
	//	Format = candidate.Format;
	//	NhomDanhMuc = candidate.NhomDanhMuc;
	//	MoTa = candidate.MoTa;
	//	TruongDuLieus = candidate.TruongDuLieus;
	//	UpdatedBy = candidate.UpdatedBy;
	//	Scope = candidate.Scope;
	//	TinhThanhId = candidate.TinhThanhId;
	//	DonViThongKeId = candidate.DonViThongKeId;
	//}
	public void Assign(KieuDanhMuc candidate)
	{
		Ten = candidate.Ten;
		MaKieu = candidate.MaKieu;
		MoTa = candidate.MoTa;
		Format = candidate.Format;
		NhomDanhMuc = candidate.NhomDanhMuc;
		TruongDuLieus = candidate.TruongDuLieus;
		ChildrenKieuDanhMucs = candidate.ChildrenKieuDanhMucs;
		DanhMucs = candidate.DanhMucs;
		UpdatedBy = candidate.UpdatedBy;
		Scope = candidate.Scope;
		TinhThanhId = candidate.TinhThanhId;
		DonViThongKeId = candidate.DonViThongKeId;
	}


	public void Deserialize()
	{
		if (TruongDuLieusJSON != null && TruongDuLieusJSON.Length > 0)
			TruongDuLieus = JsonSerializer.Deserialize<List<TruongDuLieu>>(TruongDuLieusJSON);
		if (ChildrenKieuDanhMucsJson != null && ChildrenKieuDanhMucsJson.Length > 0)
			ChildrenKieuDanhMucs = JsonSerializer.Deserialize<List<KieuDanhMuc>>(ChildrenKieuDanhMucsJson);
		if (DanhMucsJson != null && DanhMucsJson.Length > 0)
			DanhMucs = JsonSerializer.Deserialize<List<DanhMuc>>(DanhMucsJson);
	}


	public void Serialize()
	{
		TruongDuLieusJSON = TruongDuLieus != null && TruongDuLieus.Any()
				? JsonSerializer.Serialize(TruongDuLieus)
				: null;

		ChildrenKieuDanhMucsJson = ChildrenKieuDanhMucs != null && ChildrenKieuDanhMucs.Any()
				? JsonSerializer.Serialize(ChildrenKieuDanhMucs)
				: null;

		DanhMucsJson = DanhMucs != null && DanhMucs.Any()
				? JsonSerializer.Serialize(DanhMucs)
				: null;
	}
}

public class TruongDuLieu
{
	public string? Ma { get; set; }
	public string? Ten { get; set; }
	public string? KieuDuLieu { get; set; }
	public string? MoTa { get; set; }
}

[Index(nameof(MaMuc))]
[Index(nameof(TenMuc))]
public class DanhMuc : BasicInfo
{
	[Key] public long Id { get; set; }
	public required string TenMuc { get; set; }
	public required string MaMuc { get; set; }
	[NotMapped] public KieuDanhMuc? KieuDanhMuc { get; set; } // for response only
	public string? KyHieu { get; set; }
	public string? MoTa { get; set; }
	public int? Stt { get; set; }
	public string? Nguon { get; set; }
	[NotMapped] public bool IsError { get; set; } = false; // trả ra chi tiết danh mục nào lỗi được lưu chung vào Json DanhMucs của DanhMucImport

	[NotMapped] public List<KV>? DanhSachs { get; set; }

	[JsonIgnore]
	[Column(TypeName = "NCLOB")]
	public string? DanhSachsJSON { get; set; } // just for backend, not for frontend

	//public string? TrangThai { get; set; }
	[NotMapped] public bool? IsMoRong { get; set; } // use for publish add extended data
	public long Created { get; set; } = Utils.DateTimeToUnixTimeStamp(DateTime.Now); // backend automatically set
	public long Updated { get; set; } // backend automatically set
	public string? UpdatedBy { get; set; }

	public void Assign(DanhMuc candidate)
	{
		TenMuc = candidate.TenMuc;
		MaMuc = candidate.MaMuc;
		KyHieu = candidate.KyHieu;
		MoTa = candidate.MoTa;
		Stt = candidate.Stt;
		Nguon = candidate.Nguon;
		DanhSachs = candidate.DanhSachs;
		UpdatedBy = candidate.UpdatedBy;
	}

	public void Deserialize()
	{
		if (DanhSachsJSON != null && DanhSachsJSON.Length > 0)
			DanhSachs = JsonSerializer.Deserialize<List<KV>>(DanhSachsJSON);
	}

	public void Serialize()
	{
		DanhSachsJSON = JsonSerializer.Serialize(DanhSachs);
	}
}

public class KV
{
	public string? Key { get; set; }
	public string? Value { get; set; }
}

public class DanhMucImport : BasicInfo
{
	[Key] public long Id { get; set; }
	public string? TenPhienBan { get; set; } //name
	public string? MaKieuGoc { get; set; } // mã kiểu gốc 
	[NotMapped] public string? TenKieuGoc { get; set; } // cho BE trả về
	[NotMapped] public string? TenKieuImport { get; set; } // cho BE trả về
	public string? MaKieu { get; set; } // mã kiểu hiện tại để import data vào
																			//public string? RowsType { get; set; } //optional, "tree" or "flat"
	[NotMapped] public List<DanhMuc>? DanhMucs { get; set; } // nhận vào data cần import vào MaKieu hiện tại

	[JsonIgnore]
	[Column(TypeName = "NCLOB")]
	public string? DanhMucsJSON { get; set; } // just for backend, not for frontend
	public int? SoBanGhiThemMoi { get; set; } // just for response
	public int? SoBanGhiCapNhat { get; set; } // just for response
	public int? SoBanGhiLoi { get; set; } // just for response
																				//public string? ParentMaKieu { get; set; }
																				//public string? ParentMaMuc { get; set; }
	public string? Nguon { get; set; }
	//public string? TrangThai { get; set; }
	public long Created { get; set; }  = Utils.DateTimeToUnixTimeStamp(DateTime.Now);// backend automatically set
	public long Updated { get; set; } // backend automatically set
																		//public string? CreatedBy { get; set; }
	public string? UpdatedBy { get; set; }

	public void Assign(DanhMucImport candidate)
	{
		TenPhienBan = candidate.TenPhienBan;
		MaKieuGoc = candidate.MaKieuGoc;
		MaKieu = candidate.MaKieu;
		//MaKieuDanhMucs = candidate.MaKieuDanhMucs;
		//RowsType = candidate.RowsType;
		DanhMucs = candidate.DanhMucs;
		//ParentMaKieu = candidate.ParentMaKieu;
		//ParentMaMuc = candidate.ParentMaMuc;
		Nguon = candidate.Nguon;
		//TrangThai = candidate.TrangThai;
		UpdatedBy = candidate.UpdatedBy;

	}

	public void Deserialize()
	{
		if (DanhMucsJSON != null && DanhMucsJSON.Length > 0)
			DanhMucs = JsonSerializer.Deserialize<List<DanhMuc>>(DanhMucsJSON);
	}

	public void Serialize()
	{
		DanhMucsJSON = JsonSerializer.Serialize(DanhMucs);
	}
}

public class DanhMucPublish
{
	[Key] public long Id { get; set; }
	public string? TenPhienBan { get; set; } //name
	public List<string>? MaKieuDanhMucGocs { get; set; }
	[NotMapped] public string? TenKieuDanhMucGoc { get; set; } // for response only
	[NotMapped] public List<KieuDanhMuc>? DanhMucs { get; set; }

	[JsonIgnore]
	[Column(TypeName = "NCLOB")]
	public string? DanhMucsJson { get; set; } // for response only
	public long? ThoiGianHetHan { get; set; }
	public long Created { get; set; } = Utils.DateTimeToUnixTimeStamp(DateTime.Now); // backend automatically set
	public long Updated { get; set; } // backend automatically set
	public string? CreatedBy { get; set; }
	public string? UpdatedBy { get; set; }
	public int IsDraft { get; set; } = 1;
	public int IsPublish { get; set; } = 0;
	public long Published { get; set; } // thời gian-+ xuất bản 

	public void Assign(DanhMucPublish candidate)
	{
		TenPhienBan = candidate.TenPhienBan;
		ThoiGianHetHan = candidate.ThoiGianHetHan;
		UpdatedBy = candidate.UpdatedBy;
	}
	
	public void Deserialize()
	{
		if(DanhMucsJson != null && DanhMucsJson.Length > 0)
			DanhMucs = JsonSerializer.Deserialize<List<KieuDanhMuc>>(DanhMucsJson);
	}

	public void Serialize()
	{
		DanhMucsJson = JsonSerializer.Serialize(DanhMucs);
	}
}

public static class DanhMucUtils
{
	public static KieuDanhMuc? FindNodeByMaKieu(KieuDanhMuc rootNode, string maKieu)
	{
		if (rootNode == null || string.IsNullOrEmpty(maKieu))
			return null;

		// Check if current node matches
		if (rootNode.MaKieu == maKieu)
		{
			return rootNode;
		}

		// Recursively search in children
		if (rootNode.ChildrenKieuDanhMucs != null && rootNode.ChildrenKieuDanhMucs.Count > 0)
		{
			foreach (var child in rootNode.ChildrenKieuDanhMucs)
			{
				var result = FindNodeByMaKieu(child, maKieu);
				if (result != null)
					return result;
			}
		}
		return null;
	}
}