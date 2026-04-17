import { Injectable } from '@angular/core';
import { HttpClient,HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

import { LoginResult } from '../models/login-result.model';
import { UserResult } from '../models/user-result.model';
@Injectable({
  providedIn: 'root',
})
export class AccountService {
  private apiUrl = 'http://localhost:5108/api/account';

  constructor(private http: HttpClient) {}

  login(data: any): Observable<LoginResult> {
    return this.http.post<LoginResult>(`${this.apiUrl}/login`, data);
  }
  register(data: any): Observable<LoginResult> {
    return this.http.post<LoginResult>(`${this.apiUrl}/register`, data);
  }
  getCurrentUser(): Observable<UserResult> {
    return this.http.get<UserResult>(`${this.apiUrl}/me`);
  }
  changePassword(data: any): Observable<string> {
    return this.http.put(`${this.apiUrl}/change-password`, data, {
      responseType: 'text',
    });
  }
  updateProfile(data: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/update-profile`, data);
  }

  getUsers(
    email?: string,
    status?: number,
    roleId?: number,
    page: number = 1,
    pageSize: number = 10,
  ): Observable<any> {
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