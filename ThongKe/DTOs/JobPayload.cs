namespace ThongKe.DTOs;

public class BieuMauProcessedPayload
{
    public int ChiTieuId { get; set; }
    public string TableName { get; set; } = string.Empty;
    public List<Dictionary<string, object?>> Records { get; set; } = [];
    public List<string> Columns { get; set; } = [];
    public List<string> UniqueColumns { get; set; } = [];
}