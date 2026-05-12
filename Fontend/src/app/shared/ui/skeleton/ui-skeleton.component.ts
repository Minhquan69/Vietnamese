import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'ui-skeleton',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './ui-skeleton.component.html',
  styleUrl: './ui-skeleton.component.scss',
})
export class UiSkeletonComponent {
  @Input() variant: 'text' | 'rect' | 'circle' = 'rect';
  @Input() width = '100%';
  @Input() height = '1rem';
}
