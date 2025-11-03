using System.Text;
using System.Text.RegularExpressions;

namespace ThongKe.Shared.Extensions;

public static class StringExtensions
{
    private static readonly Dictionary<char, string> TelexMap = new()
    {
        // --- Nguyên âm thường ---
        ['á'] = "as", ['à'] = "af", ['ả'] = "ar", ['ã'] = "ax", ['ạ'] = "aj",
        ['ă'] = "aw", ['ắ'] = "aws", ['ằ'] = "awf", ['ẳ'] = "awr", ['ẵ'] = "awx", ['ặ'] = "awj",
        ['â'] = "aa", ['ấ'] = "aas", ['ầ'] = "aaf", ['ẩ'] = "aar", ['ẫ'] = "aax", ['ậ'] = "aaj",

        ['é'] = "es", ['è'] = "ef", ['ẻ'] = "er", ['ẽ'] = "ex", ['ẹ'] = "ej",
        ['ê'] = "ee", ['ế'] = "ees", ['ề'] = "eef", ['ể'] = "eer", ['ễ'] = "eex", ['ệ'] = "eej",

        ['í'] = "is", ['ì'] = "if", ['ỉ'] = "ir", ['ĩ'] = "ix", ['ị'] = "ij",

        ['ó'] = "os", ['ò'] = "of", ['ỏ'] = "or", ['õ'] = "ox", ['ọ'] = "oj",
        ['ô'] = "oo", ['ố'] = "oos", ['ồ'] = "oof", ['ổ'] = "oor", ['ỗ'] = "oox", ['ộ'] = "ooj",
        ['ơ'] = "ow", ['ớ'] = "ows", ['ờ'] = "owf", ['ở'] = "owr", ['ỡ'] = "owx", ['ợ'] = "owj",

        ['ú'] = "us", ['ù'] = "uf", ['ủ'] = "ur", ['ũ'] = "ux", ['ụ'] = "uj",
        ['ư'] = "uw", ['ứ'] = "uws", ['ừ'] = "uwf", ['ử'] = "uwr", ['ữ'] = "uwx", ['ự'] = "uwj",

        ['ý'] = "ys", ['ỳ'] = "yf", ['ỷ'] = "yr", ['ỹ'] = "yx", ['ỵ'] = "yj",

        ['đ'] = "dd",

        // --- Nguyên âm in hoa ---
        ['Á'] = "As", ['À'] = "Af", ['Ả'] = "Ar", ['Ã'] = "Ax", ['Ạ'] = "Aj",
        ['Ă'] = "Aw", ['Ắ'] = "Aws", ['Ằ'] = "Awf", ['Ẳ'] = "Awr", ['Ẵ'] = "Awx", ['Ặ'] = "Awj",
        ['Â'] = "Aa", ['Ấ'] = "Aas", ['Ầ'] = "Aaf", ['Ẩ'] = "Aar", ['Ẫ'] = "Aax", ['Ậ'] = "Aaj",

        ['É'] = "Es", ['È'] = "Ef", ['Ẻ'] = "Er", ['Ẽ'] = "Ex", ['Ẹ'] = "Ej",
        ['Ê'] = "Ee", ['Ế'] = "Ees", ['Ề'] = "Eef", ['Ể'] = "Eer", ['Ễ'] = "Eex", ['Ệ'] = "Eej",

        ['Í'] = "Is", ['Ì'] = "If", ['Ỉ'] = "Ir", ['Ĩ'] = "Ix", ['Ị'] = "Ij",

        ['Ó'] = "Os", ['Ò'] = "Of", ['Ỏ'] = "Or", ['Õ'] = "Ox", ['Ọ'] = "Oj",
        ['Ô'] = "Oo", ['Ố'] = "Oos", ['Ồ'] = "Oof", ['Ổ'] = "Oor", ['Ỗ'] = "Oox", ['Ộ'] = "Ooj",
        ['Ơ'] = "Ow", ['Ớ'] = "Ows", ['Ờ'] = "Owf", ['Ở'] = "Owr", ['Ỡ'] = "Owx", ['Ợ'] = "Owj",

        ['Ú'] = "Us", ['Ù'] = "Uf", ['Ủ'] = "Ur", ['Ũ'] = "Ux", ['Ụ'] = "Uj",
        ['Ư'] = "Uw", ['Ứ'] = "Uws", ['Ừ'] = "Uwf", ['Ử'] = "Uwr", ['Ữ'] = "Uwx", ['Ự'] = "Uwj",

        ['Ý'] = "Ys", ['Ỳ'] = "Yf", ['Ỷ'] = "Yr", ['Ỹ'] = "Yx", ['Ỵ'] = "Yj",

        ['Đ'] = "Dd"
    };

    public static string RemoveSpecialCharactersUnicode(this string str)
    {
        if (string.IsNullOrEmpty(str))
            return str;
        str = str.Normalize(NormalizationForm.FormC);
        var cleaned = Regex.Replace(str, @"[^\p{L}\p{N} ]", "");
        return Regex.Replace(cleaned, @"\s+", " ").Trim();
    }

    public static string ToSnakeCase(this string str)
    {
        if (string.IsNullOrEmpty(str))
            return str;
        str = str.Normalize(NormalizationForm.FormC);
        var cleaned = Regex.Replace(str, @"[^\p{L}\p{N}\s]", "");
        cleaned = cleaned.Trim();
        var snake = Regex.Replace(cleaned, @"([a-z0-9])([A-Z])", "$1_$2");
        snake = Regex.Replace(snake, @"\s+", "_").ToLower();
        return snake.Trim('_');
    }

