import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class LearningService {
  private apiUrl = 'http://localhost:5108/api/learning';

  constructor(private http: HttpClient) {}

  //level
  /*
  lay tat ca level
  thuphuong21072004
  */
  getLevels(): Observable<any> {
    return this.http.get(`${this.apiUrl}/listLevels`);
  }
  /*
  lưu level
  thuphuong21072004
  */
  saveLevel(dto: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/saveLevel`, dto, {
      responseType: 'text',
    });
  }
  
  // course
  /*
  lấy khóa học theo level
  thuphuong21072004
  */
  getCourses(levelId: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/listCourses?levelId=${levelId}`);
  }
  /*
  lưu khóa học
  thuphuong21072004
  */
  saveCourse(dto: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/saveCourse`, dto, {
      responseType: 'text',
    });
  }

  // lesson
  /*
  lấy bài học theo khóa học
  thuphuong21072004
  */
  getLessons(courseId: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/listLessons?courseId=${courseId}`);
  }
  /*
  lưu bài học
  thuphuong21072004
  */
  saveLesson(dto: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/saveLesson`, dto, {
      responseType: 'text',
    });
  }

  // User quiz
  /*
  lấy quiz theo lesson
  thuphuong21072004
  */
  getQuiz(lessonId: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/allQuiz?lessonId=${lessonId}`);
  }
  /*
  thêm bài kiểm tra
  thuphuong21072004
  */
  saveQuiz(dto: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/saveQuiz`, dto, {
      responseType: 'text',
    });
  }
  /*
  xóa bài kiểm tra
  thuphuong21072004
  */
  deleteQuiz(quizId: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/deleteQuiz/${quizId}`);
  }
  /*
  nộp quiz
  thuphuong21072004
  */
  submitQuiz(quizId: number, answerIds: number[]): Observable<any> {
    return this.http.post(
      `${this.apiUrl}/submitQuiz`,
      {
        quizId: quizId,
        answerIds: answerIds,
      },
      { responseType: 'text' },
    );
  }

  // user learning
  /*
  lấy lộ trình học
  thuphuong21072004
  */
  getLearningPath(): Observable<any> {
    return this.http.get(`${this.apiUrl}/learning-path`);
  }

  /*
  lấy tiến độ học cá nhân
  thuphuong21072004
  */
  getMyProgress(): Observable<any> {
    return this.http.get(`${this.apiUrl}/my-progress`);
  }
}
