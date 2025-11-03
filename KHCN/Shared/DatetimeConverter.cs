using System.Globalization;
using Newtonsoft.Json;

namespace KHCN.Shared;

public class DatetimeConverter : JsonConverter<DateTime>
{
    // Sử dụng TimeZone offset +7 giờ cho Vietnam
    private static readonly TimeSpan VnOffset = TimeSpan.FromHours(7);

    public override DateTime ReadJson(JsonReader reader, Type objectType, DateTime existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        if (reader.Value == null)
            return default;

        var dateString = reader.Value.ToString()!;
        var dt = DateTime.Parse(dateString, CultureInfo.InvariantCulture);

        return DateTime.SpecifyKind(dt, DateTimeKind.Utc);
    }

    public override void WriteJson(JsonWriter writer, DateTime value, JsonSerializer serializer)
    {
        var utcTime = value.Kind == DateTimeKind.Utc ? value :
            value.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(value, DateTimeKind.Utc) :
            value.ToUniversalTime();
        var vnTime = utcTime.Add(VnOffset);

        writer.WriteValue(vnTime.ToString("yyyy-MM-dd HH:mm:ss"));
    }
}