    public static string RemoveVietnameseAccents(this string str)
    {
        if (string.IsNullOrEmpty(str))
            return str;
        str = str.RemoveSpecialCharactersUnicode();
        var vietnameseChars = new Dictionary<char, char>
        {
            // Lowercase
            { 'à', 'a' }, { 'á', 'a' }, { 'ả', 'a' }, { 'ã', 'a' }, { 'ạ', 'a' },
            { 'ă', 'a' }, { 'ằ', 'a' }, { 'ắ', 'a' }, { 'ẳ', 'a' }, { 'ẵ', 'a' }, { 'ặ', 'a' },
            { 'â', 'a' }, { 'ầ', 'a' }, { 'ấ', 'a' }, { 'ẩ', 'a' }, { 'ẫ', 'a' }, { 'ậ', 'a' },
            { 'è', 'e' }, { 'é', 'e' }, { 'ẻ', 'e' }, { 'ẽ', 'e' }, { 'ẹ', 'e' },
            { 'ê', 'e' }, { 'ề', 'e' }, { 'ế', 'e' }, { 'ể', 'e' }, { 'ễ', 'e' }, { 'ệ', 'e' },
            { 'ì', 'i' }, { 'í', 'i' }, { 'ỉ', 'i' }, { 'ĩ', 'i' }, { 'ị', 'i' },
            { 'ò', 'o' }, { 'ó', 'o' }, { 'ỏ', 'o' }, { 'õ', 'o' }, { 'ọ', 'o' },
            { 'ô', 'o' }, { 'ồ', 'o' }, { 'ố', 'o' }, { 'ổ', 'o' }, { 'ỗ', 'o' }, { 'ộ', 'o' },
            { 'ơ', 'o' }, { 'ờ', 'o' }, { 'ớ', 'o' }, { 'ở', 'o' }, { 'ỡ', 'o' }, { 'ợ', 'o' },
            { 'ù', 'u' }, { 'ú', 'u' }, { 'ủ', 'u' }, { 'ũ', 'u' }, { 'ụ', 'u' },
            { 'ư', 'u' }, { 'ừ', 'u' }, { 'ứ', 'u' }, { 'ử', 'u' }, { 'ữ', 'u' }, { 'ự', 'u' },
            { 'ỳ', 'y' }, { 'ý', 'y' }, { 'ỷ', 'y' }, { 'ỹ', 'y' }, { 'ỵ', 'y' },
            { 'đ', 'd' },
            // Uppercase
            { 'À', 'A' }, { 'Á', 'A' }, { 'Ả', 'A' }, { 'Ã', 'A' }, { 'Ạ', 'A' },
            { 'Ă', 'A' }, { 'Ằ', 'A' }, { 'Ắ', 'A' }, { 'Ẳ', 'A' }, { 'Ẵ', 'A' }, { 'Ặ', 'A' },
            { 'Â', 'A' }, { 'Ầ', 'A' }, { 'Ấ', 'A' }, { 'Ẩ', 'A' }, { 'Ẫ', 'A' }, { 'Ậ', 'A' },
            { 'È', 'E' }, { 'É', 'E' }, { 'Ẻ', 'E' }, { 'Ẽ', 'E' }, { 'Ẹ', 'E' },
            { 'Ê', 'E' }, { 'Ề', 'E' }, { 'Ế', 'E' }, { 'Ể', 'E' }, { 'Ễ', 'E' }, { 'Ệ', 'E' },
            { 'Ì', 'I' }, { 'Í', 'I' }, { 'Ỉ', 'I' }, { 'Ĩ', 'I' }, { 'Ị', 'I' },
            { 'Ò', 'O' }, { 'Ó', 'O' }, { 'Ỏ', 'O' }, { 'Õ', 'O' }, { 'Ọ', 'O' },
            { 'Ô', 'O' }, { 'Ồ', 'O' }, { 'Ố', 'O' }, { 'Ổ', 'O' }, { 'Ỗ', 'O' }, { 'Ộ', 'O' },
            { 'Ơ', 'O' }, { 'Ờ', 'O' }, { 'Ớ', 'O' }, { 'Ở', 'O' }, { 'Ỡ', 'O' }, { 'Ợ', 'O' },
            { 'Ù', 'U' }, { 'Ú', 'U' }, { 'Ủ', 'U' }, { 'Ũ', 'U' }, { 'Ụ', 'U' },
            { 'Ư', 'U' }, { 'Ừ', 'U' }, { 'Ứ', 'U' }, { 'Ử', 'U' }, { 'Ữ', 'U' }, { 'Ự', 'U' },
            { 'Ỳ', 'Y' }, { 'Ý', 'Y' }, { 'Ỷ', 'Y' }, { 'Ỹ', 'Y' }, { 'Ỵ', 'Y' },
            { 'Đ', 'D' }
        };
        var result = new char[str.Length];
        for (var i = 0; i < str.Length; i++)
        {
            var c = str[i];
            result[i] = vietnameseChars.GetValueOrDefault(c, c);
        }

        return new string(result);
    }

    public static string ChangeVietnameseAccents(this string str)
    {
        if (string.IsNullOrEmpty(str))
            return str;

        var sb = new StringBuilder(str.Length * 2);

        foreach (var c in str)
            if (TelexMap.TryGetValue(c, out var telex))
                sb.Append(telex);
            else
                sb.Append(c);

        return sb.ToString();
    }
}