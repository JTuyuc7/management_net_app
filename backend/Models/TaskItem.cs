using System.ComponentModel.DataAnnotations;

namespace backend.Models;

/// <summary>
/// Represents a task item in the task management system
/// </summary>
public class TaskItem
{
    /// <summary>
    /// Primary key, auto-generated
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Title of the task (Required)
    /// </summary>
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 200 characters")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Optional description of the task
    /// </summary>
    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }

    /// <summary>
    /// Date and time when the task was created
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Indicates whether the task has been completed
    /// </summary>
    public bool IsCompleted { get; set; } = false;
}

