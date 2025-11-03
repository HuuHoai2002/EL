namespace ThongKe.Shared.Extensions;

public static class DatetimeExtension
{
    public static DateTime ToUtc(this DateTime dateTime)
    {
        return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
    }
}