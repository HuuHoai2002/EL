using System.ComponentModel.DataAnnotations;

namespace KHCN.Shared.Utils;

public static class EnumUtils
{
    public static string GetDisplayName<TEnum>(TEnum status)
    {
        try
        {
            if (status == null) return string.Empty;
            return status.GetType().GetMember(status.ToString())
                .FirstOrDefault()?
                .GetCustomAttributes(typeof(DisplayAttribute), false)
                .Cast<DisplayAttribute>()
                .FirstOrDefault()?.Name ?? status.ToString();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return string.Empty;
        }
    }
}