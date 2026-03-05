import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginComponent } from './components/login.component';
import { DashboardComponent } from './components/dashboard.component';
import { PayComponent } from './components/pay.component';
import { PayPinComponent } from './components/pay-pin.component';
import { ResultComponent } from './components/result.component';
import { HistoryComponent } from './components/history.component';
import { ProfileComponent } from './components/profile.component';
import { BalancePinComponent } from './components/balance-pin.component';
import { AuthGuard } from './guards/auth.guard';

const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'dashboard', component: DashboardComponent, canActivate: [AuthGuard] },
  { path: 'pay', component: PayComponent, canActivate: [AuthGuard] },
  { path: 'pay/pin', component: PayPinComponent, canActivate: [AuthGuard] },
  { path: 'result', component: ResultComponent, canActivate: [AuthGuard] },
  { path: 'history', component: HistoryComponent, canActivate: [AuthGuard] },
  { path: 'profile', component: ProfileComponent, canActivate: [AuthGuard] },
  { path: 'balance/pin', component: BalancePinComponent, canActivate: [AuthGuard] }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {}
