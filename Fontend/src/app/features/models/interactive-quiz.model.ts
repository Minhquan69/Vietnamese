export type InteractiveQuestionType =
  | 'MultipleChoice'
  | 'FillBlank'
  | 'DragDrop'
  | 'ReorderSentence'
  | 'Listening';

export interface PlayerAnswerDto {
  answerId: number;
  answerText: string;
  imageUrl?: string | null;
  audioUrl?: string | null;
  orderIndex: number;
}

export interface PlayerQuestionDto {
  questionId: number;
  questionText: string;
  questionType: InteractiveQuestionType;
  imageUrl?: string | null;
  audioUrl?: string | null;
  score: number;
  orderIndex: number;
  interactivePayload?: string | null;
  answers: PlayerAnswerDto[];
}

export interface PlayerQuizPackageDto {
  quizId: number;
  quizName: string;
  timeLimitMinutes?: number | null;
  passScore?: number | null;
  questions: PlayerQuestionDto[];
}

export interface QuizResponseItemDto {
  questionId: number;
  answerId?: number | null;
  fillBlank?: Record<string, string>;
  orderedAnswerIds?: number[];
  dragDrop?: Record<string, number>;
}

export interface InteractiveQuizSubmitDto {
  quizId: number;
  durationSeconds: number;
  responses: QuizResponseItemDto[];
}

export interface InteractiveQuestionOutcomeDto {
  questionId: number;
  questionType: string;
  correct: boolean;
  skipped: boolean;
  pointsEarned: number;
  explanation?: string | null;
  reviewHintJson?: string | null;
}

export interface InteractiveQuizResultDto {
  scorePercent: number;
  passed: boolean;
  correctCount: number;
  wrongCount: number;
  skippedCount: number;
  items: InteractiveQuestionOutcomeDto[];
}

export interface QuizAttemptSummaryDto {
  quizAttemptId: number;
  quizId: number;
  scorePercent: number;
  durationSeconds: number;
  correctCount: number;
  wrongCount: number;
  skippedCount: number;
  submittedUtc: string;
}
