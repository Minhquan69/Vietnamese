import type { VocabularyCardDto } from './vocabulary.model';

export interface VideoDto {
  videoId: number;
  youtubeId: string;
  title: string;
  status: number;
}

export interface TranscriptCueDto {
  transcriptId: number;
  youtubeId: string;
  sentence: string;
  startTime: number;
  endTime: number;
}

export interface VideoLearningSessionDto {
  video: VideoDto;
  transcripts: TranscriptCueDto[];
  linkedVocabulary: VocabularyCardDto[];
}

export interface ExtractedTokenDto {
  text: string;
  startIndex: number;
  length: number;
  vocabularyId: number | null;
}

export interface VideoExtractResultDto {
  transcriptId: number;
  tokens: ExtractedTokenDto[];
}
