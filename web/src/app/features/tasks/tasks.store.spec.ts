import { TestBed } from '@angular/core/testing';
import { firstValueFrom, of, throwError } from 'rxjs';
import { TasksStore } from './tasks.store';
import { TasksService } from './tasks.service';
import { ToastService } from '../../core/toast/toast.service';
import { PagedResult, TaskDto } from '../../shared/models/task.model';

const makeTask = (overrides: Partial<TaskDto> = {}): TaskDto => ({
  id: 'task-1',
  title: 'Test task',
  status: 0,
  priority: 1,
  isOverdue: false,
  userId: 'user-1',
  createdAt: new Date().toISOString(),
  updatedAt: new Date().toISOString(),
  ...overrides,
});

const pagedResult = (items: TaskDto[], totalCount = items.length): PagedResult<TaskDto> => ({
  items,
  totalCount,
  page: 1,
  pageSize: 20,
  totalPages: 1,
  hasNextPage: false,
  hasPreviousPage: false,
});

describe('TasksStore', () => {
  let store: TasksStore;
  let tasksServiceSpy: { getAll: ReturnType<typeof vi.fn>; create: ReturnType<typeof vi.fn>; update: ReturnType<typeof vi.fn>; delete: ReturnType<typeof vi.fn> };
  let toastSpy: { show: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    tasksServiceSpy = {
      getAll: vi.fn().mockReturnValue(of(pagedResult([]))),
      create: vi.fn(),
      update: vi.fn(),
      delete: vi.fn(),
    };
    toastSpy = { show: vi.fn() };

    TestBed.configureTestingModule({
      providers: [
        TasksStore,
        { provide: TasksService, useValue: tasksServiceSpy },
        { provide: ToastService, useValue: toastSpy },
      ]
    });
    store = TestBed.inject(TasksStore);
  });

  it('load() populates tasks from service', () => {
    const tasks = [makeTask()];
    tasksServiceSpy.getAll.mockReturnValue(of(pagedResult(tasks)));
    store.load();
    expect(store.tasks()).toEqual(tasks);
    expect(store.totalCount()).toBe(1);
    expect(store.loading()).toBe(false);
  });

  it('load() shows error toast on failure', () => {
    tasksServiceSpy.getAll.mockReturnValue(throwError(() => new Error('oops')));
    store.load();
    expect(store.loading()).toBe(false);
    expect(toastSpy.show).toHaveBeenCalledWith('Failed to load tasks', 'error');
  });

  it('setStatusFilter resets page and reloads', () => {
    store.setPage(3);
    store.setStatusFilter(1);
    expect(store.page()).toBe(1);
    expect(store.statusFilter()).toBe(1);
  });

  it('setSearch resets page and updates search', () => {
    store.setPage(2);
    store.setSearch('my task');
    expect(store.page()).toBe(1);
    expect(store.search()).toBe('my task');
  });

  it('totalPages computed correctly', () => {
    tasksServiceSpy.getAll.mockReturnValue(of(pagedResult([makeTask()], 45)));
    store.load();
    expect(store.totalPages()).toBe(3);
  });

  it('refreshCounts populates per-status counts', () => {
    tasksServiceSpy.getAll
      .mockReturnValueOnce(of(pagedResult([], 10))) // all
      .mockReturnValueOnce(of(pagedResult([], 4)))  // todo
      .mockReturnValueOnce(of(pagedResult([], 3)))  // in progress
      .mockReturnValueOnce(of(pagedResult([], 3))); // done
    store.refreshCounts();
    expect(store.counts()).toEqual({ all: 10, todo: 4, inProgress: 3, done: 3 });
  });

  it('create() calls service and shows success toast', async () => {
    const task = makeTask();
    tasksServiceSpy.create.mockReturnValue(of(task));
    tasksServiceSpy.getAll.mockReturnValue(of(pagedResult([task])));
    const result = await firstValueFrom(store.create({ title: 'Test', status: 0, priority: 1 }));
    expect(result).toEqual(task);
    expect(toastSpy.show).toHaveBeenCalledWith('Task created');
  });

  it('delete() calls service and shows success toast', async () => {
    tasksServiceSpy.delete.mockReturnValue(of(undefined));
    tasksServiceSpy.getAll.mockReturnValue(of(pagedResult([])));
    await firstValueFrom(store.delete('task-1'));
    expect(tasksServiceSpy.delete).toHaveBeenCalledWith('task-1');
    expect(toastSpy.show).toHaveBeenCalledWith('Task deleted');
  });
});
