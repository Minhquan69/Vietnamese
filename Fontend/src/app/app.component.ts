import { Component } from '@angular/core';
import { RouterOutlet, RouterLink, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AccountService } from './features/services/account.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, CommonModule],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
})
export class AppComponent {
  role: string | null = null; 
  userId: number | null = null;
  name: string | null = null;
  isLogin: boolean = false;
  showAccountMenu = false;

  constructor(
  private router: Router,
  private api: AccountService
) {}

  ngOnInit() {
  const token = localStorage.getItem("token");

  if (!token) {
    this.resetUser();
    return;
  }

  this.api.getCurrentUser().subscribe({
    next: (res: any) => {
      this.userId = res.userId;
      this.role = res.role ? res.role.toString().trim() : null;
      this.name = res.name;
      this.isLogin = true;

    },
    error: () => {
      this.resetUser();
    }
  });
}

  resetUser() {
    this.role = null;
    this.userId = null;
    this.name = null;
    this.isLogin = false;
  }

  toggleMenu() {
    this.showAccountMenu = !this.showAccountMenu;
  }

  logout() {
    localStorage.removeItem('token');
    this.resetUser();
    this.router.navigate(['/home']);
  }
}