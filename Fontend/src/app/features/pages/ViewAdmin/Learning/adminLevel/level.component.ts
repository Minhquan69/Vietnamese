import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { LearningService } from '../../../../services/learning.service';
import { LevelDTO } from '../../../../models/level.model';
import { BaseService } from '../../../../services/base.service';

@Component({
  selector: 'app-level',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './level.component.html',
  styleUrls: ['./level.component.css'],
})
export class LevelComponent implements OnInit {
  constructor(
    private learningService: LearningService,
    private baseService: BaseService,
  ) {}

  @Output() select = new EventEmitter<LevelDTO>();

  levels: LevelDTO[] = [];

  ngOnInit(): void {
    this.loadLevels();
  }

  loadLevels() {
    this.learningService.getLevels().subscribe((res) => {
      this.levels = res;
    });
  }

  isValidAllRows(): boolean {
    return this.levels.every(
      (level) =>
        level.isDelete ||
        (level.levelName?.trim() && level.description?.trim()),
    );
  }

  addRow() {
    if (!this.isValidAllRows()) {
      alert('Please enter all the required information before adding!');
      return;
    }

    this.levels.push({
      levelId: 0,
      levelName: '',
      description: '',
      orderIndex: 0,
      isActive: false,
      status: false,
      isDelete: false,
      courses: [],
    });
  }

  saveAll() {
    if (!this.isValidAllRows()) {
      alert('Please enter all the required information before saving!');
      return;
    }

    this.learningService.saveLevel(this.levels).subscribe({
      next: () => {
        alert('Save the list of successful Levels.');
        this.loadLevels();
      },
      error: (err) =>
        this.baseService.handleError(err, 'Level cannot be saved'),
    });
  }
}
