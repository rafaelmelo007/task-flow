import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { UserDto } from '../../shared/models/user.model';
import { AuthService } from './auth.service';

const TOKEN_KEY = 'tf_token';

@Injectable({ providedIn: 'root' })
export class AuthStore {
  private readonly router = inject(Router);
  private readonly auth = inject(AuthService);

  private readonly _token = signal<string | null>(localStorage.getItem(TOKEN_KEY));
  private readonly _user = signal<UserDto | null>(null);

  readonly token = this._token.asReadonly();
  readonly user = this._user.asReadonly();
  readonly isAuthenticated = computed(() => !!this._token());

  setAuth(token: string, user: UserDto) {
    this._token.set(token);
    this._user.set(user);
    localStorage.setItem(TOKEN_KEY, token);
  }

  logout() {
    this._token.set(null);
    this._user.set(null);
    localStorage.removeItem(TOKEN_KEY);
    this.router.navigate(['/login']);
  }

  /**
   * On app start / page reload we only have the persisted token. Fetch the current
   * user so the UI (e.g. sidebar identity) is populated. An invalid/expired token
   * surfaces as a 401 and clears the session.
   */
  restoreSession() {
    if (!this._token() || this._user()) return;
    this.auth.me().subscribe({
      next: (user) => this._user.set(user),
      error: () => this.logout(),
    });
  }
}
