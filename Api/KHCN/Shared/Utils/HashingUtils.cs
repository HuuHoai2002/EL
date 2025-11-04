using System.Security.Cryptography;
using System.Text;

namespace KHCN.Shared.Utils;

public static class HashingUtils
{
    public static string Hash(string text, int length = 32)
    {
        var bytes = Encoding.UTF8.GetBytes(text);
        var hash = SHA256.HashData(bytes);
        var hex = Convert.ToHexString(hash);
        if (length > 0 && length < hex.Length) return hex[..length];
        return hex;
    }

    public static string HashId(string text)
    {
        var bytes = Encoding.UTF8.GetBytes(text);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash); // 44 ký tự
    }

    public static string HashMd5(string text)
    {
        var bytes = Encoding.UTF8.GetBytes(text);
        var hash = MD5.HashData(bytes);
        return Convert.ToHexString(hash); // 32 ký tự
    }

    public static bool CompareMd5(string text, string hashMd5)
    {
        var hash = HashMd5(text);
        return hash.Equals(hashMd5, StringComparison.OrdinalIgnoreCase);
    }

    public static string CreateHashFromListString(List<string> items)
    {
        items.Sort((a, b) => string.Compare(a, b, StringComparison.Ordinal));
        var concatenated = string.Join(",", items);
        return HashMd5(concatenated);
    }

    // Hàm băm dữ liệu theo các cột duy nhất
    public static string HashingDataWithUniqueColumns(List<string> uniqueColumns, Dictionary<string, object?> record)
    {
        var values = new List<string>();
        foreach (var col in uniqueColumns)
            if (record.ContainsKey(col) && record[col] != null)
                values.Add(record[col]!.ToString() ?? "");
        return CreateHashFromListString(values);
    }
}