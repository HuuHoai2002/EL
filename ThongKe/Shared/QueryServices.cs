using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;
using ThongKe.Data;

namespace ThongKe.Shared;

public interface IQueryService
{
    int CountDataByChiTieuId(int id);

    Task<List<Dictionary<string, object>>> GetAllDataInTable(
        string tableName,
        string? orderBy = "id",
        DataFilter? filter = null);

    Task<List<Dictionary<string, object>>> GetAllDataInTable(
        OracleConnection conn,
        string tableName,
        string? orderBy = "id",
        DataFilter? filter = null);

    Task<List<Dictionary<string, object>>> SelectDataInTable(
        OracleConnection conn,
        string tableName,
        List<string>? selectColumns,
        string? orderBy = "id",
        DataFilter? filter = null);

    Task<Dictionary<string, object?>> GetRecordInTable(
        string tableName, string columnName, int recordId);

    Task<Dictionary<string, object?>> GetRecordWhereBuilderInTable(
        string tableName, string whereBuilder);

    Task<CursorResult<Dictionary<string, object>>> GetCursorBasedDataInTable(
        string tableName,
        string cursorColumn = "id",
        int? cursor = null,
        int limit = 10,
        DataFilter? filter = null);

    Task<List<ColumnSchema>> GetTableSchema(string tableName);

    Task<Dictionary<string, object?>> GetAggregatedColumnsAsync(
        OracleConnection conn,
        string tableName,
        List<string> columns,
        string aggregateFunction = "SUM");
    // Task<List<ColumnSchema>> GetTableSchema(string tableName);
}

public class ColumnSchema
{
    public required string ColumnName { get; set; }
    public required string DataType { get; set; }
    public int? DataLength { get; set; }
    public int? DataPrecision { get; set; }
    public int? DataScale { get; set; }
    public bool IsNullable { get; set; }
    public string? DefaultValue { get; set; }
}

public class DataFilter
{
    public string? Keyword { get; set; }

    public string? CreatedBy { get; set; }
    public int? TrangThai { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
    public DateTime? UpdatedFrom { get; set; }
    public DateTime? UpdatedTo { get; set; }
    public Dictionary<string, int>? AdditionalFilters { get; set; } = null;
    public bool Ascending { get; set; } = true;
}

public class QueryService(AppDbContext context) : IQueryService
{
    public int CountDataByChiTieuId(int id)
    {
        return context.DuLieuChiTieus.Count(x => x.ChiTieuId == id);
    }

    /// <summary>
    ///     Tính toán các cột trong bảng theo tên bảng và danh sách cột.
    /// </summary>
    /// <param name="conn"></param>
    /// <param name="tableName"></param>
    /// <param name="columns"></param>
    /// <param name="aggregateFunction"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public async Task<Dictionary<string, object?>> GetAggregatedColumnsAsync(
        OracleConnection conn,
        string tableName,
        List<string> columns,
        string aggregateFunction = "SUM")
    {
        var selectAgg = string.Join(", ",
            columns.Select(c => $"{aggregateFunction.ToUpper()}(\"{c}\") AS \"{aggregateFunction.ToLower()}_{c}\""));

        var sql = $"""
                   SELECT {selectAgg}
                   FROM "{tableName}"
                   """;

        await using var cmd = new OracleCommand(sql, conn);

        var result = new Dictionary<string, object?>();
        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return result;

        for (var i = 0; i < reader.FieldCount; i++)
            result[reader.GetName(i).Split('_', 2).Last()] =
                await reader.IsDBNullAsync(i) ? null : reader.GetValue(i);
        return result;
    }

