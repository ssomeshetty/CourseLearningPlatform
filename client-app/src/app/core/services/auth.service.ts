import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { ApiService } from './api.service';
import { Session } from '../models/platform.models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly api = inject(ApiService);
  session: Session | null = this.readSession();

  signIn(email: string, password: string): Promise<Session> {
    return firstValueFrom(this.api.request<Session>('POST', '/auth/signin', { email, password }));
  }

  signUp(value: { name: string; email: string; employeeCode: string; password: string }): Promise<Session> {
    return firstValueFrom(this.api.request<Session>('POST', '/auth/signup', { ...value, joiningDate: null }));
  }

  saveSession(session: Session): void {
    this.session = session;
    localStorage.setItem('course-platform-token', session.accessToken);
    localStorage.setItem('course-platform-session', JSON.stringify(session));
  }

  async signOut(): Promise<void> {
    try { await firstValueFrom(this.api.request<void>('POST', '/auth/signout')); } catch { /* The token is removed locally either way. */ }
    localStorage.removeItem('course-platform-token');
    localStorage.removeItem('course-platform-session');
    this.session = null;
  }

  private readSession(): Session | null {
    try { return JSON.parse(localStorage.getItem('course-platform-session') || 'null') as Session | null; }
    catch { return null; }
  }
}
