import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'tasks', pathMatch: 'full' },
  { path: 'login', loadComponent: () => import('./features/auth/login.component').then(m => m.LoginComponent) },
  { path: 'register', loadComponent: () => import('./features/auth/register.component').then(m => m.RegisterComponent) },
  { path: 'tasks', loadComponent: () => import('./features/tasks/task-list.component').then(m => m.TaskListComponent), canActivate: [authGuard] },
  { path: '**', redirectTo: 'tasks' }
];
