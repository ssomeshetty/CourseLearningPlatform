import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from './core/services/auth.service';
import { EnrollmentService } from './core/services/enrollment.service';
import { errorMessage } from './core/services/error-message';
import { Notice } from './core/models/notice';
import { Session } from './core/models/platform.models';
import { AdminUsersComponent } from './features/admin-users/admin-users.component';
import { AuthComponent } from './features/auth/auth.component';
import { CoursesComponent } from './features/courses/courses.component';
import { LearningComponent } from './features/learning/learning.component';
import { AlertMessageComponent } from './shared/components/alert-message/alert-message.component';

@Component({
  selector: 'app-page-content',
  standalone: true,
  imports: [CommonModule, AuthComponent, CoursesComponent, LearningComponent, AdminUsersComponent, AlertMessageComponent],
  templateUrl: './page-content.component.html'
})
export class PageContentComponent {
  private readonly auth = inject(AuthService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly changeDetector = inject(ChangeDetectorRef);
  private readonly enrollments = inject(EnrollmentService);

  page = '';
  recordId: number | null = null;
  message = '';
  error = false;

  constructor() {
    this.route.data.subscribe(data => {
      this.page = String(data['page'] ?? '');
      this.changeDetector.markForCheck();
    });
    this.route.paramMap.subscribe(params => {
      const id = params.get('id');
      this.recordId = id === null ? null : Number(id);
      this.changeDetector.markForCheck();
    });
  }

  get session(): Session | null {
    return this.auth.session;
  }

  get isAdmin(): boolean {
    return this.session?.role === 'Admin';
  }

  async onSignedIn(session: Session): Promise<void> {
    this.showNotice({ text: 'Signed in.' });
    await this.router.navigateByUrl(session.role === 'Admin' ? '/admin/courses' : '/courses');
  }

  showNotice(notice: Notice): void {
    this.message = notice.text;
    this.error = notice.error ?? false;
    this.changeDetector.markForCheck();
  }

  async enroll(courseId: number): Promise<void> {
    if (!this.session) return;

    try {
      await this.enrollments.enroll(this.session.id, courseId);
      this.showNotice({ text: 'Enrolled in course.' });
    } catch (error) {
      this.showNotice({ text: errorMessage(error), error: true });
    }
  }

  openCourse(id: number, admin = false): void {
    void this.router.navigateByUrl(admin ? '/admin/courses/' + id : '/courses/' + id);
  }

  openUser(id: number): void {
    void this.router.navigateByUrl('/admin/users/' + id);
  }

  backToCourses(admin = false): void {
    void this.router.navigateByUrl(admin ? '/admin/courses' : '/courses');
  }

  backToUsers(): void {
    void this.router.navigateByUrl('/admin/users');
  }
}

