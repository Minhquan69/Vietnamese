import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { LevelDTO } from '../models/level.model';
import { CourseDTO } from '../models/course.model';
import { UnitDTO } from '../models/unit.model';
import { QuizDTO } from '../models/quiz.model';
import { SubmitQuizDTO } from '../models/submit-quiz.model';

@Injectable({
  providedIn: 'root',
})
export class LearningService {
  private apiUrl = 'http://localhost:5108/api/learning';

  constructor(private http: HttpClient) {}
  //token
  private getOptions(isText: boolean = false) {
    const token = localStorage.getItem('token');
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`,
    });

    return {
      headers: headers,
      responseType: (isText ? 'text' : 'json') as 'json',
    };
  }

  // LEVEL
  getLevels(): Observable<LevelDTO[]> {
    return this.http.get<LevelDTO[]>(`${this.apiUrl}/listLevels`);
  }
  getLevelById(id: number): Observable<LevelDTO> {
    return this.http.get<LevelDTO>(`${this.apiUrl}/getLevelById?id=${id}`);
  }
  saveLevel(dto: LevelDTO[]): Observable<string> {
    return this.http.post<string>(
      `${this.apiUrl}/saveLevel`,
      dto,
      this.getOptions(true),
    );
  }

  // COURSE
  getCourses(levelId: number): Observable<CourseDTO[]> {
    return this.http.get<CourseDTO[]>(
      `${this.apiUrl}/listCourses?levelId=${levelId}`,
    );
  }
  getCourseById(id: number): Observable<CourseDTO> {
    return this.http.get<CourseDTO>(`${this.apiUrl}/getCourseById?id=${id}`);
  }
  saveCourse(dto: CourseDTO[]): Observable<string> {
    return this.http.post<string>(
      `${this.apiUrl}/saveCourse`,
      dto,
      this.getOptions(true),
    );
  }

  // Unit
  getUnits(courseId: number): Observable<UnitDTO[]> {
    return this.http.get<UnitDTO[]>(
      `${this.apiUrl}/listUnits?courseId=${courseId}`,
    );
  }
  getUnitById(id: number): Observable<UnitDTO> {
    return this.http.get<UnitDTO>(`${this.apiUrl}/getUnitById?id=${id}`);
  }
  saveUnit(dto: UnitDTO): Observable<string> {
    return this.http.post<string>(
      `${this.apiUrl}/saveUnit`,
      dto,
      this.getOptions(true),
    );
  }
  deleteUnits(ids: number[]): Observable<string> {
    return this.http.delete<string>(`${this.apiUrl}/deleteListUnit`, {
      ...this.getOptions(true),
      body: ids,
    });
  }
  uploadMedia(formData: FormData) {
    return this.http.post<any>(`${this.apiUrl}/uploadMedia`, formData);
  }

  // QUIZ
  saveQuiz(dto: QuizDTO): Observable<string> {
    return this.http.post<string>(`${this.apiUrl}/quiz/full`, dto, {
      responseType: 'text' as 'json',
    });
  }
  deleteQuiz(quizId: number): Observable<string> {
    return this.http.delete<string>(
      `${this.apiUrl}/deleteQuiz?id=${quizId}`,
      this.getOptions(true),
    );
  }
  getQuiz(unitId: number): Observable<QuizDTO> {
    return this.http.get<QuizDTO>(`${this.apiUrl}/allQuiz?UnitId=${unitId}`);
  }

  // USER
  getLearningPath(): Observable<LevelDTO[]> {
    return this.http.get<LevelDTO[]>(`${this.apiUrl}/learning-path`);
  }
  getMyProgress(): Observable<LevelDTO[]> {
    return this.http.get<LevelDTO[]>(
      `${this.apiUrl}/my-progress`,
      this.getOptions(),
    );
  }
  submitQuiz(dto: SubmitQuizDTO): Observable<string> {
    return this.http.post<string>(
      `${this.apiUrl}/submitQuiz`,
      dto,
      this.getOptions(true),
    );
  }
  getMyQuizResult(quizId: number) {
    return this.http.get<any>(
      `${this.apiUrl}/my-quiz-result?quizId=${quizId}`,
      this.getOptions(),
    );
  }
}
