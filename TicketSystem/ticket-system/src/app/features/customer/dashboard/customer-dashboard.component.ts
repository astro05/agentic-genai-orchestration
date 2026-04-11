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
import { finalize } from 'rxjs/operators';
import { TicketService } from '../../../core/services/ticket.service';
import { AuthService } from '../../../core/services/auth.service';
import { LiveRefreshService } from '../../../core/services/live-refresh.service';
import {
  TicketDto,
  TicketMessageDto,
  STATUS_LABELS,
  CATEGORY_LABELS
} from '../../../core/models/models';

@Component({
  selector: 'app-customer-dashboard',
  templateUrl: './customer-dashboard.component.html',
  styleUrls: ['./customer-dashboard.component.scss'],
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CustomerDashboardComponent implements OnInit {
  readonly tickets = signal<TicketDto[]>([]);
  readonly loading = signal(true);
  readonly error = signal('');
  readonly filter = signal('All');
  readonly search = signal('');
  readonly ticketMsg = signal('');

  readonly selectedTicket = signal<TicketDto | null>(null);
  readonly messageDraft = signal('');
  readonly replyToMessageId = signal<string | null>(null);
  readonly postingMessage = signal(false);

  readonly filtered = computed(() => {
    const list = this.tickets();
    let result =
      this.filter() === 'All'
        ? [...list]
        : list.filter(t => t.status === this.filter());
    const q = this.search().trim().toLowerCase();
    if (q) {
      result = result.filter(
        t =>
          t.title.toLowerCase().includes(q) ||
          t.description.toLowerCase().includes(q)
      );
    }
    return result;
  });

  readonly stats = computed(() => {
    const t = this.tickets();
    return {
      total: t.length,
      open: t.filter(x => x.status === 'Open').length,
      inProgress: t.filter(x => x.status === 'InProgress').length,
      resolved: t.filter(x => x.status === 'Resolved').length
    };
  });

  readonly STATUS_LABELS = STATUS_LABELS;
  readonly CATEGORY_LABELS = CATEGORY_LABELS;
  readonly filters = ['All', 'Open', 'InProgress', 'Resolved'];

  private readonly ticketService = inject(TicketService);
  public readonly authService = inject(AuthService);
  private readonly liveRefresh = inject(LiveRefreshService);
  private readonly destroyRef = inject(DestroyRef);

  ngOnInit(): void {
    this.load(false);
    this.liveRefresh.ticketRefresh$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.load(true));
  }

  load(silent = false): void {
    if (!silent) this.loading.set(true);
    this.ticketService
      .getMyTickets()
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: t => {
          this.tickets.set(t);
          const sel = this.selectedTicket();
          if (sel) {
            const cur = t.find(x => x.id === sel.id);
            if (cur) this.selectedTicket.set({ ...cur });
          }
        },
        error: () => this.error.set('Failed to load tickets.')
      });
  }

  setFilter(f: string): void {
    this.filter.set(f);
  }

  onSearchInput(v: string): void {
    this.search.set(v);
  }

  openTicketDetail(t: TicketDto): void {
    this.selectedTicket.set({ ...t });
    this.messageDraft.set('');
    this.replyToMessageId.set(null);
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
    const m = list.find(x => x.id === messageId);
    if (!m) return '';
    const text = m.body.trim();
    return text.length > 100 ? `${text.slice(0, 100)}…` : text;
  }

  sortedMessages(t: TicketDto): TicketMessageDto[] {
    const m = t.messages ?? [];
    return [...m].sort(
      (a, b) =>
        new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime()
    );
  }

  sendMessage(): void {
    const ticket = this.selectedTicket();
    if (!ticket) return;
    const body = this.messageDraft().trim();
    if (!body) return;

    this.postingMessage.set(true);
    this.ticketService
      .addTicketMessage(ticket.id, {
        body,
        replyToMessageId: this.replyToMessageId()
      })
      .pipe(finalize(() => this.postingMessage.set(false)))
      .subscribe({
        next: msg => {
          this.mergeMessage(ticket.id, msg);
          this.messageDraft.set('');
          this.replyToMessageId.set(null);
          this.ticketMsg.set('Reply posted.');
          this.liveRefresh.bump();
          setTimeout(() => this.ticketMsg.set(''), 3000);
        },
        error: () => this.error.set('Could not send reply.')
      });
  }

  private mergeMessage(ticketId: string, msg: TicketMessageDto): void {
    const now = new Date().toISOString();
    this.tickets.update(arr =>
      arr.map(t => {
        if (t.id !== ticketId) return t;
        const messages = [...(t.messages ?? []), msg];
        return { ...t, messages, updatedAt: now };
      })
    );
    const sel = this.selectedTicket();
    if (sel?.id === ticketId) {
      const messages = [...(sel.messages ?? []), msg];
      this.selectedTicket.set({ ...sel, messages, updatedAt: now });
    }
  }

  badgeClass(status: string): string {
    const map: Record<string, string> = {
      Open: 'badge-open',
      InProgress: 'badge-inprogress',
      Resolved: 'badge-resolved'
    };
    return map[status] ?? '';
  }

  priorityClass(p: string): string {
    return `badge-${p.toLowerCase()}`;
  }

  categoryLabel(c: string): string {
    return this.CATEGORY_LABELS[c] ?? c;
  }

  messageRoleClass(role: string): string {
    if (role === 'Agent') return 'msg-agent';
    if (role === 'Admin') return 'msg-admin';
    return 'msg-customer';
  }
}
