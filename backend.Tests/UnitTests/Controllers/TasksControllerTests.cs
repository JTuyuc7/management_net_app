using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using backend.Controllers;
using backend.DTOs;
using backend.Models;
using backend.Repositories;
using FluentAssertions;
using Xunit;

namespace backend.Tests.UnitTests.Controllers;

/// <summary>
/// Unit tests for TasksController
/// Tests the controller logic with mocked dependencies
/// </summary>
public class TasksControllerTests
{
    private readonly Mock<ITaskRepository> _mockRepository;
    private readonly Mock<ILogger<TasksController>> _mockLogger;
    private readonly TasksController _controller;

    public TasksControllerTests()
    {
        _mockRepository = new Mock<ITaskRepository>();
        _mockLogger = new Mock<ILogger<TasksController>>();
        _controller = new TasksController(_mockRepository.Object, _mockLogger.Object);
    }

    #region GetAllTasks Tests

    [Fact]
    public async Task GetAllTasks_ReturnsOkResult_WithListOfTasks()
    {
        // Arrange
        var tasks = new List<TaskItem>
        {
            new TaskItem { Id = 1, Title = "Task 1", CreatedDate = DateTime.UtcNow },
            new TaskItem { Id = 2, Title = "Task 2", CreatedDate = DateTime.UtcNow }
        };
        _mockRepository.Setup(repo => repo.GetAllTasksAsync())
            .ReturnsAsync(tasks);

        // Act
        var result = await _controller.GetAllTasks();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedTasks = okResult.Value.Should().BeAssignableTo<IEnumerable<TaskItem>>().Subject;
        returnedTasks.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllTasks_ReturnsOkResult_WithEmptyList_WhenNoTasksExist()
    {
        // Arrange
        _mockRepository.Setup(repo => repo.GetAllTasksAsync())
            .ReturnsAsync(new List<TaskItem>());

        // Act
        var result = await _controller.GetAllTasks();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedTasks = okResult.Value.Should().BeAssignableTo<IEnumerable<TaskItem>>().Subject;
        returnedTasks.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllTasks_ReturnsInternalServerError_WhenExceptionOccurs()
    {
        // Arrange
        _mockRepository.Setup(repo => repo.GetAllTasksAsync())
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetAllTasks();

        // Assert
        var statusCodeResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusCodeResult.StatusCode.Should().Be(500);
    }

    #endregion

    #region GetTaskById Tests

    [Fact]
    public async Task GetTaskById_ReturnsOkResult_WithTask_WhenTaskExists()
    {
        // Arrange
        var task = new TaskItem { Id = 1, Title = "Test Task", Description = "Test Description" };
        _mockRepository.Setup(repo => repo.GetTaskByIdAsync(1))
            .ReturnsAsync(task);

        // Act
        var result = await _controller.GetTaskById(1);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedTask = okResult.Value.Should().BeOfType<TaskItem>().Subject;
        returnedTask.Id.Should().Be(1);
        returnedTask.Title.Should().Be("Test Task");
    }

    [Fact]
    public async Task GetTaskById_ReturnsNotFound_WhenTaskDoesNotExist()
    {
        // Arrange
        _mockRepository.Setup(repo => repo.GetTaskByIdAsync(999))
            .ReturnsAsync((TaskItem?)null);

        // Act
        var result = await _controller.GetTaskById(999);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetTaskById_ReturnsInternalServerError_WhenExceptionOccurs()
    {
        // Arrange
        _mockRepository.Setup(repo => repo.GetTaskByIdAsync(It.IsAny<int>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetTaskById(1);

        // Assert
        var statusCodeResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusCodeResult.StatusCode.Should().Be(500);
    }

    #endregion

    #region CreateTask Tests

    [Fact]
    public async Task CreateTask_ReturnsCreatedAtAction_WithTask_WhenValidData()
    {
        // Arrange
        var createDto = new CreateTaskDto
        {
            Title = "New Task",
            Description = "New Description"
        };

        var createdTask = new TaskItem
        {
            Id = 1,
            Title = "New Task",
            Description = "New Description",
            CreatedDate = DateTime.UtcNow,
            IsCompleted = false
        };

        _mockRepository.Setup(repo => repo.AddTaskAsync(It.IsAny<TaskItem>()))
            .ReturnsAsync(createdTask);

        // Act
        var result = await _controller.CreateTask(createDto);

        // Assert
        var createdAtActionResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdAtActionResult.ActionName.Should().Be(nameof(TasksController.GetTaskById));
        
        var returnedTask = createdAtActionResult.Value.Should().BeOfType<TaskItem>().Subject;
        returnedTask.Id.Should().Be(1);
        returnedTask.Title.Should().Be("New Task");
    }

    [Fact]
    public async Task CreateTask_TrimsWhitespace_FromTitleAndDescription()
    {
        // Arrange
        var createDto = new CreateTaskDto
        {
            Title = "  Trimmed Task  ",
            Description = "  Trimmed Description  "
        };

        TaskItem? capturedTask = null;
        _mockRepository.Setup(repo => repo.AddTaskAsync(It.IsAny<TaskItem>()))
            .Callback<TaskItem>(task => capturedTask = task)
            .ReturnsAsync((TaskItem task) => { task.Id = 1; return task; });

        // Act
        await _controller.CreateTask(createDto);

        // Assert
        capturedTask.Should().NotBeNull();
        capturedTask!.Title.Should().Be("Trimmed Task");
        capturedTask.Description.Should().Be("Trimmed Description");
    }

    [Fact]
    public async Task CreateTask_ReturnsBadRequest_WhenModelStateIsInvalid()
    {
        // Arrange
        var createDto = new CreateTaskDto { Title = "" }; // Invalid: empty title
        _controller.ModelState.AddModelError("Title", "Title is required");

        // Act
        var result = await _controller.CreateTask(createDto);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CreateTask_ReturnsInternalServerError_WhenExceptionOccurs()
    {
        // Arrange
        var createDto = new CreateTaskDto { Title = "Test Task" };
        _mockRepository.Setup(repo => repo.AddTaskAsync(It.IsAny<TaskItem>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.CreateTask(createDto);

        // Assert
        var statusCodeResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusCodeResult.StatusCode.Should().Be(500);
    }

    #endregion

    #region MarkTaskAsComplete Tests

    [Fact]
    public async Task MarkTaskAsComplete_ReturnsNoContent_WhenTaskExists()
    {
        // Arrange
        _mockRepository.Setup(repo => repo.MarkTaskAsCompleteAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.MarkTaskAsComplete(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task MarkTaskAsComplete_ReturnsNotFound_WhenTaskDoesNotExist()
    {
        // Arrange
        _mockRepository.Setup(repo => repo.MarkTaskAsCompleteAsync(999))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.MarkTaskAsComplete(999);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task MarkTaskAsComplete_ReturnsInternalServerError_WhenExceptionOccurs()
    {
        // Arrange
        _mockRepository.Setup(repo => repo.MarkTaskAsCompleteAsync(It.IsAny<int>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.MarkTaskAsComplete(1);

        // Assert
        var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusCodeResult.StatusCode.Should().Be(500);
    }

    #endregion

    #region DeleteTask Tests

    [Fact]
    public async Task DeleteTask_ReturnsNoContent_WhenTaskExists()
    {
        // Arrange
        _mockRepository.Setup(repo => repo.DeleteTaskAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteTask(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteTask_ReturnsNotFound_WhenTaskDoesNotExist()
    {
        // Arrange
        _mockRepository.Setup(repo => repo.DeleteTaskAsync(999))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteTask(999);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DeleteTask_ReturnsInternalServerError_WhenExceptionOccurs()
    {
        // Arrange
        _mockRepository.Setup(repo => repo.DeleteTaskAsync(It.IsAny<int>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.DeleteTask(1);

        // Assert
        var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusCodeResult.StatusCode.Should().Be(500);
    }

    #endregion

    #region Repository Interaction Tests

    [Fact]
    public async Task GetAllTasks_CallsRepositoryOnce()
    {
        // Arrange
        _mockRepository.Setup(repo => repo.GetAllTasksAsync())
            .ReturnsAsync(new List<TaskItem>());

        // Act
        await _controller.GetAllTasks();

        // Assert
        _mockRepository.Verify(repo => repo.GetAllTasksAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateTask_CallsRepositoryWithCorrectData()
    {
        // Arrange
        var createDto = new CreateTaskDto { Title = "Test", Description = "Description" };
        _mockRepository.Setup(repo => repo.AddTaskAsync(It.IsAny<TaskItem>()))
            .ReturnsAsync(new TaskItem { Id = 1, Title = "Test" });

        // Act
        await _controller.CreateTask(createDto);

        // Assert
        _mockRepository.Verify(repo => repo.AddTaskAsync(
            It.Is<TaskItem>(t => t.Title == "Test" && t.Description == "Description" && !t.IsCompleted)
        ), Times.Once);
    }

    #endregion
}
