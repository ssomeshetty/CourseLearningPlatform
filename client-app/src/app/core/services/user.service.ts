import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { ApiService } from './api.service';
import { Page, UserAnalytics, UserProfile } from '../models/platform.models';

@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly api = inject(ApiService);
  getPage(page: number, pageSize: number): Promise<Page<UserProfile>> { return firstValueFrom(this.api.request('GET', `/users?page=${page}&pageSize=${pageSize}`)); }
  getById(id: number): Promise<UserProfile> { return firstValueFrom(this.api.request('GET', `/users/${id}`)); }
  getAnalytics(id: number): Promise<UserAnalytics> { return firstValueFrom(this.api.request('GET', `/users/${id}/analytics`)); }
  create(user: object): Promise<unknown> { return firstValueFrom(this.api.request('POST', '/users', user)); }
  update(id: number, user: object): Promise<unknown> { return firstValueFrom(this.api.request('PUT', `/users/${id}`, user)); }
  delete(id: number): Promise<void> { return firstValueFrom(this.api.request('DELETE', `/users/${id}`)); }
}
