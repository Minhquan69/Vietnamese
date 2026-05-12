import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import type { ApiEnvelope } from '../../core/auth/auth.types';
import type {
  AiTutorChatRequest,
  AiTutorChatResponse,
  TutorConversationSummary,
  TutorMessage,
} from '../models/ai-tutor.model';

@Injectable({ providedIn: 'root' })
export class AiTutorService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiV1Url}/ai-tutor`;

  private unwrap<T>(source: Observable<ApiEnvelope<T>>): Observable<T> {
    return source.pipe(map((r) => r.data));
  }

  chat(body: AiTutorChatRequest): Observable<AiTutorChatResponse> {
    return this.unwrap(this.http.post<ApiEnvelope<AiTutorChatResponse>>(`${this.base}/chat`, body));
  }

  conversations(): Observable<TutorConversationSummary[]> {
    return this.unwrap(this.http.get<ApiEnvelope<TutorConversationSummary[]>>(`${this.base}/conversations`));
  }

  messages(conversationId: number): Observable<TutorMessage[]> {
    return this.unwrap(
      this.http.get<ApiEnvelope<TutorMessage[]>>(`${this.base}/conversations/${conversationId}/messages`),
    );
  }
}