    public async Task<List<Dictionary<string, object>>> SelectDataInTable(
        OracleConnection conn,
        string tableName,
        List<string>? selectColumns,
        string? orderBy = "id",
        DataFilter? filter = null)
    {
        var results = new List<Dictionary<string, object>>();
        var conditions = new List<string>();
        var parameters = new List<DbParameter>();

        if (string.IsNullOrWhiteSpace(orderBy)) orderBy = "id";

        if (!string.IsNullOrEmpty(filter?.CreatedBy))
        {
            conditions.Add("\"created_by\" = :createdBy");
            parameters.Add(new OracleParameter("createdBy", filter.CreatedBy));
        }

        if (filter?.CreatedFrom != null)
        {
            conditions.Add("\"created_at\" >= :createdFrom");
            var param = new OracleParameter("createdFrom", OracleDbType.TimeStamp)
            {
                Value = filter.CreatedFrom.Value
            };
            parameters.Add(param);
        }

        if (filter?.CreatedTo != null)
        {
            conditions.Add("\"created_at\" <= :createdTo");
            var param = new OracleParameter("createdTo", OracleDbType.TimeStamp)
            {
                Value = filter.CreatedTo.Value
            };
            parameters.Add(param);
        }

        if (filter?.UpdatedFrom != null)
        {
            conditions.Add("\"updated_at\" >= :updatedFrom");
            var param = new OracleParameter("updatedFrom", OracleDbType.TimeStamp)
            {
                Value = filter.UpdatedFrom.Value
            };
            parameters.Add(param);
        }

        if (filter?.UpdatedTo != null)
        {
            conditions.Add("\"updated_at\" <= :updatedTo");
            var param = new OracleParameter("updatedTo", OracleDbType.TimeStamp)
            {
                Value = filter.UpdatedTo.Value
            };
            parameters.Add(param);
        }

        if (filter?.AdditionalFilters is { Count: > 0 })
        {
            foreach (var item in filter.AdditionalFilters)
            {
                var itemName = item.Key;
                var itemValue = item.Value;
                conditions.Add($"\"{itemName}\" = :{itemName}");
                var param = new OracleParameter($"{itemName}", OracleDbType.Int32)
                {
                    Value = itemValue
                };
                parameters.Add(param);
            }

            Console.WriteLine("Added additional column filters.");
        }

        var whereClause = conditions.Any() ? "WHERE " + string.Join(" AND ", conditions) : "";

        var selectClause = selectColumns != null && selectColumns.Count > 0
            ? string.Join(", ", selectColumns.Select(c => $"\"{c}\""))
            : "*";

        var sql = $"""

                                   SELECT {selectClause}
                                   FROM "{tableName}"
                                   {whereClause}
                                   ORDER BY "{orderBy}" ASC
                   """;

        if (filter?.Ascending == true) sql = sql.Replace("ASC", "DESC");

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;

        foreach (var p in parameters)
        {
            cmd.Parameters.Add(p);
            Console.WriteLine($"{p.ParameterName} = {p.Value} ({p.DbType})");
        }

        Console.WriteLine($"Excute Sql query {sql}.");
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var row = new Dictionary<string, object?>();
            for (var i = 0; i < reader.FieldCount; i++)
                row[reader.GetName(i)] = await reader.IsDBNullAsync(i) ? null : reader.GetValue(i);
            results.Add(row!);
        }

