import { Routes } from '@angular/router';
import { authGuard } from './core/auth-guard';
import { Dashboard } from './features/dashboard/dashboard';
import { Login } from './features/login/login';
import { Register } from './features/register/register';

export const routes: Routes = [
  { path: '', component: Dashboard, canActivate: [authGuard] },
  { path: 'login', component: Login },
  { path: 'register', component: Register },
  { path: '**', redirectTo: '' }
];
