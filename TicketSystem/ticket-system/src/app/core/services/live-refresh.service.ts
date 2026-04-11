import { Injectable } from '@angular/core';
import { merge, Subject, timer } from 'rxjs';

/** Emits on an interval and when mutations bump the stream so dashboards stay in sync. */
@Injectable({ providedIn: 'root' })
export class LiveRefreshService {
  private readonly manual$ = new Subject<void>();

  /** First background sync after 8s, then every 12s; `bump()` triggers an immediate sync. */
  readonly ticketRefresh$ = merge(timer(8000, 12000), this.manual$);

  bump(): void {
    this.manual$.next();
  }
}
