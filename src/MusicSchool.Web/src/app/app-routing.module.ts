import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { authGuard, loginRedirectGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    loadComponent: () => import('./modules/login/login.component').then((m) => m.LoginComponent),
    canActivate: [loginRedirectGuard]
  },
  {
    path: 'login',
    loadComponent: () => import('./modules/login/login.component').then((m) => m.LoginComponent),
    canActivate: [loginRedirectGuard]
  },
  {
    path: 'dashboard',
    loadChildren: () => import('./modules/dashboard/dashboard.routes').then((m) => m.DASHBOARD_ROUTES),
    canActivate: [authGuard]
  },
  {
    path: 'lessons',
    loadChildren: () => import('./modules/lessons/lessons.routes').then((m) => m.LESSONS_ROUTES),
    canActivate: [authGuard]
  },
  {
    path: 'families',
    loadChildren: () => import('./modules/families/families.routes').then((m) => m.FAMILIES_ROUTES),
    canActivate: [authGuard]
  },
  {
    path: 'users',
    loadChildren: () => import('./modules/users/users.routes').then((m) => m.USERS_ROUTES),
    canActivate: [authGuard]
  },
  {
    path: 'teachers',
    loadChildren: () => import('./modules/teachers/teachers.routes').then((m) => m.TEACHERS_ROUTES),
    canActivate: [authGuard]
  },
  {
    path: 'curriculum',
    loadChildren: () => import('./modules/curriculum/curriculum.routes').then((m) => m.CURRICULUM_ROUTES),
    canActivate: [authGuard]
  },
  {
    path: 'payments',
    loadChildren: () => import('./modules/payments/payments.routes').then((m) => m.PAYMENTS_ROUTES),
    canActivate: [authGuard]
  },
  {
    path: '**',
    redirectTo: ''
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {}
