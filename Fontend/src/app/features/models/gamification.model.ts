export interface GamificationProfile {
  totalXp: number;
  displayLevel: number;
  xpIntoCurrentLevel: number;
  xpRequiredForNextLevel: number;
  currentStreak: number;
  longestStreak: number;
  lastActivityDate?: string | null;
}

export interface GamificationAchievement {
  code: string;
  title: string;
  description: string;
  iconKey: string;
  unlocked: boolean;
  unlockedUtc?: string | null;
  xpReward: number;
}

export interface GamificationBadge {
  code: string;
  title: string;
  description: string;
  tier: number;
  earned: boolean;
  earnedUtc?: string | null;
}

export interface GamificationDailyChallenge {
  code: string;
  title: string;
  description: string;
  progress: number;
  target: number;
  completed: boolean;
  xpReward: number;
}

export interface GamificationState {
  profile: GamificationProfile;
  achievements: GamificationAchievement[];
  badges: GamificationBadge[];
  dailyChallenges: GamificationDailyChallenge[];
}

export interface GamificationLeaderboardRow {
  rank: number;
  userId: number;
  name: string;
  avatarUrl?: string | null;
  totalXp: number;
  displayLevel: number;
  currentStreak: number;
}
