import { Injectable } from '@angular/core';
import {
  CanActivate, Router, ActivatedRouteSnapshot
} from '@angular/router';
import { AuthService } from '../services/auth.service';
import { UserRole } from '../models/models';

// ── Auth Guard: must be logged in ─────────────────────────────
@Injectable({ providedIn: 'root' })
export class AuthGuard implements CanActivate {
  constructor(private auth: AuthService, private router: Router) {}

  canActivate(): boolean {
    if (this.auth.isLoggedIn()) return true;
    this.router.navigate(['/login']);
    return false;
  }
}

// ── Role Guard: must have correct role ────────────────────────
@Injectable({ providedIn: 'root' })
export class RoleGuard implements CanActivate {
  constructor(private auth: AuthService, private router: Router) {}

  canActivate(route: ActivatedRouteSnapshot): boolean {
    const expectedRoles: UserRole[] = route.data['roles'];
    const userRole = this.auth.role;

    if (userRole && expectedRoles.includes(userRole)) return true;

    // Redirect to their own dashboard
    this.auth.redirectByRole();
    return false;
  }
}