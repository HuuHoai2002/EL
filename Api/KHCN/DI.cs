using KHCN.Shared;

namespace KHCN;

internal static class DependencyInjection
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<IJobQueue, JobQueue>();
        services.AddHostedService<WorkerService>();
        return services;
    }
}