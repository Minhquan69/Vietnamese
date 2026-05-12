export interface LearningDashboardDto {
  stats: DashboardStatsDto;
  activitySeries: DashboardDailyPointDto[];
  continueLearning: DashboardContinueDto | null;
  recommended: DashboardContinueDto[];
  challenges: DashboardChallengeDto[];
  achievements: DashboardAchievementDto[];
  vocabReminder: DashboardVocabReminderDto | null;
}

export interface DashboardStatsDto {
  xpTotal: number;
  xpToday: number;
  streakDays: number;
  dailyGoalXp: number;
  dailyGoalProgressPercent: number;
  quizzesPassedTotal: number;
  unitsCompletedTotal: number;
}

export interface DashboardDailyPointDto {
  date: string;
  xp: number;
  hadActivity: boolean;
}

export interface DashboardContinueDto {
  unitId: number;
  unitName: string;
  courseId: number;
  courseName: string;
  levelId: number;
  levelName: string;
  isLocked: boolean;
}

export interface DashboardChallengeDto {
  id: string;
  title: string;
  description: string;
  current: number;
  target: number;
  completed: boolean;
}

export interface DashboardAchievementDto {
  id: string;
  title: string;
  description: string;
  icon: string;
  unlocked: boolean;
}

export interface DashboardVocabReminderDto {
  reviewCount: number;
  message: string;
}
