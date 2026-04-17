import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router,RouterLink } from '@angular/router';
import { AccountService } from '../../../services/account.service';
import { BaseService } from '../../../services/base.service';
@Component({
  selector: 'app-register',
  standalone: true,
  imports: [FormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css'],
})
export class RegisterComponent {
  name: string = '';
  email: string = '';
  password: string = '';
  confirmPassword: string = '';
  agree: boolean = false;

  constructor(
    private api: AccountService,
    private router: Router,
    private baseService: BaseService
  ) {}

  register() {
    
    if (this.password !== this.confirmPassword) {
      alert('Passwords do not match');
      return;
    }

    if (!this.agree) {
      alert('You must agree to terms');
      return;
    }
    const data = {
      name: this.name,
      email: this.email,
      password: this.password,
    };
    this.api.register(data).subscribe({
      next: (res: any) => {
        localStorage.setItem('token', res.token);
        this.router.navigate(['/home']).then(() => {
          window.location.reload();
        });
      },
      error: (err) => {
        this.baseService.handleError(err, 'Register failed');
      },
    });
  }
}
