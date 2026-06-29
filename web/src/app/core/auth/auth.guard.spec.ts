import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { Router, UrlTree } from '@angular/router';
import { AuthStore } from './auth.store';
import { authGuard } from './auth.guard';

describe('authGuard', () => {
  let authStore: AuthStore;
  let routerSpy: { createUrlTree: ReturnType<typeof vi.fn>; navigate: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    routerSpy = {
      createUrlTree: vi.fn().mockReturnValue({ toString: () => '/login' } as unknown as UrlTree),
      navigate: vi.fn(),
    };
    localStorage.clear();

    TestBed.configureTestingModule({
      providers: [AuthStore, { provide: Router, useValue: routerSpy }, provideHttpClient(), provideHttpClientTesting()]
    });
    authStore = TestBed.inject(AuthStore);
  });

  const runGuard = () =>
    TestBed.runInInjectionContext(() => authGuard({} as any, {} as any));

  it('returns true when authenticated', () => {
    authStore.setAuth('tok', { id: '1', email: 'a@b.com', createdAt: '' });
    expect(runGuard()).toBe(true);
  });

  it('redirects to /login when not authenticated', () => {
    const result = runGuard();
    expect(routerSpy.createUrlTree).toHaveBeenCalledWith(['/login']);
    expect(result).not.toBe(true);
  });
});
