import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth-guard';
import { AppLayout } from './layout/app-layout';

export const routes: Routes = [
  {
    path: '',
    component: AppLayout,
    canActivate: [authGuard],
    children: [
      {
        path: '',
        pathMatch: 'full',
        title: 'Water · HealthTracker',
        loadComponent: () => import('./pages/water-tracker/water-tracker'),
      },
      {
        path: 'schedule',
        title: 'Schedule · HealthTracker',
        loadComponent: () => import('./pages/schedule/schedule'),
      },
      {
        path: 'calories',
        title: 'Calories · HealthTracker',
        loadComponent: () => import('./pages/calories/calories'),
      },
    ],
  },
  {
    path: 'login',
    title: 'Sign in · HealthTracker',
    loadComponent: () => import('./pages/login/login'),
  },
  {
    path: 'register',
    title: 'Register · HealthTracker',
    loadComponent: () => import('./pages/register/register'),
  },
  { path: '**', redirectTo: '' },
];
