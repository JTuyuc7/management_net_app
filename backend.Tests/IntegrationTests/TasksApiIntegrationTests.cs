using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using backend.DTOs;
using backend.Models;
using FluentAssertions;
using Xunit;

namespace backend.Tests.IntegrationTests;

/// <summary>
/// Integration tests for the Tasks API
/// Tests the full HTTP request/response pipeline
/// </summary>
public class TasksApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;
    private readonly JsonSerializerOptions _jsonOptions;

    public TasksApiIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        // Clean up after each test to ensure isolation
        await Task.CompletedTask;
    }

    #region POST /api/tasks Tests

    [Fact]
    public async Task CreateTask_ReturnsCreated_WithValidData()
    {
        // Arrange
        var createDto = new CreateTaskDto
        {
            Title = "Integration Test Task",
            Description = "This is a test task"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tasks", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var createdTask = await response.Content.ReadFromJsonAsync<TaskItem>(_jsonOptions);
        createdTask.Should().NotBeNull();
        createdTask!.Id.Should().BeGreaterThan(0);
        createdTask.Title.Should().Be("Integration Test Task");
        createdTask.Description.Should().Be("This is a test task");
        createdTask.IsCompleted.Should().BeFalse();
        createdTask.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        // Verify Location header
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().ContainAny($"/api/tasks/{createdTask.Id}", $"/api/Tasks/{createdTask.Id}");
    }

    [Fact]
    public async Task CreateTask_ReturnsCreated_WithTitleOnly()
    {
        // Arrange
        var createDto = new CreateTaskDto { Title = "Title Only Task" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tasks", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdTask = await response.Content.ReadFromJsonAsync<TaskItem>(_jsonOptions);
        createdTask.Should().NotBeNull();
        createdTask!.Title.Should().Be("Title Only Task");
        createdTask.Description.Should().BeNull();
    }

    [Fact]
    public async Task CreateTask_ReturnsBadRequest_WithEmptyTitle()
    {
        // Arrange
        var invalidDto = new { Title = "", Description = "No title" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tasks", invalidDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTask_ReturnsBadRequest_WithMissingTitle()
    {
        // Arrange
        var invalidDto = new { Description = "Missing title field" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tasks", invalidDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTask_TrimsWhitespace_FromInputs()
    {
        // Arrange
        var createDto = new CreateTaskDto
        {
            Title = "  Whitespace Task  ",
            Description = "  Whitespace Description  "
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tasks", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdTask = await response.Content.ReadFromJsonAsync<TaskItem>(_jsonOptions);
        createdTask!.Title.Should().Be("Whitespace Task");
        createdTask.Description.Should().Be("Whitespace Description");
    }

    #endregion

    #region GET /api/tasks/{id} Tests

    [Fact]
    public async Task GetTaskById_ReturnsTask_WhenTaskExists()
    {
        // Arrange - Create a task first
        var createDto = new CreateTaskDto
        {
            Title = "Get By ID Task",
            Description = "Test Description"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/tasks", createDto);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskItem>(_jsonOptions);

        // Act
        var response = await _client.GetAsync($"/api/tasks/{createdTask!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var retrievedTask = await response.Content.ReadFromJsonAsync<TaskItem>(_jsonOptions);
        retrievedTask.Should().NotBeNull();
        retrievedTask!.Id.Should().Be(createdTask.Id);
        retrievedTask.Title.Should().Be("Get By ID Task");
        retrievedTask.Description.Should().Be("Test Description");
    }

    [Fact]
    public async Task GetTaskById_ReturnsNotFound_WhenTaskDoesNotExist()
    {
        // Act
        var response = await _client.GetAsync("/api/tasks/99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region PUT /api/tasks/{id}/complete Tests

    [Fact]
    public async Task MarkTaskAsComplete_ReturnsNoContent_AndUpdatesTask()
    {
        // Arrange - Create a task
        var createDto = new CreateTaskDto { Title = "Task to Complete" };
        var createResponse = await _client.PostAsJsonAsync("/api/tasks", createDto);
        var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskItem>(_jsonOptions);
        createdTask!.IsCompleted.Should().BeFalse();

        // Act - Mark as complete
        var response = await _client.PutAsync($"/api/tasks/{createdTask.Id}/complete", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify the task is now completed
        var getResponse = await _client.GetAsync($"/api/tasks/{createdTask.Id}");
        var completedTask = await getResponse.Content.ReadFromJsonAsync<TaskItem>(_jsonOptions);
        completedTask!.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task MarkTaskAsComplete_ReturnsNotFound_WhenTaskDoesNotExist()
    {
        // Act
        var response = await _client.PutAsync("/api/tasks/99999/complete", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region DELETE /api/tasks/{id} Tests

    [Fact]
    public async Task DeleteTask_ReturnsNoContent_AndRemovesTask()
    {
        // Arrange - Create a task
        var createDto = new CreateTaskDto { Title = "Task to Delete" };
        var createResponse = await _client.PostAsJsonAsync("/api/tasks", createDto);
        var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskItem>(_jsonOptions);

        // Act - Delete the task
        var deleteResponse = await _client.DeleteAsync($"/api/tasks/{createdTask!.Id}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify the task is deleted
        var getResponse = await _client.GetAsync($"/api/tasks/{createdTask.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteTask_ReturnsNotFound_WhenTaskDoesNotExist()
    {
        // Act
        var response = await _client.DeleteAsync("/api/tasks/99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region End-to-End Workflow Tests

    [Fact]
    public async Task CompleteWorkflow_CreateGetCompleteDelete()
    {
        // Step 1: Create a task
        var createDto = new CreateTaskDto
        {
            Title = "Workflow Test Task",
            Description = "Testing complete workflow"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/tasks", createDto);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskItem>(_jsonOptions);
        var taskId = createdTask!.Id;

        // Step 2: Get the task
        var getResponse = await _client.GetAsync($"/api/tasks/{taskId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var retrievedTask = await getResponse.Content.ReadFromJsonAsync<TaskItem>(_jsonOptions);
        retrievedTask!.IsCompleted.Should().BeFalse();

        // Step 3: Mark as complete
        var completeResponse = await _client.PutAsync($"/api/tasks/{taskId}/complete", null);
        completeResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Step 4: Verify completion
        var verifyResponse = await _client.GetAsync($"/api/tasks/{taskId}");
        var completedTask = await verifyResponse.Content.ReadFromJsonAsync<TaskItem>(_jsonOptions);
        completedTask!.IsCompleted.Should().BeTrue();

        // Step 5: Delete the task
        var deleteResponse = await _client.DeleteAsync($"/api/tasks/{taskId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Step 6: Verify deletion
        var finalGetResponse = await _client.GetAsync($"/api/tasks/{taskId}");
        finalGetResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region CORS Tests

    [Fact]
    public async Task Api_AllowsCorsFromAngularApp()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/tasks");
        request.Headers.Add("Origin", "http://localhost:4200");
        request.Headers.Add("Access-Control-Request-Method", "GET");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Helper method to create a task and return the created task object
    /// </summary>
    private async Task<TaskItem> CreateTaskAsync(string title, string? description)
    {
        var createDto = new CreateTaskDto
        {
            Title = title,
            Description = description
        };

        var response = await _client.PostAsJsonAsync("/api/tasks", createDto);
        response.EnsureSuccessStatusCode();

        var createdTask = await response.Content.ReadFromJsonAsync<TaskItem>(_jsonOptions);
        return createdTask!;
    }

    #endregion
}
