import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, inject } from '@angular/core';
import { AuthService } from './core/services/auth.service';
import { EnrollmentService } from './core/services/enrollment.service';
import { Notice } from './core/models/notice';
import { Session } from './core/models/platform.models';
import { AdminUsersComponent } from './features/admin-users/admin-users.component';
import { AuthComponent } from './features/auth/auth.component';
import { CoursesComponent } from './features/courses/courses.component';
import { LearningComponent } from './features/learning/learning.component';
import { AlertMessageComponent } from './shared/components/alert-message/alert-message.component';
import { errorMessage } from './core/services/error-message';

@Component({ selector: 'app-root', standalone: true, imports: [CommonModule, AuthComponent, CoursesComponent, LearningComponent, AdminUsersComponent, AlertMessageComponent], templateUrl: './app.component.html', styleUrl: './app.component.css' })
export class App {
  private readonly auth = inject(AuthService);
  private readonly changeDetector = inject(ChangeDetectorRef);
  private readonly enrollments = inject(EnrollmentService);
  page = this.auth.session?.role === 'Admin' ? 'admin-courses' : 'courses';
  session: Session | null = this.auth.session;
  message = '';
  error = false;
  get isAdmin(): boolean { return this.session?.role === 'Admin'; }

  onSignedIn(session: Session): void { this.session = session; this.page = session.role === 'Admin' ? 'admin-courses' : 'courses'; }
  navigate(page: string): void { this.page = page; window.scrollTo({ top: 0, behavior: 'smooth' }); }
  showNotice(notice: Notice): void { this.message = notice.text; this.error = notice.error ?? false; this.changeDetector.markForCheck(); }
  async signOut(): Promise<void> { await this.auth.signOut(); this.session = null; this.page = 'courses'; this.showNotice({ text: 'Signed out.' }); this.changeDetector.markForCheck(); }

  async enroll(courseId: number): Promise<void> {
    if (!this.session) return;
    try {
      await this.enrollments.enroll(this.session.id, courseId);
      this.showNotice({ text: 'Enrolled in course.' });
    } catch (error) {
      this.showNotice({ text: errorMessage(error), error: true });
    }
  }
}






