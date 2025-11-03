using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using ThongKe.Data;
using ThongKe.DTOs;
using ThongKe.Entities;
using ThongKe.Shared.Enums;
using static ThongKe.Entities.DonViThongKe;
using static ThongKe.Shared.Utils.Utils.PermissionUtil;


namespace ThongKe.Shared.Utils;

public class Utils
{

	public static Dictionary<char, string> telexDictionary = new()
		{
				{ 'á', "as" }, { 'à', "af" }, { 'ả', "ar" }, { 'ã', "ax" }, { 'ạ', "aj" },
				{ 'â', "aa" }, { 'ấ', "aas" }, { 'ầ', "aaf" }, { 'ẩ', "aar" }, { 'ẫ', "aax" }, { 'ậ', "aaj" },
				{ 'ă', "aw" }, { 'ắ', "aws" }, { 'ằ', "awf" }, { 'ẳ', "awr" }, { 'ẵ', "awx" }, { 'ặ', "awj" },
				{ 'Á', "As" }, { 'À', "Af" }, { 'Ả', "Ar" }, { 'Ã', "Ax" }, { 'Ạ', "Aj" },
				{ 'Â', "Aa" }, { 'Ấ', "Aas" }, { 'Ầ', "Aaf" }, { 'Ẩ', "Aar" }, { 'Ẫ', "Aax" }, { 'Ậ', "Aaj" },
				{ 'Ă', "Aw" }, { 'Ắ', "Aws" }, { 'Ằ', "Awf" }, { 'Ẳ', "Awr" }, { 'Ẵ', "Awx" }, { 'Ặ', "Awj" },
				{ 'é', "es" }, { 'è', "ef" }, { 'ẻ', "er" }, { 'ẽ', "ex" }, { 'ẹ', "ej" },
				{ 'ê', "ee" }, { 'ế', "ees" }, { 'ề', "eef" }, { 'ể', "eer" }, { 'ễ', "eex" }, { 'ệ', "eej" },
				{ 'É', "Es" }, { 'È', "Ef" }, { 'Ẻ', "Er" }, { 'Ẽ', "Ex" }, { 'Ẹ', "Ej" },
				{ 'Ê', "Ee" }, { 'Ế', "Ees" }, { 'Ề', "Eef" }, { 'Ể', "Eer" }, { 'Ễ', "Eex" }, { 'Ệ', "Eej" },
				{ 'í', "is" }, { 'ì', "if" }, { 'ỉ', "ir" }, { 'ĩ', "ix" }, { 'ị', "ij" },
				{ 'Í', "Is" }, { 'Ì', "If" }, { 'Ỉ', "Ir" }, { 'Ĩ', "Ix" }, { 'Ị', "Ij" },
				{ 'ó', "os" }, { 'ò', "of" }, { 'ỏ', "or" }, { 'õ', "ox" }, { 'ọ', "oj" },
				{ 'ô', "oo" }, { 'ố', "oos" }, { 'ồ', "oof" }, { 'ổ', "oor" }, { 'ỗ', "oox" }, { 'ộ', "ooj" },
				{ 'ơ', "ow" }, { 'ớ', "ows" }, { 'ờ', "owf" }, { 'ở', "owr" }, { 'ỡ', "owx" }, { 'ợ', "owj" },
				{ 'Ó', "Os" }, { 'Ò', "Of" }, { 'Ỏ', "Or" }, { 'Õ', "Ox" }, { 'Ọ', "Oj" },
				{ 'Ô', "Oo" }, { 'Ố', "Oos" }, { 'Ồ', "Oof" }, { 'Ổ', "Oor" }, { 'Ỗ', "Oox" }, { 'Ộ', "Ooj" },
				{ 'Ơ', "Ow" }, { 'Ớ', "Ows" }, { 'Ờ', "Owf" }, { 'Ở', "Owr" }, { 'Ỡ', "Owx" }, { 'Ợ', "Owj" },
				{ 'ú', "us" }, { 'ù', "uf" }, { 'ủ', "ur" }, { 'ũ', "ux" }, { 'ụ', "uj" },
				{ 'ư', "uw" }, { 'ứ', "uws" }, { 'ừ', "uwf" }, { 'ử', "uwr" }, { 'ữ', "uwx" }, { 'ự', "uwj" },
				{ 'Ú', "Us" }, { 'Ù', "Uf" }, { 'Ủ', "Ur" }, { 'Ũ', "Ux" }, { 'Ụ', "Uj" },
				{ 'Ư', "Uw" }, { 'Ứ', "Uws" }, { 'Ừ', "Uwf" }, { 'Ử', "Uwr" }, { 'Ữ', "Uwx" }, { 'Ự', "Uwj" },
				{ 'ý', "ys" }, { 'ỳ', "yf" }, { 'ỷ', "yr" }, { 'ỹ', "yx" }, { 'ỵ', "yj" },
				{ 'Ý', "Ys" }, { 'Ỳ', "Yf" }, { 'Ỷ', "Yr" }, { 'Ỹ', "Yx" }, { 'Ỵ', "Yj" },
				{ 'đ', "dd" }, { 'Đ', "Dd" }
		};

