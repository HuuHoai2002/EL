using ThongKe.DTOs;
using ThongKe.Entities;
using static ThongKe.Controllers.DM.AuthorController;

namespace ThongKe.Shared.Utils
{
	public static class StaticData
	{
		public static readonly List<KV> Scopes = new()
				{
						new() { Key = "tw", Value = "Trung ương" },
						new() { Key = "tinh", Value = "Tỉnh" },
						new() { Key = "cuc", Value = "Cục" },
						new() { Key = "cn", Value = "Chuyên ngành" }
				};

		public static readonly List<KV> Roles = new()
				{
						new() { Key = "admin", Value = "Admin" },
						new() { Key = "giamsat", Value = "Giám sát" },
						new() { Key = "user", Value = "User" }
				};

		public static readonly List<DonViThongKe> DonViThongKes = new()
				{
						new() { TenDonVi = "Vụ Hợp tác quốc tế", MaDonVi = "vuhoptacqt", Scope = "tw", CreatedAt = DateTime.MinValue, CreatedBy = "system", UpdatedAt = DateTime.MinValue, UpdatedBy = "system"},
						new() { TenDonVi = "Vụ Kế hoạch - Tài chính", MaDonVi = "vukehoachtaichinh", Scope = "tw", CreatedAt = DateTime.MinValue, CreatedBy = "system", UpdatedAt = DateTime.MinValue, UpdatedBy = "system"},
						new() { TenDonVi = "Vụ Khoa học và Công nghệ", MaDonVi = "vukhoahoccnghe", Scope = "tw", CreatedAt = DateTime.MinValue, CreatedBy = "system", UpdatedAt = DateTime.MinValue, UpdatedBy = "system"},
						new() { TenDonVi = "Vụ Pháp chế", MaDonVi = "vuphapche", Scope = "tw", CreatedAt = DateTime.MinValue, CreatedBy = "system", UpdatedAt = DateTime.MinValue, UpdatedBy = "system"},
						new() { TenDonVi = "Vụ Tổ chức cán bộ", MaDonVi = "vutochuc", Scope = "tw", CreatedAt = DateTime.MinValue, CreatedBy = "system", UpdatedAt = DateTime.MinValue, UpdatedBy = "system"},
						new() { TenDonVi = "Văn phòng bộ", MaDonVi = "vanphongbo", Scope = "tw", CreatedAt = DateTime.MinValue, CreatedBy = "system", UpdatedAt = DateTime.MinValue, UpdatedBy = "system"},
						new() { TenDonVi = "Thanh tra bộ", MaDonVi = "thanhtrabo", Scope = "tw", CreatedAt = DateTime.MinValue, CreatedBy = "system", UpdatedAt = DateTime.MinValue, UpdatedBy = "system"},
						new() { TenDonVi = "Cục Chuyển đổi số", MaDonVi = "cucchuyendoiso", Scope = "cuc", CreatedAt = DateTime.MinValue, CreatedBy = "system", UpdatedAt = DateTime.MinValue, UpdatedBy = "system"},
						new() { TenDonVi = "Cục Trồng trọt và Bảo vệ thực vật", MaDonVi = "cuctrongtrotbvthucvat", Scope = "cuc", CreatedAt = DateTime.MinValue, CreatedBy = "system", UpdatedAt = DateTime.MinValue, UpdatedBy = "system"},
						new() { TenDonVi = "Cục Chăn nuôi và Thú y", MaDonVi = "cucchannuoi", Scope = "cuc", CreatedAt = DateTime.MinValue, CreatedBy = "system", UpdatedAt = DateTime.MinValue, UpdatedBy = "system"},
						new() { TenDonVi = "Cục Thủy sản và Kiểm ngư", MaDonVi = "cucthuysan", Scope = "cuc", CreatedAt = DateTime.MinValue, CreatedBy = "system", UpdatedAt = DateTime.MinValue, UpdatedBy = "system"},
						new() { TenDonVi = "Cục Lâm nghiệp và Kiểm lâm", MaDonVi = "cuclamnghiep", Scope = "cuc", CreatedAt = DateTime.MinValue, CreatedBy = "system", UpdatedAt = DateTime.MinValue, UpdatedBy = "system"},
						new() { TenDonVi = "Cục Quản lý và Xây dựng công trình thủy lợi", MaDonVi = "cucqlcttl", Scope = "cuc", CreatedAt = DateTime.MinValue, CreatedBy = "system", UpdatedAt = DateTime.MinValue, UpdatedBy = "system"},
						new() { TenDonVi = "Cục Quản lý đê điều và Phòng, chống thiên tai", MaDonVi = "cucpctl", Scope = "cuc", CreatedAt = DateTime.MinValue, CreatedBy = "system", UpdatedAt = DateTime.MinValue, UpdatedBy = "system"},
						new() { TenDonVi = "Cục Kinh tế hợp tác và Phát triển nông thôn", MaDonVi = "cuckthoptac", Scope = "cuc", CreatedAt = DateTime.MinValue, CreatedBy = "system", UpdatedAt = DateTime.MinValue, UpdatedBy = "system"},
						new() { TenDonVi = "Cục Chất lượng, Chế biến và Phát triển thị trường", MaDonVi = "cucchebien", Scope = "cuc", CreatedAt = DateTime.MinValue, CreatedBy = "system", UpdatedAt = DateTime.MinValue, UpdatedBy = "system"},
						new() { TenDonVi = "Cục Quản lý đất đai", MaDonVi = "cucqldatdai", Scope = "cuc", CreatedAt = DateTime.MinValue, CreatedBy = "system", UpdatedAt = DateTime.MinValue, UpdatedBy = "system"},
						new() { TenDonVi = "Cục Quản lý tài nguyên nước", MaDonVi = "cucqltainuoc", Scope = "cuc", CreatedAt = DateTime.MinValue, CreatedBy = "system", UpdatedAt = DateTime.MinValue, UpdatedBy = "system"},
						new() { TenDonVi = "Cục Địa chất và Khoáng sản Việt Nam", MaDonVi = "cucdcksvn", Scope = "cuc", CreatedAt = DateTime.MinValue, CreatedBy = "system", UpdatedAt = DateTime.MinValue, UpdatedBy = "system"},
						new() { TenDonVi = "Cục Môi trường", MaDonVi = "cucmoitruong", Scope = "cuc", CreatedAt = DateTime.MinValue, CreatedBy = "system", UpdatedAt = DateTime.MinValue, UpdatedBy = "system"},
						new() { TenDonVi = "Cục Biến đổi khí hậu", MaDonVi = "cucbdkh", Scope = "cuc", CreatedAt = DateTime.MinValue, CreatedBy = "system", UpdatedAt = DateTime.MinValue, UpdatedBy = "system"},
						new() { TenDonVi = "Cục Bảo tồn thiên nhiên và Đa dạng sinh học", MaDonVi = "cucbaoton", Scope = "cuc", CreatedAt = DateTime.MinValue, CreatedBy = "system", UpdatedAt = DateTime.MinValue, UpdatedBy = "system"},
						new() { TenDonVi = "Cục Khí tượng Thủy văn", MaDonVi = "cuckttv", Scope = "cuc", CreatedAt = DateTime.MinValue, CreatedBy = "system", UpdatedAt = DateTime.MinValue, UpdatedBy = "system"},
						new() { TenDonVi = "Cục Biển và Hải đảo Việt Nam", MaDonVi = "cucbienhaidao", Scope = "cuc", CreatedAt = DateTime.MinValue, CreatedBy = "system", UpdatedAt = DateTime.MinValue, UpdatedBy = "system"},
						new() { TenDonVi = "Cục Đo đạc, Bản đồ và Thông tin địa lý Việt Nam", MaDonVi = "cucdodat", Scope = "cuc", CreatedAt = DateTime.MinValue, CreatedBy = "system", UpdatedAt = DateTime.MinValue, UpdatedBy = "system"},
						new() { TenDonVi = "Cục Viễn thám quốc gia", MaDonVi = "cucvientham", Scope = "cuc", CreatedAt = DateTime.MinValue, CreatedBy = "system", UpdatedAt = DateTime.MinValue, UpdatedBy = "system"},
						new() { TenDonVi = "Viện Chiến lược, Chính sách nông nghiệp và môi trường", MaDonVi = "vienchienluoc", Scope = "cn", CreatedAt = DateTime.MinValue, CreatedBy = "system", UpdatedAt = DateTime.MinValue, UpdatedBy = "system"},
						new() { TenDonVi = "Viện Khoa học môi trường", MaDonVi = "vienkhoahocmt", Scope = "cn", CreatedAt = DateTime.MinValue, CreatedBy = "system", UpdatedAt = DateTime.MinValue, UpdatedBy = "system"},
						new() { TenDonVi = "Tạp chí Nông nghiệp và Môi trường", MaDonVi = "tapchinongnghiep", Scope = "cn", CreatedAt = DateTime.MinValue, CreatedBy = "system", UpdatedAt = DateTime.MinValue, UpdatedBy = "system"},
						new() { TenDonVi = "Trung tâm Khuyến nông quốc gia", MaDonVi = "trungtamkhuyennong", Scope = "cn", CreatedAt = DateTime.MinValue, CreatedBy = "system", UpdatedAt = DateTime.MinValue, UpdatedBy = "system"},
		};
		public static readonly List<KV> TinhThanhs = new()
				{
						new() { Key = "HN", Value = "Hà Nội" },
						new() { Key = "HCM", Value = "TP. Hồ Chí Minh" },
						new() { Key = "HP", Value = "Hải Phòng" },
						new() { Key = "DN", Value = "Đà Nẵng" },
						new() { Key = "BD", Value = "Bình Dương" },
						new() { Key = "LA", Value = "Long An" },
						new() { Key = "CT", Value = "Cần Thơ" },
						new() { Key = "QN", Value = "Quảng Ninh" },
						new() { Key = "TH", Value = "Thanh Hóa" },
						new() { Key = "NA", Value = "Nghệ An" },
						new() { Key = "TT", Value = "Thừa Thiên Huế" },
						new() { Key = "KG", Value = "Kiên Giang" },
						new() { Key = "BDI", Value = "Bình Định" },
						new() { Key = "DL", Value = "Đắk Lắk" },
						new() { Key = "ST", Value = "Sóc Trăng" },
						new() { Key = "AG", Value = "An Giang" },
						new() { Key = "TB", Value = "Thái Bình" },
						new() { Key = "BG", Value = "Bắc Giang" },
						new() { Key = "BN", Value = "Bắc Ninh" },
						new() { Key = "ND", Value = "Nam Định" }
				};

		// Các hàm tiện ích check hợp lệ:
		public static bool IsValidScope(string scope) =>
				Scopes.Any(x => x.Key == scope);

		public static bool IsValidRole(string role) =>
				Roles.Any(x => x.Key == role);


		public static bool IsValidTinhThanh(string id) =>
				TinhThanhs.Any(x => x.Key == id);

		public static bool IsValidDonVi(string id) =>
			DonViThongKes.Any(x => x.MaDonVi == id);
	}
}
