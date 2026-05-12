import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import type { ApiEnvelope } from '../../core/auth/auth.types';
import type {
  ReviewResultDto,
  UserVocabularyCardDto,
  VocabularyCardDto,
  VocabularyListResultDto,
  VocabularyStatsDto,
} from '../models/vocabulary.model';

@Injectable({ providedIn: 'root' })
export class VocabularyLearningService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiV1Url}/vocabulary`;

  private unwrap<T>(source: Observable<ApiEnvelope<T>>): Observable<T> {
    return source.pipe(map((r) => r.data));
  }

  search(q: string | undefined, page = 1, pageSize = 24): Observable<VocabularyListResultDto> {
    let params = new HttpParams().set('page', String(page)).set('pageSize', String(pageSize));
    if (q?.trim()) {
      params = params.set('q', q.trim());
    }
    return this.unwrap(this.http.get<ApiEnvelope<VocabularyListResultDto>>(this.base, { params }));
  }

  getById(id: number): Observable<VocabularyCardDto> {
    return this.unwrap(this.http.get<ApiEnvelope<VocabularyCardDto>>(`${this.base}/${id}`));
  }

  getMine(id: number): Observable<UserVocabularyCardDto> {
    return this.unwrap(this.http.get<ApiEnvelope<UserVocabularyCardDto>>(`${this.base}/${id}/me`));
  }

  getDeck(limit = 24): Observable<UserVocabularyCardDto[]> {
    const params = new HttpParams().set('limit', String(limit));
    return this.unwrap(
      this.http.get<ApiEnvelope<UserVocabularyCardDto[]>>(`${this.base}/me/deck`, { params }),
    );
  }

  review(id: number, grade: 'again' | 'hard' | 'good' | 'easy'): Observable<ReviewResultDto> {
    return this.unwrap(
      this.http.post<ApiEnvelope<ReviewResultDto>>(`${this.base}/${id}/review`, { grade }),
    );
  }

  setSaved(id: number, saved: boolean): Observable<{ vocabularyId: number; saved: boolean }> {
    return this.unwrap(
      this.http.put<ApiEnvelope<{ vocabularyId: number; saved: boolean }>>(`${this.base}/${id}/saved`, {
        saved,
      }),
    );
  }

  stats(): Observable<VocabularyStatsDto> {
    return this.unwrap(this.http.get<ApiEnvelope<VocabularyStatsDto>>(`${this.base}/me/stats`));
  }

  savedList(): Observable<UserVocabularyCardDto[]> {
    return this.unwrap(this.http.get<ApiEnvelope<UserVocabularyCardDto[]>>(`${this.base}/me/saved`));
  }
}
