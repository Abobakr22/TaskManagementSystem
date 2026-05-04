using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using TaskManagementSystem.Application.DTOs;
using TaskManagementSystem.Application.Interfaces;
using TaskManagementSystem.Domain.Entities;
using TaskManagementSystem.Infrastructure.Persistence;

namespace TaskManagementSystem.Infrastructure.Services;

public class TaskService : ITaskService
{
    private readonly ApplicationDbContext _context;
    private readonly TaskQueue _taskQueue;
    private readonly IDistributedCache _cache;

    public TaskService(ApplicationDbContext context, TaskQueue taskQueue, IDistributedCache cache)
    {
        _context = context;
        _taskQueue = taskQueue;
        _cache = cache;
    }

    public async Task<TaskDto> CreateTaskAsync(CreateTaskDto dto, Guid userId)
    {
        var today = DateTime.UtcNow.Date;

        var isDuplicate = await _context.Tasks.AnyAsync(t =>
            t.UserId == userId &&
            t.Title.ToLower() == dto.Title.ToLower() &&
            t.CreatedAt.Date == today);

        if (isDuplicate)
            throw new Exception("You have already created a task with the same title today.");

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Description = dto.Description,
            Priority = dto.Priority,
            Status = TaskManagementSystem.Domain.Enums.TaskStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UserId = userId
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        //Bg Service
        await _taskQueue.QueueTaskAsync(task.Id);

        return MapToDto(task);
    }

    public async Task<TaskDto?> GetTaskByIdAsync(Guid taskId, Guid userId)
    {
        //Cache-Key
        string cacheKey = $"Task_{taskId}_{userId}";

        var cachedTaskString = await _cache.GetStringAsync(cacheKey);

        if (!string.IsNullOrEmpty(cachedTaskString))
        {
            return JsonSerializer.Deserialize<TaskDto>(cachedTaskString);
        }

        var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == userId);

        if (task == null) 
            return null;

        var taskDto = MapToDto(task);

        //Redis saving time
        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
        };

        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(taskDto), cacheOptions);

        return taskDto;
    }

    public async Task<IEnumerable<TaskDto>> GetAllTasksAsync(Guid userId)
    {
        var tasks = await _context.Tasks
            .Where(t => t.UserId == userId)
            .OrderBy(t => t.Priority)
            .ThenBy(t => t.CreatedAt)
            .ToListAsync();

        return tasks.Select(MapToDto);
    }

    public async Task<bool> UpdateTaskStatusAsync(Guid taskId, UpdateTaskStatusDto dto, Guid userId)
    {
        var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == userId);
        if (task == null) return false;

        task.Status = dto.Status;
        await _context.SaveChangesAsync();

        string cacheKey = $"Task_{taskId}_{userId}";
        await _cache.RemoveAsync(cacheKey);

        return true;
    }

    private static TaskDto MapToDto(TaskItem task)
    {
        return new TaskDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status.ToString(),
            Priority = task.Priority.ToString(),
            CreatedAt = task.CreatedAt
        };
    }
}