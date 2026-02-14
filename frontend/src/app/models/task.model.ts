export enum TaskPriority {
  Low = 'Low',
  Medium = 'Medium',
  High = 'High'
}

export interface Task {
  id: number;
  title: string;
  description: string;
  isCompleted: boolean;
  dueDate: string;
  priority: TaskPriority;
  createdDate: string;
}

export interface CreateTaskDto {
  title: string;
  description: string;
  dueDate: string;
  priority: TaskPriority;
}

export interface UpdateTaskDto {
  title: string;
  description: string;
  isCompleted: boolean;
  dueDate: string;
  priority: TaskPriority;
}
