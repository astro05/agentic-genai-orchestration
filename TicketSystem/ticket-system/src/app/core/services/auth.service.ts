import { Injectable, computed, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';
import {
  AuthResponse, CurrentUser, LoginRequest,
  RegisterRequest, UserRole
} from '../models/models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly TOKEN_KEY = 'ticket_token';
  private readonly USER_KEY  = 'ticket_user';
  private readonly apiUrl    = `${environment.apiUrl}/auth`;

  private readonly user = signal<CurrentUser | null>(this.loadUser());

  /** Use in templates so the shell reacts when login/logout updates session. */
  readonly sessionActive = computed(() => this.user() !== null);

  constructor(private http: HttpClient, private router: Router) {}

  get currentUser(): CurrentUser | null {
    return this.user();
  }

  isLoggedIn(): boolean {
    return this.user() !== null;
  }

  get role(): UserRole | null {
    return this.user()?.role ?? null;
  }

  get isAdmin():    boolean { return this.role === UserRole.Admin;    }
  get isAgent():    boolean { return this.role === UserRole.Agent;    }
  get isCustomer(): boolean { return this.role === UserRole.Customer; }

  register(req: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/register`, req).pipe(
      tap((res: AuthResponse) => this.storeSession(res))
    );
  }

  login(req: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, req).pipe(
      tap((res: AuthResponse) => this.storeSession(res))
    );
  }

  logout(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
    this.user.set(null);
    void this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  redirectByRole(): void {
    if (this.isAdmin)         { void this.router.navigate(['/admin']);    return; }
    if (this.isAgent)         { void this.router.navigate(['/agent']);    return; }
    if (this.isCustomer)      { void this.router.navigate(['/customer']); return; }
    void this.router.navigate(['/login']);
  }

  private storeSession(res: AuthResponse): void {
    const user: CurrentUser = {
      userId:   res.userId,
      fullName: res.fullName,
      email:    res.email,
      role:     res.role as UserRole,
      token:    res.token
    };
    localStorage.setItem(this.TOKEN_KEY, res.token);
    localStorage.setItem(this.USER_KEY,  JSON.stringify(user));
    this.user.set(user);
  }

  private loadUser(): CurrentUser | null {
    try {
      const raw = localStorage.getItem(this.USER_KEY);
      return raw ? JSON.parse(raw) as CurrentUser : null;
    } catch {
      return null;
    }
  }
}
