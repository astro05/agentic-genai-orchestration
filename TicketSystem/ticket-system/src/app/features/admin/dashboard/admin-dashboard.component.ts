import { Component, DestroyRef, inject, OnInit } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { forkJoin, of } from 'rxjs';
import { catchError, finalize } from 'rxjs/operators';
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
  loadingTickets = false;
  assigningId    = '';
  selectedAgent: Record<string, string> = {};
  ticketMsg = '';
  selectedTicket: TicketDto | null = null;

  // ── Users ────────────────────────────────────────────────────
  users:         UserDto[] = [];
  filteredUsers: UserDto[] = [];
  userSearch    = '';
  loadingUsers  = false;
  userActionBusyId = '';
  userMsg       = '';
  // tracks the pending dropdown selection per user (separate from committed role)
  pendingRole: Record<string, string> = {};

  // ── New user modal ───────────────────────────────────────────
  showModal    = false;
  newUser      = { fullName: '', email: '', password: '', role: 1 };
  creatingUser = false;
  modalError   = '';

  /** In-app confirm dialogs (Deactivate / permanent delete) */
  confirmDeactivateUserId: string | null = null;
  confirmDeleteUserId: string | null = null;

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
    this.loadInitialDashboard();
    this.liveRefresh.ticketRefresh$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        this.loadTickets(true);
        this.loadUsers(true);
        this.loadAgents();
        
      });

  }

  /** Single initial fetch so loading flags always clear together after tickets, users, and agents are ready. */
  private loadInitialDashboard(): void {
    this.loadingTickets = true;
    this.loadingUsers = true;
    forkJoin({
      tickets: this.adminService.getAllTickets().pipe(catchError(() => of([] as TicketDto[]))),
      users: this.adminService.getAllUsers().pipe(catchError(() => of([] as UserDto[]))),
      agents: this.adminService.getAgents().pipe(catchError(() => of([] as UserDto[])))
    }).pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => {
        this.loadingTickets = false;
        this.loadingUsers = false;
      })
    ).subscribe(({ tickets, users, agents }) => {
      this.tickets = tickets ?? [];
      this.applyTicketFilter();
      this.users = users ?? [];
      this.users.forEach(user => {
        if (!(user.id in this.pendingRole)) {
          this.pendingRole[user.id] = '';
        }
      });
      this.applyUserFilter();
      this.agents = agents ?? [];
      if (this.selectedTicket) {
        const cur = this.tickets.find(x => x.id === this.selectedTicket!.id);
        if (cur) this.selectedTicket = { ...cur };
      }
      this.loadingTickets = false;
      this.loadingUsers = false;
    });
  }

  // ── Tickets ──────────────────────────────────────────────────
  loadTickets(silent = false): void {
    if (!silent) this.loadingTickets = false;
    this.adminService.getAllTickets().pipe(
      catchError(() => of([] as TicketDto[])),
      finalize(() => { this.loadingTickets = false; })
    ).subscribe(t => {
      this.tickets = t ?? [];
      this.applyTicketFilter();
      if (this.selectedTicket) {
        const cur = this.tickets.find(x => x.id === this.selectedTicket!.id);
        if (cur) this.selectedTicket = { ...cur };
      }
      this.loadingTickets = false;
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

  openTicketDetail(t: TicketDto): void {
    this.selectedTicket = { ...t };
    const match = this.agents.find(a => a.fullName === t.assignedToName);
    this.selectedAgent[t.id] = match?.id ?? this.selectedAgent[t.id] ?? '';
  }

  closeTicketDetail(): void {
    this.selectedTicket = null;
  }

  assignTicket(ticketId: string, agentId: string): void {
    if (!agentId) return;
    const agent = this.agents.find(a => a.id === agentId);
    const agentName = agent?.fullName ?? 'agent';

    // ── Optimistic update: reflect assignment immediately ──
    this.selectedAgent[ticketId] = agentId;
    const updateInList = (list: TicketDto[]) => {
      const idx = list.findIndex(t => t.id === ticketId);
      if (idx !== -1) list[idx] = { ...list[idx], assignedToName: agentName,  status: 'InProgress' };
    };
    updateInList(this.tickets);
    updateInList(this.filteredTickets);
    if (this.selectedTicket?.id === ticketId) {
      this.selectedTicket = { ...this.selectedTicket, assignedToName: agentName, status: 'InProgress' };
    }

    this.assigningId = ticketId;
    this.adminService.assignTicket(ticketId, { agentId }).pipe(
      finalize(() => { this.assigningId = ''; })
    ).subscribe({
      next: () => {
        this.ticketMsg = `Ticket successfully assigned to ${agentName}.`;
        // Silent refresh keeps UI in sync with server without toggling the main table spinner
        this.loadTickets(true);
        this.loadAgents();
        setTimeout(() => this.ticketMsg = '', 5000);
      },
      error: () => {
        this.loadTickets(true);
        this.selectedAgent[ticketId] = '';
        this.ticketMsg = 'Assignment failed. Please try another agent or refresh the list.';
        setTimeout(() => this.ticketMsg = '', 5000);
      }
    });
  }

  // ── Users ────────────────────────────────────────────────────
  loadUsers(silent = false): void {
    if (!silent) this.loadingUsers = true;
    this.adminService.getAllUsers().pipe(
      catchError(() => of([] as UserDto[])),
      finalize(() => { this.loadingUsers = false;   this.userActionBusyId = ''; })
    ).subscribe(u => {
      this.users = u ?? [];
      this.users.forEach(user => {
        if (!(user.id in this.pendingRole)) {
          this.pendingRole[user.id] = '';
        }
      });
      this.applyUserFilter();
    });
  }

  applyUserFilter(): void {
    if (!this.userSearch.trim()) { this.filteredUsers = [...this.users]; return; }
    const q = this.userSearch.toLowerCase();
    this.filteredUsers = this.users.filter(u =>
      u.fullName.toLowerCase().includes(q) || u.email.toLowerCase().includes(q)
    );
  }

  updateRole(userId: string, newRoleValue: number): void {
    const roleNames: Record<number, string> = { 0: 'Admin', 1: 'Agent', 2: 'Customer' };
    const newRoleName = roleNames[newRoleValue];
    if (!newRoleName) return;

    // ── Optimistic update: reflect the change immediately in the table ──
    const updateInList = (list: UserDto[]) => {
      const idx = list.findIndex(u => u.id === userId);
      if (idx !== -1) list[idx] = { ...list[idx], role: newRoleName };
    };
    updateInList(this.users);
    updateInList(this.filteredUsers);

    // Reset the dropdown back to placeholder after commit
    this.pendingRole[userId] = '';

    this.adminService.updateRole({ userId, newRole: newRoleValue }).subscribe({
      next: () => {
        this.userMsg = `Role changed to ${newRoleName}.`;
        this.loadAgents();
        setTimeout(() => this.userMsg = '', 2500);
      },
      error: () => {
        // Rollback on failure — reload from server
        this.userMsg = 'Could not update role. Change reverted.';
        this.loadUsers(true);
        setTimeout(() => this.userMsg = '', 3000);
      }
    });
  }

  openDeactivateConfirm(id: string): void {
    this.confirmDeactivateUserId = id;
  }

  cancelDeactivateConfirm(): void {
    this.confirmDeactivateUserId = null;
  }

  confirmDeactivateUser(): void {
    const id = this.confirmDeactivateUserId;
    if (!id) return;
    this.confirmDeactivateUserId = null;

    const updateInactive = (list: UserDto[]) => {
      const idx = list.findIndex(u => u.id === id);
      if (idx !== -1) list[idx] = { ...list[idx], isActive: false };
    };
    updateInactive(this.users);
    updateInactive(this.filteredUsers);

    this.userActionBusyId = '';
    this.adminService.deleteUser(id).pipe(finalize(() => { this.userActionBusyId = ''; })).subscribe({
      next: () => {
        this.userMsg = 'User deactivated.';
        this.loadUsers(true);
        this.loadAgents();
        setTimeout(() => this.userMsg = '', 2500);
      },
      error: () => {
        this.userMsg = 'Could not deactivate user.';
        this.loadUsers(true);
        setTimeout(() => this.userMsg = '', 3000);
      }
    });
  }

  activateUser(id: string): void {
    this.userActionBusyId = '';
    this.adminService.activateUser(id).pipe(finalize(() => { this.userActionBusyId = ''; })).subscribe({
      next: () => {
        this.userMsg = 'User activated.';
        this.loadUsers(true);
        this.loadAgents();
        setTimeout(() => this.userMsg = '', 2500);
      },
      error: () => { this.userMsg = 'Could not activate user.'; setTimeout(() => this.userMsg = '', 3000); }
    });
  }

  openDeleteConfirm(id: string): void {
    this.confirmDeleteUserId = id;
  }

  cancelDeleteConfirm(): void {
    this.confirmDeleteUserId = null;
  }

  confirmPermanentlyDeleteUser(): void {
    const id = this.confirmDeleteUserId;
    if (!id) return;
    this.confirmDeleteUserId = null;

    const removeFrom = (list: UserDto[]) => {
      const idx = list.findIndex(u => u.id === id);
      if (idx !== -1) list.splice(idx, 1);
    };
    removeFrom(this.users);
    removeFrom(this.filteredUsers);
    delete this.pendingRole[id];

    this.userActionBusyId = '';
    this.adminService.permanentlyDeleteUser(id).pipe(finalize(() => { this.userActionBusyId = ''; })).subscribe({
      next: () => {
        this.userMsg = 'User permanently deleted.';
        this.loadUsers(true);
        this.loadAgents();
        setTimeout(() => this.userMsg = '', 2500);
      },
      error: (err) => {
        this.userMsg = err.error?.message ?? 'Could not delete user.';
        this.loadUsers(true);
        setTimeout(() => this.userMsg = '', 4000);
      }
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
    this.creatingUser = false;
    const payload = {
      fullName: this.newUser.fullName.trim(),
      email: this.newUser.email.trim(),
      password: this.newUser.password,
      role: Number(this.newUser.role)
    };
    this.adminService.createUser(payload).pipe(
      finalize(() => { this.creatingUser = false; })
    ).subscribe({
      next: () => {
        this.showModal = false;
        this.modalError = '';
        this.userMsg = 'User created successfully.';
        this.userSearch = '';
        this.loadUsers(true);
        this.loadAgents();
        setTimeout(() => this.userMsg = '', 3000);
      },
      error: (err) => {
        this.modalError = err.error?.message ?? 'Failed to create user.';
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