	public static string StringVNToKey(string name)
	{
		name = ConvertToASCII(name);
		name = name.Trim().ToLower(CultureInfo.CurrentCulture);
		return new string(name.Where((c, i) => c is '-' or '.' or >= '0' and <= '9' or >= 'a' and <= 'z').ToArray());
	}

	public static string ConvertToASCII(string input)
	{
		var result = new StringBuilder();

		foreach (var c in input)
			if (telexDictionary.ContainsKey(c))
				result.Append(telexDictionary[c]);
			else
				result.Append(c);

		return result.ToString();
	}

	public static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
	{
		// Unix timestamp is seconds past epoch
		var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
		dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
		return dtDateTime;
	}

	public static DateTime UnixTimeStampToDateTimeVN(long unixTimeStamp)
	{
		var d = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(unixTimeStamp);
		var tz = TimeZoneInfo.FindSystemTimeZoneById("Asia/Saigon");
		d = TimeZoneInfo.ConvertTimeFromUtc(d, tz);
		return d;
	}

	public static long DateTimeToUnixTimeStamp(DateTime dtDateTime)
	{
		return new DateTimeOffset(dtDateTime).ToUnixTimeSeconds();
	}

	public static long DateTimeToUnixTimeMilliseconds(DateTime dtDateTime)
	{
		return new DateTimeOffset(dtDateTime).ToUnixTimeMilliseconds();
	}

	public static int GetYearFromTimestamp(long timestamp)
	{
		var dateTime = DateTimeOffset.FromUnixTimeSeconds(timestamp).LocalDateTime;
		var year = dateTime.Year;

		return year;
	}

	public static string RundomNumber(int length)
	{
		var random = new Random();
		const string chars = "0123456789";
		return new string(Enumerable.Repeat(chars, length)
				.Select(s => s[random.Next(s.Length)]).ToArray());
	}

	public static string RundomChar(int length)
	{
		var random = new Random();
		const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
		return new string(Enumerable.Repeat(chars, length)
				.Select(s => s[random.Next(s.Length)]).ToArray());
	}

	public static string StripUnicodeCharactersFromString(string text)
	{
		if (text != null)

		{
			var arr1 = new[]
			{
								"á", "à", "ả", "ã", "ạ", "â", "ấ", "ầ", "ẩ", "ẫ", "ậ", "ă", "ắ", "ằ", "ẳ", "ẵ", "ặ",
								"đ",
								"é", "è", "ẻ", "ẽ", "ẹ", "ê", "ế", "ề", "ể", "ễ", "ệ",
								"í", "ì", "ỉ", "ĩ", "ị",
								"ó", "ò", "ỏ", "õ", "ọ", "ô", "ố", "ồ", "ổ", "ỗ", "ộ", "ơ", "ớ", "ờ", "ở", "ỡ", "ợ",
								"ú", "ù", "ủ", "ũ", "ụ", "ư", "ứ", "ừ", "ử", "ữ", "ự",
								"ý", "ỳ", "ỷ", "ỹ", "ỵ"
						};
			var arr2 = new[]
			{
								"a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a",
								"d",
								"e", "e", "e", "e", "e", "e", "e", "e", "e", "e", "e",
								"i", "i", "i", "i", "i",
								"o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o",
								"u", "u", "u", "u", "u", "u", "u", "u", "u", "u", "u",
								"y", "y", "y", "y", "y"
						};

			for (var i = 0; i < arr1.Length; i++)
			{
				text = text.Replace(arr1[i], arr2[i]);
				text = text.Replace(arr1[i].ToUpper(), arr2[i].ToUpper());
			}
		}
		else
		{
			text = null;
		}

		return text;
	}
	public static string NormalizeName(string roleName)
	{
		if (string.IsNullOrWhiteSpace(roleName))
			return string.Empty;

		// 1️⃣ Chuẩn hóa tiếng Việt: loại bỏ dấu
		string normalized = StripUnicodeCharactersFromString(roleName);

		// 2️⃣ Xóa khoảng trắng, ký tự đặc biệt
		normalized = Regex.Replace(normalized, @"[^a-zA-Z0-9]", "");

		// 3️⃣ Chuyển sang chữ hoa
		return normalized.ToUpperInvariant();
	}

