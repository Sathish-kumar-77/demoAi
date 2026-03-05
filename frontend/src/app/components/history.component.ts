import { Component, OnInit } from '@angular/core';
import { TransactionService } from '../services/transaction.service';

@Component({
  selector: 'app-history',
  standalone: false,
  templateUrl: './history.component.html'
})
export class HistoryComponent implements OnInit {
  history: any[] = [];

  constructor(private txService: TransactionService) {}

  ngOnInit() {
    this.loadHistory();
  }

  loadHistory() {
    this.txService.getHistory().subscribe((data: any) => {
      this.history = data;
    });
  }

  canRefund(tx: any) {
    return tx.fraudProbability >= 0.8 || tx.prediction === 'Fraud';
  }

  refund(tx: any) {
    this.txService.refund(tx.id).subscribe((updated: any) => {
      tx.status = updated.status;
    });
  }
}
