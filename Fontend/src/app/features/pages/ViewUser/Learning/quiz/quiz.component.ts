import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CommonModule } from '@angular/common';
import { LearningService } from '../../../../services/learning.service';
import { QuizDTO } from '../../../../models/quiz.model';
import { SubmitQuizDTO } from '../../../../models/submit-quiz.model';
import { Router } from '@angular/router';
import { UnitDTO } from '../../../../models/unit.model';

@Component({
  selector: 'app-unit',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './quiz.component.html',
  styleUrls: ['./quiz.component.css'],
})
export class QuizLearnComponent implements OnInit {
  unit!: UnitDTO;
  unitId: number = 0;

  quiz: QuizDTO | null = null;
  showQuiz = false;

  isPassed = false;
  isSubmitted = false;

  previousScore: number = 0;
  currentScore: number = 0;

  selectedAnswers: { [key: number]: number } = {};

  constructor(
    private route: ActivatedRoute,
    private learningService: LearningService,
    private router: Router,
  ) {}

  ngOnInit() {
    this.route.queryParams.subscribe((params) => {
      this.unitId = +params['unitId'];
      this.loadunit();
    });
  }

  loadunit() {
    this.learningService.getUnitById(this.unitId).subscribe((res) => {
      this.unit = res;
    });
  }

  startQuiz() {
    this.showQuiz = true;

    this.learningService.getQuiz(this.unitId).subscribe((res) => {
      this.quiz = res;

      if (this.quiz) {
        this.learningService
          .getMyQuizResult(this.quiz.quizId)
          .subscribe((rs) => {
            if (rs) {
              this.previousScore = rs.score;
            }
          });
      }
    });
  }

  selectAnswer(qId: number, aId: number) {
    this.selectedAnswers[qId] = aId;
  }

  showResult = false;
  correctCount = 0;

  submitQuiz() {
    if (!this.quiz) return;

    const totalQuestions = this.quiz.questions.length;
    const answered = Object.keys(this.selectedAnswers).length;

    if (answered < totalQuestions) {
      alert('Please answer all questions');
      return;
    }

    this.correctCount = 0;

    this.quiz.questions.forEach((q) => {
      const selectedId = this.selectedAnswers[q.questionId];
      const correct = q.answers.find((a) => a.isCorrect);

      if (correct && correct.answerId === selectedId) {
        this.correctCount++;
      }
    });

    this.showResult = true;
    this.isSubmitted = true;

    const percent = (this.correctCount / totalQuestions) * 100;
    this.isPassed = percent >= (this.quiz.passScore || 70);

    const dto: SubmitQuizDTO = {
      quizId: this.quiz.quizId,
      answerIds: Object.values(this.selectedAnswers),
    };

    this.currentScore = percent;

    this.learningService.submitQuiz(dto).subscribe(() => {
      this.learningService
        .getMyQuizResult(this.quiz!.quizId)
        .subscribe((rs) => {
          if (rs) {
            this.previousScore = rs.score;
          }
        });
    });
  }
  isCorrectAnswer(q: any, a: any): boolean {
    return a.isCorrect;
  }

  isSelected(q: any, a: any): boolean {
    return this.selectedAnswers[q.questionId] === a.answerId;
  }

  isWrong(q: any, a: any): boolean {
    return this.showResult && this.isSelected(q, a) && !a.isCorrect;
  }

  isUnanswered(q: any): boolean {
    return this.showResult && !this.selectedAnswers[q.questionId];
  }
  goBackCourse() {
    this.router.navigate(['/user/units']);
  }

  retryQuiz() {
    this.selectedAnswers = {};
    this.showResult = false;
    this.isSubmitted = false;
    this.isPassed = false;
  }
}
