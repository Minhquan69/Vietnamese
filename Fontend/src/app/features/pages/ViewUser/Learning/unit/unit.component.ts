import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { LearningService } from '../../../../services/learning.service';
import { LevelDTO } from '../../../../models/level.model';

@Component({
  selector: 'app-unit',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './unit.component.html',
  styleUrls: ['./unit.component.css'],
})
export class MyProgressComponent implements OnInit {
  levels: LevelDTO[] = [];
  loading = true;

  constructor(
    private learningService: LearningService,
    private router: Router,
  ) {}

  ngOnInit(): void {
    this.loadProgress();
  }

  loadProgress() {
    this.learningService.getMyProgress().subscribe({
      next: (res) => {
        this.levels = res;
        this.loading = false;
      },
      error: () => {
        alert('Failed to load progress');
        this.loading = false;
      },
    });
  }

  openunit(unit: any) {
    if (unit.status === null) return;

    this.router.navigate(['/unit'], {
      queryParams: { unitId: unit.unitId },
    });
  }

  getunitState(unit: any): string {
    if (unit.status === true) return 'done';
    if (unit.status === false) return 'current';
    return 'locked';
  }

  
  getCourseState(course: any): string {
  if (!course.units || course.units.length === 0) {
    if (course.status === true) return 'done';
    if (course.status === false) return 'current';
    return 'locked';
  }

  const states: string[] = course.units.map((l: any) => this.getunitState(l));

  if (states.every((s: string) => s === 'done')) return 'done';
  if (states.includes('current')) return 'current';
  if (states.includes('done')) return 'current';

  return 'locked';
  }

  getLevelState(level: any): string {
  if (!level.courses || level.courses.length === 0) {
    if (level.status === true) return 'done';
    if (level.status === false) return 'current';
    return 'locked';
  }

  const states: string[] = level.courses.map((c: any) => this.getCourseState(c));

  if (states.every((s: string) => s === 'done')) return 'done';
  if (states.includes('current')) return 'current';
  if (states.includes('done')) return 'current';

  return 'locked';
  }
}
