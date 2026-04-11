import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
  standalone: false
})
export class LoginComponent {
  form: FormGroup;
  loading = false;
  error   = '';
  showPwd = false;

  constructor(
    private fb: FormBuilder,
    private auth: AuthService,
    private router: Router
  ) {
    if (this.auth.isLoggedIn()) this.auth.redirectByRole();

    this.form = this.fb.group({
      email:    ['', [Validators.required, Validators.email]],
      password: ['', Validators.required]
    });
  }

  get f() { return this.form.controls; }

  submit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.loading = true;
    this.error   = '';

    this.auth.login(this.form.value).subscribe({
      next:  () => { this.loading = false; this.auth.redirectByRole(); },
      error: (err) => {
        this.loading = false;
        this.error = err.error?.message ?? 'Login failed. Please try again.';
      }
    });
  }

  fillDemo(role: 'admin' | 'agent' | 'customer'): void {
    const creds: Record<string, { email: string; password: string }> = {
      admin:    { email: 'admin@ticketsys.com',    password: 'Admin@123'    },
      agent:    { email: 'agent@ticketsys.com',    password: 'Agent@123'    },
      customer: { email: 'customer@ticketsys.com', password: 'Customer@123' }
    };
    this.form.patchValue(creds[role]);
  }
}