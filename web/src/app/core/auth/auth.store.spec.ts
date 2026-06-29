import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { AuthStore } from './auth.store';

describe('AuthStore', () => {
  let store: AuthStore;
  let routerSpy: { navigate: ReturnType<typeof vi.fn> };

  const fakeUser = { id: 'u1', email: 'test@test.com', createdAt: new Date().toISOString() };
  const TOKEN_KEY = 'tf_token';

  beforeEach(() => {
    routerSpy = { navigate: vi.fn() };
    localStorage.clear();

    TestBed.configureTestingModule({
      providers: [AuthStore, { provide: Router, useValue: routerSpy }, provideHttpClient(), provideHttpClientTesting()]
    });
    store = TestBed.inject(AuthStore);
  });

  it('is not authenticated initially when no token in storage', () => {
    expect(store.isAuthenticated()).toBe(false);
    expect(store.token()).toBeNull();
  });

  it('setAuth stores token and marks authenticated', () => {
    store.setAuth('my-token', fakeUser);
    expect(store.isAuthenticated()).toBe(true);
    expect(store.token()).toBe('my-token');
    expect(store.user()).toEqual(fakeUser);
    expect(localStorage.getItem(TOKEN_KEY)).toBe('my-token');
  });

  it('logout clears token and redirects to /login', () => {
    store.setAuth('my-token', fakeUser);
    store.logout();
    expect(store.isAuthenticated()).toBe(false);
    expect(store.token()).toBeNull();
    expect(localStorage.getItem(TOKEN_KEY)).toBeNull();
    expect(routerSpy.navigate).toHaveBeenCalledWith(['/login']);
  });

  it('restores token from localStorage on init', () => {
    localStorage.setItem(TOKEN_KEY, 'persisted-token');
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      providers: [AuthStore, { provide: Router, useValue: routerSpy }, provideHttpClient(), provideHttpClientTesting()]
    });
    const freshStore = TestBed.inject(AuthStore);
    expect(freshStore.isAuthenticated()).toBe(true);
    expect(freshStore.token()).toBe('persisted-token');
  });
});
