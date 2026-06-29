import { ChangeDetectionStrategy, Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { AuthStore } from '../../core/auth/auth.store';
import { ToastService } from '../../core/toast/toast.service';
import { IconComponent, IconName } from '../../shared/ui/icon.component';
import { TaskDto, TaskPriority, TaskStatus } from '../../shared/models/task.model';
import { TaskFormComponent } from './task-form.component';
import { TasksStore } from './tasks.store';

interface FilterDef {
  label: string;
  value: TaskStatus | null;
  icon: IconName;
  count: () => number;
}

@Component({
  selector: 'app-task-list',
  standalone: true,
  imports: [TaskFormComponent, IconComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="app-shell">
      <!-- Sidebar -->
      <aside class="sidebar">
        <div class="brand">
          <span class="brand-mark"><app-icon name="logo" [size]="20" [strokeWidth]="2" /></span>
          <span class="brand-name">TaskFlow</span>
        </div>

        <nav class="nav">
          <div class="nav-label">
            <app-icon name="filter" [size]="13" /> Filters
          </div>
          @for (f of filters; track f.label) {
            <button
              class="nav-item"
              [class.active]="store.statusFilter() === f.value"
              (click)="store.setStatusFilter(f.value)"
            >
              <app-icon [name]="f.icon" [size]="16" />
              <span class="nav-item-label">{{ f.label }}</span>
              <span class="nav-item-count">{{ f.count() }}</span>
            </button>
          }
        </nav>

        <div class="sidebar-spacer"></div>

        <div class="stats-card">
          <div class="stats-title">Overview</div>
          <div class="stat-row"><span>Open</span><b>{{ openCount() }}</b></div>
          <div class="stat-row"><span>Done</span><b>{{ store.counts().done }}</b></div>
          <div class="stat-row"><span>Total</span><b>{{ store.counts().all }}</b></div>
        </div>

        <div class="sidebar-footer">
          <div class="user-line" [title]="authStore.user()?.email ?? ''">
            <span class="avatar">{{ initials() }}</span>
            <span class="user-email">{{ authStore.user()?.email }}</span>
          </div>
          <button class="icon-btn" aria-label="Sign out" title="Sign out" (click)="authStore.logout()">
            <app-icon name="log-out" [size]="16" />
          </button>
        </div>
      </aside>

      <!-- Main -->
      <main class="main">
        <header class="page-header">
          <div class="page-header-top">
            <button class="icon-btn mobile-only" aria-label="Toggle filters" (click)="mobileFilters.set(!mobileFilters())">
              <app-icon name="filter" [size]="18" />
            </button>
            <h1 class="page-title">My Tasks</h1>
            <div class="search-box">
              <app-icon name="search" [size]="16" class="search-icon" />
              <input
                class="search-input"
                type="search"
                placeholder="Search tasks…"
                aria-label="Search tasks"
                [value]="searchText()"
                (input)="searchText.set($any($event.target).value)"
              />
            </div>
            <button class="btn btn-primary" (click)="openCreate()">
              <app-icon name="plus" [size]="16" /> New Task
            </button>
          </div>

          <!-- Mobile-only filter row (sidebar is hidden below lg) -->
          @if (mobileFilters()) {
            <div class="mobile-filters">
              @for (f of filters; track f.label) {
                <button
                  class="chip"
                  [class.active]="store.statusFilter() === f.value"
                  (click)="store.setStatusFilter(f.value)"
                >{{ f.label }} <span class="chip-count">{{ f.count() }}</span></button>
              }
            </div>
          }
        </header>

        @if (store.loading()) {
          <div class="task-list">
            @for (s of [1,2,3,4]; track s) { <div class="skeleton-card"></div> }
          </div>
        } @else if (store.tasks().length === 0) {
          <div class="empty-state">
            <app-icon name="inbox" [size]="40" [strokeWidth]="1.5" />
            <h3>No tasks found</h3>
            <p>{{ store.search() ? 'Try a different search.' : 'Create your first task to get started.' }}</p>
            <button class="btn btn-primary" (click)="openCreate()">
              <app-icon name="plus" [size]="16" /> New Task
            </button>
          </div>
        } @else {
          <div class="task-list">
            @for (task of store.tasks(); track task.id) {
              <article class="task-card" [class.is-done]="task.status === 2">
                <span class="status-icon" [class]="'status-' + statusKey(task.status)" [title]="statusLabel(task.status)">
                  <app-icon [name]="statusIcon(task.status)" [size]="18" />
                </span>

                <div class="task-body">
                  <div class="task-title">{{ task.title }}</div>
                  @if (task.description) {
                    <div class="task-desc">{{ task.description }}</div>
                  }
                </div>

                <div class="task-meta">
                  <span class="priority" [class]="'priority-' + priorityKey(task.priority)">
                    <app-icon [name]="priorityIcon(task.priority)" [size]="14" />
                    {{ priorityLabel(task.priority) }}
                  </span>
                  @if (task.dueDate) {
                    <span class="due" [class.overdue]="task.isOverdue">
                      <app-icon name="calendar" [size]="14" />
                      {{ formatDate(task.dueDate) }}
                    </span>
                  }
                </div>

                <div class="task-actions">
                  <button class="icon-btn" aria-label="Edit task" title="Edit" (click)="openEdit(task)">
                    <app-icon name="edit" [size]="16" />
                  </button>
                  <button class="icon-btn danger" aria-label="Delete task" title="Delete" (click)="confirmDelete(task)">
                    <app-icon name="trash" [size]="16" />
                  </button>
                </div>
              </article>
            }
          </div>

          @if (store.totalPages() > 1) {
            <div class="pager">
              <button class="icon-btn" aria-label="Previous page" [disabled]="!store.hasPrev()" (click)="store.setPage(store.page()-1)">
                <app-icon name="chevron-left" [size]="18" />
              </button>
              <span class="pager-info">{{ store.page() }} / {{ store.totalPages() }}</span>
              <button class="icon-btn" aria-label="Next page" [disabled]="!store.hasNext()" (click)="store.setPage(store.page()+1)">
                <app-icon name="chevron-right" [size]="18" />
              </button>
            </div>
          }
        }
      </main>
    </div>

    @if (showForm()) {
      <app-task-form [task]="editTask()" (close)="closeForm()" (saved)="closeForm()" />
    }

    @if (deleteTarget(); as target) {
      <div class="modal-overlay" (click)="deleteTarget.set(null)">
        <div class="modal modal-sm" (click)="$event.stopPropagation()" (keydown.escape)="deleteTarget.set(null)" tabindex="-1">
          <div class="modal-icon danger"><app-icon name="trash" [size]="22" /></div>
          <h3>Delete task?</h3>
          <p class="muted">“{{ target.title }}” will be permanently removed.</p>
          <div class="modal-actions">
            <button class="btn btn-secondary" (click)="deleteTarget.set(null)">Cancel</button>
            <button class="btn btn-danger" [disabled]="deleting()" (click)="doDelete()">
              @if (deleting()) { <span class="spinner"></span> } Delete
            </button>
          </div>
        </div>
      </div>
    }

    <div class="toast-container">
      @for (t of toast.toasts(); track t.id) {
        <div class="toast" [class]="'toast-' + t.type">
          <app-icon [name]="t.type === 'error' ? 'alert' : 'check-circle'" [size]="16" />
          {{ t.message }}
        </div>
      }
    </div>
  `,
})
export class TaskListComponent implements OnInit {
  readonly store = inject(TasksStore);
  readonly authStore = inject(AuthStore);
  readonly toast = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly showForm = signal(false);
  readonly editTask = signal<TaskDto | null>(null);
  readonly deleteTarget = signal<TaskDto | null>(null);
  readonly deleting = signal(false);
  readonly mobileFilters = signal(false);
  readonly searchText = signal('');

  readonly filters: FilterDef[] = [
    { label: 'All Tasks', value: null, icon: 'inbox', count: () => this.store.counts().all },
    { label: 'Todo', value: 0, icon: 'circle', count: () => this.store.counts().todo },
    { label: 'In Progress', value: 1, icon: 'circle-dashed', count: () => this.store.counts().inProgress },
    { label: 'Done', value: 2, icon: 'check-circle', count: () => this.store.counts().done },
  ];

  readonly openCount = computed(() => this.store.counts().todo + this.store.counts().inProgress);
  readonly initials = computed(() => (this.authStore.user()?.email ?? '?').slice(0, 2).toUpperCase());

  ngOnInit() {
    this.store.load();
    // Debounced search — cancels stale requests via the store's switchMap pipeline.
    toObservable(this.searchText)
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntilDestroyed(this.destroyRef))
      .subscribe((s) => this.store.setSearch(s));
  }

  openCreate() { this.editTask.set(null); this.showForm.set(true); }
  openEdit(t: TaskDto) { this.editTask.set(t); this.showForm.set(true); }
  closeForm() { this.showForm.set(false); this.editTask.set(null); }
  confirmDelete(t: TaskDto) { this.deleteTarget.set(t); }

  doDelete() {
    const t = this.deleteTarget();
    if (!t) return;
    this.deleting.set(true);
    this.store.delete(t.id).subscribe({
      next: () => { this.deleting.set(false); this.deleteTarget.set(null); },
      error: () => { this.deleting.set(false); },
    });
  }

  statusKey(s: TaskStatus): string { return ['todo', 'inprogress', 'done'][s]; }
  statusLabel(s: TaskStatus): string { return ['Todo', 'In Progress', 'Done'][s]; }
  statusIcon(s: TaskStatus): IconName { return (['circle', 'circle-dashed', 'check-circle'] as IconName[])[s]; }

  priorityKey(p: TaskPriority): string { return ['low', 'medium', 'high'][p]; }
  priorityLabel(p: TaskPriority): string { return ['Low', 'Med', 'High'][p]; }
  priorityIcon(p: TaskPriority): IconName { return (['chevron-down', 'equal', 'chevrons-up'] as IconName[])[p]; }

  formatDate(d: string): string {
    return new Date(d).toLocaleDateString(undefined, { month: 'short', day: 'numeric' });
  }
}
