using ThongKe.Shared;

namespace ThongKe;

internal static class DependencyInjection
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<IJobQueue, JobQueue>();
        services.AddScoped<IQueryService, QueryService>();
        services.AddScoped<ITableService, TableService>();
        services.AddHostedService<WorkerService>();
        // services.AddHttpClient<IRequestService, RequestService>(client =>
        // {
        //     client.Timeout = TimeSpan.FromSeconds(10);
        // });
        return services;
    }
}