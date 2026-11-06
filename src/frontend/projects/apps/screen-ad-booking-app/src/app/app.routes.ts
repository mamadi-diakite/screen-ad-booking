import { Routes } from '@angular/router';
import { authGuard, guestGuard } from '@screen-ad-booking/shared/core';
import { ChangePasswordPage } from './auth/change-password-page';
import { LoginPage } from './auth/login-page';
import { RegisterPage } from './auth/register-page';
import { HomePage } from './home/home-page';

export const routes: Routes = [
  { path: '', component: HomePage, title: 'Screen Ad Booking' },
  {
    path: 'login',
    component: LoginPage,
    canActivate: [guestGuard],
    title: 'Log in · Screen Ad Booking',
  },
  {
    path: 'register',
    component: RegisterPage,
    canActivate: [guestGuard],
    title: 'Create an account · Screen Ad Booking',
  },
  {
    path: 'account/password',
    component: ChangePasswordPage,
    canActivate: [authGuard],
    title: 'Change password · Screen Ad Booking',
  },
  { path: '**', redirectTo: '' },
];
