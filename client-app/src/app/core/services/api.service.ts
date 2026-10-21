import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = 'http://localhost:5198/api';

  request<T>(method: string, path: string, body?: unknown) {
    const token = localStorage.getItem('course-platform-token');
    let headers = new HttpHeaders();
    if (token) headers = headers.set('Authorization', `Bearer ${token}`);
    if (body !== undefined) headers = headers.set('Content-Type', 'application/json');
    if (method.toUpperCase() === 'GET') headers = headers.set('Cache-Control', 'no-cache');
    return this.http.request<T>(method, `${this.baseUrl}${path}`, { body, headers });
  }
}




