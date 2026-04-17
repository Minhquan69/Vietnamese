import { CourseDTO } from './course.model';

export interface LevelDTO {
  levelId: number;
  levelName: string;
  description: string;
  orderIndex: number;
  isActive: boolean;
  status?: boolean;
  isDelete: boolean;
  courses: CourseDTO[];
}
