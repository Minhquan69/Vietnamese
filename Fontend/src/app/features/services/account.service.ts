import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, catchError, map } from 'rxjs';

import { UserResult } from '../models/user-result.model';
import { environment } from '../../../environments/environment';
import type { ApiEnvelope } from '../../core/auth/auth.types';

@Injectable({
  providedIn: 'root',
})
export class AccountService {
  private apiUrl = `${environment.apiBaseUrl}/account`;

  constructor(private http: HttpClient) {}

  getCurrentUser(): Observable<UserResult> {
    return this.http
      .get<ApiEnvelope<UserResult>>(`${environment.apiV1Url}/auth/profile`)
      .pipe(
        map((r) => r.data),
        catchError(() => this.http.get<UserResult>(`${this.apiUrl}/me`)),
      );
  }

  changePassword(data: { oldPassword: string; newPassword: string }): Observable<string> {
    return this.http.put(`${this.apiUrl}/change-password`, data, {
      responseType: 'text',
    });
  }

  updateProfile(data: { name: string; email: string }): Observable<unknown> {
    return this.http.put(`${this.apiUrl}/update-profile`, data);
  }

  uploadAvatar(file: File): Observable<{ avatarUrl: string }> {
    const fd = new FormData();
    fd.append('file', file);
    return this.http
      .post<ApiEnvelope<{ avatarUrl: string }>>(
        `${environment.apiV1Url}/auth/me/avatar`,
        fd,
      )
      .pipe(map((r) => r.data));
  }

  getUsers(
    email?: string,
    status?: number,
    roleId?: number,
    page: number = 1,
    pageSize: number = 10,
  ): Observable<unknown> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);

    if (email) params = params.set('email', email);
    if (status !== null && status !== undefined)
      params = params.set('status', status);
    if (roleId !== null && roleId !== undefined)
      params = params.set('roleId', roleId);

    return this.http.get(`${this.apiUrl}/users`, { params });
  }

  updateUserStatus(userId: number, status: number) {
    return this.http.put(`${this.apiUrl}/users/${userId}/status`, null, {
      params: { status: status },
    });
  }

  updateUserRole(userId: number, roleId: number) {
    return this.http.put(`${this.apiUrl}/users/${userId}/role`, { roleId });
  }
}
