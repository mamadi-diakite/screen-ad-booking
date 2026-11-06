import { Routes } from '@angular/router';
import { advertiserGuard, authGuard, guestGuard } from '@screen-ad-booking/shared/core';
import { ChangePasswordPage } from './auth/change-password-page';
import { LoginPage } from './auth/login-page';
import { RegisterPage } from './auth/register-page';
import { CreditsPage } from './credits/credits-page';
import { HomePage } from './home/home-page';
import { VenuesPage } from './venues/venues-page';
import { VideosPage } from './videos/videos-page';

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
  {
    path: 'venues',
    component: VenuesPage,
    canActivate: [authGuard],
    title: 'Venues · Screen Ad Booking',
  },
  {
    path: 'credits',
    component: CreditsPage,
    canActivate: [advertiserGuard],
    title: 'Credits · Screen Ad Booking',
  },
  {
    path: 'videos',
    component: VideosPage,
    canActivate: [advertiserGuard],
    title: 'Videos · Screen Ad Booking',
  },
  { path: '**', redirectTo: '' },
];
