using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TaskManagementSystem.Domain.Enums;
using TaskManagementSystem.Infrastructure.Persistence;

namespace TaskManagementSystem.Infrastructure.Services;

public class TaskProcessingBackgroundService : BackgroundService
{
    private readonly TaskQueue _taskQueue;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TaskProcessingBackgroundService> _logger;

    public TaskProcessingBackgroundService(
        TaskQueue taskQueue,
        IServiceProvider serviceProvider,
        ILogger<TaskProcessingBackgroundService> logger)
    {
        _taskQueue = taskQueue;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🚀 Background Task Processor started.");

        await foreach (var taskId in _taskQueue.DequeueAsync(stoppingToken))
        {
            _logger.LogInformation($"⏳ Starting background processing for Task ID: {taskId}");

            try
            {
                await Task.Delay(10000, stoppingToken);

                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var task = await dbContext.Tasks.FindAsync(new object[] { taskId }, stoppingToken);
                if (task != null)
                {
                    task.Status = Domain.Enums.TaskStatus.InProgress;
                    await dbContext.SaveChangesAsync(stoppingToken);

                    _logger.LogInformation($"✅ Task ID: {taskId} processing completed. Status updated to {task.Status}.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error occurred executing Task ID: {taskId}.");
            }
        }
    }
}