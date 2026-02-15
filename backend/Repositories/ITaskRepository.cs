using backend.Models;

namespace backend.Repositories;

/// <summary>
/// Repository interface for Task operations
/// </summary>
public interface ITaskRepository
{
    /// <summary>
    /// Gets all tasks
    /// </summary>
    Task<IEnumerable<TaskItem>> GetAllTasksAsync();

    /// <summary>
    /// Gets a task by its ID
    /// </summary>
    Task<TaskItem?> GetTaskByIdAsync(int id);

    /// <summary>
    /// Adds a new task
    /// </summary>
    Task<TaskItem> AddTaskAsync(TaskItem task);

    /// <summary>
    /// Updates an existing task
    /// </summary>
    Task<TaskItem?> UpdateTaskAsync(TaskItem task);

    /// <summary>
    /// Deletes a task by its ID
    /// </summary>
    Task<bool> DeleteTaskAsync(int id);

    /// <summary>
    /// Marks a task as complete
    /// </summary>
    Task<bool> MarkTaskAsCompleteAsync(int id);
}

