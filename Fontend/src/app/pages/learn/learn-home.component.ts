import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { LearningService } from '../../features/services/learning.service';
import type { LevelDTO } from '../../features/models/level.model';

@Component({
  selector: 'app-learn-home',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './learn-home.component.html',
  styleUrl: './learn-home.component.scss',
})
export class LearnHomeComponent implements OnInit {
  private readonly learning = inject(LearningService);

  readonly levels = signal<LevelDTO[]>([]);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);

  ngOnInit(): void {
    this.learning.getLevels().subscribe({
      next: (list) => {
        this.levels.set(list.filter((l) => l.isActive));
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Could not load levels.');
        this.loading.set(false);
      },
    });
  }
}
