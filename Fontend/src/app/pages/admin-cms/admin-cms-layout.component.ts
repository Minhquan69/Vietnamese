import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthSessionService } from '../../core/auth/auth-session.service';

@Component({
  selector: 'app-admin-cms-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './admin-cms-layout.component.html',
  styleUrls: ['./admin-cms-layout.component.scss', './admin-cms.shared.scss'],
})
export class AdminCmsLayoutComponent {
  private readonly session = inject(AuthSessionService);

  readonly isAdmin = signal(this.session.getRoleFromAccessToken() === 'Admin');
}
