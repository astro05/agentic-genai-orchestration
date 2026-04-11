import { Injectable } from '@angular/core';
import { merge, Subject, timer } from 'rxjs';

/** Emits on an interval and when mutations bump the stream so dashboards stay in sync. */
@Injectable({ providedIn: 'root' })
export class LiveRefreshService {
  private readonly manual$ = new Subject<void>();

  /** First tick after 4s, then every 4s; also emits when `bump()` is called. */
  readonly ticketRefresh$ = merge(timer(4000, 4000), this.manual$);

  bump(): void {
    this.manual$.next();
  }
}
