import { Component } from '@angular/core';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-navbar',
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.scss'],
  standalone: false
})
export class NavbarComponent {
  constructor(public authService: AuthService) {}

  get dashboardRoute(): string {
    if (this.authService.isAdmin)    return '/admin';
    if (this.authService.isAgent)    return '/agent';
    return '/customer';
  }
}