import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import type { ApiEnvelope } from '../../core/auth/auth.types';
import type {
  VideoExtractResultDto,
  VideoLearningSessionDto,
} from '../models/video-learning.model';

@Injectable({ providedIn: 'root' })
export class VideoLearningService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiV1Url}/video-learning`;

  private unwrap<T>(source: Observable<ApiEnvelope<T>>): Observable<T> {
    return source.pipe(map((r) => r.data));
  }

  session(youtubeId: string): Observable<VideoLearningSessionDto> {
    const params = new HttpParams().set('youtubeId', youtubeId);
    return this.unwrap(this.http.get<ApiEnvelope<VideoLearningSessionDto>>(`${this.base}/session`, { params }));
  }

  extract(youtubeId: string, transcriptId: number): Observable<VideoExtractResultDto> {
    let params = new HttpParams().set('youtubeId', youtubeId).set('transcriptId', String(transcriptId));
    return this.unwrap(this.http.get<ApiEnvelope<VideoExtractResultDto>>(`${this.base}/extract`, { params }));
  }

  link(body: {
    youtubeId: string;
    vocabularyId: number;
    transcriptId?: number | null;
    contextSnippet?: string | null;
  }): Observable<boolean> {
    return this.unwrap(this.http.post<ApiEnvelope<boolean>>(`${this.base}/link`, body));
  }
}
