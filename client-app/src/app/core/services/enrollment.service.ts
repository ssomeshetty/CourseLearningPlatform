import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { ApiService } from './api.service';
import { Enrollment, UserAnalytics } from '../models/platform.models';

@Injectable({ providedIn: 'root' })
export class EnrollmentService {
  private readonly api = inject(ApiService);
  getForUser(userId: number): Promise<Enrollment[]> { return firstValueFrom(this.api.request('GET', `/users/${userId}/courses`)); }
  getAnalytics(userId: number): Promise<UserAnalytics> { return firstValueFrom(this.api.request('GET', `/users/${userId}/analytics`)); }
  enroll(userId: number, courseId: number): Promise<unknown> { return firstValueFrom(this.api.request('POST', `/users/${userId}/courses/${courseId}`)); }
  updateStatus(userId: number, courseId: number, status: string): Promise<unknown> { return firstValueFrom(this.api.request('PUT', `/users/${userId}/courses/${courseId}/status`, { status })); }
}
