import { Component, OnInit } from '@angular/core';
import { TransactionService } from '../services/transaction.service';

@Component({
  selector: 'app-history',
  templateUrl: './history.component.html'
})
export class HistoryComponent implements OnInit {
  history: any[] = [];

  constructor(private txService: TransactionService) {}

  ngOnInit() {
    this.txService.getHistory().subscribe((data: any) => {
      this.history = data;
    });
  }
}
