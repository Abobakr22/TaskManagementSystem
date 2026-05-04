using TaskManagementSystem.Application.DTOs;

namespace TaskManagementSystem.Application.Interfaces;

public interface ITaskService
{
    Task<TaskDto> CreateTaskAsync(CreateTaskDto dto, Guid userId);
    Task<TaskDto?> GetTaskByIdAsync(Guid taskId, Guid userId);
    Task<IEnumerable<TaskDto>> GetAllTasksAsync(Guid userId);
    Task<bool> UpdateTaskStatusAsync(Guid taskId, UpdateTaskStatusDto dto, Guid userId);
}