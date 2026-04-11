import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss'],
  standalone: false
})
export class RegisterComponent {
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
      fullName: ['', [Validators.required, Validators.minLength(2)]],
      email:    ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      role:     [2, Validators.required]   // 2 = Customer, 1 = Agent
    });
  }

  get f() { return this.form.controls; }

  submit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.loading = true;
    this.error   = '';

    this.auth.register(this.form.value).subscribe({
      next:  () => { this.loading = false; this.auth.redirectByRole(); },
      error: (err) => {
        this.loading = false;
        this.error = err.error?.message ?? 'Registration failed. Please try again.';
      }
    });
  }
}