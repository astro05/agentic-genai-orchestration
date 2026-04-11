import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  computed,
  inject,
  OnInit,
  signal
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { forkJoin, of } from 'rxjs';
import { catchError, finalize } from 'rxjs/operators';
import { AdminService } from '../../../core/services/admin.service';
import { AuthService } from '../../../core/services/auth.service';
import { LiveRefreshService } from '../../../core/services/live-refresh.service';
import {
  TicketDto,
  TicketMessageDto,
  UserDto,
  STATUS_LABELS,
  CATEGORY_LABELS
} from '../../../core/models/models';
import { TicketService } from '../../../core/services/ticket.service';

@Component({
  selector: 'app-admin-dashboard',
  templateUrl: './admin-dashboard.component.html',
  styleUrls: ['./admin-dashboard.component.scss'],
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AdminDashboardComponent implements OnInit {
  readonly activeTab = signal<'tickets' | 'users'>('tickets');

  readonly tickets = signal<TicketDto[]>([]);
  readonly agents = signal<UserDto[]>([]);
  readonly ticketSearch = signal('');
  readonly ticketFilter = signal('All');
  readonly loadingTickets = signal(false);
  readonly assigningId = signal('');
  readonly selectedAgent = signal<Record<string, string>>({});
  readonly ticketMsg = signal('');
  readonly selectedTicket = signal<TicketDto | null>(null);
  readonly messageDraft = signal('');
  readonly replyToMessageId = signal<string | null>(null);
  readonly postingMessage = signal(false);

  readonly users = signal<UserDto[]>([]);
  readonly userSearch = signal('');
  readonly loadingUsers = signal(false);
  readonly userActionBusyId = signal('');
  readonly userMsg = signal('');
  readonly pendingRole = signal<Record<string, string>>({});

  readonly showModal = signal(false);
  readonly newUser = signal({
    fullName: '',
    email: '',
    password: '',
    role: 1
  });
  readonly creatingUser = signal(false);
  readonly modalError = signal('');

  readonly confirmDeactivateUserId = signal<string | null>(null);
  readonly confirmDeleteUserId = signal<string | null>(null);

  readonly STATUS_LABELS = STATUS_LABELS;
  readonly CATEGORY_LABELS = CATEGORY_LABELS;
  readonly ticketFilters = ['All', 'Open', 'InProgress', 'Resolved'];

  readonly roleOptions = [
    { label: 'Admin', value: 0 },
    { label: 'Agent', value: 1 },
    { label: 'Customer', value: 2 }
  ];

  readonly filteredTickets = computed(() => {
    const list = this.tickets();
    let r =
      this.ticketFilter() === 'All'
        ? [...list]
        : list.filter(t => t.status === this.ticketFilter());
    const q = this.ticketSearch().trim().toLowerCase();
    if (q) {
      r = r.filter(
        t =>
          t.title.toLowerCase().includes(q) ||
          t.createdByName.toLowerCase().includes(q) ||
          (t.assignedToName ?? '').toLowerCase().includes(q)
      );
    }
    return r;
  });

  readonly filteredUsers = computed(() => {
    const list = this.users();
    const search = this.userSearch().trim();
    if (!search) return [...list];
    const q = search.toLowerCase();
    return list.filter(
      u =>
        u.fullName.toLowerCase().includes(q) ||
        u.email.toLowerCase().includes(q)
    );
  });

  readonly ticketStats = computed(() => {
    const t = this.tickets();
    return {
      total: t.length,
      open: t.filter(x => x.status === 'Open').length,
      inProgress: t.filter(x => x.status === 'InProgress').length,
      resolved: t.filter(x => x.status === 'Resolved').length,
      unassigned: t.filter(x => !x.assignedToName).length
    };
  });

  private readonly adminService = inject(AdminService);
  private readonly ticketService = inject(TicketService);
  public readonly authService = inject(AuthService);
  private readonly liveRefresh = inject(LiveRefreshService);
  private readonly destroyRef = inject(DestroyRef);

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

  private loadInitialDashboard(): void {
    this.loadingTickets.set(true);
    this.loadingUsers.set(true);
    forkJoin({
      tickets: this.adminService
        .getAllTickets()
        .pipe(catchError(() => of([] as TicketDto[]))),
      users: this.adminService
        .getAllUsers()
        .pipe(catchError(() => of([] as UserDto[]))),
      agents: this.adminService
        .getAgents()
        .pipe(catchError(() => of([] as UserDto[])))
    })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.loadingTickets.set(false);
          this.loadingUsers.set(false);
        })
      )
      .subscribe(({ tickets, users, agents }) => {
        this.tickets.set(tickets ?? []);
        this.users.set(users ?? []);
        this.pendingRole.update(m => {
          const next = { ...m };
          (users ?? []).forEach(user => {
            if (!(user.id in next)) next[user.id] = '';
          });
          return next;
        });
        this.agents.set(agents ?? []);
        const sel = this.selectedTicket();
        if (sel) {
          const cur = this.tickets().find(x => x.id === sel.id);
          if (cur) this.selectedTicket.set({ ...cur });
        }
      });
  }

  loadTickets(silent = false): void {
    if (!silent) this.loadingTickets.set(true);
    this.adminService
      .getAllTickets()
      .pipe(
        catchError(() => of([] as TicketDto[])),
        finalize(() => this.loadingTickets.set(false))
      )
      .subscribe(t => {
        this.tickets.set(t ?? []);
        const sel = this.selectedTicket();
        if (sel) {
          const cur = this.tickets().find(x => x.id === sel.id);
          if (cur) this.selectedTicket.set({ ...cur });
        }
      });
  }

  loadAgents(): void {
    this.adminService.getAgents().subscribe({
      next: a => this.agents.set(a)
    });
  }

  setTicketFilter(f: string): void {
    this.ticketFilter.set(f);
  }

  onTicketSearchInput(v: string): void {
    this.ticketSearch.set(v);
  }

  openTicketDetail(t: TicketDto): void {
    this.selectedTicket.set({ ...t });
    this.messageDraft.set('');
    this.replyToMessageId.set(null);
    const match = this.agents().find(a => a.fullName === t.assignedToName);
    this.selectedAgent.update(m => ({
      ...m,
      [t.id]: match?.id ?? m[t.id] ?? ''
    }));
  }

  closeTicketDetail(): void {
    this.selectedTicket.set(null);
    this.messageDraft.set('');
    this.replyToMessageId.set(null);
    this.postingMessage.set(false);
  }

  setReplyTo(m: TicketMessageDto | null): void {
    this.replyToMessageId.set(m?.id ?? null);
  }

  replyPreview(messageId: string | null | undefined): string {
    if (!messageId) return '';
    const list = this.selectedTicket()?.messages ?? [];
    const msg = list.find(x => x.id === messageId);
    if (!msg) return '';
    const text = msg.body.trim();
    return text.length > 100 ? `${text.slice(0, 100)}…` : text;
  }

  sortedMessages(t: TicketDto): TicketMessageDto[] {
    const m = t.messages ?? [];
    return [...m].sort(
      (a, b) =>
        new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime()
    );
  }

  sendAdminMessage(): void {
    const ticket = this.selectedTicket();
    if (!ticket) return;
    const body = this.messageDraft().trim();
    if (!body) return;
    const replyTo = this.replyToMessageId();
    this.postingMessage.set(true);
    this.ticketService
      .addTicketMessage(ticket.id, {
        body,
        replyToMessageId: replyTo
      })
      .pipe(finalize(() => this.postingMessage.set(false)))
      .subscribe({
        next: msg => {
          const now = new Date().toISOString();
          this.tickets.update(arr =>
            arr.map(t =>
              t.id === ticket.id
                ? {
                    ...t,
                    messages: [...(t.messages ?? []), msg],
                    updatedAt: now
                  }
                : t
            )
          );
          const sel = this.selectedTicket();
          if (sel?.id === ticket.id) {
            const messages = [...(sel.messages ?? []), msg];
            this.selectedTicket.set({ ...sel, messages, updatedAt: now });
          }
          this.messageDraft.set('');
          this.replyToMessageId.set(null);
          this.ticketMsg.set('Message posted.');
          this.liveRefresh.bump();
          setTimeout(() => this.ticketMsg.set(''), 3000);
        },
        error: () => this.ticketMsg.set('Could not post message.')
      });
  }

  messageRoleClass(role: string): string {
    if (role === 'Agent') return 'msg-agent';
    if (role === 'Admin') return 'msg-admin';
    return 'msg-customer';
  }

  assignTicket(ticketId: string, agentId: string): void {
    if (!agentId) return;
    const agent = this.agents().find(a => a.id === agentId);
    const agentName = agent?.fullName ?? 'agent';

    this.selectedAgent.update(m => ({ ...m, [ticketId]: agentId }));
    this.tickets.update(arr => {
      const idx = arr.findIndex(t => t.id === ticketId);
      if (idx === -1) return arr;
      const copy = [...arr];
      copy[idx] = {
        ...copy[idx],
        assignedToName: agentName,
        status: 'InProgress'
      };
      return copy;
    });
    const sel = this.selectedTicket();
    if (sel?.id === ticketId) {
      this.selectedTicket.set({
        ...sel,
        assignedToName: agentName,
        status: 'InProgress'
      });
    }

    this.assigningId.set(ticketId);
    this.adminService
      .assignTicket(ticketId, { agentId })
      .pipe(finalize(() => this.assigningId.set('')))
      .subscribe({
        next: () => {
          this.ticketMsg.set(`Ticket successfully assigned to ${agentName}.`);
          this.loadTickets(true);
          this.loadAgents();
          setTimeout(() => this.ticketMsg.set(''), 5000);
        },
        error: () => {
          this.loadTickets(true);
          this.selectedAgent.update(m => ({ ...m, [ticketId]: '' }));
          this.ticketMsg.set(
            'Assignment failed. Please try another agent or refresh the list.'
          );
          setTimeout(() => this.ticketMsg.set(''), 5000);
        }
      });
  }

  loadUsers(silent = false): void {
    if (!silent) this.loadingUsers.set(true);
    this.adminService
      .getAllUsers()
      .pipe(
        catchError(() => of([] as UserDto[])),
        finalize(() => {
          this.loadingUsers.set(false);
          this.userActionBusyId.set('');
        })
      )
      .subscribe(u => {
        this.users.set(u ?? []);
        this.pendingRole.update(m => {
          const next = { ...m };
          (u ?? []).forEach(user => {
            if (!(user.id in next)) next[user.id] = '';
          });
          return next;
        });
      });
  }

  updateRole(userId: string, newRoleValue: number): void {
    const roleNames: Record<number, string> = {
      0: 'Admin',
      1: 'Agent',
      2: 'Customer'
    };
    const newRoleName = roleNames[newRoleValue];
    if (!newRoleName) return;

    this.users.update(arr => {
      const idx = arr.findIndex(u => u.id === userId);
      if (idx === -1) return arr;
      const copy = [...arr];
      copy[idx] = { ...copy[idx], role: newRoleName };
      return copy;
    });

    this.pendingRole.update(m => ({ ...m, [userId]: '' }));

    this.adminService.updateRole({ userId, newRole: newRoleValue }).subscribe({
      next: () => {
        this.userMsg.set(`Role changed to ${newRoleName}.`);
        this.loadAgents();
        setTimeout(() => this.userMsg.set(''), 2500);
      },
      error: () => {
        this.userMsg.set('Could not update role. Change reverted.');
        this.loadUsers(true);
        setTimeout(() => this.userMsg.set(''), 3000);
      }
    });
  }

  openDeactivateConfirm(id: string): void {
    this.confirmDeactivateUserId.set(id);
  }

  cancelDeactivateConfirm(): void {
    this.confirmDeactivateUserId.set(null);
  }

  confirmDeactivateUser(): void {
    const id = this.confirmDeactivateUserId();
    if (!id) return;
    this.confirmDeactivateUserId.set(null);

    this.users.update(arr => {
      const idx = arr.findIndex(u => u.id === id);
      if (idx === -1) return arr;
      const copy = [...arr];
      copy[idx] = { ...copy[idx], isActive: false };
      return copy;
    });

    this.userActionBusyId.set('');
    this.adminService
      .deleteUser(id)
      .pipe(finalize(() => this.userActionBusyId.set('')))
      .subscribe({
        next: () => {
          this.userMsg.set('User deactivated.');
          this.loadUsers(true);
          this.loadAgents();
          setTimeout(() => this.userMsg.set(''), 2500);
        },
        error: () => {
          this.userMsg.set('Could not deactivate user.');
          this.loadUsers(true);
          setTimeout(() => this.userMsg.set(''), 3000);
        }
      });
  }

  activateUser(id: string): void {
    this.userActionBusyId.set('');
    this.adminService
      .activateUser(id)
      .pipe(finalize(() => this.userActionBusyId.set('')))
      .subscribe({
        next: () => {
          this.userMsg.set('User activated.');
          this.loadUsers(true);
          this.loadAgents();
          setTimeout(() => this.userMsg.set(''), 2500);
        },
        error: () => {
          this.userMsg.set('Could not activate user.');
          setTimeout(() => this.userMsg.set(''), 3000);
        }
      });
  }

  openDeleteConfirm(id: string): void {
    this.confirmDeleteUserId.set(id);
  }

  cancelDeleteConfirm(): void {
    this.confirmDeleteUserId.set(null);
  }

  confirmPermanentlyDeleteUser(): void {
    const id = this.confirmDeleteUserId();
    if (!id) return;
    this.confirmDeleteUserId.set(null);

    this.users.update(arr => arr.filter(u => u.id !== id));
    this.pendingRole.update(m => {
      const { [id]: _, ...rest } = m;
      return rest;
    });

    this.userActionBusyId.set('');
    this.adminService
      .permanentlyDeleteUser(id)
      .pipe(finalize(() => this.userActionBusyId.set('')))
      .subscribe({
        next: () => {
          this.userMsg.set('User permanently deleted.');
          this.loadUsers(true);
          this.loadAgents();
          setTimeout(() => this.userMsg.set(''), 2500);
        },
        error: err => {
          this.userMsg.set(err.error?.message ?? 'Could not delete user.');
          this.loadUsers(true);
          setTimeout(() => this.userMsg.set(''), 4000);
        }
      });
  }

  openModal(): void {
    this.showModal.set(true);
    this.modalError.set('');
    this.newUser.set({ fullName: '', email: '', password: '', role: 1 });
  }

  patchNewUser(field: 'fullName' | 'email' | 'password' | 'role', value: string | number): void {
    this.newUser.update(u => ({ ...u, [field]: value }));
  }

  createUser(): void {
    const nu = this.newUser();
    if (!nu.fullName || !nu.email || !nu.password) {
      this.modalError.set('All fields are required.');
      return;
    }
    this.creatingUser.set(true);
    const payload = {
      fullName: nu.fullName.trim(),
      email: nu.email.trim(),
      password: nu.password,
      role: Number(nu.role)
    };
    this.adminService
      .createUser(payload)
      .pipe(finalize(() => this.creatingUser.set(false)))
      .subscribe({
        next: () => {
          this.showModal.set(false);
          this.modalError.set('');
          this.userMsg.set('User created successfully.');
          this.userSearch.set('');
          this.loadUsers(true);
          this.loadAgents();
          setTimeout(() => this.userMsg.set(''), 3000);
        },
        error: err => {
          this.modalError.set(err.error?.message ?? 'Failed to create user.');
        }
      });
  }

  badgeClass(status: string): string {
    const m: Record<string, string> = {
      Open: 'badge-open',
      InProgress: 'badge-inprogress',
      Resolved: 'badge-resolved'
    };
    return m[status] ?? '';
  }

  priorityClass(p: string): string {
    return `badge-${p.toLowerCase()}`;
  }

  categoryLabel(c: string): string {
    return this.CATEGORY_LABELS[c] ?? c;
  }

  roleClass(r: string): string {
    const m: Record<string, string> = {
      Admin: 'role-admin',
      Agent: 'role-agent',
      Customer: 'role-customer'
    };
    return m[r] ?? '';
  }

  onUserSearchInput(v: string): void {
    this.userSearch.set(v);
  }

  closeCreateModal(): void {
    this.showModal.set(false);
  }

  /** Stable string for agent `<select>` binding when the map has no key yet. */
  selectedAgentIdFor(ticketId: string): string {
    return this.selectedAgent()[ticketId] ?? '';
  }
}
