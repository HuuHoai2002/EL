using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Oracle.ManagedDataAccess.Client;
using ThongKe.Data;
using ThongKe.DTOs;
using ThongKe.Shared;
using ThongKe.Shared.Utils;

namespace ThongKe.Controllers.CT;

[Route("api/phantichchitieus")]
[ApiController]
public class PhanTichChiTieuController(
    AppDbContext context,
    IQueryService queryService,
    IConfiguration configuration
) : ControllerBase
{
    [HttpPost("get-all-records")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllRecords([FromBody] PhanTichChiTieuRequestDto request)
    {
        if (request.ChiTieuIds == null || request.ChiTieuIds.Count == 0)
            return BadRequest(ApiResponse<string>.Fail(
                ["Danh sách id chỉ tiêu không được để trống."]
            ));

        if (string.IsNullOrEmpty(request.GroupByColumn))
            return BadRequest(ApiResponse<string>.Fail(
                ["Cột phân tổ để group by không được để trống."]
            ));

        // Lấy danh sách chỉ tiêu từ database theo ids
        var chiTieus = await context.ChiTieus
            .Where(x => request.ChiTieuIds.Contains(x.Id))
            .ToListAsync();

        if (chiTieus.Count == 0)
            return BadRequest(ApiResponse<string>.Fail(
                ["Không tìm thấy chỉ tiêu nào với danh sách id đã cung cấp."]
            ));

        if (chiTieus.Count != request.ChiTieuIds.Count)
            return BadRequest(ApiResponse<string>.Fail(
                [$"Chỉ tìm thấy {chiTieus.Count}/{request.ChiTieuIds.Count} chỉ tiêu. Vui lòng kiểm tra lại danh sách id."]
            ));

        // Kiểm tra xem GroupByColumn có phải là phân tổ chung không
        var (chiTieuColumns, commonPhanToColumns, error) = QueryUtils.ExtractListChiTieuPhanTos(chiTieus);
        
        if (!string.IsNullOrEmpty(error))
            return BadRequest(ApiResponse<string>.Fail([error]));

        if (!commonPhanToColumns.Contains(request.GroupByColumn))
            return BadRequest(ApiResponse<string>.Fail(
                [$"Cột '{request.GroupByColumn}' không phải là phân tổ chung của các chỉ tiêu đã chọn."]
            ));

        // Convert request to DataFilter
        var dataFilter = new PhanTichChiTieuQueryDto
        {
            ChiTieuIds = request.ChiTieuIds,
            GroupByColumn = request.GroupByColumn,
            Cursor = request.Cursor,
            PageSize = request.PageSize,
            Filters = request.Filters,
            SearchTerm = request.SearchTerm,
            SearchColumns = request.SearchColumns,
            IncludeStatistics = request.IncludeStatistics,
            ChiTieuFilters = request.ChiTieuFilters,
            GroupByFilter = request.GroupByFilter
        };
        
        // Lấy dữ liệu từ các bảng chỉ tiêu và group by theo phân tổ chung
        var results = await GetGroupedDataFromChiTieuTables(chiTieus, request.GroupByColumn, dataFilter);
        
        return Ok(ApiResponse<object>.Ok(results));
    }

    private (string sql, List<OracleParameter> parameters) BuildFilteredSqlQuery(
        string tableName, 
        List<FilterCondition>? conditions)
    {
        var parameters = new List<OracleParameter>();
        var whereConditions = new List<string>();
        
        if (conditions?.Count > 0)
        {
            foreach (var condition in conditions)
            {
                var (conditionClause, conditionParams) = BuildWhereCondition(condition);
                if (!string.IsNullOrEmpty(conditionClause))
                {
                    whereConditions.Add(conditionClause);
                    parameters.AddRange(conditionParams);
                }
            }
        }
        
        var finalWhereClause = whereConditions.Count > 0 
            ? "WHERE " + string.Join(" AND ", whereConditions)
            : "";
            
        var sql = $"""
            SELECT *
            FROM "{tableName}"
            {finalWhereClause}
            ORDER BY "id" DESC
        """;
        
        return (sql, parameters);
    }
    
    private (string whereClause, List<OracleParameter> parameters) BuildWhereCondition(FilterCondition condition)
    {
        var parameters = new List<OracleParameter>();
        
        // Validate condition
        if (condition == null || string.IsNullOrEmpty(condition.ColumnName))
            return ("", parameters);
            
        var columnName = $"\"{condition.ColumnName}\"";
        var paramName = $"param_{condition.ColumnName}_{Guid.NewGuid():N}".Replace("-", "");
        
        try
        {
            switch (condition.Operation)
            {
                case FilterOperation.Equal:
                    if (condition.Value == null)
                        return ($"{columnName} IS NULL", parameters);
                    parameters.Add(new OracleParameter(paramName, condition.Value));
                    return ($"{columnName} = :{paramName}", parameters);
                    
                case FilterOperation.Greater:
                    if (condition.Value == null)
                        return ("", parameters);
                    parameters.Add(new OracleParameter(paramName, condition.Value));
                    return ($"{columnName} > :{paramName}", parameters);
                    
                case FilterOperation.Less:
                    if (condition.Value == null)
                        return ("", parameters);
                    parameters.Add(new OracleParameter(paramName, condition.Value));
                    return ($"{columnName} < :{paramName}", parameters);
                    
                case FilterOperation.GreaterOrEqual:
                    if (condition.Value == null)
                        return ("", parameters);
                    parameters.Add(new OracleParameter(paramName, condition.Value));
                    return ($"{columnName} >= :{paramName}", parameters);
                    
                case FilterOperation.LessOrEqual:
                    if (condition.Value == null)
                        return ("", parameters);
                    parameters.Add(new OracleParameter(paramName, condition.Value));
                    return ($"{columnName} <= :{paramName}", parameters);
                    
                case FilterOperation.Between:
                    if (condition.Value == null || condition.ValueTo == null)
                        return ("", parameters);
                        
                    var paramNameFrom = $"{paramName}_from";
                    var paramNameTo = $"{paramName}_to";
                    parameters.Add(new OracleParameter(paramNameFrom, condition.Value));
                    parameters.Add(new OracleParameter(paramNameTo, condition.ValueTo));
                    return ($"{columnName} BETWEEN :{paramNameFrom} AND :{paramNameTo}", parameters);
                    
                case FilterOperation.Contains:
                    if (condition.Value == null)
                        return ("", parameters);
                    parameters.Add(new OracleParameter(paramName, $"%{condition.Value}%"));
                    return ($"UPPER({columnName}) LIKE UPPER(:{paramName})", parameters);
                    
                case FilterOperation.StartsWith:
                    if (condition.Value == null)
                        return ("", parameters);
                    parameters.Add(new OracleParameter(paramName, $"{condition.Value}%"));
                    return ($"UPPER({columnName}) LIKE UPPER(:{paramName})", parameters);
                    
                case FilterOperation.EndsWith:
                    if (condition.Value == null)
                        return ("", parameters);
                    parameters.Add(new OracleParameter(paramName, $"%{condition.Value}"));
                    return ($"UPPER({columnName}) LIKE UPPER(:{paramName})", parameters);
                    
                default:
                    return ("", parameters);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error building where condition: {ex.Message}");
            return ("", parameters);
        }
    }
    
    private bool ApplyGroupByFilter(object groupData, GroupByFilter? groupByFilter)
    {
        if (groupByFilter?.Conditions == null || groupByFilter.Conditions.Count == 0)
            return true;
        
        try
        {
            var dynamicGroupData = (dynamic)groupData;
            
            foreach (var condition in groupByFilter.Conditions)
            {
                if (condition == null || string.IsNullOrEmpty(condition.ColumnName))
                    continue;
                    
                var passed = condition.ColumnName.ToLower() switch
                {
                    "phantovalue" => EvaluateCondition(dynamicGroupData.PhanToValue, condition),
                    "chitieusum" => EvaluateCondition(dynamicGroupData.ChiTieuSum, condition),
                    "recordcount" => EvaluateCondition(dynamicGroupData.RecordCount, condition),
                    "phantokey" => EvaluateCondition(dynamicGroupData.PhanToKey, condition),
                    "totalsum" => HasProperty(dynamicGroupData, "TotalSum") ? EvaluateCondition(dynamicGroupData.TotalSum, condition) : true,
                    "totalrecords" => HasProperty(dynamicGroupData, "TotalRecords") ? EvaluateCondition(dynamicGroupData.TotalRecords, condition) : true,
                    _ => true // Không filter nếu column không tồn tại
                };
                
                if (!passed) 
                    return false;
            }
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error applying GroupBy filter: {ex.Message}");
            return true; // Trả về true khi có lỗi để không filter
        }
    }
    
    private bool HasProperty(dynamic obj, string propertyName)
    {
        try
        {
            var value = obj.GetType().GetProperty(propertyName)?.GetValue(obj);
            return value != null;
        }
        catch
        {
            return false;
        }
    }
    
    private bool EvaluateCondition(object? value, FilterCondition condition)
    {
        if (condition == null) 
            return true;
        
        try
        {
            // Special handling for null values
            if (value == null)
                return condition.Operation == FilterOperation.Equal && condition.Value == null;
            
            var conditionResult = condition.Operation switch
            {
                FilterOperation.Equal => CompareValues(value, condition.Value) == 0,
                FilterOperation.Greater => CompareValues(value, condition.Value) > 0,
                FilterOperation.Less => CompareValues(value, condition.Value) < 0,
                FilterOperation.GreaterOrEqual => CompareValues(value, condition.Value) >= 0,
                FilterOperation.LessOrEqual => CompareValues(value, condition.Value) <= 0,
                FilterOperation.Between => condition.ValueTo != null && 
                                         CompareValues(value, condition.Value) >= 0 && 
                                         CompareValues(value, condition.ValueTo) <= 0,
                FilterOperation.Contains => value.ToString()?.ToUpper().Contains(condition.Value?.ToString()?.ToUpper() ?? "") == true,
                FilterOperation.StartsWith => value.ToString()?.ToUpper().StartsWith(condition.Value?.ToString()?.ToUpper() ?? "") == true,
                FilterOperation.EndsWith => value.ToString()?.ToUpper().EndsWith(condition.Value?.ToString()?.ToUpper() ?? "") == true,
                _ => true
            };
            
            return conditionResult;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error evaluating condition: {ex.Message}");
            return true; // Trả về true khi có lỗi để không filter
        }
    }
    
    private int CompareValues(object? value1, object? value2)
    {
        if (value1 == null && value2 == null) return 0;
        if (value1 == null) return -1;
        if (value2 == null) return 1;
        
        try
        {
            // Thử convert thành số trước
            if (double.TryParse(value1.ToString(), out var num1) && 
                double.TryParse(value2.ToString(), out var num2))
            {
                return num1.CompareTo(num2);
            }
            
            // Thử convert thành DateTime
            if (DateTime.TryParse(value1.ToString(), out var date1) &&
                DateTime.TryParse(value2.ToString(), out var date2))
            {
                return date1.CompareTo(date2);
            }
            
            // Nếu không phải số hoặc ngày thì so sánh string
            return string.Compare(value1.ToString(), value2.ToString(), StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error comparing values: {ex.Message}");
            return 0; // Trả về 0 (equal) khi có lỗi
        }
    }

    private async Task<object> GetGroupedDataFromChiTieuTables(List<Entities.ChiTieu> chiTieus, string groupByColumn, PhanTichChiTieuQueryDto data)
    {
        var result = new List<object>();

        // Lấy thông tin phân tổ chung
        var (_, commonPhanToColumns, _) = QueryUtils.ExtractListChiTieuPhanTosWithNames(chiTieus);
        var commonPhanToInfo = commonPhanToColumns.FirstOrDefault(pt => pt.ColumnName == groupByColumn);

        foreach (var chiTieu in chiTieus)
        {
            if (string.IsNullOrEmpty(chiTieu.TableName))
            {
                // Thêm chỉ tiêu không có bảng vào kết quả
                result.Add(new
                {
                    ChiTieuInfo = new
                    {
                        chiTieu.Id,
                        chiTieu.TenChiTieu,
                        chiTieu.ColumnName,
                        chiTieu.TableName,
                        chiTieu.MoTa
                    },
                    PhanToChungInfo = new
                    {
                        ColumnName = groupByColumn,
                        TenPhanTo = commonPhanToInfo.TenPhanTo
                    },
                    Data = new List<object>(),
                    TotalRecords = 0,
                    Error = "Chỉ tiêu chưa có bảng dữ liệu (TableName rỗng)"
                });
                continue;
            }

            try
            {
                // Deserialize để đảm bảo có thông tin phân tổ
                chiTieu.Deserialize();
                
                // Sử dụng raw SQL query với connection riêng để tránh connection dispose issues
                List<Dictionary<string, object>> tableData;
                try
                {
                    // Tìm filter cho chỉ tiêu hiện tại
                    var chiTieuFilter = data.ChiTieuFilters?.FirstOrDefault(f => f.ChiTieuId == chiTieu.Id);
                    
                    // Build SQL with filters
                    var (sql, parameters) = BuildFilteredSqlQuery(chiTieu.TableName, chiTieuFilter?.Conditions);
                    
                    tableData = new List<Dictionary<string, object>>();
                    
                    // Sử dụng Oracle connection trực tiếp để tránh disposal issues
                    var connectionString = configuration.GetConnectionString("OracleDbConnection");
                    using var connection = new OracleConnection(connectionString);
                    await connection.OpenAsync();
                    
                    using var command = connection.CreateCommand();
                    command.CommandText = sql;
                    
                    // Add parameters
                    foreach (var parameter in parameters)
                    {
                        command.Parameters.Add(parameter);
                    }
                    
                    using var reader = await command.ExecuteReaderAsync();
                    
                    // Debug: Log actual column names from database (only for development)
                    if (!reader.HasRows)
                    {
                        Console.WriteLine($"No data returned for ChiTieu {chiTieu.Id}");
                    }
                    
                    while (await reader.ReadAsync())
                    {
                        var row = new Dictionary<string, object>();
                        for (var i = 0; i < reader.FieldCount; i++)
                        {
                            var columnName = reader.GetName(i);
                            var value = await reader.IsDBNullAsync(i) ? null : reader.GetValue(i);
                            row[columnName] = value;
                        }
                        tableData.Add(row);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"SQL Error for ChiTieu ID {chiTieu.Id}: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    
                    // Nếu lỗi connection, skip chỉ tiêu này và tiếp tục
                    Console.WriteLine($"Error processing ChiTieu ID {chiTieu.Id}: {ex.Message}");
                    continue;
                }
                
                    // Debug: Log số lượng records
                Console.WriteLine($"ChiTieu ID {chiTieu.Id} - TableName: {chiTieu.TableName} - Records: {tableData.Count}");
                
                // Get the actual column name by removing _$$[number] suffix from chiTieu.ColumnName
                var actualColumnName = chiTieu.ColumnName;
                if (actualColumnName.Contains("_$$"))
                {
                    actualColumnName = actualColumnName.Substring(0, actualColumnName.IndexOf("_$$"));
                }
                Console.WriteLine($"ChiTieu column mapping: {chiTieu.ColumnName} -> {actualColumnName}");
                
                // Group by theo cột phân tổ chung và tính tổng cho chỉ tiêu này
                var groupedDataForChiTieu = new Dictionary<string, object>();
                var processedRecords = 0;
                var skippedRecords = 0;
                var allChiTieuValues = new List<double>(); // Thu thập tất cả giá trị để tính thống kê mô tả (chỉ khi cần)
                
                foreach (var record in tableData)
                {
                    // Debug: Log từng record
                    Console.WriteLine($"Processing record - GroupByColumn: {groupByColumn} exists: {record.ContainsKey(groupByColumn)}, ChiTieuColumn: {actualColumnName} exists: {record.ContainsKey(actualColumnName)}");
                    
                    if (!record.ContainsKey(groupByColumn) || record[groupByColumn] == null)
                    {
                        skippedRecords++;
                        Console.WriteLine($"Skipped record - missing groupByColumn: {groupByColumn}");
                        continue;
                    }

                    if (!record.ContainsKey(actualColumnName) || record[actualColumnName] == null)
                    {
                        skippedRecords++;
                        Console.WriteLine($"Skipped record - missing chiTieuColumn: {actualColumnName}");
                        continue;
                    }

                    var groupKey = record[groupByColumn]!.ToString()!;
                    
                    // Chuyển đổi giá trị chỉ tiêu thành số
                    if (double.TryParse(record[actualColumnName]!.ToString(), out double chiTieuValue))
                    {
                        processedRecords++;
                        
                        // Chỉ thu thập giá trị cho thống kê khi được yêu cầu
                        if (data.IncludeStatistics)
                        {
                            allChiTieuValues.Add(chiTieuValue);
                        }
                        
                        if (!groupedDataForChiTieu.ContainsKey(groupKey))
                        {
                            var newGroup = new
                            {
                                PhanToValue = record[groupByColumn],
                                ChiTieuSum = chiTieuValue,
                                RecordCount = 1
                            };
                            
                            // Chỉ lưu Values nếu cần tính thống kê
                            if (data.IncludeStatistics)
                            {
                                groupedDataForChiTieu[groupKey] = new
                                {
                                    newGroup.PhanToValue,
                                    newGroup.ChiTieuSum,
                                    newGroup.RecordCount,
                                    Values = new List<double> { chiTieuValue }
                                };
                            }
                            else
                            {
                                groupedDataForChiTieu[groupKey] = newGroup;
                            }
                        }
                        else
                        {
                            var existing = (dynamic)groupedDataForChiTieu[groupKey];
                            
                            if (data.IncludeStatistics)
                            {
                                var existingValues = new List<double>(existing.Values) { chiTieuValue };
                                groupedDataForChiTieu[groupKey] = new
                                {
                                    PhanToValue = existing.PhanToValue,
                                    ChiTieuSum = existing.ChiTieuSum + chiTieuValue,
                                    RecordCount = existing.RecordCount + 1,
                                    Values = existingValues
                                };
                            }
                            else
                            {
                                groupedDataForChiTieu[groupKey] = new
                                {
                                    PhanToValue = existing.PhanToValue,
                                    ChiTieuSum = existing.ChiTieuSum + chiTieuValue,
                                    RecordCount = existing.RecordCount + 1
                                };
                            }
                        }
                    }
                    else
                    {
                        skippedRecords++;
                    }
                }

                // Tính thống kê mô tả tổng thể cho chỉ tiêu (chỉ khi được yêu cầu)
                object? overallStatistics = null;
                if (data.IncludeStatistics)
                {
                    var stats = StatisticsUtils.CalculateDescriptiveStatistics(allChiTieuValues);
                    overallStatistics = new
                    {
                        stats.Mean,
                        stats.Median,
                        stats.Variance,
                        stats.StandardDeviation,
                        stats.Skewness,
                        stats.Kurtosis,
                        stats.Min,
                        stats.Max,
                        stats.Q1,
                        stats.Q3,
                        stats.IQR,
                        stats.CoefficientOfVariation,
                        stats.SkewnessInterpretation,
                        stats.KurtosisInterpretation,
                        stats.OutlierAnalysis,
                        stats.DistributionAnalysis
                    };
                }

                // Tạo object cho chỉ tiêu này
                var chiTieuResult = new
                {
                    // Thông tin cơ bản của chỉ tiêu
                    ChiTieuInfo = new
                    {
                        chiTieu.Id,
                        chiTieu.TenChiTieu,
                        chiTieu.ColumnName,
                        chiTieu.TableName,
                        chiTieu.MoTa
                    },
                    
                    // Thông tin phân tổ chung
                    PhanToChungInfo = new
                    {
                        ColumnName = groupByColumn,
                        TenPhanTo = commonPhanToInfo.TenPhanTo
                    },
                    
                    // Data được group theo phân tổ chung với thống kê cho từng group (nếu được yêu cầu)
                    Data = groupedDataForChiTieu.Select(kvp => 
                    {
                        var baseData = new
                        {
                            PhanToValue = ((dynamic)kvp.Value).PhanToValue,
                            ChiTieuSum = ((dynamic)kvp.Value).ChiTieuSum,
                            RecordCount = ((dynamic)kvp.Value).RecordCount,
                            PhanToKey = kvp.Key
                        };
                        
                        if (data.IncludeStatistics)
                        {
                            var groupValues = ((dynamic)kvp.Value).Values;
                            var groupStatistics = StatisticsUtils.CalculateDescriptiveStatistics(groupValues);
                            
                            return (object)new
                            {
                                baseData.PhanToValue,
                                baseData.ChiTieuSum,
                                baseData.RecordCount,
                                baseData.PhanToKey,
                                
                                // Thống kê mô tả cho group này
                                Statistics = new
                                {
                                    groupStatistics.Mean,
                                    groupStatistics.Median,
                                    groupStatistics.Variance,
                                    groupStatistics.StandardDeviation,
                                    groupStatistics.Skewness,
                                    groupStatistics.Kurtosis,
                                    groupStatistics.Min,
                                    groupStatistics.Max,
                                    groupStatistics.Q1,
                                    groupStatistics.Q3,
                                    groupStatistics.IQR,
                                    groupStatistics.CoefficientOfVariation,
                                    groupStatistics.SkewnessInterpretation,
                                    groupStatistics.KurtosisInterpretation,
                                    groupStatistics.OutlierAnalysis,
                                    groupStatistics.DistributionAnalysis
                                }
                            };
                        }
                        else
                        {
                            return (object)baseData;
                        }
                    })
                    .Where(item => ApplyGroupByFilter(item, data.GroupByFilter)) // Apply GroupBy filter
                    .OrderBy(x => ((dynamic)x).PhanToKey).ToList(),
                    
                    // Thống kê mô tả tổng thể cho chỉ tiêu (chỉ khi được yêu cầu)
                    OverallStatistics = overallStatistics,
                    
                    // Metadata
                    TotalRecords = tableData.Count,
                    ProcessedRecords = processedRecords,
                    SkippedRecords = skippedRecords
                };

                result.Add(chiTieuResult);
                
                // Thêm delay nhỏ để tránh connection issues
                await Task.Delay(10);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing ChiTieu ID {chiTieu.Id}: {ex.Message}");
                
                // Thêm chỉ tiêu với lỗi vào kết quả
                result.Add(new
                {
                    ChiTieuInfo = new
                    {
                        chiTieu.Id,
                        chiTieu.TenChiTieu,
                        chiTieu.ColumnName,
                        chiTieu.TableName,
                        chiTieu.MoTa
                    },
                    PhanToChungInfo = new
                    {
                        ColumnName = groupByColumn,
                        TenPhanTo = commonPhanToInfo.TenPhanTo
                    },
                    Data = new List<object>(),
                    TotalRecords = 0,
                    Error = ex.Message
                });
            }
        }

        // Group data across ChiTieus by common PhanTo values
        var crossChiTieuGroupedData = new Dictionary<string, object>();
        
        // Thu thập tất cả data từ các chỉ tiêu thành công (không có Error property)
        foreach (var chiTieuResult in result)
        {
            var resultObject = (dynamic)chiTieuResult;
            
            // Skip nếu có Error
            try
            {
                var errorCheck = resultObject.Error;
                continue; // Có Error, skip
            }
            catch
            {
                // Không có Error property, tiếp tục xử lý
            }
            
            var chiTieuData = resultObject.Data;
            var chiTieuInfo = resultObject.ChiTieuInfo;
            
            foreach (var dataItem in chiTieuData)
            {
                var phanToValue = ((dynamic)dataItem).PhanToValue?.ToString();
                if (string.IsNullOrEmpty(phanToValue)) continue;
                
                if (!crossChiTieuGroupedData.ContainsKey(phanToValue))
                {
                    crossChiTieuGroupedData[phanToValue] = new
                    {
                        PhanToValue = phanToValue,
                        ChiTieus = new List<object>(),
                        TotalSum = 0m,
                        TotalRecords = 0
                    };
                }
                
                var existingGroup = (dynamic)crossChiTieuGroupedData[phanToValue];
                var chiTieuSum = ((dynamic)dataItem).ChiTieuSum;
                var recordCount = ((dynamic)dataItem).RecordCount;
                
                var updatedChiTieus = new List<object>(existingGroup.ChiTieus)
                {
                    new
                    {
                        ChiTieuId = chiTieuInfo.Id,
                        TenChiTieu = chiTieuInfo.TenChiTieu,
                        ColumnName = chiTieuInfo.ColumnName,
                        Sum = chiTieuSum,
                        RecordCount = recordCount
                    }
                };
                
                crossChiTieuGroupedData[phanToValue] = new
                {
                    PhanToValue = phanToValue,
                    ChiTieus = updatedChiTieus,
                    TotalSum = existingGroup.TotalSum + (decimal)chiTieuSum,
                    TotalRecords = existingGroup.TotalRecords + (int)recordCount
                };
            }
        }

        // Tính thống kê mô tả cho CrossChiTieuGroupedData (chỉ khi được yêu cầu)
        object? crossGroupOverallStatistics = null;
        if (data.IncludeStatistics)
        {
            var crossGroupStatistics = crossChiTieuGroupedData.Values
                .Select(group => (double)((dynamic)group).TotalSum)
                .ToList();
            var crossStats = StatisticsUtils.CalculateDescriptiveStatistics(crossGroupStatistics);
            
            crossGroupOverallStatistics = new
            {
                crossStats.Mean,
                crossStats.Median,
                crossStats.Variance,
                crossStats.StandardDeviation,
                crossStats.Skewness,
                crossStats.Kurtosis,
                crossStats.Min,
                crossStats.Max,
                crossStats.Q1,
                crossStats.Q3,
                crossStats.IQR,
                crossStats.CoefficientOfVariation,
                crossStats.SkewnessInterpretation,
                crossStats.KurtosisInterpretation,
                crossStats.OutlierAnalysis,
                crossStats.DistributionAnalysis
            };
        }

        // Xử lý CrossChiTieuGroupedData theo điều kiện
        var crossChiTieuData = crossChiTieuGroupedData.Values
            .Select(group => 
            {
                var groupData = (dynamic)group;
                
                if (data.IncludeStatistics)
                {
                    var chiTieuSums = ((IEnumerable<object>)groupData.ChiTieus)
                        .Select(ct => (double)((dynamic)ct).Sum).ToList();
                    var groupStats = StatisticsUtils.CalculateDescriptiveStatistics(chiTieuSums);
                    
                    return (object)new
                    {
                        groupData.PhanToValue,
                        groupData.ChiTieus,
                        groupData.TotalSum,
                        groupData.TotalRecords,
                        
                        // Thống kê mô tả cho group này
                        Statistics = new
                        {
                            groupStats.Mean,
                            groupStats.Median,
                            groupStats.Variance,
                            groupStats.StandardDeviation,
                            groupStats.Skewness,
                            groupStats.Kurtosis,
                            groupStats.Min,
                            groupStats.Max,
                            groupStats.Q1,
                            groupStats.Q3,
                            groupStats.IQR,
                            groupStats.CoefficientOfVariation,
                            groupStats.SkewnessInterpretation,
                            groupStats.KurtosisInterpretation,
                            groupStats.OutlierAnalysis,
                            groupStats.DistributionAnalysis
                        }
                    };
                }
                else
                {
                    return (object)new
                    {
                        groupData.PhanToValue,
                        groupData.ChiTieus,
                        groupData.TotalSum,
                        groupData.TotalRecords
                    };
                }
            })
            .Where(item => ApplyGroupByFilter(item, data.GroupByFilter)) // Apply GroupBy filter cho CrossChiTieu data
            .OrderBy(x => ((dynamic)x).PhanToValue);

        return new
        {
            ChiTieus = result,
            CrossChiTieuGroupedData = crossChiTieuData,
                
            // Thống kê mô tả tổng thể cho tất cả CrossGroups (chỉ khi được yêu cầu)
            CrossGroupOverallStatistics = crossGroupOverallStatistics,
            
            Summary = new
            {
                TotalChiTieus = result.Count,
                GroupByColumn = groupByColumn,
                PhanToChungTen = commonPhanToInfo.TenPhanTo,
                CrossGroupCount = crossChiTieuGroupedData.Count,
                IncludeStatistics = data.IncludeStatistics
            }
        };
    }

    [HttpPost("get-common-phan-tos")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCommonPhanTos([FromBody] List<int> chiTieuIds)
    {
        if (chiTieuIds == null || chiTieuIds.Count == 0)
            return BadRequest(ApiResponse<string>.Fail(
                ["Danh sách id chỉ tiêu không được để trống."]
            ));

        // Lấy danh sách chỉ tiêu từ database theo ids
        var chiTieus = await context.ChiTieus
            .Where(x => chiTieuIds.Contains(x.Id))
            .ToListAsync();

        if (chiTieus.Count == 0)
            return BadRequest(ApiResponse<string>.Fail(
                ["Không tìm thấy chỉ tiêu nào với danh sách id đã cung cấp."]
            ));

        if (chiTieus.Count != chiTieuIds.Count)
            return BadRequest(ApiResponse<string>.Fail(
                [$"Chỉ tìm thấy {chiTieus.Count}/{chiTieuIds.Count} chỉ tiêu. Vui lòng kiểm tra lại danh sách id."]
            ));

        // Sử dụng QueryUtils để lấy phân tổ chung với tên đầy đủ
        var (chiTieuColumns, commonPhanToColumns, error) = QueryUtils.ExtractListChiTieuPhanTosWithNames(chiTieus);

        if (!string.IsNullOrEmpty(error))
            return BadRequest(ApiResponse<string>.Fail([error]));

        var result = new
        {
            ChiTieuColumns = chiTieuColumns,
            CommonPhanToColumns = commonPhanToColumns.Select(pt => new
            {
                ColumnName = pt.ColumnName,
                TenPhanTo = pt.TenPhanTo
            }),
            ChiTieus = chiTieus.Select(ct => new
            {
                ct.Id,
                ct.TenChiTieu,
                ct.ColumnName,
                ct.TableName
            })
        };

        return Ok(ApiResponse<object>.Ok(result));
    }

    // [HttpPost("cursor-filter")]
    // public async Task<IActionResult> GetCursorBasedData([FromBody] BangChiTieuQueryDto data)
    // {
    //     var phienBanChiTieu = await context.PhienBanChiTieus.FindAsync(data.PhienBanChiTieuId);
    //     if (phienBanChiTieu == null)
    //         return BadRequest(ApiResponse<string>.Fail(
    //             ["Không tìm thấy phiên bản chỉ tiêu."]
    //         ));
    //     phienBanChiTieu.Deserialize();
    //     var chiTieu = phienBanChiTieu.ChiTieus.FirstOrDefault(x => x.TableName == data.TableName);
    //     if (chiTieu == null)
    //         return BadRequest(ApiResponse<string>.Fail(
    //             ["Không tìm thấy chỉ tiêu tương ứng với bảng dữ liệu."]
    //         ));
    //     chiTieu.Deserialize();
    //     var results =
    //         await queryService.GetCursorBasedDataInTable(chiTieu.TableName!, "id", data.Cursor, data.PageSize, data);
    //     phienBanChiTieu.ChiTieus = [];
    //     results.Metadata = new Dictionary<string, object>
    //     {
    //         { "chiTieu", chiTieu },
    //         { "phienBanChiTieu", phienBanChiTieu }
    //     };
    //     return Ok(ApiResponse<Dictionary<string, object>>.CursorResult(results));
    // }
}