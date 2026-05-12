import { Component, Input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { ShellNavItem } from '../shell-nav.types';

@Component({
  selector: 'app-shell-sidebar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './shell-sidebar.component.html',
  styleUrl: './shell-sidebar.component.scss',
})
export class ShellSidebarComponent {
  @Input({ required: true }) items: ShellNavItem[] = [];
  @Input() appName = 'VietPhuong';
  @Input() tagline = 'AI-native Vietnamese';

  /** Emitted when a nav link is chosen (mobile: close drawer) */
  readonly navigate = output<void>();

  onNav(): void {
    this.navigate.emit();
  }
}