	public static class PermissionUtil
	{
		public static dynamic CanCreateDanhMuc(IHttpContextAccessor httpContextAccessor, DbContext context)
		{
			var userId = Int64.Parse(httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
			var user = context.Set<User>().Find(userId);

			if (!IsUserActive(user))
			{
				return new { CanCreate = false };
			}

			if (user.RoleId == UserRoles.Admin)
			{
				// admin được tạo theo Scope của mình hoặc cũng có thể là admin trung ương mới được tạo??
				return new { CanCreate = true, Scope = user.Scope, TinhThanhId = user.TinhThanhId, DonViThongKeId = user.DonViThongKeId, CreatedBy = user.UserName };
			}
			return new { CanCreate = false, Message = "Không có quyền" };
		}

		public static bool CanReadAuth<T>(IHttpContextAccessor httpContextAccessor, DbContext context, T entity)
		{
			// lấy thông tin user từ token 
			var userId = Int64.Parse(httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
			var user = context.Set<User>().Find(userId);
			if (user.MemberJson != null)
			{
				user.Deserialize();
				user.Subordinates = context.Set<User>().Where(x => user.Members!.Contains(x.Id)).ToList();
			}

			if (!IsUserActive(user)) return false;

			var scope = GetPropertyValue<string>(entity, "Scope");
			var tinhThanhId = GetPropertyValue<string>(entity, "TinhThanhId");
			var donViThongKeId = GetPropertyValue<string>(entity, "DonViThongKeId");
			var createdBy = GetPropertyValue<string>(entity, "CreatedBy");

			if (scope == null) return false;

			// Admin xem được data cấp dưới
			if (user.RoleId == UserRoles.Admin)
				return IsSameOrLowerScope(user, scope, tinhThanhId, donViThongKeId);

			// User và đơn vị thống kê chỉ xem bản thân mình
			if (user.RoleId == UserRoles.User)
			{
				return userId == GetPropertyValue<long>(entity, "Id");
			}

			if (user.RoleId == UserRoles.GiamSat)
				return CanGiamSatRead(user, createdBy);

			return false;
		}
		public static bool CanReadNghiepVu<T>(IHttpContextAccessor httpContextAccessor, DbContext context, T entity)
		{
			// lấy thông tin user từ token 
			var userId = Int64.Parse(httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
			var user = context.Set<User>().Find(userId);
			if (user.MemberJson != null)
			{
				user.Deserialize();
				user.Subordinates = context.Set<User>().Where(x => user.Members!.Contains(x.Id)).ToList();
			}

			if (!IsUserActive(user)) return false;

			var scope = GetPropertyValue<string>(entity, "Scope");
			var tinhThanhId = GetPropertyValue<string>(entity, "TinhThanhId");
			var donViThongKeId = GetPropertyValue<string>(entity, "DonViThongKeId");
			var createdBy = GetPropertyValue<string>(entity, "CreatedBy");

			if (scope == null) return false;

			// Admin xem được data phạm vi của minh
			if (user.RoleId == UserRoles.Admin)
				return IsCungPhamVi(user, scope, tinhThanhId, donViThongKeId);

			// User chính ở scope chính và user ở đơn vị thống kê chỉ xem được data của mình
			if (user.RoleId == UserRoles.User)
			{
				return user.UserName == createdBy;
			}

			if (user.RoleId == UserRoles.GiamSat)
				return CanGiamSatRead(user, createdBy);

			return false;
		}
		public enum UpdatePermissionType
		{
			Public,    // Cho phép cấp trên chỉnh sửa
			Private    // Chỉ user được sửa chính mình
		}

		public enum GiamSatUserReadUpSert
		{
			Yes,
			No
		}

		// Helper methods với reflection
		private static T? GetPropertyValue<T>(object obj, string propertyName)
		{
			var property = obj.GetType().GetProperty(propertyName);
			if (property == null) return default(T);

			return (T?)property.GetValue(obj);
		}

		// Các method kiểm tra scope (không thay đổi)
		private static bool IsSameScopeHierarchy(User user, string entityScope, string? entityTinhThanhId, string? entityDonViThongKeId)
		{
			if (user.Scope == Scope.Tw)
				return entityScope == Scope.Tw;

			if (user.Scope == Scope.Cuc)
				return entityScope == Scope.Cuc;

			if (user.Scope == Scope.Tinh)
				return (entityScope == Scope.Tinh && entityTinhThanhId == user.TinhThanhId)
					|| (entityScope == Scope.Cn && entityTinhThanhId == user.TinhThanhId);

			if (user.Scope == Scope.Cn)
				return entityScope == Scope.Cn && entityDonViThongKeId == user.DonViThongKeId;

			return false;
		}
		private static bool IsCungPhamVi(User user, string entityScope, string? entityTinhThanhId, string? entityDonViThongKeId)
		{
			if (user.Scope == Scope.Tw)
				return entityScope == Scope.Tw;

			if (user.Scope == Scope.Cuc)
				return entityScope == Scope.Cuc;

			if (user.Scope == Scope.Tinh)
				return (entityScope == Scope.Tinh && entityTinhThanhId == user.TinhThanhId);
			if (user.Scope == Scope.Cn)
				return entityScope == Scope.Cn && entityDonViThongKeId == user.DonViThongKeId;
			return false;
		}
		private static bool IsSameOrLowerScope(User user, string entityScope, string? entityTinhThanhId, string? entityDonViThongKeId)
		{
			if (user.Scope == Scope.Tw)
				return true;

			if (user.Scope == Scope.Cuc)
				return entityScope == Scope.Cuc;

			if (user.Scope == Scope.Tinh)
				return (entityScope == Scope.Tinh && entityTinhThanhId == user.TinhThanhId) ||
							 (entityScope == Scope.Cn && entityTinhThanhId == user.TinhThanhId);

			if (user.Scope == Scope.Cn)
				return entityScope == Scope.Cn && entityDonViThongKeId == user.DonViThongKeId;

			return false;
		}

		// Các method cho Giám sát
		private static bool CanGiamSatRead(User user, string entityCreatedBy)
		{
			if (user.Subordinates == null) return false;
			var subordinateName = user.Subordinates.Select(s => s.UserName).ToList();
			return subordinateName.Contains(entityCreatedBy);
		}

		private static bool CanGiamSatUpdate(User user, string entityCreatedBy)
		{
			if (user.Subordinates == null) return false;
			var subordinateName = user.Subordinates.Select(s => s.UserName).ToList();
			return subordinateName.Contains(entityCreatedBy);
		}

		private static bool CanGiamSatDelete(User user, string entityCreatedBy)
		{
			if (user.Subordinates == null) return false;
			var subordinateName = user.Subordinates.Select(s => s.UserName).ToList();
			return subordinateName.Contains(entityCreatedBy);
		}

		private static bool IsUserActive(User user)
		{
			return user.IsActive == 1 && user.IsLocked != 1;
		}

		// Lấy thông tin đơn vị thống kê từ ID fix cứng
		private static DonViThongKe? GetDonViThongKe(DbContext context, string donViThongKeId)
		{
			var getDb = context.Set<DonViThongKe>().FirstOrDefault(dv => dv.MaDonVi == donViThongKeId);
			if (getDb == null)
			{
				var getStatic = StaticData.DonViThongKes.FirstOrDefault(dv => dv.MaDonVi == donViThongKeId);
				if (getStatic == null)
				{
					return null;
				}
				return getStatic;
			}
			return getDb;
		}

		// Filter data với reflection
		public static IQueryable<User> ApplyDataFilterAuth<T>(IHttpContextAccessor httpContextAccessor, DbContext context, IQueryable<User> query) where T : BasicInfo
		{
			// lấy thông tin user từ token 
			var userId = Int64.Parse(httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
			var user = context.Set<User>().Find(userId);

			user.Deserialize();

			if (!IsUserActive(user)) return query.Where(x => false);
			var resultQuery = query;

			if (user.RoleId == UserRoles.Admin)
			{
				if (Scope.Tw == user.Scope)
				{
					return query.Where(e => e.Scope == Scope.Tw || e.Scope == Scope.Tinh || e.Scope == Scope.Cuc);
				}
				if (Scope.Cuc == user.Scope)
				{
					return query.Where(e => e.Scope == Scope.Cuc);
				}
				if (Scope.Tinh == user.Scope)
				{
					return resultQuery = query.Where(e => e.TinhThanhId == user.TinhThanhId && (e.Scope == Scope.Tinh || e.Scope == Scope.Cn));
				}
				if (Scope.Cn == user.Scope)
				{
					query.Where(e => e.Scope == Scope.Cn);
				}
			}
			// role user, dvtk không được xem chỉ xem chính mình
			//if (user.RoleId == UserRoles.User)
			//{
			//	if (Scope.Tw == user.Scope)
			//	{
			//		return query;
			//	}
			//	if (Scope.Cuc == user.Scope)
			//	{
			//		return query.Where(e => e.Scope == Scope.Cuc && e.DonViThongKeId != null);
			//	}
			//	if (Scope.Tinh == user.Scope)
			//	{
			//		return query.Where(e => e.Scope == Scope.Tinh && e.DonViThongKeId != null);
			//	}
			//	if (Scope.Cn == user.Scope)
			//	{
			//		return query.Where(e => e.Scope == Scope.Cn && e.DonViThongKeId != null);
			//	}
			//}
			// role giám sát, chỉ xem người mình giám sát
			if (user.RoleId == UserRoles.GiamSat)
			{
				var member = new List<User>();
				if (user.Members != null && user.Members.Count > 0)
				{
					member = context.Set<User>().AsQueryable().Where(u => user.Members!.Contains(u.Id)).ToList();
				}
				if (user.GiamSatDonViThongKeIds != null && user.GiamSatDonViThongKeIds.Count > 0)
				{
					member.AddRange(context.Set<User>().AsQueryable().Where(u => user.GiamSatDonViThongKeIds!.Contains(u.DonViThongKeId!)).ToList());
				}
				return member.AsQueryable();
			}
			// trả về rỗng
			return query.Take(0);
		}
		public static dynamic CanCreateAuth<T>(IHttpContextAccessor httpContextAccessor, DbContext context, T entity)
		{
			var userId = Int64.Parse(httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
			var user = context.Set<User>().Find(userId);

			if (!IsUserActive(user))
			{
				return new { CanCreate = false };
			}

			var scope = GetPropertyValue<string>(entity, "Scope");
			var tinhThanhId = GetPropertyValue<string>(entity, "TinhThanhId");
			var donViThongKeId = GetPropertyValue<string>(entity, "DonViThongKeId");
			var createdBy = GetPropertyValue<string>(entity, "CreatedBy");

			if (user.RoleId == UserRoles.Admin)
			{
				// Admin trung ương - trả ra entity với các trường gửi từ FE
				if (user.Scope == Scope.Tw)
				{// admin trung ương chỉ được tạo cho trung ương, tỉnh và cục
					if (scope == Scope.Cn)
					{
						return new { CanCreate = false };
					}
					return new { CanCreate = true, Scope = scope, TinhThanhId = tinhThanhId, DonViThongKeId = donViThongKeId, CreatedBy = user.UserName };
				}

				// Admin tỉnh - gắn TinhThanhId và giới hạn Scope cho phép
				if (user.Scope == Scope.Tinh)
				{
					// nếu scope gửi từ FE không phải Tỉnh và cn thì không cho tạo
					if (scope != Scope.Tinh && scope != Scope.Cn)
					{
						return new { CanCreate = false };
					}

					return new { CanCreate = true, Scope = scope, TinhThanhId = user.TinhThanhId, DonViThongKeId = donViThongKeId, CreatedBy = user.UserName };
				}
				// admin cục chỉ được tạo trong phạm vi cục
				if (user.Scope == Scope.Cuc)
				{
					return new { CanCreate = true, Scope = user.Scope, TinhThanhId = user.TinhThanhId, DonViThongKeId = donViThongKeId, CreatedBy = user.UserName };
				}
				else if (user.Scope == Scope.Cn && scope == Scope.Cn)
				{
					return new { CanCreate = true, Scope = user.Scope, TinhThanhId = user.TinhThanhId, DonViThongKeId = donViThongKeId, CreatedBy = user.UserName };
				}
				else
				{
					return new { CanCreate = false };
				}
			}

			// role user, giám sát không được tạo tài khoản
			if (user.RoleId == UserRoles.GiamSat || user.RoleId == UserRoles.User)
			{
				return new { CanCreate = false, Message = "Không được tạo dữ liệu" };
			}

			return new { CanCreate = false, Message = "Không có quyền" };
		}
		public static bool CanUpdateAuth<T>(IHttpContextAccessor httpContextAccessor, DbContext context, T entity, UpdatePermissionType permissionType = UpdatePermissionType.Public)
		{
			var userId = Int64.Parse(httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

			var user = context.Set<User>()
					.AsNoTracking()
					.FirstOrDefault(u => u.Id == userId);
			if (user == null) return false;

			if (user.MemberJson != null)
			{
				user.Deserialize();
				user.Subordinates = context.Set<User>().Where(x => user.Members!.Contains(x.Id)).ToList();
			}

			if (!IsUserActive(user)) return false;

			var scope = GetPropertyValue<string>(entity, "Scope");
			var tinhThanhId = GetPropertyValue<string>(entity, "TinhThanhId");
			var donViThongKeId = GetPropertyValue<string>(entity, "DonViThongKeId");
			var createdBy = GetPropertyValue<string>(entity, "CreatedBy");
			var idObj = GetPropertyValue<object>(entity, "Id");
			long entityId = Convert.ToInt64(idObj);

			if (scope == null) return false;


			// chỉ user mới được sửa tài khoản của mình chỉ dành cho quản lý tài khoản
			if (permissionType == UpdatePermissionType.Private)
			{
				if (entity is User userEntity)
				{
					return userEntity.Id == userId; // Chỉ được sửa chính mình
				}

				return createdBy == user.UserName || entityId == userId;
			}

			// admin cấp cao hơn được khóa tài khoản cấp thấp hơn
			if (user.RoleId == UserRoles.Admin)
			{
				if (user.Scope == Scope.Tw && (scope == Scope.Tinh || scope == Scope.Cuc))
					return true; // Admin trung ương chỉ được khóa tài khoản có scope = Scope.Tinh hoặc Scope.Cuc
											 // admin cục sửa trong cục
				if (user.Scope == Scope.Cuc && scope == Scope.Cuc)
					return IsSameScopeHierarchy(user, scope, tinhThanhId, donViThongKeId);
				//admin tỉnh sửa trong tỉnh và cấp dưới tỉnh
				if (user.Scope == Scope.Tinh && (scope == Scope.Tinh || scope == Scope.Cn))
					return true;
				//admin chuyên ngành sửa trong chuyên ngành
				if (user.Scope == Scope.Cn && scope == Scope.Cn)
					return IsSameScopeHierarchy(user, scope, tinhThanhId, donViThongKeId);
			}

			// đơn vị thống kê và user không cho khóa, mở khóa tài khoản
			if (user.RoleId == UserRoles.User)
			{
				//var donViThongKe = GetDonViThongKe(context, user.DonViThongKeId);
				//if (donViThongKe == null) return false;

				//return createdBy == user.UserName &&
				//			 scope == donViThongKe.Scope &&
				//			 donViThongKeId == user.DonViThongKeId &&
				//			 IsSameScopeHierarchy(user, scope, tinhThanhId, donViThongKeId);
				return false;
			}
			// giám sát không cho sửa tài khoản
			if (user.RoleId == UserRoles.GiamSat)
				return false;
			//return CanGiamSatUpdate(user, createdBy);

			return false;
		}

		public static IQueryable<T> ApplyDataFilterNghiepVu<T>(IHttpContextAccessor httpContextAccessor, DbContext context, IQueryable<T> query, GiamSatUserReadUpSert giamSatUserReadUpSert = GiamSatUserReadUpSert.Yes) where T : BasicInfo
		{
			var userId = long.Parse(httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
			var user = context.Set<User>().Find(userId);

			if (!IsUserActive(user)) return query.Take(0);

			var resultQuery = query;

			if (user.RoleId == UserRoles.Admin)
			{
				if (Scope.Tw == user.Scope)
					return query.Where(e => e.Scope != null && e.Scope == Scope.Tw);

				if (Scope.Cuc == user.Scope)
					return query.Where(e => e.Scope != null && e.Scope == Scope.Cuc);

				if (Scope.Tinh == user.Scope)
					return query.Where(e => e.Scope != null && e.Scope == Scope.Tinh && e.TinhThanhId == user.TinhThanhId);

				if (Scope.Cn == user.Scope)
					return query.Where(e => e.Scope != null && e.Scope == Scope.Cn && e.TinhThanhId == user.TinhThanhId);
			}

			// Nếu user có role GiamSat
			if (user.RoleId == UserRoles.GiamSat)
			{
				user.Deserialize();
				var subordinates = new List<User>();
				if (user.MemberJson != null)
				{
					subordinates.AddRange(context.Set<User>().Where(x => user.Members!.Contains(x.Id)).ToList());
				}
				if (user.GiamSatDonViThongKeIds != null)
				{
					subordinates.AddRange(context.Set<User>().Where(x => user.GiamSatDonViThongKeIds!.Contains(x.DonViThongKeId)).ToList());
				}

				user.Subordinates = subordinates.Distinct().ToList(); // Loại bỏ trùng lặp nếu có
				var subordinateNames = user.Subordinates?.Select(s => s.UserName).ToList() ?? new List<string>();

				// Thêm chính user vào danh sách để họ có thể xem dữ liệu của mình
				subordinateNames.Add(user.UserName);

				// Nếu là thành viên của đơn vị thống kê, lọc theo DonViThongKeId
				if (user.DonViThongKeId != null)
				{
					return query.Where(e => e.Scope != null
							&& e.DonViThongKeId == user.DonViThongKeId
							&& subordinateNames.Contains(e.CreatedBy));
				}

				// Nếu không phải thành viên đơn vị thống kê, chỉ xem dữ liệu của người giám sát
				return query.Where(e => subordinateNames.Contains(e.CreatedBy));
			}

			// thành viên đơn vị thống kê xem dữ liệu của chính nó
			if (user.DonViThongKeId != null)
			{
				if (Scope.Tw == user.Scope)
					return query.Where(e => e.Scope != null && e.Scope == Scope.Tw && e.DonViThongKeId == user.DonViThongKeId && e.CreatedBy == user.CreatedBy);

				if (Scope.Cuc == user.Scope)
					return query.Where(e => e.Scope != null && e.Scope == Scope.Cuc && e.DonViThongKeId == user.DonViThongKeId && e.CreatedBy == user.CreatedBy);

				if (Scope.Tinh == user.Scope)
					return query.Where(e => e.Scope != null && e.Scope == Scope.Tinh && e.DonViThongKeId == user.DonViThongKeId && e.CreatedBy == user.CreatedBy);

				if (Scope.Cn == user.Scope)
					return query.Where(e => e.Scope != null && e.Scope == Scope.Cn && e.DonViThongKeId == user.DonViThongKeId && e.CreatedBy == user.CreatedBy);
			}
			return query.Take(0);
		}
		public static dynamic CanCreateNghiepVu(IHttpContextAccessor httpContextAccessor, DbContext context, GiamSatUserReadUpSert giamSatUserReadUpSert = GiamSatUserReadUpSert.Yes)
		{
			var userId = Int64.Parse(httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
			var user = context.Set<User>().Find(userId);

			if (!IsUserActive(user))
			{
				return new { CanCreate = false };
			}

			if (user.RoleId == UserRoles.Admin)
			{
				// admin được tạo theo Scope của mình - chưa biết có phải là admin trung ương thì chỉ xem kh CRUD ngoài qltk
				return new { CanCreate = true, Scope = user.Scope, TinhThanhId = user.TinhThanhId, DonViThongKeId = user.DonViThongKeId, CreatedBy = user.UserName };
			}
			// giamsat, user, user đơn vị không được tạo
			if (giamSatUserReadUpSert == GiamSatUserReadUpSert.No && (user.RoleId == UserRoles.GiamSat || user.RoleId == UserRoles.User))
			{
				return new { CanCreate = false, Message = "Không có quyền" };
			}
			// User, User donvi được tạo
			if (user.RoleId == UserRoles.User)
			{
				return new { CanCreate = true, Scope = user.Scope, TinhThanhId = user.TinhThanhId, DonViThongKeId = user.DonViThongKeId, CreateBy = user.UserName };
			}
			// GiamSat được tạo dữ liệu
			if (user.RoleId == UserRoles.GiamSat)
			{
				return new { CanCreate = true, Scope = user.Scope, TinhThanhId = user.TinhThanhId, DonViThongKeId = user.DonViThongKeId, CreatedBy = user.UserName };
			}
			return new { CanCreate = false, Message = "Không có quyền" };
		}
		public static bool CanUpdateNghiepVu<T>(IHttpContextAccessor httpContextAccessor, DbContext context, T entity, GiamSatUserReadUpSert giamSatUserReadUpSert = GiamSatUserReadUpSert.Yes)
		{

			var userId = Int64.Parse(httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

			var user = context.Set<User>()
					.AsNoTracking()
					.FirstOrDefault(u => u.Id == userId);
			if (user == null) return false;

			if (user.MemberJson != null)
			{
				user.Deserialize();
				user.Subordinates = context.Set<User>().Where(x => user.Members!.Contains(x.Id)).ToList();
			}

			if (!IsUserActive(user)) return false;

			var scope = GetPropertyValue<string>(entity, "Scope");
			var tinhThanhId = GetPropertyValue<string>(entity, "TinhThanhId");
			var donViThongKeId = GetPropertyValue<string>(entity, "DonViThongKeId");
			var createdBy = GetPropertyValue<string>(entity, "CreatedBy");
			var idObj = GetPropertyValue<object>(entity, "Id");
			long entityId = Convert.ToInt64(idObj);

			if (scope == null) return false;

			// giamsat, user, user đơn vị không được sửa
			if (giamSatUserReadUpSert == GiamSatUserReadUpSert.No && (user.RoleId == UserRoles.GiamSat || user.RoleId == UserRoles.User))
			{
				return false;
			}

			// admin chỉ được sửa dữ liệu trong scope của mình
			if (user.RoleId == UserRoles.Admin)
				return IsCungPhamVi(user, scope, tinhThanhId, donViThongKeId);

			// đơn vị thống kê , user chỉ được sửa của mình tạo ra
			if (user.RoleId == UserRoles.User)
			{

				return createdBy == user.UserName;

				//var donViThongKe = GetDonViThongKe(context, user.DonViThongKeId);
				//if (donViThongKe == null) return false;

				//return createdBy == user.UserName &&
				//			 scope == donViThongKe.Scope &&
				//			 donViThongKeId == user.DonViThongKeId &&
				//			 IsSameScopeHierarchy(user, scope, tinhThanhId, donViThongKeId);
			}
			// giám sát sửa thông tin của người mình giám sát
			if (user.RoleId == UserRoles.GiamSat)
			{
				user.Deserialize();
				var subordinates = new List<User>();
				if (user.MemberJson != null)
				{
					subordinates.AddRange(context.Set<User>().Where(x => user.Members!.Contains(x.Id)).ToList());
				}
				if (user.GiamSatDonViThongKeIds != null)
				{
					subordinates.AddRange(context.Set<User>().Where(x => user.GiamSatDonViThongKeIds!.Contains(x.DonViThongKeId)).ToList());
				}
				user.Subordinates = subordinates.Distinct().ToList(); // Loại bỏ trùng lặp nếu có

				return CanGiamSatUpdate(user, createdBy);
			}
			return false;
		}
		public static bool CanDeleteNghiepVu<T>(IHttpContextAccessor httpContextAccessor, DbContext context, T entity, GiamSatUserReadUpSert giamSatUserReadUpSert = GiamSatUserReadUpSert.Yes)
		{
			bool isRead = CanReadNghiepVu(httpContextAccessor, context, entity);
			if (!isRead) return false;
			// lấy thông tin user từ token 
			var userId = Int64.Parse(httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
			var user = context.Set<User>().Find(userId);

			if (!IsUserActive(user)) return false;

			var scope = GetPropertyValue<string>(entity, "Scope");
			var tinhThanhId = GetPropertyValue<string>(entity, "TinhThanhId");
			var donViThongKeId = GetPropertyValue<string>(entity, "DonViThongKeId");
			var createdBy = GetPropertyValue<string>(entity, "CreatedBy");

			if (scope == null) return false;

			if (user.RoleId == UserRoles.Admin)
				// bắt buộc cùng phạm vi không có trên xóa cấp dưới
				return IsCungPhamVi(user, scope, tinhThanhId, donViThongKeId);
			//if (user.RoleId == UserRoles.User)
			//	return false;

			// giamsat, user, user đơn vị không được tạo
			if (giamSatUserReadUpSert == GiamSatUserReadUpSert.No && (user.RoleId == UserRoles.GiamSat || user.RoleId == UserRoles.User))
			{
				return false;
			}

			if (user.RoleId == UserRoles.User)
				return true;
			// giám sát cho xoa người mình giám sát
			if (user.RoleId == UserRoles.GiamSat)
			{

				user.Deserialize();
				var subordinates = new List<User>();
				if (user.MemberJson != null)
				{
					subordinates.AddRange(context.Set<User>().Where(x => user.Members!.Contains(x.Id)).ToList());
				}
				if (user.GiamSatDonViThongKeIds != null)
				{
					subordinates.AddRange(context.Set<User>().Where(x => user.GiamSatDonViThongKeIds!.Contains(x.DonViThongKeId)).ToList());
				}
				user.Subordinates = subordinates.Distinct().ToList(); // Loại bỏ trùng lặp nếu có

				return CanGiamSatDelete(user, createdBy);
			}
			return false;
		}
	}
}
