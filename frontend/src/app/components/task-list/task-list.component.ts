import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TaskService } from '../../services/task.service';
import {Task, TaskPriority} from '../../models/task.model';
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
        console.log('Tasks loaded successfully:', data);
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
      day: 'numeric'
    });
  }

  getPriorityClass(priority?: TaskPriority | null): string {
    if (!priority) {
      return 'bg-slate-200 text-slate-700';
    }

    switch (priority.toLowerCase()) {
      case 'high':
        return 'bg-rose-500 text-white';
      case 'medium':
        return 'bg-amber-400 text-slate-900';
      case 'low':
        return 'bg-emerald-500 text-white';
      default:
        return 'bg-slate-200 text-slate-700';
    }
  }

  // Toggle task completion status
  toggleTaskCompletion(task: Task): void {
    const updatedTask = {
      title: task.title,
      description: task.description,
      isCompleted: !task.isCompleted,
      dueDate: task.dueDate,
      priority: task.priority
    };

    this.taskService.updateTask(task.id, updatedTask).subscribe({
      next: () => {
        // Update the task in the local array
        task.isCompleted = !task.isCompleted;
      },
      error: (err) => {
        console.error('Error toggling task completion:', err);
        alert('Failed to update task status. Please try again.');
      }
    });
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
