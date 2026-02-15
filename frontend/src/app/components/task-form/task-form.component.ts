import { Component, EventEmitter, Output, Input, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { TaskService } from '../../services/task.service';
import { CreateTaskDto, Task } from '../../models/task.model';

@Component({
  selector: 'app-task-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './task-form.component.html',
  styleUrls: ['./task-form.component.css']
})
export class TaskFormComponent implements OnChanges {
  @Input() editingTask: Task | null = null;
  @Output() taskCreated = new EventEmitter<void>();
  @Output() taskUpdated = new EventEmitter<void>();
  @Output() editCancelled = new EventEmitter<void>();

  taskForm: FormGroup;
  isSubmitting = false;
  showForm = false;
  errorMessage: string | null = null;
  successMessage: string | null = null;
  isEditMode = false;

  constructor(
    private fb: FormBuilder,
    private taskService: TaskService
  ) {
    this.taskForm = this.fb.group({
      title: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(100)]],
      description: ['', [Validators.maxLength(500)]]
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['editingTask'] && this.editingTask) {
      this.isEditMode = true;
      this.showForm = true;
      this.populateForm(this.editingTask);
    } else if (changes['editingTask'] && !this.editingTask) {
      this.isEditMode = false;
      this.resetForm();
    }
  }

  populateForm(task: Task): void {
    this.taskForm.patchValue({
      title: task.title,
      description: task.description || ''
    });
  }

  setFormVisible(isVisible: boolean): void {
    this.showForm = isVisible;
    if (!this.showForm) {
      this.resetForm();
    }
  }

  toggleForm(): void {
    if (this.isEditMode) {
      this.cancelEdit();
    } else {
      this.showForm = !this.showForm;
      if (!this.showForm) {
        this.resetForm();
      }
    }
  }

  cancelEdit(): void {
    this.isEditMode = false;
    this.showForm = false;
    this.resetForm();
    this.editCancelled.emit();
  }

  onSubmit(): void {
    if (this.taskForm.invalid) {
      this.markFormGroupTouched(this.taskForm);
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = null;
    this.successMessage = null;

    const formValue = this.taskForm.value;

    if (this.isEditMode && this.editingTask) {
      // Update existing task
      const updatedTask = {
        title: formValue.title,
        description: formValue.description || '',
        isCompleted: this.editingTask.isCompleted
      };

      this.taskService.updateTask(this.editingTask.id, updatedTask).subscribe({
        next: () => {
          this.successMessage = 'Task updated successfully!';
          this.resetForm();
          this.taskUpdated.emit();

          setTimeout(() => {
            this.successMessage = null;
            this.showForm = false;
            this.isEditMode = false;
          }, 2000);
        },
        error: (err) => {
          this.errorMessage = 'Failed to update task. Please try again.';
          this.isSubmitting = false;
          console.error('Error updating task:', err);
        }
      });
    } else {
      // Create new task
      const newTask: CreateTaskDto = {
        title: formValue.title,
        description: formValue.description || undefined
      };

      this.taskService.createTask(newTask).subscribe({
        next: () => {
          this.successMessage = 'Task created successfully!';
          this.resetForm();
          this.taskCreated.emit();

          setTimeout(() => {
            this.successMessage = null;
            this.showForm = false;
          }, 2000);
        },
        error: (err) => {
          this.errorMessage = 'Failed to create task. Please try again.';
          this.isSubmitting = false;
          console.error('Error creating task:', err);
        }
      });
    }
  }

  resetForm(): void {
    this.taskForm.reset();
    this.isSubmitting = false;
    this.errorMessage = null;
  }

  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      control?.markAsTouched();
    });
  }

  // Helper methods for template
  isFieldInvalid(fieldName: string): boolean {
    const field = this.taskForm.get(fieldName);
    return !!(field && field.invalid && field.touched);
  }

  getFieldError(fieldName: string): string {
    const field = this.taskForm.get(fieldName);
    if (field?.errors) {
      if (field.errors['required']) return `${fieldName} is required`;
      if (field.errors['minlength']) return `${fieldName} is too short`;
      if (field.errors['maxlength']) return `${fieldName} is too long`;
    }
    return '';
  }
}
