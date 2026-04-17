import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { LearningService } from '../../../../services/learning.service';
import { UnitDTO } from '../../../../models/unit.model';
import { QuizDTO } from '../../../../models/quiz.model';
import { QuestionDTO } from '../../../../models/question.model';
import { AnswerDTO } from '../../../../models/answer.model';
import { BaseService } from '../../../../services/base.service';

@Component({
  selector: 'app-unit-detail',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './unit-detail.component.html',
  styleUrls: ['./unit-detail.component.css'],
})
export class UnitDetailComponent implements OnInit {
  unitId: number = 0;
  courseId: number = 0;
  videoFullUrl: string = '';
  tempVideoUrl: string = '';

  quiz: QuizDTO | null = null;

  unit: UnitDTO = {
    unitId: 0,
    courseId: 0,
    unitName: '',
    videoUrl: '',
    duration: 0,
    objective: '',
    createdBy: '',
    createdDate: new Date(),
    description: '',
    orderIndex: 0,
    isActive: true,
    isDelete: false,
  };

  constructor(
    private route: ActivatedRoute,
    private learningService: LearningService,
    private router: Router,
    private baseService:BaseService,
  ) {}

  ngOnInit(): void {
    const unitIdParam = this.route.snapshot.queryParamMap.get('unitId');
    const courseIdParam = this.route.snapshot.queryParamMap.get('courseId');

    if (!unitIdParam) {
      alert('Thiếu unitId trên URL');
      return;
    }

    this.unitId = Number(unitIdParam);
    this.courseId = Number(courseIdParam || 0);

    console.log('unitId =', this.unitId); // debug

    this.learningService.getUnitById(this.unitId).subscribe((res) => {
      this.unit = res;

      if (res.videoUrl && res.videoUrl.trim() !== '') {
        this.videoFullUrl = `http://localhost:5108/videos/${res.videoUrl}`;
      }

      this.loadQuiz();
    });
  }

  save() {
    if (this.tempVideoUrl) {
      this.unit.videoUrl = this.tempVideoUrl;
    }

    this.learningService.saveUnit(this.unit).subscribe({
      next: () => {
        alert('Save the lesson successfully.');
        this.tempVideoUrl = '';
      },
      error: (err) => this.baseService.handleError(err, 'Error saving lesson'),
    });
  }

  isDragging = false;

  onDragOver(event: DragEvent) {
    event.preventDefault(); 
    this.isDragging = true;
  }

  onDragLeave() {
    this.isDragging = false;
  }

  onDrop(event: DragEvent) {
    event.preventDefault();
    this.isDragging = false;

    const file = event.dataTransfer?.files[0];
    if (file) {
      this.handleFile(file);
    }
  }

  onFileSelect(event: any) {
    const file = event.target.files[0];
    if (file) {
      this.handleFile(file);
    }
  }

  handleFile(file: File) {
    this.uploadFile(file);
  }
  onVideoLoaded(event: any) {
    const video = event.target;
    this.unit.duration = Math.floor(video.duration);
  }

  uploadFile(file: File) {
    const formData = new FormData();
    formData.append('file', file);

    this.videoFullUrl = URL.createObjectURL(file);

    this.learningService.uploadMedia(formData).subscribe((res: any) => {
      this.tempVideoUrl = res.fileName;
      this.videoFullUrl = `http://localhost:5108/videos/${res.fileName}`;
    });
  }

  loadQuiz() {
    this.learningService.getQuiz(this.unitId).subscribe((res) => {
      this.quiz = res || null;
    });
  }
  addQuiz() {
    if (!this.unitId) {
      alert('nvalid UnitId');
      return;
    }

    this.quiz = {
      quizId: 0,
      unitId: this.unitId,
      quizName: '',
      passScore: 0,
      questions: [],
    };
  }
  addQuestion() {
    this.quiz?.questions.push({
      questionId: 0,
      quizId: this.quiz.quizId,
      questionText: '',
      answers: [],
      isDelete: false,
    });
  }
  deleteQuestion(q: QuestionDTO) {
    q.isDelete = true;
  }
  addAnswer(q: QuestionDTO) {
    q.answers.push({
      answerId: 0,
      questionId: q.questionId,
      answerText: '',
      isCorrect: false,
      isDelete: false,
    });
  }
  deleteAnswer(a: AnswerDTO) {
    a.isDelete = true;
  }
  saveQuiz() {
    if (!this.quiz) return;

    if (!this.cleanQuiz()) return;

    this.learningService.saveQuiz(this.quiz).subscribe({
      next: () => {
        alert('Saved Quiz successfully');
        this.loadQuiz();
      },
      error: (err) => this.baseService.handleError(err, 'Error saving Quiz'),
    });
  }
  deleteQuiz() {
    if (!this.quiz) return;

    const confirmDelete = confirm('Are you sure to delete this quiz?');
    if (!confirmDelete) return;

    this.learningService.deleteQuiz(this.quiz.quizId).subscribe({
      next: () => {
        alert('Quiz deleted');
        this.quiz = null;
      },
      error: (err) => this.baseService.handleError(err, 'Error deleting Quiz'),
    });
  }
  selectCorrectAnswer(q: QuestionDTO, a: AnswerDTO) {
    q.answers.forEach((x) => (x.isCorrect = false));
    a.isCorrect = true;
  }
  cleanQuiz(): boolean {
    if (!this.quiz) return false;

    this.quiz.questions = this.quiz.questions
      .map((q) => {
        if (q.isDelete) return q;

        if (
          (!q.questionText || q.questionText.trim() === '') &&
          !q.imageUrl &&
          !q.audioUrl
        ) {
          return null;
        }

        q.answers = q.answers.filter((a) => {
          if (a.isDelete) return true;
          return a.answerText?.trim() || a.imageUrl || a.audioUrl;
        });

        const validAnswers = q.answers.filter((a) => !a.isDelete);

        if (validAnswers.length < 2) return null;

        if (!validAnswers.some((a) => a.isCorrect)) return null;

        return q;
      })
      .filter((q) => q !== null) as any;

    return true;
  }

  handleMediaUpload(event: any, target: any) {
    const file = event.target.files[0];
    if (!file) return;

    const formData = new FormData();
    formData.append('file', file);

    this.learningService.uploadMedia(formData).subscribe((res) => {
      if (res.folder === 'images') {
        target.imageUrl = res.fileName;
      } else if (res.folder === 'audios') {
        target.audioUrl = res.fileName;
      } else if (res.folder === 'videos') {
        target.videoUrl = res.fileName;
        this.videoFullUrl = `http://localhost:5108/videos/${res.fileName}`;
      }
    });
  }
  removeMedia(target: any) {
    target.imageUrl = null;
    target.audioUrl = null;
  }
  
}