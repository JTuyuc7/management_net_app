using Microsoft.AspNetCore.Mvc;
using backend.Models;
using backend.Repositories;
using backend.DTOs;

namespace backend.Controllers;

/// <summary>
/// API Controller for managing tasks
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITaskRepository _taskRepository;
    private readonly ILogger<TasksController> _logger;

    public TasksController(ITaskRepository taskRepository, ILogger<TasksController> logger)
    {
        _taskRepository = taskRepository;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/tasks - Returns all tasks
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TaskItem>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TaskItem>>> GetAllTasks()
    {
        try
        {
            var tasks = await _taskRepository.GetAllTasksAsync();
            return Ok(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all tasks");
            return StatusCode(500, "An error occurred while retrieving tasks");
        }
    }

    /// <summary>
    /// GET /api/tasks/{id} - Returns a specific task by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TaskItem), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskItem>> GetTaskById(int id)
    {
        try
        {
            var task = await _taskRepository.GetTaskByIdAsync(id);

            if (task == null)
            {
                return NotFound(new { message = $"Task with ID {id} not found" });
            }

            return Ok(task);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving task with ID {TaskId}", id);
            return StatusCode(500, "An error occurred while retrieving the task");
        }
    }

    /// <summary>
    /// POST /api/tasks - Creates a new task
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TaskItem), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TaskItem>> CreateTask([FromBody] CreateTaskDto createTaskDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var task = new TaskItem
            {
                Title = createTaskDto.Title.Trim(),
                Description = createTaskDto.Description?.Trim(),
                CreatedDate = DateTime.UtcNow,
                DueDate = createTaskDto.DueDate,
                Status = createTaskDto.Status?.Trim() ?? "Pending",
                Priority = createTaskDto.Priority?.Trim() ?? "Medium",
                IsCompleted = false
            };

            var createdTask = await _taskRepository.AddTaskAsync(task);

            return CreatedAtAction(
                nameof(GetTaskById),
                new { id = createdTask.Id },
                createdTask
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating task");
            return StatusCode(500, "An error occurred while creating the task");
        }
    }

    /// <summary>
    /// PUT /api/tasks/{id} - Updates an existing task
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(TaskItem), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskItem>> UpdateTask(int id, [FromBody] UpdateTaskDto updateTaskDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var taskToUpdate = new TaskItem
            {
                Id = id,
                Title = updateTaskDto.Title.Trim(),
                Description = updateTaskDto.Description?.Trim(),
                IsCompleted = updateTaskDto.IsCompleted,
                DueDate = updateTaskDto.DueDate,
                Status = updateTaskDto.Status?.Trim() ?? "Pending",
                Priority = updateTaskDto.Priority?.Trim() ?? "Medium"
            };

            var updatedTask = await _taskRepository.UpdateTaskDetailsAsync(taskToUpdate);
            if (updatedTask == null)
            {
                return NotFound(new { message = $"Task with ID {id} not found" });
            }

            return Ok(updatedTask);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task {TaskId}", id);
            return StatusCode(500, "An error occurred while updating the task");
        }
    }

    /// <summary>
    /// PUT /api/tasks/{id}/complete - Marks a task as complete
    /// </summary>
    [HttpPut("{id}/complete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkTaskAsComplete(int id)
    {
        try
        {
            var success = await _taskRepository.MarkTaskAsCompleteAsync(id);

            if (!success)
            {
                return NotFound(new { message = $"Task with ID {id} not found" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking task {TaskId} as complete", id);
            return StatusCode(500, "An error occurred while updating the task");
        }
    }

    /// <summary>
    /// DELETE /api/tasks/{id} - Deletes a task
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTask(int id)
    {
        try
        {
            var success = await _taskRepository.DeleteTaskAsync(id);

            if (!success)
            {
                return NotFound(new { message = $"Task with ID {id} not found" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting task {TaskId}", id);
            return StatusCode(500, "An error occurred while deleting the task");
        }
    }
}

