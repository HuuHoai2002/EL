using System.Threading.Channels;
using ThongKe.DTOs;
using Constants = ThongKe.Shared.Enums.Constants;

namespace ThongKe.Shared;

public enum JobType
{
    InsertFormRecord
}

// ReSharper disable once ClassNeverInstantiated.Global
public class Job
{
    public JobType Type { get; set; } = default!;
    public object Payload { get; set; } = null!;
}

public interface IJobQueue
{
    void Enqueue(Job job);
    IAsyncEnumerable<Job> ReadAllAsync(CancellationToken token);
}

/// <summary>
///     Hàng đợi chứa các job cần xử lý
/// </summary>
public class JobQueue : IJobQueue
{
    private readonly Channel<Job> _queue = Channel.CreateUnbounded<Job>();

    /// <summary>
    ///     Thêm job vào hàng đợi
    /// </summary>
    public void Enqueue(Job job)
    {
        _queue.Writer.TryWrite(job);
    }

    public IAsyncEnumerable<Job> ReadAllAsync(CancellationToken token)
    {
        return _queue.Reader.ReadAllAsync(token);
    }
}

/// <summary>
///     Xử lý các task chạy nền
/// </summary>
public class WorkerService : BackgroundService
{
    private readonly ILogger<WorkerService> _logger;
    private readonly IJobQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;

    // ReSharper disable once ConvertToPrimaryConstructor
    public WorkerService(IJobQueue queue, ILogger<WorkerService> logger, IServiceScopeFactory scopeFactory)
    {
        _queue = queue;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var totalJobs = 0;
        _logger.LogInformation("Worker is runing.");
        await foreach (var job in _queue.ReadAllAsync(stoppingToken))
        {
            var timeString = DateTime.Now.ToString(Constants.DateFormat);
            totalJobs++;
            try
            {
                switch (job.Type)
                {
                    case JobType.InsertFormRecord:
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var tableService = scope.ServiceProvider.GetRequiredService<ITableService>();
                        var processedRecords = (List<BieuMauProcessedPayload>)job.Payload;

                        foreach (var processedRecord in processedRecords)
                        {
                            _logger.LogInformation(
                                "[{Time}] Thêm dữ liệu vào bảng {TableName} vơi {Count} bản ghi.",
                                timeString, processedRecord.TableName, processedRecord.Records.Count);
                            var result =
                                await tableService.AutoUpsertManyRecordsToTable(processedRecord.TableName,
                                    processedRecord.Columns,
                                    processedRecord.UniqueColumns, processedRecord.Records);
                            _logger.LogInformation(
                                "[{Time}] Đã thêm dữ liệu vào bảng {TableName} với kết quả: {Result}.",
                                timeString, processedRecord.TableName, result);
                        }

                        break;
                    }

                    default:
                        _logger.LogWarning("[{Time}] Process fail because Job::{Type} not exist.", timeString,
                            job.Type);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Process fail {Type}", job.Type);
            }
        }

        _logger.LogInformation("Processed jobs: {TotalJobs}", totalJobs);
    }
}