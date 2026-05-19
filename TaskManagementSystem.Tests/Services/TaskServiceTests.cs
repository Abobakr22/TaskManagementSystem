using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using TaskManagementSystem.Application.DTOs;
using TaskManagementSystem.Domain.Entities;
using TaskManagementSystem.Infrastructure.Persistence;
using TaskManagementSystem.Infrastructure.Services;
using Microsoft.Extensions.Caching.Distributed;

namespace TaskManagementSystem.Tests.Services;

public class TaskServiceTests
{
    private readonly Mock<IDistributedCache> _cacheMock;
    private readonly Mock<TaskQueue> _taskQueueMock;
    private ApplicationDbContext CreateInMemoryContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new ApplicationDbContext(options);
    }

    public TaskServiceTests()
    {
        _cacheMock = new Mock<IDistributedCache>();
        _taskQueueMock = new Mock<TaskQueue>();
    }

    // ===== CreateTaskAsync =====

    [Fact]
    public async Task CreateTaskAsync_ShouldCreateTask_WhenNoDuplicate()
    {
        // Arrange
        var context = CreateInMemoryContext("CreateTask_Success");
        var service = new TaskService(context, _taskQueueMock.Object, _cacheMock.Object);
        var userId = Guid.NewGuid();
        var dto = new CreateTaskDto
        {
            Title = "New Task",
            Description = "Test Description",
            Priority = Domain.Enums.TaskPriority.Medium
        };

        // Act
        var result = await service.CreateTaskAsync(dto, userId);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("New Task");
        context.Tasks.Count().Should().Be(1);
    }

    [Fact]
    public async Task CreateTaskAsync_ShouldThrowException_WhenDuplicateTitleSameDay()
    {
        // Arrange
        var context = CreateInMemoryContext("CreateTask_Duplicate");
        var userId = Guid.NewGuid();

        context.Tasks.Add(new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = "Duplicate Task",
            Description = "desc",
            Priority = Domain.Enums.TaskPriority.Low,
            Status = Domain.Enums.TaskStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UserId = userId
        });
        await context.SaveChangesAsync();

        var service = new TaskService(context, _taskQueueMock.Object, _cacheMock.Object);
        var dto = new CreateTaskDto
        {
            Title = "Duplicate Task",
            Description = "desc",
            Priority = Domain.Enums.TaskPriority.Low
        };

        // Act
        var act = async () => await service.CreateTaskAsync(dto, userId);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*same title today*");
    }

    // ===== GetTaskByIdAsync =====

    [Fact]
    public async Task GetTaskByIdAsync_ShouldReturnTask_WhenFoundInDb()
    {
        // Arrange
        var context = CreateInMemoryContext("GetById_DB");
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();

        context.Tasks.Add(new TaskItem
        {
            Id = taskId,
            Title = "Test Task",
            Description = "desc",
            Priority = Domain.Enums.TaskPriority.High,
            Status = Domain.Enums.TaskStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UserId = userId
        });
        await context.SaveChangesAsync();

        _cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), default))
                  .ReturnsAsync((byte[]?)null);

        var service = new TaskService(context, _taskQueueMock.Object, _cacheMock.Object);

        // Act
        var result = await service.GetTaskByIdAsync(taskId, userId);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("Test Task");
    }

    [Fact]
    public async Task GetTaskByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        // Arrange
        var context = CreateInMemoryContext("GetById_NotFound");

        _cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), default))
                  .ReturnsAsync((byte[]?)null);

        var service = new TaskService(context, _taskQueueMock.Object, _cacheMock.Object);

        // Act
        var result = await service.GetTaskByIdAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    // ===== UpdateTaskStatusAsync =====

    [Fact]
    public async Task UpdateTaskStatusAsync_ShouldReturnTrue_WhenTaskExists()
    {
        // Arrange
        var context = CreateInMemoryContext("UpdateStatus_Success");
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();

        context.Tasks.Add(new TaskItem
        {
            Id = taskId,
            Title = "Task",
            Description = "desc",
            Priority = Domain.Enums.TaskPriority.Low,
            Status = Domain.Enums.TaskStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UserId = userId
        });
        await context.SaveChangesAsync();

        var service = new TaskService(context, _taskQueueMock.Object, _cacheMock.Object);
        var dto = new UpdateTaskStatusDto { Status = Domain.Enums.TaskStatus.Done };

        // Act
        var result = await service.UpdateTaskStatusAsync(taskId, dto, userId);

        // Assert
        result.Should().BeTrue();
        context.Tasks.First().Status.Should().Be(Domain.Enums.TaskStatus.Done);
    }

    [Fact]
    public async Task UpdateTaskStatusAsync_ShouldReturnFalse_WhenTaskNotFound()
    {
        // Arrange
        var context = CreateInMemoryContext("UpdateStatus_NotFound");
        var service = new TaskService(context, _taskQueueMock.Object, _cacheMock.Object);
        var dto = new UpdateTaskStatusDto { Status = Domain.Enums.TaskStatus.Done };

        // Act
        var result = await service.UpdateTaskStatusAsync(Guid.NewGuid(), dto, Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    // ===== GetAllTasksAsync =====

    [Fact]
    public async Task GetAllTasksAsync_ShouldReturnOnlyUserTasks()
    {
        // Arrange
        var context = CreateInMemoryContext("GetAll_UserFilter");
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        context.Tasks.AddRange(
            new TaskItem { Id = Guid.NewGuid(), Title = "My Task", Description = "d", Priority = Domain.Enums.TaskPriority.High, Status = Domain.Enums.TaskStatus.Pending, CreatedAt = DateTime.UtcNow, UserId = userId },
            new TaskItem { Id = Guid.NewGuid(), Title = "Other Task", Description = "d", Priority = Domain.Enums.TaskPriority.Low, Status = Domain.Enums.TaskStatus.Pending, CreatedAt = DateTime.UtcNow, UserId = otherUserId }
        );
        await context.SaveChangesAsync();

        var service = new TaskService(context, _taskQueueMock.Object, _cacheMock.Object);

        // Act
        var result = await service.GetAllTasksAsync(userId);

        // Assert
        result.Should().HaveCount(1);
        result.First().Title.Should().Be("My Task");
    }
}