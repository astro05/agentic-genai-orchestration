import { Component, signal } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { finalize } from 'rxjs/operators';
import { AuthService } from '../../../core/services/auth.service';
import { readApiErrorMessage } from '../../../core/utils/http-error.util';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss'],
  standalone: false
})
export class RegisterComponent {
  form: FormGroup;
  readonly loading = signal(false);
  readonly error = signal('');
  showPwd = false;

  constructor(
    private fb: FormBuilder,
    private auth: AuthService
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
    this.loading.set(true);
    this.error.set('');

    this.auth.register(this.form.value).pipe(
      finalize(() => this.loading.set(false))
    ).subscribe({
      next:  () => { this.auth.redirectByRole(); },
      error: (err: unknown) => {
        this.error.set(readApiErrorMessage(err, 'Registration failed. Please try again.'));
      }
    });
  }
}
