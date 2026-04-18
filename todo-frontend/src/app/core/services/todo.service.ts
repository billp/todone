import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { TodoItem, ReorderItem, UpdateTodoPayload } from '../models/todo.model';

@Injectable({ providedIn: 'root' })
export class TodoService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/todos`;

  getAll(): Observable<TodoItem[]> {
    return this.http.get<TodoItem[]>(this.apiUrl);
  }

  create(title: string, emoji: string | null): Observable<TodoItem> {
    return this.http.post<TodoItem>(this.apiUrl, { title, emoji });
  }

  update(id: number, changes: UpdateTodoPayload): Observable<TodoItem> {
    return this.http.patch<TodoItem>(`${this.apiUrl}/${id}`, changes);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  reorder(items: ReorderItem[]): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/reorder`, items);
  }
}
