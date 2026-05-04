using TaskManagementSystem.Domain.Enums;

namespace TaskManagementSystem.Domain.Entities;

public class TaskItem
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Enums.TaskStatus Status { get; set; } = Enums.TaskStatus.Pending;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // FK and Nav Prop
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
}