export interface SpeakingEvaluateResponse {
  attemptId: number;
  transcript: string;
  audioUrl?: string | null;
  durationMs: number;
  pronunciationScore: number;
  fluencyScore: number;
  toneScore: number;
  overallScore: number;
  feedback?: string | null;
  tips: string[];
}

export interface SpeakingAttemptSummary {
  attemptId: number;
  referenceText?: string | null;
  transcriptPreview: string;
  durationMs: number;
  overallScore: number;
  createdUtc: string;
}

export interface SpeakingAnalytics {
  attemptCount: number;
  averageOverall: number;
  averagePronunciation: number;
  averageFluency: number;
  averageTone: number;
  recent: SpeakingAttemptSummary[];
}
