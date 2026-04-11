import { Component, DestroyRef, inject, OnInit } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AdminService }  from '../../../core/services/admin.service';
import { AuthService }   from '../../../core/services/auth.service';
import { LiveRefreshService } from '../../../core/services/live-refresh.service';
import {
  TicketDto, UserDto, STATUS_LABELS, CATEGORY_LABELS
} from '../../../core/models/models';

@Component({
  selector: 'app-admin-dashboard',
  templateUrl: './admin-dashboard.component.html',
  styleUrls: ['./admin-dashboard.component.scss'],
  standalone: false
})
export class AdminDashboardComponent implements OnInit {
  activeTab = 'tickets';

  // ── Tickets ──────────────────────────────────────────────────
  tickets:        TicketDto[] = [];
  filteredTickets: TicketDto[] = [];
  agents:         UserDto[]   = [];
  ticketSearch   = '';
  ticketFilter   = 'All';
  loadingTickets = true;
  assigningId    = '';
  selectedAgent: Record<string, string> = {};
  ticketMsg = '';

  // ── Users ────────────────────────────────────────────────────
  users:         UserDto[] = [];
  filteredUsers: UserDto[] = [];
  userSearch    = '';
  loadingUsers  = true;
  deletingId    = '';
  userMsg       = '';

  // ── New user modal ───────────────────────────────────────────
  showModal    = false;
  newUser      = { fullName: '', email: '', password: '', role: 1 };
  creatingUser = false;
  modalError   = '';

  readonly STATUS_LABELS   = STATUS_LABELS;
  readonly CATEGORY_LABELS = CATEGORY_LABELS;
  readonly ticketFilters   = ['All', 'Open', 'InProgress', 'Resolved'];

  readonly roleOptions = [
    { label: 'Admin',    value: 0 },
    { label: 'Agent',    value: 1 },
    { label: 'Customer', value: 2 }
  ];

  private readonly adminService = inject(AdminService);
  public  readonly authService  = inject(AuthService);
  private readonly liveRefresh  = inject(LiveRefreshService);
  private readonly destroyRef   = inject(DestroyRef);

  ngOnInit(): void {
    this.loadTickets(false);
    this.loadUsers(false);
    this.loadAgents();
    this.liveRefresh.ticketRefresh$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        this.loadTickets(true);
        this.loadUsers(true);
        this.loadAgents();
      });
  }

  // ── Tickets ──────────────────────────────────────────────────
  loadTickets(silent = false): void {
    if (!silent) this.loadingTickets = true;
    this.adminService.getAllTickets().subscribe({
      next:  t  => { this.tickets = t; this.applyTicketFilter(); this.loadingTickets = false; },
      error: () => this.loadingTickets = false
    });
  }

  loadAgents(): void {
    this.adminService.getAgents().subscribe({
      next: a => { this.agents = a; }
    });
  }

  applyTicketFilter(): void {
    let r = this.ticketFilter === 'All'
      ? [...this.tickets]
      : this.tickets.filter(t => t.status === this.ticketFilter);
    if (this.ticketSearch.trim()) {
      const q = this.ticketSearch.toLowerCase();
      r = r.filter(t =>
        t.title.toLowerCase().includes(q) ||
        t.createdByName.toLowerCase().includes(q) ||
        (t.assignedToName ?? '').toLowerCase().includes(q)
      );
    }
    this.filteredTickets = r;
  }

  setTicketFilter(f: string): void { this.ticketFilter = f; this.applyTicketFilter(); }

  assignTicket(ticketId: string): void {
    const agentId = this.selectedAgent[ticketId];
    if (!agentId) return;
    this.assigningId = ticketId;
    this.adminService.assignTicket(ticketId, { agentId }).subscribe({
      next: () => {
        this.assigningId = '';
        this.ticketMsg = 'Ticket assigned successfully.';
        this.loadTickets(true);
        setTimeout(() => this.ticketMsg = '', 3000);
      },
      error: () => { this.assigningId = ''; }
    });
  }

  // ── Users ────────────────────────────────────────────────────
  loadUsers(silent = false): void {
    if (!silent) this.loadingUsers = true;
    this.adminService.getAllUsers().subscribe({
      next:  u  => { this.users = u; this.applyUserFilter(); this.loadingUsers = false; },
      error: () => this.loadingUsers = false
    });
  }

  applyUserFilter(): void {
    if (!this.userSearch.trim()) { this.filteredUsers = [...this.users]; return; }
    const q = this.userSearch.toLowerCase();
    this.filteredUsers = this.users.filter(u =>
      u.fullName.toLowerCase().includes(q) || u.email.toLowerCase().includes(q)
    );
  }

  updateRole(userId: string, event: Event): void {
    const val = +(event.target as HTMLSelectElement).value;
    this.adminService.updateRole({ userId, newRole: val }).subscribe({
      next: () => {
        this.userMsg = 'Role updated.';
        this.loadUsers(true);
        setTimeout(() => this.userMsg = '', 2500);
      }
    });
  }

  deleteUser(id: string): void {
    if (!confirm('Deactivate this user?')) return;
    this.deletingId = id;
    this.adminService.deleteUser(id).subscribe({
      next: () => {
        this.deletingId = '';
        this.userMsg = 'User deactivated.';
        this.loadUsers(true);
        setTimeout(() => this.userMsg = '', 2500);
      },
      error: () => this.deletingId = ''
    });
  }

  openModal(): void {
    this.showModal  = true;
    this.modalError = '';
    this.newUser    = { fullName: '', email: '', password: '', role: 1 };
  }

  createUser(): void {
    if (!this.newUser.fullName || !this.newUser.email || !this.newUser.password) {
      this.modalError = 'All fields are required.'; return;
    }
    this.creatingUser = true;
    this.adminService.createUser(this.newUser as any).subscribe({
      next: () => {
        this.creatingUser = false;
        this.showModal    = false;
        this.userMsg      = 'User created successfully.';
        this.loadUsers(true);
        this.loadAgents();
        setTimeout(() => this.userMsg = '', 3000);
      },
      error: (err) => {
        this.creatingUser = false;
        this.modalError   = err.error?.message ?? 'Failed to create user.';
      }
    });
  }

  // ── Helpers ──────────────────────────────────────────────────
  get ticketStats() {
    return {
      total:      this.tickets.length,
      open:       this.tickets.filter(t => t.status === 'Open').length,
      inProgress: this.tickets.filter(t => t.status === 'InProgress').length,
      resolved:   this.tickets.filter(t => t.status === 'Resolved').length,
      unassigned: this.tickets.filter(t => !t.assignedToName).length,
    };
  }

  badgeClass(status: string): string {
    const m: Record<string, string> = { Open: 'badge-open', InProgress: 'badge-inprogress', Resolved: 'badge-resolved' };
    return m[status] ?? '';
  }
  priorityClass(p: string): string { return `badge-${p.toLowerCase()}`; }
  categoryLabel(c: string): string { return this.CATEGORY_LABELS[c] ?? c; }
  roleClass(r: string): string {
    const m: Record<string, string> = { Admin: 'role-admin', Agent: 'role-agent', Customer: 'role-customer' };
    return m[r] ?? '';
  }
}
