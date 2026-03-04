import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-dashboard',
  standalone: false,
  templateUrl: './dashboard.component.html'
})
export class DashboardComponent implements OnInit {
  balance: number | null = null;

  constructor(private router: Router, private auth: AuthService) {}

  ngOnInit() {
    const saved = sessionStorage.getItem('latestBalance');
    this.balance = saved ? Number(saved) : null;
  }

  navigate(path: string) {
    this.router.navigate([path]);
  }

  openPinPage() {
    this.router.navigate(['/balance/pin']);
  }

  logout() {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}
