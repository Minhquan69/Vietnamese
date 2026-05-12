import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import type { ApiEnvelope } from '../../core/auth/auth.types';
import type {
  InteractiveQuizResultDto,
  InteractiveQuizSubmitDto,
  PlayerQuizPackageDto,
  QuizAttemptSummaryDto,
} from '../models/interactive-quiz.model';

@Injectable({ providedIn: 'root' })
export class InteractiveQuizService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiV1Url}/interactive-quiz`;

  private unwrap<T>(source: Observable<ApiEnvelope<T>>): Observable<T> {
    return source.pipe(map((r) => r.data));
  }

  take(quizId: number): Observable<PlayerQuizPackageDto> {
    const params = new HttpParams().set('quizId', String(quizId));
    return this.unwrap(this.http.get<ApiEnvelope<PlayerQuizPackageDto>>(`${this.base}/take`, { params }));
  }

  submit(body: InteractiveQuizSubmitDto): Observable<InteractiveQuizResultDto> {
    return this.unwrap(this.http.post<ApiEnvelope<InteractiveQuizResultDto>>(`${this.base}/submit`, body));
  }

  attempts(quizId?: number): Observable<QuizAttemptSummaryDto[]> {
    let params = new HttpParams();
    if (quizId != null) {
      params = params.set('quizId', String(quizId));
    }
    return this.unwrap(this.http.get<ApiEnvelope<QuizAttemptSummaryDto[]>>(`${this.base}/attempts`, { params }));
  }
}
