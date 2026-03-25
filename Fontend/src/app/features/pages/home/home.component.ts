import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
interface Course {
  title: string;
  description: string;
}

interface Feature {
  title: string;
  description: string;
}

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css',
})
export class HomeComponent {
  heroTitle = 'Learn Vietnamese with VietPhuong';

  heroDescription =
    'Discover Vietnamese through interactive lessons, vocabulary practice and real-life conversations.';

  features: Feature[] = [
    {
      title: 'Vocabulary Learning',
      description: 'Learn essential Vietnamese words with examples.',
    },
    {
      title: 'Pronunciation Practice',
      description: 'Improve speaking with listening and speaking exercises.',
    },
    {
      title: 'Interactive Lessons',
      description: 'Practice through quizzes and conversations.',
    },
  ];

  courses: Course[] = [
    {
      title: 'Greetings',
      description: 'Learn how to say hello and introduce yourself.',
    },
    {
      title: 'Numbers',
      description: 'Learn Vietnamese numbers easily.',
    },
    {
      title: 'Food',
      description: 'Useful phrases for ordering food.',
    },
  ];

  startLearning() {
    console.log('Start learning clicked');
  }

  createAccount() {
    console.log('Create account clicked');
  }
}
