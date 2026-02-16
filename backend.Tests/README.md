# Backend Tests Documentation

This document provides comprehensive information about the test suite for the Task Management API backend.

## 📋 Table of Contents

- [Overview](#overview)
- [Test Structure](#test-structure)
- [Running Tests](#running-tests)
- [Test Coverage](#test-coverage)
- [Testing Patterns](#testing-patterns)
- [Continuous Integration](#continuous-integration)

## 🎯 Overview

The test suite includes **30+ comprehensive test cases** covering:

- **Unit Tests**: Testing individual components in isolation
- **Integration Tests**: Testing the full API request/response pipeline
- **End-to-End Workflows**: Testing complete user scenarios

### Technologies Used

- **xUnit**: Testing framework
- **Moq**: Mocking library for unit tests
- **FluentAssertions**: Readable assertion library
- **WebApplicationFactory**: Integration testing with in-memory test server
- **Entity Framework In-Memory Database**: Database testing without external dependencies

## 📁 Test Structure

```
backend.Tests/
├── backend.Tests.csproj          # Test project configuration
├── README.md                      # This file
├── UnitTests/
│   ├── Repositories/
│   │   └── TaskRepositoryTests.cs    # 13 repository tests
│   └── Controllers/
│       └── TasksControllerTests.cs   # 17 controller tests
└── IntegrationTests/
    ├── CustomWebApplicationFactory.cs # Test server configuration
    └── TasksApiIntegrationTests.cs    # 15 integration tests
```

## 🚀 Running Tests

### Prerequisites

- .NET 10.0 SDK installed
- All NuGet packages restored

### Run All Tests

```bash
# From the backend.Tests directory
dotnet test

# From the project root
dotnet test backend.Tests/backend.Tests.csproj
```

### Run Specific Test Categories

```bash
# Run only unit tests
dotnet test --filter "FullyQualifiedName~UnitTests"

# Run only integration tests
dotnet test --filter "FullyQualifiedName~IntegrationTests"

# Run only repository tests
dotnet test --filter "FullyQualifiedName~TaskRepositoryTests"

# Run only controller tests
dotnet test --filter "FullyQualifiedName~TasksControllerTests"
```

### Run Specific Test

```bash
# Run a single test by name
dotnet test --filter "FullyQualifiedName~GetAllTasks_ReturnsOkResult_WithListOfTasks"
```

### Run with Detailed Output

```bash
# Verbose output
dotnet test --verbosity detailed

# Show test output
dotnet test --logger "console;verbosity=detailed"
```

### Generate Code Coverage Report

```bash
# Run tests with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# With coverage threshold
dotnet test /p:CollectCoverage=true /p:Threshold=80
```

## 📊 Test Coverage

### Repository Tests (13 tests)

**TaskRepositoryTests.cs** - Tests data access layer with in-memory database

- ✅ GetAllTasksAsync returns empty list when no tasks exist
- ✅ GetAllTasksAsync returns all tasks ordered by CreatedDate descending
- ✅ GetTaskByIdAsync returns task when task exists
- ✅ GetTaskByIdAsync returns null when task does not exist
- ✅ AddTaskAsync creates task and sets CreatedDate
- ✅ AddTaskAsync persists task to database
- ✅ UpdateTaskAsync updates existing task
- ✅ UpdateTaskAsync returns null when task does not exist
- ✅ DeleteTaskAsync removes task and returns true
- ✅ DeleteTaskAsync returns false when task does not exist
- ✅ MarkTaskAsCompleteAsync marks task as complete and returns true
- ✅ MarkTaskAsCompleteAsync returns false when task does not exist
- ✅ MarkTaskAsCompleteAsync can mark already completed task

### Controller Tests (17 tests)

**TasksControllerTests.cs** - Tests controller logic with mocked dependencies

**GetAllTasks Tests:**
- ✅ Returns OK with list of tasks
- ✅ Returns OK with empty list when no tasks exist
- ✅ Returns 500 Internal Server Error when exception occurs

**GetTaskById Tests:**
- ✅ Returns OK with task when task exists
- ✅ Returns 404 Not Found when task does not exist
- ✅ Returns 500 Internal Server Error when exception occurs

**CreateTask Tests:**
- ✅ Returns 201 Created with task when valid data
- ✅ Trims whitespace from title and description
- ✅ Returns 400 Bad Request when model state is invalid
- ✅ Returns 500 Internal Server Error when exception occurs

**MarkTaskAsComplete Tests:**
- ✅ Returns 204 No Content when task exists
- ✅ Returns 404 Not Found when task does not exist
- ✅ Returns 500 Internal Server Error when exception occurs

**DeleteTask Tests:**
- ✅ Returns 204 No Content when task exists
- ✅ Returns 404 Not Found when task does not exist
- ✅ Returns 500 Internal Server Error when exception occurs

**Repository Interaction Tests:**
- ✅ Calls repository once for GetAllTasks
- ✅ Calls repository with correct data for CreateTask

### Integration Tests (15 tests)

**TasksApiIntegrationTests.cs** - Tests full HTTP request/response pipeline

**GET /api/tasks Tests:**
- ✅ Returns empty array when no tasks exist
- ✅ Returns all tasks after creating multiple tasks

**POST /api/tasks Tests:**
- ✅ Returns 201 Created with valid data
- ✅ Returns 201 Created with title only
- ✅ Returns 400 Bad Request with empty title
- ✅ Returns 400 Bad Request with missing title
- ✅ Trims whitespace from inputs

**GET /api/tasks/{id} Tests:**
- ✅ Returns task when task exists
- ✅ Returns 404 Not Found when task does not exist

**PUT /api/tasks/{id}/complete Tests:**
- ✅ Returns 204 No Content and updates task
- ✅ Returns 404 Not Found when task does not exist

**DELETE /api/tasks/{id} Tests:**
- ✅ Returns 204 No Content and removes task
- ✅ Returns 404 Not Found when task does not exist

**End-to-End Workflow Tests:**
- ✅ Complete workflow: Create → Get → Complete → Delete
- ✅ Multiple tasks workflow: Create multiple and verify list

**CORS Tests:**
- ✅ Allows CORS from Angular app

## 🧪 Testing Patterns

### Arrange-Act-Assert (AAA) Pattern

All tests follow the AAA pattern for clarity:

```csharp
[Fact]
public async Task GetTaskById_ReturnsTask_WhenTaskExists()
{
    // Arrange - Set up test data and dependencies
    var task = new TaskItem { Id = 1, Title = "Test Task" };
    _mockRepository.Setup(r => r.GetTaskByIdAsync(1)).ReturnsAsync(task);
    
    // Act - Execute the method being tested
    var result = await _controller.GetTaskById(1);
    
    // Assert - Verify the expected outcome
    var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
    var returnedTask = okResult.Value.Should().BeOfType<TaskItem>().Subject;
    returnedTask.Title.Should().Be("Test Task");
}
```

### Test Naming Convention

Tests follow the pattern: `MethodName_ExpectedBehavior_StateUnderTest`

Examples:
- `GetAllTasks_ReturnsOkResult_WithListOfTasks`
- `CreateTask_ReturnsBadRequest_WhenModelStateIsInvalid`
- `DeleteTask_ReturnsNotFound_WhenTaskDoesNotExist`

### Mocking with Moq

Unit tests use Moq to isolate components:

```csharp
var mockRepository = new Mock<ITaskRepository>();
mockRepository.Setup(repo => repo.GetAllTasksAsync())
    .ReturnsAsync(new List<TaskItem>());
```

### Integration Testing with WebApplicationFactory

Integration tests use a real test server:

```csharp
public class TasksApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    
    public TasksApiIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }
}
```

## 🔄 Continuous Integration

### GitHub Actions Example

```yaml
name: Run Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '10.0.x'
      - name: Restore dependencies
        run: dotnet restore
      - name: Build
        run: dotnet build --no-restore
      - name: Test
        run: dotnet test --no-build --verbosity normal
```

## 📈 Best Practices

1. **Test Isolation**: Each test is independent and doesn't rely on other tests
2. **In-Memory Database**: Each test gets a fresh database instance
3. **Descriptive Names**: Test names clearly describe what is being tested
4. **Comprehensive Coverage**: Tests cover happy paths, edge cases, and error scenarios
5. **Fast Execution**: All tests run quickly using in-memory databases
6. **Readable Assertions**: FluentAssertions makes test assertions clear and readable

## 🐛 Debugging Tests

### Run Tests in Debug Mode (Visual Studio / Rider)

1. Set breakpoints in test code
2. Right-click on test → Debug Test(s)
3. Step through code to diagnose issues

### View Test Output

```bash
dotnet test --logger "console;verbosity=detailed"
```

### Filter Failed Tests

```bash
# Re-run only failed tests
dotnet test --filter "TestCategory=Failed"
```

## 📝 Adding New Tests

### 1. Repository Test Example

```csharp
[Fact]
public async Task YourNewTest_ExpectedBehavior_StateUnderTest()
{
    // Arrange
    var task = new TaskItem { Title = "Test" };
    await _repository.AddTaskAsync(task);
    
    // Act
    var result = await _repository.GetTaskByIdAsync(task.Id);
    
    // Assert
    result.Should().NotBeNull();
    result!.Title.Should().Be("Test");
}
```

### 2. Controller Test Example

```csharp
[Fact]
public async Task YourNewTest_ExpectedBehavior_StateUnderTest()
{
    // Arrange
    _mockRepository.Setup(r => r.GetAllTasksAsync())
        .ReturnsAsync(new List<TaskItem>());
    
    // Act
    var result = await _controller.GetAllTasks();
    
    // Assert
    result.Result.Should().BeOfType<OkObjectResult>();
}
```

### 3. Integration Test Example

```csharp
[Fact]
public async Task YourNewTest_ExpectedBehavior_StateUnderTest()
{
    // Arrange
    var createDto = new CreateTaskDto { Title = "Test" };
    
    // Act
    var response = await _client.PostAsJsonAsync("/api/tasks", createDto);
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Created);
}
```

## 🎓 Learning Resources

- [xUnit Documentation](https://xunit.net/)
- [Moq Documentation](https://github.com/moq/moq4)
- [FluentAssertions Documentation](https://fluentassertions.com/)
- [ASP.NET Core Integration Tests](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests)

## 📞 Support

For questions or issues with the test suite, please refer to the main project README or open an issue in the repository.

---

**Happy Testing! 🧪✨**
