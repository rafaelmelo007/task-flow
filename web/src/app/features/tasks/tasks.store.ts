import { DestroyRef, Injectable, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Observable, Subject, forkJoin, of } from 'rxjs';
import { catchError, map, switchMap, tap } from 'rxjs/operators';
import {
  CreateTaskRequest,
  PagedResult,
  TaskDto,
  TaskStatus,
  UpdateTaskRequest,
} from '../../shared/models/task.model';
import { TasksService, TaskQueryParams } from './tasks.service';
import { ToastService } from '../../core/toast/toast.service';

export interface StatusCounts {
  all: number;
  todo: number;
  inProgress: number;
  done: number;
}

@Injectable({ providedIn: 'root' })
export class TasksStore {
  private readonly tasksService = inject(TasksService);
  private readonly toast = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  // ── private writable state, exposed read-only ────────────────────────────
  private readonly _tasks = signal<TaskDto[]>([]);
  private readonly _totalCount = signal(0);
  private readonly _page = signal(1);
  private readonly _pageSize = signal(20);
  private readonly _statusFilter = signal<TaskStatus | null>(null);
  private readonly _search = signal('');
  private readonly _loading = signal(false);
  private readonly _counts = signal<StatusCounts>({ all: 0, todo: 0, inProgress: 0, done: 0 });

  readonly tasks = this._tasks.asReadonly();
  readonly totalCount = this._totalCount.asReadonly();
  readonly page = this._page.asReadonly();
  readonly pageSize = this._pageSize.asReadonly();
  readonly statusFilter = this._statusFilter.asReadonly();
  readonly search = this._search.asReadonly();
  readonly loading = this._loading.asReadonly();
  readonly counts = this._counts.asReadonly();

  readonly totalPages = computed(() => Math.ceil(this._totalCount() / this._pageSize()) || 1);
  readonly hasNext = computed(() => this._page() < this.totalPages());
  readonly hasPrev = computed(() => this._page() > 1);

  // switchMap pipeline cancels stale in-flight requests (e.g. rapid search).
  private readonly loadTrigger = new Subject<void>();

  constructor() {
    this.loadTrigger
      .pipe(
        tap(() => this._loading.set(true)),
        switchMap(() =>
          this.tasksService.getAll(this.currentParams()).pipe(
            catchError(() => {
              this.toast.show('Failed to load tasks', 'error');
              return of<PagedResult<TaskDto>>({
                items: [],
                totalCount: 0,
                page: 1,
                pageSize: this._pageSize(),
                totalPages: 1,
                hasNextPage: false,
                hasPreviousPage: false,
              });
            }),
          ),
        ),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((r) => {
        this._tasks.set(r.items);
        this._totalCount.set(r.totalCount);
        this._loading.set(false);
      });
  }

  private currentParams(): TaskQueryParams {
    const params: TaskQueryParams = { page: this._page(), pageSize: this._pageSize() };
    if (this._statusFilter() !== null) params.status = this._statusFilter()!;
    if (this._search()) params.search = this._search();
    return params;
  }

  load() {
    this.loadTrigger.next();
    this.refreshCounts();
  }

  /** Accurate per-status totals for the sidebar (cheap pageSize=1 probes). */
  refreshCounts() {
    const probe = (status?: TaskStatus): Observable<number> =>
      this.tasksService
        .getAll({ page: 1, pageSize: 1, status })
        .pipe(
          map((r) => r.totalCount),
          catchError(() => of(0)),
        );

    forkJoin([probe(undefined), probe(0), probe(1), probe(2)])
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(([all, todo, inProgress, done]) => {
        this._counts.set({ all, todo, inProgress, done });
      });
  }

  setStatusFilter(status: TaskStatus | null) {
    this._statusFilter.set(status);
    this._page.set(1);
    this.load();
  }

  setSearch(s: string) {
    this._search.set(s);
    this._page.set(1);
    this.load();
  }

  setPage(p: number) {
    this._page.set(p);
    this.loadTrigger.next();
  }

  create(req: CreateTaskRequest): Observable<TaskDto> {
    return this.tasksService.create(req).pipe(
      tap(() => {
        this.toast.show('Task created');
        this.load();
      }),
    );
  }

  update(id: string, req: UpdateTaskRequest): Observable<TaskDto> {
    return this.tasksService.update(id, req).pipe(
      tap(() => {
        this.toast.show('Task updated');
        this.load();
      }),
    );
  }

  delete(id: string): Observable<void> {
    return this.tasksService.delete(id).pipe(
      tap(() => {
        this.toast.show('Task deleted');
        this.load();
      }),
    );
  }
}
