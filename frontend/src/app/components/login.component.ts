import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html'
})
export class LoginComponent {
  email = '';
  password = '';
  error = '';
  isRegister = false;

  constructor(private auth: AuthService, private router: Router) {}

  toggleMode() {
    this.isRegister = !this.isRegister;
    this.error = '';
  }

  submit() {
    this.error = '';
    if (this.isRegister) {
      this.auth.register(this.email, this.password).subscribe({
        next: () => {
          this.isRegister = false;
        },
        error: () => {
          this.error = 'Unable to register. Try a different email.';
        }
      });
      return;
    }

    this.auth.login(this.email, this.password).subscribe({
      next: () => this.router.navigate(['/dashboard']),
      error: () => {
        this.error = 'Invalid login credentials.';
      }
    });
  }
}
