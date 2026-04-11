import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
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

  private currentUserSubject = new BehaviorSubject<CurrentUser | null>(this.loadUser());
  currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient, private router: Router) {}

  get currentUser(): CurrentUser | null {
    return this.currentUserSubject.value;
  }

  isLoggedIn(): boolean {
    return !!this.currentUser;
  }

  get role(): UserRole | null {
    return this.currentUser?.role ?? null;
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
    this.currentUserSubject.next(null);
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  redirectByRole(): void {
    if (this.isAdmin)         this.router.navigate(['/admin']);
    else if (this.isAgent)    this.router.navigate(['/agent']);
    else if (this.isCustomer) this.router.navigate(['/customer']);
    else                      this.router.navigate(['/login']);
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
    this.currentUserSubject.next(user);
  }

  private loadUser(): CurrentUser | null {
    try {
      const raw = localStorage.getItem(this.USER_KEY);
      return raw ? JSON.parse(raw) : null;
    } catch { 
      return null; 
    }
  }
}