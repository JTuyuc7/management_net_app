import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TaskService } from '../../services/task.service';
import { Task } from '../../models/task.model';
import { TaskFormComponent } from '../task-form/task-form.component';

@Component({
  selector: 'app-task-list',
  standalone: true,
  imports: [CommonModule, TaskFormComponent],
  templateUrl: './task-list.component.html',
  styleUrls: ['./task-list.component.css']
})
export class TaskListComponent implements OnInit {
  tasks: Task[] = [];
  loading: boolean = true;
  error: string | null = null;
  editingTask: Task | null = null;

  constructor(private taskService: TaskService) { }

  ngOnInit(): void {
    this.loadTasks();
  }

  loadTasks(): void {
    this.loading = true;
    this.error = null;

    this.taskService.getTasks().subscribe({
      next: (data) => {
        this.tasks = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load tasks. Please make sure the backend is running.';
        this.loading = false;
        console.error('Error loading tasks:', err);
      }
    });
  }

  formatDate(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  // Toggle task completion status using the /complete endpoint
  toggleTaskCompletion(task: Task): void {
    if (task.isCompleted) {
      // If already completed, we need to use the update endpoint to mark as incomplete
      const updatedTask = {
        title: task.title,
        description: task.description,
        isCompleted: false
      };

      this.taskService.updateTask(task.id, updatedTask).subscribe({
        next: () => {
          task.isCompleted = false;
        },
        error: (err) => {
          console.error('Error toggling task completion:', err);
          alert('Failed to update task status. Please try again.');
        }
      });
    } else {
      // Use the dedicated /complete endpoint
      this.taskService.markTaskAsComplete(task.id).subscribe({
        next: () => {
          task.isCompleted = true;
        },
        error: (err) => {
          console.error('Error marking task as complete:', err);
          alert('Failed to mark task as complete. Please try again.');
        }
      });
    }
  }

  // Delete a task
  deleteTask(task: Task): void {
    if (confirm(`Are you sure you want to delete "${task.title}"?`)) {
      this.taskService.deleteTask(task.id).subscribe({
        next: () => {
          this.loadTasks(); // Refresh the list
        },
        error: (err) => {
          console.error('Error deleting task:', err);
          alert('Failed to delete task. Please try again.');
        }
      });
    }
  }

  // Start editing a task
  editTask(task: Task): void {
    this.editingTask = { ...task }; // Create a copy
  }

  // Cancel editing
  cancelEdit(): void {
    this.editingTask = null;
  }

  // Handle task updated
  onTaskUpdated(): void {
    this.editingTask = null;
    this.loadTasks();
  }
}
