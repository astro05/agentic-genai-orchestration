import { Component, OnInit } from '@angular/core';
import { TicketService } from '../../../core/services/ticket.service';
import { AuthService }   from '../../../core/services/auth.service';
import { TicketDto, STATUS_LABELS, CATEGORY_LABELS } from '../../../core/models/models';

@Component({
  selector: 'app-customer-dashboard',
  templateUrl: './customer-dashboard.component.html',
  styleUrls: ['./customer-dashboard.component.scss'],
  standalone: false
})
export class CustomerDashboardComponent implements OnInit {
  tickets: TicketDto[] = [];
  filtered: TicketDto[] = [];
  loading  = true;
  error    = '';
  filter   = 'All';
  search   = '';

  readonly STATUS_LABELS   = STATUS_LABELS;
  readonly CATEGORY_LABELS = CATEGORY_LABELS;
  readonly filters = ['All', 'Open', 'InProgress', 'Resolved'];

  constructor(
    private ticketService: TicketService,
    public  authService:   AuthService
  ) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading = true;
    this.ticketService.getMyTickets().subscribe({
      next:  t  => { this.tickets = t; this.applyFilter(); this.loading = false; },
      error: () => { this.error = 'Failed to load tickets.'; this.loading = false; }
    });
  }

  applyFilter(): void {
    let result = this.filter === 'All'
      ? [...this.tickets]
      : this.tickets.filter(t => t.status === this.filter);

    if (this.search.trim()) {
      const q = this.search.toLowerCase();
      result = result.filter(t =>
        t.title.toLowerCase().includes(q) ||
        t.description.toLowerCase().includes(q)
      );
    }
    this.filtered = result;
  }

  setFilter(f: string): void { this.filter = f; this.applyFilter(); }

  get stats() {
    return {
      total:      this.tickets.length,
      open:       this.tickets.filter(t => t.status === 'Open').length,
      inProgress: this.tickets.filter(t => t.status === 'InProgress').length,
      resolved:   this.tickets.filter(t => t.status === 'Resolved').length,
    };
  }

  badgeClass(status: string): string {
    const map: Record<string, string> = {
      Open: 'badge-open', InProgress: 'badge-inprogress', Resolved: 'badge-resolved'
    };
    return map[status] ?? '';
  }

  priorityClass(p: string): string {
    return `badge-${p.toLowerCase()}`;
  }

  categoryLabel(c: string): string {
    return this.CATEGORY_LABELS[c] ?? c;
  }
}