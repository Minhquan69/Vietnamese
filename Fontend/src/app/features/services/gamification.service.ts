import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import type { ApiEnvelope } from '../../core/auth/auth.types';
import type { GamificationLeaderboardRow, GamificationState } from '../models/gamification.model';

@Injectable({ providedIn: 'root' })
export class GamificationService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiV1Url}/gamification`;

  private unwrap<T>(source: Observable<ApiEnvelope<T>>): Observable<T> {
    return source.pipe(map((r) => r.data));
  }

  me(): Observable<GamificationState> {
    return this.unwrap(this.http.get<ApiEnvelope<GamificationState>>(`${this.base}/me`));
  }

  leaderboard(take = 50): Observable<GamificationLeaderboardRow[]> {
    return this.unwrap(
      this.http.get<ApiEnvelope<GamificationLeaderboardRow[]>>(`${this.base}/leaderboard`, {
        params: { take: take.toString() },
      }),
    );
  }
}
