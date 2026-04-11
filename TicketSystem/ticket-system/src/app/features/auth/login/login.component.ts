import { Component, signal } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { finalize } from 'rxjs/operators';
import { AuthService } from '../../../core/services/auth.service';
import { isDeactivatedUserError, readApiErrorMessage } from '../../../core/utils/http-error.util';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
  standalone: false
})
export class LoginComponent {
  form: FormGroup;
  readonly loading = signal(false);
  readonly error = signal('');
  showPwd = false;
  readonly showDeactivatedPopup = signal(false);

  constructor(
    private fb: FormBuilder,
    private auth: AuthService
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
    this.loading.set(true);
    this.error.set('');
    this.showDeactivatedPopup.set(false);

    this.auth.login(this.form.value).pipe(
      finalize(() => this.loading.set(false))
    ).subscribe({
      next: () => { this.auth.redirectByRole(); },
      error: (err: unknown) => {
        if (isDeactivatedUserError(err)) {
          this.showDeactivatedPopup.set(true);
        } else {
          this.error.set(readApiErrorMessage(err, 'Login failed. Please try again.'));
        }
      }
    });
  }

  closePopup(): void {
    this.showDeactivatedPopup.set(false);
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
