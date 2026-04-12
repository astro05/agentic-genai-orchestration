import { Injectable } from '@angular/core';
import {
  CanActivate, Router, ActivatedRouteSnapshot
} from '@angular/router';
import { AuthService } from '../services/auth.service';

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