using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

/// <summary>
/// Data Transfer Object for updating an existing task
/// </summary>
public class UpdateTaskDto
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 200 characters")]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }

    public bool IsCompleted { get; set; }

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
