import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { ApiService } from './api.service';
import { Course, CourseAnalytics, CourseInput, Page } from '../models/platform.models';

@Injectable({ providedIn: 'root' })
export class CourseService {
  private readonly api = inject(ApiService);
  getPage(page: number, pageSize: number): Promise<Page<Course>> { return firstValueFrom(this.api.request('GET', `/courses?page=${page}&pageSize=${pageSize}`)); }
  getById(id: number): Promise<Course> { return firstValueFrom(this.api.request('GET', `/courses/${id}`)); }
  getAnalytics(id: number): Promise<CourseAnalytics> { return firstValueFrom(this.api.request('GET', `/courses/${id}/analytics`)); }
  create(course: CourseInput): Promise<unknown> { return firstValueFrom(this.api.request('POST', '/courses', course)); }
  update(id: number, course: CourseInput): Promise<unknown> { return firstValueFrom(this.api.request('PUT', `/courses/${id}`, course)); }
  delete(id: number): Promise<void> { return firstValueFrom(this.api.request('DELETE', `/courses/${id}`)); }
}
