import { inject } from '@angular/core';
import { CanActivateFn, Routes, Router } from '@angular/router';
import { AuthService } from './core/services/auth.service';
import { PageContentComponent } from './page-content.component';

const learnerOnly: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!auth.session) return router.createUrlTree(['/account']);
  if (auth.session.role === 'Admin') return router.createUrlTree(['/admin/courses']);
  return true;
};

const adminOnly: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!auth.session) return router.createUrlTree(['/account']);
  if (auth.session.role !== 'Admin') return router.createUrlTree(['/courses']);
  return true;
};

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'courses' },
  { path: 'courses', component: PageContentComponent, data: { page: 'courses' } },
  { path: 'courses/:id', component: PageContentComponent, data: { page: 'course-detail' } },
  { path: 'account', component: PageContentComponent, data: { page: 'account' } },
  { path: 'my-courses', component: PageContentComponent, canActivate: [learnerOnly], data: { page: 'my-courses' } },
  { path: 'my-analytics', component: PageContentComponent, canActivate: [learnerOnly], data: { page: 'my-analytics' } },
  { path: 'admin/courses', component: PageContentComponent, canActivate: [adminOnly], data: { page: 'admin-courses' } },
  { path: 'admin/courses/:id', component: PageContentComponent, canActivate: [adminOnly], data: { page: 'admin-course-detail' } },
  { path: 'admin/users', component: PageContentComponent, canActivate: [adminOnly], data: { page: 'admin-users' } },
  { path: 'admin/users/:id', component: PageContentComponent, canActivate: [adminOnly], data: { page: 'admin-user-detail' } },
  { path: '**', redirectTo: 'courses' },
];

