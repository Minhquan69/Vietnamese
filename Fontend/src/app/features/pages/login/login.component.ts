import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AccountService } from '../../services/account.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
})
export class LoginComponent {
  email: string = '';
  password: string = '';

  constructor(
    private api: AccountService,
    private router: Router,
  ) {}

  login() {
  const data = {
    email: this.email,
    password: this.password,
  };

  this.api.login(data).subscribe({
    next: (res: any) => {
      localStorage.setItem('token', res.token);

      this.router.navigate(['/home']).then(() => {
        window.location.reload();
      });
    },
    error: () => {
      alert('Email or password is incorrect');
    },
  });
}
}
