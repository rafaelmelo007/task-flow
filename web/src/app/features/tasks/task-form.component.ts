import { ChangeDetectionStrategy, Component, OnInit, inject, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { IconComponent } from '../../shared/ui/icon.component';
import { CreateTaskRequest, TaskDto, TaskPriority, TaskStatus, UpdateTaskRequest } from '../../shared/models/task.model';
import { TasksStore } from './tasks.store';

@Component({
  selector: 'app-task-form',
  standalone: true,
  imports: [FormsModule, IconComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="modal-overlay" (click)="close.emit()">
      <div class="modal" (click)="$event.stopPropagation()" (keydown.escape)="close.emit()" tabindex="-1">
        <div class="modal-header">
          <h2>{{ task() ? 'Edit Task' : 'New Task' }}</h2>
          <button class="icon-btn" aria-label="Close" (click)="close.emit()">
            <app-icon name="x" [size]="18" />
          </button>
        </div>

        <form (ngSubmit)="onSubmit()">
          <div class="form-group">
            <label class="form-label">Title</label>
            <input class="input" [(ngModel)]="title" name="title" required maxlength="200" placeholder="What needs doing?" />
            @if (errors()['title']) { <div class="form-error">{{ errors()['title'] }}</div> }
          </div>

          <div class="form-group">
            <label class="form-label">Description</label>
            <textarea class="input" [(ngModel)]="description" name="description" maxlength="2000" placeholder="Add detail (optional)"></textarea>
          </div>

          <div class="form-grid">
            <div class="form-group">
              <label class="form-label">Status</label>
              <select class="input" [(ngModel)]="status" name="status">
                <option [value]="0">Todo</option>
                <option [value]="1">In Progress</option>
                <option [value]="2">Done</option>
              </select>
            </div>
            <div class="form-group">
              <label class="form-label">Priority</label>
              <select class="input" [(ngModel)]="priority" name="priority">
                <option [value]="0">Low</option>
                <option [value]="1">Medium</option>
                <option [value]="2">High</option>
              </select>
            </div>
          </div>

          <div class="form-group">
            <label class="form-label">Due Date</label>
            <input class="input" type="datetime-local" [(ngModel)]="dueDate" name="dueDate" />
          </div>

          @if (serverError()) {
            <div class="form-error form-error-block">
              <app-icon name="alert" [size]="14" /> {{ serverError() }}
            </div>
          }

          <div class="modal-actions">
            <button type="button" class="btn btn-secondary" (click)="close.emit()">Cancel</button>
            <button type="submit" class="btn btn-primary" [disabled]="saving()">
              @if (saving()) { <span class="spinner"></span> }
              {{ task() ? 'Save Changes' : 'Create Task' }}
            </button>
          </div>
        </form>
      </div>
    </div>
  `,
})
export class TaskFormComponent implements OnInit {
  readonly task = input<TaskDto | null>(null);
  readonly close = output<void>();
  readonly saved = output<TaskDto>();

  private readonly store = inject(TasksStore);

  title = '';
  description = '';
  status: TaskStatus = 0;
  priority: TaskPriority = 1;
  dueDate = '';
  saving = signal(false);
  errors = signal<Record<string, string>>({});
  serverError = signal('');

  ngOnInit() {
    const t = this.task();
    if (t) {
      this.title = t.title;
      this.description = t.description ?? '';
      this.status = t.status;
      this.priority = t.priority;
      this.dueDate = t.dueDate ? new Date(t.dueDate).toISOString().slice(0, 16) : '';
    }
  }

  onSubmit() {
    const errs: Record<string, string> = {};
    if (!this.title.trim()) errs['title'] = 'Title is required';
    if (Object.keys(errs).length) { this.errors.set(errs); return; }
    this.errors.set({});
    this.serverError.set('');
    this.saving.set(true);

    const payload: CreateTaskRequest = {
      title: this.title.trim(),
      description: this.description || undefined,
      status: +this.status as TaskStatus,
      priority: +this.priority as TaskPriority,
      dueDate: this.dueDate ? new Date(this.dueDate).toISOString() : null,
    };

    const t = this.task();
    const request$ = t
      ? this.store.update(t.id, payload as UpdateTaskRequest)
      : this.store.create(payload);

    request$.subscribe({
      next: (result) => {
        this.saved.emit(result);
        this.close.emit();
      },
      error: (e: unknown) => {
        this.serverError.set(this.extractError(e));
        this.saving.set(false);
      },
    });
  }

  private extractError(e: unknown): string {
    const err = (e as { error?: { errors?: Record<string, string[]>; detail?: string; title?: string } })?.error;
    if (err?.errors) {
      return Object.entries(err.errors).map(([k, v]) => `${k}: ${v.join(', ')}`).join('; ');
    }
    return err?.detail || err?.title || 'Failed to save task';
  }
}
