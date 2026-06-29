import { TestBed } from '@angular/core/testing';
import { ToastService } from './toast.service';

describe('ToastService', () => {
  let service: ToastService;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [ToastService] });
    service = TestBed.inject(ToastService);
    vi.useFakeTimers();
  });

  afterEach(() => vi.useRealTimers());

  it('show() adds a toast with correct type', () => {
    service.show('hello', 'success');
    expect(service.toasts()).toHaveLength(1);
    expect(service.toasts()[0].message).toBe('hello');
    expect(service.toasts()[0].type).toBe('success');
  });

  it('dismiss() removes a toast by id', () => {
    service.show('a');
    service.show('b');
    const id = service.toasts()[0].id;
    service.dismiss(id);
    expect(service.toasts()).toHaveLength(1);
    expect(service.toasts()[0].message).toBe('b');
  });

  it('auto-dismisses toast after 3500ms', () => {
    service.show('auto');
    expect(service.toasts()).toHaveLength(1);
    vi.advanceTimersByTime(3500);
    expect(service.toasts()).toHaveLength(0);
  });

  it('show() defaults to success type', () => {
    service.show('ok');
    expect(service.toasts()[0].type).toBe('success');
  });
});
