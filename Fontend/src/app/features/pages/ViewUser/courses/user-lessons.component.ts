import { Component, OnInit } from '@angular/core';
import { LearningService } from '../../../services/learning.service';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-user-learning',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './user-lessons.component.html',
  styleUrls: ['./user-lessons.component.css'],
})
export class UserLearningComponent implements OnInit {
  learningPath: any[] = [];

  selectedLesson: any;
  videoUrl!: SafeResourceUrl;

  constructor(
    private learningService: LearningService,
    private sanitizer: DomSanitizer,
  ) {}

  ngOnInit(): void {
    this.loadLearningPath();
  }

  loadLearningPath() {
    this.learningService.getLearningPath().subscribe((res: any) => {
      this.learningPath = res;
    });
  }

  selectLesson(lesson: any) {
    if (lesson.isLocked) {
      alert('You must complete previous lesson first');
      return;
    }

    this.selectedLesson = lesson;

    this.videoUrl = this.sanitizer.bypassSecurityTrustResourceUrl(
      'https://www.youtube.com/embed/' + lesson.youtubeId,
    );
  }

  completeLesson() {
    this.learningService
      .completeLesson(this.selectedLesson.lessonId)
      .subscribe(() => {
        alert('Lesson completed');
        this.loadLearningPath();
      });
  }
}
