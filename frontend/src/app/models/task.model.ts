export interface Task {
  id: number;
  title: string;
  description?: string;
  isCompleted: boolean;
  createdDate: string;
}

export interface CreateTaskDto {
  title: string;
  description?: string;
}

export interface UpdateTaskDto {
  title: string;
  description?: string;
  isCompleted: boolean;
}
