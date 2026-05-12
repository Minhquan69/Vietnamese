import { Component, Input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ThemeService } from '../../core/theme/theme.service';

@Component({
  selector: 'app-shell-topbar',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './shell-topbar.component.html',
  styleUrl: './shell-topbar.component.scss',
})
export class ShellTopbarComponent {
  @Input({ required: true }) pageTitle = '';
  @Input() userLabel: string | null = null;
  @Input() isLogin = false;

  readonly menuToggle = output<void>();
  readonly logout = output<void>();

  constructor(readonly theme: ThemeService) {}

  onMenu(): void {
    this.menuToggle.emit();
  }

  onLogout(): void {
    this.logout.emit();
  }

  toggleTheme(): void {
    this.theme.toggle();
  }
}
