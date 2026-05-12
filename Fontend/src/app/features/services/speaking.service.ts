import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import type { ApiEnvelope } from '../../core/auth/auth.types';
import type {
  SpeakingAnalytics,
  SpeakingAttemptSummary,
  SpeakingEvaluateResponse,
} from '../models/speaking.model';

@Injectable({ providedIn: 'root' })
export class SpeakingService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiV1Url}/speaking`;

  private unwrap<T>(source: Observable<ApiEnvelope<T>>): Observable<T> {
    return source.pipe(map((r) => r.data));
  }

  evaluate(formData: FormData): Observable<SpeakingEvaluateResponse> {
    return this.unwrap(
      this.http.post<ApiEnvelope<SpeakingEvaluateResponse>>(`${this.base}/evaluate`, formData),
    );
  }

  analytics(): Observable<SpeakingAnalytics> {
    return this.unwrap(this.http.get<ApiEnvelope<SpeakingAnalytics>>(`${this.base}/analytics`));
  }

  history(take = 50): Observable<SpeakingAttemptSummary[]> {
    return this.unwrap(
      this.http.get<ApiEnvelope<SpeakingAttemptSummary[]>>(`${this.base}/history`, {
        params: { take: take.toString() },
      }),
    );
  }
}
