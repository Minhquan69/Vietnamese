export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface AdminUserRow {
  userId: number;
  name: string;
  email: string;
  roleId: number;
  roleName: string;
  status: number;
  avatarUrl?: string | null;
}

export interface AdminCourseRow {
  courseId: number;
  courseName: string;
  levelId: number;
  levelName: string;
  orderIndex: number;
  isActive: boolean;
}

export interface AdminLessonRow {
  lessonId: number;
  unitId: number;
  unitName: string;
  courseId: number;
  courseName: string;
  levelName: string;
  title: string;
  lessonType: string;
  orderIndex: number;
  isActive: boolean;
}

export interface AdminVocabularyRow {
  vocabularyId: number;
  word: string;
  meaningEn?: string | null;
  partOfSpeech?: string | null;
  isActive: boolean;
  createdUtc: string;
}

export interface AdminQuizRow {
  quizId: number;
  quizName: string;
  refType: string;
  refId: number;
  passScore?: number | null;
  timeLimit?: number | null;
  isActive: boolean;
  createdDate: string;
}

export interface AdminAnalyticsTotals {
  usersTotal: number;
  usersActive: number;
  coursesTotal: number;
  lessonsTotal: number;
  vocabulariesTotal: number;
  quizzesTotal: number;
  quizAttemptsLast30Days: number;
  speakingAttemptsLast30Days: number;
}

export interface AdminAnalyticsSeriesPoint {
  date: string;
  quizCompletions: number;
  speakingSessions: number;
}

export interface AdminAnalyticsSummary {
  totals: AdminAnalyticsTotals;
  activityLast14Days: AdminAnalyticsSeriesPoint[];
}
