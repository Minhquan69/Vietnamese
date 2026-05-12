import {
  Component,
  OnInit,
  computed,
  inject,
  signal,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  NavigationEnd,
  Router,
  RouterOutlet,
} from '@angular/router';
import { filter } from 'rxjs/operators';
import { AccountService } from '../../features/services/account.service';
import { AuthSessionService } from '../../core/auth/auth-session.service';
import { ShellSidebarComponent } from '../shell-sidebar/shell-sidebar.component';
import { ShellTopbarComponent } from '../shell-topbar/shell-topbar.component';
import { ShellNavItem } from '../shell-nav.types';

const NAV_CATALOG: ShellNavItem[] = [
  { label: 'Overview', path: '/dashboard', icon: 'spark' },
  { label: 'Course hub', path: '/learn', icon: 'path', roles: ['User'] },
  { label: 'Learning path', path: '/home', icon: 'path', roles: ['User'] },
  { label: 'My courses', path: '/user/units', icon: 'book', roles: ['User'] },
  { label: 'Practice tests', path: '/user/tests', icon: 'target', roles: ['User'] },
  { label: 'Vocabulary studio', path: '/vocabulary', icon: 'book' },
  { label: 'Vocabulary search', path: '/user/search', icon: 'search' },
  { label: 'AI tutor', path: '/ai-tutor', icon: 'spark', roles: ['User'] },
  { label: 'Speaking lab', path: '/speaking-lab', icon: 'spark', roles: ['User'] },
  { label: 'Guild hall', path: '/guild', icon: 'stack', roles: ['User'] },
  { label: 'Interactive quiz', path: '/quiz/interactive', icon: 'target', roles: ['User'] },
  { label: 'Quiz', path: '/user/quiz', icon: 'book', roles: ['User'] },
  { label: 'Users', path: '/admin/users', icon: 'users', roles: ['Admin'] },
  { label: 'CMS Studio', path: '/admin/cms', icon: 'clipboard', roles: ['Admin', 'Moderator'] },
  { label: 'Videos', path: '/admin/videos', icon: 'video', roles: ['Admin', 'Moderator'] },
  { label: 'Levels', path: '/admin/levels', icon: 'stack', roles: ['Admin', 'Moderator'] },
  { label: 'Courses', path: '/admin/courses', icon: 'stack', roles: ['Admin', 'Moderator'] },
  { label: 'Units', path: '/admin/units', icon: 'stack', roles: ['Admin', 'Moderator'] },
  { label: 'Unit detail', path: '/admin/unitDetail', icon: 'stack', roles: ['Admin', 'Moderator'] },
  { label: 'Tests', path: '/admin/tests', icon: 'clipboard', roles: ['Admin', 'Moderator'] },
  { label: 'Placements', path: '/admin/placements', icon: 'clipboard', roles: ['Admin', 'Moderator'] },
  { label: 'Profile', path: '/profile', icon: 'user', roles: ['User', 'Admin', 'Moderator'] },
  { label: 'Sign in', path: '/login', icon: 'user', roles: ['Guest'] },
  { label: 'Create account', path: '/user/register', icon: 'user', roles: ['Guest'] },
];

const TITLE_BY_PATH: Record<string, string> = {
  '/dashboard': 'Overview',
  '/learn': 'Course hub',
  '/home': 'Learning path',
  '/user/units': 'My courses',
  '/vocabulary': 'Vocabulary studio',
  '/user/search': 'Vocabulary search',
  '/user/tests': 'Practice tests',
  '/ai-tutor': 'AI tutor',
  '/speaking-lab': 'Speaking lab',
  '/guild': 'Guild hall',
  '/quiz/interactive': 'Interactive quiz',
  '/user/quiz': 'Quiz',
  '/profile': 'Profile',
  '/admin/users': 'Users',
  '/admin/cms': 'CMS Studio',
  '/admin/videos': 'Videos',
  '/admin/levels': 'Levels',
  '/admin/courses': 'Courses',
  '/admin/units': 'Units',
  '/admin/unitDetail': 'Unit detail',
  '/admin/tests': 'Tests',
  '/admin/placements': 'Placements',
};

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    ShellSidebarComponent,
    ShellTopbarComponent,
  ],
  templateUrl: './main-layout.component.html',
  styleUrl: './main-layout.component.scss',
})
export class MainLayoutComponent implements OnInit {
  private readonly router = inject(Router);
  private readonly account = inject(AccountService);
  private readonly authSession = inject(AuthSessionService);

  readonly navOpen = signal(false);
  readonly isLogin = signal(false);
  readonly role = signal<string | null>(null);
  readonly userName = signal<string | null>(null);
  readonly pageTitle = signal('Overview');

  readonly visibleNav = computed(() => {
    const login = this.isLogin();
    const r = this.role();
    const effective = !login ? 'Guest' : (r ?? 'User');
    return NAV_CATALOG.filter((item) => {
      if (!item.roles?.length) {
        return true;
      }
      return item.roles.includes(effective);
    });
  });

  ngOnInit(): void {
    this.refreshTitle(this.router.url);
    this.router.events
      .pipe(filter((e): e is NavigationEnd => e instanceof NavigationEnd))
      .subscribe(() => this.refreshTitle(this.router.url));

    const token = this.authSession.getAccessToken();
    if (!token) {
      this.resetSession();
      return;
    }

    this.account.getCurrentUser().subscribe({
      next: (res: { userId: number; email: string; name: string; role: string }) => {
        this.isLogin.set(true);
        this.role.set(res.role ? res.role.toString().trim() : null);
        this.userName.set(res.name ?? res.email ?? 'Learner');
      },
      error: () => this.resetSession(),
    });
  }

  toggleNav(): void {
    this.navOpen.update((v) => !v);
  }

  closeNav(): void {
    this.navOpen.set(false);
  }

  onLogout(): void {
    this.resetSession();
    this.closeNav();
    this.authSession.logout(true);
  }

  private resetSession(): void {
    this.isLogin.set(false);
    this.role.set(null);
    this.userName.set(null);
  }

  private refreshTitle(url: string): void {
    const path = url.split('?')[0] || '/dashboard';
    if (path.startsWith('/admin/cms')) {
      const seg = path.replace('/admin/cms', '').split('/').filter(Boolean)[0] ?? 'dashboard';
      const cmsTitles: Record<string, string> = {
        dashboard: 'CMS · Overview',
        courses: 'CMS · Courses',
        lessons: 'CMS · Lessons',
        vocabulary: 'CMS · Vocabulary',
        quizzes: 'CMS · Quizzes',
        users: 'CMS · Users',
      };
      this.pageTitle.set(cmsTitles[seg] ?? 'CMS Studio');
      return;
    }
    this.pageTitle.set(TITLE_BY_PATH[path] ?? 'Learn');
  }
}
