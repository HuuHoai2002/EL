using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;

namespace ThongKe.Shared.Middlewares;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError($"""Exception occurred: {ex.GetType().Name}, {ex.Message}, {ex.StackTrace}""");
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message, errors) = exception switch
        {
            DbUpdateException dbEx when dbEx.InnerException is OracleException oraEx =>
                HandleOracleException(oraEx),

            DbUpdateException => (
                HttpStatusCode.InternalServerError,
                "Lỗi cơ sở dữ liệu",
                new[] { "Không thể lưu thay đổi vào cơ sở dữ liệu" }
            ),

            OracleException oraEx => HandleOracleException(oraEx),

            ArgumentException argEx => (
                HttpStatusCode.BadRequest,
                "Tham số không hợp lệ",
                new[] { argEx.Message }
            ),

            UnauthorizedAccessException => (
                HttpStatusCode.Unauthorized,
                "Không được phép",
                new[] { "Từ chối truy cập" }
            ),

            TimeoutException => (
                HttpStatusCode.RequestTimeout,
                "Hết thời gian chờ",
                new[] { "Yêu cầu mất quá nhiều thời gian để xử lý" }
            ),

            _ => (
                HttpStatusCode.InternalServerError,
                "Lỗi hệ thống",
                new[] { "Đã xảy ra lỗi không mong muốn" }
            )
        };

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var response = ApiResponse<string>.Fail(errors.Prepend(message).ToList(), true);
        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }

    private (HttpStatusCode statusCode, string message, string[] errors) HandleOracleException(OracleException ex)
    {
        return ex.Number switch
        {
            // Unique constraint violated
            1 => (
                HttpStatusCode.Conflict,
                "Dữ liệu trùng lặp.",
                new[]
                {
                    "Dữ liệu đã tồn tại, vui lòng kiểm tra lại.", ex.Message
                }
            ),

            // Cannot insert NULL into column
            1400 => (
                HttpStatusCode.BadRequest,
                "Thiếu dữ liệu bắt buộc.",
                new[] { $"Trường '{ex.Message}' không được để trống.", ex.Message }
            ),

            // Foreign key violated
            2291 or 2292 => (
                HttpStatusCode.BadRequest,
                "Vi phạm ràng buộc khóa ngoại.",
                new[] { "Bản ghi tham chiếu không tồn tại hoặc không thể xóa do có dữ liệu liên quan.", ex.Message }
            ),

            // Value too large for column
            12899 => (
                HttpStatusCode.BadRequest,
                "Dữ liệu quá dài.",
                new[] { "Văn bản cung cấp quá dài so với giới hạn cho phép.", ex.Message }
            ),

            // Numeric or value error
            1438 or 6502 => (
                HttpStatusCode.BadRequest,
                "Giá trị ngoài phạm vi.",
                new[] { "Giá trị số nằm ngoài phạm vi cho phép.", ex.Message }
            ),

            // Invalid number / conversion error
            1722 => (
                HttpStatusCode.BadRequest,
                "Định dạng dữ liệu không hợp lệ.",
                new[] { "Định dạng dữ liệu cung cấp không đúng.", ex.Message }
            ),

            // Deadlock detected
            60 => (
                HttpStatusCode.Conflict,
                "Xung đột giao dịch.",
                new[] { "Phát hiện xung đột giao dịch. Vui lòng thử lại thao tác.", ex.Message }
            ),

            // Table or view does not exist
            942 => (
                HttpStatusCode.BadRequest,
                "Lỗi cấu hình cơ sở dữ liệu.",
                new[] { "Bảng dữ liệu cần thiết không tồn tại.", ex.Message }
            ),

            // Column not found
            904 => (
                HttpStatusCode.BadRequest,
                "Lỗi cấu trúc cơ sở dữ liệu.",
                new[] { "Cột dữ liệu cần thiết không tồn tại.", ex.Message }
            ),

            // Insufficient privileges
            1031 => (
                HttpStatusCode.Forbidden,
                "Từ chối truy cập cơ sở dữ liệu.",
                new[] { "Không đủ quyền thực hiện thao tác này trên cơ sở dữ liệu.", ex.Message }
            ),

            _ => (
                HttpStatusCode.InternalServerError,
                "Lỗi cơ sở dữ liệu.",
                new[]
                {
                    $"Đã xảy ra lỗi Oracle ({ex.Number}). Vui lòng liên hệ hỗ trợ nếu vấn đề vẫn tiếp diễn.", ex.Message
                }
            )
        };
    }
}