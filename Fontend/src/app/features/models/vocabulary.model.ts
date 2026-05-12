export interface VocabularyCardDto {
  vocabularyId: number;
  word: string;
  ipa?: string | null;
  audioUrl?: string | null;
  meaningEn?: string | null;
  partOfSpeech?: string | null;
  exampleSentence?: string | null;
  exampleTranslation?: string | null;
  contextNote?: string | null;
}

export interface UserVocabularyCardDto extends VocabularyCardDto {
  saved: boolean;
  masteryScore: number;
  familiarity: number;
  intervalDays: number;
  repetitions: number;
  nextReviewUtc: string;
  isDue: boolean;
}

export interface VocabularyListResultDto {
  items: VocabularyCardDto[];
  total: number;
}

export interface ReviewResultDto {
  vocabularyId: number;
  masteryScore: number;
  familiarity: number;
  intervalDays: number;
  repetitions: number;
  nextReviewUtc: string;
  easeFactor: number;
}

export interface VocabularyStatsDto {
  savedCount: number;
  dueCount: number;
  averageMastery: number;
  masteredCount: number;
}