        return results;
    }

    public async Task<List<Dictionary<string, object>>> GetAllDataInTable(
        OracleConnection conn,
        string tableName,
        string? orderBy = "id",
        DataFilter? filter = null)
    {
        var results = new List<Dictionary<string, object>>();
        var conditions = new List<string>();
        var parameters = new List<DbParameter>();

        if (string.IsNullOrWhiteSpace(orderBy)) orderBy = "id";

        if (!string.IsNullOrEmpty(filter?.CreatedBy))
        {
            conditions.Add("\"created_by\" = :createdBy");
            parameters.Add(new OracleParameter("createdBy", filter.CreatedBy));
        }

        if (filter?.CreatedFrom != null)
        {
            conditions.Add("\"created_at\" >= :createdFrom");
            var param = new OracleParameter("createdFrom", OracleDbType.TimeStamp)
            {
                Value = filter.CreatedFrom.Value
            };
            parameters.Add(param);
        }

        if (filter?.CreatedTo != null)
        {
            conditions.Add("\"created_at\" <= :createdTo");
            var param = new OracleParameter("createdTo", OracleDbType.TimeStamp)
            {
                Value = filter.CreatedTo.Value
            };
            parameters.Add(param);
        }

        if (filter?.UpdatedFrom != null)
        {
            conditions.Add("\"updated_at\" >= :updatedFrom");
            var param = new OracleParameter("updatedFrom", OracleDbType.TimeStamp)
            {
                Value = filter.UpdatedFrom.Value
            };
            parameters.Add(param);
        }

        if (filter?.UpdatedTo != null)
        {
            conditions.Add("\"updated_at\" <= :updatedTo");
            var param = new OracleParameter("updatedTo", OracleDbType.TimeStamp)
            {
                Value = filter.UpdatedTo.Value
            };
            parameters.Add(param);
        }

        if (filter?.AdditionalFilters is { Count: > 0 })
        {
            foreach (var item in filter.AdditionalFilters)
            {
                var itemName = item.Key;
                var itemValue = item.Value;
                conditions.Add($"\"{itemName}\" = :{itemName}");
                var param = new OracleParameter($"{itemName}", OracleDbType.Int32)
                {
                    Value = itemValue
                };
                parameters.Add(param);
            }

            Console.WriteLine("Added additional column filters.");
        }

        var whereClause = conditions.Any() ? "WHERE " + string.Join(" AND ", conditions) : "";

        var sql = $"""

                                   SELECT *
                                   FROM "{tableName}"
                                   {whereClause}
                                   ORDER BY "{orderBy}" ASC
                   """;

        if (filter?.Ascending == true) sql = sql.Replace("ASC", "DESC");

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;

        foreach (var p in parameters)
        {
            cmd.Parameters.Add(p);
            Console.WriteLine($"{p.ParameterName} = {p.Value} ({p.DbType})");
        }

        Console.WriteLine($"Excute Sql query {sql}.");
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var row = new Dictionary<string, object?>();
            for (var i = 0; i < reader.FieldCount; i++)
                row[reader.GetName(i)] = await reader.IsDBNullAsync(i) ? null : reader.GetValue(i);
            results.Add(row!);
        }

        return results;
    }

    /// <summary>
    ///     Lấy tất cả dữ liệu trong bảng theo tên bảng với bộ lọc tùy chọn.
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="orderBy"></param>
    /// <param name="filter"></param>
    /// <returns></returns>
    public async Task<List<Dictionary<string, object>>> GetAllDataInTable(
        string tableName,
        string? orderBy = "id",
        DataFilter? filter = null)
    {
        var results = new List<Dictionary<string, object>>();
        var conditions = new List<string>();
        var parameters = new List<DbParameter>();

        if (string.IsNullOrWhiteSpace(orderBy)) orderBy = "id";

        if (!string.IsNullOrEmpty(filter?.CreatedBy))
        {
            conditions.Add("\"created_by\" = :createdBy");
            parameters.Add(new OracleParameter("createdBy", filter.CreatedBy));
        }

        if (filter?.CreatedFrom != null)
        {
            conditions.Add("\"created_at\" >= :createdFrom");
            var param = new OracleParameter("createdFrom", OracleDbType.TimeStamp)
            {
                Value = filter.CreatedFrom.Value
            };
            parameters.Add(param);
        }

        if (filter?.CreatedTo != null)
        {
            conditions.Add("\"created_at\" <= :createdTo");
            var param = new OracleParameter("createdTo", OracleDbType.TimeStamp)
            {
                Value = filter.CreatedTo.Value
            };
            parameters.Add(param);
        }

        if (filter?.UpdatedFrom != null)
        {
            conditions.Add("\"updated_at\" >= :updatedFrom");
            var param = new OracleParameter("updatedFrom", OracleDbType.TimeStamp)
            {
                Value = filter.UpdatedFrom.Value
            };
            parameters.Add(param);
        }

        if (filter?.UpdatedTo != null)
        {
            conditions.Add("\"updated_at\" <= :updatedTo");
            var param = new OracleParameter("updatedTo", OracleDbType.TimeStamp)
            {
                Value = filter.UpdatedTo.Value
            };
            parameters.Add(param);
        }

        if (filter?.AdditionalFilters is { Count: > 0 })
        {
            foreach (var item in filter.AdditionalFilters)
            {
                var itemName = item.Key;
                var itemValue = item.Value;
                conditions.Add($"\"{itemName}\" = :{itemName}");
                var param = new OracleParameter($"{itemName}", OracleDbType.Int32)
                {
                    Value = itemValue
                };
                parameters.Add(param);
            }

            Console.WriteLine("Added additional column filters.");
        }

        var whereClause = conditions.Any() ? "WHERE " + string.Join(" AND ", conditions) : "";

        var sql = $"""

                                   SELECT *
                                   FROM "{tableName}"
                                   {whereClause}
                                   ORDER BY "{orderBy}" ASC
                   """;

        if (filter?.Ascending == true) sql = sql.Replace("ASC", "DESC");

        await using var conn = context.Database.GetDbConnection();
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;

        foreach (var p in parameters)
        {
            cmd.Parameters.Add(p);
            Console.WriteLine($"{p.ParameterName} = {p.Value} ({p.DbType})");
        }

        Console.WriteLine($"Excute Sql query {sql}.");
        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var row = new Dictionary<string, object?>();
            for (var i = 0; i < reader.FieldCount; i++)
                row[reader.GetName(i)] = await reader.IsDBNullAsync(i) ? null : reader.GetValue(i);
            results.Add(row!);
        }

        return results;
    }

    /// <summary>
    ///     Lấy một bản ghi trong bảng theo tên bảng, tên cột và giá trị của cột (thường là ID).
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

        var connectionString = context.Database.GetConnectionString();
        await using var conn = new OracleConnection(connectionString);
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.Parameters.Add(new OracleParameter("recordId", OracleDbType.Int32) { Value = recordId });

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

    public async Task<Dictionary<string, object?>> GetRecordWhereBuilderInTable(
        string tableName, string whereBuilder)
    {
        var sql = $"""
                       SELECT *
                       FROM "{tableName}"
                       WHERE {whereBuilder}
                       FETCH FIRST 1 ROWS ONLY
                   """;

        var connectionString = context.Database.GetConnectionString();
        await using var conn = new OracleConnection(connectionString);
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;

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
    ///     Lấy dữ liệu trong bảng với phân trang theo con trỏ (cursor-based pagination).
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="cursorColumn"></param>
    /// <param name="cursor"></param>
    /// <param name="limit"></param>
    /// <param name="filter"></param>
    /// <returns></returns>
    public async Task<CursorResult<Dictionary<string, object>>> GetCursorBasedDataInTable(
        string tableName,
        string cursorColumn = "id",
        int? cursor = null,
        int limit = 10,
        DataFilter? filter = null)
    {
        var result = new List<Dictionary<string, object>>();
        var conditions = new List<string>();
        var parameters = new List<DbParameter>();

        if (string.IsNullOrWhiteSpace(cursorColumn)) cursorColumn = "id";

        conditions.Add($"\"{cursorColumn}\" > :cursor");
        parameters.Add(new OracleParameter("cursor", OracleDbType.Int32) { Value = cursor is > 0 ? cursor : 0 });

        if (!string.IsNullOrEmpty(filter?.CreatedBy))
        {
            conditions.Add("\"created_by\" = :createdBy");
            parameters.Add(new OracleParameter("createdBy", filter.CreatedBy));
        }

        if (filter?.CreatedFrom != null)
        {
            conditions.Add("\"created_at\" >= :createdFrom");
            var param = new OracleParameter("createdFrom", OracleDbType.TimeStamp)
            {
                Value = filter.CreatedFrom.Value
            };
            parameters.Add(param);
        }

        if (filter?.CreatedTo != null)
        {
            conditions.Add("\"created_at\" <= :createdTo");
            var param = new OracleParameter("createdTo", OracleDbType.TimeStamp)
            {
                Value = filter.CreatedTo.Value
            };
            parameters.Add(param);
        }

        if (filter?.UpdatedFrom != null)
        {
            conditions.Add("\"updated_at\" >= :updatedFrom");
            var param = new OracleParameter("updatedFrom", OracleDbType.TimeStamp)
            {
                Value = filter.UpdatedFrom.Value
            };
            parameters.Add(param);
        }

        if (filter?.UpdatedTo != null)
        {
            conditions.Add("\"updated_at\" <= :updatedTo");
            var param = new OracleParameter("updatedTo", OracleDbType.TimeStamp)
            {
                Value = filter.UpdatedTo.Value
            };
            parameters.Add(param);
        }

        if (filter?.AdditionalFilters is { Count: > 0 })
            foreach (var item in filter.AdditionalFilters)
            {
                var itemName = item.Key;
                var itemValue = item.Value;
                conditions.Add($"\"{itemName}\" = :{itemName}");
                var param = new OracleParameter($"{itemName}", OracleDbType.Int32)
                {
                    Value = itemValue
                };
                parameters.Add(param);
            }

        var whereClause = conditions.Any() ? "WHERE " + string.Join(" AND ", conditions) : "";

        var fetchLimit = limit + 1;

        var sql = $"""
                   SELECT *
                   FROM "{tableName}"
                   {whereClause}
                   ORDER BY "{cursorColumn}" ASC
                   FETCH FIRST :fetchLimit ROWS ONLY
                   """;

        if (filter?.Ascending == true) sql = sql.Replace("ASC", "DESC");

        parameters.Add(new OracleParameter("fetchLimit", OracleDbType.Int32)
        {
            Value = fetchLimit
        });

        await using var conn = context.Database.GetDbConnection();
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;

        foreach (var p in parameters)
        {
            cmd.Parameters.Add(p);
            Console.WriteLine($"{p.ParameterName} = {p.Value} ({p.DbType})");
        }

        Console.WriteLine($"Execute Cursor SQL query: {sql}");
        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var row = new Dictionary<string, object?>();
            for (var i = 0; i < reader.FieldCount; i++)
                row[reader.GetName(i)] = await reader.IsDBNullAsync(i) ? null : reader.GetValue(i);
            result.Add(row!);
        }

        var hasMore = result.Count > limit;
        if (hasMore) result.RemoveAt(result.Count - 1);

        var cursorResult = new CursorResult<Dictionary<string, object>>
        {
            Records = result,
            HasNext = hasMore
        };

        if (result.Count == 0 || !cursorResult.HasNext) return cursorResult;

        var lastRecord = result.Last();
        cursorResult.NextCursor = lastRecord.TryGetValue(cursorColumn, out var value)
            ? int.TryParse(value.ToString(), out var nextCursor) ? nextCursor : null
            : null;

        return cursorResult;
    }

    /// <summary>
    ///     Lấy cấu trúc bảng (danh sách cột và kiểu dữ liệu)
    /// </summary>
    /// <param name="tableName"></param>
    /// <returns></returns>
    public async Task<List<ColumnSchema>> GetTableSchema(string tableName)
    {
        var sql = $"""

                           SELECT 
                               COLUMN_NAME,
                               DATA_TYPE,
                               DATA_LENGTH,
                               DATA_PRECISION,
                               DATA_SCALE,
                               NULLABLE,
                               DATA_DEFAULT
                           FROM USER_TAB_COLUMNS
                           WHERE TABLE_NAME = '{tableName}'
                           ORDER BY COLUMN_ID
                   """;

        var result = new List<ColumnSchema>();

        await using var conn = context.Database.GetDbConnection();
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            result.Add(new ColumnSchema
            {
                ColumnName = reader.GetString(0),
                DataType = reader.GetString(1),
                DataLength = reader.IsDBNull(2) ? null : reader.GetInt32(2),
                DataPrecision = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                DataScale = reader.IsDBNull(4) ? null : reader.GetInt32(4),
                IsNullable = reader.GetString(5) == "Y",
                DefaultValue = reader.IsDBNull(6) ? null : reader.GetString(6)
            });

        return result;
    }
}