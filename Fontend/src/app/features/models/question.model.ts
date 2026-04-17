import { AnswerDTO } from './answer.model';

export interface QuestionDTO {
  questionId: number;
  quizId: number;
  questionText?: string | null;
    imageUrl?: string | null; 
    audioUrl?: string | null;
  isDelete: boolean;
  answers: AnswerDTO[];
}
