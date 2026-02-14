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

    /// <summary>
    /// Optional due date for the task
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// Optional status of the task (e.g., "Pending", "In Progress", "Completed")
    /// </summary>
    [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
    public string? Status { get; set; } = "Pending";

    /// <summary>
    /// Indicates the priority of the task (e.g., "Low", "Medium", "High")
    /// </summary>
    [StringLength(20, ErrorMessage = "Priority cannot exceed 20 characters")]
    public string? Priority { get; set; } = "Medium";
}
