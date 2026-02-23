import { Component } from '@angular/core';
import { Router } from '@angular/router';

interface ResultResponse {
  isFraud: boolean;
  fraudProbability: number;
  reasons: string[];
  transactionId: number;
}

@Component({
  selector: 'app-result',
  standalone: false,
  templateUrl: './result.component.html'
})
export class ResultComponent {
  result: ResultResponse | null = null;

  constructor(private router: Router) {
    const raw = localStorage.getItem('lastResult');
    if (raw) {
      this.result = JSON.parse(raw);
    }
  }

  backToDashboard() {
    this.router.navigate(['/dashboard']);
  }
}
