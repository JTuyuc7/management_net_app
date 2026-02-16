using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using backend.Repositories;
using FluentAssertions;
using Xunit;

namespace backend.Tests.UnitTests.Repositories;

/// <summary>
/// Unit tests for TaskRepository
/// Tests the data access layer with an in-memory database
/// </summary>
public class TaskRepositoryTests : IDisposable
{
    private readonly TaskDbContext _context;
    private readonly TaskRepository _repository;

    public TaskRepositoryTests()
    {
        // Create a unique in-memory database for each test
        var options = new DbContextOptionsBuilder<TaskDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TaskDbContext(options);
        _repository = new TaskRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task GetAllTasksAsync_ReturnsEmptyList_WhenNoTasksExist()
    {
        // Act
        var result = await _repository.GetAllTasksAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllTasksAsync_ReturnsAllTasks_OrderedByCreatedDateDescending()
    {
        // Arrange
        var task1 = new TaskItem { Title = "Task 1", CreatedDate = DateTime.UtcNow.AddHours(-2) };
        var task2 = new TaskItem { Title = "Task 2", CreatedDate = DateTime.UtcNow.AddHours(-1) };
        var task3 = new TaskItem { Title = "Task 3", CreatedDate = DateTime.UtcNow };

        await _repository.AddTaskAsync(task1);
        await _repository.AddTaskAsync(task2);
        await _repository.AddTaskAsync(task3);

        // Act
        var result = await _repository.GetAllTasksAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().BeInDescendingOrder(t => t.CreatedDate);
        result.First().Title.Should().Be("Task 3");
    }

    [Fact]
    public async Task GetTaskByIdAsync_ReturnsTask_WhenTaskExists()
    {
        // Arrange
        var task = new TaskItem { Title = "Test Task", Description = "Test Description" };
        var addedTask = await _repository.AddTaskAsync(task);

        // Act
        var result = await _repository.GetTaskByIdAsync(addedTask.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(addedTask.Id);
        result.Title.Should().Be("Test Task");
        result.Description.Should().Be("Test Description");
    }

    [Fact]
    public async Task GetTaskByIdAsync_ReturnsNull_WhenTaskDoesNotExist()
    {
        // Act
        var result = await _repository.GetTaskByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AddTaskAsync_CreatesTask_AndSetsCreatedDate()
    {
        // Arrange
        var task = new TaskItem
        {
            Title = "New Task",
            Description = "New Description",
            IsCompleted = false
        };

        // Act
        var result = await _repository.AddTaskAsync(task);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Title.Should().Be("New Task");
        result.Description.Should().Be("New Description");
        result.IsCompleted.Should().BeFalse();
        result.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task AddTaskAsync_PersistsTaskToDatabase()
    {
        // Arrange
        var task = new TaskItem { Title = "Persistent Task" };

        // Act
        var addedTask = await _repository.AddTaskAsync(task);
        var retrievedTask = await _repository.GetTaskByIdAsync(addedTask.Id);

        // Assert
        retrievedTask.Should().NotBeNull();
        retrievedTask!.Title.Should().Be("Persistent Task");
    }

    [Fact]
    public async Task UpdateTaskAsync_UpdatesExistingTask()
    {
        // Arrange
        var task = new TaskItem { Title = "Original Title", Description = "Original Description" };
        var addedTask = await _repository.AddTaskAsync(task);

        // Act
        addedTask.Title = "Updated Title";
        addedTask.Description = "Updated Description";
        addedTask.IsCompleted = true;
        var result = await _repository.UpdateTaskAsync(addedTask);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("Updated Title");
        result.Description.Should().Be("Updated Description");
        result.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateTaskAsync_ReturnsNull_WhenTaskDoesNotExist()
    {
        // Arrange
        var nonExistentTask = new TaskItem { Id = 999, Title = "Non-existent" };

        // Act
        var result = await _repository.UpdateTaskAsync(nonExistentTask);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteTaskAsync_RemovesTask_AndReturnsTrue()
    {
        // Arrange
        var task = new TaskItem { Title = "Task to Delete" };
        var addedTask = await _repository.AddTaskAsync(task);

        // Act
        var result = await _repository.DeleteTaskAsync(addedTask.Id);

        // Assert
        result.Should().BeTrue();
        var deletedTask = await _repository.GetTaskByIdAsync(addedTask.Id);
        deletedTask.Should().BeNull();
    }

    [Fact]
    public async Task DeleteTaskAsync_ReturnsFalse_WhenTaskDoesNotExist()
    {
        // Act
        var result = await _repository.DeleteTaskAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task MarkTaskAsCompleteAsync_MarksTaskAsComplete_AndReturnsTrue()
    {
        // Arrange
        var task = new TaskItem { Title = "Task to Complete", IsCompleted = false };
        var addedTask = await _repository.AddTaskAsync(task);

        // Act
        var result = await _repository.MarkTaskAsCompleteAsync(addedTask.Id);

        // Assert
        result.Should().BeTrue();
        var completedTask = await _repository.GetTaskByIdAsync(addedTask.Id);
        completedTask!.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task MarkTaskAsCompleteAsync_ReturnsFalse_WhenTaskDoesNotExist()
    {
        // Act
        var result = await _repository.MarkTaskAsCompleteAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task MarkTaskAsCompleteAsync_CanMarkAlreadyCompletedTask()
    {
        // Arrange
        var task = new TaskItem { Title = "Already Complete", IsCompleted = true };
        var addedTask = await _repository.AddTaskAsync(task);

        // Act
        var result = await _repository.MarkTaskAsCompleteAsync(addedTask.Id);

        // Assert
        result.Should().BeTrue();
        var completedTask = await _repository.GetTaskByIdAsync(addedTask.Id);
        completedTask!.IsCompleted.Should().BeTrue();
    }
}
