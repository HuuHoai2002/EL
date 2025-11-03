using System.Reflection;
using ThongKe.Entities;
using ThongKe.Shared.Enums;

namespace ThongKe.Shared.Utils;

public static class QueryUtils
{
    // ReSharper disable once InconsistentNaming
    private static readonly List<string> commonPhanToColumns =
    [
        "bieu_mau_id", "hashing"
    ];

    public static PropertyInfo? GetPropertyInfo<T>(string propertyName)
    {
        return typeof(T).GetProperty(propertyName,
            BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
    }

    // Lọc ra các cột hợp lệ (không phải cột phân tổ chung)
    private static List<string> FilterValidColumns(List<string> columns)
    {
        return columns
            .Where(col => !commonPhanToColumns.Contains(col))
            .Distinct()
            .ToList();
    }

    // Hàm lấy ra các chi tieu và phân tổ từ một chỉ tiêu
    public static (List<string> chiTieuColumns, List<string> phanToColumns) ExtractChiTieuPhanTo(
        ChiTieu chiTieu)
    {
        List<string> chiTieuColumns = [chiTieu.ColumnName.Trim()];
        chiTieu.Deserialize();
        var phanToColumns = FlattenPhanToColumns(chiTieu.PhanTos).ToList();
        return (FilterValidColumns(chiTieuColumns), FilterValidColumns(phanToColumns));
    }

    // Hàm lấy ra các chi tieu và phân tổ chung từ một danh sách chỉ tiêu
    public static (List<string> chiTieuColumns, List<string> commonPhanToColumns, string? error)
        ExtractListChiTieuPhanTos(
            List<ChiTieu> chiTieus)
    {
        var chiTieuColumns = chiTieus
            .Select(ct => ct.ColumnName.Trim())
            .ToList();

        chiTieus[0].Deserialize();

        var commonPhanTos = FlattenPhanToColumns(chiTieus[0].PhanTos).ToHashSet();

        foreach (var chiTieu in chiTieus.Skip(1))
        {
            chiTieu.Deserialize();
            var currentPhanTos = FlattenPhanToColumns(chiTieu.PhanTos).ToHashSet();
            commonPhanTos.IntersectWith(currentPhanTos);
        }

        if (commonPhanTos.Count == 0)
            return (chiTieuColumns, [],
                "Không có phân tổ chung giữa các chỉ tiêu này. Để tạo kiểu bảng thống kê, vui lòng chọn các chỉ tiêu có ít nhất một phân tổ chung.");

        return (FilterValidColumns(chiTieuColumns), FilterValidColumns(commonPhanTos.ToList()), null);
    }

    // Hàm lấy ra các chi tieu và phân tổ chung với tên đầy đủ từ một danh sách chỉ tiêu
    public static (List<string> chiTieuColumns, List<(string ColumnName, string TenPhanTo)> commonPhanToColumns, string?
        error)
        ExtractListChiTieuPhanTosWithNames(List<ChiTieu> chiTieus)
    {
        var chiTieuColumns = chiTieus
            .Select(ct => ct.ColumnName.Trim())
            .ToList();

        chiTieus[0].Deserialize();

        var commonPhanTos = FlattenPhanTosWithNames(chiTieus[0].PhanTos)
            .GroupBy(x => x.ColumnName)
            .ToDictionary(g => g.Key, g => g.First().TenPhanTo);

        foreach (var chiTieu in chiTieus.Skip(1))
        {
            chiTieu.Deserialize();
            var currentPhanTos = FlattenPhanTosWithNames(chiTieu.PhanTos)
                .GroupBy(x => x.ColumnName)
                .ToDictionary(g => g.Key, g => g.First().TenPhanTo);

            var keysToRemove = commonPhanTos.Keys.Except(currentPhanTos.Keys).ToList();
            foreach (var key in keysToRemove) commonPhanTos.Remove(key);
        }

        if (commonPhanTos.Count == 0)
            return (chiTieuColumns, [],
                "Không có phân tổ chung giữa các chỉ tiêu này. Để tạo kiểu bảng thống kê, vui lòng chọn các chỉ tiêu có ít nhất một phân tổ chung.");

        var filteredCommonPhanTos = commonPhanTos
            .Where(pt => !commonPhanToColumns.Contains(pt.Key))
            .Select(pt => (pt.Key, pt.Value))
            .ToList();

        return (FilterValidColumns(chiTieuColumns), filteredCommonPhanTos, null);
    }

    // Hàm đệ quy lấy ra tất cả các tên cột phân tổ từ một danh sách phân tổ
    private static List<string> FlattenPhanToColumns(IEnumerable<PhanTo> phanTos)
    {
        var result = new List<string>();
        foreach (var pt in phanTos)
        {
            result.Add(pt.ColumnName.Trim());
            if (pt.Children.Count > 0)
                result.AddRange(FlattenPhanToColumns(pt.Children));
        }

        return result;
    }

    // Hàm đệ quy lấy ra tất cả các phân tổ từ một danh sách phân tổ
    private static List<PhanTo> FlattenPhanTos(IEnumerable<PhanTo> phanTos)
    {
        var result = new List<PhanTo>();
        foreach (var pt in phanTos)
        {
            result.Add(pt);
            if (pt.Children.Count > 0)
                result.AddRange(FlattenPhanTos(pt.Children));
        }

        return result;
    }

    public static OptionValue? GetTypeByPhanToColumnName(List<PhanTo> phanTos, string columnName)
    {
        var phanTo = FlattenPhanTos(phanTos).FirstOrDefault(pt => pt.ColumnName == columnName);
        return phanTo == null ? null : Constants.LoaiDuLieus.First(x => x.Value == phanTo.DataType);
    }

    // Hàm đệ quy lấy ra tất cả thông tin phân tổ (tên cột + tên tiếng Việt) từ một danh sách phân tổ
    private static List<(string ColumnName, string TenPhanTo)> FlattenPhanTosWithNames(IEnumerable<PhanTo> phanTos)
    {
        var result = new List<(string ColumnName, string TenPhanTo)>();
        foreach (var pt in phanTos)
        {
            result.Add((pt.ColumnName.Trim(), pt.TenPhanTo.Trim()));
            if (pt.Children.Count > 0)
                result.AddRange(FlattenPhanTosWithNames(pt.Children));
        }

        return result;
    }

    // Hàm lấy ra các tên cột từ danh sách chỉ tiêu và phân tổ
    public static List<string> ExtractChiTieuColumns(List<ChiTieu> chiTieus)
    {
        var chiTieuColumns = chiTieus
            .Select(ct => ct.ColumnName.Trim())
            .ToList();

        var phanToColumns = new List<string>();
        foreach (var chiTieu in chiTieus)
        {
            chiTieu.Deserialize();
            phanToColumns.AddRange(FlattenPhanToColumns(chiTieu.PhanTos));
        }

        return chiTieuColumns
            .Concat(phanToColumns)
            .Concat(commonPhanToColumns)
            .Distinct()
            .ToList();
    }

    // Hàm lấy ra các tên cột từ danh sách chỉ tiêu và phân tổ, chỉ lấy các cột có hậu tố $danh_muc (khóa ngoại)
    public static List<string> ExtractChiTieuUniqueColumns(List<ChiTieu> chiTieus)
    {
        var phanToColumns = new List<string>();
        foreach (var chiTieu in chiTieus)
        {
            chiTieu.Deserialize();
            phanToColumns.AddRange(FlattenPhanToColumns(chiTieu.PhanTos));
        }

        var results = phanToColumns
            .Where(x => x.StartsWith("phan_to_") && x.EndsWith("$danh_muc"))
            .Distinct()
            .ToList();
        // Thêm cột hashing, bỏ cột id
        results.Add("hashing");
        results.Remove("id");
        return results;
    }

    // Hàm lấy ra các tên cột từ danh sách chỉ tiêu và phân tổ, có thêm cột id (khi cập nhật dữ liệu)
    public static List<string> ExtractChiTieuColumnsWithId(List<ChiTieu> chiTieus)
    {
        return ExtractChiTieuColumns(chiTieus)
            .Concat(["id"])
            .Distinct()
            .ToList();
    }

    public static string? ValidateDataType(OptionValue dataType, object? value)
    {
        string? result = null;
        if (value == null) return result;
        switch (dataType.AccentLabel)
        {
            case "so_lieu":
            {
                // value must by number
                if (!double.TryParse(value.ToString(), out _))
                    result = "Dữ liệu không hợp lệ. Vui lòng nhập số liệu hợp lệ.";
                break;
            }
            case "ngay":
            {
                // value must by integer between 1 and 31
                if (!int.TryParse(value.ToString(), out var intValue) || intValue < 1 || intValue > 31)
                    result = "Dữ liệu không hợp lệ. Vui lòng nhập ngày hợp lệ (từ 1 đến 31).";
                break;
            }
            case "thang":
            {
                // value must by integer between 1 and 12
                if (!int.TryParse(value.ToString(), out var intValue) || intValue < 1 || intValue > 12)
                    result = "Dữ liệu không hợp lệ. Vui lòng nhập tháng hợp lệ (từ 1 đến 12).";
                break;
            }
            case "nam":
            {
                // value must by integer between 1900 and 3000
                if (!int.TryParse(value.ToString(), out var intValue) || intValue < 1900 || intValue > 3000)
                    result = "Dữ liệu không hợp lệ. Vui lòng nhập năm hợp lệ (từ 1900 đến 2100).";
                break;
            }
            case "dung_sai":
            {
                // value must by boolean or 0/1
                if (value.ToString() != "0" && value.ToString() != "1" &&
                    !bool.TryParse(value.ToString(), out _))
                    result = "Dữ liệu không hợp lệ. Vui lòng nhập đúng/sai hợp lệ.";
                break;
            }
        }

        return result;
    }
}