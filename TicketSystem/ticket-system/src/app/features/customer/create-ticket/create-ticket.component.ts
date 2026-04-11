import { Component, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { TicketService } from '../../../core/services/ticket.service';
import { LiveRefreshService } from '../../../core/services/live-refresh.service';

@Component({
  selector: 'app-create-ticket',
  templateUrl: './create-ticket.component.html',
  styleUrls: ['./create-ticket.component.scss'],
  standalone: false
})
export class CreateTicketComponent {
  form: FormGroup;
  loading  = false;
  success  = false;
  error    = '';

  private readonly liveRefresh = inject(LiveRefreshService);

  constructor(
    private fb:            FormBuilder,
    private ticketService: TicketService,
    private router:        Router
  ) {
    this.form = this.fb.group({
      title:       ['', [Validators.required, Validators.minLength(5)]],
      description: ['', [Validators.required, Validators.minLength(20)]]
    });
  }

  get f() { return this.form.controls; }

  get charCount(): number { return this.form.value.description?.length ?? 0; }

  submit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.loading = true;
    this.error   = '';

    this.ticketService.createTicket(this.form.value).subscribe({
      next: () => {
        this.loading = false;
        this.success = true;
        this.liveRefresh.bump();
        setTimeout(() => this.router.navigate(['/customer']), 2000);
      },
      error: (err) => {
        this.loading = false;
        this.error = err.error?.message ?? 'Failed to create ticket.';
      }
    });
  }
}