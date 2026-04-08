import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LearningService } from '../../../services/learning.service';

@Component({
  selector: 'app-learning-admin',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-lessons.component.html',
  styleUrls: ['./admin-lessons.component.css'],
})
export class LearningAdminComponent implements OnInit {
  levels: any[] = [];
  courses: any[] = [];
  lessons: any[] = [];

  selectedLevel: any;
  selectedCourse: any;

  // form
  newLevelName = '';
  newCourseName = '';
  newLessonName = '';
  videoId = '';

  constructor(private learningService: LearningService) {}

  ngOnInit(): void {
    this.loadLevels();
  }

  // ================= LEVEL =================
  loadLevels() {
    this.learningService.getLevels().subscribe((res: any) => {
      this.levels = res;
    });
  }

  addLevel() {
    const dto = {
      levelName: this.newLevelName,
      description: '',
      orderIndex: 0,
    };

    this.learningService.addLevel(dto).subscribe(() => {
      alert('Add level success');
      this.newLevelName = '';
      this.loadLevels();
    });
  }

  selectLevel(level: any) {
    this.selectedLevel = level;
    this.learningService.getCourses(level.levelId).subscribe((res: any) => {
      this.courses = res;
      this.lessons = [];
    });
  }

  // ================= COURSE =================
  addCourse() {
    const dto = {
      levelId: this.selectedLevel.levelId,
      courseName: this.newCourseName,
      description: '',
      orderIndex: 0,
    };

    this.learningService.addCourse(dto).subscribe(() => {
      alert('Add course success');
      this.newCourseName = '';
      this.selectLevel(this.selectedLevel);
    });
  }

  selectCourse(course: any) {
    this.selectedCourse = course;
    this.learningService.getLessons(course.courseId).subscribe((res: any) => {
      this.lessons = res;
    });
  }

  // ================= LESSON =================
  addLesson() {
    const dto = {
      courseId: this.selectedCourse.courseId,
      lessonName: this.newLessonName,
      videoId: this.videoId,
      orderIndex: 0,
    };

    this.learningService.addLesson(dto).subscribe(() => {
      alert('Add lesson success');
      this.newLessonName = '';
      this.videoId = '';
      this.selectCourse(this.selectedCourse);
    });
  }

  deleteLesson(id: number) {
    if (!confirm('Delete lesson?')) return;

    this.learningService.deleteLesson(id).subscribe(() => {
      this.selectCourse(this.selectedCourse);
    });
  }
}
