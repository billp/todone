export interface TodoItem {
  id: number;
  title: string;
  emoji: string | null;
  isCompleted: boolean;
  sortOrder: number;
  createdAt: string;
  userId: number;
}

export interface AuthResponse {
  token: string;
  username: string;
}

export interface ReorderItem {
  id: number;
  sortOrder: number;
  isCompleted: boolean;
}

export interface UpdateTodoPayload {
  title?: string;
  isCompleted?: boolean;
  sortOrder?: number;
  emoji?: string | null;
}
