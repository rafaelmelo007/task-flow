import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { AuthStore } from '../../core/auth/auth.store';
import { Router } from '@angular/router';
import { IconComponent } from '../../shared/ui/icon.component';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule, RouterLink, IconComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="auth-container">
      <div class="auth-card">
        <div class="auth-brand">
          <span class="brand-mark"><app-icon name="logo" [size]="22" [strokeWidth]="2" /></span>
          <span class="auth-brand-name">TaskFlow</span>
        </div>
        <p class="auth-subtitle">Sign in to your account</p>

        <div class="demo-hint">
          <strong>Demo:</strong> demo&#64;taskflow.app · Demo123!
        </div>

        <form (ngSubmit)="onSubmit()" #f="ngForm">
          <div class="form-group">
            <label class="form-label">Email</label>
            <div class="input-icon">
              <app-icon name="mail" [size]="16" />
              <input class="input" type="email" [(ngModel)]="email" name="email" required placeholder="you@example.com" />
            </div>
          </div>
          <div class="form-group">
            <label class="form-label">Password</label>
            <div class="input-icon">
              <app-icon name="lock" [size]="16" />
              <input class="input" type="password" [(ngModel)]="password" name="password" required placeholder="••••••••" />
            </div>
          </div>
          @if (error()) {
            <div class="form-error form-error-block"><app-icon name="alert" [size]="14" /> {{ error() }}</div>
          }
          <button class="btn btn-primary btn-block" type="submit" [disabled]="loading()">
            @if (loading()) { <span class="spinner"></span> } Sign In
          </button>
        </form>
        <p class="auth-switch">
          No account? <a routerLink="/register">Register</a>
        </p>
      </div>
    </div>
  `
})
export class LoginComponent {
  private readonly authService = inject(AuthService);
  private readonly authStore = inject(AuthStore);
  private readonly router = inject(Router);

  email = '';
  password = '';
  loading = signal(false);
  error = signal('');

  onSubmit() {
    this.loading.set(true);
    this.error.set('');
    this.authService.login({ email: this.email, password: this.password }).subscribe({
      next: r => {
        this.authStore.setAuth(r.token, r.user);
        this.router.navigate(['/tasks']);
      },
      error: e => {
        this.loading.set(false);
        this.error.set(e.error?.error || 'Invalid email or password');
      }
    });
  }
}
