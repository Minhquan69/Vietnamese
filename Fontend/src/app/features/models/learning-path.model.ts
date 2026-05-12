export interface CourseCatalogItemDto {
  courseId: number;
  courseName: string;
  description?: string | null;
  levelId: number;
  levelName: string;
  unitCount: number;
  lessonTotal: number;
  lessonsCompleted: number;
  progressPercent: number;
}

export interface CourseLearnDetailDto {
  courseId: number;
  courseName: string;
  description?: string | null;
  levelId: number;
  levelName: string;
  lessonTotal: number;
  lessonsCompleted: number;
  progressPercent: number;
  units: UnitLearnSummaryDto[];
  continueUnitId: number | null;
  continueLessonId: number | null;
}

export interface UnitLearnSummaryDto {
  unitId: number;
  unitName: string;
  objective?: string | null;
  orderIndex: number;
  lessonTotal: number;
  lessonsCompleted: number;
  progressPercent: number;
  unitUnlocked: boolean;
  unitPathComplete: boolean | null;
}

export interface LessonOutlineDto {
  lessonId: number;
  lessonType: string;
  title: string;
  summary?: string | null;
  orderIndex: number;
  durationMinutes: number;
  completed: boolean;
}

export interface UnitOutlineDto {
  unitId: number;
  unitName: string;
  description?: string | null;
  objective?: string | null;
  courseId: number;
  courseName: string;
  levelId: number;
  levelName: string;
  videoUrl?: string | null;
  lessonTotal: number;
  lessonsCompleted: number;
  progressPercent: number;
  continueLessonId: number | null;
  lessons: LessonOutlineDto[];
  quizUnitId: number | null;
}

export interface LessonPlayerDto {
  lessonId: number;
  lessonType: string;
  title: string;
  summary?: string | null;
  orderIndex: number;
  durationMinutes: number;
  contentJson?: string | null;
  completed: boolean;
  unitId: number;
  unitName: string;
  courseId: number;
  courseName: string;
  levelId: number;
  levelName: string;
  videoUrl?: string | null;
  previousLessonId: number | null;
  nextLessonId: number | null;
  quizUnitId: number | null;
}

export interface LessonCompleteResultDto {
  lessonId: number;
  completed: boolean;
  unitProgressPercent: number;
  nextLessonId: number | null;
}
