import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import type { ApiEnvelope } from '../../core/auth/auth.types';
import type {
  CourseCatalogItemDto,
  CourseLearnDetailDto,
  LessonCompleteResultDto,
  LessonPlayerDto,
  UnitOutlineDto,
} from '../models/learning-path.model';

@Injectable({ providedIn: 'root' })
export class LearningPathService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiV1Url}/learning-path`;

  private unwrap<T>(source: Observable<ApiEnvelope<T>>): Observable<T> {
    return source.pipe(map((r) => r.data));
  }

  getCatalog(levelId: number): Observable<CourseCatalogItemDto[]> {
    return this.unwrap(
      this.http.get<ApiEnvelope<CourseCatalogItemDto[]>>(`${this.base}/catalog`, {
        params: { levelId: String(levelId) },
      }),
    );
  }

  getCourseDetail(courseId: number): Observable<CourseLearnDetailDto> {
    return this.unwrap(
      this.http.get<ApiEnvelope<CourseLearnDetailDto>>(`${this.base}/courses/${courseId}/detail`),
    );
  }

  getUnitOutline(unitId: number): Observable<UnitOutlineDto> {
    return this.unwrap(this.http.get<ApiEnvelope<UnitOutlineDto>>(`${this.base}/units/${unitId}/outline`));
  }

  getLesson(lessonId: number): Observable<LessonPlayerDto> {
    return this.unwrap(this.http.get<ApiEnvelope<LessonPlayerDto>>(`${this.base}/lessons/${lessonId}`));
  }

  completeLesson(lessonId: number): Observable<LessonCompleteResultDto> {
    return this.unwrap(
      this.http.post<ApiEnvelope<LessonCompleteResultDto>>(
        `${this.base}/lessons/${lessonId}/complete`,
        {},
      ),
    );
  }
}
