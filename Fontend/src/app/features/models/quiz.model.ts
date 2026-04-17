import { QuestionDTO } from './question.model';

export interface QuizDTO {
  quizId: number;
  unitId: number;
  quizName: string;
  passScore: number;
  questions: QuestionDTO[];
}
