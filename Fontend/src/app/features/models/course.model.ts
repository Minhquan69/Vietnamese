import { UnitDTO } from './unit.model';

export interface CourseDTO {
  courseId: number;
  levelId: number;
  courseName: string;
  description: string;
  orderIndex: number;
  createdBy: string;
  isActive: boolean;
  status?: boolean;
  isDelete: boolean;
  units?: UnitDTO[];
}
