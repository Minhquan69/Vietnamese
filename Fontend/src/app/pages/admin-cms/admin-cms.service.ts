import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import type { ApiEnvelope } from '../../core/auth/auth.types';
import type {
  AdminAnalyticsSummary,
  AdminCourseRow,
  AdminLessonRow,
  AdminQuizRow,
  AdminUserRow,
  AdminVocabularyRow,
  PagedResult,
} from './admin-cms.model';

@Injectable({ providedIn: 'root' })
export class AdminCmsService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiV1Url}/admin/cms`;

  private unwrap<T>(source: Observable<ApiEnvelope<T>>): Observable<T> {
    return source.pipe(map((r) => r.data));
  }

  analytics(): Observable<AdminAnalyticsSummary> {
    return this.unwrap(this.http.get<ApiEnvelope<AdminAnalyticsSummary>>(`${this.base}/analytics`));
  }

  users(
    page: number,
    pageSize: number,
    email?: string,
    status?: number | null,
    roleId?: number | null,
  ): Observable<PagedResult<AdminUserRow>> {
    let p = new HttpParams()
      .set('page', String(page))
      .set('pageSize', String(pageSize));
    if (email?.trim()) {
      p = p.set('email', email.trim());
    }
    if (status != null && !Number.isNaN(status)) {
      p = p.set('status', String(status));
    }
    if (roleId != null && roleId > 0) {
      p = p.set('roleId', String(roleId));
    }
    return this.unwrap(
      this.http.get<ApiEnvelope<PagedResult<AdminUserRow>>>(`${this.base}/users`, { params: p }),
    );
  }

  courses(
    page: number,
    pageSize: number,
    levelId?: number | null,
    q?: string,
    activeOnly?: boolean | null,
  ): Observable<PagedResult<AdminCourseRow>> {
    let p = new HttpParams()
      .set('page', String(page))
      .set('pageSize', String(pageSize));
    if (levelId != null && levelId > 0) {
      p = p.set('levelId', String(levelId));
    }
    if (q?.trim()) {
      p = p.set('q', q.trim());
    }
    if (activeOnly === true) {
      p = p.set('activeOnly', 'true');
    }
    return this.unwrap(
      this.http.get<ApiEnvelope<PagedResult<AdminCourseRow>>>(`${this.base}/courses`, { params: p }),
    );
  }

  lessons(
    page: number,
    pageSize: number,
    filters: {
      levelId?: number | null;
      courseId?: number | null;
      unitId?: number | null;
      q?: string;
      activeOnly?: boolean | null;
    },
  ): Observable<PagedResult<AdminLessonRow>> {
    let p = new HttpParams()
      .set('page', String(page))
      .set('pageSize', String(pageSize));
    const f = filters;
    if (f.levelId != null && f.levelId > 0) {
      p = p.set('levelId', String(f.levelId));
    }
    if (f.courseId != null && f.courseId > 0) {
      p = p.set('courseId', String(f.courseId));
    }
    if (f.unitId != null && f.unitId > 0) {
      p = p.set('unitId', String(f.unitId));
    }
    if (f.q?.trim()) {
      p = p.set('q', f.q.trim());
    }
    if (f.activeOnly === true) {
      p = p.set('activeOnly', 'true');
    }
    return this.unwrap(
      this.http.get<ApiEnvelope<PagedResult<AdminLessonRow>>>(`${this.base}/lessons`, { params: p }),
    );
  }

  vocabulary(
    page: number,
    pageSize: number,
    q?: string,
    activeOnly?: boolean | null,
  ): Observable<PagedResult<AdminVocabularyRow>> {
    let p = new HttpParams()
      .set('page', String(page))
      .set('pageSize', String(pageSize));
    if (q?.trim()) {
      p = p.set('q', q.trim());
    }
    if (activeOnly === true) {
      p = p.set('activeOnly', 'true');
    }
    return this.unwrap(
      this.http.get<ApiEnvelope<PagedResult<AdminVocabularyRow>>>(`${this.base}/vocabulary`, { params: p }),
    );
  }

  quizzes(
    page: number,
    pageSize: number,
    q?: string,
    refType?: string,
    activeOnly?: boolean | null,
  ): Observable<PagedResult<AdminQuizRow>> {
    let p = new HttpParams()
      .set('page', String(page))
      .set('pageSize', String(pageSize));
    if (q?.trim()) {
      p = p.set('q', q.trim());
    }
    if (refType?.trim()) {
      p = p.set('refType', refType.trim());
    }
    if (activeOnly === true) {
      p = p.set('activeOnly', 'true');
    }
    return this.unwrap(
      this.http.get<ApiEnvelope<PagedResult<AdminQuizRow>>>(`${this.base}/quizzes`, { params: p }),
    );
  }
}
