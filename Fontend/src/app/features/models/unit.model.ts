export interface UnitDTO {
  unitId: number;
  courseId: number;
  unitName: string;
  videoUrl: string;
  duration: number;
  objective:string;
  createdBy:string;
  createdDate: Date;
  description:string;
  orderIndex: number;
  isActive: boolean;
  status?: boolean; 
  isDelete: boolean;
}

