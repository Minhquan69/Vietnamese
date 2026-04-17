import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { LearningService } from '../../services/learning.service';
import { LevelDTO } from '../../models/level.model';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css',
})
export class HomeComponent implements OnInit {

  heroTitle = 'Learn Vietnamese with VietPhuong';

  heroDescription =
    'Discover Vietnamese through interactive units, vocabulary practice and real-life conversations.';

  features = [
    {
      title: 'Vocabulary Learning',
      description: 'Learn essential Vietnamese words with examples.',
    },
    {
      title: 'Pronunciation Practice',
      description: 'Improve speaking with listening and speaking exercises.',
    },
    {
      title: 'Interactive units',
      description: 'Practice through quizzes and conversations.',
    },
  ];

  levels: LevelDTO[] = [];

  constructor(private learningService: LearningService) {}

  ngOnInit(): void {
    this.loadLearningPath();
  }

  loadLearningPath() {
    this.learningService.getLearningPath().subscribe(
  (res: LevelDTO[]) => {
    this.levels = res;
  },
  (err) => {
    console.error(err);
  }
);
  }
}