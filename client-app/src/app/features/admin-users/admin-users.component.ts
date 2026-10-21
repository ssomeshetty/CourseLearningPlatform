import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, EventEmitter, Input, OnInit, Output, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Notice } from '../../core/models/notice';
import { Page, UserAnalytics, UserProfile } from '../../core/models/platform.models';
import { errorMessage } from '../../core/services/error-message';
import { UserService } from '../../core/services/user.service';
import { PaginationComponent } from '../../shared/components/pagination/pagination.component';

@Component({ selector: 'app-admin-users', standalone: true, imports: [CommonModule, FormsModule, PaginationComponent], templateUrl: './admin-users.component.html', styles: [` .panel{border:1px solid #ddd;border-radius:8px;padding:20px;background:#fff}.admin-page h1{margin-bottom:6px}.section-head{display:flex;align-items:center;justify-content:space-between;gap:16px;margin-bottom:20px}.section-copy{color:#666;margin-bottom:0}.eyebrow{margin-bottom:7px;color:#666;font-size:.72rem;letter-spacing:.12em;font-weight:700}.heading-actions{display:grid;justify-items:end;gap:14px}.form-grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(min(100%,220px),1fr));gap:14px;margin-bottom:20px}.form-grid label{display:grid;gap:7px;font-size:.9rem;font-weight:600}.wide-field{grid-column:1/-1}.user-list{border:1px solid #ddd;border-radius:8px;overflow:hidden}.list-heading,.user-row{display:grid;grid-template-columns:1.2fr 1.7fr 1fr .8fr 90px;align-items:center;gap:12px;padding:13px 16px}.list-heading{background:#f3f3f3;color:#555;font-size:.8rem;font-weight:700}.user-row{border-top:1px solid #eee;min-height:64px}.name-link{justify-self:start;padding:0;border:0;background:transparent;color:#111;text-align:left;text-decoration:underline;text-underline-offset:3px}.name-link:hover{background:transparent;color:#555}.email-cell{overflow-wrap:anywhere}.row-action{justify-self:start;padding:7px 11px}.detail-heading{margin-bottom:22px}.detail-heading>.section-head{margin-top:20px}.actions{display:flex;gap:8px;flex-wrap:wrap}.detail-grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(190px,1fr));gap:12px;margin:20px 0}.detail-grid article{display:grid;gap:7px;padding:15px;border:1px solid #ddd;border-radius:7px;overflow-wrap:anywhere}.detail-grid span{font-size:.82rem;color:#666}.analytics-block{margin-top:24px}.analytics-block h2{margin-bottom:16px}.metric-grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(145px,1fr));gap:12px}.metric-card{padding:15px;border:1px solid #ddd;border-radius:7px;display:grid;gap:9px}.metric-card span{color:#555;font-size:.88rem}.metric-card strong{font-size:1.5rem}.note{margin:12px 0 0;color:#666;font-size:.88rem}.empty-state{padding:26px;border:1px dashed #bbb;border-radius:8px;text-align:center;color:#555}@media(max-width:760px){.list-heading{display:none}.user-row{grid-template-columns:1fr auto;gap:8px}.email-cell{grid-column:1}.user-row>span:nth-of-type(2),.user-row>span:nth-of-type(3){font-size:.84rem;color:#666}.user-row>span:nth-of-type(2)::before{content:"Code: "}.user-row>span:nth-of-type(3)::before{content:"Role: "}.row-action{grid-column:2;grid-row:1/4;align-self:center}}@media(max-width:560px){.section-head{align-items:flex-start;flex-direction:column}.heading-actions{justify-items:start}.wide-field{grid-column:auto}}`] })
export class AdminUsersComponent implements OnInit {
  private readonly service = inject(UserService);
  private readonly changeDetector = inject(ChangeDetectorRef);
  @Output() notice = new EventEmitter<Notice>();
  @Input() mode: 'manage' | 'analytics' = 'manage';
  readonly pageSize = 10;
  page = 1;
  totalPages = 1;
  users: UserProfile[] = [];
  selectedUser: UserProfile | null = null;
  analytics: UserAnalytics | null = null;
  newUser = { name: '', email: '', employeeCode: '', password: '' };
  showCreateForm = false;
  editingUser = false;
  editForm = { name: '', email: '', employeeCode: '', joiningDate: '' };

  ngOnInit(): void { void this.load(); }
  async load(): Promise<void> {
    try { const result: Page<UserProfile> = await this.service.getPage(this.page, this.pageSize); this.users = result.items; this.totalPages = Math.max(result.totalPages, 1); }
    catch (error) { this.sendError(error); }
  }
  async changePage(page: number): Promise<void> { this.page = page; await this.load(); }
  async create(): Promise<void> {
    try { await this.service.create({ ...this.newUser, joiningDate: new Date().toISOString().slice(0, 10) }); this.newUser = { name: '', email: '', employeeCode: '', password: '' }; this.showCreateForm = false; this.notice.emit({ text: 'User created.' }); await this.load(); }
    catch (error) { this.sendError(error); }
  }
  async details(id: number): Promise<void> {
    try {
      [this.selectedUser, this.analytics] = await Promise.all([this.service.getById(id), this.service.getAnalytics(id)]);
      this.editingUser = false;
      this.changeDetector.markForCheck();
    } catch (error) { this.sendError(error); }
  }
  async showAnalytics(id: number): Promise<void> {
    try { this.analytics = await this.service.getAnalytics(id); this.selectedUser = this.users.find(user => user.id === id) ?? null; this.changeDetector.markForCheck(); }
    catch (error) { this.sendError(error); }
  }
  async edit(user: UserProfile): Promise<void> {
    await this.details(user.id);
    if (!this.selectedUser) return;
    this.editForm = { name: user.name, email: user.email, employeeCode: user.employeeCode, joiningDate: user.joiningDate };
    this.editingUser = true;
  }
  async saveEdit(): Promise<void> {
    if (!this.selectedUser) return;
    try {
      await this.service.update(this.selectedUser.id, this.editForm);
      this.notice.emit({ text: 'User updated.' }); this.changeDetector.markForCheck();
      await this.load();
      await this.details(this.selectedUser.id);
    } catch (error) { this.sendError(error); }
  }
  backToList(): void { this.selectedUser = null; this.analytics = null; this.editingUser = false; }
  async remove(user: UserProfile): Promise<void> {
    if (!confirm(`Delete ${user.name}?`)) return;
    try { await this.service.delete(user.id); if (this.selectedUser?.id === user.id) this.backToList(); if (this.users.length === 1 && this.page > 1) this.page--; this.notice.emit({ text: 'User deleted.' }); await this.load(); this.changeDetector.markForCheck(); }
    catch (error) { this.sendError(error); }
  }
  private sendError(error: unknown): void { this.notice.emit({ text: errorMessage(error), error: true }); }
}








