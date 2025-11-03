using System.Data;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;
using ThongKe.Data;
using ThongKe.Entities;
using ThongKe.Shared.Enums;
using ThongKe.Shared.Extensions;
using ThongKe.Shared.Utils;

namespace ThongKe.Shared;

public interface ITableService
{
    Task<(string? error, int? result)> CreateBangThongKe(KieuBangThongKe kieuBang);

    Task<(string? error, int? result)> UpsertBangChiTieu(
        ChiTieu chiTieu, bool isUpdate = false);

    Task<(string? error, int? result)>
        UpdateBangThongKe(string oldTableName, KieuBangThongKe kieuBang);

    Task<(string? error, int? result)> DropTable(string tableName);

    Task<(string? error, int result)> DropMultipleTables(List<string> tableNames);

    Task<(string? error, int? result)> AddColumnInTable(string tableName, List<string> columns);

    Task<int?> DeleteManyRecordsInTable(string tableName, string columnName, List<int> ids);

    Task<int> AutoUpsertManyRecordsToTable(string tableName, List<string> columns,
        List<string> uniqueColumns,
        List<Dictionary<string, object?>> records);

    string GetTableNameForBangChiTieu(ChiTieu chiTieu, int phienBanChiTieuId);
    string GetTableNameForBangThongKe(KieuBangThongKe kieuBang);
}

public class TableService(AppDbContext context) : ITableService
{
    /// <summary>
    ///     Xóa nhiều bản ghi trong bảng dựa trên danh sách ID
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="columnName"></param>
    /// <param name="ids"></param>
    /// <returns></returns>
    public async Task<int?> DeleteManyRecordsInTable(string tableName, string columnName, List<int> ids)
    {
        if (ids.Count == 0)
            return null;

        var parameters = new List<OracleParameter>();
        var inParams = new List<string>();

        for (var i = 0; i < ids.Count; i++)
        {
            var paramName = $":p{i}";
            inParams.Add(paramName);
            parameters.Add(new OracleParameter(paramName, OracleDbType.Int32) { Value = ids[i] });
        }

        var sql = $"""DELETE FROM "{tableName}" WHERE "{columnName}" IN ({string.Join(", ", inParams)})""";

        return await context.Database.ExecuteSqlRawAsync(sql, parameters);
    }

    /// <summary>
    ///     Xóa bảng trong database
    /// </summary>
    /// <param name="tableName"></param>
    /// <returns></returns>
    public async Task<(string? error, int? result)> DropTable(string tableName)
    {
        try
        {
            var sql = $@"DROP TABLE ""{tableName}"" CASCADE CONSTRAINTS";
            var result = await context.Database.ExecuteSqlRawAsync(sql);
            return (null, result);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return (e.Message, null);
        }
    }

    /// <summary>
    ///     Xóa nhiều bảng trong database
    /// </summary>
    /// <param name="tableNames"></param>
    /// <returns></returns>
    public async Task<(string? error, int result)> DropMultipleTables(List<string> tableNames)
    {
        var totalDropped = 0;
        foreach (var tableName in tableNames)
        {
            var (error, result) = await DropTable(tableName);
            if (error != null)
                return ($"Lỗi khi xóa bảng {tableName}: {error}", totalDropped);
            if (result != null)
                totalDropped += result.Value;
        }

        return (null, totalDropped);
    }

    /// <summary>
    ///     Cập nhật bảng thống kê bằng cách xóa bảng cũ và tạo bảng mới
    /// </summary>
    /// <param name="oldTableName"></param>
    /// <param name="kieuBang"></param>
    /// <returns></returns>
    public async Task<(string? error, int? result)> UpdateBangThongKe(string oldTableName, KieuBangThongKe kieuBang)
    {
        try
        {
            var dropResult = await DropTable(oldTableName);
            if (dropResult.error != null)
                return ($"Lỗi khi xóa bảng cũ: {dropResult.error}", null);

            var (chiTieuColumns, commonPhanToColumns, error) = QueryUtils.ExtractListChiTieuPhanTos(kieuBang.ChiTieus);
            if (error != null) return (error, null);
            var result = await CreateTableBase(kieuBang.TableName!, chiTieuColumns, commonPhanToColumns);
            return (null, result);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return (e.Message, null);
        }
    }

    /// <summary>
    ///     Tạo bảng thống kê
    /// </summary>
    /// <param name="kieuBang"></param>
    /// <returns></returns>
    public async Task<(string? error, int? result)> CreateBangThongKe(KieuBangThongKe kieuBang)
    {
        try
        {
            var (chiTieuColumns, commonPhanToColumns, error) = QueryUtils.ExtractListChiTieuPhanTos(kieuBang.ChiTieus);
            if (error != null) return (error, null);
            var result = await CreateTableBase(kieuBang.TableName!, chiTieuColumns, commonPhanToColumns);
            return (null, result);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return (e.Message, null);
        }
    }

