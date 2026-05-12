import { HttpClient, HttpBackend } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Router } from '@angular/router';
import { jwtDecode } from 'jwt-decode';
import { Observable, map, tap } from 'rxjs';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment';
import type { ApiEnvelope, AuthTokens } from './auth.types';

const ACCESS = 'accessToken';
const REFRESH = 'refreshToken';
const LEGACY = 'token';

@Injectable({ providedIn: 'root' })
export class AuthSessionService {
  private readonly http = inject(HttpClient);
  private readonly httpBackend = inject(HttpBackend);
  private readonly router = inject(Router);

  /** HttpClient that bypasses interceptors (refresh calls) */
  private readonly rawHttp = new HttpClient(this.httpBackend);

  private refreshPromise: Promise<boolean> | null = null;

  private authV1Url(): string {
    return `${environment.apiV1Url}/auth`;
  }

  getAccessToken(): string | null {
    return localStorage.getItem(ACCESS) ?? localStorage.getItem(LEGACY);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(REFRESH);
  }

  hasValidAccessToken(): boolean {
    const t = this.getAccessToken();
    if (!t) {
      return false;
    }
    try {
      const decoded = jwtDecode<{ exp?: number }>(t);
      if (!decoded.exp) {
        return true;
      }
      return decoded.exp * 1000 > Date.now() + 5000;
    } catch {
      return true;
    }
  }

  getRoleFromAccessToken(): string | null {
    const t = this.getAccessToken();
    if (!t) {
      return null;
    }
    try {
      const decoded = jwtDecode<Record<string, unknown>>(t);
      const r =
        (decoded['role'] as string | undefined) ||
        (decoded[
          'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'
        ] as string | undefined);
      return r ?? null;
    } catch {
      return null;
    }
  }

  persistTokens(tokens: AuthTokens): void {
    localStorage.setItem(ACCESS, tokens.accessToken);
    localStorage.setItem(REFRESH, tokens.refreshToken);
    localStorage.setItem(LEGACY, tokens.token ?? tokens.accessToken);
  }

  clearSession(): void {
    localStorage.removeItem(ACCESS);
    localStorage.removeItem(REFRESH);
    localStorage.removeItem(LEGACY);
  }

  login(email: string, password: string): Observable<AuthTokens> {
    return this.http
      .post<ApiEnvelope<AuthTokens>>(`${this.authV1Url()}/login`, { email, password })
      .pipe(
        tap((res) => {
          if (!res.success || !res.data) {
            throw new Error(res.message || 'Login failed');
          }
          this.persistTokens(res.data);
        }),
        map((res) => res.data),
      );
  }

  register(payload: { name: string; email: string; password: string }): Observable<AuthTokens> {
    return this.http
      .post<ApiEnvelope<AuthTokens>>(`${this.authV1Url()}/register`, payload)
      .pipe(
        tap((res) => {
          if (!res.success || !res.data) {
            throw new Error(res.message || 'Register failed');
          }
          this.persistTokens(res.data);
        }),
        map((res) => res.data),
      );
  }

  forgotPassword(email: string): Observable<unknown> {
    return this.http
      .post<ApiEnvelope<unknown>>(`${this.authV1Url()}/forgot-password`, { email })
      .pipe(map((r) => r.data));
  }

  resetPassword(payload: {
    email: string;
    token: string;
    newPassword: string;
  }): Observable<unknown> {
    return this.http
      .post<ApiEnvelope<unknown>>(`${this.authV1Url()}/reset-password`, payload)
      .pipe(map((r) => r.data));
  }

  refreshAccessToken(): Promise<boolean> {
    if (this.refreshPromise) {
      return this.refreshPromise;
    }
    this.refreshPromise = this.performRefresh().finally(() => {
      this.refreshPromise = null;
    });
    return this.refreshPromise;
  }

  private async performRefresh(): Promise<boolean> {
    const refresh = this.getRefreshToken();
    if (!refresh) {
      return false;
    }
    try {
      const res = await firstValueFrom(
        this.rawHttp.post<ApiEnvelope<AuthTokens>>(`${this.authV1Url()}/refresh-token`, {
          refreshToken: refresh,
        }),
      );
      if (!res.success || !res.data) {
        this.clearSession();
        return false;
      }
      this.persistTokens(res.data);
      return true;
    } catch {
      this.clearSession();
      return false;
    }
  }

  logout(navigateToLogin = true): void {
    this.clearSession();
    if (navigateToLogin) {
      void this.router.navigate(['/login']);
    }
  }
}
