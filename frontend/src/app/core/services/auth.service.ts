import { Injectable, computed, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs/operators';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthResponse, LoginDto, RegisterDto, CurrentUser, ApiResponse } from '../models/auth.models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly API = `${environment.apiUrl}/auth`;

  // Angular 17 signals-based state
  private _currentUser = signal<CurrentUser | null>(this.loadUserFromStorage());

  readonly currentUser  = this._currentUser.asReadonly();
  readonly isLoggedIn   = computed(() => !!this._currentUser());
  readonly isAdmin      = computed(() => this._currentUser()?.role === 'Admin');
  readonly isManager    = computed(() =>
    ['Admin', 'Manager'].includes(this._currentUser()?.role ?? ''));

  constructor(private http: HttpClient, private router: Router) {}

  register(dto: RegisterDto): Observable<ApiResponse<AuthResponse>> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.API}/register`, dto)
      .pipe(tap(res => { if (res.success) this.handleAuth(res.data); }));
  }

  login(dto: LoginDto): Observable<ApiResponse<AuthResponse>> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.API}/login`, dto)
      .pipe(tap(res => { if (res.success) this.handleAuth(res.data); }));
  }

  refreshToken(): Observable<ApiResponse<AuthResponse>> {
    const refreshToken = localStorage.getItem('refreshToken') ?? '';
    return this.http.post<ApiResponse<AuthResponse>>(`${this.API}/refresh`, { refreshToken })
      .pipe(tap(res => { if (res.success) this.handleAuth(res.data); }));
  }

  logout(): void {
    this.http.post(`${this.API}/logout`, {}).subscribe();
    this.clearAuth();
    this.router.navigate(['/auth/login']);
  }

  getAccessToken(): string | null {
    return localStorage.getItem('accessToken');
  }

  // ── private ────────────────────────────────────────────────────────────────

  private handleAuth(data: AuthResponse): void {
    localStorage.setItem('accessToken',  data.accessToken);
    localStorage.setItem('refreshToken', data.refreshToken);

    const user = this.parseUser(data.accessToken);
    localStorage.setItem('currentUser', JSON.stringify(user));
    this._currentUser.set(user);
  }

  private clearAuth(): void {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('currentUser');
    this._currentUser.set(null);
  }

  private loadUserFromStorage(): CurrentUser | null {
    try {
      const stored = localStorage.getItem('currentUser');
      return stored ? JSON.parse(stored) : null;
    } catch { return null; }
  }

  private parseUser(token: string): CurrentUser {
    const payload = JSON.parse(atob(token.split('.')[1]));
    const name    = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'];
    return {
      id:       payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'],
      name,
      email:    payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'],
      role:     payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'],
      initials: name.split(' ').slice(0, 2).map((w: string) => w[0].toUpperCase()).join('')
    };
  }
}
