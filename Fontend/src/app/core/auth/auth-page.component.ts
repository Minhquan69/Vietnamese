import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UiCardComponent } from '../../shared/ui/card/ui-card.component';

@Component({
  selector: 'app-auth-page',
  standalone: true,
  imports: [CommonModule, UiCardComponent],
  templateUrl: './auth-page.component.html',
  styleUrl: './auth-page.component.scss',
})
export class AuthPageComponent {
  @Input() title = '';
  @Input() subtitle = '';
}
