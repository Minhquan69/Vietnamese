export interface AnswerDTO {
  answerId: number;
  questionId: number;
  answerText?: string | null;
    imageUrl?: string | null; 
    audioUrl?: string | null;
  isCorrect: boolean;
  isDelete: boolean;
}
