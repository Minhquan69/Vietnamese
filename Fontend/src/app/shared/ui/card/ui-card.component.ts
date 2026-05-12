import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'ui-card',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './ui-card.component.html',
  styleUrl: './ui-card.component.scss',
})
export class UiCardComponent {
  /** elevated surface | glassmorphism */
  @Input() variant: 'elevated' | 'glass' = 'elevated';
  @Input() interactive = false;
  @Input() padding: 'none' | 'sm' | 'md' | 'lg' = 'md';
}