    /// <summary>
    ///     Tạo bảng chỉ tiêu cho mỗi chỉ tiêu trong phiên bản chỉ tiêu
    /// </summary>
    /// <param name="chiTieu"></param>
    /// <returns></returns>
    public async Task<(string? error, int? result)> UpsertBangChiTieu(ChiTieu chiTieu, bool isUpdate = false)
    {
        try
        {
            var (chiTieuColumns, phanToColumns) = QueryUtils.ExtractChiTieuPhanTo(chiTieu);
            var conn = context.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open) await conn.OpenAsync();
            if (isUpdate)
            {
                var isExists = await CheckTableExists((OracleConnection)conn, chiTieu.TableName!);
                if (isExists > 0)
                {
                    var dropResult = await DropTable(chiTieu.TableName!);
                    if (dropResult.error != null)
                        Console.WriteLine($"Lỗi khi xóa bảng cũ: {dropResult.error}");
                }
            }

            var result = await CreateTableBase(chiTieu.TableName!, chiTieuColumns, phanToColumns, [
                "bieu_mau_id::NUMBER(10)"
            ]);

            return (null, result);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return (e.Message, null);
        }
    }

    /// <summary>
    ///     Tự động thêm mới hoặc cập nhật nhiều bản ghi trong bảng dựa trên danh sách các cột và cột duy nhất
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="columns"></param>
    /// <param name="uniqueColumns"></param>
    /// <param name="records"></param>
    /// <returns></returns>
    public async Task<int> AutoUpsertManyRecordsToTable(string tableName, List<string> columns,
        List<string> uniqueColumns,
        List<Dictionary<string, object?>> records)
    {
        const string idColumn = "id";

        var recordsWithoutId = records.Where(r =>
            !r.ContainsKey(idColumn) || string.IsNullOrEmpty(r[idColumn]!.ToString())).ToList();

        var totalAffected = 0;

        await using var conn = (OracleConnection)context.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open) await conn.OpenAsync();
        if (recordsWithoutId.Count == 0) return totalAffected;

        var insertColumns = columns.Where(c => !c.Equals(idColumn, StringComparison.OrdinalIgnoreCase)).ToList();

        var affected =
            await InsertMultipleDataWithUniqueColumns(conn, tableName, insertColumns, uniqueColumns,
                recordsWithoutId);
        totalAffected += affected;

        return totalAffected;
    }

    /// <summary>
    ///     Thêm cột mới vào bảng
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="columns"></param>
    /// <returns></returns>
    public async Task<(string? error, int? result)> AddColumnInTable(string tableName, List<string> columns)
    {
        try
        {
            var columnsDef = string.Join(", ", columns.Select(c => $"\"{c}\" NUMBER"));
            var sql = $"""
                           ALTER TABLE "{tableName}"
                           ADD {columnsDef}
                       """;
            var result = await context.Database.ExecuteSqlRawAsync(sql);
            return (null, result);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return (e.Message, null);
        }
    }

    /// <summary>
    ///     Lấy tên bảng cho bảng chỉ tiêu dựa trên tên chỉ tiêu và phiên bản chỉ tiêu
    /// </summary>
    /// <param name="chiTieu"></param>
    /// <param name="phienBanChiTieuId"></param>
    /// <returns></returns>
    public string GetTableNameForBangChiTieu(ChiTieu chiTieu, int phienBanChiTieuId)
    {
        return
            $"BangChiTieu_{chiTieu.TenChiTieu.RemoveVietnameseAccents().ToSnakeCase()}_$ChiTieuId{chiTieu.Id}_$PhienBanId{phienBanChiTieuId}";
    }

    /// <summary>
    ///     Lấy tên bảng cho bảng thống kê dựa trên tên kiểu bảng và ID kiểu bảng
    /// </summary>
    /// <param name="kieuBang"></param>
    /// <returns></returns>
    public string GetTableNameForBangThongKe(KieuBangThongKe kieuBang)
    {
        return
            $"BangThongKe_{kieuBang.Ten.RemoveVietnameseAccents().ToSnakeCase()}_$KieuBangId{kieuBang.Id}";
    }

    /// <summary>
    ///     Lấy tổng số dòng trong bảng
    /// </summary>
    /// <param name="tableName"></param>
    /// <returns></returns>
    public async Task<int> GetTableRowCount(string tableName)
    {
        var sql = $@"SELECT COUNT(*) FROM ""{tableName}""";
        await using var conn = context.Database.GetDbConnection();
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    /// <summary>
    ///     Lấy một bản ghi trong bảng dựa trên ID
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="columnName"></param>
    /// <param name="recordId"></param>
    /// <returns></returns>
    public async Task<Dictionary<string, object?>> GetRecordInTable(
        string tableName, string columnName, int recordId)
    {
        var sql = $"""

                           SELECT *
                           FROM "{tableName}"
                           WHERE "{columnName}" = :recordId
                           FETCH FIRST 1 ROWS ONLY
                       
                   """;

        await using var conn = context.Database.GetDbConnection();
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;

        cmd.Parameters.Add(new OracleParameter("recordId", OracleDbType.Int32) { Value = recordId });

        Console.WriteLine($"Execute Sql query {sql} with recordId={recordId}");

        await using var reader = await cmd.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            var row = new Dictionary<string, object?>();
            for (var i = 0; i < reader.FieldCount; i++)
                row[reader.GetName(i)] = await reader.IsDBNullAsync(i) ? null : reader.GetValue(i);
            return row;
        }

        return new Dictionary<string, object?>();
    }

    /// <summary>
    ///     Tạo bảng cơ sở với các cột chỉ tiêu và phân tổ, có thể thêm các cột bổ sung khác
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="chiTieuColumns"></param>
    /// <param name="phanToColumns"></param>
    /// <param name="additionalColumns"></param>
    /// <returns></returns>
    private async Task<int?> CreateTableBase(string tableName, List<string> chiTieuColumns, List<string> phanToColumns,
        List<string>? additionalColumns = null)
    {
        var columns = new List<string>
        {
            "\"id\" NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY",
            "\"trang_thai\" NUMBER(1) DEFAULT 1",
            "\"hashing\" VARCHAR2(255)",
            "\"tinh_thanh_id\" VARCHAR2(255)",
            "\"don_vi_id\" VARCHAR2(255)",
            "\"scope\" VARCHAR2(255)",
            "\"co_quan_thuc_hien_id\" VARCHAR2(255)",
            "\"created_at\" TIMESTAMP WITH TIME ZONE DEFAULT (CURRENT_TIMESTAMP AT TIME ZONE 'UTC')",
            "\"updated_at\" TIMESTAMP WITH TIME ZONE DEFAULT (CURRENT_TIMESTAMP AT TIME ZONE 'UTC')",
            "\"created_by\" VARCHAR2(255)",
            "\"updated_by\" VARCHAR2(255)"
        };

        if (additionalColumns is { Count: > 0 })
            columns.AddRange(additionalColumns.Select(c =>
            {
                var part = c.Split("::", StringSplitOptions.RemoveEmptyEntries);
                return part.Length == 2 ? $"\"{part[0]}\" {part[1]}" : $"\"{c}\" VARCHAR2(255)";
            }));

        columns.AddRange(chiTieuColumns.Select(c => $"\"{c}\" NUMBER"));

        // Auto detect data types for phan to columns
        foreach (var phanToColumn in phanToColumns)
        {
            var dataType = Constants.LoaiDuLieus.FirstOrDefault(x => phanToColumn.EndsWith($"_${x.Value}"));
            columns.Add($"\"{phanToColumn}\" {dataType?.OracleDbType}");
        }

        var sql = $"""
                       CREATE TABLE "{tableName}" (
                           {string.Join(",\n", columns)}
                       )
                   """;
        return await context.Database.ExecuteSqlRawAsync(sql);
    }

    /// <summary>
    ///     Kiểm tra bảng có tồn tại trong database hay không
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="conn"></param>
    /// <returns></returns>
    private async Task<int> CheckTableExists(OracleConnection conn, string tableName)
    {
        var sql = $"""
                        SELECT COUNT(*) 
                        FROM USER_TABLES 
                        WHERE TABLE_NAME = '{tableName}' 
                   """;

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }


    /// <summary>
    ///     Thêm nhiều bản ghi với các cột duy nhất để tránh trùng lặp
    /// </summary>
    /// <param name="conn"></param>
    /// <param name="tableName"></param>
    /// <param name="columns"></param>
    /// <param name="uniqueColumns"></param>
    /// <param name="records"></param>
    /// <returns></returns>
    private async Task<int> InsertMultipleDataWithUniqueColumns(OracleConnection conn, string tableName,
        List<string> columns,
        List<string> uniqueColumns,
        List<Dictionary<string, object?>> records)
    {
        var matchConditions = "target.\"hashing\" = source.\"hashing\"";
        var updateColumns = columns.Where(c => !uniqueColumns.Contains(c)).ToList();
        var insertColumnList = string.Join(", ", columns.Select(c => $"\"{c}\""));
        var insertValueList = string.Join(", ", columns.Select(c => $"source.\"{c}\""));
        var updateSetClause = string.Join(", ", updateColumns.Select(c => $"target.\"{c}\" = source.\"{c}\""))
                              + ", target.\"updated_at\" = CURRENT_TIMESTAMP AT TIME ZONE 'UTC'";

        var totalProcessed = 0;

        foreach (var record in records)
        {
            var sourceColumns = string.Join(", ", columns.Select(c => $":{c} as \"{c}\""));

            var sql = $"""
                       MERGE INTO "{tableName}" target
                       USING (SELECT {sourceColumns} FROM dual) source
                       ON ({matchConditions})
                       WHEN MATCHED THEN
                           UPDATE SET {updateSetClause}
                       WHEN NOT MATCHED THEN
                           INSERT ({insertColumnList})
                           VALUES ({insertValueList})
                       """;

            await using var cmd = new OracleCommand(sql, conn);

            foreach (var column in columns)
            {
                var value = record.TryGetValue(column, out var val) ? val ?? DBNull.Value : DBNull.Value;
                cmd.Parameters.Add(new OracleParameter(column, value));
            }

            var rowsAffected = await cmd.ExecuteNonQueryAsync();
            totalProcessed += rowsAffected;
        }

        return totalProcessed;
    }
}