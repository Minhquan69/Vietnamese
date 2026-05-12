import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'ui-progress-donut',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './ui-progress-donut.component.html',
  styleUrl: './ui-progress-donut.component.scss',
})
export class UiProgressDonutComponent {
  readonly gradientId = `donut-grad-${Math.random().toString(36).slice(2, 11)}`;

  /** 0–100 */
  @Input() value = 0;
  @Input() size = 88;
  @Input() stroke = 8;
  @Input() label = '';

  get normalized(): number {
    return Math.max(0, Math.min(100, this.value));
  }

  get radius(): number {
    return (this.size - this.stroke) / 2;
  }

  get circumference(): number {
    return 2 * Math.PI * this.radius;
  }

  get dashoffset(): number {
    return this.circumference * (1 - this.normalized / 100);
  }
}
