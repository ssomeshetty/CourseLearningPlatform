import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, EventEmitter, Input, OnInit, Output, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Course, CourseAnalytics, CourseInput } from '../../core/models/platform.models';
import { Notice } from '../../core/models/notice';
import { CourseService } from '../../core/services/course.service';
import { errorMessage } from '../../core/services/error-message';
import { CourseCardComponent } from '../../shared/components/course-card/course-card.component';
import { PaginationComponent } from '../../shared/components/pagination/pagination.component';

@Component({ selector: 'app-courses', standalone: true, imports: [CommonModule, FormsModule, CourseCardComponent, PaginationComponent], templateUrl: './courses.component.html', styles: [` .panel{border:1px solid #ddd;border-radius:8px;padding:22px;margin:0 0 24px;background:#fff}.section-head{display:flex;align-items:center;justify-content:space-between;gap:18px;margin:0 0 18px}.section-head h1,.section-head h2{margin-bottom:6px}.course-heading{margin-top:8px}.section-copy{color:#666;margin-bottom:0}.eyebrow{margin-bottom:7px;color:#666;font-size:.72rem;letter-spacing:.12em;font-weight:700}.heading-actions{display:grid;justify-items:end;gap:14px}.form-grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(min(100%,220px),1fr));gap:14px;margin-bottom:20px}.form-grid label{display:grid;gap:7px;font-size:.9rem;font-weight:600}.wide-field{grid-column:1/-1}.actions{display:flex;gap:8px;flex-wrap:wrap}.course-grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(min(100%,260px),1fr));gap:16px;margin-bottom:24px}.course-list{border:1px solid #ddd;border-radius:8px;overflow:hidden}.list-heading,.course-row{display:grid;grid-template-columns:1.6fr 1.2fr .8fr .7fr 80px;align-items:center;gap:12px;padding:13px 16px}.list-heading{background:#f3f3f3;color:#555;font-size:.8rem;font-weight:700}.course-row{border-top:1px solid #eee;min-height:64px}.name-link{justify-self:start;padding:0;border:0;background:transparent;color:#111;text-align:left;text-decoration:underline;text-underline-offset:3px}.name-link:hover{background:transparent;color:#555}.row-action{justify-self:start;padding:7px 11px}.course-detail{margin-top:20px}.detail-meta{display:grid;grid-template-columns:repeat(auto-fit,minmax(160px,1fr));gap:12px;margin:20px 0}.detail-meta article{display:grid;gap:6px;padding:14px;border:1px solid #ddd;border-radius:7px}.detail-meta span{font-size:.82rem;color:#666}.description{margin:22px 0}.description p{line-height:1.6}.description img{max-width:min(100%,480px);max-height:260px;object-fit:cover;border-radius:7px}.analytics-block{margin-top:28px}.analytics-block h2{margin-bottom:15px}.metric-grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(145px,1fr));gap:12px}.metric-card{padding:15px;border:1px solid #ddd;border-radius:7px;display:grid;gap:9px}.metric-card span{color:#555;font-size:.88rem}.metric-card strong{font-size:1.5rem}.empty-state{padding:28px;border:1px dashed #bbb;border-radius:8px;text-align:center;color:#555}@media(max-width:760px){.list-heading{display:none}.course-row{grid-template-columns:1fr auto;gap:8px}.course-row>span{font-size:.84rem;color:#666}.course-row>span:nth-of-type(1){grid-column:1}.course-row>span:nth-of-type(2)::before{content:"Duration: "}.course-row>span:nth-of-type(3)::before{content:"Price: "}.row-action{grid-column:2;grid-row:1/5;align-self:center}}@media(max-width:560px){.section-head{align-items:flex-start;flex-direction:column}.heading-actions{justify-items:start}.wide-field{grid-column:auto}}`] })
export class CoursesComponent implements OnInit {
  private readonly service = inject(CourseService);
  private readonly changeDetector = inject(ChangeDetectorRef);
  @Output() notice = new EventEmitter<Notice>();
  @Input() isAdmin = false;
  @Input() mode: 'catalog' | 'manage' = 'catalog';
  @Input() signedIn = false;
  @Output() enrollRequest = new EventEmitter<number>();
  readonly pageSize = 10;
  page = 1;
  totalPages = 1;
  courses: Course[] = [];
  loading = false;
  selectedCourse: Course | null = null;
  analytics: CourseAnalytics | null = null;
  editingId: number | null = null;
  showForm = false;
  form: CourseInput = this.emptyForm();

  ngOnInit(): void { void this.load(); }
  async load(): Promise<void> {
    this.loading = true;
    try {
      const result = await this.service.getPage(this.page, this.pageSize);
      this.courses = result.items ?? [];
      this.totalPages = Math.max(result.totalPages ?? 1, 1);
      console.info('Course page loaded: ' + JSON.stringify({ page: result.page, totalCount: result.totalCount, received: this.courses.length, titles: this.courses.map(course => course.title) }));
    } catch (error) { this.sendError(error); }
    finally { this.loading = false; this.changeDetector.markForCheck(); }
  }
  async changePage(page: number): Promise<void> { this.page = page; await this.load(); }
  async showCourse(id: number): Promise<void> {
    try {
      this.selectedCourse = await this.service.getById(id);
      this.analytics = this.mode === 'manage' ? await this.service.getAnalytics(id) : null;
      this.changeDetector.markForCheck();
    } catch (error) { this.sendError(error); }
  }
  edit(course: Course): void { this.editingId = course.id; this.form = { ...course }; this.selectedCourse = null; this.analytics = null; this.showForm = true; window.scrollTo({ top: 0, behavior: 'smooth' }); }
  cancelEdit(): void { this.editingId = null; this.form = this.emptyForm(); this.showForm = false; }
  async save(): Promise<void> {
    try {
      const body = { ...this.form, cost: Number(this.form.cost), hours: Number(this.form.hours) };
      if (this.editingId === null) await this.service.create(body); else await this.service.update(this.editingId, body);
      this.cancelEdit(); this.notice.emit({ text: 'Course saved.' }); await this.load();
    } catch (error) { this.sendError(error); }
  }
  async remove(id: number): Promise<void> {
    if (!confirm('Delete this course?')) return;
    try { await this.service.delete(id); this.selectedCourse = null; this.analytics = null; if (this.courses.length === 1 && this.page > 1) this.page--; this.notice.emit({ text: 'Course deleted.' }); await this.load(); }
    catch (error) { this.sendError(error); }
  }
  async showAnalytics(id: number): Promise<void> {
    try { this.selectedCourse = this.courses.find(course => course.id === id) ?? null; this.analytics = await this.service.getAnalytics(id); }
    catch (error) { this.sendError(error); }
  }
  private emptyForm(): CourseInput { return { title: '', description: '', instructor: '', imageUrl: '', cost: null, hours: null }; }
  private sendError(error: unknown): void { this.notice.emit({ text: errorMessage(error), error: true }); }
}














