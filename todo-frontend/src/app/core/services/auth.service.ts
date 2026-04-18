import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';
import { AuthResponse } from '../models/todo.model';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);

  private apiUrl = `${environment.apiUrl}/auth`;
  isLoggedIn$ = new BehaviorSubject<boolean>(this.hasToken());

  register(username: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/register`, { username, password })
      .pipe(tap(res => this.storeAuth(res)));
  }

  login(username: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, { username, password })
      .pipe(tap(res => this.storeAuth(res)));
  }

  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('username');
    this.isLoggedIn$.next(false);
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  getUsername(): string | null {
    return localStorage.getItem('username');
  }

  private hasToken(): boolean {
    return !!localStorage.getItem('token');
  }

  private storeAuth(res: AuthResponse): void {
    localStorage.setItem('token', res.token);
    localStorage.setItem('username', res.username);
    this.isLoggedIn$.next(true);
  }
}
