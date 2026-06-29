export type TaskStatus = 0 | 1 | 2; // Todo=0, InProgress=1, Done=2
export type TaskPriority = 0 | 1 | 2; // Low=0, Medium=1, High=2

export interface TaskDto {
  id: string;
  title: string;
  description?: string;
  status: TaskStatus;
  priority: TaskPriority;
  dueDate?: string;
  isOverdue: boolean;
  userId: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateTaskRequest {
  title: string;
  description?: string;
  status: TaskStatus;
  priority: TaskPriority;
  dueDate?: string | null;
}

export interface UpdateTaskRequest {
  title: string;
  description?: string;
  status: TaskStatus;
  priority: TaskPriority;
  dueDate?: string | null;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export const STATUS_LABELS: Record<TaskStatus, string> = { 0: 'Todo', 1: 'In Progress', 2: 'Done' };
export const PRIORITY_LABELS: Record<TaskPriority, string> = { 0: 'Low', 1: 'Medium', 2: 'High' };
