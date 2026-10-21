import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, EventEmitter, Input, OnChanges, Output, SimpleChanges, inject } from '@angular/core';
import { Enrollment, UserAnalytics } from '../../core/models/platform.models';
import { Notice } from '../../core/models/notice';
import { EnrollmentService } from '../../core/services/enrollment.service';
import { errorMessage } from '../../core/services/error-message';

@Component({ selector: 'app-learning', standalone: true, imports: [CommonModule], templateUrl: './learning.component.html', styles: [` .panel{border:1px solid #ddd;border-radius:8px;padding:22px;background:#fff}.section-head{display:flex;align-items:center;justify-content:space-between;gap:18px;margin-bottom:18px}.section-head h2,.section-head h3{margin-bottom:6px}.section-copy{color:#666;margin-bottom:0}.eyebrow{margin-bottom:7px;color:#666;font-size:.72rem;letter-spacing:.12em;font-weight:700}.course-grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(min(100%,250px),1fr));gap:16px}.course-card{border:1px solid #ddd;border-radius:7px;padding:18px;display:flex;flex-direction:column;gap:8px}.course-card h3{margin-bottom:4px}.course-card label{display:grid;gap:7px;margin-top:auto;font-size:.9rem;font-weight:600}.status-line{color:#555}.analytics-block{margin-top:32px}.summary-heading{margin-bottom:12px}.metric-grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(145px,1fr));gap:12px}.metric-card{padding:16px;border:1px solid #ddd;border-radius:7px;display:grid;gap:10px}.metric-card span{color:#555;font-size:.88rem}.metric-card strong{font-size:1.6rem}.note{margin-top:12px;color:#666;font-size:.88rem}.empty-state{padding:26px;border:1px dashed #bbb;border-radius:8px;text-align:center;color:#555}@media(max-width:600px){.section-head{align-items:flex-start;flex-direction:column}}`] })
export class LearningComponent implements OnChanges {
  private readonly service = inject(EnrollmentService);
  private readonly changeDetector = inject(ChangeDetectorRef);
  @Input() userId: number | null = null;
  @Input() mode: 'enrollments' | 'analytics' = 'enrollments';
  @Output() notice = new EventEmitter<Notice>();
  enrollments: Enrollment[] = [];
  analytics: UserAnalytics | null = null;

  ngOnChanges(changes: SimpleChanges): void {
    if ((changes['userId'] || changes['mode']) && this.userId !== null) {
      void this.refresh();
    }
  }
  async refresh(): Promise<boolean> {
    if (this.userId === null) return false;
    try {
      if (this.mode === 'analytics') {
        this.analytics = await this.service.getAnalytics(this.userId);
      } else {
        this.enrollments = await this.service.getForUser(this.userId);
      }
      this.changeDetector.markForCheck();
      return true;
    } catch (error) {
      this.sendError(error);
      return false;
    }
  }
  async updateStatus(item: Enrollment, status: string): Promise<void> {
    if (this.userId === null) return;
    try { await this.service.updateStatus(this.userId, item.courseId, status); await this.refresh(); this.notice.emit({ text: 'Course status updated.' }); }
    catch (error) { this.sendError(error); }
  }
  private sendError(error: unknown): void { this.notice.emit({ text: errorMessage(error), error: true }); }
}






